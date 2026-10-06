using System;
using System.Collections.Generic;

namespace Prism {
public interface ILevelProvider { Level[] Load(); }
public sealed class BuiltInLevelProvider : ILevelProvider { public Level[] Load()=>Levels.Create(); }

public static class Levels {
 static readonly string[] Chapters={
  "Yansıma","Spektrum","Seçici renk","Odak","Renk zinciri",
  "Kırılma","Bileşim","Optik senfoni","Geometri","Ustalık"
 };

 static readonly string[] VariantLabels={
  "Başlangıç","Kuzey rotası","Karşı kıyı","Güney rotası",
  "Sessiz yüzey","Ters ufuk","Çapraz yol","Kırık köşe",
  "İnce ayar","Son prova"
 };

 public static Level[] Create(){
  var levels=new List<Level>(100);
  var seeds=BaseSeeds();

  AddVariants(levels,seeds[0],10);
  AddVariants(levels,seeds[1],10);
  AddVariants(levels,seeds[2],10);
  AddVariants(levels,seeds[3],10);
  AddVariants(levels,seeds[5],10);
  AddVariants(levels,seeds[6],10);
  AddVariants(levels,seeds[4],10);
  AddVariants(levels,seeds[7],10);

  AddVariants(levels,MirrorChain(
   "geometry-3","Üç açı",
   new V(-4,-3.15),
   new[]{new V(-2.75,-2.55),new V(-1.05,-0.25),new V(1.25,-1.25)},
   new V(3.65,2.55),3),5);

  AddVariants(levels,MirrorChain(
   "geometry-4","Dört kırılma değil, dört yansıma",
   new V(-4,2.95),
   new[]{new V(-2.75,2.15),new V(-1.1,0.0),new V(0.9,1.1),new V(2.15,-1.35)},
   new V(3.8,-3.0),4),5);

  AddVariants(levels,MirrorChain(
   "mastery-5a","Açıortay laboratuvarı",
   new V(-4,-3.05),
   new[]{new V(-2.85,-2.2),new V(-1.45,0.25),new V(0.0,-1.15),new V(1.25,1.45),new V(2.55,-0.15)},
   new V(3.85,3.55),5),4);

  AddVariants(levels,MirrorChain(
   "mastery-5b","Ters geometri",
   new V(-3.9,3.15),
   new[]{new V(-2.55,2.15),new V(-1.05,-0.55),new V(0.45,1.15),new V(1.65,-1.55),new V(2.85,0.25)},
   new V(3.9,-3.75),5),2);

  AddVariants(levels,MirrorChain(
   "mastery-6","Son teorem",
   new V(-4,-3.3),
   new[]{new V(-3.0,-2.05),new V(-1.9,0.45),new V(-0.35,-1.2),new V(0.9,1.6),new V(2.05,-0.45),new V(3.05,1.55)},
   new V(4.05,3.2),6),4);

  if(levels.Count!=100)throw new InvalidOperationException("PrisM curriculum must contain exactly 100 levels.");

  for(int i=0;i<levels.Count;i++){
   var level=levels[i];
   level.Chapter=Chapters[i/10];
   level.Difficulty=1+i/10;
   level.Par=Math.Max(1,level.Solution?.Length??0);
   if(i>=80)level.RequireAllPiecesActive=true;
  }
  AuthorMasteryRooms(levels);
  AddSpatialConstraints(levels);
  return levels.ToArray();
 }

