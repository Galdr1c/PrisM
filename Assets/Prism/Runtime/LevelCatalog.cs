using System;
using System.Collections.Generic;
using UnityEngine;

namespace Prism {
[Serializable]
public struct PieceDefinition {
 [SerializeField] Kind kind;
 [SerializeField] Vector2 position;
 [SerializeField] float angle;

 public Piece ToPiece()=>new Piece(kind,new V(position.x,position.y),angle);
 public static PieceDefinition FromPiece(Piece piece)=>new PieceDefinition{kind=piece.Kind,position=new Vector2((float)piece.Position.X,(float)piece.Position.Y),angle=(float)piece.Angle};
}

[Serializable]
public struct GoalDefinition {
 [SerializeField] Vector2 position;
 [SerializeField] int band;
 [SerializeField] float radius;
 [SerializeField] float threshold;

 public Goal ToGoal()=>new Goal(new V(position.x,position.y),band){Radius=radius,Threshold=threshold};
 public static GoalDefinition FromGoal(Goal goal)=>new GoalDefinition{position=new Vector2((float)goal.Position.X,(float)goal.Position.Y),band=goal.Band,radius=(float)goal.Radius,threshold=(float)goal.Threshold};
}

[Serializable]
public struct WallDefinition {
 [SerializeField] Vector2 a;
 [SerializeField] Vector2 b;
 [SerializeField] float thickness;
 [SerializeField] string purpose;
 [SerializeField] int cluster;

 public Wall ToWall()=>new Wall(new V(a.x,a.y),new V(b.x,b.y),thickness>0?thickness:Wall.DefaultThickness,purpose,cluster);
 public static WallDefinition FromWall(Wall wall)=>new WallDefinition{a=new Vector2((float)wall.A.X,(float)wall.A.Y),b=new Vector2((float)wall.B.X,(float)wall.B.Y),thickness=(float)WallGeometry.Thickness(wall),purpose=wall.Purpose,cluster=wall.Cluster};
}

[Serializable]
public struct WaterZoneDefinition {
 [SerializeField] Vector2 min;
 [SerializeField] Vector2 max;
 [SerializeField] float refractiveIndex;

 public WaterZone ToWaterZone()=>new WaterZone(new V(min.x,min.y),new V(max.x,max.y),refractiveIndex);
 public static WaterZoneDefinition FromWaterZone(WaterZone zone)=>new WaterZoneDefinition{min=new Vector2((float)zone.Min.X,(float)zone.Min.Y),max=new Vector2((float)zone.Max.X,(float)zone.Max.Y),refractiveIndex=(float)zone.Index};
}

[Serializable]
public sealed class LevelDefinition {
 [SerializeField] string id="";
 [SerializeField] string displayName="";
 [SerializeField,TextArea] string lesson="";
 [SerializeField,TextArea] string hint="";
 [SerializeField] string chapter="";
 [SerializeField,Range(1,10)] int difficulty=1;
 [SerializeField,Min(1)] int par=1;
 [SerializeField] bool requireAllPiecesActive;
 [SerializeField] Vector2 source;
 [SerializeField] Vector2 direction=Vector2.right;
 [SerializeField] float sourceWidth=.32f;
 [SerializeField] Kind[] stock=Array.Empty<Kind>();
 [SerializeField] PieceDefinition[] initial=Array.Empty<PieceDefinition>();
 [SerializeField] PieceDefinition[] solution=Array.Empty<PieceDefinition>();
 [SerializeField] GoalDefinition[] goals=Array.Empty<GoalDefinition>();
 [SerializeField] WallDefinition[] walls=Array.Empty<WallDefinition>();
 [SerializeField] WaterZoneDefinition[] waterZones=Array.Empty<WaterZoneDefinition>();

 public string Id=>id;
 public string DisplayName=>displayName;

 public Level ToLevel(){
  return new Level{
   Id=id??"",
   Name=displayName??"",
   Lesson=lesson??"",
   Hint=hint??"",
   Chapter=chapter??"",
   Difficulty=difficulty,
   Par=par,
   RequireAllPiecesActive=requireAllPiecesActive,
   Source=new V(source.x,source.y),
   Direction=new V(direction.x,direction.y),
   Width=sourceWidth,
   Stock=Copy(stock),
   Initial=Map(initial,x=>x.ToPiece()),
   Solution=Map(solution,x=>x.ToPiece()),
   Goals=Map(goals,x=>x.ToGoal()),
   Walls=Map(walls,x=>x.ToWall()),
   WaterZones=Map(waterZones,x=>x.ToWaterZone())
  };
 }

 public void Import(Level level){
  if(level==null)return;
  id=level.Id??"";
  displayName=level.Name??"";
  lesson=level.Lesson??"";
  hint=level.Hint??"";
  chapter=level.Chapter??"";
  difficulty=level.Difficulty;
  par=Math.Max(1,level.Par);
  requireAllPiecesActive=level.RequireAllPiecesActive;
  source=new Vector2((float)level.Source.X,(float)level.Source.Y);
  direction=new Vector2((float)level.Direction.X,(float)level.Direction.Y);
  sourceWidth=(float)level.Width;
  stock=Copy(level.Stock);
  initial=Map(level.Initial,PieceDefinition.FromPiece);
  solution=Map(level.Solution,PieceDefinition.FromPiece);
  goals=Map(level.Goals,GoalDefinition.FromGoal);
  walls=Map(level.Walls,WallDefinition.FromWall);
  waterZones=Map(level.WaterZones,WaterZoneDefinition.FromWaterZone);
 }

