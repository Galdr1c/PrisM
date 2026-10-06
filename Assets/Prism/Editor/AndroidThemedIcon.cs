using System.IO;
using System.Xml;
using UnityEditor.Android;

namespace Prism.Editor {
// Unity's two-layer adaptive slots do not emit Android 13's monochrome layer.
public sealed class AndroidThemedIcon : IPostGenerateGradleAndroidProject {
 const string AndroidNamespace="http://schemas.android.com/apk/res/android";
 public int callbackOrder=>1100;
 public void OnPostGenerateGradleAndroidProject(string path){
  string resources=Path.Combine(path,"launcher/src/main/res");
  if(!Directory.Exists(resources))resources=Path.Combine(Directory.GetParent(path).FullName,"launcher/src/main/res");
  string adaptive=Path.Combine(resources,"mipmap-anydpi-v26");
  string monochrome=Path.GetFullPath("Assets/Prism/Generated/MonochromeIcon.png");
  if(!Directory.Exists(adaptive)||!File.Exists(monochrome))throw new FileNotFoundException("Generated adaptive icon or monochrome source is missing.");
  string drawable=Path.Combine(resources,"drawable-nodpi");Directory.CreateDirectory(drawable);
  File.Copy(monochrome,Path.Combine(drawable,"prism_monochrome.png"),true);
  string themed=Path.Combine(resources,"mipmap-anydpi-v33");Directory.CreateDirectory(themed);
  int count=0;
  foreach(string file in Directory.GetFiles(adaptive,"*.xml")){
   var document=new XmlDocument();document.Load(file);
   if(document.DocumentElement.Name!="adaptive-icon")continue;
   var layer=document.CreateElement("monochrome");
   var reference=document.CreateAttribute("android","drawable",AndroidNamespace);reference.Value="@drawable/prism_monochrome";layer.Attributes.Append(reference);
   document.DocumentElement.AppendChild(layer);
   document.Save(Path.Combine(themed,Path.GetFileName(file)));count++;
  }
  if(count==0)throw new InvalidDataException("No adaptive icon XML found for themed launcher output.");
 }
}
}
