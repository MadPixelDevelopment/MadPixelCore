using UnityEngine;

namespace Unity.Services.LevelPlay
{
    static class ObjectUtility
    {
        internal static void DestroySafely<T>(T obj) where T : Object
        {
            if (obj == null) return;

            if (Application.isPlaying)
            {
                Object.Destroy(obj);
            }
            else
            {
                Object.DestroyImmediate(obj);
            }
        }
    }
}