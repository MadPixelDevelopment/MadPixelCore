using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
using System.IO;

namespace MadPixel.Editor {
    
    public class MPCPostBuild  {
#if UNITY_IOS        
        // Set the IDFA request description:
        const string k_TrackingDescription = "Your data will be used to provide you a better and personalized ad experience.";
 
        [PostProcessBuild(0)]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string pathToXcode) {
            if (buildTarget == BuildTarget.iOS) {
                AddPListValues(pathToXcode);
            }
        }
 
        // Implement a function to read and write values to the plist file:
        static void AddPListValues(string pathToXcode) {

            // Retrieve the plist file from the Xcode project directory:
            string plistPath = pathToXcode + "/Info.plist";
            PlistDocument plistObj = new PlistDocument();
 
 
            // Read the values from the plist file:
            plistObj.ReadFromString(File.ReadAllText(plistPath));
 
            // Set values from the root object:
            PlistElementDict plistRoot = plistObj.root;
 
            // Set the description key-value in the plist:
            plistRoot.SetString("NSUserTrackingUsageDescription", k_TrackingDescription);

            // Set Google Analytics flags
            SetDefaultGoogleKey(plistRoot, "GOOGLE_ANALYTICS_DEFAULT_ALLOW_ANALYTICS_STORAGE", true);
            SetDefaultGoogleKey(plistRoot, "GOOGLE_ANALYTICS_DEFAULT_ALLOW_AD_STORAGE", false);
            SetDefaultGoogleKey(plistRoot, "GOOGLE_ANALYTICS_DEFAULT_ALLOW_AD_USER_DATA", false);
            SetDefaultGoogleKey(plistRoot, "GOOGLE_ANALYTICS_DEFAULT_ALLOW_AD_PERSONALIZATION_SIGNALS", false);

            // Save changes to the plist:
            File.WriteAllText(plistPath, plistObj.WriteToString());

        }

        private static void SetDefaultGoogleKey(PlistElementDict a_plistRoot, string a_key, bool a_value) {
            a_plistRoot.SetBoolean(a_key, a_value);
        }
#endif
    }
}


