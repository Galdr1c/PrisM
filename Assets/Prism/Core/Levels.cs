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
  DistinguishMasteryGeometry(levels);
  return levels.ToArray();
 }

 static void DistinguishMasteryGeometry(List<Level> levels){
  var signatures=new HashSet<string>(StringComparer.Ordinal);
  for(int index=80;index<levels.Count;index++){
   var level=levels[index];
   var original=new Piece[level.Solution.Length];
   for(int j=0;j<original.Length;j++)original[j]=level.Solution[j].Copy();
   V originalDirection=level.Direction;
   bool accepted=false;
   for(int attempt=1;attempt<=96;attempt++){
    double amount=.12+((attempt-1)%6)*.055;
    for(int j=0;j<original.Length;j++){
     var origin=original[j];
     double phase=(index-79)*1.83+j*2.71+attempt*1.37;
     V displacement=new V(Math.Sin(phase)*amount,Math.Cos(phase*1.31)*amount);
     level.Solution[j]=new Piece(Kind.Mirror,origin.Position+displacement);
    }
    var points=new V[level.Solution.Length+2];
    points[0]=level.Source;
    for(int j=0;j<level.Solution.Length;j++)points[j+1]=level.Solution[j].Position;
    points[points.Length-1]=level.Goals[0].Position;
    level.Direction=(points[1]-points[0]).Unit;
    for(int j=0;j<level.Solution.Length;j++)
     level.Solution[j].Angle=MirrorLineAngle(points[j+1]-points[j],points[j+2]-points[j+1]);
    string signature=TurnSignature(points);
    if(signatures.Contains(signature))continue;
    var session=new Session(level);
    bool valid=true;
    for(int j=0;j<level.Solution.Length;j++)
     if(!session.Place(Kind.Mirror,level.Solution[j].Position)){valid=false;break;}
    if(!valid)continue;
    for(int j=0;j<level.Solution.Length;j++)session.Pieces[j].Angle=level.Solution[j].Angle;
    if(Optics.Solve(level,level.Initial).Complete)continue;
    var solved=Optics.Solve(level,session.Pieces);
    if(solved.Truncated||!session.IsComplete(solved))continue;
    signatures.Add(signature);
    accepted=true;
    break;
   }
   if(!accepted)throw new InvalidOperationException("Could not author distinct playable mastery geometry: "+level.Id);
   level.Hint="Geliş ve çıkış ışınlarını birer doğru olarak çiz. Her aynanın açısını bu iki yönün açıortayından türet; tüm aynalar ışık yolunda aktif olmalı.";
  }
 }

 static string TurnSignature(V[] points){
  var turns=new string[points.Length-2];
  for(int j=1;j<points.Length-1;j++){
   V incoming=(points[j]-points[j-1]).Unit;
   V outgoing=(points[j+1]-points[j]).Unit;
   double angle=Math.Atan2(V.Cross(incoming,outgoing),V.Dot(incoming,outgoing))*180/Math.PI;
   turns[j-1]=Math.Round(Math.Abs(angle)/5).ToString();
  }
  return string.Join("-",turns);
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
  for(int i=0;i<source.Length;i++)result[i]=new Wall(T(source[i].A,symmetry)+offset,T(source[i].B,symmetry)+offset);
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
