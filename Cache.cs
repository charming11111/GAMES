using System;
using System.IO;
using System.Linq;
using System.Collections.Concurrent;
partial class Shelf {
 static ConcurrentDictionary<string,long> invalidCovers=new ConcurrentDictionary<string,long>();
 static int networkRequests=0;
 static bool HasCover(string path){try{var info=new FileInfo(path);long stamp;if(!info.Exists||info.Length<64)return false;return !invalidCovers.TryGetValue(path,out stamp)||stamp!=info.LastWriteTimeUtc.Ticks;}catch{return false;}}
 static void BadCover(string path){try{invalidCovers[path]=File.GetLastWriteTimeUtc(path).Ticks;}catch{}}
 static void WriteCover(string path,byte[] bytes){string temp=path+"."+Guid.NewGuid().ToString("N")+".tmp";try{File.WriteAllBytes(temp,bytes);if(File.Exists(path))File.Replace(temp,path,null);else File.Move(temp,path);long ignored;invalidCovers.TryRemove(path,out ignored);}finally{if(File.Exists(temp))File.Delete(temp);}}
 static string CacheSummary(System.Collections.Generic.IEnumerable<Game> items){int count=0;foreach(var g in items)if(HasCover(g.Cover))count++;return "本地封面 "+count+" 张 · 启动时扫描";}
 static void AuditLibrary(){var app=new System.Windows.Application();var shelf=new Shelf(true);shelf.SyncRefreshRate();var clock=System.Diagnostics.Stopwatch.StartNew();shelf.Scan();long cold=clock.ElapsedMilliseconds;clock.Restart();for(int i=0;i<5;i++)shelf.Render();double warm=clock.Elapsed.TotalMilliseconds/5;int loaded=0;var missing=new System.Collections.Generic.List<string>();foreach(var game in shelf.games){try{if(HasCover(game.Cover)&&ReadBitmap(game.Cover,400)!=null)loaded++;else missing.Add(game.Name);}catch{missing.Add(game.Name);}}File.WriteAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"audit-result.json"),json.Serialize(new{Home=home,Entries=shelf.games.Select(g=>new{Name=g.Name,Path=g.Path,Cover=g.Cover,Exists=File.Exists(g.Cover)}).ToArray(),Total=shelf.games.Count,Loaded=loaded,Missing=missing,NetworkRequests=networkRequests,ColdLoadMs=cold,WarmRefreshMs=warm,AnimationTargetHz=shelf.animationRate,RenderTier=(System.Windows.Media.RenderCapability.Tier>>16)}));shelf.Close();app.Shutdown();}
}


