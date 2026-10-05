using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Prism {
public enum VisualQualityTier { Auto=0,Low=1,Medium=2,High=3 }

public static class VisualEnvironment {
 static GameObject root;
 static Bloom bloom;
 static ColorAdjustments color;
 static VisualQualityTier requested=VisualQualityTier.Auto;
 static VisualQualityTier resolved=VisualQualityTier.Medium;
 static bool reducedMotion,highContrast,colorSymbols,precisionMode,paused;
 static float beamScale=1f,bloomScale=1f,motionTime;
 static bool motionHooked;
 static int lastMotionFrame=-1;

 public static VisualQualityTier Requested=>requested;
 public static VisualQualityTier Resolved=>resolved;
 public static bool ReducedMotion=>reducedMotion;
 public static bool HighContrast=>highContrast;
 public static bool ColorSymbols=>colorSymbols;
 public static bool PrecisionMode=>precisionMode;
 public static float BeamScale=>beamScale;
 public static float BloomScale=>bloomScale;
 public static bool Paused=>paused;
 public static int CircleSegments=>resolved==VisualQualityTier.Low?20:resolved==VisualQualityTier.High?40:28;
 public static string QualityLabel{
  get{
   switch(requested){
    case VisualQualityTier.Low:return "Düşük";
    case VisualQualityTier.Medium:return "Orta";
    case VisualQualityTier.High:return "Yüksek";
    default:return "Otomatik ("+ResolvedLabel(resolved)+")";
   }
  }
 }

 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
 static void Bootstrap(){
  requested=(VisualQualityTier)Mathf.Clamp(PlayerPrefs.GetInt("prism.quality",0),0,3);
  reducedMotion=PlayerPrefs.GetInt("prism.reducedMotion",0)!=0;
  highContrast=PlayerPrefs.GetInt("prism.highContrast",0)!=0;
  colorSymbols=PlayerPrefs.GetInt("prism.colorSymbols",0)!=0;
  precisionMode=PlayerPrefs.GetInt("prism.precisionMode",0)!=0;
  beamScale=Mathf.Clamp(PlayerPrefs.GetFloat("prism.beamScale",1f),.5f,1.5f);
  bloomScale=Mathf.Clamp(PlayerPrefs.GetFloat("prism.bloomScale",1f),0f,1.5f);
  Configure(Camera.main);
 }

 public static void Configure(Camera camera){
  if(camera==null)return;
  camera.allowHDR=true;
  var data=camera.GetComponent<UniversalAdditionalCameraData>();
  if(data==null)data=camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
  data.renderPostProcessing=true;
  data.renderShadows=false;
  data.dithering=true;

  if(root==null){
   root=new GameObject("Prism Visual Environment");
   var volume=root.AddComponent<Volume>();volume.isGlobal=true;volume.priority=50f;
   var profile=ScriptableObject.CreateInstance<VolumeProfile>();profile.name="Prism Runtime Volume";volume.sharedProfile=profile;
   bloom=profile.Add<Bloom>(true);
   var tone=profile.Add<Tonemapping>(true);tone.mode.Override(TonemappingMode.ACES);
   color=profile.Add<ColorAdjustments>(true);
   Object.DontDestroyOnLoad(root);
  }
  if(!motionHooked){RenderPipelineManager.beginCameraRendering+=TickMotion;motionHooked=true;}
  Apply();
 }

 static void TickMotion(ScriptableRenderContext context,Camera camera){
  if(lastMotionFrame!=Time.frameCount){
   lastMotionFrame=Time.frameCount;
   if(!paused&&!reducedMotion)motionTime+=Time.unscaledDeltaTime;
  }
  Shader.SetGlobalFloat("_PrismMotionTime",motionTime);
 }

 public static void SetQuality(VisualQualityTier tier){
  requested=tier;
  PlayerPrefs.SetInt("prism.quality",(int)tier);
  PlayerPrefs.Save();
  Apply();
 }

 public static VisualQualityTier NextQuality(){
  var next=(VisualQualityTier)(((int)requested+1)%4);
  SetQuality(next);
  return next;
 }

 public static void SetCelebration(float amount){
  if(bloom==null)return;
  amount=Mathf.Clamp01(amount);
  float baseIntensity=resolved==VisualQualityTier.Low ? .30f : resolved==VisualQualityTier.High ? .52f : .42f;
  float lift=resolved==VisualQualityTier.Low ? .12f : resolved==VisualQualityTier.High ? .22f : .18f;
  bloom.intensity.Override((baseIntensity+(reducedMotion?0f:amount*lift))*bloomScale);
 }

 public static void SetReducedMotion(bool value){reducedMotion=value;PlayerPrefs.SetInt("prism.reducedMotion",value?1:0);PlayerPrefs.Save();}
 public static void SetHighContrast(bool value){highContrast=value;PlayerPrefs.SetInt("prism.highContrast",value?1:0);PlayerPrefs.Save();Apply();}
 public static void SetColorSymbols(bool value){colorSymbols=value;PlayerPrefs.SetInt("prism.colorSymbols",value?1:0);PlayerPrefs.Save();}
 public static void SetPrecisionMode(bool value){precisionMode=value;PlayerPrefs.SetInt("prism.precisionMode",value?1:0);PlayerPrefs.Save();}
 public static void SetBeamScale(float value){beamScale=Mathf.Clamp(value,.5f,1.5f);PlayerPrefs.SetFloat("prism.beamScale",beamScale);PlayerPrefs.Save();}
 public static void SetBloomScale(float value){bloomScale=Mathf.Clamp(value,0f,1.5f);PlayerPrefs.SetFloat("prism.bloomScale",bloomScale);PlayerPrefs.Save();Apply();}
 public static void SetPaused(bool value){paused=value;}

 static void Apply(){
  resolved=Resolve(requested);
  Application.targetFrameRate=60;
  QualitySettings.vSyncCount=0;
  QualitySettings.antiAliasing=resolved==VisualQualityTier.High?4:resolved==VisualQualityTier.Medium?2:0;

  if(bloom!=null){
   bloom.threshold.Override(resolved==VisualQualityTier.Low?1.05f:.95f);
   bloom.intensity.Override((resolved==VisualQualityTier.Low?.30f:resolved==VisualQualityTier.High?.52f:.42f)*bloomScale);
   bloom.scatter.Override(resolved==VisualQualityTier.Low?.44f:resolved==VisualQualityTier.High?.58f:.52f);
   bloom.clamp.Override(resolved==VisualQualityTier.Low?3.5f:5f);
   bloom.highQualityFiltering.Override(resolved==VisualQualityTier.High);
  }
  if(color!=null){
   color.postExposure.Override(resolved==VisualQualityTier.Low?-.14f:-.1f);
   color.contrast.Override(highContrast?15f:resolved==VisualQualityTier.High?9f:7f);
   color.saturation.Override(resolved==VisualQualityTier.Low?5f:7f);
  }
 }

 static VisualQualityTier Resolve(VisualQualityTier tier){
  if(tier!=VisualQualityTier.Auto)return tier;
  if(!Application.isMobilePlatform)return VisualQualityTier.High;
  int memory=SystemInfo.systemMemorySize;
  int graphics=SystemInfo.graphicsMemorySize;
  if((memory>0&&memory<3500)||(graphics>0&&graphics<1000))return VisualQualityTier.Low;
  if(memory>=6000&&graphics>=2000)return VisualQualityTier.High;
  return VisualQualityTier.Medium;
 }

 static string ResolvedLabel(VisualQualityTier tier){
  switch(tier){
   case VisualQualityTier.Low:return "Düşük";
   case VisualQualityTier.High:return "Yüksek";
   default:return "Orta";
  }
 }
}
}
