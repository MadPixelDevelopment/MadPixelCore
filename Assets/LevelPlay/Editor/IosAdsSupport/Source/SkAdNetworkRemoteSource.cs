using System.IO;
using System.Net;

namespace Unity.Services.LevelPlay.Editor
{
    internal class SkAdNetworkRemoteSource : ISkAdNetworkSource
    {
        public string Path { get; }

        public SkAdNetworkRemoteSource(string path)
        {
            Path = path;
        }

        public Stream Open()
        {
            return new WebClient().OpenRead(Path);
        }
    }
}
