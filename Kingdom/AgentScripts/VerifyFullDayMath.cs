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
 var c=UnityEngine.Object.FindAnyObjectByType<TenkokuForestSchedule>();var report=new StringBuilder();
 double[] times={0,60,120,264,408,480,552,696,840,930,1020,1920,2820,5640};
 double[] expected={6,7,8,10,12,13,14,16,18,19,20,25,30,54};
 string[] phases={"Sunrise","Sunrise","Morning","Morning","Noon","Noon","Afternoon","Afternoon","Sunset","Sunset","Night","Night","Sunrise","Sunrise"};
 for(int i=0;i<times.Length;i++){double hours=TenkokuForestSchedule.EvaluateSkyHours(times[i],c.sunriseStartHour,c.sunsetMinutes,c.nightMinutes,c.sunriseMinutes,c.daylightMinutes);string phase=TenkokuForestSchedule.GetPhase(times[i],c.sunsetMinutes,c.nightMinutes,c.sunriseMinutes,c.daylightMinutes);if(Math.Abs(hours-expected[i])>1e-7||phase!=phases[i])throw new Exception("Boundary mismatch at "+times[i]+": "+hours+"/"+phase);report.AppendLine("PASS "+times[i]+"s -> "+hours+" absolute sky hours, "+phase);}
 double prior=double.NegativeInfinity;for(double t=0;t<=c.LoopSeconds*2;t+=.5){double hours=TenkokuForestSchedule.EvaluateSkyHours(t,c.sunriseStartHour,c.sunsetMinutes,c.nightMinutes,c.sunriseMinutes,c.daylightMinutes);if(hours<=prior)throw new Exception("Clock stopped or reversed at "+t);prior=hours;}
 foreach(double boundary in new[]{120d,408d,552d,840d,1020d,2820d}){double before=TenkokuForestSchedule.EvaluateSkyHours(boundary-.001,c.sunriseStartHour,c.sunsetMinutes,c.nightMinutes,c.sunriseMinutes,c.daylightMinutes);double after=TenkokuForestSchedule.EvaluateSkyHours(boundary+.001,c.sunriseStartHour,c.sunsetMinutes,c.nightMinutes,c.sunriseMinutes,c.daylightMinutes);if(after-before>.001)throw new Exception("Discontinuous clock at "+boundary);}
 report.AppendLine("PASS clock is continuous and advances monotonically across two full loops.");File.WriteAllText("AgentScripts/full-day-cycle-math-check.txt",report.ToString());result.Log(report.ToString());
 EditorApplication.isPlaying=true;
 }
}