 // Room constraints are authored before sources, receivers and optics. Each gate
 // spans the board, so the opening (rather than a decorative bar) governs routes.
 static void AuthorMasteryRooms(List<Level> levels){
  for(int index=80;index<100;index++){
   var legacy=levels[index];int variant=index-80;bool final=index>=90;
   double top=2.25+(variant%5)*.12,bottom=-2.5-(variant%4)*.10;
   V[] turns={new V(-2.8,-2.8),new V(-1.3,top),new V(.2,bottom),new V(1.4,2.7)};
   // Alternating gate heights establish the zig-zag channel before the route is instantiated.
   var walls=new List<Wall>();
   AddGate(walls,-2.05,(-2.8+top)*.5,1.5,1);
   AddGate(walls,-.55,(top+bottom)*.5,1.5,2);
   AddGate(walls,.8,(bottom+2.7)*.5,1.85,3);
   if(final)AddGate(walls,3.15,2.7,.72,4);
   var authored=new Level{
    Source=new V(-4.2,-2.8),Direction=new V(1,0),Width=.4,
    Walls=walls.ToArray(),
    WaterZones=new[]{new WaterZone(new V(-4,-3.2),new V(-3.6,-2.4))},
    Goals=new[]{new Goal(new V(variant%2==0?4.4:3.35,2.7),variant%3==0?6:3){Radius=.09,Threshold=.55},
     new Goal(turns[3]+(turns[3]-turns[2]).Unit*1.0,variant%3==0?3:6){Radius=.3,Threshold=.5}},
    Stock=new[]{Kind.Mirror,Kind.Mirror,Kind.Mirror,variant%3==0?Kind.Red:Kind.Green,variant%2==0?Kind.Lens:Kind.Sphere},
    Solution=new[]{new Piece(Kind.Mirror,turns[0],MirrorLineAngle(new V(1,0),turns[1]-turns[0])),
     new Piece(Kind.Mirror,turns[1],MirrorLineAngle(turns[1]-turns[0],turns[2]-turns[1])),
     new Piece(Kind.Mirror,turns[2],MirrorLineAngle(turns[2]-turns[1],turns[3]-turns[2])),
     new Piece(variant%3==0?Kind.Red:Kind.Green,turns[3],MirrorLineAngle(turns[3]-turns[2],new V(1,0))),
     new Piece(variant%2==0?Kind.Lens:Kind.Sphere,new V(2.4,2.7),90)},
    RequireAllPiecesActive=true,
    Lesson="Kapıları sırayla geç; odağı ve renk ayrımını aynı rotada birleştir.",
    Hint="Üç aynayla dönüşümlü kapıları geç. Renk seçici ışığı iki hedefe ayırır; lens veya cam küre son ışını küçük hedefte toplar."
   };
   if(variant%5==4){
    // Spectral chamber: a red branch is removed before the gate route; the
    // remaining spectrum must pass four reflections and disperse at the exit.
    authored.Source=new V(-4.4,-2.8);authored.Width=.08;
    authored.WaterZones=new[]{new WaterZone(new V(-4.2,-3.2),new V(-3.9,-2.4))};
    authored.Solution[3].Kind=Kind.Mirror;
    authored.Solution[4]=new Piece(Kind.Prism,new V(2.4,2.7),90);
    var pieces=new List<Piece>{new Piece(Kind.Red,new V(-3.6,-2.8),45)};pieces.AddRange(authored.Solution);
    authored.Solution=pieces.ToArray();authored.Stock=new Kind[pieces.Count];for(int k=0;k<pieces.Count;k++)authored.Stock[k]=pieces[k].Kind;
    if(final){walls.RemoveRange(6,2);AddGate(walls,3.15,2.2,.85,4);authored.Walls=walls.ToArray();}
    authored.Goals=Array.Empty<Goal>();
    var traced=Optics.Solve(authored,authored.Solution);
    var targets=new List<Goal>{new Goal(new V(-3.6,-1.4),6){Radius=.28,Threshold=.5}};
    foreach(int band in new[]{0,4}){
     V position=new V();bool found=false;double closest=double.PositiveInfinity;
     foreach(var beam in traced.Beams){
      V dir=(beam.B-beam.A).Unit;if(beam.Band!=band||beam.A.X<2.4||beam.A.X>3||dir.X<=0||beam.B.X<4.2)continue;
      double deviation=Math.Abs(beam.A.Y-2.4);if(deviation>=closest)continue;
      closest=deviation;position=beam.A+dir*((4.2-beam.A.X)/dir.X);found=true;
     }
     if(!found)throw new InvalidOperationException("Spectral chamber exit missing: "+legacy.Id);
     targets.Add(new Goal(position,band){Radius=.055,Threshold=.2});
    }
    authored.Goals=targets.ToArray();
    authored.Lesson="Kırmızıyı girişte ayır; kapılardan geçen spektrumu prizmayla iki hedefe dağıt.";
    authored.Hint="Kırmızı seçici ilk hedefi besler. Dört yansımadan sonra prizma mor ve sarıyı ayrı alıcılara yollar.";
   }
   var transformed=Transform(authored,variant%8,new V(),legacy.Id,legacy.Name,authored.Hint);
   transformed.Chapter=legacy.Chapter;transformed.Difficulty=legacy.Difficulty;transformed.Par=authored.Solution.Length;
   levels[index]=transformed;
  }
 }
 static void AddGate(List<Wall> walls,double x,double y,double halfOpening,int cluster){
  walls.Add(new Wall(new V(x,-4.9),new V(x,y-halfOpening),Wall.DefaultThickness,"gate",cluster));
  walls.Add(new Wall(new V(x,y+halfOpening),new V(x,4.9),Wall.DefaultThickness,"gate",cluster));
 }
 static void AddSpatialConstraints(List<Level> levels){
  for(int index=0;index<levels.Count;index++){
   var level=levels[index];
   if(index<80){
    // Retain purposeful seed baffles and replace all post-solution filler with
    // occluders which physically cut a source/receiver or optic/receiver shortcut.
    var walls=new List<Wall>();
    foreach(var wall in level.Walls){var tagged=wall;tagged.Purpose="baffle";tagged.Cluster=walls.Count+1;walls.Add(tagged);}
    level.Walls=walls.ToArray();
    int target=index==0?0:index<10?1:index<30?2:index<50?3:index<70?4:5;
    var probes=new List<V>{level.Source};foreach(var piece in level.Solution)probes.Add(piece.Position);
    for(int probe=0;probe<probes.Count&&walls.Count<target;probe++)foreach(var goal in level.Goals){
     V a=probes[probe],b=goal.Position;V direction=(b-a).Unit;
     bool alreadyBlocked=false;foreach(var wall in walls)if(WallGeometry.Blocks(wall,a,b))alreadyBlocked=true;
     if(alreadyBlocked)continue;
     for(int step=2;step<=8&&walls.Count<target;step++){
      V center=a+(b-a)*(step/10.0);
      for(double length=3.2;length>=1.6;length-=.4){
       var candidate=new Wall(center-direction.Perp*(length*.5),center+direction.Perp*(length*.5),Wall.DefaultThickness,"shortcut occluder",walls.Count+1);
       bool clear=true;
       if(WallGeometry.Distance(level.Source,candidate)<.5)clear=false;
       if(!WallGeometry.Blocks(candidate,a,b))clear=false;
       foreach(var receiver in level.Goals)if(WallGeometry.Distance(receiver.Position,candidate)<receiver.Radius+.15)clear=false;
       foreach(var piece in level.Solution)if(WallGeometry.Distance(piece.Position,candidate)<PlacementRules.Clearance(piece.Kind)+.10)clear=false;
       foreach(var wall in walls)if(SegmentDistance(candidate,wall)<.4)clear=false;
       if(!clear)continue;
       walls.Add(candidate);level.Walls=walls.ToArray();
       var solved=Optics.Solve(level,level.Solution);
       if(!solved.Complete||solved.Truncated){walls.RemoveAt(walls.Count-1);level.Walls=walls.ToArray();continue;}
       break;
      }
     }
    }
    level.Walls=walls.ToArray();
    for(int w=level.Walls.Length-1;w>=0;w--){
     V from,to;if(SpatialValidation.TryWitness(level,w,out from,out to))continue;
     walls.RemoveAt(w);level.Walls=walls.ToArray();
    }
   }
   ValidateKnownSolution(level);
  }
 }
 static void ValidateKnownSolution(Level level){
  var session=new Session(level);
  foreach(var piece in level.Solution){
   if(!session.Place(piece.Kind,piece.Position))throw new InvalidOperationException("Spatial geometry blocks placement: "+level.Id);
   session.Pieces[session.Pieces.Count-1].Angle=piece.Angle;
  }
  var solved=Optics.Solve(level,session.Pieces);
  if(solved.Truncated||!session.IsComplete(solved))throw new InvalidOperationException("Spatial geometry blocks solution: "+level.Id+" active="+solved.ActivePieceCount+" energies="+string.Join(",",solved.Energy));
 }
 static double PointSegmentDistance(V point,Wall wall){
  V edge=wall.B-wall.A;double squared=V.Dot(edge,edge);
  double t=squared<1e-12?0:Math.Max(0,Math.Min(1,V.Dot(point-wall.A,edge)/squared));
  return (point-(wall.A+edge*t)).Length;
 }
 static double SegmentDistance(Wall a,Wall b){
  V da=a.B-a.A,db=b.B-b.A;double denominator=V.Cross(da,db);
  if(Math.Abs(denominator)>1e-10){double t=V.Cross(b.A-a.A,db)/denominator,u=V.Cross(b.A-a.A,da)/denominator;if(t>=0&&t<=1&&u>=0&&u<=1)return 0;}
  return Math.Min(Math.Min(PointSegmentDistance(a.A,b),PointSegmentDistance(a.B,b)),Math.Min(PointSegmentDistance(b.A,a),PointSegmentDistance(b.B,a)));
 }
 static Level[] BaseSeeds()=>new[]{
  new Level {
   Id="reflection-01",Name="İlk yansıma",Lesson="Işığa yeni bir yön ver.",
   Hint="Aynayı ışığın üzerine koy. 45° ile yukarı yansıt.",
   Source=new V(-4,-2),Direction=new V(1,0),Stock=new[]{Kind.Mirror},
   Goals=new[]{new Goal(new V(0,3),-1)},
   Solution=new[]{new Piece(Kind.Mirror,new V(0,-2),45)}
  },
  new Level {
   Id="dispersion-01",Name="Beyazın içindeki",Lesson="Prizma ile renkleri ortaya çıkar.",
   Hint="Prizmayı ışığın üzerine taşı ve renk yelpazesini hedefe çevir.",
   Source=new V(-4,0),Direction=new V(1,0),Stock=new[]{Kind.Prism},
   Goals=new[]{new Goal(new V(3,-2),3)},
   Solution=new[]{new Piece(Kind.Prism,new V(0,0),90)}
  },
  new Level {
   Id="selective-green-01",Name="Bir ışık, iki yol",Lesson="Yeşili ayır, kırmızıyı geçir.",
   Hint="Yeşil seçici ayna yeşili yansıtır, diğer renkleri geçirir.",
   Source=new V(-4,0),Direction=new V(1,0),Stock=new[]{Kind.Green},
   Goals=new[]{new Goal(new V(-1,3),3),new Goal(new V(3,0),6)},
   Solution=new[]{new Piece(Kind.Green,new V(-1,0),45)}
  },
  new Level {
   Id="focus-01",Name="Odak noktası",Lesson="Geniş ışığı küçük bir hedefte topla.",
   Hint="Lensi dik tut. Odak noktası lensten 2,4 birim uzakta.",
   Source=new V(-4,0),Direction=new V(1,0),Width=1.2,Stock=new[]{Kind.Lens},
   Goals=new[]{new Goal(new V(2.4,0),-1){Radius=0.17,Threshold=0.8}},
   Solution=new[]{new Piece(Kind.Lens,new V(0,0),90)}
  },
  new Level {
   Id="garden-01",Name="Işık bahçesi",Lesson="Üç parçayla iki hedefi birlikte aydınlat.",
   Hint="Önce aynayla yukarı dön. Yeşili sağa ayır, kalan ışığı lensle topla.",
   Source=new V(-4,-3),Direction=new V(1,0),Width=0.75,
   Stock=new[]{Kind.Mirror,Kind.Green,Kind.Lens},
   Goals=new[]{new Goal(new V(2.5,0),3),new Goal(new V(-2,3.4),6){Radius=0.19,Threshold=0.75}},
   Walls=new[]{new Wall(new V(-0.5,-4),new V(-0.5,-1))},
   Solution=new[]{new Piece(Kind.Mirror,new V(-2,-3),45),new Piece(Kind.Green,new V(-2,0),45),new Piece(Kind.Lens,new V(-2,1),0)}
  },
  new Level {
   Id="selective-red-01",Name="Kızıl yankı",Lesson="Kırmızı ışığı ayır, kalanı tayfa böl.",
   Hint="Kırmızı seçiciyle kırmızıyı yukarı gönder. Prizmayla yeşili hedefe kır.",
   Source=new V(-4,0),Direction=new V(1,0),Width=0.35,
   Stock=new[]{Kind.Red,Kind.Prism},
   Goals=new[]{new Goal(new V(-1,3),6){Radius=0.45,Threshold=0.4},new Goal(new V(4,-2),3){Radius=0.45,Threshold=0.2}},
   Solution=new[]{new Piece(Kind.Red,new V(-1,0),45),new Piece(Kind.Prism,new V(1,0),90)}
  },
  new Level {
   Id="water-01",Name="Suyun hafızası",Lesson="Işık suya girdiğinde yön değiştirir.",
   Hint="Aynayla ışığı suya yönlendir. Küre ile hedefe odakla.",
   Source=new V(-4,2),Direction=new V(1,0),Width=0.4,
   Stock=new[]{Kind.Mirror,Kind.Sphere},
   WaterZones=new[]{new WaterZone(new V(-1.5,-3),new V(3,0.5),1.333)},
   Walls=new[]{new Wall(new V(0,0.5),new V(0,3.5))},
   Goals=new[]{new Goal(new V(1.5,-2.3),-1){Radius=0.35,Threshold=0.4}},
   Solution=new[]{new Piece(Kind.Mirror,new V(-2,2),337.5),new Piece(Kind.Sphere,new V(1.0,-1.5),0)}
  },
  new Level {
   Id="symphony-01",Name="Optik senfoni",Lesson="Dört parça, üç hedef, tek bir uyum.",
   Hint="Aynayla yukarı dön. Kırmızı seçiciyle kırmızıyı suya ayır. Prizmayla yeşil ve maviyi ayır, küreyle odakla.",
   Source=new V(-4,-2.5),Direction=new V(1,0),Width=0.4,
   Stock=new[]{Kind.Mirror,Kind.Red,Kind.Prism,Kind.Sphere},
   Walls=new[]{new Wall(new V(-0.5,-4),new V(-0.5,-0.8)),new Wall(new V(0.5,0.5),new V(0.5,3.5))},
   WaterZones=new[]{new WaterZone(new V(0.0,-1.5),new V(3.5,0.5),1.333)},
   Goals=new[]{
    new Goal(new V(2.3,-0.5),6){Radius=0.35,Threshold=0.35},
    new Goal(new V(-3.0,2.3),3){Radius=0.45,Threshold=0.2},
    new Goal(new V(-0.3,2.8),1){Radius=0.45,Threshold=0.2}},
   Solution=new[]{
    new Piece(Kind.Mirror,new V(-2,-2.5),45),
    new Piece(Kind.Red,new V(-2,-0.5),45),
    new Piece(Kind.Sphere,new V(1.3,-0.5),0),
    new Piece(Kind.Prism,new V(-2,1.2),90)}
  }
 };

