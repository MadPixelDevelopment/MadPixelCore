using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Security;
using UnityEngine.Serialization;

namespace MadPixel.InApps {
    [RequireComponent(typeof(GamingServices))]
    public class MobileInAppPurchaser : MonoBehaviour, IDetailedStoreListener {
        #region Fields
        public const string VERSION = "1.0.7";

        private IStoreController m_storeController;
        private IExtensionProvider m_storeExstensionProvider;
        [FormerlySerializedAs("bInitOnStart")]
        [SerializeField] private bool m_initOnStart;

        [SerializeField] private bool m_debugLogsOn;

        [FormerlySerializedAs("adsFreeSKU")]
        [SerializeField] private string m_adsFreeSKU;

        [FormerlySerializedAs("otherNonConsumablesSKUs")]
        [SerializeField] private List<string> m_otherNonConsumablesSKUs;

        [FormerlySerializedAs("productsSKUs")]
        [SerializeField] private List<string> consumablesSKUs;

        [FormerlySerializedAs("subscriptionsSKUs")]
        [SerializeField] private List<string> m_subscriptionsSKUs;

        [FormerlySerializedAs("GamingServices")]
        [SerializeField] private GamingServices m_gamingServices;

        public static string AdsFreeSKU {
            get {
                if (Exist) {
                    return Instance.m_adsFreeSKU;
                }

                return null;
            }
        }
        public static List<string> ConsumablesList {
            get {
                if (Exist) {
                    return Instance.consumablesSKUs;
                }

                return null;
            }
        }

        public static List<string> NonConsumablesList {
            get {
                if (Exist) {
                    return Instance.m_otherNonConsumablesSKUs;
                }

                return null;
            }
        }

        public static List<string> SubscriptionsList {
            get {
                if (Exist) {
                    return Instance.m_subscriptionsSKUs;
                }

                return null;
            }
        }

        #endregion
        
        #region Events
        public UnityAction<Product> OnPurchaseResult;
        #endregion


        #region Static

        protected static MobileInAppPurchaser m_instance;

        public static bool Exist {
            get { return (m_instance != null); }
        }

        public static MobileInAppPurchaser Instance {
            get {
                if (m_instance == null) {
                    Debug.LogError("MobileInAppPurchaser wasn't created yet!");

                    GameObject go = new GameObject();
                    go.name = "MobileInAppPurchaser";
                    m_instance = go.AddComponent(typeof(MobileInAppPurchaser)) as MobileInAppPurchaser;
                }

                return m_instance;
            }
        }

        public static void Destroy(bool immediate = false) {
            if (m_instance != null && m_instance.gameObject != null) {
                if (immediate) {
                    DestroyImmediate(m_instance.gameObject);
                } else {
                    GameObject.Destroy(m_instance.gameObject);
                }
            }

            m_instance = null;
        }

        #endregion


        #region Initialization
        private void Awake() {
            if (m_instance == null) {
                m_instance = this;
                GameObject.DontDestroyOnLoad(this.gameObject);
                if (m_gamingServices == null) {
                    m_gamingServices = GetComponent<GamingServices>();
                }
                m_gamingServices.Initialize();
            } else {
                GameObject.Destroy(gameObject);
                Debug.LogError($"Already have MobileInAppPurchaser on scene!");
            }
        }


        private void Start() {
            if (m_initOnStart) {
                Init();
            }
        }

        /// <summary>
        /// Initializes Consumables, Non-Consumables and Subscriptions from Prefab's serialized values
        /// </summary>
        public void Init() {
            if (!string.IsNullOrEmpty(AdsFreeSKU)
                || (ConsumablesList != null && ConsumablesList.Count > 0)
                || (NonConsumablesList != null && NonConsumablesList.Count > 0)
                || (SubscriptionsList != null && SubscriptionsList.Count > 0)) {
                Init(AdsFreeSKU, ConsumablesList, NonConsumablesList, SubscriptionsList);
            }
            else {
                Debug.LogWarning("Cant init InApps with null Data!");
            }
        }


        /// <summary>
        /// Initializes Consumables, Non-Consumables and Subscriptions
        /// </summary>
        /// <param name="a_subscriptionSKUs">all subscription SKUs from your configs, which are valid while the subscription is active (like extra lives, accelerated construction time)</param>
        /// <param name="a_consumableSKUs">all consumable SKUs from your configs, that you can purchase multiple times (like coins, gems)</param>
        /// <param name="a_adsFreeSKU">non consumable SKU for a one-time AdsFree purchase</param>
        public void Init(string a_adsFreeSKU, List<string> a_consumableSKUs, List<string> a_nonConsumableSKUs = null, List<string> a_subscriptionSKUs = null) {
            if (!string.IsNullOrEmpty(a_adsFreeSKU)) {
                if (a_nonConsumableSKUs == null) {
                    a_nonConsumableSKUs = new List<string>() { a_adsFreeSKU };
                }
                else {
                    if (!a_nonConsumableSKUs.Contains(a_adsFreeSKU)) {
                        a_nonConsumableSKUs.Add(a_adsFreeSKU);
                    }
                }
            }

            Init(a_nonConsumableSKUs, a_consumableSKUs, a_subscriptionSKUs);
        }


