using UnityEditor;
using UnityEngine;

namespace Prism {
[CustomEditor(typeof(LevelCatalog))]
public sealed class LevelCatalogEditor : Editor {
 public override void OnInspectorGUI(){
  DrawDefaultInspector();
  EditorGUILayout.Space(10);
  var catalog=(LevelCatalog)target;

  if(GUILayout.Button("Import Built-in Levels")){
   Undo.RecordObject(catalog,"Import PrisM built-in levels");
   catalog.Import(Levels.Create());
   EditorUtility.SetDirty(catalog);
   AssetDatabase.SaveAssets();
  }

  if(GUILayout.Button("Validate Catalog")){
   bool ok=catalog.Validate(out string message);
   if(ok)Debug.Log("PrisM LevelCatalog: "+message,catalog);
   else Debug.LogError("PrisM LevelCatalog: "+message,catalog);
  }
 }

 [MenuItem("PrisM/Authoring/Create or Refresh Default Level Catalog")]
 static void CreateOrRefresh(){
  const string resourcesPath="Assets/Prism/Resources";
  const string assetPath=resourcesPath+"/LevelCatalog.asset";

  if(!AssetDatabase.IsValidFolder(resourcesPath)){
   if(!AssetDatabase.IsValidFolder("Assets/Prism"))AssetDatabase.CreateFolder("Assets","Prism");
   AssetDatabase.CreateFolder("Assets/Prism","Resources");
  }

  var catalog=AssetDatabase.LoadAssetAtPath<LevelCatalog>(assetPath);
  if(catalog==null){
   catalog=ScriptableObject.CreateInstance<LevelCatalog>();
   AssetDatabase.CreateAsset(catalog,assetPath);
  }

  Undo.RecordObject(catalog,"Refresh PrisM default level catalog");
  catalog.Import(Levels.Create());
  EditorUtility.SetDirty(catalog);
  AssetDatabase.SaveAssets();
  AssetDatabase.Refresh();

  if(!catalog.Validate(out string message))Debug.LogError("PrisM LevelCatalog: "+message,catalog);
  else Debug.Log("PrisM LevelCatalog refreshed: "+message,catalog);

  Selection.activeObject=catalog;
  EditorGUIUtility.PingObject(catalog);
 }
}
}
