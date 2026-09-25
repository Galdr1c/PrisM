using UnityEngine;

namespace Prism {
public sealed class PrismFeedback : MonoBehaviour {
 const int SampleRate=22050;
 AudioSource source;
 AudioClip clickClip,invalidClip,completeClip;
 float lastClickTime=-1f;
 float lastHapticTime=-10f;
 public bool AudioEnabled {get;private set;}
 public bool HapticsEnabled {get;private set;}

 void Awake(){
  source=gameObject.AddComponent<AudioSource>();
  source.playOnAwake=false;
  source.spatialBlend=0f;
  source.volume=.65f;
  clickClip=GestureTone("PrisM click",760f,520f,.045f,.28f);
  invalidClip=GestureTone("PrisM invalid",280f,180f,.12f,.24f);
  completeClip=CompletionTone();
  AudioEnabled=PlayerPrefs.GetInt("prism.audio",1)!=0;
  HapticsEnabled=PlayerPrefs.GetInt("prism.haptics",1)!=0;
 }

 void OnDestroy(){
  if(clickClip!=null)Destroy(clickClip);
  if(invalidClip!=null)Destroy(invalidClip);
  if(completeClip!=null)Destroy(completeClip);
 }

 public void SetAudio(bool enabled){
  AudioEnabled=enabled;
  if(!enabled&&source!=null)source.Stop();
  PlayerPrefs.SetInt("prism.audio",enabled?1:0);
  PlayerPrefs.Save();
 }
 public void SetHaptics(bool enabled){
  HapticsEnabled=enabled;
  PlayerPrefs.SetInt("prism.haptics",enabled?1:0);
  PlayerPrefs.Save();
 }

 public void Click(){
  if(!AudioEnabled||source==null)return;
  if(Time.unscaledTime-lastClickTime<.035f)return;
  lastClickTime=Time.unscaledTime;
  source.PlayOneShot(clickClip);
 }
 public void Invalid(){if(AudioEnabled&&source!=null)source.PlayOneShot(invalidClip);}
 public void Complete(){
  if(AudioEnabled&&source!=null)source.PlayOneShot(completeClip);
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
  if(HapticsEnabled&&Time.unscaledTime-lastHapticTime>=1f){
   lastHapticTime=Time.unscaledTime;
   Handheld.Vibrate();
  }
#endif
 }

 static AudioClip GestureTone(string name,float startHz,float endHz,float seconds,float amplitude){
  int samples=Mathf.CeilToInt(seconds*SampleRate);
  var data=new float[samples];
  double phase=0;
  for(int i=0;i<samples;i++){
   float t=i/(float)(samples-1);
   float hz=Mathf.Lerp(startHz,endHz,t);
   phase+=2.0*Mathf.PI*hz/SampleRate;
   float attack=Mathf.Min(1f,t*seconds/.003f);
   float envelope=attack*(1f-t)*(1f-t);
   data[i]=(float)(Mathf.Sin((float)phase)+.22f*Mathf.Sin((float)(phase*2.01)))*envelope*amplitude;
  }
  return CreateClip(name,data);
 }

 static AudioClip CompletionTone(){
  const float seconds=.52f;
  const float noteSeconds=.32f;
  float[] notes={523.25f,659.25f,783.99f};
  int samples=Mathf.CeilToInt(seconds*SampleRate);
  var data=new float[samples];
  for(int i=0;i<samples;i++){
   float time=i/(float)SampleRate;
   float value=0f;
   for(int n=0;n<notes.Length;n++){
    float local=time-n*.09f;
    if(local<0f||local>noteSeconds)continue;
    float attack=Mathf.Min(1f,local/.008f);
    float release=1f-local/noteSeconds;
    float phase=2f*Mathf.PI*notes[n]*local;
    value+=(Mathf.Sin(phase)+.18f*Mathf.Sin(phase*2f))*attack*release*release*.14f;
   }
   data[i]=value;
  }
  return CreateClip("PrisM complete",data);
 }

 static AudioClip CreateClip(string name,float[] data){
  var clip=AudioClip.Create(name,data.Length,1,SampleRate,false);
  clip.SetData(data,0);
  return clip;
 }
}
}
