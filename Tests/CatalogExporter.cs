using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Prism;

public static class CatalogExporter {
    // Rewrite only walls and revision. Preserve all Unity metadata, stable IDs,
    // optical components and other serialized authoring data verbatim.
    public static string Rebuild(string source, Level[] levels, string revision) {
        var boundaries=Regex.Matches(source,@"(?m)^  - id: ");
        if(boundaries.Count!=levels.Length)throw new InvalidOperationException("Catalog entry count mismatch");
        var result=new StringBuilder(source.Length+4096);
        result.Append(source.Substring(0,boundaries[0].Index));
        for(int i=0;i<boundaries.Count;i++){
            int from=boundaries[i].Index;
            int to=i+1<boundaries.Count?boundaries[i+1].Index:source.Length;
            string section=source.Substring(from,to-from);
            var id=Regex.Match(section,@"^  - id: (.*)$",RegexOptions.Multiline);
            if(!id.Success||id.Groups[1].Value.Trim()!=levels[i].Id)
                throw new InvalidOperationException("Catalog ID order mismatch at "+i);
            string walls=SerializeWalls(levels[i].Walls);
            int changed=0;
            section=Regex.Replace(section,@"(?ms)^    walls:.*?(?=^    waterZones:)",m=>{changed++;return walls;});
            if(changed!=1)throw new InvalidOperationException("Missing walls section in "+levels[i].Id);
            result.Append(section);
        }
        string updated=result.ToString();
        int revisionCount=0;
        updated=Regex.Replace(updated,@"(?m)^  campaignRevision:.*$",m=>{
            revisionCount++;return "  campaignRevision: "+revision;
        });
        if(revisionCount!=1)throw new InvalidOperationException("Missing catalog revision");
        return updated;
    }
    static string N(double value)=>((float)value).ToString("R",CultureInfo.InvariantCulture);
    static string SerializeWalls(Wall[] walls){
        if(walls==null||walls.Length==0)return "    walls: []\n";
        var b=new StringBuilder("    walls:\n");
        foreach(var w in walls){
            b.Append("    - a: {x: ").Append(N(w.A.X)).Append(", y: ").Append(N(w.A.Y)).Append("}\n");
            b.Append("      b: {x: ").Append(N(w.B.X)).Append(", y: ").Append(N(w.B.Y)).Append("}\n");
            b.Append("      thickness: ").Append(N(WallGeometry.Thickness(w))).Append("\n");
            b.Append("      purpose: ").Append(w.Purpose??"").Append("\n");
            b.Append("      cluster: ").Append(w.Cluster.ToString(CultureInfo.InvariantCulture)).Append("\n");
        }
        return b.ToString();
    }
}
