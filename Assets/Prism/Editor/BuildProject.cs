using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
namespace Prism.Editor {
public static class BuildProject {
 public static void Build(){
  const string scenePath="Assets/Scenes/Prism.unity";
  if(!File.Exists(scenePath))throw new FileNotFoundException("Gameplay scene is missing",scenePath);
  EditorSceneManager.OpenScene(scenePath,OpenSceneMode.Single);
  var pipeline=AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>("Assets/Settings/Mobile_RPAsset.asset");GraphicsSettings.defaultRenderPipeline=pipeline;for(int i=0;i<QualitySettings.names.Length;i++){QualitySettings.SetQualityLevel(i);QualitySettings.renderPipeline=pipeline;}QualitySettings.vSyncCount=0;
  EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(scenePath,true)};
  PlayerSettings.companyName="Prism Studio";PlayerSettings.productName="PrisM — Işık Atölyesi";PlayerSettings.defaultScreenWidth=810;PlayerSettings.defaultScreenHeight=1206;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.resizableWindow=true;PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;PlayerSettings.runInBackground=true;
  const string applicationId="com.prismstudio.lightworkshop";PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android,applicationId);PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.iOS,applicationId);PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
  AssetDatabase.SaveAssets();Directory.CreateDirectory("Builds/Windows");
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{scenePath},locationPathName="Builds/Windows/PrisM.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
  if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Build failed: "+report.summary.result);
 }
}
}
