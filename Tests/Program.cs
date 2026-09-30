using System;
using System.Collections.Generic;
using System.IO;
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
        Check("spectral target labels are color-independent", PieceInfo.BandName(-1)=="Beyaz"&&PieceInfo.BandName(1)=="Mavi"&&PieceInfo.BandName(3)=="Yeşil"&&PieceInfo.BandName(6)=="Kırmızı");
        var sphereTest=Optics.Solve(new Level{Source=new V(-3,0),Direction=new V(1,0),Width=0.6,Goals=new[]{new Goal(new V(0.95,0),-1){Radius=0.3,Threshold=0.5}}}, new[]{new Piece(Kind.Sphere,new V(0,0),0)});
        Check("sphere focuses wide beam onto focal goal", sphereTest.Complete);
        var waterLevel=new Level{Source=new V(-3,1),Direction=new V(1,0),Goals=new[]{new Goal(new V(3,1),-1)},WaterZones=new[]{new WaterZone(new V(-1,-2),new V(1,2),1.333)}};
        Check("normal-incidence through water reaches goal", Optics.Solve(waterLevel,new Piece[0]).Complete);
        string repoRoot=Directory.GetCurrentDirectory();
        if(!Directory.Exists(Path.Combine(repoRoot,"Assets")))repoRoot=Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"../../../.."));
        bool sourceHygiene=true;
        string prismSource=Path.Combine(repoRoot,"Assets","Prism");
        if(Directory.Exists(prismSource)){
            foreach(var path in Directory.GetFiles(prismSource,"*.cs",SearchOption.AllDirectories)){
                if(File.ReadAllText(path).Contains("\\n")){sourceHygiene=false;Console.WriteLine("INFO literal patch newline in "+path);}
            }
        }else sourceHygiene=false;
        Check("Unity C# sources contain no literal patch-newline artifacts",sourceHygiene);
        bool conflictFree=true,validUnityMeta=true;
        foreach(var path in Directory.GetFiles(prismSource,"*",SearchOption.AllDirectories)){
            string extension=Path.GetExtension(path);
            if(extension!=".cs"&&extension!=".shader"&&extension!=".meta")continue;
            string content=File.ReadAllText(path);
            if(System.Text.RegularExpressions.Regex.IsMatch(content,@"(?m)^(<<<<<<< |=======|>>>>>>> )"))conflictFree=false;
            if(extension==".meta"&&!System.Text.RegularExpressions.Regex.IsMatch(content,@"(?m)^guid: [0-9a-f]{32}\r?$"))validUnityMeta=false;
        }
        Check("Unity source and shaders contain no merge markers",conflictFree);
        Check("Unity metadata has valid standalone GUID lines",validUnityMeta);

        string buildSourcePath=Path.Combine(repoRoot,"Assets","Prism","Editor","BuildProject.cs");
        string buildSource=File.Exists(buildSourcePath)?File.ReadAllText(buildSourcePath):"";
        Check("Android release contract targets API 36",buildSource.Contains("AndroidApiLevel36"));
        Check("Android release contract is ARM64",buildSource.Contains("AndroidArchitecture.ARM64"));
        Check("Android release contract builds app bundle",buildSource.Contains("buildAppBundle=true"));
        Check("Android release signing comes from environment",buildSource.Contains("PRISM_KEYSTORE_PATH")&&buildSource.Contains("PRISM_KEY_ALIAS_PASS"));

        string ignorePath=Path.Combine(repoRoot,".gitignore");
        string ignore=File.Exists(ignorePath)?File.ReadAllText(ignorePath):"";
        Check("generated release assets stay out of source control",ignore.Contains("Builds/")&&ignore.Contains("Assets/Prism/Generated/"));

        var curriculumTimer=System.Diagnostics.Stopwatch.StartNew();
        var levels=Levels.Create();
        curriculumTimer.Stop();
        Console.WriteLine("INFO Desktop curriculum generation: "+curriculumTimer.Elapsed.TotalMilliseconds.ToString("F0")+" ms (not a mobile benchmark)");
        Check("one hundred levels", levels.Length==100);
        string bakedPath=Path.Combine(repoRoot,"Assets","Prism","Resources","LevelCatalog.asset");
        string baked=File.Exists(bakedPath)?File.ReadAllText(bakedPath):"";
        int bakedCount=System.Text.RegularExpressions.Regex.Matches(baked,@"(?m)^  - id: ").Count;
        Check("build includes versioned one hundred level catalog",bakedCount==100&&baked.Contains("campaignRevision: 2026-09-30-brick-obstacles-v1"));
        var ids=new HashSet<string>();
        bool validIds=true;
        foreach(var level in levels){if(string.IsNullOrWhiteSpace(level.Id)||!ids.Add(level.Id))validIds=false;}
        Check("level IDs are stable and unique",validIds);
        bool chapterStructure=true,difficultyProgression=true;
        var chapterCounts=new Dictionary<string,int>();
        for(int i=0;i<levels.Length;i++){
            var level=levels[i];
            if(string.IsNullOrWhiteSpace(level.Chapter))chapterStructure=false;
            if(!chapterCounts.ContainsKey(level.Chapter))chapterCounts[level.Chapter]=0;
            chapterCounts[level.Chapter]++;
            if(level.Difficulty!=1+i/10)difficultyProgression=false;

            var initial=Optics.Solve(level, level.Initial);
            Check(level.Name+" begins unsolved", !initial.Complete);

            var result=Optics.Solve(level,level.Solution);
            Check(level.Name+" known solution reaches goals",result.Complete);
            Check(level.Name+" solver stays within budget",!result.Truncated);

            var solvedSession=new Session(level);
            solvedSession.Reveal();
            Check(level.Name+" gameplay completion rules pass",solvedSession.IsComplete(Optics.Solve(level,solvedSession.Pieces)));
        }
        foreach(var pair in chapterCounts)if(pair.Value!=10)chapterStructure=false;
        Check("ten chapters contain ten levels each",chapterStructure&&chapterCounts.Count==10);
        Check("difficulty rises once per chapter",difficultyProgression);

        bool curriculumMetadata=true,masteryStructure=true,boundsValid=true;
        for(int i=0;i<levels.Length;i++){
            var level=levels[i];
            if(level.Par!=Math.Max(1,level.Solution.Length))curriculumMetadata=false;
            if(Math.Abs(level.Source.X)>4.5||Math.Abs(level.Source.Y)>4.5)boundsValid=false;
            foreach(var goal in level.Goals)if(Math.Abs(goal.Position.X)>4.5||Math.Abs(goal.Position.Y)>4.5)boundsValid=false;
            foreach(var piece in level.Solution)if(Math.Abs(piece.Position.X)>4.25||Math.Abs(piece.Position.Y)>4.25)boundsValid=false;
            if(i>=80&&!level.RequireAllPiecesActive)masteryStructure=false;
            if(i>=80&&i<90&&level.Solution.Length<5)masteryStructure=false;
            if(i>=90&&level.Solution.Length<6)masteryStructure=false;
        }
        Check("curriculum par metadata matches known solutions",curriculumMetadata);
        Check("final 20 levels enforce multi-piece geometry mastery",masteryStructure);
        bool wallProgression=true;
        for(int i=1;i<levels.Length;i++){
            int expected=i<20?1:i<50?2:i<80?3:i<90?4:5;
            if(levels[i].Walls.Length<expected)wallProgression=false;
        }
        Check("brick wall obstacles grow through the campaign",wallProgression);
        var masteryGeometry=new HashSet<string>();
        for(int i=80;i<levels.Length;i++){
            var points=new List<V>{levels[i].Source};
            foreach(var piece in levels[i].Solution)points.Add(piece.Position);
            points.Add(levels[i].Goals[0].Position);
            var turns=new List<string>();
            for(int j=1;j<points.Count-1;j++){
                V incoming=(points[j]-points[j-1]).Unit;
                V outgoing=(points[j+1]-points[j]).Unit;
                double angle=Math.Atan2(V.Cross(incoming,outgoing),V.Dot(incoming,outgoing))*180/Math.PI;
                turns.Add(Math.Round(Math.Abs(angle)/5).ToString());
            }
            masteryGeometry.Add(string.Join("-",turns));
        }
        Check("final 20 levels have distinct turn geometry",masteryGeometry.Count==20);
        Check("sources goals and known solutions stay inside playable bounds",boundsValid);

        bool solutionsRespectPlacement=true;
        foreach(var level in levels) {
            var placementSession=new Session(level);
            foreach(var piece in level.Solution) {
                if(!placementSession.Place(piece.Kind,piece.Position)){solutionsRespectPlacement=false;break;}
                placementSession.Pieces[placementSession.Pieces.Count-1].Angle=piece.Angle;
            }
            if(!solutionsRespectPlacement)break;
            var placedResult=Optics.Solve(level,placementSession.Pieces);
            if(!placementSession.IsComplete(placedResult)){solutionsRespectPlacement=false;break;}
        }
        Check("all 100 solutions obey placement and gameplay completion rules",solutionsRespectPlacement);
        var wallLevel=new Level { Source=new V(-3,0), Direction=new V(1,0), Goals=new[]{new Goal(new V(3,0),-1)}, Walls=new[]{new Wall(new V(0,-2),new V(0,2))} };
        Check("wall blocks all target energy", !Optics.Solve(wallLevel,new Piece[0]).Complete);
        var placementLevel=new Level {
            Source=new V(-3,0),
            Stock=new[]{Kind.Mirror,Kind.Mirror},
            Goals=new[]{new Goal(new V(3,0),-1)},
            Walls=new[]{new Wall(new V(0,-1),new V(0,1))}
        };
        var ruleSession=new Session(placementLevel);
        Check("placement blocks source overlap",!ruleSession.Place(Kind.Mirror,new V(-3,0)));
        Check("placement blocks goal overlap",!ruleSession.Place(Kind.Mirror,new V(3,0)));
        Check("placement blocks wall overlap",!ruleSession.Place(Kind.Mirror,new V(0,0)));
        Check("placement accepts open board space",ruleSession.Place(Kind.Mirror,new V(-1.2,1.5)));
        Check("placement blocks piece overlap",!ruleSession.Place(Kind.Mirror,new V(-1.1,1.5)));
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

        var reuseLevel=new Level{Source=new V(-3,0),Direction=new V(1,0),Goals=new[]{new Goal(new V(3,0),-1)}};
        var reuseResult=Optics.Solve(reuseLevel,new Piece[0]);
        int firstBeamCount=reuseResult.Beams.Count;
        reuseResult.Energy[0]=999;
        var reused=Optics.Solve(reuseLevel,new Piece[0],reuseResult);
        Check("solver reuses result object",object.ReferenceEquals(reuseResult,reused));
        Check("solver clears reusable energy buffers",Math.Abs(reused.Energy[0]-1)<1e-6);
        Check("solver clears and rebuilds reusable beam buffers",reused.Beams.Count==firstBeamCount);
        Check("solver reusable active-piece state resets",reused.ActivePieceCount==0);

        var timer=System.Diagnostics.Stopwatch.StartNew();for(int i=0;i<100;i++)Optics.Solve(levels[79],levels[79].Solution);timer.Stop();Console.WriteLine("INFO Desktop core mean solve (chapter 8): "+(timer.Elapsed.TotalMilliseconds/100).ToString("F3")+" ms (not a mobile benchmark)");
        Environment.ExitCode=failures==0?0:1;
    }
}
