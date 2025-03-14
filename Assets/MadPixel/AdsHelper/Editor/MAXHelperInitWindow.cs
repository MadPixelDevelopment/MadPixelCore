using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace MAXHelper {
    public class MAXHelperInitWindow : EditorWindow {
        #region Fields
        private const string NEW_CONFIGS_PATH = "Assets/Resources/MAXCustomSettings.asset";
        private const string LP_MAXPACK_PACKAGE_PATH = "Assets/MadPixel/AdsHelper/Configs/MPC_LevelPlay_MaximunPack.unitypackage";

        private const string LevelPlay_FOLDER = "https://drive.google.com/drive/u/0/folders/1Lr8CUtKAu6DpOrcfJ6xQ9f7Viu67x8BN";
        private const string LevelPlay_MAXPACK_DRIVE = "https://drive.google.com/file/d/1MUzNhwh6LnT_fv6viwMbIzMzc42e7kfL/view?usp=sharing";

        private Vector2 scrollPosition;
        private static readonly Vector2 windowMinSize = new Vector2(450, 250);
        private static readonly Vector2 windowPrefSize = new Vector2(850, 400);

        private GUIStyle titleLabelStyle;
        private GUIStyle warningLabelStyle;
        private GUIStyle linkLabelStyle;
        private GUIStyle versionsLabelStyle;

        private static GUILayoutOption sdkKeyLabelFieldWidthOption = GUILayout.Width(120);
        private static GUILayoutOption sdkKeyTextFieldWidthOption = GUILayout.Width(650);
        private static GUILayoutOption buttonFieldWidth = GUILayout.Width(160);
        private static GUILayoutOption adUnitLabelWidthOption = GUILayout.Width(140);
        private static GUILayoutOption adUnitTextWidthOption = GUILayout.Width(150);
        private static GUILayoutOption adMobLabelFieldWidthOption = GUILayout.Width(100);
        private static GUILayoutOption adMobUnitTextWidthOption = GUILayout.Width(280);
        private static GUILayoutOption adUnitToggleOption = GUILayout.Width(180);
        private static GUILayoutOption bannerColorLabelOption = GUILayout.Width(250);

        private MAXCustomSettings CustomSettings;
        private bool bMaxVariantInstalled;
        private bool bUseAmazon;
        #endregion

        #region Menu Item
        [MenuItem("Mad Pixel/SDK Setup", priority = 0)]
        public static void ShowWindow() {
            var Window = EditorWindow.GetWindow<MAXHelperInitWindow>("Mad Pixel. SDK Setup", true);

            Window.Setup();
        }

        private void Setup() {
            minSize = windowMinSize;
            LoadConfigFromFile();

        }
        #endregion



        #region Editor Window Lifecyle Methods

        private void OnGUI() {
            if (CustomSettings != null) {
                using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition, false, false)) {
                    scrollPosition = scrollView.scrollPosition;

                    GUILayout.Space(5);

                    titleLabelStyle = new GUIStyle(EditorStyles.label) {
                        fontSize = 14,
                        fontStyle = FontStyle.Bold,
                        fixedHeight = 20
                    };

                    versionsLabelStyle = new GUIStyle(EditorStyles.label) {
                        fontSize = 12,
                    };
                    ColorUtility.TryParseHtmlString("#C4ECFD", out Color vColor);
                    versionsLabelStyle.normal.textColor = vColor;


                    if (linkLabelStyle == null) {
                        linkLabelStyle = new GUIStyle(EditorStyles.label) {
                            fontSize = 12,
                            wordWrap = false,
                        };
                    }
                    ColorUtility.TryParseHtmlString("#7FD6FD", out Color C);
                    linkLabelStyle.normal.textColor = C;

                    // Draw AppLovin MAX plugin details
                    EditorGUILayout.LabelField("1. Fill in your SDK Key", titleLabelStyle);

                    DrawSDKKeyPart();
                    DrawUnitIDsPart();

                    DrawInstallButtons();
                    DrawAnalyticsKeys();

                    DrawLinks();
                }
            }


            if (GUI.changed) {
                EditorUtility.SetDirty(CustomSettings);
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

            CustomSettings.levelPlayKey = DrawTextField("LevelPlay Key (Droid)", CustomSettings.levelPlayKey, adUnitTextWidthOption, adMobLabelFieldWidthOption);
            CustomSettings.levelPlayKey_ios = DrawTextField("LevelPlay Key (IOS)", CustomSettings.levelPlayKey_ios, adUnitTextWidthOption, adMobLabelFieldWidthOption);

            GUILayout.Space(5);
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            CustomSettings.bUseBanners = GUILayout.Toggle(CustomSettings.bUseBanners, "Use Banners", adUnitToggleOption);

            if (CustomSettings.bUseBanners) {

                GUILayout.BeginHorizontal();
                GUILayout.Space(10);
                GUILayout.Label("Banner Position", EditorStyles.boldLabel);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(10);
                CustomSettings.useTopBannerPosition = EditorGUILayout.Toggle("show banner on top?", CustomSettings.useTopBannerPosition);
                GUILayout.EndHorizontal();
            }
        }


        private void DrawUnitIDsPart() {
            GUILayout.Space(16);
            EditorGUILayout.LabelField("2. Fill in your Ad Unit IDs (from MadPixel managers)", titleLabelStyle);
            using (new EditorGUILayout.VerticalScope("box")) {
                if (CustomSettings == null) {
                    LoadConfigFromFile();
                }

                GUI.enabled = true;
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                GUILayout.Space(4);
                CustomSettings.bUseRewardeds = GUILayout.Toggle(CustomSettings.bUseRewardeds, "Use Rewarded Ads", adUnitToggleOption);
                GUI.enabled = CustomSettings.bUseRewardeds;
                CustomSettings.RewardedID = DrawTextField("Rewarded Ad Unit (Android)", CustomSettings.RewardedID, adUnitLabelWidthOption, adUnitTextWidthOption);
                CustomSettings.RewardedID_IOS = DrawTextField("Rewarded Ad Unit (IOS)", CustomSettings.RewardedID_IOS, adUnitLabelWidthOption, adUnitTextWidthOption);
                GUILayout.EndHorizontal();
                GUILayout.Space(4);

                GUI.enabled = true;
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                GUILayout.Space(4);
                CustomSettings.bUseInters = GUILayout.Toggle(CustomSettings.bUseInters, "Use Interstitials", adUnitToggleOption);
                GUI.enabled = CustomSettings.bUseInters;
                CustomSettings.InterstitialID = DrawTextField("Inerstitial Ad Unit (Android)", CustomSettings.InterstitialID, adUnitLabelWidthOption, adUnitTextWidthOption);
                CustomSettings.InterstitialID_IOS = DrawTextField("Interstitial Ad Unit (IOS)", CustomSettings.InterstitialID_IOS, adUnitLabelWidthOption, adUnitTextWidthOption);
                GUILayout.EndHorizontal();
                GUILayout.Space(4);

                GUI.enabled = true;
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                GUILayout.Space(4);
                CustomSettings.bUseBanners = GUILayout.Toggle(CustomSettings.bUseBanners, "Use Banners", adUnitToggleOption);
                GUI.enabled = CustomSettings.bUseBanners;
                CustomSettings.BannerID = DrawTextField("Banner Ad Unit (Android)", CustomSettings.BannerID, adUnitLabelWidthOption, adUnitTextWidthOption);
                CustomSettings.BannerID_IOS = DrawTextField("Banner Ad Unit (IOS)", CustomSettings.BannerID_IOS, adUnitLabelWidthOption, adUnitTextWidthOption);
                GUILayout.EndHorizontal();
                GUILayout.Space(4);

                GUI.enabled = true;
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                if (CustomSettings.bUseBanners) {
                    GUILayout.Space(24);

                    CustomSettings.BannerBackground = EditorGUILayout.ColorField("Banner Background Color: ", CustomSettings.BannerBackground, bannerColorLabelOption);

                    GUILayout.Space(4);

                }

                GUILayout.EndHorizontal();

                GUI.enabled = true;
            }
        }

        private void DrawInstallButtons() {
            GUILayout.Space(16);
            EditorGUILayout.LabelField("4. Install our full mediations", titleLabelStyle);
            using (new EditorGUILayout.VerticalScope("box")) {
                GUILayout.BeginHorizontal();
                GUILayout.Space(10);

                if (!MackPackUnitypackageExists()) {
                    EditorGUILayout.LabelField("You dont have MPC_LevelPlay_MaximunPack.unitypackage in your project. Probably your git added it to gitignore", sdkKeyTextFieldWidthOption);

                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(10);

                    if (GUILayout.Button(new GUIContent("Download latest Maximum mediations package"), adMobUnitTextWidthOption)) {
                        Application.OpenURL(LevelPlay_MAXPACK_DRIVE);
                    }

                    GUILayout.EndHorizontal();
                    GUILayout.Space(10);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                }

                GUI.enabled = MackPackUnitypackageExists();
                if (bMaxVariantInstalled) {
                    EditorGUILayout.LabelField("You have installed default Maximum pack of mediations", sdkKeyTextFieldWidthOption);
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                }
                if (GUILayout.Button(new GUIContent(bMaxVariantInstalled ? "Reimport maximum pack" : "Install maximum pack"), buttonFieldWidth)) {
                    AssetDatabase.ImportPackage(LP_MAXPACK_PACKAGE_PATH, true);
                    //CheckMaxVersion();
                }

                GUI.enabled = true;
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }
        }

        private void DrawAnalyticsKeys() {
            GUILayout.Space(16);
            EditorGUILayout.LabelField("5. Insert analytics info", titleLabelStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);

            CustomSettings.appmetricaKey = DrawTextField("AppmetricaKey",
                CustomSettings.appmetricaKey, adMobLabelFieldWidthOption, adMobUnitTextWidthOption);
            CustomSettings.appsFlyerID_ios = DrawTextField("IOS App ID",
                CustomSettings.appsFlyerID_ios, adMobLabelFieldWidthOption, adMobUnitTextWidthOption);

            GUILayout.Space(5);
            GUILayout.EndHorizontal();
        }

        private void DrawLinks() {
            GUILayout.Space(15);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("MPC_LevelPlay plugin and documentation:", GUILayout.Width(245));
            if (GUILayout.Button(new GUIContent("here"), GUILayout.Width(70))) {
                Application.OpenURL(LevelPlay_FOLDER);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("MPC_LevelPlay_edition v" + GetVersion("Assets/MadPixel/Version_levelPlay.md"), versionsLabelStyle, adUnitToggleOption);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Forked from MPC v." + GetVersion(), versionsLabelStyle, adUnitTextWidthOption);
            GUILayout.EndHorizontal();
        }

        private string DrawTextField(string fieldTitle, string text, GUILayoutOption labelWidth, GUILayoutOption textFieldWidthOption = null) {
            GUILayout.BeginHorizontal();
            GUILayout.Space(4);
            EditorGUILayout.LabelField(new GUIContent(fieldTitle), labelWidth);
            GUILayout.Space(4);
            text = (textFieldWidthOption == null) ? GUILayout.TextField(text) : GUILayout.TextField(text, textFieldWidthOption);
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
            GUILayout.Space(4);

            return text;
        }

        #endregion

        #region Helpers
        private void LoadConfigFromFile() {
            var Obj = AssetDatabase.LoadAssetAtPath(NEW_CONFIGS_PATH, typeof(MAXCustomSettings));
            if (Obj != null) {
                CustomSettings = (MAXCustomSettings)Obj;
            }
            else {
                Debug.Log("CustomSettings file doesn't exist, creating a new one...");
                var Instance = MAXCustomSettings.CreateInstance("MAXCustomSettings");
                AssetDatabase.CreateAsset(Instance, NEW_CONFIGS_PATH);
            }
        }

        public static string GetVersion(string a_path = "Assets/MadPixel/Version.md") {
            var versionText = File.ReadAllText(a_path);
            if (string.IsNullOrEmpty(versionText)) {
                return "--";
            }

            int subLength = versionText.IndexOf('-');
            versionText = versionText.Substring(10, subLength - 10);
            return versionText;
        }
        private bool MackPackUnitypackageExists() {
            return File.Exists(LP_MAXPACK_PACKAGE_PATH);
        }
        #endregion
    }
}
