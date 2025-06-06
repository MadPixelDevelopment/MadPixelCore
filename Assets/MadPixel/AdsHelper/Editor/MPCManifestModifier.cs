#if UNITY_ANDROID
using UnityEditor.Android;
using System.Xml;
using UnityEngine;

namespace MadPixel.Editor {
    public class ManifestModifier : IPostGenerateGradleAndroidProject {
        public int callbackOrder { get { return 0; } }
        private const string ANALYTICS_STORAGE_TAG = "google_analytics_default_allow_analytics_storage";
        private const string AD_PERSONALIZATION_TAG = "google_analytics_default_allow_ad_personalization_signals";
        private const string AD_USER_DATA_TAG = "google_analytics_default_allow_ad_user_data";
        private const string AD_STORAGE_TAG = "google_analytics_default_allow_ad_storage";
        private const string NAMESPACE_URI = "http://schemas.android.com/apk/res/android";

        public void OnPostGenerateGradleAndroidProject(string path) {
            string manifestPath = path + "/src/main/AndroidManifest.xml";
            XmlDocument doc = new XmlDocument();
            doc.Load(manifestPath);

            XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("android", NAMESPACE_URI);

            XmlNode applicationNode = doc.SelectSingleNode("manifest/application");
            if (applicationNode == null) return;

            // Check if the tags already exist
            bool hasAnalyticsStorage = false;
            bool hasAdStorage = false;
            bool hasAdPersonalization = false;
            bool hasAdUserData = false;

            foreach (XmlNode node in applicationNode.ChildNodes) {
                if (node.Name == "meta-data") {
                    var nameAttr = node.Attributes["name", nsManager.LookupNamespace("android")];
                    if (nameAttr != null) {
                        if (nameAttr.Value == ANALYTICS_STORAGE_TAG) {
                            SetApplicationTag(node);
                            hasAnalyticsStorage = true;
                        }
                        if (nameAttr.Value == AD_STORAGE_TAG) {
                            SetApplicationTag(node, "false");
                            hasAdStorage = true;
                        }

                        if (nameAttr.Value == AD_PERSONALIZATION_TAG) {
                            SetApplicationTag(node, "false");
                            hasAdPersonalization = true;
                        }

                        if (nameAttr.Value == AD_USER_DATA_TAG) {
                            SetApplicationTag(node, "false");
                            hasAdUserData = true;
                        }
                    }
                }
            }

            // Add the tags if they don't exist
            if (!hasAnalyticsStorage) {
                AddApplicationTag(doc, applicationNode, ANALYTICS_STORAGE_TAG);
            }

            if (!hasAdStorage) {
                AddApplicationTag(doc, applicationNode, AD_STORAGE_TAG, "false");
            }

            if (!hasAdPersonalization) {
                AddApplicationTag(doc, applicationNode, AD_PERSONALIZATION_TAG, "false");
            }

            if (!hasAdUserData) {
                AddApplicationTag(doc, applicationNode, AD_USER_DATA_TAG, "false");
            }

            doc.Save(manifestPath);

            foreach (var childNode in applicationNode.ChildNodes) {
                Debug.Log($"[MadPixel] manifestnode: {childNode.ToString()}" );
            }
        }

        private void AddApplicationTag(XmlDocument a_doc, XmlNode a_node, string a_tag, string a_value = "true") {
            XmlElement analyticsElement = a_doc.CreateElement("meta-data");
            analyticsElement.SetAttribute("name", NAMESPACE_URI, a_tag);
            analyticsElement.SetAttribute("value", NAMESPACE_URI, a_value);
            a_node.AppendChild(analyticsElement);
        }

        private void SetApplicationTag(XmlNode a_node, string a_value = "true") {
            XmlAttribute valueAttr = a_node.Attributes["value", NAMESPACE_URI];
            if (valueAttr != null) {
                valueAttr.Value = a_value;
            }
        }
    } 
}
#endif