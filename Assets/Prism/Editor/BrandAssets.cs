using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
#if UNITY_ANDROID
using UnityEditor.Android;
#endif
using UnityEngine;

namespace Prism.Editor {
public static class BrandAssets {
 const string GeneratedDir="Assets/Prism/Generated";
 const string IconPath=GeneratedDir+"/AppIcon.png";
 const string AdaptiveForegroundPath=GeneratedDir+"/AdaptiveForeground.png";
 const string AdaptiveBackgroundPath=GeneratedDir+"/AdaptiveBackground.png";
 const string MonochromePath=GeneratedDir+"/MonochromeIcon.png";
 const string GlyphPath="Assets/Prism/Resources/Brand/OpticalGlyph.png";
 const string StoreDir="Builds/StoreAssets";

 [MenuItem("PRISM/Release/Generate Store Assets")]
 public static void GenerateStoreAssets(){
  Directory.CreateDirectory(StoreDir);
  string storeIcon=Path.Combine(StoreDir,"play-icon-512.png");
  WritePng(storeIcon,CreateIcon(512));
  if(new FileInfo(storeIcon).Length>1024*1024)throw new Exception("Play icon exceeds the 1024 KB upload limit: "+storeIcon);
  WritePng(Path.Combine(StoreDir,"feature-graphic-1024x500.png"),CreateFeatureGraphic());
  WritePng(GlyphPath,CreateGlyph(512,false));
  WritePng(MonochromePath,CreateGlyph(1024,true,.64f));
  foreach(int size in new[]{32,48,64,128,192}){
   WritePng(Path.Combine(StoreDir,"icon-"+size+".png"),CreateIcon(size));
   WritePng(Path.Combine(StoreDir,"monochrome-"+size+".png"),CreateGlyph(size,true));
  }
  Debug.Log("PRISM store assets generated in "+Path.GetFullPath(StoreDir));
 }

 public static void PrepareAndroidBranding(){
  Directory.CreateDirectory(GeneratedDir);
  var icon=CreateIcon(1024);
  WritePng(IconPath,icon);
  AssetDatabase.ImportAsset(IconPath,ImportAssetOptions.ForceUpdate);

  var importer=AssetImporter.GetAtPath(IconPath) as TextureImporter;
  if(importer!=null){
   importer.textureType=TextureImporterType.Default;
   importer.textureCompression=TextureImporterCompression.Uncompressed;
   importer.mipmapEnabled=false;
   importer.alphaIsTransparency=true;
   importer.maxTextureSize=1024;
   importer.SaveAndReimport();
  }

  var loaded=AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
  if(loaded==null)throw new Exception("Generated Android icon could not be imported.");

  GenerateStoreAssets();
  var target=NamedBuildTarget.Android;
  int[] sizes=PlayerSettings.GetIconSizes(target,IconKind.Application);
  if(sizes==null||sizes.Length==0)throw new Exception("Android icon slots are unavailable. Install Android Build Support for this Unity editor.");
  var icons=new Texture2D[sizes.Length];
  for(int i=0;i<icons.Length;i++)icons[i]=loaded;
  PlayerSettings.SetIcons(target,icons,IconKind.Application);

#if UNITY_ANDROID
  WritePng(AdaptiveForegroundPath,CreateAdaptiveForeground(1024));
  WritePng(AdaptiveBackgroundPath,CreateIconBackground(1024));
  var foreground=ImportIcon(AdaptiveForegroundPath);
  var background=ImportIcon(AdaptiveBackgroundPath);
  var monochrome=ImportIcon(MonochromePath);
  ApplyPlatformIcons(target,AndroidPlatformIconKind.Adaptive,foreground,background,monochrome);
#endif

  AssetDatabase.SaveAssets();
 }

#if UNITY_ANDROID
 static Texture2D ImportIcon(string path){
  AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);
  var importer=AssetImporter.GetAtPath(path) as TextureImporter;
  if(importer==null)throw new Exception("Generated Android icon could not be imported: "+path);
  importer.textureType=TextureImporterType.Default;
  importer.textureShape=TextureImporterShape.Texture2D;
  importer.textureCompression=TextureImporterCompression.Uncompressed;
  importer.mipmapEnabled=false;
  importer.alphaIsTransparency=true;
  importer.maxTextureSize=1024;
  importer.SaveAndReimport();
  var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
  if(texture==null)throw new Exception("Generated Android icon could not be loaded: "+path);
  return texture;
 }

 static void ApplyPlatformIcons(NamedBuildTarget target,PlatformIconKind kind,Texture2D foreground,Texture2D background,Texture2D monochrome){
  var slots=PlayerSettings.GetPlatformIcons(target,kind);
  for(int i=0;i<slots.Length;i++){
   var textures=new Texture2D[slots[i].maxLayerCount];
   textures[0]=foreground;
   if(textures.Length>1)textures[1]=background;
   if(textures.Length>2)textures[2]=monochrome;
   slots[i].SetTextures(textures);
  }
  PlayerSettings.SetPlatformIcons(target,kind,slots);
 }
#endif

 static Texture2D CreateIcon(int size){
  var tex=CreateIconBackground(size);
  DrawIconArt(tex,size,1f);
  tex.Apply(false,false);
  return tex;
 }

 // All outputs share this optical fold; no triangle illustration or thin rainbow rays.
 static readonly Color32 Background=new Color32(9,8,18,255);
 static readonly Color[] Spectrum={
  new Color(.55f,.39f,1),new Color(.35f,.53f,1),new Color(.42f,.84f,.96f),
  new Color(.53f,.89f,.72f),new Color(.96f,.91f,.57f),new Color(1,.66f,.50f),new Color(.96f,.43f,.62f)
 };

