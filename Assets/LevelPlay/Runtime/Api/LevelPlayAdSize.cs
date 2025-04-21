using System;

namespace com.unity3d.mediation
{
    /// <summary>
    /// Represents dimensions and descriptions for different types of advertisement sizes.
    /// </summary>
    [Obsolete("The namespace com.unity3d.mediation is deprecated. Use LevelPlayAdSize under the new namespace Unity.Services.LevelPlay.")]
    public class LevelPlayAdSize
    {
        readonly Unity.Services.LevelPlay.LevelPlayAdSize m_AdSize;

        LevelPlayAdSize(Unity.Services.LevelPlay.LevelPlayAdSize adSize)
        {
            m_AdSize = adSize;
        }

        // Forwarding properties
        public string Description => m_AdSize.Description;
        public int Width => m_AdSize.Width;
        public int Height => m_AdSize.Height;
        public int CustomWidth => m_AdSize.CustomWidth;

        public override string ToString()
        {
            return m_AdSize.ToString();
        }

        // Forward static properties
        public static LevelPlayAdSize BANNER => new LevelPlayAdSize(Unity.Services.LevelPlay.LevelPlayAdSize.BANNER);
        public static LevelPlayAdSize LARGE => new LevelPlayAdSize(Unity.Services.LevelPlay.LevelPlayAdSize.LARGE);
        public static LevelPlayAdSize MEDIUM_RECTANGLE => new LevelPlayAdSize(Unity.Services.LevelPlay.LevelPlayAdSize.MEDIUM_RECTANGLE);
        public static LevelPlayAdSize LEADERBOARD => new LevelPlayAdSize(Unity.Services.LevelPlay.LevelPlayAdSize.LEADERBOARD);

        // Forward static methods
        public static LevelPlayAdSize CreateCustomBannerSize(int width, int height)
        {
            return new LevelPlayAdSize(Unity.Services.LevelPlay.LevelPlayAdSize.CreateCustomBannerSize(width, height));
        }

        public static LevelPlayAdSize CreateAdaptiveAdSize(int customWidth = -1)
        {
            return new LevelPlayAdSize(Unity.Services.LevelPlay.LevelPlayAdSize.CreateAdaptiveAdSize(customWidth));
        }

        internal Unity.Services.LevelPlay.IPlatformLevelPlayAdSize GetPlatformLevelPlayAdSize()
        {
            return m_AdSize.GetPlatformLevelPlayAdSize();
        }
    }
}

namespace Unity.Services.LevelPlay
{
    /// <summary>
    /// Represents dimensions and descriptions for different types of advertisement sizes.
    /// </summary>
    public class LevelPlayAdSize
    {
        IPlatformLevelPlayAdSize m_PlatformLevelPlayAdSize;

        internal IPlatformLevelPlayAdSize GetPlatformLevelPlayAdSize()
        {
            return m_PlatformLevelPlayAdSize;
        }

        /// <summary>
        /// Standard banner size
        /// </summary>
        public static LevelPlayAdSize BANNER = new LevelPlayAdSize(PlatformLevelPlayAdSizeType.Banner);

        /// <summary>
        /// Standard large size
        /// </summary>
        public static LevelPlayAdSize LARGE = new LevelPlayAdSize(PlatformLevelPlayAdSizeType.Large);

        /// <summary>
        /// Standard mrec size
        /// </summary>
        public static LevelPlayAdSize MEDIUM_RECTANGLE = new LevelPlayAdSize(PlatformLevelPlayAdSizeType.MediumRectangle);

        /// <summary>
        /// Standard leaderboard size
        /// </summary>
        public static LevelPlayAdSize LEADERBOARD = new LevelPlayAdSize(PlatformLevelPlayAdSizeType.LeaderBoard);

        // Constructor for the com.unity3d.mediation namespace to use.
        internal LevelPlayAdSize() : this(PlatformLevelPlayAdSizeType.Unknown) {}

        // internal constructor for testing and private use inside this class
        internal LevelPlayAdSize(IPlatformLevelPlayAdSize adSize)
        {
            m_PlatformLevelPlayAdSize = adSize;
        }

        LevelPlayAdSize(PlatformLevelPlayAdSizeType adSizeType)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            m_PlatformLevelPlayAdSize = new UnsupportedLevelPlayAdSize();
#elif (UNITY_IPHONE || UNITY_IOS)
            m_PlatformLevelPlayAdSize = new IosLevelPlayAdSize(adSizeType);
#elif UNITY_ANDROID
            m_PlatformLevelPlayAdSize = new AndroidLevelPlayAdSize(adSizeType);
#endif
        }

