// <copyright file="VectorCompressionService.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Domain.VectorCompression;

/// <summary>
/// Unified service for vector compression using spectral methods.
/// Refactored to follow immutable event sourcing pattern with PipelineBranch.
/// All compression operations track statistics through events.
/// </summary>
public static class VectorCompressionService
{
    /// <summary>
    /// Compression statistics for monitoring.
    /// </summary>
    public record CompressionStats(
        int VectorsCompressed,
        long OriginalBytes,
        long CompressedBytes,
        double AverageCompressionRatio,
        double AverageEnergyRetained);

    /// <summary>
    /// Synchronous compress operation that returns Result with compressed data and event.
    /// Pure function that returns both the compressed data and an event record for tracking.
    /// </summary>
    public static Result<(byte[] CompressedData, VectorCompressionEvent Event)> Compress(
        float[] vector,
        CompressionConfig config,
        CompressionMethod? method = null)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(vector);
            ArgumentNullException.ThrowIfNull(config);

            CompressionMethod m = method ?? config.DefaultMethod;

            if (m == CompressionMethod.Adaptive)
            {
                m = SelectOptimalMethod(vector);
            }

            // Create compressors
            FourierVectorCompressor fftCompressor = new FourierVectorCompressor(
                config.TargetDimension,
                FourierVectorCompressor.CompressionStrategy.HighestMagnitude);

            DCTVectorCompressor dctCompressor = new DCTVectorCompressor(
                config.TargetDimension,
                config.EnergyThreshold);

            byte[] result;
            double energyRetained = 1.0;

            switch (m)
            {
                case CompressionMethod.DCT:
                    DCTCompressedVector dct = dctCompressor.Compress(vector);
                    result = WrapWithHeader(CompressionMethod.DCT, dct.ToBytes());
                    energyRetained = dct.EnergyRetained;
                    break;

                case CompressionMethod.QuantizedDCT:
                    DCTCompressedVector dctQ = dctCompressor.Compress(vector);
                    QuantizedDCTVector quantized = dctCompressor.Quantize(dctQ, 8);
                    result = WrapWithHeader(CompressionMethod.QuantizedDCT, quantized.ToBytes());
                    energyRetained = dctQ.EnergyRetained;
                    break;

                case CompressionMethod.FFT:
                    CompressedVector fft = fftCompressor.Compress(vector);
                    result = WrapWithHeader(CompressionMethod.FFT, fft.ToBytes());
                    break;

                default:
                    return Result<(byte[], VectorCompressionEvent)>.Failure($"Unknown compression method: {m}");
            }

            // Create compression event
            VectorCompressionEvent compressionEvent = VectorCompressionEvent.Create(
                method: m.ToString(),
                originalBytes: vector.Length * sizeof(float),
                compressedBytes: result.Length,
                energyRetained: energyRetained);

