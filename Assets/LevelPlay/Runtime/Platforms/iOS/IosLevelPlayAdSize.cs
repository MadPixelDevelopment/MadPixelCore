#if UNITY_IOS
using System;
using System.Runtime.InteropServices;
using com.unity3d.mediation;

namespace Unity.Services.LevelPlay
{
#pragma warning disable 0618
    internal class IosLevelPlayAdSize : IosNativeObject, IPlatformLevelPlayAdSize
    {
#pragma warning restore 0618
        PlatformLevelPlayAdSizeType m_PlatformLevelPlayAdSizeType;

        internal IosLevelPlayAdSize(PlatformLevelPlayAdSizeType adSizeType) : base(false)
        {
            m_PlatformLevelPlayAdSizeType = adSizeType;
            NativePtr = LPMCreateAdSizeWithType(adSizeType, 0, 0);
        }

        internal IosLevelPlayAdSize(int width, int height) : base(false)
        {
            m_PlatformLevelPlayAdSizeType = PlatformLevelPlayAdSizeType.Custom;
            NativePtr = LPMCreateAdSizeWithType(PlatformLevelPlayAdSizeType.Custom, width, height);
        }

        IosLevelPlayAdSize(PlatformLevelPlayAdSizeType adSizeType, IntPtr nativePtr) : base(false)
        {
            m_PlatformLevelPlayAdSizeType = adSizeType;
            NativePtr = nativePtr;
        }

        public int Width
        {
            get
            {
                return LPMGetAdSizeWidth(NativePtr);
            }
        }

        public int Height
        {
            get
            {
                return LPMGetAdSizeHeight(NativePtr);
            }
        }

        public PlatformLevelPlayAdSizeType AdSizeType
        {
            get
            {
                return m_PlatformLevelPlayAdSizeType;
            }
        }

        internal static IosLevelPlayAdSize CreateAdaptiveAdSize()
        {
            return new IosLevelPlayAdSize(PlatformLevelPlayAdSizeType.Adaptive, LPMCreateAdaptiveAdSize());
        }

        internal static IosLevelPlayAdSize CreateAdaptiveAdSize(int width)
        {
            return new IosLevelPlayAdSize(PlatformLevelPlayAdSizeType.Adaptive, LPMCreateAdaptiveAdSizeWithWidth(width));
        }

        // Width and height are only used if the type is Custom
        [DllImport("__Internal", EntryPoint = "LPMCreateAdSizeWithType")]
        static extern IntPtr LPMCreateAdSizeWithType(PlatformLevelPlayAdSizeType adSizeType, int width, int height);

        [DllImport("__Internal", EntryPoint = "LPMCreateAdaptiveAdSize")]
        static extern IntPtr LPMCreateAdaptiveAdSize();

        [DllImport("__Internal", EntryPoint = "LPMCreateAdaptiveAdSizeWithWidth")]
        static extern IntPtr LPMCreateAdaptiveAdSizeWithWidth(int width);

        [DllImport("__Internal", EntryPoint = "LPMGetAdSizeWidth")]
        static extern int LPMGetAdSizeWidth(IntPtr adSize);

        [DllImport("__Internal", EntryPoint = "LPMGetAdSizeHeight")]
        static extern int LPMGetAdSizeHeight(IntPtr adSize);
    }
}
#endif