 static Texture2D CreateIconBackground(int size){
  var tex=new Texture2D(size,size,TextureFormat.RGBA32,false);
  var pixels=new Color32[size*size];
  for(int i=0;i<pixels.Length;i++)pixels[i]=Background;
  tex.SetPixels32(pixels);tex.Apply(false,false);return tex;
 }

 static Texture2D CreateAdaptiveForeground(int size)=>CreateGlyph(size,false,.64f);

 static Texture2D CreateGlyph(int size,bool monochrome,float scale=1){
  var tex=new Texture2D(size,size,TextureFormat.RGBA32,false);
  tex.SetPixels32(new Color32[size*size]);
  DrawGlyph(tex,new Vector2(size*.5f,size*.5f),size*scale,monochrome);
  tex.Apply(false,false);return tex;
 }

 static void DrawIconArt(Texture2D tex,int size,float scale)=>DrawGlyph(tex,new Vector2(size*.5f,size*.5f),size*scale,false);

 static void DrawGlyph(Texture2D tex,Vector2 center,float size,bool monochrome){
  Vector2 P(float x,float y)=>center+new Vector2(x-.5f,y-.5f)*size;
  // A broad exit wedge and angular beam stay distinct even at launcher scale.
  Vector2 tip=P(.56f,.60f),top=P(.88f,.84f),bottom=P(.88f,.48f);
  int minX=Mathf.Clamp(Mathf.FloorToInt(tip.x),0,tex.width-1);
  int maxX=Mathf.Clamp(Mathf.CeilToInt(top.x),0,tex.width-1);
  int minY=Mathf.Clamp(Mathf.FloorToInt(bottom.y),0,tex.height-1);
  int maxY=Mathf.Clamp(Mathf.CeilToInt(top.y),0,tex.height-1);
  for(int y=minY;y<=maxY;y++)for(int x=minX;x<=maxX;x++){
   float t=(x+.5f-tip.x)/(top.x-tip.x);
   float lo=Mathf.Lerp(tip.y,bottom.y,t),hi=Mathf.Lerp(tip.y,top.y,t);
   float v=(y+.5f-lo)/Mathf.Max(.001f,hi-lo);
   if(t>=0&&t<=1&&v>=0&&v<=1){
    float band=v*(Spectrum.Length-1);int index=Mathf.Min(Mathf.FloorToInt(band),Spectrum.Length-2);
    Blend(tex,x,y,monochrome?Color.white:Color.Lerp(Spectrum[index],Spectrum[index+1],band-index));
   }
  }
  Color ivory=monochrome?Color.white:new Color(.96f,.95f,.91f);
  // Two corners describe optical folding without depending on color.
  Vector2[] fold={P(.12f,.36f),P(.43f,.36f),P(.32f,.62f),P(.56f,.62f),P(.56f,.54f),P(.44f,.54f),P(.55f,.28f),P(.12f,.28f)};
  FillPolygon(tex,fold,ivory);
 }

 static void FillPolygon(Texture2D tex,Vector2[] points,Color color){
  float lx=tex.width,rx=0,ly=tex.height,ry=0;
  foreach(var p in points){lx=Mathf.Min(lx,p.x);rx=Mathf.Max(rx,p.x);ly=Mathf.Min(ly,p.y);ry=Mathf.Max(ry,p.y);}
  for(int y=Mathf.Max(0,Mathf.FloorToInt(ly));y<=Mathf.Min(tex.height-1,Mathf.CeilToInt(ry));y++)
   for(int x=Mathf.Max(0,Mathf.FloorToInt(lx));x<=Mathf.Min(tex.width-1,Mathf.CeilToInt(rx));x++){
    bool inside=false;float px=x+.5f,py=y+.5f;
    for(int i=0,j=points.Length-1;i<points.Length;j=i++){
     var a=points[i];var b=points[j];
     if((a.y>py)!=(b.y>py)&&px<(b.x-a.x)*(py-a.y)/(b.y-a.y)+a.x)inside=!inside;
    }
    if(inside)Blend(tex,x,y,color);
   }
 }

 static Texture2D CreateFeatureGraphic(){
  const int w=1024,h=500;
  var tex=new Texture2D(w,h,TextureFormat.RGB24,false);
  var pixels=new Color32[w*h];
  for(int i=0;i<pixels.Length;i++)pixels[i]=Background;
  tex.SetPixels32(pixels);
  DrawGlyph(tex,new Vector2(512,250),520,false);
  tex.Apply(false,false);return tex;
 }

 static void WritePng(string path,Texture2D tex){
  string dir=Path.GetDirectoryName(path);
  if(!string.IsNullOrEmpty(dir))Directory.CreateDirectory(dir);
  File.WriteAllBytes(path,tex.EncodeToPNG());
  UnityEngine.Object.DestroyImmediate(tex);
 }

 static void Blend(Texture2D tex,int x,int y,Color src){
  Color dst=tex.GetPixel(x,y);
  float a=Mathf.Clamp01(src.a);
  float outA=dst.a+(1-dst.a)*a;
  if(outA<=0)return;
  tex.SetPixel(x,y,new Color(
   Mathf.Clamp01((dst.r*dst.a*(1-a)+src.r*a)/outA),
   Mathf.Clamp01((dst.g*dst.a*(1-a)+src.g*a)/outA),
   Mathf.Clamp01((dst.b*dst.a*(1-a)+src.b*a)/outA),
   outA));
 }

}
}
