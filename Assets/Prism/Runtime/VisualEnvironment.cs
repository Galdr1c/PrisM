using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Prism {
public static class VisualEnvironment {
 static GameObject root;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
 static void Bootstrap(){Configure(Camera.main);}
 public static void Configure(Camera camera){
  if(camera==null)return;
  camera.allowHDR=true;
  var data=camera.GetComponent<UniversalAdditionalCameraData>();
  if(data==null)data=camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
  data.renderPostProcessing=true;
  data.renderShadows=false;
  data.dithering=true;
  if(root!=null)return;
  root=new GameObject("Prism Visual Environment");
  var volume=root.AddComponent<Volume>();volume.isGlobal=true;volume.priority=50f;
  var profile=ScriptableObject.CreateInstance<VolumeProfile>();profile.name="Prism Runtime Volume";volume.sharedProfile=profile;
  var bloom=profile.Add<Bloom>(true);bloom.threshold.Override(0.82f);bloom.intensity.Override(0.72f);bloom.scatter.Override(0.68f);bloom.clamp.Override(8f);bloom.highQualityFiltering.Override(false);
  var tone=profile.Add<Tonemapping>(true);tone.mode.Override(TonemappingMode.ACES);
  var color=profile.Add<ColorAdjustments>(true);color.postExposure.Override(-0.1f);color.contrast.Override(8f);color.saturation.Override(7f);
  Object.DontDestroyOnLoad(root);
 }
}
}
