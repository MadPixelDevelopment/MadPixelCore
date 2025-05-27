using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.IO;
using MadPixel;

namespace MadPixel.Editor {
    public class MPCSetupWindow : EditorWindow {
        #region Fields
        private const string NEW_CONFIGS_PATH = "Assets/Resources/MadPixelCustomSettings.asset";
        private const string MAXPACK_PACKAGE_PATH = "Assets/MadPixel/MAXHelper/Configs/MaximumPack.unitypackage";
        private const string MEDIATIONS_PATH = "Assets/MAXSdk/Mediation/";

        private const string MPC_FOLDER = "https://github.com/MadPixelDevelopment/MadPixelCore/releases";
        private const string MAX_PACK_INDEPENDENT = "https://github.com/MadPixelDevelopment/MadPixelCore/raw/main/Assets/MadPixel/MAXHelper/Configs/MaximumPack.unitypackage";

        private const string ADS_DOC =
            "https://docs.google.com/document/d/1lx9wWCD4s8v4aXH1pb0oQENz01UszdalHtnznmQv2vc/edit#heading=h.y039lv8byi2i";

        private List<string> MAX_VARIANT_PACKAGES = new List<string>() { "ByteDance", "Fyber", "Google", "InMobi", "Mintegral", "MyTarget", "Vungle"};

        private Vector2 m_scrollPosition;
        private static readonly Vector2 m_windowMinSize = new Vector2(450, 250);
        private static readonly Vector2 m_windowPrefSize = new Vector2(850, 400);

        private GUIStyle m_titleLabelStyle;
        private GUIStyle m_warningLabelStyle; 
        private GUIStyle m_linkLabelStyle;
        private GUIStyle m_versionsLabelStyle;

        private static GUILayoutOption m_sdkKeyLabelFieldWidthOption = GUILayout.Width(120);
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
            AddImportCallbacks();
            CheckMaxVersion();

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

                    DrawTestPart();

                    DrawInstallButtons();

                    DrawAnalyticsKeys();

