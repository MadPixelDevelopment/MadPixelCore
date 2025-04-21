using System;
using UnityEngine;

namespace com.unity3d.mediation
{
    /// <summary>
    /// Represents positions on the screen where banner ads can be placed.
    /// </summary>
    [Obsolete("The namespace com.unity3d.mediation is deprecated. Use LevelPlayBannerPosition under the new namespace Unity.Services.LevelPlay.")]
    public class LevelPlayBannerPosition
    {
        readonly Unity.Services.LevelPlay.LevelPlayBannerPosition m_Position;

        internal string Description => m_Position.Description;
        internal Vector2 Position => m_Position.Position;

        LevelPlayBannerPosition(Unity.Services.LevelPlay.LevelPlayBannerPosition position)
        {
            m_Position = position;
        }

        /// <summary>
        /// The banner position at the top-left corner of the screen.
        /// </summary>
        public static readonly LevelPlayBannerPosition TopLeft = new LevelPlayBannerPosition(Unity.Services.LevelPlay.LevelPlayBannerPosition.TopLeft);

        /// <summary>
        /// The banner position at the top-center of the screen.
        /// </summary>
        public static readonly LevelPlayBannerPosition TopCenter = new LevelPlayBannerPosition(Unity.Services.LevelPlay.LevelPlayBannerPosition.TopCenter);

        /// <summary>
        /// The banner position at the top-right corner of the screen.
        /// </summary>
        public static readonly LevelPlayBannerPosition TopRight = new LevelPlayBannerPosition(Unity.Services.LevelPlay.LevelPlayBannerPosition.TopRight);

        /// <summary>
        /// The banner position at the center-left side of the screen.
        /// </summary>
        public static readonly LevelPlayBannerPosition CenterLeft = new LevelPlayBannerPosition(Unity.Services.LevelPlay.LevelPlayBannerPosition.CenterLeft);

        /// <summary>
        /// The banner position at the center of the screen.
        /// </summary>
        public static readonly LevelPlayBannerPosition Center = new LevelPlayBannerPosition(Unity.Services.LevelPlay.LevelPlayBannerPosition.Center);

        /// <summary>
        /// The banner position at the center-right side of the screen.
        /// </summary>
        public static readonly LevelPlayBannerPosition CenterRight = new LevelPlayBannerPosition(Unity.Services.LevelPlay.LevelPlayBannerPosition.CenterRight);

        /// <summary>
        /// The banner position at the bottom-left corner of the screen.
        /// </summary>
        public static readonly LevelPlayBannerPosition BottomLeft = new LevelPlayBannerPosition(Unity.Services.LevelPlay.LevelPlayBannerPosition.BottomLeft);

        /// <summary>
        /// The banner position at the bottom-center of the screen.
        /// </summary>
        public static readonly LevelPlayBannerPosition BottomCenter = new LevelPlayBannerPosition(Unity.Services.LevelPlay.LevelPlayBannerPosition.BottomCenter);

        /// <summary>
        /// The banner position at the bottom-right corner of the screen.
        /// </summary>
        public static readonly LevelPlayBannerPosition BottomRight = new LevelPlayBannerPosition(Unity.Services.LevelPlay.LevelPlayBannerPosition.BottomRight);

        /// <summary>
        /// Initializes a new instance of the <see cref="LevelPlayBannerPosition"/> class with a custom position.
        /// </summary>
        /// <param name="position">The custom position vector. The starting point is always from the top-left corner of the screen.</param>
        public LevelPlayBannerPosition(Vector2 position)
        {
            m_Position = new Unity.Services.LevelPlay.LevelPlayBannerPosition(position);
        }
    }
}

namespace Unity.Services.LevelPlay
{
    /// <summary>
    /// Represents positions on the screen where banner ads can be placed.
    /// </summary>
    public class LevelPlayBannerPosition
    {
        enum Presets
        {
            TopLeft,
            TopCenter,
            TopRight,
            CenterLeft,
            Center,
            CenterRight,
            BottomLeft,
            BottomCenter,
            BottomRight,
            Custom
        }

        internal readonly Vector2 Position;

        /// <summary>
        /// Gets the description of the banner position.
        /// </summary>
        public readonly string Description;

        /// <summary>
        /// The banner position at the top-left corner of the screen.
        /// </summary>
        public static readonly LevelPlayBannerPosition TopLeft = new LevelPlayBannerPosition(Presets.TopLeft);

        /// <summary>
        /// The banner position at the top-center of the screen.
        /// </summary>
        public static readonly LevelPlayBannerPosition TopCenter = new LevelPlayBannerPosition(Presets.TopCenter);

        /// <summary>
        /// The banner position at the top-right corner of the screen.
        /// </summary>
        public static readonly LevelPlayBannerPosition TopRight = new LevelPlayBannerPosition(Presets.TopRight);

        /// <summary>
        /// The banner position at the center-left side of the screen.
        /// </summary>
        public static readonly LevelPlayBannerPosition CenterLeft = new LevelPlayBannerPosition(Presets.CenterLeft);

        /// <summary>
        /// The banner position at the center of the screen.
        /// </summary>
        public static readonly LevelPlayBannerPosition Center = new LevelPlayBannerPosition(Presets.Center);

        /// <summary>
        /// The banner position at the center-right side of the screen.
        /// </summary>
        public static readonly LevelPlayBannerPosition CenterRight = new LevelPlayBannerPosition(Presets.CenterRight);

        /// <summary>
        /// The banner position at the bottom-left corner of the screen.
        /// </summary>
        public static readonly LevelPlayBannerPosition BottomLeft = new LevelPlayBannerPosition(Presets.BottomLeft);

        /// <summary>
        /// The banner position at the bottom-center of the screen.
        /// </summary>
        public static readonly LevelPlayBannerPosition BottomCenter = new LevelPlayBannerPosition(Presets.BottomCenter);

        /// <summary>
        /// The banner position at the bottom-right corner of the screen.
        /// </summary>
        public static readonly LevelPlayBannerPosition BottomRight = new LevelPlayBannerPosition(Presets.BottomRight);

        LevelPlayBannerPosition(Presets presets, Vector2 position = default)
        {
            Position = position;
            Description = presets.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LevelPlayBannerPosition"/> class with a custom position.
        /// </summary>
        /// <param name="position">The custom position vector. The starting point is always from the top-left corner of the screen.</param>
        public LevelPlayBannerPosition(Vector2 position) : this(Presets.Custom, position) {}

        /// <summary>
        /// Returns a string representation of the banner position.
        /// </summary>
        public override string ToString()
        {
            return $"Banner Position: { Description }";
        }
    }
}
