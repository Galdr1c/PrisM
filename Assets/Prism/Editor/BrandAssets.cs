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
 const string StoreDir="Builds/StoreAssets";
 const string SourceArtPath="artifacts/logo.png";

 [MenuItem("PRISM/Release/Generate Store Assets")]
 public static void GenerateStoreAssets(){
  Directory.CreateDirectory(StoreDir);
  string storeIcon=Path.Combine(StoreDir,"play-icon-512.png");
  WritePng(storeIcon,CreateIcon(512));
  if(new FileInfo(storeIcon).Length>1024*1024)throw new Exception("Play icon exceeds the 1024 KB upload limit: "+storeIcon);
  WritePng(Path.Combine(StoreDir,"feature-graphic-1024x500.png"),CreateFeatureGraphic());
  WritePng(MonochromePath,CreateGlyph(1024,true,.64f));
  foreach(int size in new[]{32,48,64,128,192}){
   WritePng(Path.Combine(StoreDir,"icon-"+size+".png"),CreateIcon(size));
   WritePng(Path.Combine(StoreDir,"monochrome-"+size+".png"),CreateGlyph(size,true));
  }
  AssetDatabase.ImportAsset(MonochromePath,ImportAssetOptions.ForceUpdate|ImportAssetOptions.ForceSynchronousImport);
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
  return SampleSource(size,size,1f,false);
 }

 // Full-color branding is supplied by the player, never replaced by a fallback glyph.
 static readonly Color32 Background=new Color32(9,8,18,255);

 static Texture2D CreateIconBackground(int size){
  var tex=new Texture2D(size,size,TextureFormat.RGBA32,false);
  var pixels=new Color32[size*size];
  for(int i=0;i<pixels.Length;i++)pixels[i]=Background;
  tex.SetPixels32(pixels);tex.Apply(false,false);return tex;
 }

 static Texture2D CreateAdaptiveForeground(int size)=>SampleSource(size,size,.74f,true);

 static Texture2D SampleSource(int width,int height,float scale,bool transparent){
  if(!File.Exists(SourceArtPath))throw new FileNotFoundException("PRISM supplied icon is missing.",SourceArtPath);
  var bytes=File.ReadAllBytes(SourceArtPath);
  if(bytes.Length<8||bytes[0]!=137||bytes[1]!=80||bytes[2]!=78||bytes[3]!=71||bytes[4]!=13||bytes[5]!=10||bytes[6]!=26||bytes[7]!=10)throw new Exception("PRISM icon source must be a PNG.");
  var source=new Texture2D(2,2,TextureFormat.RGBA32,false);
  try{
   if(!ImageConversion.LoadImage(source,bytes,false))throw new Exception("PRISM icon is not a valid PNG.");
   if(source.width!=source.height)throw new Exception("PRISM supplied icon must be square.");
   var output=new Texture2D(width,height,TextureFormat.RGBA32,false);
   var pixels=new Color32[width*height];
   float span=Mathf.Min(width,height)*scale,left=(width-span)*.5f,bottom=(height-span)*.5f;
   for(int y=0;y<height;y++)for(int x=0;x<width;x++){
    float u=(x+.5f-left)/span,v=(y+.5f-bottom)/span;
    pixels[y*width+x]=u>=0&&u<=1&&v>=0&&v<=1 ? (Color32)source.GetPixelBilinear(u,v) : transparent ? new Color32(0,0,0,0) : Background;
   }
   output.SetPixels32(pixels);output.Apply(false,false);return output;
  }finally{UnityEngine.Object.DestroyImmediate(source);}
 }

 static Texture2D CreateGlyph(int size,bool monochrome,float scale=1){
  var tex=new Texture2D(size,size,TextureFormat.RGBA32,false);
  tex.SetPixels32(new Color32[size*size]);
  DrawGlyph(tex,new Vector2(size*.5f,size*.5f),size*scale);
  tex.Apply(false,false);return tex;
 }

 static void DrawGlyph(Texture2D tex,Vector2 center,float size){
  Vector2 P(float x,float y)=>center+new Vector2(x-.5f,y-.5f)*size;
  // Themed launcher silhouette follows the supplied prism / incoming beam / downward exit.
  FillPolygon(tex,new[]{P(.14f,.69f),P(.46f,.69f),P(.46f,.74f),P(.14f,.74f)},Color.white);
  FillPolygon(tex,new[]{P(.48f,.85f),P(.40f,.57f),P(.73f,.66f)},Color.white);
  FillPolygon(tex,new[]{P(.54f,.57f),P(.64f,.60f),P(.69f,.24f),P(.54f,.24f)},Color.white);
  Vector2 target=P(.61f,.22f);
  for(int y=0;y<tex.height;y++)for(int x=0;x<tex.width;x++){
   float distance=Vector2.Distance(new Vector2(x+.5f,y+.5f),target)/size;
   if(distance<.12f&&distance>.075f)Blend(tex,x,y,Color.white);
  }
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
  return SampleSource(1024,500,1f,false);
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
