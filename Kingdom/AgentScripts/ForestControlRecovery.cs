using System;
using System.IO;
using System.Linq;
using System.Text;
using IdyllicFantasyNature;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Object=UnityEngine.Object;

[InitializeOnLoad]
public static class ForestControlRecovery
{
    const string Backup="SetupBackup/ForestControlReset";
    static ForestControlRecovery() { EditorApplication.update+=Tick; }
    static T[] All<T>() where T:Component => Resources.FindObjectsOfTypeAll<T>().Where(t=>t.gameObject.scene==SceneManager.GetActiveScene()).ToArray();

    [MenuItem("Tools/GlowKeeper/Restore Forest Controls")]
    static void Restore()
    {
        try
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new Exception("Leave Play mode before restoring scene controls.");
            var scene=SceneManager.GetActiveScene();
            if(scene.path!="Assets/Scenes/GlowKeeper Forest.unity")throw new Exception("Open GlowKeeper Forest first.");
            Directory.CreateDirectory(Backup);
            if(!File.Exists(Backup+"/Forest-live-before-reset.unity"))EditorSceneManager.SaveScene(scene,Backup+"/Forest-live-before-reset.unity",true);
            var controls=scene.GetRootGameObjects().Single(go=>go.name=="Controls");
            var player=controls.transform.Find("Controller");
            if(!player)throw new Exception("Original forest Controller is missing.");
            var camera=All<Camera>().Single(c=>c.CompareTag("MainCamera"));
            int removed=0,missing=0;
            var wizardObjects=All<Transform>().Where(t=>t.name.IndexOf("wizard",StringComparison.OrdinalIgnoreCase)>=0||AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(t.gameObject)).StartsWith("Assets/WizardPBR/")).Select(t=>t.gameObject).ToArray();
            foreach(var go in wizardObjects)
            {
                if(!go||go==controls||go==player.gameObject||go==camera.gameObject)continue;
                Undo.DestroyObjectImmediate(go);removed++;
            }
            foreach(var t in controls.GetComponentsInChildren<Transform>(true))
            {
                missing+=GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
                foreach(var behaviour in t.GetComponents<MonoBehaviour>().Where(b=>b&&b.GetType().Name.IndexOf("Wizard",StringComparison.OrdinalIgnoreCase)>=0))Undo.DestroyObjectImmediate(behaviour);
            }
            var movement=player.GetComponent<PlayerMovement>();
            if(!movement)movement=Undo.AddComponent<PlayerMovement>(player.gameObject);
            var capsule=player.GetComponent<CharacterController>();
            if(!capsule)capsule=Undo.AddComponent<CharacterController>(player.gameObject);
            Undo.RecordObjects(new Object[]{player,camera.transform,camera,movement,capsule},"Restore first person forest controls");
            player.gameObject.SetActive(true);movement.enabled=true;capsule.enabled=true;
            player.SetPositionAndRotation(new Vector3(169.991f,19.93849f,166.09459f),Quaternion.Euler(0,275.869f,0));
            player.localScale=Vector3.one;
            capsule.height=1.9f;capsule.radius=.5f;capsule.center=Vector3.zero;capsule.stepOffset=.3f;capsule.slopeLimit=45;
            var movementData=new SerializedObject(movement);
            movementData.FindProperty("_movementSpeed").floatValue=6;
            movementData.FindProperty("_runMultiplier").floatValue=1.8f;
            movementData.FindProperty("_gravity").floatValue=-9.81f;
            movementData.FindProperty("_jumpHeight").floatValue=1;
            movementData.ApplyModifiedProperties();
            Undo.SetTransformParent(camera.transform,player,"Restore first person camera parent");
            camera.transform.localPosition=new Vector3(0,.7f,0);camera.transform.localRotation=Quaternion.identity;camera.transform.localScale=Vector3.one;
            camera.gameObject.SetActive(true);camera.enabled=true;
            var look=camera.GetComponent<CameraMovement>();
            if(!look)look=Undo.AddComponent<CameraMovement>(camera.gameObject);
            Undo.RecordObject(look,"Restore mouse look");look.enabled=true;
            var lookData=new SerializedObject(look);lookData.FindProperty("_controller").objectReferenceValue=player;lookData.FindProperty("_mouseSensity").floatValue=1;lookData.ApplyModifiedProperties();
            int bends=0;
            foreach(var bend in All<VegetationBendControl>())
            {
                Undo.RecordObject(bend,"Reconnect vegetation bend origin");
                var data=new SerializedObject(bend);data.FindProperty("_bendOrigin").objectReferenceValue=player;data.ApplyModifiedProperties();bends++;
            }
            foreach(var component in All<GlowKeeper.TenkokuForestSchedule>())
            {component.sky.manualCamera=camera.transform;component.sky.mainCamera=camera.transform;EditorUtility.SetDirty(component.sky);}
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Selection.activeGameObject=player.gameObject;
            var view=SceneView.lastActiveSceneView;if(view)view.LookAt(player.position+Vector3.up,Quaternion.Euler(22,245,0),12);
            File.WriteAllText(Backup+"/restoration.txt",$"Removed scene wizard objects: {removed}\nRemoved missing control scripts: {missing}\nReconnected vegetation components: {bends}\n"+Describe());
            Debug.Log("Forest controls restored. Wizard scene setup removed; imported models and textures retained.");
        }
        catch(Exception e){File.WriteAllText(Backup+"/restoration-error.txt",e.ToString());Debug.LogException(e);}
    }

    static string Describe()
    {
        var player=All<PlayerMovement>().Single();var camera=Camera.main;var look=camera.GetComponent<CameraMovement>();
        var s=new StringBuilder();s.AppendLine($"Movement enabled={player.enabled}; capsule enabled={player.GetComponent<CharacterController>().enabled}; mouse look enabled={look.enabled}");
        s.AppendLine($"Camera parent={camera.transform.parent.name}; camera local={camera.transform.localPosition}; player={player.transform.position}");
        s.AppendLine("Unassigned bend origins="+All<VegetationBendControl>().Count(b=>!new SerializedObject(b).FindProperty("_bendOrigin").objectReferenceValue));
        s.AppendLine("Wizard-named scene objects="+All<Transform>().Count(t=>t.name.IndexOf("wizard",StringComparison.OrdinalIgnoreCase)>=0));
        s.AppendLine("Missing scene scripts="+All<Transform>().Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)));
        return s.ToString();
    }

    [MenuItem("Tools/GlowKeeper/Check Restored Controls")]
    static void StartCheck(){SessionState.SetBool("ForestRecovery.Check",true);EditorApplication.isPlaying=true;}
    static int stage=-1,frames;
    static Keyboard testKeyboard;
    static bool previousBackground;
    static Vector3 start;
    static double deadline;
    static StringBuilder report;
    static readonly Key[] Keys={Key.W,Key.S,Key.A,Key.D,Key.Space};
    static void Tick()
    {
        if(!SessionState.GetBool("ForestRecovery.Check",false)||!EditorApplication.isPlaying)return;
        try
        {
            EditorApplication.QueuePlayerLoopUpdate();
            var player=All<PlayerMovement>().Single();
            if(stage<0)
            {
                previousBackground=Application.runInBackground;Application.runInBackground=true;
                testKeyboard=InputSystem.AddDevice<Keyboard>();report=new StringBuilder(Describe());
                frames=0;stage=0;deadline=EditorApplication.timeSinceStartup+.6;return;
            }
            if(++frames<3||EditorApplication.timeSinceStartup<deadline)return;
            frames=0;
            if(stage%2==0&&stage/2<Keys.Length)
            {
                start=player.transform.position;
                InputSystem.QueueStateEvent(testKeyboard,new KeyboardState(Keys[stage/2]));
                deadline=EditorApplication.timeSinceStartup+.15;stage++;return;
            }
            if(stage%2==1)
            {
                var delta=player.transform.position-start;
                var key=Keys[stage/2];
                float amount=key==Key.Space?delta.y:new Vector2(delta.x,delta.z).magnitude;
                report.AppendLine(key+" displacement="+delta+" passed="+(amount>.05f));
                InputSystem.QueueStateEvent(testKeyboard,new KeyboardState());
                deadline=EditorApplication.timeSinceStartup+.25;stage++;return;
            }
            File.WriteAllText(Backup+"/play-check.txt",report.ToString());
            Cleanup();EditorApplication.isPlaying=false;
        }
        catch(Exception e){File.WriteAllText(Backup+"/play-check-error.txt",e.ToString());Cleanup();EditorApplication.isPlaying=false;}
    }
    static void Cleanup()
    {
        if(testKeyboard!=null&&testKeyboard.added)InputSystem.RemoveDevice(testKeyboard);
        testKeyboard=null;Application.runInBackground=previousBackground;stage=-1;SessionState.SetBool("ForestRecovery.Check",false);
    }
}
