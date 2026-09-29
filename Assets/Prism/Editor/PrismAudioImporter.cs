using UnityEditor;
using UnityEngine;

namespace Prism.EditorTools {
// Enforce the authored assets' mobile import policy without affecting other audio.
public sealed class PrismAudioImporter : AssetPostprocessor {
 void OnPreprocessAudio(){
  if(!assetPath.StartsWith("Assets/Prism/Resources/Audio/"))return;
  var importer=(AudioImporter)assetImporter;
  bool music=assetPath.EndsWith("/OpticalLaboratory.wav");
  var settings=importer.defaultSampleSettings;
  settings.loadType=music?AudioClipLoadType.Streaming:AudioClipLoadType.DecompressOnLoad;
  settings.compressionFormat=music?AudioCompressionFormat.Vorbis:AudioCompressionFormat.PCM;
  settings.sampleRateSetting=AudioSampleRateSetting.PreserveSampleRate;
  settings.quality=.65f;
  settings.preloadAudioData=true;
  importer.defaultSampleSettings=settings;
  importer.forceToMono=!music;
  importer.loadInBackground=music;
 }
}
}
