using UnityEditor;

namespace Prism.Editor {
public sealed class VisualAssetImporter : AssetPostprocessor {
 void OnPreprocessTexture(){
  if(assetPath!="Assets/Prism/Resources/Art/LabSurface.png"&&assetPath!="Assets/Prism/Resources/Art/WaterSurface.png"&&assetPath!="Assets/Prism/Resources/Art/MasterySurface.png")return;
  var importer=(TextureImporter)assetImporter;
  importer.textureType=TextureImporterType.Default;
  importer.sRGBTexture=true;
  importer.alphaSource=TextureImporterAlphaSource.None;
  importer.mipmapEnabled=false;
  importer.isReadable=false;
  importer.wrapMode=UnityEngine.TextureWrapMode.Clamp;
  importer.filterMode=UnityEngine.FilterMode.Bilinear;
  importer.maxTextureSize=1024;
  importer.textureCompression=TextureImporterCompression.Compressed;
  var android=importer.GetPlatformTextureSettings("Android");
  android.overridden=true;
  android.maxTextureSize=1024;
  android.format=TextureImporterFormat.ASTC_6x6;
  importer.SetPlatformTextureSettings(android);
 }
}
}
