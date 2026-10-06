using UnityEngine;

namespace Prism {
public sealed class PrismFeedback : MonoBehaviour {
 AudioSource source,music;
 AudioClip clickClip,placeClip,rotateClip,invalidClip,goalClip,completeClip,milestoneClip;
 float lastClickTime=-1f;
 float lastRotateTime=-1f;
 float lastGoalTime=-1f;
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
 float lastHapticTime=-10f;
#endif
 bool appPaused,appFocused=true,musicStarted;
 public bool AudioEnabled {get;private set;}
 public bool MusicEnabled {get;private set;}
 public bool HapticsEnabled {get;private set;}

 void Awake(){
  source=gameObject.AddComponent<AudioSource>();
  source.playOnAwake=false;
  source.spatialBlend=0f;
  source.volume=.65f;
  source.priority=64;
  clickClip=LoadClip("UI");
  placeClip=LoadClip("Place");
  rotateClip=LoadClip("Rotate");
  invalidClip=LoadClip("Invalid");
  goalClip=LoadClip("Goal");
  completeClip=LoadClip("Complete");
  milestoneClip=MilestoneTone();
  music=gameObject.AddComponent<AudioSource>();
  music.playOnAwake=false;
  music.spatialBlend=0f;
  music.loop=true;
  music.priority=192;
  music.volume=0f;
  music.clip=LoadClip("OpticalLaboratory");
  AudioEnabled=PlayerPrefs.GetInt("prism.audio",1)!=0;
  // Preserve the prior mute preference when introducing an independent music control.
  MusicEnabled=PlayerPrefs.GetInt("prism.music",AudioEnabled?1:0)!=0;
  HapticsEnabled=PlayerPrefs.GetInt("prism.haptics",1)!=0;
  RefreshMusic();
 }

 void Update(){
  if(music!=null&&MusicEnabled&&!appPaused&&appFocused)
   music.volume=Mathf.MoveTowards(music.volume,.7f,Time.unscaledDeltaTime*.7f);
 }

 void OnDestroy(){if(milestoneClip!=null)Destroy(milestoneClip);}

 void OnApplicationPause(bool paused){appPaused=paused;RefreshMusic();if(paused&&source!=null)source.Stop();}
 void OnApplicationFocus(bool focused){appFocused=focused;RefreshMusic();if(!focused&&source!=null)source.Stop();}

 void RefreshMusic(){
  if(music==null||music.clip==null)return;
  if(!MusicEnabled||appPaused||!appFocused){music.Pause();music.volume=0f;return;}
  if(musicStarted)music.UnPause();
  else {music.Play();musicStarted=true;}
 }
 public void SetMusic(bool enabled){
  MusicEnabled=enabled;
  RefreshMusic();
  PlayerPrefs.SetInt("prism.music",enabled?1:0);
  PlayerPrefs.Save();
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
  if(!CanPlay())return;
  if(Time.unscaledTime-lastClickTime<.035f)return;
  lastClickTime=Time.unscaledTime;
  Play(clickClip);
 }
 public void Place(){Place(Kind.Prism);}
 public void Place(Kind kind){Play(placeClip,kind==Kind.Mirror ? 1.25f : kind==Kind.Sphere ? .82f : kind==Kind.Lens ? .94f : 1f);Pulse(new long[]{0,12},new int[]{0,30});}
 public void Rotate(){
  if(!CanPlay()||Time.unscaledTime-lastRotateTime<.09f)return;
  lastRotateTime=Time.unscaledTime;
  Play(rotateClip);
 }
 public void Goal(){
  if(Time.unscaledTime-lastGoalTime<.12f)return;
  lastGoalTime=Time.unscaledTime;
  Play(goalClip);
  Pulse(new long[]{0,8},new int[]{0,22});
 }
 public void Invalid(){Play(invalidClip);Pulse(new long[]{0,10,45,10},new int[]{0,25,0,25});}
 public void Complete(bool milestone=false){
  Play(milestone?milestoneClip:completeClip);
  Pulse(milestone?new long[]{0,18,60,18,85,45}:new long[]{0,14,55,14,75,28},new int[]{0,40,0,40,0,milestone?90:65});
#if UNITY_IOS && !UNITY_EDITOR
  if(HapticsEnabled&&!appPaused&&appFocused&&Time.unscaledTime-lastHapticTime>=1f){
   lastHapticTime=Time.unscaledTime;
   Handheld.Vibrate();
  }
#endif
 }

 void Pulse(long[] timings,int[] amplitudes){
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
  if(!HapticsEnabled||appPaused||!appFocused||Time.unscaledTime-lastHapticTime<.18f)return;
  lastHapticTime=Time.unscaledTime;
#if UNITY_ANDROID
  try{
   using(var player=new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
   using(var activity=player.GetStatic<AndroidJavaObject>("currentActivity"))
   using(var vibrator=activity.Call<AndroidJavaObject>("getSystemService","vibrator")){
    if(!vibrator.Call<bool>("hasVibrator"))return;
    using(var effects=new AndroidJavaClass("android.os.VibrationEffect"))
    using(var effect=effects.CallStatic<AndroidJavaObject>("createWaveform",timings,amplitudes,-1))vibrator.Call("vibrate",effect);
   }
  }catch(System.Exception){/* Unsupported hardware preserves its audio and visual feedback. */}
#else
  Handheld.Vibrate();
#endif
#endif
 }

 bool CanPlay()=>AudioEnabled&&source!=null&&!appPaused&&appFocused;
 void Play(AudioClip clip,float pitch=1f){if(CanPlay()&&clip!=null){source.pitch=pitch;source.PlayOneShot(clip);}}
 static AudioClip LoadClip(string name){
  var clip=Resources.Load<AudioClip>("Audio/"+name);
  if(clip==null)Debug.LogWarning("PrisM audio asset missing: "+name);
  return clip;
 }

 // Keep the incoming milestone's richer five-note resolve distinct from normal wins.
 // This is the only generated clip; the loaded WAV assets remain Resources-owned.
 static AudioClip MilestoneTone(){
  const int sampleRate=22050;
  const float seconds=.96f,noteSeconds=.46f,step=.095f;
  float[] notes={392f,523.25f,659.25f,783.99f,1046.5f};
  var data=new float[Mathf.CeilToInt(seconds*sampleRate)];
  for(int i=0;i<data.Length;i++){
   float time=i/(float)sampleRate,value=0f;
   for(int n=0;n<notes.Length;n++){
    float local=time-n*step;
    if(local<0f||local>noteSeconds)continue;
    float attack=Mathf.Min(1f,local/.007f),release=1f-local/noteSeconds;
    float phase=2f*Mathf.PI*notes[n]*local;
    float shimmer=Mathf.Sin(phase*2.01f)*.16f+Mathf.Sin(phase*3.98f)*.055f;
    value+=(Mathf.Sin(phase)+shimmer)*attack*release*release*.115f;
   }
   data[i]=Mathf.Clamp(value,-.92f,.92f);
  }
  var clip=AudioClip.Create("PrisM milestone",data.Length,1,sampleRate,false);
  clip.SetData(data,0);
  return clip;
 }
}
}
