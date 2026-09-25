using System;
using System.Collections.Generic;
namespace Prism {
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
 static double DistanceToSegment(V p,Wall wall){
  V edge=wall.B-wall.A;double denom=V.Dot(edge,edge);
  if(denom<1e-10)return (p-wall.A).Length;
  double t=Math.Max(0,Math.Min(1,V.Dot(p-wall.A,edge)/denom));
  return (p-(wall.A+edge*t)).Length;
 }
 public static bool IsValid(Level level,IList<Piece> pieces,Kind kind,V position,int ignoreIndex=-1){
  if(level==null||pieces==null)return false;
  if(Math.Abs(position.X)>BoardHalfExtent||Math.Abs(position.Y)>BoardHalfExtent)return false;
  double clearance=Clearance(kind);
  if((position-level.Source).Length<clearance+.35)return false;
  foreach(var goal in level.Goals)if((position-goal.Position).Length<clearance+goal.Radius)return false;
  foreach(var wall in level.Walls)if(DistanceToSegment(position,wall)<clearance)return false;
  for(int i=0;i<pieces.Count;i++){
   if(i==ignoreIndex)continue;
   var other=pieces[i];
   if((position-other.Position).Length<clearance+Clearance(other.Kind))return false;
  }
  return true;
 }
}
public class Session {
 public Level Level;public List<Piece> Pieces=new List<Piece>();
 readonly Stack<List<Piece>> history=new Stack<List<Piece>>(); List<Piece> before;
 static List<Piece> Copy(IEnumerable<Piece> ps){var copy=new List<Piece>();foreach(var p in ps)copy.Add(p.Copy());return copy;}
 public Session(Level l){Level=l;Pieces=Copy(l.Initial);}
 public int Remaining(Kind kind){int n=0;foreach(var k in Level.Stock)if(k==kind)n++;foreach(var p in Pieces)if(p.Kind==kind)n--;return n;}
 public bool Place(Kind kind,V position){if(Remaining(kind)<=0||!PlacementRules.IsValid(Level,Pieces,kind,position))return false;BeginEdit();Pieces.Add(new Piece(kind,position));EndEdit();return true;}
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
