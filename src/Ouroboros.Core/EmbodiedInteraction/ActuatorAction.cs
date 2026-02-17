namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// An action to execute through an actuator.
/// </summary>
/// <param name="ActionType">Type of action (e.g., "speak", "turn_on", "set_color").</param>
/// <param name="Parameters">Action parameters.</param>
public sealed record ActuatorAction(
    string ActionType,
    IReadOnlyDictionary<string, object>? Parameters = null)
{
    /// <summary>
    /// Creates a speak action.
    /// </summary>
    public static ActuatorAction Speak(string text, string? emotion = null) =>
        new("speak", new Dictionary<string, object>
        {
            ["text"] = text,
            ["emotion"] = emotion ?? "neutral"
        });

    /// <summary>
    /// Creates a power on action.
    /// </summary>
    public static ActuatorAction TurnOn() => new("turn_on");

    /// <summary>
    /// Creates a power off action.
    /// </summary>
    public static ActuatorAction TurnOff() => new("turn_off");

    /// <summary>
    /// Creates a set color action.
    /// </summary>
    public static ActuatorAction SetColor(byte r, byte g, byte b) =>
        new("set_color", new Dictionary<string, object>
        {
            ["red"] = r,
            ["green"] = g,
            ["blue"] = b
        });

    /// <summary>
    /// Creates a pan left action.
    /// </summary>
    /// <param name="speed">Pan speed (0.0 to 1.0).</param>
    /// <param name="durationMs">Duration in milliseconds.</param>
    public static ActuatorAction PanLeft(float speed = 0.5f, int durationMs = 500) =>
        new("pan_left", new Dictionary<string, object>
        {
            ["speed"] = speed,
            ["duration_ms"] = durationMs
        });

    /// <summary>
    /// Creates a pan right action.
    /// </summary>
    /// <param name="speed">Pan speed (0.0 to 1.0).</param>
    /// <param name="durationMs">Duration in milliseconds.</param>
    public static ActuatorAction PanRight(float speed = 0.5f, int durationMs = 500) =>
        new("pan_right", new Dictionary<string, object>
        {
            ["speed"] = speed,
            ["duration_ms"] = durationMs
        });

    /// <summary>
    /// Creates a tilt up action.
    /// </summary>
    /// <param name="speed">Tilt speed (0.0 to 1.0).</param>
    /// <param name="durationMs">Duration in milliseconds.</param>
    public static ActuatorAction TiltUp(float speed = 0.5f, int durationMs = 500) =>
        new("tilt_up", new Dictionary<string, object>
        {
            ["speed"] = speed,
            ["duration_ms"] = durationMs
        });

    /// <summary>
    /// Creates a tilt down action.
    /// </summary>
    /// <param name="speed">Tilt speed (0.0 to 1.0).</param>
    /// <param name="durationMs">Duration in milliseconds.</param>
    public static ActuatorAction TiltDown(float speed = 0.5f, int durationMs = 500) =>
        new("tilt_down", new Dictionary<string, object>
        {
            ["speed"] = speed,
            ["duration_ms"] = durationMs
        });

    /// <summary>
    /// Creates a combined pan/tilt move action.
    /// </summary>
    /// <param name="panSpeed">Pan speed (-1.0 left to 1.0 right).</param>
    /// <param name="tiltSpeed">Tilt speed (-1.0 down to 1.0 up).</param>
    /// <param name="durationMs">Duration in milliseconds.</param>
    public static ActuatorAction PtzMove(float panSpeed, float tiltSpeed, int durationMs = 500) =>
        new("ptz_move", new Dictionary<string, object>
        {
            ["pan_speed"] = panSpeed,
            ["tilt_speed"] = tiltSpeed,
            ["duration_ms"] = durationMs
        });

    /// <summary>
    /// Creates a go-to-home-position action.
    /// </summary>
    public static ActuatorAction PtzHome() => new("ptz_home");

    /// <summary>
    /// Creates a stop-all-movement action.
    /// </summary>
    public static ActuatorAction PtzStop() => new("ptz_stop");

    /// <summary>
    /// Creates a go-to-preset action.
    /// </summary>
    /// <param name="presetToken">Preset identifier.</param>
    public static ActuatorAction PtzGoToPreset(string presetToken) =>
        new("ptz_go_to_preset", new Dictionary<string, object>
        {
            ["preset_token"] = presetToken
        });

    /// <summary>
    /// Creates a save-preset action.
    /// </summary>
    /// <param name="presetName">Name for the preset position.</param>
    public static ActuatorAction PtzSetPreset(string presetName) =>
        new("ptz_set_preset", new Dictionary<string, object>
        {
            ["preset_name"] = presetName
        });

    /// <summary>
    /// Creates a patrol sweep action (pan left, right, then return center).
    /// </summary>
    /// <param name="speed">Sweep speed (0.0 to 1.0).</param>
    public static ActuatorAction PtzPatrolSweep(float speed = 0.3f) =>
        new("ptz_patrol_sweep", new Dictionary<string, object>
        {
            ["speed"] = speed
        });
}