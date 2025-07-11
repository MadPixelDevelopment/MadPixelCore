using System;

[Obsolete("This interface will be made private in version 9.0.0.")]
/// <summary>
/// Interface representing LevelPlay's user segment
/// </summary>
public interface IUnitySegment
{
    /// <summary>
    /// Event triggered when a segment is received
    /// </summary>
    event Action<String> OnSegmentRecieved;
}
