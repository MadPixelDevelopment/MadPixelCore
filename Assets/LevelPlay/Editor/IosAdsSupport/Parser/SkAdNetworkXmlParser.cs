using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Services.LevelPlay.Editor
{
    internal class SkAdNetworkXmlParser
    {
        private const string k_SkAdNetworkIdentifier = "SKAdNetworkIdentifier";

        public static HashSet<string> ParseSource(ISkAdNetworkSource source)
        {
            var foundIds = new HashSet<string>();
            try
            {
                var xmlDocument = EditorServices.Instance.XmlDocumentFactory.CreateXmlDocument();

                using (var stream = source.Open())
                {
                    if (stream == null)
                    {
                        EditorServices.Instance.LevelPlayLogger.LogWarning("[Unity SKAdNetwork Parser] Unable to parse SKAdNetwork file: {source.Path}");
                        return foundIds;
                    }

                    xmlDocument.Load(stream);
                }

                var items = xmlDocument.GetElementsByTagName("key");
                for (var x = 0; x < items.Count; x++)
                {
                    if (items[x].InnerText == k_SkAdNetworkIdentifier)
                    {
                        var nextSibling = items[x]?.NextSibling;
                        if (nextSibling != null)
                        {
                            foundIds.Add(nextSibling.InnerText);
                        }
                    }
                }
            }
            catch (Exception)
            {
                EditorServices.Instance.LevelPlayLogger.LogWarning($"[Unity SKAdNetwork Parser] Unable to parse SKAdNetwork file: {source.Path}");
            }

            return foundIds;
        }
    }
}
