using System;
using System.IO;
using UnityEngine;
using UnityEditor;
internal class CommandScript : IRunCommand
{
    public void Execute(ExecutionResult result)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Lamppost/Prefab/Prefab Lamppost.prefab");
        var preview = new PreviewRenderUtility();
        Texture2D image = null;
        try
        {
            var instance = UnityEngine.Object.Instantiate(prefab);
            preview.AddSingleGO(instance);
            var renderers = instance.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) throw new Exception("No lamppost mesh found.");
            var bounds = renderers[0].bounds;
            foreach (var renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
                foreach (var material in renderer.sharedMaterials)
                {
                    if (!material || material.shader.name != "Universal Render Pipeline/Lit" || ShaderUtil.ShaderHasError(material.shader))
                        throw new Exception("Lamppost has missing or invalid material.");
                }
            }
            preview.camera.orthographic = true;
            preview.camera.orthographicSize = bounds.extents.y * 1.15f;
            var distance = bounds.size.magnitude * 2f;
            preview.camera.transform.position = bounds.center + new Vector3(0.8f, 0.15f, -1f).normalized * distance;
            preview.camera.transform.LookAt(bounds.center);
            preview.camera.nearClipPlane = 0.01f;
            preview.camera.farClipPlane = distance * 4f;
            preview.camera.clearFlags = CameraClearFlags.SolidColor;
            preview.camera.backgroundColor = new Color(0.26f,0.29f,0.32f,1f);
            preview.lights[0].intensity = 2f;
            preview.lights[0].transform.rotation = Quaternion.Euler(35,35,0);
            preview.lights[1].intensity = 1.5f;
            preview.lights[1].transform.rotation = Quaternion.Euler(315,200,0);
            preview.ambientColor = new Color(0.35f,0.35f,0.35f);
            preview.BeginStaticPreview(new Rect(0,0,512,768));
            preview.Render(true);
            image = preview.EndStaticPreview();
            var path = Path.GetFullPath("SetupBackup/LamppostURP/lamppost-fixed.png");
            File.WriteAllBytes(path, image.EncodeToPNG());
            result.Log("Preview saved: " + path + "; all prefab material slots use valid URP shaders.");
        }
        finally
        {
            if (image) UnityEngine.Object.DestroyImmediate(image);
            preview.Cleanup();
        }
    }
}