 static Level MirrorChain(string id,string name,V source,V[] points,V goal,int mirrorCount){
  if(points==null||points.Length!=mirrorCount)throw new ArgumentException("Mirror chain point count mismatch.");
  var solution=new Piece[mirrorCount];
  var stock=new Kind[mirrorCount];
  for(int i=0;i<mirrorCount;i++){
   V incoming=i==0?points[i]-source:points[i]-points[i-1];
   V outgoing=i==mirrorCount-1?goal-points[i]:points[i+1]-points[i];
   solution[i]=new Piece(Kind.Mirror,points[i],MirrorLineAngle(incoming,outgoing));
   stock[i]=Kind.Mirror;
  }
  return new Level{
   Id=id,Name=name,
   Lesson="Geliş ve çıkış doğrultularının açıortayını aynanın doğrultusu yap.",
   Hint="Her aynada α = (geliş açısı + çıkış açısı) / 2 ilişkisini düşün. Bu deneyde bütün aynalar ışık yolunda aktif olmalı.",
   Source=source,Direction=(points[0]-source).Unit,Width=0.08,
   Stock=stock,Goals=new[]{new Goal(goal,-1){Radius=0.34,Threshold=0.55}},
   Solution=solution,RequireAllPiecesActive=true
  };
 }

 static double MirrorLineAngle(V incoming,V outgoing){
  double a=Math.Atan2(incoming.Y,incoming.X)*180/Math.PI;
  double b=Math.Atan2(outgoing.Y,outgoing.X)*180/Math.PI;
  while(b-a>180)b-=360;
  while(b-a<=-180)b+=360;
  return Normalize((a+b)*0.5);
 }

