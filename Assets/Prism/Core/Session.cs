using System.Collections.Generic;
namespace Prism {
public class Session {
 public Level Level;public List<Piece> Pieces=new List<Piece>();
 readonly Stack<List<Piece>> history=new Stack<List<Piece>>(); List<Piece> before;
 static List<Piece> Copy(IEnumerable<Piece> ps){var copy=new List<Piece>();foreach(var p in ps)copy.Add(p.Copy());return copy;}
 public Session(Level l){Level=l;Pieces=Copy(l.Initial);}
 public int Remaining(Kind kind){int n=0;foreach(var k in Level.Stock)if(k==kind)n++;foreach(var p in Pieces)if(p.Kind==kind)n--;return n;}
 public bool Place(Kind kind,V position){if(Remaining(kind)<=0)return false;BeginEdit();Pieces.Add(new Piece(kind,position));EndEdit();return true;}
 public void BeginEdit(){before=Copy(Pieces);} public void EndEdit(){if(before!=null){history.Push(before);before=null;}}
 public void Undo(){if(history.Count>0)Pieces=history.Pop();before=null;}
 public void Reset(){BeginEdit();Pieces=Copy(Level.Initial);EndEdit();}
 public void Remove(int index){if(index<0||index>=Pieces.Count)return;BeginEdit();Pieces.RemoveAt(index);EndEdit();}
 public void Reveal(){BeginEdit();Pieces=Copy(Level.Solution);EndEdit();}
}
}