                    DrawLinks();
                }
            }


            if (GUI.changed) {
                AppLovinSettings.Instance.SaveAsync();
                EditorUtility.SetDirty(m_customSettings);
            }
        }

        private void OnDisable() {
            if (m_customSettings != null) {
                AppLovinSettings.Instance.SdkKey = MadPixelCustomSettings.APPLOVIN_SDK_KEY;
            }

            AssetDatabase.SaveAssets();
        }


        #endregion

        #region Draw Functions
        private void DrawSDKKeyPart() {
            GUI.enabled = true;

            using (new EditorGUILayout.VerticalScope("box")) {
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                GUILayout.Space(4);
                AppLovinSettings.Instance.QualityServiceEnabled = GUILayout.Toggle(AppLovinSettings.Instance.QualityServiceEnabled, "  Enable MAX Ad Review (turn this on for production build)");
                GUILayout.EndHorizontal();
                GUILayout.Space(4);
            }
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
                GUILayout.Space(4);

                GUI.enabled = true;
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                if (m_customSettings.bUseBanners) {
                    GUILayout.Space(24);

                    m_customSettings.BannerBackground = EditorGUILayout.ColorField("Banner Background Color: ", m_customSettings.BannerBackground, m_bannerColorLabelOption);

                    GUILayout.Space(4);

                }

                GUILayout.EndHorizontal();

                GUI.enabled = true;
            }
        }

        private void DrawTestPart() {
            GUILayout.Space(16);
            EditorGUILayout.LabelField("3. For testing mediations: enable Mediation Debugger", m_titleLabelStyle);

            using (new EditorGUILayout.VerticalScope("box")) {
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                GUILayout.Space(4);

                if (m_warningLabelStyle == null) {
                    m_warningLabelStyle = new GUIStyle(EditorStyles.label) {
                        fontSize = 13,
                        fontStyle = FontStyle.Bold,
                        fixedHeight = 20
                    };
                }

                ColorUtility.TryParseHtmlString("#D22F2F", out Color C);
                m_warningLabelStyle.normal.textColor = C;

                if (m_customSettings.bShowMediationDebugger) {
                    EditorGUILayout.LabelField("For Test builds only. Do NOT enable this option in the production build!", m_warningLabelStyle);
                }

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(4);
                m_customSettings.bShowMediationDebugger = GUILayout.Toggle(m_customSettings.bShowMediationDebugger, "Show Mediation Debugger", m_adUnitToggleOption);
                GUILayout.EndHorizontal();
            }
        }
        

        private void DrawInstallButtons() {
            GUILayout.Space(16);
            EditorGUILayout.LabelField("4. Install our full mediations", m_titleLabelStyle);
            using (new EditorGUILayout.VerticalScope("box")) {
                GUILayout.BeginHorizontal();
                GUILayout.Space(10);

                if (!MackPackUnitypackageExists()) {
                    EditorGUILayout.LabelField("You dont have MaximunPack.unitypackage in your project. Probably your git added it to gitignore", m_sdkKeyTextFieldWidthOption);
                    
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(10);

                    if (GUILayout.Button(new GUIContent("Download latest Maximum mediations package"), m_adMobUnitTextWidthOption)) {
                        Application.OpenURL(MAX_PACK_INDEPENDENT);
                    }

                    GUILayout.EndHorizontal();
                    GUILayout.Space(10);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                }

                GUI.enabled = MackPackUnitypackageExists();
                if (m_isMaxVariantInstalled) {
                    EditorGUILayout.LabelField("You have installed default Maximum pack of mediations", m_sdkKeyTextFieldWidthOption);
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                }
                if (GUILayout.Button(new GUIContent(m_isMaxVariantInstalled ? "Reimport maximum pack" : "Install maximum pack"), m_buttonFieldWidth)) {
                    AssetDatabase.ImportPackage(MAXPACK_PACKAGE_PATH, true);
                    CheckMaxVersion();
                }

                GUI.enabled = true;
                GUILayout.EndHorizontal();

                //if (bMaxVariantInstalled) {

                    GUILayout.Space(10);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(10);

                    AppLovinSettings.Instance.AdMobAndroidAppId = DrawTextField("AndroidAdMobID",
                        AppLovinSettings.Instance.AdMobAndroidAppId, m_adMobLabelFieldWidthOption, m_adMobUnitTextWidthOption);
                    AppLovinSettings.Instance.AdMobIosAppId = DrawTextField("IOSAdMobID",
                        AppLovinSettings.Instance.AdMobIosAppId, m_adMobLabelFieldWidthOption, m_adMobUnitTextWidthOption);

                    GUILayout.Space(5);
                    GUILayout.EndHorizontal();
                //}
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
            EditorGUILayout.LabelField("Read MPC Documentation", GUILayout.Width(150));
            if (GUILayout.Button(new GUIContent("here"), GUILayout.Width(50))) {
                Application.OpenURL(ADS_DOC);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Download latest MadPixelCore plugin", GUILayout.Width(215));
            if (GUILayout.Button(new GUIContent("from here"), GUILayout.Width(70))) {
                Application.OpenURL(MPC_FOLDER);
            }
            GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal();
            //EditorGUILayout.LabelField("Ads Manager v." + AdsManager.Version, versionsLabelStyle, sdkKeyLabelFieldWidthOption);
            //GUILayout.EndHorizontal();



            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("MPC v." + GetVersion(), m_versionsLabelStyle, m_sdkKeyLabelFieldWidthOption);
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
            var obj = AssetDatabase.LoadAssetAtPath(NEW_CONFIGS_PATH, typeof(MadPixelCustomSettings));
            if (obj != null) {
                m_customSettings = (MadPixelCustomSettings)obj;
            } else {
                Debug.Log("CustomSettings file doesn't exist, creating a new one...");
                var instance = MadPixelCustomSettings.CreateInstance(AdsManager.SETTINGS_FILE_NAME);
                AssetDatabase.CreateAsset(instance, NEW_CONFIGS_PATH);
            }
        }

        private void CheckMaxVersion() {
            string[] filesPaths = System.IO.Directory.GetFiles(MEDIATIONS_PATH);
            if (filesPaths != null && filesPaths.Length > 0) {
                List<string> Paths = filesPaths.ToList();
                bool bMissingPackage = false;
                foreach (string PackageName in MAX_VARIANT_PACKAGES) {
                    if (!filesPaths.Contains(MEDIATIONS_PATH + PackageName + ".meta")) {
                        bMissingPackage = true;
                        break;
                    }
                }

                m_isMaxVariantInstalled = !bMissingPackage;
            }
        }

        public static string GetVersion() {
            var versionText = File.ReadAllText("Assets/MadPixel/Version.md");
            if (string.IsNullOrEmpty(versionText)) {
                return "--";
            }

            int subLength = versionText.IndexOf('-');
            versionText = versionText.Substring(10, subLength - 10);
            return versionText;
        }

        private void AddImportCallbacks() {
            AssetDatabase.importPackageCompleted += packageName => {
                Debug.Log($"Package {packageName} installed");
                CheckMaxVersion();
            };

            AssetDatabase.importPackageCancelled += packageName => {
                Debug.Log($"Package {packageName} cancelled");
            };

            AssetDatabase.importPackageFailed += (packageName, errorMessage) => {
                Debug.Log($"Package {packageName} failed");
            };
        }

        private bool MackPackUnitypackageExists() {
            return File.Exists(MAXPACK_PACKAGE_PATH);
        }

        #endregion
    }
}