        /// <summary>
        /// Initializes Consumables, Non-Consumables and Subscriptions
        /// </summary>
        /// <param name="a_subscriptionSKUs">all subscription SKUs from your configs, which are valid while the subscription is active (like extra lives, accelerated construction time)</param>
        /// <param name="a_consumableSKUs">all consumable SKUs from your configs, that you can purchase multiple times (like coins, gems)</param>
        /// <param name="a_nonConsumableSKUs">all non consumable SKUs from your configs, for a one-time purchase (like AdsFree)</param>
        public void Init(List<string> a_nonConsumableSKUs, List<string> a_consumableSKUs, List<string> a_subscriptionSKUs = null) {
            StartCoroutine(InitCoroutine(a_nonConsumableSKUs, a_consumableSKUs, a_subscriptionSKUs));
        }

        private IEnumerator InitCoroutine(List<string> a_nonConsumableSKUs, List<string> a_consumableSKUs, List<string> a_subscriptionSKUs) {
            ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            if (a_nonConsumableSKUs != null && a_nonConsumableSKUs.Count > 0) {
                foreach (string sku in a_nonConsumableSKUs) {
                    builder.AddProduct(sku, ProductType.NonConsumable);
                }
            }

            if (a_consumableSKUs != null && a_consumableSKUs.Count > 0) {
                foreach (string sku in a_consumableSKUs) {
                    builder.AddProduct(sku, ProductType.Consumable);
                }
            }

            if (a_subscriptionSKUs != null && a_subscriptionSKUs.Count > 0) {
                foreach (string sku in a_subscriptionSKUs) {
                    builder.AddProduct(sku, ProductType.Subscription);
                }
            }

            if (m_debugLogsOn) {
                foreach (var product in builder.products) {
                    Debug.Log($"Added product: {product.id}, type; {product.type}");
                }
            }

            yield return new WaitWhile(() => m_gamingServices.IsLoading);

            UnityPurchasing.Initialize(this, builder);
        }
        #endregion

