using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace MAXHelper {
    public class MAXHelperInitWindow : EditorWindow {
        #region Fields
        private const string CONFIGS_PATH = "Assets/MadPixel/MAXHelper/Configs/MAXCustomSettings.asset";
        private const string PACKAGE_PATH = "Assets/MadPixel/MAXHelper/Configs/MaximumPack.unitypackage";
        private const string AMAZON_PACKAGE_PATH = "Assets/MadPixel/MAXHelper/Configs/Amazon_APS.unitypackage";
        private const string MEDIATIONS_PATH = "Assets/MAXSdk/Mediation/";
        private const string AMAZON_PATH = "Assets/Amazon/Plugins";
        private const string MPC_FOLDER = "https://github.com/MadPixelDevelopment/MadPixelCore/releases";

        private const string ADS_DOC =
            "https://docs.google.com/document/d/1lx9wWCD4s8v4aXH1pb0oQENz01UszdalHtnznmQv2vc/edit#heading=h.y039lv8byi2i";

        private List<string> MAX_VARIANT_PACKAGES = new List<string>() { "ByteDance", "Fyber", "Google", "InMobi", "Mintegral", "MyTarget", "Vungle"};

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
        [MenuItem("Mad Pixel/Setup Ads", priority = 0)]
        public static void ShowWindow() {
            var Window = EditorWindow.GetWindow<MAXHelperInitWindow>("Mad Pixel. Setup Ads", true);

            Window.Setup();
        }

        private void Setup() {
            minSize = windowMinSize;
            LoadConfigFromFile();
            AddImportCallbacks();
            CheckMaxVersion();

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

                    DrawTestPart();

                    DrawInstallButtons();

                    DrawAmazon();

                    DrawLinks();
                }
            }


            if (GUI.changed) {
                AppLovinSettings.Instance.SaveAsync();
                EditorUtility.SetDirty(CustomSettings);
            }
        }

        private void OnDisable() {
            if (CustomSettings != null) {
                AppLovinSettings.Instance.SdkKey = CustomSettings.SDKKey;
            }

            AssetDatabase.SaveAssets();
        }


        #endregion

        #region Draw Functions
        private void DrawSDKKeyPart() {
            GUI.enabled = true;
            CustomSettings.SDKKey = DrawTextField("AppLovin SDK Key", CustomSettings.SDKKey, sdkKeyLabelFieldWidthOption, sdkKeyTextFieldWidthOption);

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

        private void DrawTestPart() {
            GUILayout.Space(16);
            EditorGUILayout.LabelField("3. For testing mediations: enable Mediation Debugger", titleLabelStyle);

            using (new EditorGUILayout.VerticalScope("box")) {
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                GUILayout.Space(4);

                if (warningLabelStyle == null) {
                    warningLabelStyle = new GUIStyle(EditorStyles.label) {
                        fontSize = 13,
                        fontStyle = FontStyle.Bold,
                        fixedHeight = 20
                    };
                }

                ColorUtility.TryParseHtmlString("#D22F2F", out Color C);
                warningLabelStyle.normal.textColor = C;

                if (CustomSettings.bShowMediationDebugger) {
                    EditorGUILayout.LabelField("For Test builds only. Do NOT enable this option in the production build!", warningLabelStyle);
                }

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(4);
                CustomSettings.bShowMediationDebugger = GUILayout.Toggle(CustomSettings.bShowMediationDebugger, "Show Mediation Debugger", adUnitToggleOption);
                GUILayout.EndHorizontal();
            }
        }
        

        private void DrawInstallButtons() {
            GUILayout.Space(16);
            EditorGUILayout.LabelField("4. Install our full mediations", titleLabelStyle);
            using (new EditorGUILayout.VerticalScope("box")) {
                GUILayout.BeginHorizontal();
                GUILayout.Space(10);
                GUI.enabled = false;
                if (GUILayout.Button(new GUIContent("Minimum pack is installed"), buttonFieldWidth)) {
                    // nothing here
                }

                GUI.enabled = true;

                GUILayout.Space(5);
                GUILayout.Space(10);
                GUI.enabled = !bMaxVariantInstalled;
                if (GUILayout.Button(new GUIContent(bMaxVariantInstalled ? "Maximum pack is installed" : "Install maximum pack"), buttonFieldWidth)) {
                    AssetDatabase.ImportPackage(PACKAGE_PATH, true);
                    CheckMaxVersion();
                }

                GUI.enabled = true;
                GUILayout.EndHorizontal();

                //if (bMaxVariantInstalled) {

                    GUILayout.Space(10);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(10);

                    AppLovinSettings.Instance.AdMobAndroidAppId = DrawTextField("AndroidAdMobID",
                        AppLovinSettings.Instance.AdMobAndroidAppId, adMobLabelFieldWidthOption, adMobUnitTextWidthOption);
                    AppLovinSettings.Instance.AdMobIosAppId = DrawTextField("IOSAdMobID",
                        AppLovinSettings.Instance.AdMobIosAppId, adMobLabelFieldWidthOption, adMobUnitTextWidthOption);

                    GUILayout.Space(5);
                    GUILayout.EndHorizontal();
                //}
            }
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

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Ads Manager v." + AdsManager.Version, versionsLabelStyle, sdkKeyLabelFieldWidthOption);
            GUILayout.EndHorizontal();



            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("MPC v." + GetVersion(), versionsLabelStyle, sdkKeyLabelFieldWidthOption);
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

        private void DrawAmazon() {
            GUILayout.Space(16);
            EditorGUILayout.LabelField("5. Amazon", titleLabelStyle);
            using (new EditorGUILayout.VerticalScope("box")) {
                GUILayout.BeginHorizontal();
                GUILayout.Space(4);
                bool bHasFolder = Directory.Exists(AMAZON_PATH);

                if (bHasFolder) {
                    EditorGUILayout.LabelField("Amazon plugin is installed", adMobUnitTextWidthOption);
                    bUseAmazon = MAXHelperDefineSymbols.HasAmazonActivated();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(4);
                    EditorGUILayout.LabelField(
                        bUseAmazon ? "Amazon is used by AdsManager" : "You dont have Amazon initialized",
                        adMobUnitTextWidthOption);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(4);
                    if (GUILayout.Button(new GUIContent(bUseAmazon ? "Deactivate Amazon" : "Activate Amazon"),
                            adMobUnitTextWidthOption)) {
                        OnActivateAmazonClick();
                    }

                    if (bUseAmazon) {
                        DrawAmazonValues();
                    }
                }
                else {
                    EditorGUILayout.LabelField("You dont have Amazon Plugin installed", adMobUnitTextWidthOption);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(4);
                    if (GUILayout.Button(new GUIContent("Install Amazon plugin"), adMobUnitTextWidthOption)) {
                        OnInstallAmazonPluginClick();
                    }

                    if (bUseAmazon) {
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Space(4);
                        if (GUILayout.Button(new GUIContent(bUseAmazon ? "Deactivate Amazon" : "Activate Amazon"),
                                adMobUnitTextWidthOption)) {
                            OnActivateAmazonClick();
                        }
                    }
                }

                GUILayout.EndHorizontal();
            }
        }

        private void DrawAmazonValues() {
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(4);
            CustomSettings.AmazonSDKKey = DrawTextField("Amazon SDK key (Android)", CustomSettings.AmazonSDKKey, buttonFieldWidth, adMobUnitTextWidthOption);
            CustomSettings.AmazonSDKKey_IOS = DrawTextField("Amazon SDK key (iOS)", CustomSettings.AmazonSDKKey_IOS, buttonFieldWidth, adMobUnitTextWidthOption);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(4);
            CustomSettings.AmazonRewardedID = DrawTextField("Rewarded Ad Unit (Android)", CustomSettings.AmazonRewardedID, buttonFieldWidth, adMobUnitTextWidthOption);
            CustomSettings.AmazonRewardedID_IOS = DrawTextField("Rewarded Ad Unit (iOS)", CustomSettings.AmazonRewardedID_IOS, buttonFieldWidth, adMobUnitTextWidthOption);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(4);
            CustomSettings.AmazonInterstitialID = DrawTextField("Interstitial Ad Unit (Android)", CustomSettings.AmazonInterstitialID, buttonFieldWidth, adMobUnitTextWidthOption);
            CustomSettings.AmazonInterstitialID_IOS = DrawTextField("Interstitial Ad Unit (iOS)", CustomSettings.AmazonInterstitialID_IOS, buttonFieldWidth, adMobUnitTextWidthOption);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(4);
            CustomSettings.AmazonBannerID = DrawTextField("Banner Ad Unit (Android)", CustomSettings.AmazonBannerID, buttonFieldWidth, adMobUnitTextWidthOption);
            CustomSettings.AmazonBannerID_IOS = DrawTextField("Banner Ad Unit (iOS)", CustomSettings.AmazonBannerID_IOS, buttonFieldWidth, adMobUnitTextWidthOption);
        }

        #endregion

        #region Helpers
        private void LoadConfigFromFile() {
            var Obj = AssetDatabase.LoadAssetAtPath(CONFIGS_PATH, typeof(MAXCustomSettings));
            if (Obj != null) {
                CustomSettings = (MAXCustomSettings)Obj;
            } else {
                Debug.Log("CustomSettings file doesn't exist, creating a new one...");
                var Instance = MAXCustomSettings.CreateInstance("MAXCustomSettings");
                AssetDatabase.CreateAsset(Instance, CONFIGS_PATH);
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

                bMaxVariantInstalled = !bMissingPackage;
            }
        }

        public static string GetVersion() {
            var versionText = File.ReadAllText("Assets/MadPixel/Version.md");
            if (string.IsNullOrEmpty(versionText)) {
                return "--";
            }

            versionText = versionText.Substring(10, 4);
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

        private void OnInstallAmazonPluginClick() {
            AssetDatabase.ImportPackage(AMAZON_PACKAGE_PATH, true);
        }
        
        private void OnActivateAmazonClick() {
            bUseAmazon = !bUseAmazon;
            if (bUseAmazon) {
                MAXHelperDefineSymbols.DefineSymbols();
            }
            else {
                MAXHelperDefineSymbols.DefineSymbols(false);
            }
        }

        #endregion
    }
}
