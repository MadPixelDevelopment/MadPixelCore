#if UNITY_EDITOR
using UnityEngine;

namespace Unity.Services.LevelPlay
{
    class EditorLevelPlayAdSize : IPlatformLevelPlayAdSize
    {
        PlatformLevelPlayAdSizeType m_Type;
        int m_Width;
        int m_Height;

        public int Width => m_Width;

        public int Height => m_Height;

        public PlatformLevelPlayAdSizeType AdSizeType => m_Type;

        internal EditorLevelPlayAdSize(PlatformLevelPlayAdSizeType type)
        {
            SetType(type);
        }

        internal EditorLevelPlayAdSize(int width, int height)
        {
            this.m_Width = width;
            this.m_Height = height;
            this.m_Type = PlatformLevelPlayAdSizeType.Custom;
        }

        internal static EditorLevelPlayAdSize CreateAdaptiveAdSize(int width = 0)
        {
            EditorLevelPlayAdSize editorAdSize = new EditorLevelPlayAdSize(Screen.width, 120);
            editorAdSize.m_Type = PlatformLevelPlayAdSizeType.Adaptive;
            return editorAdSize;
        }

        private void SetType(PlatformLevelPlayAdSizeType type)
        {
            m_Type = type;
            switch (type)
            {
                case PlatformLevelPlayAdSizeType.Banner:
                    this.m_Height = 50;
                    this.m_Width = 320;
                    break;
                case PlatformLevelPlayAdSizeType.Large:
                    this.m_Height = 90;
                    this.m_Width = 320;
                    break;
                case PlatformLevelPlayAdSizeType.MediumRectangle:
                    this.m_Height = 250;
                    this.m_Width = 300;
                    break;
                case PlatformLevelPlayAdSizeType.LeaderBoard:
                    this.m_Height = 90;
                    this.m_Width = 728;
                    break;
                default:
                    this.m_Height = 0;
                    this.m_Width = 8;
                    break;
            }
        }
    }
}
#endif
