using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MadPixelCore.Editor {
    public class MPCDeleteFoldersWindow : EditorWindow {
        private static GUILayoutOption m_widthOption = GUILayout.Width(280);
        private static bool m_hasAppmetrica = false;
        private static bool m_hasEDM = false;
        private GUIStyle m_boldText;
        private static MPCDeleteFoldersWindow m_instance;

        public static MPCDeleteFoldersWindow FindFirstInstance() {
            var windows = (MPCDeleteFoldersWindow[])Resources.FindObjectsOfTypeAll(typeof(MPCDeleteFoldersWindow));
            if (windows.Length == 0)
                return null;
            return windows[0];
        }

        private void Awake() {
            m_boldText = new GUIStyle() {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                fixedHeight = 20
            };
        }
        public static void ShowWindow(bool a_hasAppmetrica, bool a_hasEDM) {
            m_hasAppmetrica = a_hasAppmetrica;
            m_hasEDM = a_hasEDM;
            if (m_instance == null) {
                m_instance = FindFirstInstance();
                if (m_instance == null) {
                    m_instance = GetWindow<MPCDeleteFoldersWindow>("Attention!", true);
                    m_instance.minSize = new Vector2(200, 200);
                    m_instance.Show();
                }
            }

            m_instance.Focus();
        }

        private void OnGUI() {
            GUILayout.Space(20);

            GUILayout.Label("You have these assets as packages:", EditorStyles.boldLabel);
            GUILayout.Space(20);
            if (m_hasAppmetrica) {
                GUILayout.Label($"- Appmetrica");
            }

            if (m_hasEDM) {
                GUILayout.Label($"- External Dependency Manager");
            }

            GUILayout.Space(20);

            GUILayout.Label($"Duplicated packages can result in errors and crashes.");
            GUILayout.Label($"Do you want to delete old versions?");

            GUILayout.Space(20);

            if (GUILayout.Button(new GUIContent("Yes, delete old duplicated assets"), m_widthOption)) {
                MPCChecker.DeleteOldPackages(true);
                m_instance.Close();
            }
            GUILayout.Space(10);

            GUI.color = Color.red;
            if (GUILayout.Button(new GUIContent("No, I take the risks"), m_widthOption)) {
                MPCChecker.DeleteOldPackages(false);
                m_instance.Close();
            }

        }
    }

}