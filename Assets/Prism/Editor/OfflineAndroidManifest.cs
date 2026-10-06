using System.IO;
using System.Xml;
using UnityEditor;
using UnityEditor.Android;

namespace Prism.Editor {
public sealed class OfflineAndroidManifest : IPostGenerateGradleAndroidProject {
 const string AndroidNamespace="http://schemas.android.com/apk/res/android";
 const string ToolsNamespace="http://schemas.android.com/tools";
 public int callbackOrder=>1000;
 public void OnPostGenerateGradleAndroidProject(string path){
  if(EditorUserBuildSettings.development)return;
  string manifestPath=Path.Combine(path,"src/main/AndroidManifest.xml");
  if(!File.Exists(manifestPath))throw new FileNotFoundException("Generated Android manifest is missing",manifestPath);
  var document=new XmlDocument();document.Load(manifestPath);
  var manifest=document.DocumentElement;
  foreach(XmlNode node in document.SelectNodes("/manifest/uses-permission"))
   if(node.Attributes?["name",AndroidNamespace]?.Value=="android.permission.INTERNET")manifest.RemoveChild(node);
  bool vibrate=false;
  foreach(XmlNode node in document.SelectNodes("/manifest/uses-permission"))
   if(node.Attributes?["name",AndroidNamespace]?.Value=="android.permission.VIBRATE")vibrate=true;
  if(!vibrate){
   var permission=document.CreateElement("uses-permission");
   var permissionName=document.CreateAttribute("android","name",AndroidNamespace);permissionName.Value="android.permission.VIBRATE";permission.Attributes.Append(permissionName);
   manifest.AppendChild(permission);
  }
  manifest.SetAttribute("xmlns:tools",ToolsNamespace);
  var removal=document.CreateElement("uses-permission");
  var name=document.CreateAttribute("android","name",AndroidNamespace);name.Value="android.permission.INTERNET";removal.Attributes.Append(name);
  var action=document.CreateAttribute("tools","node",ToolsNamespace);action.Value="remove";removal.Attributes.Append(action);
  manifest.AppendChild(removal);
  document.Save(manifestPath);
 }
}
}
