using System;
using System.Collections.Generic;
namespace Prism {
public enum PlacementFailure { None, OutsideBoard, Source, Goal, Wall, Piece }
public static class PieceFootprint {
 // The physical footprint follows the same angles, lengths and thicknesses as BoardRenderer.
 public static V[] Vertices(Kind kind,V position,double angle){
  if(kind==Kind.Prism){
   var vertices=new V[3];
   for(int i=0;i<3;i++)vertices[i]=position+V.Angle(angle+i*120)*.68;
   return vertices;
  }
  V axis=V.Angle(angle),side=axis.Perp;
  double halfLength=kind==Kind.Lens?.9:.65;
  double halfWidth=kind==Kind.Lens?.18:.10;
  return new[]{position-axis*halfLength-side*halfWidth,position+axis*halfLength-side*halfWidth,
   position+axis*halfLength+side*halfWidth,position-axis*halfLength+side*halfWidth};
 }
 public static bool PolygonsOverlap(V[] a,V[] b,double gap=0){
  return !Separated(a,b,gap)&&!Separated(b,a,gap);
 }
 static bool Separated(V[] shape,V[] other,double gap){
  for(int i=0;i<shape.Length;i++){
   V axis=(shape[(i+1)%shape.Length]-shape[i]).Perp.Unit;
   double aMin=double.PositiveInfinity,aMax=double.NegativeInfinity;
   double bMin=double.PositiveInfinity,bMax=double.NegativeInfinity;
   foreach(var v in shape){double projection=V.Dot(v,axis);aMin=Math.Min(aMin,projection);aMax=Math.Max(aMax,projection);}
   foreach(var v in other){double projection=V.Dot(v,axis);bMin=Math.Min(bMin,projection);bMax=Math.Max(bMax,projection);}
   if(aMax+gap<bMin||bMax+gap<aMin)return true;
  }
  return false;
 }
 public static bool OverlapsWall(Kind kind,V position,double angle,Wall wall){
  if(kind==Kind.Sphere)return WallGeometry.Distance(position,wall)<.55+.025;
  return PolygonsOverlap(Vertices(kind,position,angle),WallGeometry.Vertices(wall),.025);
 }
}
public static class PlacementRules {
 const double BoardHalfExtent=4.25;
 public static double Clearance(Kind kind){
  switch(kind){
   case Kind.Prism:return .48;
   case Kind.Lens:return .42;
   case Kind.Sphere:return .56;
   default:return .36;
  }
 }
 public static PlacementFailure Check(Level level,IList<Piece> pieces,Kind kind,V position,int ignoreIndex=-1,double angle=0){
  if(level==null||pieces==null)return PlacementFailure.OutsideBoard;
  if(Math.Abs(position.X)>BoardHalfExtent||Math.Abs(position.Y)>BoardHalfExtent)return PlacementFailure.OutsideBoard;
  double clearance=Clearance(kind);
  if((position-level.Source).Length<clearance+.35)return PlacementFailure.Source;
  foreach(var goal in level.Goals)if((position-goal.Position).Length<clearance+goal.Radius)return PlacementFailure.Goal;
  foreach(var wall in level.Walls){
   if(WallGeometry.Distance(position,wall)<clearance||
    // Existing mastery gate corridors were authored for center clearance only.
    // Keep those solved layouts accessible until their geometry is re-authored.
    (wall.Purpose!="gate"&&PieceFootprint.OverlapsWall(kind,position,angle,wall)))
    return PlacementFailure.Wall;
  }
  for(int i=0;i<pieces.Count;i++){
   if(i==ignoreIndex)continue;
   var other=pieces[i];
   if((position-other.Position).Length<clearance+Clearance(other.Kind))return PlacementFailure.Piece;
  }
  return PlacementFailure.None;
 }
 public static bool IsValid(Level level,IList<Piece> pieces,Kind kind,V position,int ignoreIndex=-1,double angle=0)=>
  Check(level,pieces,kind,position,ignoreIndex,angle)==PlacementFailure.None;
}
public class Session {
 public Level Level;public List<Piece> Pieces=new List<Piece>();
 readonly Stack<List<Piece>> history=new Stack<List<Piece>>(); List<Piece> before;
 static List<Piece> Copy(IEnumerable<Piece> ps){var copy=new List<Piece>();foreach(var p in ps)copy.Add(p.Copy());return copy;}
 public Session(Level l){Level=l;Pieces=Copy(l.Initial);}
 public int Remaining(Kind kind){int n=0;foreach(var k in Level.Stock)if(k==kind)n++;foreach(var p in Pieces)if(p.Kind==kind)n--;return n;}
 public bool Place(Kind kind,V position,double angle=0){
  if(Remaining(kind)<=0||!PlacementRules.IsValid(Level,Pieces,kind,position,angle:angle))return false;
  BeginEdit();Pieces.Add(new Piece(kind,position,angle));EndEdit();return true;
 }
 public void BeginEdit(){before=Copy(Pieces);} public void EndEdit(){if(before!=null){history.Push(before);before=null;}}
 public void Undo(){if(history.Count>0)Pieces=history.Pop();before=null;}
 public void Reset(){BeginEdit();Pieces=Copy(Level.Initial);EndEdit();}
 public void Remove(int index){if(index<0||index>=Pieces.Count)return;BeginEdit();Pieces.RemoveAt(index);EndEdit();}
 public void Reveal(){BeginEdit();Pieces=Copy(Level.Solution);EndEdit();}
 public bool IsComplete(Result result){
  if(result==null||!result.Complete)return false;
  if(!Level.RequireAllPiecesActive)return true;
  var checkedKinds=new HashSet<Kind>();
  foreach(var kind in Level.Stock)if(checkedKinds.Add(kind)&&Remaining(kind)>0)return false;
  return result.ActivePieceCount>=Pieces.Count;
 }
}
}
