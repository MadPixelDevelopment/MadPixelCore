using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

namespace MadPixel {
    public static class ExtensionMethods {
        public static MPReceipt GetReceipt(Product product) {
            MPReceipt receipt = new MPReceipt();
            receipt.SKU = product.definition.id;
            receipt.Product = product;

            StoreNamePurcahseInfoSignature(product, out string D, out string S);

            receipt.Data = D;
            receipt.Signature = S;

            return receipt;
        }


        private static void StoreNamePurcahseInfoSignature(Product product, out string purchaseInfo, out string signature) {
            SimpleJSON.JSONNode jsNode = SimpleJSON.JSON.Parse(product.receipt);

            signature = "empty";
            purchaseInfo = "empty";

#if UNITY_IOS
            purchaseInfo = jsNode["Payload"];
#elif UNITY_ANDROID
            SimpleJSON.JSONNode payloadNode = SimpleJSON.JSON.Parse(jsNode["Payload"]);
            signature = payloadNode["signature"];
            purchaseInfo = payloadNode["json"];
#endif

            if (signature != "empty") { signature = RemoveQuotes(signature); }
            if (purchaseInfo != "empty") { purchaseInfo = RemoveQuotes(purchaseInfo); }
        }

        private static string RemoveQuotes(string str) {
            if (string.IsNullOrEmpty(str)) {
                Debug.Log("ERROR! RemoveQuotes: string is null or empty!");
                return "empty";
            }
            string newStr = str;

            if (str[0] == '"')
                newStr = newStr.Remove(0, 1);
            if (str[str.Length - 1] == '"')
                newStr = newStr.Remove(newStr.Length - 1, 1);

            return newStr;
        }

        public static string RemoveAllWhitespacesAndNewLines(string InString) {
            if (!string.IsNullOrEmpty(InString)) {
                return (InString.Replace("\n", string.Empty).Replace("\r", string.Empty).Replace(" ", string.Empty));
            }
            return (InString);
        }
    }

    public class MPReceipt {
        public string SKU;
        public Product Product;
        public string Signature;
        public string Data;
    }
}
