using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Animations;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result)
 {
 if(EditorApplication.isPlaying)throw new Exception("Stop Play before replacing the model");
 var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(scene.path!="Assets/Glowkeeper_Update.unity")throw new Exception("Wrong scene");
 var player=UnityEngine.Object.FindAnyObjectByType<ThirdPersonAvatarController>();var old=player.animator;
 if(old.name!="Jumanji_AllAnim")throw new Exception("Expected original avatar");
 var prefabRoot=PrefabUtility.GetOutermostPrefabInstanceRoot(old.gameObject); if(prefabRoot)PrefabUtility.UnpackPrefabInstance(prefabRoot,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
 var root=player.transform;
 var rootBefore=EditorJsonUtility.ToJson(root);var controllerBefore=EditorJsonUtility.ToJson(player.GetComponent<CharacterController>());
 var movementBefore=EditorJsonUtility.ToJson(player);var collect=player.GetComponent<StaffFireflyCollector>();var collectBefore=EditorJsonUtility.ToJson(collect);
 var camera=UnityEngine.Object.FindAnyObjectByType<ThirdPersonCamera>();var cameraBefore=EditorJsonUtility.ToJson(camera);
 var world=scene.GetRootGameObjects().Where(g=>g!=player.gameObject).SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToDictionary(t=>t,t=>EditorJsonUtility.ToJson(t));
 var staff=old.GetComponentsInChildren<Transform>(true).Single(t=>t.name=="Staff_mesh_grp");var constraint=staff.GetComponent<ParentConstraint>();var socket=constraint.GetSource(0).sourceTransform;
 old.Rebind();old.Update(0);
 var oldHand=old.GetBoneTransform(HumanBodyBones.RightHand);
 Quaternion handOrientation=oldHand.rotation;
 var oldBounds=BoundsOf(old.gameObject,staff);
 var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Axe Warrior/Prefab/Axe_Warrior.prefab");
 var warrior=(GameObject)PrefabUtility.InstantiatePrefab(prefab,root);warrior.name="Axe Warrior Avatar";
 warrior.transform.localPosition=Vector3.zero;warrior.transform.localRotation=Quaternion.identity;warrior.transform.localScale=Vector3.one;
 var animator=warrior.GetComponent<Animator>();if(!animator.avatar||!animator.avatar.isHuman||!animator.avatar.isValid)throw new Exception("Warrior rig invalid");
 animator.runtimeAnimatorController=old.runtimeAnimatorController;animator.applyRootMotion=old.applyRootMotion;animator.cullingMode=old.cullingMode;animator.updateMode=old.updateMode;
 // Retain the functional staff in the right hand; the warrior keeps its left axe.
 warrior.GetComponentsInChildren<Transform>(true).Single(t=>t.name=="Axe_R").gameObject.SetActive(false);
 animator.Rebind();animator.Update(0);
 var bounds=BoundsOf(warrior,null);
 float scale=oldBounds.size.y/bounds.size.y;
 warrior.transform.localScale=Vector3.one*scale;
 bounds=BoundsOf(warrior,null);
 var cc=player.GetComponent<CharacterController>();float ground=root.position.y+cc.center.y-cc.height*.5f;
 warrior.transform.position+=Vector3.up*(ground+.02f-bounds.min.y);
 var newHand=animator.GetBoneTransform(HumanBodyBones.RightHand);
 // Keep the old staff's grip frame; compensate for the two rigs' different bone axes.
 staff.SetParent(root,true);
 socket.SetParent(newHand,false);socket.localPosition=Vector3.zero;socket.rotation=handOrientation;
 constraint.SetSource(0,new ConstraintSource{sourceTransform=socket,weight=1});
 player.animator=animator;
 UnityEngine.Object.DestroyImmediate(old.gameObject);
 animator.Update(0);
 EditorUtility.SetDirty(player);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
 if(EditorJsonUtility.ToJson(root)!=rootBefore||EditorJsonUtility.ToJson(cc)!=controllerBefore||EditorJsonUtility.ToJson(camera)!=cameraBefore||EditorJsonUtility.ToJson(collect)!=collectBefore)throw new Exception("A preserved control or attachment changed");
 foreach(var kv in world)if(EditorJsonUtility.ToJson(kv.Key)!=kv.Value)throw new Exception("World transform changed: "+kv.Key.name);
 int missing=scene.GetRootGameObjects().Sum(g=>g.GetComponentsInChildren<Transform>(true).Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)));
 if(missing>0||!collect.fireflyHoldPoint||!collect.fireflyHoldPoint.IsChildOf(staff))throw new Exception("Attachment or script reference lost");
 var report="Replaced only the avatar figure with Axe_Warrior.prefab. Warrior uses its own valid humanoid Avatar and the original animation controller. Scale="+scale+", old visual height="+oldBounds.size.y+", new visual height="+BoundsOf(warrior,null).size.y+". Feet aligned 0.02m above controller base. Existing right-hand staff/firefly point and left axe retained. PlayerRoot, collider, camera, collection settings and world transforms unchanged. Missing scripts="+missing+".";
 File.WriteAllText("AgentScripts/axe-warrior-replacement.txt",report);result.Log(report);
 }
 static Bounds BoundsOf(GameObject go,Transform exclude){bool first=true;var bounds=new Bounds();foreach(var r in go.GetComponentsInChildren<Renderer>()){if(exclude&&r.transform.IsChildOf(exclude))continue;if(!r.enabled)continue;if(first){bounds=r.bounds;first=false;}else bounds.Encapsulate(r.bounds);}return bounds;}
}
