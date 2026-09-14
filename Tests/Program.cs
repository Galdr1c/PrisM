using System;
using System.Collections.Generic;
using Prism;
class Program
{
    static int failures;
    static void Check(string name, bool ok) { Console.WriteLine((ok ? "PASS " : "FAIL ") + name); if (!ok) failures++; }
    static void Main()
    {
        Check("mirror sends rightward beam upward", (Optics.Reflect(new V(1,0), new V(-1,1).Unit) - new V(0,1)).Length < 0.0001);
        V refracted;
        Check("normal-incidence refraction preserves direction", Optics.Refract(new V(1,0),new V(-1,0),1,1.5,out refracted) && (refracted-new V(1,0)).Length<0.0001);
        Check("glass-to-air critical angle reflects internally", !Optics.Refract(new V(0.5,0.8660254),new V(-1,0),1.5,1,out refracted));
        Check("blue glass refracts more than red", Optics.Index(0)>Optics.Index(6));
        Check("sphere has its own display name", PieceInfo.Name(Kind.Sphere)=="Cam küre");
        Check("sphere rotation is disabled", !PieceInfo.CanRotate(Kind.Sphere));
        Check("mirror rotation stays enabled", PieceInfo.CanRotate(Kind.Mirror));
        var sphereTest=Optics.Solve(new Level{Source=new V(-3,0),Direction=new V(1,0),Width=0.6,Goals=new[]{new Goal(new V(0.95,0),-1){Radius=0.3,Threshold=0.5}}}, new[]{new Piece(Kind.Sphere,new V(0,0),0)});
        Check("sphere focuses wide beam onto focal goal", sphereTest.Complete);
        var waterLevel=new Level{Source=new V(-3,1),Direction=new V(1,0),Goals=new[]{new Goal(new V(3,1),-1)},WaterZones=new[]{new WaterZone(new V(-1,-2),new V(1,2),1.333)}};
        Check("normal-incidence through water reaches goal", Optics.Solve(waterLevel,new Piece[0]).Complete);
        var levels=Levels.Create();
        Check("eight levels", levels.Length==8);
        var ids=new HashSet<string>();
        bool validIds=true;
        foreach(var level in levels){if(string.IsNullOrWhiteSpace(level.Id)||!ids.Add(level.Id))validIds=false;}
        Check("level IDs are stable and unique",validIds);
        foreach(var level in levels) {
            var initial=Optics.Solve(level, level.Initial);
            Check(level.Name+" begins unsolved", !initial.Complete);
            var result=Optics.Solve(level,level.Solution);
            Check(level.Name+" known solution completes",result.Complete);
            Check(level.Name+" solver stays within budget",!result.Truncated);
        }
        var wallLevel=new Level { Source=new V(-3,0), Direction=new V(1,0), Goals=new[]{new Goal(new V(3,0),-1)}, Walls=new[]{new Wall(new V(0,-2),new V(0,2))} };
        Check("wall blocks all target energy", !Optics.Solve(wallLevel,new Piece[0]).Complete);
        var session=new Session(levels[0]);
        Check("stock starts available", session.Remaining(Kind.Mirror)==1);
        Check("can place available piece", session.Place(Kind.Mirror,new V(0,-2)));
        Check("stock prevents extra pieces", !session.Place(Kind.Mirror,new V(1,1)));
        if(session.Pieces.Count>0){
            session.BeginEdit();session.Pieces[0].Angle=45;session.EndEdit();
            Check("edit completes mirror level",Optics.Solve(levels[0],session.Pieces).Complete);
            session.Undo();Check("undo restores angle",session.Pieces.Count==1&&session.Pieces[0].Angle==0);
            session.Undo();Check("undo restores inventory",session.Pieces.Count==0&&session.Remaining(Kind.Mirror)==1);
        }
        var loop=new Level{Source=new V(0,0),Direction=new V(1,0)};
        var loopResult=Optics.Solve(loop,new[]{new Piece(Kind.Mirror,new V(-1,0),90),new Piece(Kind.Mirror,new V(1,0),90)});
        Check("parallel mirrors terminate within budget",loopResult.Truncated&&loopResult.Beams.Count<=7*13*24);
        var direct=new Level{Source=new V(-3,0),Direction=new V(1,0),Goals=new[]{new Goal(new V(3,0),-1)}};
        var directResult=Optics.Solve(direct,new Piece[0]);
        Check("broad-spectrum energy is normalized",Math.Abs(directResult.Energy[0]-1)<1e-6);
        Check("green receiver rejects red-only route",Optics.Solve(new Level{Source=new V(-4,0),Direction=new V(1,0),Goals=new[]{new Goal(new V(0,3),3)}},new[]{new Piece(Kind.Red,new V(0,0),45)}).Energy[0]==0);

        var timer=System.Diagnostics.Stopwatch.StartNew();for(int i=0;i<100;i++)Optics.Solve(levels[4],levels[4].Solution);timer.Stop();Console.WriteLine("INFO Desktop core mean solve: "+(timer.Elapsed.TotalMilliseconds/100).ToString("F3")+" ms (not a mobile benchmark)");
        Environment.ExitCode=failures==0?0:1;
    }
}
