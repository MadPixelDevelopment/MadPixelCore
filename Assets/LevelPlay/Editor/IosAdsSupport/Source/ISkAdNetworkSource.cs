using System.IO;

namespace Unity.Services.LevelPlay.Editor
{
    internal interface ISkAdNetworkSource
    {
        string Path { get; }
        Stream Open();
    }
}
