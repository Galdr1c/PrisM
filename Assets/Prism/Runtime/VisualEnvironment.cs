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

 public static VisualQualityTier Requested=>requested;
 public static VisualQualityTier Resolved=>resolved;
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
  Apply();
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

 static void Apply(){
  resolved=Resolve(requested);
  Application.targetFrameRate=60;
  QualitySettings.vSyncCount=0;
  QualitySettings.antiAliasing=resolved==VisualQualityTier.High?4:resolved==VisualQualityTier.Medium?2:0;

  if(bloom!=null){
   bloom.threshold.Override(resolved==VisualQualityTier.Low?1.0f:.82f);
   bloom.intensity.Override(resolved==VisualQualityTier.Low?.48f:resolved==VisualQualityTier.High?.9f:.7f);
   bloom.scatter.Override(resolved==VisualQualityTier.Low?.52f:resolved==VisualQualityTier.High?.72f:.64f);
   bloom.clamp.Override(resolved==VisualQualityTier.Low?5f:8f);
   bloom.highQualityFiltering.Override(resolved==VisualQualityTier.High);
  }
  if(color!=null){
   color.postExposure.Override(resolved==VisualQualityTier.Low?-.14f:-.1f);
   color.contrast.Override(resolved==VisualQualityTier.High?9f:7f);
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
