using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result)
 {
 const string dir="Assets/GlowKeeper/Characters/AxeWarrior";if(!AssetDatabase.IsValidFolder("Assets/GlowKeeper/Characters"))AssetDatabase.CreateFolder("Assets/GlowKeeper","Characters");if(!AssetDatabase.IsValidFolder(dir))AssetDatabase.CreateFolder("Assets/GlowKeeper/Characters","AxeWarrior");
 var p=UnityEngine.Object.FindAnyObjectByType<ThirdPersonAvatarController>();var a=p.animator;var arm=a.transform.Find("arm").GetComponent<SkinnedMeshRenderer>();
 string meshPath=dir+"/LinenSleeves.asset";var mesh=AssetDatabase.LoadAssetAtPath<UnityEngine.Mesh>(meshPath);
 if(!mesh){mesh=UnityEngine.Object.Instantiate(arm.sharedMesh);mesh.name="Linen Sleeves";var vertices=mesh.vertices;var normals=mesh.normals;for(int i=0;i<vertices.Length;i++)vertices[i]+=normals[i]*.02f;mesh.vertices=vertices;mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,meshPath);}
 string materialPath=dir+"/LinenSleeves.mat";var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
 if(!material){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.name="Warrior Linen Shirt";material.SetColor("_BaseColor",new Color(.78f,.67f,.48f));material.SetFloat("_Smoothness",.15f);material.SetFloat("_Metallic",0);AssetDatabase.CreateAsset(material,materialPath);}
 var sleeve=a.transform.Find("Linen Sleeves");if(!sleeve){var go=new GameObject("Linen Sleeves");go.transform.SetParent(arm.transform.parent,false);sleeve=go.transform;}
 sleeve.localPosition=arm.transform.localPosition;sleeve.localRotation=arm.transform.localRotation;sleeve.localScale=arm.transform.localScale;
 var renderer=sleeve.GetComponent<SkinnedMeshRenderer>();if(!renderer)renderer=sleeve.gameObject.AddComponent<SkinnedMeshRenderer>();renderer.sharedMesh=mesh;renderer.bones=arm.bones;renderer.rootBone=arm.rootBone;renderer.sharedMaterial=material;renderer.localBounds=arm.localBounds;renderer.shadowCastingMode=arm.shadowCastingMode;renderer.receiveShadows=arm.receiveShadows;
 arm.gameObject.SetActive(false);EditorSceneManager.MarkSceneDirty(p.gameObject.scene);EditorSceneManager.SaveScene(p.gameObject.scene);AssetDatabase.SaveAssets();
 result.Log("Added beige linen sleeves matching the reference beneath the existing shoulder armor and bracers. Sleeve mesh reuses the imported upper-arm skinning, with a small cloth allowance. Original imported assets unchanged.");
 }
}