        #region Public
        public bool IsInitialized() {
            return (m_storeController != null && m_storeExstensionProvider != null);
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
        public Product GetProduct(string a_sku) {
            if (!string.IsNullOrEmpty(a_sku)) {
                if (IsInitialized()) {
                    return m_storeController.products.WithID(a_sku);
                }
            }
            return null;
        }


        /// <summary>
        /// Use <c>GetProductPriceString</c> to return a price string.
        /// </summary>
        public string GetProductPriceString(string a_sku) {
            if (!string.IsNullOrEmpty(a_sku)) {
                if (IsInitialized()) {
                    var product = m_storeController.products.WithID(a_sku);
                    if (product != null) {
                        return product.metadata.localizedPriceString;
                    }
                }
            }
            return "";
        }

        /// <summary>
        /// Use <c>IsSubscribedTo</c> to check if the subscription is active.
        /// <example>
        /// For example:
        /// <code>
        /// if(IsSubscribedTo("com.company.game.speed_boost")) speed *= 2;
        /// </code>
        /// </example>
        /// </summary>
        public bool IsSubscribedTo(string a_sku) {
            Product product = GetProduct(a_sku);
            if (product == null || product.receipt == null) {
                return false;
            }

            var subscriptionManager = new SubscriptionManager(product, null);
            var info = subscriptionManager.getSubscriptionInfo();

            return info.isSubscribed() == Result.True;
        }

        /// <summary>
        /// Call this method to buy
        /// </summary>
        public static bool BuyProduct(string a_sku) {
            if (Exist) {
                return Instance.BuyProductInner(a_sku);
            }

            return false;
        }
        
        public bool BuyProductInner(string a_sku) {
            if (IsInitialized()) {
                Product product = m_storeController.products.WithID(a_sku);
                if (product != null && product.availableToPurchase) {
                    if (m_debugLogsOn){
                        Debug.LogWarning($"[MobileInAppPurchaser] Purchasing product asynchronously: {product.definition.id}");
                    }

                    m_storeController.InitiatePurchase(product);
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
                if (m_debugLogsOn) {
                    Debug.LogWarning("[MobileInAppPurchaser] RestorePurchases started ...");
                }

                var google = m_storeExstensionProvider.GetExtension<IGooglePlayStoreExtensions>();
                // Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
                // the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
                google.RestoreTransactions(OnTransactionsRestored);
            }
            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.OSXPlayer) {
                if (m_debugLogsOn) {
                    Debug.LogWarning("[MobileInAppPurchaser] RestorePurchases started ...");
                }

                var apple = m_storeExstensionProvider.GetExtension<IAppleExtensions>();
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


        /// <summary>
        /// Call this method to check receipt on Non-consumable like adsfree
        /// </summary>
        public static bool HasReceipt(string a_sku) {
            if (Exist) {
                if (Instance.IsInitialized()) {
                    Product product = Instance.GetProduct(a_sku);
                    if (product != null) {
                        return product.hasReceipt;
                    }
                }
            }

            return false;
        }
        #endregion

        #region Helpers
        private void OnTransactionsRestored(bool a_result, string a_message) {
            if (a_result) {
                // This does not mean anything was restored,
                // merely that the restoration process succeeded.

                // The first phase of restoration. If no more responses are received on ProcessPurchase then 
                // no purchases are available to be restored.
                if (m_debugLogsOn) {
                    Debug.LogWarning($"[MobileInAppPurchaser] RestorePurchases continuing: {a_result}. If no further messages, no purchases available to restore.");
                }
            }
            else {
                Debug.LogError(string.Format("[MobileInAppPurchaser] RestorePurchases Restoration failed."));
            }
        }
        #endregion


        #region IStoreListener
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions) {
            m_storeController = controller;
            m_storeExstensionProvider = extensions;
        }

        public void OnInitializeFailed(InitializationFailureReason error) {
            Debug.LogWarning($"[MobileInAppPurchaser] INIT FAILED! Reason: {error}");
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message) {
            Debug.LogWarning($"[MobileInAppPurchaser] INIT FAILED! Reason: {error}");
        }

        public void OnPurchaseFailed(Product Prod, PurchaseFailureReason FailureReason) {
            string ProductSKU = Prod.definition.id;

            if (m_debugLogsOn) {
                Debug.LogWarning($"[MobileInAppPurchaser] OnPurchaseFailed: FAIL. Product: '{ProductSKU}' Receipt: {Prod.receipt}");
                Debug.LogWarning($"PurchaseFailureReason: {FailureReason}");
            }

            OnPurchaseResult?.Invoke(null);
        }

        public void OnPurchaseFailed(Product a_product, PurchaseFailureDescription a_failureDescription) {
            if (a_product != null) {
                if (m_debugLogsOn) {
                    Debug.LogWarning(
                        $"[MobileInAppPurchaser] OnPurchaseFailed: FAIL. Product: '{a_product.definition.id}' Receipt: {a_product.receipt}" +
                        $" Purchase failure reason: {a_failureDescription.reason}," +
                        $" Purchase failure details: {a_failureDescription.message}");
                }
            }

            OnPurchaseResult?.Invoke(null);
        }

        // Proccess purchase and use Unity validation to check it for freud
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs a_args) {
            if (m_debugLogsOn) {
                Debug.LogWarning($"[MobileInAppPurchaser] ProcessPurchase: PASS. Product: '{a_args.purchasedProduct.definition.id}' Receipt: {a_args.purchasedProduct.receipt}");
            }

            // NOTE: UNITY VALIDATION

            List<string> receitsIDs = new List<string>();
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
                var result = validator.Validate(a_args.purchasedProduct.receipt);
                // For informational purposes, we list the receipt(s)
                Debug.Log("Valid!");
                foreach (IPurchaseReceipt productReceipt in result) {
                    receitsIDs.Add(productReceipt.productID);
                }
            } catch (IAPSecurityException) {
                Debug.Log("Invalid receipt, not unlocking content");
                validPurchase = false;
            }

#endif

            // It is important you check not just that the receipt is valid, but also what information it contains.
            // A common technique by users attempting to access content without purchase is to supply receipts from other products or applications.
            // These receipts are genuine and do pass validation, so you should make decisions based on the product IDs parsed by the CrossPlatformValidator.

            bool isIdValid = false;
            if (receitsIDs != null && receitsIDs.Count > 0) {
                foreach (string productId in receitsIDs) {
                    //Debug.Log($"{E.purchasedProduct.definition.storeSpecificId}, and {ProductID}");
                    if (a_args.purchasedProduct.definition.storeSpecificId.Equals(productId)) {
                        isIdValid = true;
                        break;
                    }
                }
            }

#if UNITY_EDITOR
            validPurchase = isIdValid = true;
#endif

            // Process a successful purchase after validation
            if (validPurchase && isIdValid) {
                if (IsInitialized()) {
                    Product product = a_args.purchasedProduct;
                    OnPurchaseResult?.Invoke(product);

                    MadPixelAnalytics.AnalyticsManager.PaymentSucceed(product);
                }
            } else {
                OnPurchaseResult?.Invoke(null);
            }
            return PurchaseProcessingResult.Complete;
        }
        #endregion
    }
}