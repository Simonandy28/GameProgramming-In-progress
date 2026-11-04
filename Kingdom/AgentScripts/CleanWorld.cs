using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using GlowKeeper;
internal class CommandScript : IRunCommand
{
 public void Execute(ExecutionResult result)
 {
 if(EditorApplication.isPlaying)throw new Exception("Stop Play before cleanup.");
 string backup="SetupBackup/WorldCleanup-2026-11-04";
 var report=new StringBuilder();
 string active=SceneManager.GetActiveScene().path;
 string[] scenes={active,"Assets/Scenes/GlowKeeper Forest.unity","Assets/_Recovery/0.unity","Assets/_Recovery/0 (1).unity"};
 foreach(var path in scenes.Distinct())
 {
 Backup(path,backup);
 if(File.Exists(path+".meta"))Backup(path+".meta",backup);
 var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
 var objects=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).Select(t=>t.gameObject).ToArray();
 var preserved=objects.Where(g=>!g.GetComponentInParent<ZombieAI>() && !g.name.Contains("Zombie") && g.transform.root.name!="Day Night Cycle").ToDictionary(g=>g.GetInstanceID(),g=>EditorJsonUtility.ToJson(g.transform));
 var controllers=objects.SelectMany(g=>g.GetComponents<ThirdPersonAvatarController>()).ToArray();
 var avatarBefore=controllers.ToDictionary(c=>c.GetInstanceID(),c=>EditorJsonUtility.ToJson(c));
 var animators=objects.Where(g=>g.transform.root.name=="PlayerRoot").SelectMany(g=>g.GetComponents<Animator>()).ToDictionary(a=>a.GetInstanceID(),a=>EditorJsonUtility.ToJson(a));
 var schedules=objects.SelectMany(g=>g.GetComponents<TenkokuForestSchedule>()).ToArray();
 var timings=schedules.Select(c=>new[]{c.sunsetMinutes,c.nightMinutes,c.sunriseMinutes,c.daylightMinutes}).ToArray();
 foreach(var c in schedules){c.moonlightGrading=null;EditorUtility.SetDirty(c);}
 int zombies=0,deaths=0,cycles=0,helpers=0;
 foreach(var g in objects.Where(g=>g && g.GetComponent<ZombieAI>()).ToArray()){UnityEngine.Object.DestroyImmediate(g);zombies++;}
 foreach(var g in objects.Where(g=>g).ToArray())
 {
 if(!g)continue; foreach(var c in g.GetComponents<PlayerDeath>()){UnityEngine.Object.DestroyImmediate(c);deaths++;}
 if(g.GetComponent<ForestDayNightCycle>()){UnityEngine.Object.DestroyImmediate(g);cycles++;continue;}
 if(g.name=="ZombieNavMesh"||g.name.StartsWith("NoZombieWater")){UnityEngine.Object.DestroyImmediate(g);helpers++;}
 }
 foreach(var kv in preserved){var g=objects.FirstOrDefault(x=>x && x.GetInstanceID()==kv.Key);if(g && EditorJsonUtility.ToJson(g.transform)!=kv.Value)throw new Exception("Transform changed: "+g.name);}
 foreach(var c in controllers)if(EditorJsonUtility.ToJson(c)!=avatarBefore[c.GetInstanceID()])throw new Exception("Avatar movement changed");
 foreach(var a in objects.Where(g=>g).SelectMany(g=>g.GetComponents<Animator>()).Where(a=>animators.ContainsKey(a.GetInstanceID())))if(EditorJsonUtility.ToJson(a)!=animators[a.GetInstanceID()])throw new Exception("Avatar Animator changed");
 for(int i=0;i<schedules.Length;i++)if(!timings[i].SequenceEqual(new[]{schedules[i].sunsetMinutes,schedules[i].nightMinutes,schedules[i].sunriseMinutes,schedules[i].daylightMinutes}))throw new Exception("Sky timings changed");
 EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
 report.AppendLine(path+": removed zombie="+zombies+", health/death="+deaths+", legacy cycle="+cycles+", zombie navigation helpers="+helpers+"; preserved world transforms, avatar configuration/Animator and sky timings.");
 }
 string profilePath="Assets/GlowKeeper/ForestSettings/ForestPostProcessing.asset";
 Backup(profilePath,backup);
 var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
 string[] filters={"ColorAdjustments","WhiteBalance","ChannelMixer","SplitToning","ShadowsMidtonesHighlights","LiftGammaGain","ColorCurves","ColorLookup"};
 foreach(var c in profile.components.Where(c=>filters.Contains(c.GetType().Name))){c.active=false;EditorUtility.SetDirty(c);report.AppendLine("Removed color filter override: "+c.GetType().Name);}
 EditorUtility.SetDirty(profile);
 Backup("ProjectSettings/EditorBuildSettings.asset",backup);
 var old=EditorBuildSettings.scenes.Where(s=>s.path!=active).Select(s=>new EditorBuildSettingsScene(s.path,false));
 EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(active,true)}.Concat(old).ToArray();
 AssetDatabase.SaveAssets();
 EditorSceneManager.OpenScene(active,OpenSceneMode.Single);
 File.WriteAllText("AgentScripts/world-cleanup-changes.txt",report.ToString());
 result.Log(report.ToString());
 }
 static void Backup(string path,string root){string dest=root+"/"+path;Directory.CreateDirectory(Path.GetDirectoryName(dest));if(!File.Exists(dest))File.Copy(path,dest);}
}

