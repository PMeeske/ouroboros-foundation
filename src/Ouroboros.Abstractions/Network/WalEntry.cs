// <copyright file="WalEntry.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Ouroboros.Abstractions.Network;

/// <summary>
/// Simplified representation of a Write-Ahead Log entry.
/// This is a minimal version used by interfaces; the full implementation remains in Network.
/// </summary>
public sealed record WalEntry
{
    /// <summary>
    /// Gets the unique identifier for this log entry.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the type of entry (NodeAdded, EdgeAdded, etc.).
    /// </summary>
    public string EntryType { get; init; }

    /// <summary>
    /// Gets the timestamp when this entry was created.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// Gets the serialized data for this entry.
    /// </summary>
    public string DataJson { get; init; }

    /// <summary>
    /// Gets the sequence number of this entry.
    /// </summary>
    public long SequenceNumber { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WalEntry"/> class.
    /// </summary>
    public WalEntry(
        Guid id,
        string entryType,
        DateTimeOffset timestamp,
        string dataJson,
        long sequenceNumber)
    {
        this.Id = id;
        this.EntryType = entryType ?? throw new ArgumentNullException(nameof(entryType));
        this.Timestamp = timestamp;
        this.DataJson = dataJson ?? throw new ArgumentNullException(nameof(dataJson));
        this.SequenceNumber = sequenceNumber;
    }
}