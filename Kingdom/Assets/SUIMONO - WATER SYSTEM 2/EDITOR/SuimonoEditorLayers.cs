using UnityEditor;
using UnityEngine;

// Inspector layer choices must not depend on a water component's Start/LateUpdate.
// Keep all 32 slots so unnamed layers retain their actual mask bit positions.
internal static class SuimonoEditorLayers
{
    internal static string[] Names
    {
        get
        {
            var names = new string[32];
            for (int index = 0; index < names.Length; index++)
            {
                string name = LayerMask.LayerToName(index);
                names[index] = string.IsNullOrEmpty(name) ? "Layer " + index : name;
            }
            return names;
        }
    }
}