 static T[] Copy<T>(T[] source){
  if(source==null||source.Length==0)return Array.Empty<T>();
  var result=new T[source.Length];Array.Copy(source,result,source.Length);return result;
 }

 static TOut[] Map<TIn,TOut>(TIn[] source,Func<TIn,TOut> convert){
  if(source==null||source.Length==0)return Array.Empty<TOut>();
  var result=new TOut[source.Length];for(int i=0;i<source.Length;i++)result[i]=convert(source[i]);return result;
 }
}

[CreateAssetMenu(menuName="PrisM/Level Catalog",fileName="LevelCatalog")]
public sealed class LevelCatalog : ScriptableObject {
 [SerializeField] LevelDefinition[] levels=Array.Empty<LevelDefinition>();
 [SerializeField] string campaignRevision="";

 public int Count=>levels?.Length??0;
 public string CampaignRevision=>campaignRevision;
 public LevelDefinition[] Definitions=>levels??Array.Empty<LevelDefinition>();

 public Level[] Build(){
  if(levels==null||levels.Length==0)return Array.Empty<Level>();
  var result=new Level[levels.Length];
  for(int i=0;i<levels.Length;i++)result[i]=levels[i]?.ToLevel()??new Level();
  return result;
 }

 public void Import(Level[] source,string revision=null){
  if(source==null){levels=Array.Empty<LevelDefinition>();return;}
  if(revision!=null)campaignRevision=revision;
  levels=new LevelDefinition[source.Length];
  for(int i=0;i<source.Length;i++){var definition=new LevelDefinition();definition.Import(source[i]);levels[i]=definition;}
 }

 public bool Validate(out string message,bool verifySolutions=true){
  if(levels==null||levels.Length==0){message="Catalog has no levels.";return false;}
  var ids=new HashSet<string>(StringComparer.Ordinal);
  for(int i=0;i<levels.Length;i++){
   var definition=levels[i];
   if(definition==null){message="Level "+(i+1)+" is null.";return false;}
   if(string.IsNullOrWhiteSpace(definition.Id)){message="Level "+(i+1)+" has no stable ID.";return false;}
   if(!ids.Add(definition.Id)){message="Duplicate level ID: "+definition.Id;return false;}
   var runtime=definition.ToLevel();
   if(runtime.Direction.Length<1e-6){message="Level "+definition.Id+" has a zero light direction.";return false;}
   if(runtime.Goals==null||runtime.Goals.Length==0){message="Level "+definition.Id+" has no goals.";return false;}
   if(runtime.Difficulty<1||runtime.Difficulty>10){message="Level "+definition.Id+" has invalid difficulty.";return false;}
   if(string.IsNullOrWhiteSpace(runtime.Chapter)){message="Level "+definition.Id+" has no chapter.";return false;}
   foreach(var wall in runtime.Walls)if((wall.B-wall.A).Length<1e-6){message="Level "+definition.Id+" has a degenerate wall.";return false;}
   if(!verifySolutions)continue;
   for(int w=0;w<runtime.Walls.Length;w++){
    V from,to;if(!SpatialValidation.TryWitness(runtime,w,out from,out to)){message="Level "+definition.Id+" has a wall without a constraint witness.";return false;}
   }
   var placement=new Session(runtime);
   foreach(var piece in runtime.Solution){
    if(!placement.Place(piece.Kind,piece.Position,piece.Angle)){message="Level "+definition.Id+" known solution cannot be placed.";return false;}
    placement.Pieces[placement.Pieces.Count-1].Angle=piece.Angle;
   }
   var initialResult=Optics.Solve(runtime,runtime.Initial);
   if(initialResult.Complete){message="Level "+definition.Id+" starts solved.";return false;}
   var solutionResult=Optics.Solve(runtime,runtime.Solution);
   if(!solutionResult.Complete){message="Level "+definition.Id+" known solution does not complete.";return false;}
   if(solutionResult.Truncated){message="Level "+definition.Id+" known solution exceeds the optical interaction budget.";return false;}
   var session=new Session(runtime);session.Reveal();if(!session.IsComplete(Optics.Solve(runtime,session.Pieces))){message="Level "+definition.Id+" fails gameplay completion rules.";return false;}
  }
  message=levels.Length+(verifySolutions?" levels valid, including placement, wall witnesses and known solutions.":" level definitions valid.");
  return true;
 }
}

public sealed class ScriptableObjectLevelProvider : ILevelProvider {
 readonly LevelCatalog catalog;
 public ScriptableObjectLevelProvider(LevelCatalog catalog){this.catalog=catalog;}
 public Level[] Load(){
  if(catalog!=null&&catalog.Count==100&&catalog.CampaignRevision==LevelCatalogLoader.CampaignRevision){
   if(catalog.Validate(out string message,false))return catalog.Build();
   Debug.LogWarning("PrisM LevelCatalog ignored: "+message);
  }
  return Levels.Create();
 }
}

public static class LevelCatalogLoader {
 const string ResourceName="LevelCatalog";
 // Changes to the physical wall curriculum require rebaking LevelCatalog.asset.
  // Mismatched revisions safely fall back to the deterministic authoring code.
  public const string CampaignRevision="2026-10-10-spatial-optics-v3";
 public static Level[] Load(){
  var catalog=Resources.Load<LevelCatalog>(ResourceName);
  return new ScriptableObjectLevelProvider(catalog).Load();
 }
}
}
