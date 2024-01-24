using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

#if UNITY_EDITOR
namespace MAXHelper {
    public static class MAXHelperDefineSymbols {
        public static readonly string AMAZON_DEF = "MADPIXEL_AMAZON_DROID";

        public static void DefineSymbols(bool bActive = true) {
            string alreadyDefined = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Android);
            List<string> scriptingDefinesStringList = alreadyDefined.Split(';').ToList();
            if (bActive) {
                if (scriptingDefinesStringList.Contains(AMAZON_DEF)) {
                    return;
                }
                scriptingDefinesStringList.Add(AMAZON_DEF);
            }
            else {
                if (scriptingDefinesStringList.Contains(AMAZON_DEF)) {
                    scriptingDefinesStringList.Remove(AMAZON_DEF);
                }
                else {
                    return;
                }
            }

            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Android, string.Join(";", scriptingDefinesStringList.ToArray()));
        }

        public static bool HasAmazonActivated() {
            string alreadyDefined = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Android);
            List<string> scriptingDefinesStringList = alreadyDefined.Split(';').ToList();
            return scriptingDefinesStringList.Contains(AMAZON_DEF);
        }
    } 
}
#endif