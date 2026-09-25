using UnityEngine;

namespace Prism {
public sealed class PrismFeedback : MonoBehaviour {
 AudioSource source;
 AudioClip clickClip,invalidClip,completeClip;
 public bool AudioEnabled {get;private set;}
 public bool HapticsEnabled {get;private set;}

 void Awake(){
  source=gameObject.AddComponent<AudioSource>();
  source.playOnAwake=false;source.spatialBlend=0;source.volume=.48f;
  clickClip=Tone("PrisM click",720,920,.045f,.16f);
  invalidClip=Tone("PrisM invalid",170,130,.09f,.18f);
  completeClip=CompletionTone();
  AudioEnabled=PlayerPrefs.GetInt("prism.audio",1)!=0;
  HapticsEnabled=PlayerPrefs.GetInt("prism.haptics",1)!=0;
 }

 public void SetAudio(bool enabled){AudioEnabled=enabled;PlayerPrefs.SetInt("prism.audio",enabled?1:0);PlayerPrefs.Save();}
 public void SetHaptics(bool enabled){HapticsEnabled=enabled;PlayerPrefs.SetInt("prism.haptics",enabled?1:0);PlayerPrefs.Save();}

 public void Click(){if(AudioEnabled)source.PlayOneShot(clickClip,.7f);}
 public void Invalid(){if(AudioEnabled)source.PlayOneShot(invalidClip,.72f);Pulse();}
 public void Complete(){if(AudioEnabled)source.PlayOneShot(completeClip,.9f);Pulse();}

 void Pulse(){
  if(!HapticsEnabled)return;
#if UNITY_ANDROID || UNITY_IOS
  Handheld.Vibrate();
#endif
 }

 static AudioClip Tone(string name,float startHz,float endHz,float seconds,float amplitude){
  const int sampleRate=22050;
  int samples=Mathf.Max(1,Mathf.CeilToInt(seconds*sampleRate));
  var data=new float[samples];
  double phase=0;
  for(int i=0;i<samples;i++){
   float t=i/(float)Mathf.Max(1,samples-1);
   float hz=Mathf.Lerp(startHz,endHz,t);
   phase+=2.0*Mathf.PI*hz/sampleRate;
   float envelope=Mathf.Sin(Mathf.PI*t);
   data[i]=(float)Math.Sin(phase)*envelope*amplitude;
  }
  var clip=AudioClip.Create(name,samples,1,sampleRate,false);
  clip.SetData(data,0);
  return clip;
 }

 static AudioClip CompletionTone(){
  const int sampleRate=22050;
  const float seconds=.42f;
  int samples=Mathf.CeilToInt(seconds*sampleRate);
  var data=new float[samples];
  float[] hz={523.25f,659.25f,783.99f};
  for(int i=0;i<samples;i++){
   float t=i/(float)sampleRate;
   float value=0;
   for(int n=0;n<hz.Length;n++){
    float local=t-n*.09f;
    if(local<0||local>.24f)continue;
    float env=Mathf.Sin(Mathf.PI*local/.24f);
    value+=Mathf.Sin(2*Mathf.PI*hz[n]*local)*env*.075f;
   }
   data[i]=value;
  }
  var clip=AudioClip.Create("PrisM complete",samples,1,sampleRate,false);
  clip.SetData(data,0);
  return clip;
 }
}
}
