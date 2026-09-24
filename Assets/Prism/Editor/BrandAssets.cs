using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Prism.Editor {
public static class BrandAssets {
 const string GeneratedDir="Assets/Prism/Generated";
 const string IconPath=GeneratedDir+"/AppIcon.png";
 const string StoreDir="Builds/StoreAssets";

 [MenuItem("PrisM/Release/Generate Store Assets")]
 public static void GenerateStoreAssets(){
  Directory.CreateDirectory(StoreDir);
  WritePng(Path.Combine(StoreDir,"play-icon-512.png"),CreateIcon(512));
  WritePng(Path.Combine(StoreDir,"feature-graphic-1024x500.png"),CreateFeatureGraphic());
  Debug.Log("PrisM store assets generated in "+Path.GetFullPath(StoreDir));
 }

 public static void PrepareAndroidBranding(){
  Directory.CreateDirectory(GeneratedDir);
  var icon=CreateIcon(1024);
  WritePng(IconPath,icon);
  UnityEngine.Object.DestroyImmediate(icon);
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

  var target=NamedBuildTarget.Android;
  int[] sizes=PlayerSettings.GetIconSizes(target,IconKind.Application);
  if(sizes==null||sizes.Length==0)throw new Exception("Android icon slots are unavailable. Install Android Build Support for this Unity editor.");
  var icons=new Texture2D[sizes.Length];
  for(int i=0;i<icons.Length;i++)icons[i]=loaded;
  PlayerSettings.SetIcons(target,icons,IconKind.Application);

#if UNITY_ANDROID
  ApplyPlatformIcons(target,AndroidPlatformIconKind.Adaptive,loaded);
#endif

  GenerateStoreAssets();
  AssetDatabase.SaveAssets();
 }

#if UNITY_ANDROID
 static void ApplyPlatformIcons(NamedBuildTarget target,PlatformIconKind kind,Texture2D texture){
  var slots=PlayerSettings.GetPlatformIcons(target,kind);
  for(int i=0;i<slots.Length;i++){
   int layers=Mathf.Max(1,slots[i].maxLayerCount);
   var textures=new Texture2D[layers];
   for(int layer=0;layer<layers;layer++)textures[layer]=texture;
   slots[i].SetTextures(textures);
  }
  PlayerSettings.SetPlatformIcons(target,kind,slots);
 }
#endif

 static Texture2D CreateIcon(int size){
  var tex=new Texture2D(size,size,TextureFormat.RGBA32,false,true);
  var pixels=new Color32[size*size];
  Color32 bg0=new Color32(4,11,18,255),bg1=new Color32(10,34,45,255);
  Vector2 center=new Vector2(size*.5f,size*.5f);
  float max=size*.72f;
  for(int y=0;y<size;y++)for(int x=0;x<size;x++){
   float d=Vector2.Distance(new Vector2(x,y),center)/max;
   pixels[y*size+x]=Lerp(bg1,bg0,Mathf.Clamp01(d));
  }
  tex.SetPixels32(pixels);

  float s=size;
  Vector2 a=new Vector2(.31f*s,.70f*s);
  Vector2 b=new Vector2(.50f*s,.28f*s);
  Vector2 c=new Vector2(.69f*s,.70f*s);

  DrawGlowLine(tex,new Vector2(.08f*s,.52f*s),new Vector2(.39f*s,.52f*s),.028f*s,new Color(1f,.96f,.82f,1));
  Color[] spectrum={
   new Color(.43f,.28f,1),new Color(.22f,.5f,1),new Color(.08f,.82f,1),
   new Color(.3f,1,.58f),new Color(.92f,1,.38f),new Color(1,.62f,.18f),new Color(1,.26f,.30f)
  };
  for(int i=0;i<spectrum.Length;i++){
   float oy=(i-3)*.032f*s;
   DrawGlowLine(tex,new Vector2(.57f*s,.52f*s),new Vector2(.91f*s,.35f*s+oy),.012f*s,spectrum[i]);
  }

  FillTriangle(tex,a,b,c,new Color(.16f,.48f,.62f,.34f));
  DrawGlowLine(tex,a,b,.014f*s,new Color(.82f,.96f,1,1));
  DrawGlowLine(tex,b,c,.014f*s,new Color(.82f,.96f,1,1));
  DrawGlowLine(tex,c,a,.014f*s,new Color(.82f,.96f,1,1));

  tex.Apply(false,false);
  return tex;
 }

 static Texture2D CreateFeatureGraphic(){
  const int w=1024,h=500;
  var tex=new Texture2D(w,h,TextureFormat.RGB24,false,true);
  var pixels=new Color32[w*h];
  Color32 left=new Color32(4,12,20,255),right=new Color32(7,28,38,255);
  for(int y=0;y<h;y++)for(int x=0;x<w;x++){
   float tx=x/(float)(w-1);
   float vignette=Mathf.Clamp01(Vector2.Distance(new Vector2(x/(float)w,y/(float)h),new Vector2(.55f,.5f))/.82f);
   pixels[y*w+x]=Lerp(Lerp(left,right,tx),new Color32(2,7,12,255),vignette*.45f);
  }
  tex.SetPixels32(pixels);

  for(int x=34;x<w;x+=34)for(int y=28;y<h;y+=34)DrawDisc(tex,new Vector2(x,y),1.4f,new Color(.18f,.36f,.43f,.45f));

  Vector2 a=new Vector2(410,365),b=new Vector2(520,118),c=new Vector2(630,365);
  DrawGlowLine(tex,new Vector2(75,250),new Vector2(456,250),20,new Color(1,.96f,.84f,1));

  Color[] spectrum={
   new Color(.43f,.28f,1),new Color(.22f,.5f,1),new Color(.08f,.82f,1),
   new Color(.3f,1,.58f),new Color(.92f,1,.38f),new Color(1,.62f,.18f),new Color(1,.26f,.30f)
  };
  for(int i=0;i<spectrum.Length;i++){
   float offset=(i-3)*27;
   DrawGlowLine(tex,new Vector2(570,250),new Vector2(962,142+offset),8,spectrum[i]);
  }

  FillTriangle(tex,a,b,c,new Color(.12f,.46f,.62f,.28f));
  DrawGlowLine(tex,a,b,8,new Color(.78f,.96f,1,1));
  DrawGlowLine(tex,b,c,8,new Color(.78f,.96f,1,1));
  DrawGlowLine(tex,c,a,8,new Color(.78f,.96f,1,1));
  DrawDisc(tex,new Vector2(140,90),38,new Color(.91f,.77f,.49f,.16f));
  DrawDisc(tex,new Vector2(880,402),62,new Color(.12f,.7f,.9f,.08f));

  tex.Apply(false,false);
  return tex;
 }

 static void WritePng(string path,Texture2D tex){
  string dir=Path.GetDirectoryName(path);
  if(!string.IsNullOrEmpty(dir))Directory.CreateDirectory(dir);
  File.WriteAllBytes(path,tex.EncodeToPNG());
  UnityEngine.Object.DestroyImmediate(tex);
 }

 static void FillTriangle(Texture2D tex,Vector2 a,Vector2 b,Vector2 c,Color color){
  int minX=Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(a.x,Mathf.Min(b.x,c.x))),0,tex.width-1);
  int maxX=Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(a.x,Mathf.Max(b.x,c.x))),0,tex.width-1);
  int minY=Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(a.y,Mathf.Min(b.y,c.y))),0,tex.height-1);
  int maxY=Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(a.y,Mathf.Max(b.y,c.y))),0,tex.height-1);
  float area=Cross(b-a,c-a);
  if(Mathf.Abs(area)<.001f)return;
  for(int y=minY;y<=maxY;y++)for(int x=minX;x<=maxX;x++){
   Vector2 p=new Vector2(x+.5f,y+.5f);
   float u=Cross(b-a,p-a)/area,v=Cross(c-b,p-b)/area,w=Cross(a-c,p-c)/area;
   if((u>=0&&v>=0&&w>=0)||(u<=0&&v<=0&&w<=0))Blend(tex,x,y,color);
  }
 }

 static float Cross(Vector2 a,Vector2 b)=>a.x*b.y-a.y*b.x;

 static void DrawGlowLine(Texture2D tex,Vector2 a,Vector2 b,float width,Color color){
  DrawLine(tex,a,b,width*2.8f,new Color(color.r,color.g,color.b,.10f));
  DrawLine(tex,a,b,width*1.55f,new Color(color.r,color.g,color.b,.26f));
  DrawLine(tex,a,b,width,color);
  DrawLine(tex,a,b,Mathf.Max(1,width*.26f),Color.Lerp(color,Color.white,.72f));
 }

 static void DrawLine(Texture2D tex,Vector2 a,Vector2 b,float width,Color color){
  int minX=Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(a.x,b.x)-width),0,tex.width-1);
  int maxX=Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(a.x,b.x)+width),0,tex.width-1);
  int minY=Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(a.y,b.y)-width),0,tex.height-1);
  int maxY=Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(a.y,b.y)+width),0,tex.height-1);
  Vector2 ab=b-a;float denom=Mathf.Max(.0001f,Vector2.Dot(ab,ab));
  float radius=width*.5f;
  for(int y=minY;y<=maxY;y++)for(int x=minX;x<=maxX;x++){
   Vector2 p=new Vector2(x+.5f,y+.5f);
   float t=Mathf.Clamp01(Vector2.Dot(p-a,ab)/denom);
   float dist=Vector2.Distance(p,a+ab*t);
   if(dist<=radius)Blend(tex,x,y,new Color(color.r,color.g,color.b,color.a*(1-dist/Mathf.Max(.001f,radius)*.25f)));
  }
 }

 static void DrawDisc(Texture2D tex,Vector2 center,float radius,Color color){
  int minX=Mathf.Clamp(Mathf.FloorToInt(center.x-radius),0,tex.width-1);
  int maxX=Mathf.Clamp(Mathf.CeilToInt(center.x+radius),0,tex.width-1);
  int minY=Mathf.Clamp(Mathf.FloorToInt(center.y-radius),0,tex.height-1);
  int maxY=Mathf.Clamp(Mathf.CeilToInt(center.y+radius),0,tex.height-1);
  for(int y=minY;y<=maxY;y++)for(int x=minX;x<=maxX;x++){
   float d=Vector2.Distance(new Vector2(x+.5f,y+.5f),center);
   if(d<=radius)Blend(tex,x,y,new Color(color.r,color.g,color.b,color.a*(1-d/radius*.35f)));
  }
 }

 static void Blend(Texture2D tex,int x,int y,Color src){
  Color dst=tex.GetPixel(x,y);
  float a=Mathf.Clamp01(src.a);
  tex.SetPixel(x,y,new Color(
   Mathf.Clamp01(dst.r*(1-a)+src.r*a),
   Mathf.Clamp01(dst.g*(1-a)+src.g*a),
   Mathf.Clamp01(dst.b*(1-a)+src.b*a),
   Mathf.Clamp01(dst.a+(1-dst.a)*a)));
 }

 static Color32 Lerp(Color32 a,Color32 b,float t){
  t=Mathf.Clamp01(t);
  return new Color32(
   (byte)Mathf.RoundToInt(Mathf.Lerp(a.r,b.r,t)),
   (byte)Mathf.RoundToInt(Mathf.Lerp(a.g,b.g,t)),
   (byte)Mathf.RoundToInt(Mathf.Lerp(a.b,b.b,t)),
   (byte)Mathf.RoundToInt(Mathf.Lerp(a.a,b.a,t)));
 }
}
}
