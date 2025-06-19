using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Purchasing;

namespace MadPixel {
    public static class ExtensionMethods {
        public static MPReceipt GetReceipt(Product a_product) {
            MPReceipt receipt = new MPReceipt();
            receipt.sku = a_product.definition.id;
            receipt.product = a_product;

            StoreNamePurcahseInfoSignature(a_product, out string o_data, out string o_signature);

            receipt.data = o_data;
            receipt.signature = o_signature;

            return receipt;
        }


        private static void StoreNamePurcahseInfoSignature(Product a_product, out string o_purchaseInfo, out string o_signature) {
            SimpleJSON.JSONNode jsNode = SimpleJSON.JSON.Parse(a_product.receipt);

            o_signature = "empty";
            o_purchaseInfo = "empty";

#if UNITY_IOS
            o_purchaseInfo = jsNode["Payload"];
#elif UNITY_ANDROID
            SimpleJSON.JSONNode payloadNode = SimpleJSON.JSON.Parse(jsNode["Payload"]);
            o_signature = payloadNode["signature"];
            o_purchaseInfo = payloadNode["json"];
#endif

            if (o_signature != "empty") { o_signature = RemoveQuotes(o_signature); }
            if (o_purchaseInfo != "empty") { o_purchaseInfo = RemoveQuotes(o_purchaseInfo); }
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

        public static string RemoveAllWhitespacesAndNewLines(string a_string) {
            if (!string.IsNullOrEmpty(a_string)) {
                return (a_string.Replace("\n", string.Empty).Replace("\r", string.Empty).Replace(" ", string.Empty));
            }
            return (a_string);
        }
    }

    public class MPReceipt {
        public string sku;
        public Product product;
        public string signature;
        public string data;
    }

    public class XOREncryption {
        public static string Encrypt(string a_value, string a_key) {
            return Convert.ToBase64String(XOREncryption.Encode(Encoding.UTF8.GetBytes(a_value), Encoding.UTF8.GetBytes(a_key)));
        }

        public static string Decrypt(string a_value, string a_key) {
            return Encoding.UTF8.GetString(XOREncryption.Encode(Convert.FromBase64String(a_value), Encoding.UTF8.GetBytes(a_key)));
        }

        private static byte[] Encode(byte[] a_bytes, byte[] a_key) {
            int index1 = 0;
            for (int index2 = 0; index2 < a_bytes.Length; ++index2) {
                a_bytes[index2] ^= a_key[index1];
                if (++index1 == a_key.Length)
                    index1 = 0;
            }
            return a_bytes;
        }
    }
}
