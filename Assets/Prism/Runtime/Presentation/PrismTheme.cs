using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Prism {
// Presentation resources only: technical package/namespace identity stays Prism.
public static class PrismTheme {
 public static readonly Color Background=new Color32(9,8,18,255);
 public static readonly Color Surface=new Color(20/255f,18/255f,34/255f,.92f);
 public static readonly Color Ivory=new Color32(245,242,232,255);
 public static readonly Color Muted=new Color32(173,167,189,255);
 public static readonly Color Accent=new Color32(143,213,228,255);
 public static readonly Color Success=new Color32(142,225,183,255);
 static TMP_FontAsset regular,semibold;
 static Sprite glyph;

 public static Sprite Glyph {
  get {
   if(glyph==null)glyph=Resources.Load<Sprite>("Brand/OpticalGlyph");
   return glyph;
  }
 }

 public static TMP_FontAsset Font(bool bold=false){
  if(bold){if(semibold==null)semibold=CreateFont("Sora-SemiBold");return semibold;}
  if(regular==null)regular=CreateFont("Sora-Regular");
  return regular;
 }

 static TMP_FontAsset CreateFont(string name){
  // The shader sits in Resources so Unity retains it in player builds.
  Resources.Load<Shader>("Brand/Shaders/TMP_SDF-Mobile");
  var source=Resources.Load<UnityEngine.Font>("Brand/Fonts/"+name);
  if(source==null){Debug.LogError("PRISM font resource is missing: "+name);return null;}
  var font=TMP_FontAsset.CreateFontAsset(source,64,8,GlyphRenderMode.SDFAA,1024,1024,AtlasPopulationMode.Dynamic,true);
  if(font==null)return null;
  font.name=name+" Dynamic SDF";
  if(TMP_Settings.defaultFontAsset==null)TMP_Settings.defaultFontAsset=font;
  font.TryAddCharacters("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789ÇĞİÖŞÜçğıöşü ·—×%!?.,:;()+-/",out string missing);
  if(!string.IsNullOrEmpty(missing))Debug.LogWarning("PRISM font characters unavailable: "+missing);
  return font;
 }

 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
 static void Reset(){regular=null;semibold=null;glyph=null;}
}
}
