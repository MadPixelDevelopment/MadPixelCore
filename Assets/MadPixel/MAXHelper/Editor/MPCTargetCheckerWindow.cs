using UnityEditor;
using UnityEngine;

public class MPCTargetCheckerWindow : EditorWindow {
    public static void ShowWindow(int min, int target) {
        var instance = GetWindow<MPCTargetCheckerWindow>("Target API check", true);
        instance.minSize = new Vector2(400, 200);
        instance.Show();
    }

    private void OnGUI() {
        GUILayout.Space(20);

        GUILayout.Label("Your API levels were not correct", EditorStyles.boldLabel);

        GUILayout.Space(20);

        GUILayout.Label($"We have updated your target API to 33");
        GUILayout.Label("We have updated your min API to 24 ");

        GUILayout.Space(20);

    }
}