 static void AddVariants(List<Level> levels,Level seed,int count){
  if(count<1||count>10)throw new ArgumentOutOfRangeException(nameof(count));
  for(int i=0;i<count;i++){
   int symmetry=i<8?i:0;
   V offset=i==8?new V(.14,.18):i==9?new V(-.14,-.18):new V(0,0);
   bool preserve=i==0;
   string id=preserve?seed.Id:seed.Id+"-v"+(i+1).ToString("00");
   string name=preserve?seed.Name:seed.Name+" · "+VariantLabels[i];
   string hint=preserve?seed.Hint:"Işığın her etkileşimden sonraki yönünü izle; önce parçaların sırasını, sonra açılarını çöz.";
   levels.Add(Transform(seed,symmetry,offset,id,name,hint));
  }
 }

 static Level Transform(Level source,int symmetry,V offset,string id,string name,string hint){
  var result=new Level{
   Id=id,Name=name,Lesson=source.Lesson,Hint=hint,
   Source=T(source.Source,symmetry)+offset,
   Direction=TDir(source.Direction,symmetry),
   Width=source.Width,
   Stock=Copy(source.Stock),
   Initial=TransformPieces(source.Initial,symmetry,offset),
   Solution=TransformPieces(source.Solution,symmetry,offset),
   Goals=TransformGoals(source.Goals,symmetry,offset),
   Walls=TransformWalls(source.Walls,symmetry,offset),
   WaterZones=TransformWater(source.WaterZones,symmetry,offset),
   RequireAllPiecesActive=source.RequireAllPiecesActive
  };
  return result;
 }

