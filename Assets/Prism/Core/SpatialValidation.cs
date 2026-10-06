using System;
using System.Collections.Generic;
namespace Prism {
// Concrete counterfactual witnesses, rather than purpose strings, validate walls.
public static class SpatialValidation {
 public static bool TryWitness(Level level,int wallIndex,out V from,out V to){
  from=to=new V();if(wallIndex<0||wallIndex>=level.Walls.Length)return false;
  Wall selected=level.Walls[wallIndex];
  var points=new List<V>{level.Source};foreach(var piece in level.Solution)points.Add(piece.Position);foreach(var goal in level.Goals)points.Add(goal.Position);
  foreach(var a in points)foreach(var b in points){if((b-a).Length<.1)continue;if(FreedByRemoval(level,selected,a,b)){from=a;to=b;return true;}}
  // A full gate must contain a real solved crossing and two finite barriers.
  // Its transverse wrong-route probe crosses one closed part of the gate.
  if(selected.Purpose!="gate")return false;
  Wall mate=new Wall();bool found=false;
  foreach(var other in level.Walls)if(other.Cluster==selected.Cluster&&((other.A-selected.A).Length>.01||(other.B-selected.B).Length>.01)){mate=other;found=true;break;}
  if(!found)return false;
  V axis=(selected.B-selected.A).Unit,normal=axis.Perp;
  if(Math.Abs(V.Cross((mate.B-mate.A).Unit,axis))>.001)return false;
  V gateOrigin=selected.A;double lo=double.PositiveInfinity,hi=double.NegativeInfinity;
  foreach(var endpoint in new[]{selected.A,selected.B,mate.A,mate.B}){double t=V.Dot(endpoint-gateOrigin,axis);lo=Math.Min(lo,t);hi=Math.Max(hi,t);}
  if(hi-lo<8.5)return false;
  bool crossing=false;var solved=Optics.Solve(level,level.Solution);
  foreach(var beam in solved.Beams){double a=V.Dot(beam.A-gateOrigin,normal),b=V.Dot(beam.B-gateOrigin,normal);if(a*b<0){V p=beam.A+(beam.B-beam.A)*(a/(a-b));if(WallGeometry.Distance(p,selected)>.01&&WallGeometry.Distance(p,mate)>.01){crossing=true;break;}}}
  if(!crossing)return false;
  V center=(selected.A+selected.B)*.5;
  from=center-normal*.7;to=center+normal*.7;
  return FreedByRemoval(level,selected,from,to);
 }
 public static bool FreedByRemoval(Level level,Wall selected,V a,V b){
  bool blocked=false;
  foreach(var wall in level.Walls){
   if(!WallGeometry.Blocks(wall,a,b))continue;
   bool removed=selected.Purpose=="gate"?wall.Cluster==selected.Cluster:
    (wall.A-selected.A).Length<1e-8&&(wall.B-selected.B).Length<1e-8;
   if(!removed)return false;blocked=true;
  }
  return blocked;
 }
}
}