            return Result<(byte[], VectorCompressionEvent)>.Success((result, compressionEvent));
        }
        catch (ArgumentNullException ex)
        {
            return Result<(byte[], VectorCompressionEvent)>.Failure($"Compression input error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<(byte[], VectorCompressionEvent)>.Failure($"Compression failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Decompresses vector data back to float array.
    /// Pure function with no side effects.
    /// </summary>
    /// <param name="data">Compressed vector data.</param>
    /// <param name="config">Compression configuration.</param>
    /// <returns>Result containing decompressed vector.</returns>
    public static Result<float[]> Decompress(byte[] data, CompressionConfig config)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(data);
            ArgumentNullException.ThrowIfNull(config);

            (CompressionMethod method, byte[]? payload) = UnwrapHeader(data);

            // Create compressors
            FourierVectorCompressor fftCompressor = new FourierVectorCompressor(
                config.TargetDimension,
                FourierVectorCompressor.CompressionStrategy.HighestMagnitude);

            DCTVectorCompressor dctCompressor = new DCTVectorCompressor(
                config.TargetDimension,
                config.EnergyThreshold);

            float[] result = method switch
            {
                CompressionMethod.DCT => dctCompressor.Decompress(DCTCompressedVector.FromBytes(payload)),
                CompressionMethod.QuantizedDCT => dctCompressor.DecompressQuantized(QuantizedDCTVector.FromBytes(payload)),
                CompressionMethod.FFT => fftCompressor.Decompress(CompressedVector.FromBytes(payload)),
                _ => throw new InvalidOperationException($"Unknown compression method: {method}")
            };

            return Result<float[]>.Success(result);
        }
        catch (InvalidOperationException ex)
        {
            return Result<float[]>.Failure($"Decompression format error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<float[]>.Failure($"Decompression failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Computes similarity between two compressed vectors without full decompression.
    /// Pure function with no side effects.
    /// </summary>
    public static Result<double> CompressedSimilarity(byte[] a, byte[] b, CompressionConfig config)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(a);
            ArgumentNullException.ThrowIfNull(b);
            ArgumentNullException.ThrowIfNull(config);

            (CompressionMethod methodA, byte[]? payloadA) = UnwrapHeader(a);
            (CompressionMethod methodB, byte[]? payloadB) = UnwrapHeader(b);

            // Create compressors
            FourierVectorCompressor fftCompressor = new FourierVectorCompressor(
                config.TargetDimension,
                FourierVectorCompressor.CompressionStrategy.HighestMagnitude);

            DCTVectorCompressor dctCompressor = new DCTVectorCompressor(
                config.TargetDimension,
                config.EnergyThreshold);

            if (methodA != methodB)
            {
                // Fall back to full decompression if methods differ
                Result<float[]> vecAResult = Decompress(a, config);
                Result<float[]> vecBResult = Decompress(b, config);

                if (vecAResult.IsFailure || vecBResult.IsFailure)
                {
                    return Result<double>.Failure("Failed to decompress vectors for similarity");
                }

                return Result<double>.Success(CosineSimilarity(vecAResult.Value, vecBResult.Value));
            }

            double similarity = methodA switch
            {
                CompressionMethod.DCT => dctCompressor.CompressedSimilarity(
                    DCTCompressedVector.FromBytes(payloadA),
                    DCTCompressedVector.FromBytes(payloadB)),
                CompressionMethod.FFT => fftCompressor.CompressedSimilarity(
                    CompressedVector.FromBytes(payloadA),
                    CompressedVector.FromBytes(payloadB)),
                _ => throw new InvalidOperationException($"Compressed similarity not supported for method: {methodA}")
            };

            return Result<double>.Success(similarity);
        }
        catch (InvalidOperationException ex)
        {
            return Result<double>.Failure($"Compressed similarity format error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<double>.Failure($"Compressed similarity failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets compression statistics from a collection of compression events.
    /// Pure function that derives statistics from event stream.
    /// </summary>
    public static Result<VectorCompressionStats> GetStats(IEnumerable<VectorCompressionEvent> events)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(events);

            List<VectorCompressionEvent> eventList = events.ToList();

            if (eventList.Count == 0)
            {
                return Result<VectorCompressionStats>.Success(new VectorCompressionStats
                {
                    VectorsCompressed = 0,
                    TotalOriginalBytes = 0,
                    TotalCompressedBytes = 0,
                    AverageEnergyRetained = 0.0,
                    MethodBreakdown = new Dictionary<string, int>()
                });
            }

            long totalOriginal = eventList.Sum(e => e.OriginalBytes);
            long totalCompressed = eventList.Sum(e => e.CompressedBytes);
            double avgEnergy = eventList.Average(e => e.EnergyRetained);
            Dictionary<string, int> methodBreakdown = eventList.GroupBy(e => e.Method)
                .ToDictionary(g => g.Key, g => g.Count());

            VectorCompressionStats stats = new VectorCompressionStats
            {
                VectorsCompressed = eventList.Count,
                TotalOriginalBytes = totalOriginal,
                TotalCompressedBytes = totalCompressed,
                AverageEnergyRetained = avgEnergy,
                FirstCompressionAt = eventList.Min(e => e.Timestamp),
                LastCompressionAt = eventList.Max(e => e.Timestamp),
                MethodBreakdown = methodBreakdown
            };

            return Result<VectorCompressionStats>.Success(stats);
        }
        catch (ArgumentNullException ex)
        {
            return Result<VectorCompressionStats>.Failure($"Stats input error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<VectorCompressionStats>.Failure($"Failed to compute stats: {ex.Message}");
        }
    }

    /// <summary>
    /// Batch compress multiple vectors efficiently.
    /// Returns compressed data and compression events for tracking.
    /// </summary>
    public static async Task<Result<(IReadOnlyList<byte[]> CompressedData, IReadOnlyList<VectorCompressionEvent> Events)>> BatchCompressAsync(
        IEnumerable<float[]> vectors,
        CompressionConfig config,
        CompressionMethod? method = null)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(vectors);
            ArgumentNullException.ThrowIfNull(config);

            List<float[]> vectorList = vectors.ToList();
            List<byte[]> compressedResults = new List<byte[]>();
            List<VectorCompressionEvent> compressionEvents = new List<VectorCompressionEvent>();

            // Process in parallel for efficiency
            List<Task<Result<(byte[] CompressedData, VectorCompressionEvent Event)>>> compressionTasks = vectorList.Select(v => Task.Run(() => Compress(v, config, method))).ToList();
            Result<(byte[] CompressedData, VectorCompressionEvent Event)>[] results = await Task.WhenAll(compressionTasks);

            foreach (Result<(byte[] CompressedData, VectorCompressionEvent Event)> result in results)
            {
                if (result.IsFailure)
                {
                    return Result<(IReadOnlyList<byte[]>, IReadOnlyList<VectorCompressionEvent>)>.Failure(result.Error);
                }

                compressedResults.Add(result.Value.CompressedData);
                compressionEvents.Add(result.Value.Event);
            }

            return Result<(IReadOnlyList<byte[]>, IReadOnlyList<VectorCompressionEvent>)>.Success(
                (compressedResults.AsReadOnly(), compressionEvents.AsReadOnly()));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (AggregateException ex)
        {
            return Result<(IReadOnlyList<byte[]>, IReadOnlyList<VectorCompressionEvent>)>.Failure($"Batch compression failed (aggregate): {ex.InnerException?.Message ?? ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<(IReadOnlyList<byte[]>, IReadOnlyList<VectorCompressionEvent>)>.Failure($"Batch compression failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Analyzes a vector and returns compression statistics preview.
    /// Pure function with no side effects.
    /// </summary>
    public static Result<CompressionPreview> Preview(float[] vector, CompressionConfig config)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(vector);
            ArgumentNullException.ThrowIfNull(config);

            FourierVectorCompressor fftCompressor = new FourierVectorCompressor(
                config.TargetDimension,
                FourierVectorCompressor.CompressionStrategy.HighestMagnitude);

            DCTVectorCompressor dctCompressor = new DCTVectorCompressor(
                config.TargetDimension,
                config.EnergyThreshold);

            DCTCompressedVector dct = dctCompressor.Compress(vector);
            CompressedVector fft = fftCompressor.Compress(vector);

            CompressionPreview preview = new CompressionPreview(
                OriginalDimension: vector.Length,
                OriginalSizeBytes: vector.Length * sizeof(float),
                DCTCompressedSize: dct.ToBytes().Length,
                DCTEnergyRetained: dct.EnergyRetained,
                FFTCompressedSize: fft.ToBytes().Length,
                FFTCompressionRatio: fft.CompressionRatio,
                QuantizedDCTSize: dctCompressor.Quantize(dct, 8).ToBytes().Length);

            return Result<CompressionPreview>.Success(preview);
        }
        catch (ArgumentNullException ex)
        {
            return Result<CompressionPreview>.Failure($"Preview input error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<CompressionPreview>.Failure($"Preview generation failed: {ex.Message}");
        }
    }

    private static CompressionMethod SelectOptimalMethod(float[] vector)
    {
        // Use DCT for typical embedding vectors - it's more efficient for real-valued data
        // FFT is better when there are periodic patterns

        // Quick heuristic: check if vector has periodic structure
        double periodicScore = ComputePeriodicityScore(vector);

        return periodicScore > 0.7 ? CompressionMethod.FFT : CompressionMethod.DCT;
    }

    private static double ComputePeriodicityScore(float[] vector)
    {
        if (vector.Length < 16)
            return 0;

        // Simple autocorrelation check for periodicity
        int testLag = vector.Length / 4;
        double mean = vector.Average();
        double variance = vector.Sum(v => (v - mean) * (v - mean)) / vector.Length;

        if (variance < float.Epsilon)
            return 0;

        double autocorr = 0;
        for (int i = 0; i < vector.Length - testLag; i++)
        {
            autocorr += (vector[i] - mean) * (vector[i + testLag] - mean);
        }

        autocorr /= (vector.Length - testLag) * variance;

        return Math.Abs(autocorr);
    }

    private static byte[] WrapWithHeader(CompressionMethod method, byte[] payload)
    {
        // Header: 4 bytes magic + 1 byte method + 4 bytes payload length
        byte[] result = new byte[9 + payload.Length];
        result[0] = (byte)'O';
        result[1] = (byte)'V';
        result[2] = (byte)'C';
        result[3] = (byte)'1'; // Version 1
        result[4] = (byte)method;
        BitConverter.GetBytes(payload.Length).CopyTo(result, 5);
        payload.CopyTo(result, 9);
        return result;
    }

    private static (CompressionMethod Method, byte[] Payload) UnwrapHeader(byte[] data)
    {
        if (data.Length < 9 || data[0] != 'O' || data[1] != 'V' || data[2] != 'C')
        {
            throw new InvalidOperationException("Invalid compressed vector format");
        }

        CompressionMethod method = (CompressionMethod)data[4];
        int payloadLen = BitConverter.ToInt32(data, 5);
        byte[] payload = new byte[payloadLen];
        Array.Copy(data, 9, payload, 0, payloadLen);

        return (method, payload);
    }

    private static double CosineSimilarity(float[] a, float[] b)
    {
        int len = Math.Min(a.Length, b.Length);
        double dot = 0, normA = 0, normB = 0;

        for (int i = 0; i < len; i++)
        {
            dot += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }

        if (normA == 0 || normB == 0)
            return 0;

        return dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }
}