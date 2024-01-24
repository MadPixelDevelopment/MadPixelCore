using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

namespace MadPixel.InApps {
    [RequireComponent(typeof(GamingServices))]
    public class MobileInAppPurchaser : MonoBehaviour, IStoreListener {
        #region Fields
        public const string VERSION = "1.0.3";

        private IStoreController StoreController;
        private IExtensionProvider StoreExstensionProvider;

        [SerializeField] private bool bInitOnStart;
        [SerializeField] private bool bDebugLogging;
        [SerializeField] private string adsFreeSKU;
        [SerializeField] private List<string> productsSKUs; 
        [SerializeField] private GamingServices GamingServices;

        public static string AdsFreeSKU {
            get {
                if (Exist) {
                    return Instance.adsFreeSKU;
                }

                return null;
            }
        }
        public static List<string> ConsumablesList {
            get {
                if (Exist) {
                    return Instance.productsSKUs;
                }

                return null;
            }
        }

        #endregion
        
        #region Events
        public UnityAction<Product> OnPurchaseResult;
        #endregion


        #region Static

        protected static MobileInAppPurchaser _instance;

        public static bool Exist {
            get { return (_instance != null); }
        }

        public static MobileInAppPurchaser Instance {
            get {
                if (_instance == null) {
                    Debug.LogError("MobileInAppPurchaser wasn't created yet!");

                    GameObject go = new GameObject();
                    go.name = "MobileInAppPurchaser";
                    _instance = go.AddComponent(typeof(MobileInAppPurchaser)) as MobileInAppPurchaser;
                }

                return _instance;
            }
        }

        public static void Destroy(bool immediate = false) {
            if (_instance != null && _instance.gameObject != null) {
                if (immediate) {
                    DestroyImmediate(_instance.gameObject);
                } else {
                    GameObject.Destroy(_instance.gameObject);
                }
            }

            _instance = null;
        }

        #endregion


        #region Initialization
        private void Awake() {
            if (_instance == null) {
                _instance = this;
                GameObject.DontDestroyOnLoad(this.gameObject);
                if (GamingServices == null) {
                    GamingServices = GetComponent<GamingServices>();
                }
                GamingServices.Initialize();
            } else {
                GameObject.Destroy(gameObject);
                Debug.LogError($"Already have Analytics on scene!");
            }
        }


        private void Start() {
            if (bInitOnStart) {
                if (!string.IsNullOrEmpty(AdsFreeSKU) || (ConsumablesList != null && ConsumablesList.Count > 0)) {
                    Init(AdsFreeSKU, ConsumablesList);
                }
                else {
                    Debug.LogWarning("Cant init InApps with null Data!");
                }
            }
        }




        /// <summary>
        /// Initializes Consumables and Non-Consumables
        /// </summary>
        /// <param name="ConsumableSKUs">all consumable SKUs from your configs, that you can purchase multiple times (like coins, gems)</param>
        /// <param name="AdsFreeSKU">non consumable SKU for a one-time AdsFree purchase</param>
        public void Init(string AdsFreeSKU, List<string> ConsumableSKUs = null) {
            StartCoroutine(InitCoroutine(AdsFreeSKU, ConsumableSKUs));
        }

        private IEnumerator InitCoroutine(string AdsFreeSKU, List<string> ConsumableSKUs = null) {
            ConfigurationBuilder Builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            if (!string.IsNullOrEmpty(AdsFreeSKU)) {
                Builder.AddProduct(AdsFreeSKU, ProductType.NonConsumable);
            }

            if (ConsumableSKUs != null && ConsumableSKUs.Count > 0) {
                foreach (string SKU in ConsumableSKUs) {
                    Builder.AddProduct(SKU, ProductType.Consumable);
                }
            }

            yield return new WaitWhile(() => GamingServices.IsLoading);

            UnityPurchasing.Initialize(this, Builder);
        }
        #endregion

        #region Public
        public bool IsInitialized() {
            return (StoreController != null && StoreExstensionProvider != null);
        }


        /// <summary>
        /// Use <c>GetProduct</c> to return a Product.
        /// <example>
        /// For example:
        /// <code>
        /// Product p = GetProduct("com.company.game.gold_1");
        /// UIPriceTag.text = p.metadata.localizedPrice;
        /// </code>
        /// </example>
        /// </summary>
        public Product GetProduct(string SKU) {
            if (!string.IsNullOrEmpty(SKU)) {
                if (IsInitialized()) {
                    return StoreController.products.WithID(SKU);
                }
            }
            return null;
        }

        /// <summary>
        /// Call this method to buy
        /// </summary>
        public static bool BuyProduct(string SKU) {
            if (Exist) {
                return Instance.BuyProductInner(SKU);
            }

            return false;
        }
        
