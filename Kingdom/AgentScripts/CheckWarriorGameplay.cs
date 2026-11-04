using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result)
 {
 if(!EditorApplication.isPlaying)throw new Exception("Start Play before this check");
 var p=UnityEngine.Object.FindAnyObjectByType<ThirdPersonAvatarController>();var a=p.animator;var collector=p.GetComponent<StaffFireflyCollector>();var depositor=p.GetComponent<StaffLampDepositor>();var cc=p.GetComponent<CharacterController>();
 var start=p.transform.position;var rotation=p.transform.rotation;bool background=Application.runInBackground;Application.runInBackground=true;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
 var water=UnityEngine.Object.FindObjectsByType<BoxCollider>().Where(c=>c.CompareTag("Water")&&c.isTrigger).OrderByDescending(c=>c.bounds.size.x*c.bounds.size.z).First();
 var lamp=depositor.lamps.First(l=>l&&l.fireflyDestination);var flies=collector.fireflies.Where(f=>f.fireflyRoot).Select(f=>f.fireflyRoot).ToArray();
 var report=new StringBuilder();int stage=0;double then=Time.timeAsDouble;Vector3 previous=p.transform.position;float waterY=0;int frames=0;
 EditorApplication.CallbackFunction tick=null;
 Action<Key[]> keys=k=>InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(k));
 Action<Vector3> place=v=>{cc.enabled=false;p.transform.position=v;cc.enabled=true;};
 Action<bool,string> check=(ok,label)=>report.AppendLine((ok?"PASS ":"FAIL ")+label);
 tick=()=>{
 if(!EditorApplication.isPlaying||!p){EditorApplication.update-=tick;File.WriteAllText("AgentScripts/axe-warrior-playcheck.txt",report+"\nPlay ended before checks finished.");return;}
 double elapsed=Time.timeAsDouble-then;
 try{
 switch(stage){
 case 0:
  check(a.avatar.isHuman&&a.avatar.isValid&&a.isHuman,"Warrior humanoid retargeting valid");
  check(a.runtimeAnimatorController.name=="WizardAnimator","Original animation controller retained");
  previous=p.transform.position;keys(new[]{Key.W});stage++;then=Time.timeAsDouble;break;
 case 1:
  if(elapsed<.6)break;
  check(Vector3.Distance(previous,p.transform.position)>.1f&&a.GetBool("IsWalking"),"Walk input moves warrior and activates walking");
  report.AppendLine("Walk clips="+string.Join(",",a.GetCurrentAnimatorClipInfo(0).Select(c=>c.clip.name)));
  keys(new[]{Key.W,Key.LeftShift});previous=p.transform.position;stage++;then=Time.timeAsDouble;break;
 case 2:
  if(elapsed<.6)break;
  check(Vector3.Distance(previous,p.transform.position)>.1f&&a.GetBool("IsRunning"),"Run input moves warrior and activates running");
  report.AppendLine("Run clips="+string.Join(",",a.GetCurrentAnimatorClipInfo(0).Select(c=>c.clip.name)));
  Capture("AgentScripts/axe-warrior-running.png");keys(new Key[0]);place(start);stage++;then=Time.timeAsDouble;break;
 case 3:
  if(elapsed<.5||!cc.isGrounded){if(elapsed>3){check(false,"Grounded before jump");stage=5;then=Time.timeAsDouble;}break;}
  previous=p.transform.position;keys(new[]{Key.Space});stage++;then=Time.timeAsDouble;break;
 case 4:
  if(elapsed<.15)break;
  check(a.GetBool("IsJumping")&&p.transform.position.y>previous.y,"Jump input activates jump animation and lifts avatar");keys(new Key[0]);stage++;then=Time.timeAsDouble;break;
 case 5:
  if(elapsed<1.6)break;
  place(water.bounds.center-Vector3.up*.25f);keys(new[]{Key.W});stage++;then=Time.timeAsDouble;break;
 case 6:
  if(elapsed<.15)break;
  check(a.GetBool("IsSwimming")&&a.GetBool("IsSwimMoving"),"Water trigger and swimming movement work with new Animator");
  report.AppendLine("Swim clips="+string.Join(",",a.GetCurrentAnimatorClipInfo(0).Select(c=>c.clip.name)));
  Capture("AgentScripts/axe-warrior-swimming.png");waterY=p.transform.position.y;keys(new[]{Key.Space});stage++;then=Time.timeAsDouble;break;
 case 7:
  if(elapsed<.12)break;
  check(p.transform.position.y>waterY,"Swimming ascend input works");waterY=p.transform.position.y;keys(new[]{Key.LeftCtrl});stage++;then=Time.timeAsDouble;break;
 case 8:
  if(elapsed<.12)break;
  check(p.transform.position.y<waterY,"Swimming descend input works");keys(new Key[0]);place(start);stage++;then=Time.timeAsDouble;break;
 case 9:
  if(elapsed<.4)break;
  for(int i=0;i<flies.Length;i++)flies[i].position=p.transform.position+Vector3.up+Vector3.right*(1+i*.2f);
  check(collector.fireflyHoldPoint&&collector.fireflyHoldPoint.root==p.transform,"Existing firefly hold point retained on player staff");
  keys(new[]{Key.X});stage++;then=Time.timeAsDouble;break;
 case 10:
  if(elapsed<.1)break;keys(new Key[0]);stage++;then=Time.timeAsDouble;break;
 case 11:
  if(elapsed<2)break;
  check(collector.CollectedCount==flies.Length,"X collects all nearby fireflies onto the retained staff");
  check(flies.All(f=>Vector3.Distance(f.position,collector.fireflyHoldPoint.position)<.35f),"Collected fireflies follow the new avatar staff attachment");
  place(lamp.fireflyDestination.position+Vector3.right*2);stage++;then=Time.timeAsDouble;break;
 case 12:
  if(elapsed<.2)break;keys(new[]{Key.E});stage++;then=Time.timeAsDouble;break;
 case 13:
  if(elapsed<.1)break;keys(new Key[0]);stage++;then=Time.timeAsDouble;break;
 case 14:
  if(elapsed<1.5)break;
  check(lamp.IsLit&&collector.CollectedCount==0,"E deposits fireflies and lights lamp");
  check(flies.All(f=>f.IsChildOf(lamp.fireflyDestination)),"Deposited fireflies reach the lamp destination");
  check(UnityEngine.Object.FindAnyObjectByType<ThirdPersonCamera>().target==p.transform,"Original third-person camera still follows PlayerRoot");
  report.AppendLine("Completed at gameplay time="+Time.time+", scale="+Time.timeScale);
  keys(new Key[0]);place(start);p.transform.rotation=rotation;Application.runInBackground=background;
  File.WriteAllText("AgentScripts/axe-warrior-playcheck.txt",report.ToString());EditorApplication.update-=tick;break;
 }
 }catch(Exception e){keys(new Key[0]);File.WriteAllText("AgentScripts/axe-warrior-playcheck.txt",report+"\nERROR "+e);EditorApplication.update-=tick;Application.runInBackground=background;}
 };
 EditorApplication.update+=tick;result.Log("Queued movement, humanoid animation, jump, swim, and firefly collection/deposit checks using the existing input actions.");
 }
 static void Capture(string path){var camera=Camera.main;var previous=camera.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1280,720,24,RenderTextureFormat.ARGB32);Texture2D image=null;try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());}finally{camera.targetTexture=previous;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(rt);if(image)UnityEngine.Object.DestroyImmediate(image);}}
}