 static Piece[] TransformPieces(Piece[] source,int symmetry,V offset){
  if(source==null||source.Length==0)return Array.Empty<Piece>();
  var result=new Piece[source.Length];
  for(int i=0;i<source.Length;i++)result[i]=new Piece(source[i].Kind,T(source[i].Position,symmetry)+offset,TAngle(source[i].Angle,symmetry));
  return result;
 }

 static Goal[] TransformGoals(Goal[] source,int symmetry,V offset){
  if(source==null||source.Length==0)return Array.Empty<Goal>();
  var result=new Goal[source.Length];
  for(int i=0;i<source.Length;i++){
   var g=source[i];
   result[i]=new Goal(T(g.Position,symmetry)+offset,g.Band){Radius=g.Radius,Threshold=g.Threshold};
  }
  return result;
 }

 static Wall[] TransformWalls(Wall[] source,int symmetry,V offset){
  if(source==null||source.Length==0)return Array.Empty<Wall>();
  var result=new Wall[source.Length];
  for(int i=0;i<source.Length;i++)result[i]=new Wall(T(source[i].A,symmetry)+offset,T(source[i].B,symmetry)+offset,WallGeometry.Thickness(source[i]),source[i].Purpose,source[i].Cluster);
  return result;
 }

 static WaterZone[] TransformWater(WaterZone[] source,int symmetry,V offset){
  if(source==null||source.Length==0)return Array.Empty<WaterZone>();
  var result=new WaterZone[source.Length];
  for(int i=0;i<source.Length;i++){
   V a=T(source[i].Min,symmetry)+offset,b=T(source[i].Max,symmetry)+offset;
   result[i]=new WaterZone(new V(Math.Min(a.X,b.X),Math.Min(a.Y,b.Y)),new V(Math.Max(a.X,b.X),Math.Max(a.Y,b.Y)),source[i].Index);
  }
  return result;
 }

 static Kind[] Copy(Kind[] source){
  if(source==null||source.Length==0)return Array.Empty<Kind>();
  var result=new Kind[source.Length];Array.Copy(source,result,source.Length);return result;
 }

 static V TDir(V p,int symmetry)=>T(p,symmetry);
 static V T(V p,int symmetry){
  switch(symmetry){
   case 1:return new V(-p.Y,p.X);
   case 2:return new V(-p.X,-p.Y);
   case 3:return new V(p.Y,-p.X);
   case 4:return new V(p.X,-p.Y);
   case 5:return new V(-p.X,p.Y);
   case 6:return new V(p.Y,p.X);
   case 7:return new V(-p.Y,-p.X);
   default:return p;
  }
 }

 static double TAngle(double angle,int symmetry){
  switch(symmetry){
   case 1:return Normalize(angle+90);
   case 2:return Normalize(angle+180);
   case 3:return Normalize(angle+270);
   case 4:return Normalize(-angle);
   case 5:return Normalize(180-angle);
   case 6:return Normalize(90-angle);
   case 7:return Normalize(270-angle);
   default:return Normalize(angle);
  }
 }

 static double Normalize(double angle)=>(angle%360+360)%360;
}
}
