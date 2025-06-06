#if UNITY_EDITOR
using MadPixelAnalytics;
using UnityEditor;
using UnityEngine;

namespace MadPixel.Editor {
    [CustomEditor(typeof(AppsFlyerComp))]
    public class MPCAppsflyerCustomEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            serializedObject.Update();
            SerializedProperty showPurchaseConnectorToggle = serializedObject.FindProperty("m_usePurchaseConnector");
            EditorGUILayout.PropertyField(showPurchaseConnectorToggle);

            if (!showPurchaseConnectorToggle.boolValue) {
                SerializedProperty monetizationKeyField = serializedObject.FindProperty("m_monetizationPublicKey");
                EditorGUILayout.PropertyField(monetizationKeyField);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif