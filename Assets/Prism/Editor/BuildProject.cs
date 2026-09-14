using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
namespace Prism.Editor {
public static class BuildProject {
 public static void Build(){
  var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  var camera=new GameObject("Camera").AddComponent<Camera>();camera.tag="MainCamera";camera.orthographic=true;camera.transform.position=new Vector3(0,0,-10);
  new GameObject("PrisM").AddComponent<PrismGame>();
  var pipeline=AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>("Assets/Settings/Mobile_RPAsset.asset");GraphicsSettings.defaultRenderPipeline=pipeline;for(int i=0;i<QualitySettings.names.Length;i++){QualitySettings.SetQualityLevel(i);QualitySettings.renderPipeline=pipeline;}QualitySettings.vSyncCount=0;
  Directory.CreateDirectory("Assets/Scenes");EditorSceneManager.SaveScene(scene,"Assets/Scenes/Prism.unity");EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/Prism.unity",true)};
  PlayerSettings.companyName="Prism Studio";PlayerSettings.productName="PrisM — Işık Atölyesi";PlayerSettings.defaultScreenWidth=810;PlayerSettings.defaultScreenHeight=1206;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.resizableWindow=true;PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;PlayerSettings.runInBackground=true;
  const string applicationId="com.prismstudio.lightworkshop";PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android,applicationId);PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.iOS,applicationId);PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
  AssetDatabase.SaveAssets();Directory.CreateDirectory("Builds/Windows");
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/Prism.unity"},locationPathName="Builds/Windows/PrisM.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
  if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Build failed: "+report.summary.result);
 }
}
}
