using UnityEditor;
using UnityEngine;

namespace Prism.Editor {
// Keep import policy limited to PRISM's resources, avoiding unrelated project fonts/textures.
public sealed class BrandImporter : AssetPostprocessor {
 void OnPreprocessTexture(){
  if(assetPath!="Assets/Prism/Resources/Brand/OpticalGlyph.png" && !assetPath.StartsWith("Assets/Prism/Generated/"))return;
  var importer=(TextureImporter)assetImporter;
  importer.textureShape=TextureImporterShape.Texture2D;
  importer.textureCompression=TextureImporterCompression.Uncompressed;
  importer.mipmapEnabled=false;
  importer.alphaIsTransparency=true;
  importer.wrapMode=TextureWrapMode.Clamp;
  importer.maxTextureSize=1024;
  if(assetPath.EndsWith("OpticalGlyph.png")){
   importer.textureType=TextureImporterType.Sprite;
   importer.spriteImportMode=SpriteImportMode.Single;
   importer.spritePixelsPerUnit=100;
  }
 }

 void OnPreprocessFont(){
  if(!assetPath.StartsWith("Assets/Prism/Resources/Brand/Fonts/Sora-"))return;
  var importer=(TrueTypeFontImporter)assetImporter;
  importer.includeFontData=true;
  importer.fontTextureCase=FontTextureCase.Dynamic;
  importer.fontSize=32;
 }
}
}
