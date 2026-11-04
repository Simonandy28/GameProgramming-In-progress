using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using GlowKeeper;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result)
 {
 if(!EditorApplication.isPlaying)throw new Exception("Enter Play for native sky checks");
 var c=UnityEngine.Object.FindAnyObjectByType<TenkokuForestSchedule>();var report=new StringBuilder();Application.runInBackground=true;EditorApplication.isPaused=false;Time.timeScale=0;
 double[] samples={0,60,264,480,696,930,1920,2820};string[] labels={"Dawn","Sunrise","Morning","Noon","Afternoon","Sunset","Night","NextDawn"};int[] hours={6,7,10,13,16,19,1,6};int index=0;int startFrame=Time.frameCount;double started=Time.realtimeSinceStartupAsDouble;c.SetElapsedTime(samples[0]);
 Directory.CreateDirectory("AgentScripts/FullDayPreviews");EditorApplication.CallbackFunction tick=null;
 tick=()=>{
 if(!EditorApplication.isPlaying||!c){Time.timeScale=1;EditorApplication.update-=tick;return;}
 if(Time.frameCount-startFrame<8||Time.realtimeSinceStartupAsDouble-started<.2)return;
 try{
 float elevation=Mathf.Asin(Mathf.Clamp(-c.library.sunlightObject.forward.y,-1,1))*Mathf.Rad2Deg;
 var expectedDate=new DateTime(c.skyYear,c.skyMonth,c.skyDay).AddHours(TenkokuForestSchedule.EvaluateSkyHours(samples[index],c.sunriseStartHour,c.sunsetMinutes,c.nightMinutes,c.sunriseMinutes,c.daylightMinutes)); bool correctClock=c.sky.currentHour==hours[index]&&c.sky.currentDay==expectedDate.Day;
 if(!correctClock)throw new Exception("Native date/time mismatch at "+labels[index]);
 if(index==3&&elevation<35)throw new Exception("Noon sun is not high in the sky: "+elevation);
 if(index==6&&elevation>-20)throw new Exception("Midnight sun is above expected horizon: "+elevation);
 report.AppendLine("PASS "+labels[index]+" phase="+c.Phase+" clock="+c.sky.currentYear+"-"+c.sky.currentMonth+"-"+c.sky.currentDay+" "+c.sky.currentHour.ToString("00")+":"+c.sky.currentMinute.ToString("00")+" sun elevation="+elevation.ToString("F2")+" sun enabled="+c.library.lightObjectWorld.enabled+" moon enabled="+c.library.lightObjectNight.enabled);
 if(index!=0&&index!=7)Capture("AgentScripts/FullDayPreviews/"+labels[index]+".png");
 if(++index==samples.Length){report.AppendLine("PASS native sky reaches noon, transitions to night and advances to the following day's sunrise.");File.WriteAllText("AgentScripts/full-day-cycle-native-check.txt",report.ToString());c.SetElapsedTime(0);Time.timeScale=1;Application.runInBackground=PlayerSettings.runInBackground;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;EditorApplication.update-=tick;return;}
 c.SetElapsedTime(samples[index]);startFrame=Time.frameCount;started=Time.realtimeSinceStartupAsDouble;
 }catch(Exception e){File.WriteAllText("AgentScripts/full-day-cycle-native-check.txt",report+"\nFAIL "+e);Time.timeScale=1;Application.runInBackground=PlayerSettings.runInBackground;EditorApplication.update-=tick;}
 };
 EditorApplication.update+=tick;result.Log("Previewing the native Tenkoku sky at dawn, sunrise, morning, noon, afternoon, sunset, midnight and the next dawn.");
 }
 static void Capture(string path){var c=Camera.main;var oldPosition=c.transform.position;var oldRotation=c.transform.rotation;float fov=c.fieldOfView;var p=UnityEngine.Object.FindAnyObjectByType<ThirdPersonAvatarController>();c.transform.position=p.transform.position+Vector3.up*5.5f-Vector3.forward*7;c.transform.LookAt(p.transform.position+Vector3.up*7+Vector3.forward*35);c.fieldOfView=65;var old=c.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1280,720,24,RenderTextureFormat.ARGB32);Texture2D img=null;try{c.targetTexture=rt;c.Render();RenderTexture.active=rt;img=new Texture2D(1280,720,TextureFormat.RGB24,false);img.ReadPixels(new Rect(0,0,1280,720),0,0);img.Apply();File.WriteAllBytes(path,img.EncodeToPNG());}finally{c.transform.SetPositionAndRotation(oldPosition,oldRotation);c.fieldOfView=fov;c.targetTexture=old;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(rt);if(img)UnityEngine.Object.DestroyImmediate(img);}}
}

