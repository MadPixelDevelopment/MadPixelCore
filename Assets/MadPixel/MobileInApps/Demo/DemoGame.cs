using System.Collections;
using System.Collections.Generic;
using MadPixel.InApps;
using UnityEngine;
using UnityEngine.Purchasing;

namespace MadPixel.Examples {
    public class DemoGame : MonoBehaviour {
        [SerializeField] private List<string> InappProducts;
        [SerializeField] private string AdsFreeProduct;

        private void Start() {
            MobileInAppPurchaser.Instance.Init(AdsFreeProduct, InappProducts);
        }

        private void OnDestroy() {
            Unsubscribe();
        }

        private void Unsubscribe() {
            if (MobileInAppPurchaser.Exist) {
                MobileInAppPurchaser.Instance.OnPurchaseResult -= OnPurchaseResult;
            }
        }

        private void OnPurchaseResult(Product Prod) {
            if (Prod != null) {
                Debug.LogWarning($"Purchase complete! Implement logic for {Prod.definition.id}");
            }
            else {
                Debug.LogError($"Purchase went wrong!");
            }

            Unsubscribe();
        }

        public void OnBuyButtonClick() {
            MobileInAppPurchaser.Instance.OnPurchaseResult += OnPurchaseResult;
            MobileInAppPurchaser.BuyProduct(AdsFreeProduct);
        }
    } 
}
