using System;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

public class GamingServices : MonoBehaviour {
    [HideInInspector] public bool IsLoading = true;
    const string k_Environment = "production";
    public void Initialize(/*Action onSuccess, Action<string> onError*/) {
        IsLoading = true;
        try {
            var options = new InitializationOptions().SetEnvironmentName(k_Environment);
            UnityServices.InitializeAsync(options).ContinueWith(task => {
                IsLoading = false;
            });
        } catch (Exception exception) {
            Debug.LogError(exception.Message);
            IsLoading = false;
        }
    }
}