        LevelPlayAdSize(int width, int height)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            m_PlatformLevelPlayAdSize = new UnsupportedLevelPlayAdSize();
#elif (UNITY_IPHONE || UNITY_IOS)
            m_PlatformLevelPlayAdSize = new IosLevelPlayAdSize(width, height);
#elif UNITY_ANDROID
            m_PlatformLevelPlayAdSize = new AndroidLevelPlayAdSize(width, height);
#endif
        }

        /// <summary>
        /// Creates a custom banner size with specified dimensions.
        /// </summary>
        /// <param name="width">The width of the custom banner in pixels.</param>
        /// <param name="height">The height of the custom banner in pixels.</param>
        /// <returns>A new instance of <see cref="LevelPlayAdSize"/> representing the custom size.</returns>
        public static LevelPlayAdSize CreateCustomBannerSize(int width, int height)
        {
            return new LevelPlayAdSize(width, height);
        }

        /// <summary>
        /// Creates an adaptive banner with default screen width.
        /// The default screen width is used if the custom width is not specified. Specify the custom width if necessary.
        /// </summary>
        /// <param name="customWidth">Custom width of the adaptive banner container.
        /// On Android, it is measured in DP(density-independent pixels), and on IOS, it is in measured in Points.</param>
        /// <returns>A new instance of <see cref="LevelPlayAdSize"/> representing the Adaptive size.</returns>
        public static LevelPlayAdSize CreateAdaptiveAdSize(int customWidth = -1)
        {
            if (customWidth < 0)
            {
#if UNITY_EDITOR || UNITY_STANDALONE
                return new LevelPlayAdSize(new UnsupportedLevelPlayAdSize());
#elif (UNITY_IPHONE || UNITY_IOS)
                return new LevelPlayAdSize(IosLevelPlayAdSize.CreateAdaptiveAdSize());
#elif UNITY_ANDROID
                return new LevelPlayAdSize(AndroidLevelPlayAdSize.CreateAdaptiveAdSize());
#else
                throw new PlatformNotSupportedException("Adaptive Ad Size is not supported on this platform.");
#endif
            }
            else
            {
#if UNITY_EDITOR || UNITY_STANDALONE
                return new LevelPlayAdSize(new UnsupportedLevelPlayAdSize());
#elif (UNITY_IPHONE || UNITY_IOS)
                return new LevelPlayAdSize(IosLevelPlayAdSize.CreateAdaptiveAdSize(customWidth));
#elif UNITY_ANDROID
                return new LevelPlayAdSize(AndroidLevelPlayAdSize.CreateAdaptiveAdSize(customWidth));
#else
                throw new PlatformNotSupportedException("Adaptive Ad Size is not supported on this platform.");
#endif
            }
        }

        /// <summary>
        /// Description for the banner
        /// </summary>
        public string Description
        {
            get
            {
                switch (m_PlatformLevelPlayAdSize.AdSizeType)
                {
                    case PlatformLevelPlayAdSizeType.Banner:
                        return "BANNER";
                    case PlatformLevelPlayAdSizeType.Large:
                        return "LARGE";
                    case PlatformLevelPlayAdSizeType.MediumRectangle:
                        return "MEDIUM_RECTANGLE";
                    case PlatformLevelPlayAdSizeType.LeaderBoard:
                        return "LEADERBOARD";
                    case PlatformLevelPlayAdSizeType.Custom:
                        return "CUSTOM";
                    case PlatformLevelPlayAdSizeType.Adaptive:
                        return "ADAPTIVE";
                    default:
                        return "UNKNOWN";
                }
            }
        }

        /// <summary>
        /// Width of the banner
        /// </summary>
        public int Width { get { return m_PlatformLevelPlayAdSize.Width; } }

        /// <summary>
        /// Height of the banner
        /// </summary>
        public int Height { get { return m_PlatformLevelPlayAdSize.Height; } }

        /// <summary>
        /// Custom width of the banner in DP
        /// </summary>
        [Obsolete("CustomWidth will be removed in version 9.0.0. Please use LevelPlayAdSize.Width instead.")]
        public int CustomWidth { get { return m_PlatformLevelPlayAdSize.Width; } }

        public override string ToString()
        {
            return string.Format("Description: {0}, Width: {1}, Height: {2}", Description, Width, Height);
        }
    }
}
