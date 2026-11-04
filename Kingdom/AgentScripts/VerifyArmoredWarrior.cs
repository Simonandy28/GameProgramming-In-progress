using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using GlowKeeper;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result)
 {
 if(!EditorApplication.isPlaying)throw new Exception("Start Play before verification");
 var p=UnityEngine.Object.FindAnyObjectByType<ThirdPersonAvatarController>();var a=p.animator;var cc=p.GetComponent<CharacterController>();var sky=UnityEngine.Object.FindAnyObjectByType<TenkokuForestSchedule>();
 var start=p.transform.position;var rot=p.transform.rotation;var report=new StringBuilder();Application.runInBackground=true;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
 var water=UnityEngine.Object.FindObjectsByType<BoxCollider>().Where(c=>c.CompareTag("Water")&&c.isTrigger).OrderByDescending(c=>c.bounds.size.x*c.bounds.size.z).First();
 sky.SetElapsedTime(2100);int stage=0;double then=Time.timeAsDouble;Vector3 before=start;float waterY=0;
 Action<Key[]> keys=k=>InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(k));Action<Vector3> place=v=>{cc.enabled=false;p.transform.position=v;cc.enabled=true;};Action<bool,string> check=(ok,label)=>report.AppendLine((ok?"PASS ":"FAIL ")+label);
 EditorApplication.CallbackFunction tick=null;
 tick=()=>{
 if(!EditorApplication.isPlaying||!p){EditorApplication.update-=tick;File.WriteAllText("AgentScripts/warrior-armor-playcheck.txt",report+"\nPlay ended before checks finished.");return;}
 double elapsed=Time.timeAsDouble-then;
 try{
 switch(stage){
 case 0:
  if(elapsed<1.2)break;
  check(a.avatar.isHuman&&a.avatar.isValid,"Humanoid rig valid");
  var names=a.GetComponentsInChildren<Renderer>().Select(r=>r.name).ToArray();
  check(new[]{"A1_Armor","A1_Armor_Belt","A1_Pant","A1_Boots","A1_Knee Plate_L","A1_Knee Plate_R","A1_Shoulder Plates","Linen Sleeves","Axe_L","Axe_R"}.All(n=>names.Contains(n)),"Full clothed armor set and both axes visible");
  check(!p.GetComponentsInChildren<Transform>(true).Any(t=>t.name.Contains("Staff")||t.name=="FireflyHoldPoint"),"No magic staff or staff attachment");
  check(p.GetComponents<MonoBehaviour>().Length==1,"PlayerRoot retains movement only; collection/deposit mechanics absent");
  check(sky.sunsetMinutes==3&&sky.nightMinutes==30&&sky.sunriseMinutes==2&&sky.daylightMinutes==2,"Original sky timings retained");
  foreach(bool left in new[]{true,false}){float gap=SoleGap(p,left);report.AppendLine((left?"Left":"Right")+" boot sole-to-ground gap="+gap+"m");check(Mathf.Abs(gap)<.065f,(left?"Left":"Right")+" boot planted on terrain");}
  var leftFoot=a.GetBoneTransform(HumanBodyBones.LeftFoot);var rightFoot=a.GetBoneTransform(HumanBodyBones.RightFoot);
  check(p.transform.InverseTransformPoint(leftFoot.position).x<p.transform.InverseTransformPoint(rightFoot.position).x,"Legs retain correct left/right stance");
  Capture("AgentScripts/warrior-armor-final.png",p,true);before=p.transform.position;keys(new[]{Key.W});stage++;then=Time.timeAsDouble;break;
 case 1:
  if(elapsed<.8)break;check(Vector3.Distance(before,p.transform.position)>.2f&&a.GetBool("IsWalking")&&a.GetCurrentAnimatorStateInfo(0).IsName("Walking"),"Walking moves the character and reaches Walking animation");
  before=p.transform.position;keys(new[]{Key.W,Key.LeftShift});stage++;then=Time.timeAsDouble;break;
 case 2:
  if(elapsed<.8)break;check(Vector3.Distance(before,p.transform.position)>.2f&&a.GetBool("IsRunning")&&a.GetCurrentAnimatorStateInfo(0).IsName("Running"),"Running moves the character and reaches Running animation");
  Capture("AgentScripts/warrior-armor-running.png",p,false);keys(new Key[0]);place(start);stage++;then=Time.timeAsDouble;break;
 case 3:
  if(elapsed<.4||!cc.isGrounded){if(elapsed>3){check(false,"Grounded before jump");stage=5;then=Time.timeAsDouble;}break;}
  before=p.transform.position;keys(new[]{Key.Space});stage++;then=Time.timeAsDouble;break;
 case 4:
  if(elapsed<.15)break;check(a.GetBool("IsJumping")&&p.transform.position.y>before.y,"Jump raises avatar and activates jumping");keys(new Key[0]);stage++;then=Time.timeAsDouble;break;
 case 5:
  if(elapsed<1.6)break;place(water.bounds.center-Vector3.up*.25f);keys(new[]{Key.W});stage++;then=Time.timeAsDouble;break;
 case 6:
  if(elapsed<1)break;check(a.GetBool("IsSwimming")&&a.GetBool("IsSwimMoving")&&a.GetCurrentAnimatorStateInfo(0).IsName("Swimming"),"Swimming reaches actual Swimming animation with armor");
  Capture("AgentScripts/warrior-armor-swimming.png",p,false);waterY=p.transform.position.y;keys(new[]{Key.Space});stage++;then=Time.timeAsDouble;break;
 case 7:
  if(elapsed<.12)break;check(p.transform.position.y>waterY,"Swimming ascent works");waterY=p.transform.position.y;keys(new[]{Key.LeftCtrl});stage++;then=Time.timeAsDouble;break;
 case 8:
  if(elapsed<.12)break;check(p.transform.position.y<waterY,"Swimming descent works");keys(new Key[0]);place(start);p.transform.rotation=rot;sky.SetElapsedTime(0);Application.runInBackground=PlayerSettings.runInBackground;
  report.AppendLine("Completed at "+Time.time+" seconds; timeScale="+Time.timeScale);File.WriteAllText("AgentScripts/warrior-armor-playcheck.txt",report.ToString());EditorApplication.update-=tick;break;
 }
 }catch(Exception e){keys(new Key[0]);File.WriteAllText("AgentScripts/warrior-armor-playcheck.txt",report+"\nERROR "+e);EditorApplication.update-=tick;Application.runInBackground=PlayerSettings.runInBackground;}
 };
 EditorApplication.update+=tick;result.Log("Checking clothing, staff removal, boot-ground contact, walking/running, jumping and swimming using live input.");
 }
 static float SoleGap(ThirdPersonAvatarController p,bool left){var r=p.animator.GetComponentsInChildren<SkinnedMeshRenderer>().Single(x=>x.name=="A1_Boots");var mesh=new UnityEngine.Mesh();r.BakeMesh(mesh,false);var points=mesh.vertices.Select(v=>r.transform.TransformPoint(v)).Where(v=>(p.transform.InverseTransformPoint(v).x<0)==left).ToArray();var sole=points.OrderBy(v=>v.y).First();var hit=Physics.RaycastAll(sole+Vector3.up*.6f,Vector3.down,2,~0,QueryTriggerInteraction.Ignore).Where(h=>!h.collider.transform.IsChildOf(p.transform)&&h.normal.y>.4f).OrderBy(h=>h.distance).First();UnityEngine.Object.DestroyImmediate(mesh);return sole.y-hit.point.y;}
 static void Capture(string path,ThirdPersonAvatarController p,bool front){var c=Camera.main;var pos=c.transform.position;var rotation=c.transform.rotation;float fov=c.fieldOfView;var old=c.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1000,1000,24,RenderTextureFormat.ARGB32);Texture2D img=null;try{if(front){c.transform.position=p.transform.position+p.transform.forward*3.5f+p.transform.right*.5f+Vector3.up*1.1f;c.transform.LookAt(p.transform.position+Vector3.up*.85f);c.fieldOfView=42;}c.targetTexture=rt;c.Render();RenderTexture.active=rt;img=new Texture2D(1000,1000,TextureFormat.RGB24,false);img.ReadPixels(new Rect(0,0,1000,1000),0,0);img.Apply();File.WriteAllBytes(path,img.EncodeToPNG());}finally{c.transform.SetPositionAndRotation(pos,rotation);c.fieldOfView=fov;c.targetTexture=old;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(rt);if(img)UnityEngine.Object.DestroyImmediate(img);}}
}
