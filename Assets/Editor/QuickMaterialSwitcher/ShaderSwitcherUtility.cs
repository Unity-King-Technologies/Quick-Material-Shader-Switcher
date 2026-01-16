using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class ShaderSwitcherUtility
{
    private static List<Shader> cachedShaders = new List<Shader>();

    public static List<Shader> GetAllShaders()
    {
        if (cachedShaders.Count == 0)
        {
            string[] guids = AssetDatabase.FindAssets("t:Shader");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
                if (shader != null)
                {
                    cachedShaders.Add(shader);
                }
            }
        }
        return cachedShaders;
    }

    public static void SwitchShader(Material material, Shader newShader)
    {
        if (material == null || newShader == null) return;

        Undo.RecordObject(material, "Switch Shader");
        material.shader = newShader;

        // Attempt to preserve properties if possible
        // This is a simplified version; in practice, you'd map properties intelligently
        if (newShader.name.Contains("Standard") && material.HasProperty("_MainTex"))
        {
            // Keep main texture if switching to similar shader
        }
    }

    public static void BatchSwitchShaders(IEnumerable<Material> materials, Shader newShader)
    {
        foreach (var mat in materials)
        {
            SwitchShader(mat, newShader);
        }
    }
}
