using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Linq;
using UnityEngine.Networking;

namespace Unity.Services.LevelPlay.Editor
{
    internal sealed class Adapter : IEquatable<Adapter>
    {
        internal string KeyName { get; }
        internal string DisplayName { get; }
        internal string DependencyXmlURL { get; }
        internal string DependencyXmlFileName { get; }
        internal string SKAdNetworkIdXmlURL { get; }
        internal bool IsRecommended { get; }
        internal bool IsNew { get; }

        internal Dictionary<string, AdapterVersion> Versions { get; }

        internal Adapter(string keyName, string displayName, string dependencyXmlURL, string dependencyXmlFileName, string skAdNetworkIdUrl, bool isRecommended, bool isNew, Dictionary<string, AdapterVersion> versions)
        {
            KeyName = keyName;
            DisplayName = displayName;
            DependencyXmlURL = dependencyXmlURL;
            DependencyXmlFileName = dependencyXmlFileName;
            SKAdNetworkIdXmlURL = skAdNetworkIdUrl;
            IsRecommended = isRecommended;
            IsNew = isNew;
            Versions = versions;
        }

        internal Adapter(string keyName, Dictionary<string, object> jsonDictionary)
        {
            KeyName = keyName;
            DisplayName = jsonDictionary["displayName"] as string;
            DependencyXmlURL = jsonDictionary["dependencyXmlURL"] as string;
            DependencyXmlFileName = jsonDictionary["dependencyXmlFileName"] as string;
            SKAdNetworkIdXmlURL = jsonDictionary.TryGetValue("SKAdNetworkIdXmlURL", out var value) ? value as string : string.Empty;
            if (jsonDictionary.ContainsKey("isRecommended"))
            {
                IsRecommended = (bool)jsonDictionary["isRecommended"];
            }
            else
            {
                IsRecommended = false;
            }
            if (jsonDictionary.ContainsKey("isNew"))
            {
                IsNew = (bool)jsonDictionary["isNew"];
            }
            else
            {
                IsNew = false;
            }
            Versions = new Dictionary<string, AdapterVersion>();
            var versions = jsonDictionary["versions"] as Dictionary<string, object>;
            foreach (var version in versions)
            {
                Versions.Add(version.Key, new AdapterVersion(version.Key, version.Value as Dictionary<string, object>));
            }
        }

        internal string GetDependencyXmlPath()
        {
            return Path.Combine("Assets", "LevelPlay", "Editor", this.DependencyXmlFileName);
        }

        public bool Equals(Adapter other)
        {
            if (other == null)
            {
                return false;
            }

            if (!KeyName.Equals(other.KeyName))
            {
                return false;
            }

            if (!DisplayName.Equals(other.DisplayName))
            {
                return false;
            }

            if (!DependencyXmlURL.Equals(other.DependencyXmlURL))
            {
                return false;
            }

            if (!DependencyXmlFileName.Equals(other.DependencyXmlFileName))
            {
                return false;
            }

            if (!SKAdNetworkIdXmlURL.Equals(other.SKAdNetworkIdXmlURL))
            {
                return false;
            }

            if (IsRecommended != other.IsRecommended)
            {
                return false;
            }

            if (IsNew != other.IsNew)
            {
                return false;
            }

            if (Versions.Count != other.Versions.Count)
            {
                return false;
            }

            foreach (var version in Versions)
            {
                if (!other.Versions.ContainsKey(version.Key))
                {
                    return false;
                }

                if (!version.Value.Equals(other.Versions[version.Key]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
