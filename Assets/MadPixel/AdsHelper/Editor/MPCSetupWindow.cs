using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace MadPixel.Editor {
    public class MPCSetupWindow : EditorWindow {
        #region Fields
        private const string NEW_CONFIGS_PATH = "Assets/Resources/MadPixelCustomSettings.asset";
        private const string LP_MAXPACK_PACKAGE_PATH = "Assets/MadPixel/AdsHelper/Configs/MPC_LevelPlay_MaximunPack.unitypackage";

        private const string LP_FOLDER = "https://drive.google.com/drive/u/0/folders/1Lr8CUtKAu6DpOrcfJ6xQ9f7Viu67x8BN";
        private const string LP_MAXPACK_DRIVE = "https://drive.google.com/file/d/1MUzNhwh6LnT_fv6viwMbIzMzc42e7kfL/view?usp=sharing";

        private Vector2 m_scrollPosition;
        private static readonly Vector2 m_windowMinSize = new Vector2(450, 250);
        private static readonly Vector2 m_windowPrefSize = new Vector2(850, 400);

        private GUIStyle m_titleLabelStyle;
        private GUIStyle m_linkLabelStyle;
        private GUIStyle m_versionsLabelStyle;

        private static GUILayoutOption m_sdkKeyTextFieldWidthOption = GUILayout.Width(650);
        private static GUILayoutOption m_buttonFieldWidth = GUILayout.Width(160);
        private static GUILayoutOption m_adUnitLabelWidthOption = GUILayout.Width(140);
        private static GUILayoutOption m_adUnitTextWidthOption = GUILayout.Width(150);
        private static GUILayoutOption m_adMobLabelFieldWidthOption = GUILayout.Width(100);
        private static GUILayoutOption m_adMobUnitTextWidthOption = GUILayout.Width(280);
        private static GUILayoutOption m_adUnitToggleOption = GUILayout.Width(180);
        private static GUILayoutOption m_bannerColorLabelOption = GUILayout.Width(250);

        private MadPixelCustomSettings m_customSettings;
        private bool m_isMaxVariantInstalled;
        #endregion

        #region Menu Item
        [MenuItem("Mad Pixel/SDK Setup", priority = 0)]
        public static void ShowWindow() {
            var window = EditorWindow.GetWindow<MPCSetupWindow>("Mad Pixel. SDK Setup", true);

            window.Setup();
        }

        private void Setup() {
            minSize = m_windowMinSize;
            LoadConfigFromFile();

        }
        #endregion



        #region Editor Window Lifecyle Methods

        private void OnGUI() {
            if (m_customSettings != null) {
                using (var scrollView = new EditorGUILayout.ScrollViewScope(m_scrollPosition, false, false)) {
                    m_scrollPosition = scrollView.scrollPosition;

                    GUILayout.Space(5);

                    m_titleLabelStyle = new GUIStyle(EditorStyles.label) {
                        fontSize = 14,
                        fontStyle = FontStyle.Bold,
                        fixedHeight = 20
                    };

                    m_versionsLabelStyle = new GUIStyle(EditorStyles.label) {
                        fontSize = 12,
                    };
                    ColorUtility.TryParseHtmlString("#C4ECFD", out Color vColor);
                    m_versionsLabelStyle.normal.textColor = vColor;


                    if (m_linkLabelStyle == null) {
                        m_linkLabelStyle = new GUIStyle(EditorStyles.label) {
                            fontSize = 12,
                            wordWrap = false,
                        };
                    }
                    ColorUtility.TryParseHtmlString("#7FD6FD", out Color C);
                    m_linkLabelStyle.normal.textColor = C;

                    // Draw AppLovin MAX plugin details
                    EditorGUILayout.LabelField("1. Fill in your SDK Key", m_titleLabelStyle);

                    DrawSDKKeyPart();
                    DrawUnitIDsPart();

                    DrawAnalyticsKeys();

                    DrawLinks();
                }
            }


            if (GUI.changed) {
                EditorUtility.SetDirty(m_customSettings);
            }
        }

        private void OnDisable() {
            AssetDatabase.SaveAssets();
        }


        #endregion

        #region Draw Functions
        private void DrawSDKKeyPart() {
            GUI.enabled = true;
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);

            m_customSettings.levelPlayKey = DrawTextField("LevelPlay Key (Droid)", m_customSettings.levelPlayKey, m_adUnitTextWidthOption, m_adMobLabelFieldWidthOption);
            m_customSettings.levelPlayKey_ios = DrawTextField("LevelPlay Key (IOS)", m_customSettings.levelPlayKey_ios, m_adUnitTextWidthOption, m_adMobLabelFieldWidthOption);

            GUILayout.Space(5);
            GUILayout.EndHorizontal();
        }


        private void DrawUnitIDsPart() {
            GUILayout.Space(16);
            EditorGUILayout.LabelField("2. Fill in your Ad Unit IDs (from MadPixel managers)", m_titleLabelStyle);
            using (new EditorGUILayout.VerticalScope("box")) {
                if (m_customSettings == null) {
                    LoadConfigFromFile();
                }

                GUI.enabled = true;
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                GUILayout.Space(4);
                m_customSettings.bUseRewardeds = GUILayout.Toggle(m_customSettings.bUseRewardeds, "Use Rewarded Ads", m_adUnitToggleOption);
                GUI.enabled = m_customSettings.bUseRewardeds;
                m_customSettings.RewardedID = DrawTextField("Rewarded Ad Unit (Android)", m_customSettings.RewardedID, m_adUnitLabelWidthOption, m_adUnitTextWidthOption);
                m_customSettings.RewardedID_IOS = DrawTextField("Rewarded Ad Unit (IOS)", m_customSettings.RewardedID_IOS, m_adUnitLabelWidthOption, m_adUnitTextWidthOption);
                GUILayout.EndHorizontal();
                GUILayout.Space(4);

                GUI.enabled = true;
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                GUILayout.Space(4);
                m_customSettings.bUseInters = GUILayout.Toggle(m_customSettings.bUseInters, "Use Interstitials", m_adUnitToggleOption);
                GUI.enabled = m_customSettings.bUseInters;
                m_customSettings.InterstitialID = DrawTextField("Inerstitial Ad Unit (Android)", m_customSettings.InterstitialID, m_adUnitLabelWidthOption, m_adUnitTextWidthOption);
                m_customSettings.InterstitialID_IOS = DrawTextField("Interstitial Ad Unit (IOS)", m_customSettings.InterstitialID_IOS, m_adUnitLabelWidthOption, m_adUnitTextWidthOption);
                GUILayout.EndHorizontal();
                GUILayout.Space(4);

                GUI.enabled = true;
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                GUILayout.Space(4);
                m_customSettings.bUseBanners = GUILayout.Toggle(m_customSettings.bUseBanners, "Use Banners", m_adUnitToggleOption);
                GUI.enabled = m_customSettings.bUseBanners;
                m_customSettings.BannerID = DrawTextField("Banner Ad Unit (Android)", m_customSettings.BannerID, m_adUnitLabelWidthOption, m_adUnitTextWidthOption);
                m_customSettings.BannerID_IOS = DrawTextField("Banner Ad Unit (IOS)", m_customSettings.BannerID_IOS, m_adUnitLabelWidthOption, m_adUnitTextWidthOption);
                GUILayout.EndHorizontal();

                GUI.enabled = true;
                GUILayout.Space(4);
                if (m_customSettings.bUseBanners) {
                    //GUILayout.BeginHorizontal();
                    //GUILayout.Space(24);
                    //m_customSettings.BannerBackground = EditorGUILayout.ColorField("Banner Background Color: ", m_customSettings.BannerBackground, m_bannerColorLabelOption);
                    //GUILayout.Space(4);
                    //GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(24);
                    m_customSettings.useTopBannerPosition = EditorGUILayout.Toggle("Show banner at the top?", m_customSettings.useTopBannerPosition, m_bannerColorLabelOption);
                    GUILayout.EndHorizontal();
                }

                GUI.enabled = true;
            }
        }

        private void DrawAnalyticsKeys() {
            GUILayout.Space(16);
            EditorGUILayout.LabelField("5. Insert analytics info", m_titleLabelStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);

            m_customSettings.appmetricaKey = DrawTextField("AppmetricaKey",
                m_customSettings.appmetricaKey, m_adMobLabelFieldWidthOption, m_adMobUnitTextWidthOption);
            m_customSettings.appsFlyerID_ios = DrawTextField("IOS App ID",
                m_customSettings.appsFlyerID_ios, m_adMobLabelFieldWidthOption, m_adMobUnitTextWidthOption);

            GUILayout.Space(5);
            GUILayout.EndHorizontal();
        }

        private void DrawLinks() {
            GUILayout.Space(15);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("MPC_LevelPlay plugin and documentation:", GUILayout.Width(245));
            if (GUILayout.Button(new GUIContent("here"), GUILayout.Width(70))) {
                Application.OpenURL(LP_FOLDER);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("MPC_LevelPlay_edition v" + GetVersion(), m_versionsLabelStyle, m_adUnitToggleOption);
            GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal();
            //EditorGUILayout.LabelField("Forked from MPC v." + GetVersion(), m_versionsLabelStyle, m_adUnitTextWidthOption);
            //GUILayout.EndHorizontal();
        }

        private string DrawTextField(string a_fieldTitle, string a_text, GUILayoutOption a_labelWidth, GUILayoutOption a_textFieldWidthOption = null) {
            GUILayout.BeginHorizontal();
            GUILayout.Space(4);
            EditorGUILayout.LabelField(new GUIContent(a_fieldTitle), a_labelWidth);
            GUILayout.Space(4);
            a_text = (a_textFieldWidthOption == null) ? GUILayout.TextField(a_text) : GUILayout.TextField(a_text, a_textFieldWidthOption);
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
            GUILayout.Space(4);

            return a_text;
        }

        #endregion

        #region Helpers
        private void LoadConfigFromFile() {
            var Obj = AssetDatabase.LoadAssetAtPath(NEW_CONFIGS_PATH, typeof(MadPixelCustomSettings));
            if (Obj != null) {
                m_customSettings = (MadPixelCustomSettings)Obj;
            }
            else {
                Debug.Log("CustomSettings file doesn't exist, creating a new one...");
                var Instance = MadPixelCustomSettings.CreateInstance(AdsManager.SETTINGS_FILE_NAME);
                AssetDatabase.CreateAsset(Instance, NEW_CONFIGS_PATH);
            }
        }

        public static string GetVersion(string a_path = "Assets/MadPixel/Version_levelPlay.md") {
            var versionText = File.ReadAllText(a_path);
            if (string.IsNullOrEmpty(versionText)) {
                return "--";
            }

            int subLength = versionText.IndexOf('-');
            versionText = versionText.Substring(10, subLength - 10);
            return versionText;
        }
        
        private bool MaxPackUnitypackageExists() {
            return File.Exists(LP_MAXPACK_PACKAGE_PATH);
        }
        #endregion
    }
}