        public bool BuyProductInner(string SKU) {
            if (IsInitialized()) {
                Product Product = StoreController.products.WithID(SKU);
                if (Product != null && Product.availableToPurchase) {
                    if (bDebugLogging){
                        Debug.LogWarning($"[MobileInAppPurchaser] Purchasing product asynchronously: {Product.definition.id}");
                    }

                    StoreController.InitiatePurchase(Product);
                    return true;
                } else {
                    Debug.LogError("[MobileInAppPurchaser] BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                }
            } else {
                Debug.LogError("[MobileInAppPurchaser] BuyProductID FAIL. Not initialized.");
            }
            return false;
        }

        // TODO: https://docs.unity3d.com/Manual/UnityIAPRestoringTransactions.html  (FOR Non-Consumable or renewable Subscription)
        // Restore purchases previously made by this customer. Some platforms automatically restore purchases, like Google. 
        // Apple currently requires explicit purchase restoration for IAP, conditionally displaying a password prompt.
        public void RestorePurchases() {
            if (!IsInitialized()) {
                Debug.LogWarning("[MobileInAppPurchaser] RestorePurchases FAIL. Not initialized.");
                return;
            }

            if (Application.platform == RuntimePlatform.Android) {
                if (bDebugLogging) {
                    Debug.LogWarning("[MobileInAppPurchaser] RestorePurchases started ...");
                }

                var google = StoreExstensionProvider.GetExtension<IGooglePlayStoreExtensions>();
                // Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
                // the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
                google.RestoreTransactions(OnTransactionsRestored);
            }
            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.OSXPlayer) {
                if (bDebugLogging) {
                    Debug.LogWarning("[MobileInAppPurchaser] RestorePurchases started ...");
                }

                var apple = StoreExstensionProvider.GetExtension<IAppleExtensions>();
                // Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
                // the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
                apple.RestoreTransactions(OnTransactionsRestored);
            }
            //else {
            //    // We are not running on an Apple device. No work is necessary to restore purchases.
            //    Logger.Log(string.Format("[MobileInAppPurchaser] RestorePurchases FAIL. Not supported " + 
            //        "on this platform. Current = {0}", Application.platform));
            //}
        }
        #endregion

        #region Helpers
        private void OnTransactionsRestored(bool result) {
            if (result) {
                // This does not mean anything was restored,
                // merely that the restoration process succeeded.

                // The first phase of restoration. If no more responses are received on ProcessPurchase then 
                // no purchases are available to be restored.
                if (bDebugLogging) {
                    Debug.LogWarning($"[MobileInAppPurchaser] RestorePurchases continuing: {result}. If no further messages, no purchases available to restore.");
                }
            }
            else {
                Debug.LogError(string.Format("[MobileInAppPurchaser] RestorePurchases Restoration failed."));
            }
        }
        #endregion


        #region IStoreListener
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions) {
            StoreController = controller;
            StoreExstensionProvider = extensions;
        }

        public void OnInitializeFailed(InitializationFailureReason error) {
            Debug.LogWarning($"[MobileInAppPurchaser] INIT FAILED! Reason: {error}");
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message) {
            Debug.LogWarning($"[MobileInAppPurchaser] INIT FAILED! Reason: {error}");
        }

        public void OnPurchaseFailed(Product Prod, PurchaseFailureReason FailureReason) {
            string ProductSKU = Prod.definition.id;

            if (bDebugLogging) {
                Debug.LogWarning($"[MobileInAppPurchaser] OnPurchaseFailed: FAIL. Product: '{ProductSKU}' Receipt: {Prod.receipt}");
                Debug.LogWarning($"PurchaseFailureReason: {FailureReason}");
            }

            OnPurchaseResult?.Invoke(null);
        }




        // Proccess purchase and use Unity validation to check it for freud
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs E) {
            if (bDebugLogging) {
                Debug.LogWarning($"[MobileInAppPurchaser] ProcessPurchase: PASS. Product: '{E.purchasedProduct.definition.id}' Receipt: {E.purchasedProduct.receipt}");
            }

            // NOTE: UNITY VALIDATION

            List<string> ReceitsIDs = new List<string>();
            bool validPurchase = true; // Presume valid for platforms with no R.V.
                                       // Unity IAP's validation logic is only included on these platforms.
#if !UNITY_EDITOR
            // Prepare the validator with the secrets we prepared in the Editor
            // obfuscation window.


            var validator = new CrossPlatformValidator(GooglePlayTangle.Data(),
                AppleTangle.Data(), Application.identifier);

            try {
                // On Google Play, result has a single product ID.
                // On Apple stores, receipts contain multiple products.
                var result = validator.Validate(E.purchasedProduct.receipt);
                // For informational purposes, we list the receipt(s)
                Debug.Log("Valid!");
                foreach (IPurchaseReceipt productReceipt in result) {
                    ReceitsIDs.Add(productReceipt.productID);
                }
            } catch (IAPSecurityException) {
                Debug.Log("Invalid receipt, not unlocking content");
                validPurchase = false;
            }

#endif

            // It is important you check not just that the receipt is valid, but also what information it contains.
            // A common technique by users attempting to access content without purchase is to supply receipts from other products or applications.
            // These receipts are genuine and do pass validation, so you should make decisions based on the product IDs parsed by the CrossPlatformValidator.

            bool bValidID = false;
            if (ReceitsIDs != null && ReceitsIDs.Count > 0) {
                foreach (string ProductID in ReceitsIDs) {
                    //Debug.Log($"{E.purchasedProduct.definition.storeSpecificId}, and {ProductID}");
                    if (E.purchasedProduct.definition.storeSpecificId.Equals(ProductID)) {
                        bValidID = true;
                        break;
                    }
                }
            }

#if UNITY_EDITOR
            validPurchase = bValidID = true;
#endif

            // Process a successful purchase after validation
            if (validPurchase && bValidID) {
                if (IsInitialized()) {
                    Product Prod = E.purchasedProduct;
                    OnPurchaseResult?.Invoke(Prod);

                    MadPixelAnalytics.AnalyticsManager.PaymentSucceed(Prod);
                    MAXHelper.AdsManager.AddPurchaseKeyword();
                }
            } else {
                OnPurchaseResult?.Invoke(null);
            }
            return PurchaseProcessingResult.Complete;
        }
        #endregion
    }
}