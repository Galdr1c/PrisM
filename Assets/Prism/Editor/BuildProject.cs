using System;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;

namespace Prism.Editor {
public static class BuildProject {
 const string ScenePath="Assets/Scenes/Prism.unity";
 const string ApplicationId="com.prismstudio.lightworkshop";
 const string DefaultVersion="0.9.0";
 const int DefaultVersionCode=90;

 public static void Build(){BuildWindowsDevelopment();}

 public static void BuildWindowsDevelopment(){
  if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.StandaloneWindows64)throw new Exception("Windows build must launch Unity with -buildTarget win64.");
  ConfigureCommon();
  Directory.CreateDirectory("Builds/Windows");
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
   scenes=new[]{ScenePath},
   locationPathName="Builds/Windows/PrisM.exe",
   target=BuildTarget.StandaloneWindows64,
   options=BuildOptions.Development
  });
  EnsureSuccess(report,"Windows development");
 }

 public static void BuildAndroidRelease(){
  ConfigureCommon();
  ValidateContent();

  string version=Environment.GetEnvironmentVariable("PRISM_VERSION_NAME");
  if(string.IsNullOrWhiteSpace(version))version=DefaultVersion;
  int versionCode=DefaultVersionCode;
  string code=Environment.GetEnvironmentVariable("PRISM_VERSION_CODE");
  if(!string.IsNullOrWhiteSpace(code)&&!int.TryParse(code,out versionCode))throw new Exception("PRISM_VERSION_CODE must be an integer.");

  if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.Android)throw new Exception("Android release must launch Unity with -buildTarget android.");

  PlayerSettings.bundleVersion=version;
  PlayerSettings.Android.bundleVersionCode=versionCode;
  PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel26;
  PlayerSettings.Android.targetSdkVersion=AndroidSdkVersions.AndroidApiLevel36;
  PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
  PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
  PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,ApplicationId);
  PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;
  PlayerSettings.Android.optimizedFramePacing=true;
  PlayerSettings.Android.renderOutsideSafeArea=true;
  PlayerSettings.Android.requestedVisibleInsets=AndroidWindowInsetsType.None;
  PlayerSettings.Android.systemBarsBehavior=AndroidSystemBarsBehavior.ShowTransientBarsBySwipe;
  PlayerSettings.Android.fullscreenMode=FullScreenMode.FullScreenWindow;
  PlayerSettings.Android.forceInternetPermission=false;
  PlayerSettings.Android.forceSDCardPermission=false;
  PlayerSettings.Android.predictiveBackSupport=true;
  PlayerSettings.Android.appCategory="game";
  PlayerSettings.Android.androidTVCompatibility=false;
  PlayerSettings.Android.gamepadSupportLevel=AndroidGamepadSupportLevel.None;
  PlayerSettings.Android.resizeableActivity=true;
  PlayerSettings.use32BitDisplayBuffer=true;
  PlayerSettings.stripEngineCode=true;
  EditorUserBuildSettings.buildAppBundle=true;
  BrandAssets.PrepareAndroidBranding();

  string keystore=RequireEnvironment("PRISM_KEYSTORE_PATH");
  string keystorePass=RequireEnvironment("PRISM_KEYSTORE_PASS");
  string alias=RequireEnvironment("PRISM_KEY_ALIAS");
  string aliasPass=RequireEnvironment("PRISM_KEY_ALIAS_PASS");
  keystore=Path.GetFullPath(keystore);
  if(!File.Exists(keystore))throw new FileNotFoundException("Android release keystore was not found.",keystore);

  string outputDir="Builds/Android";
  Directory.CreateDirectory(outputDir);
  string output=Path.Combine(outputDir,"PrisM-"+version+"-"+versionCode+".aab");

  try{
   PlayerSettings.Android.useCustomKeystore=true;
   PlayerSettings.Android.keystoreName=keystore;
   PlayerSettings.Android.keystorePass=keystorePass;
   PlayerSettings.Android.keyaliasName=alias;
   PlayerSettings.Android.keyaliasPass=aliasPass;

   var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
    scenes=new[]{ScenePath},
    locationPathName=output,
    target=BuildTarget.Android,
    targetGroup=BuildTargetGroup.Android,
    options=BuildOptions.None
   });
   EnsureSuccess(report,"Android release");
   Debug.Log("PrisM Android App Bundle ready: "+Path.GetFullPath(output));
  }finally{
   PlayerSettings.Android.keystorePass="";
   PlayerSettings.Android.keyaliasPass="";
  }
 }

 static void ConfigureCommon(){
  if(!File.Exists(ScenePath))throw new FileNotFoundException("Gameplay scene is missing",ScenePath);
  EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);

  var pipeline=AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>("Assets/Settings/Mobile_RPAsset.asset");
  if(pipeline==null)throw new Exception("Mobile URP pipeline asset is missing.");
  GraphicsSettings.defaultRenderPipeline=pipeline;
  int qualityIndex=EditorUserBuildSettings.activeBuildTarget==BuildTarget.Android?0:Mathf.Min(1,QualitySettings.names.Length-1);
  QualitySettings.SetQualityLevel(qualityIndex,true);
  QualitySettings.renderPipeline=pipeline;
  QualitySettings.vSyncCount=0;

  EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
  PlayerSettings.companyName="Prism Studio";
  PlayerSettings.productName="PrisM — Işık Atölyesi";
  PlayerSettings.defaultScreenWidth=810;
  PlayerSettings.defaultScreenHeight=1206;
  PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
  PlayerSettings.resizableWindow=true;
  PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;
  PlayerSettings.runInBackground=false;
  PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,ApplicationId);
  PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS,ApplicationId);
  AssetDatabase.SaveAssets();
 }

 static void ValidateContent(){
  var levels=Levels.Create();
  if(levels.Length!=100)throw new Exception("Release requires exactly 100 levels; found "+levels.Length+".");
  var ids=new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);
  for(int i=0;i<levels.Length;i++){
   var level=levels[i];
   if(!ids.Add(level.Id))throw new Exception("Duplicate level ID: "+level.Id);
   var session=new Session(level);
   session.Reveal();
   var solved=Optics.Solve(level,session.Pieces);
   if(!session.IsComplete(solved))throw new Exception("Known solution fails release validation: "+level.Id);
   if(solved.Truncated)throw new Exception("Solver budget exceeded in release validation: "+level.Id);
  }
 }

 static string RequireEnvironment(string name){
  string value=Environment.GetEnvironmentVariable(name);
  if(string.IsNullOrWhiteSpace(value))throw new Exception(name+" is required for signed Android release builds.");
  return value;
 }

 static void EnsureSuccess(BuildReport report,string label){
  if(report.summary.result!=BuildResult.Succeeded)throw new Exception(label+" build failed: "+report.summary.result);
 }
}
}
