using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Threading;
partial class Shelf {
 class IndexedBranch {public long Stamp;public List<FoundGame> Games;}
 class IndexedRoot {public string[] Files;public List<FoundGame> Direct;public Dictionary<string,IndexedBranch> Branches;}
 Dictionary<string,IndexedRoot> discoveryIndex=new Dictionary<string,IndexedRoot>(StringComparer.OrdinalIgnoreCase);
 readonly object discoveryLock=new object(),dirtyLock=new object();
 HashSet<string> dirtyPaths=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
 List<FileSystemWatcher> discoveryWatchers=new List<FileSystemWatcher>();
 DispatcherTimer discoveryDebounce;
 bool forceFullDiscovery,discoveryNotificationPosted;
 DateTime lastDiscoveryChange;
 int indexReusedBranches,indexScannedBranches,indexEntryChecks;
 static bool UnderDirectory(string path,string directory){return path.Equals(directory,StringComparison.OrdinalIgnoreCase)||path.StartsWith(directory.TrimEnd('\\')+"\\",StringComparison.OrdinalIgnoreCase);}
 static string[] EntryFiles(string root){return SafeFiles(root).Where(p=>new[]{".exe",".lnk",".url"}.Contains(Path.GetExtension(p).ToLowerInvariant())).OrderBy(p=>p,StringComparer.OrdinalIgnoreCase).ToArray();}
 void SeedDiscoveryIndex(LibrarySnapshot snapshot){
  if(snapshot.Index!=null){discoveryIndex=new Dictionary<string,IndexedRoot>(snapshot.Index,StringComparer.OrdinalIgnoreCase);return;}
  // Migrate the previous snapshot without revisiting its thousands of subdirectories.
  foreach(string root in MinimalScanRoots(snapshot.Roots)){
   DirectoryInventory inventory;if(snapshot.Directories==null||!snapshot.Directories.TryGetValue(root,out inventory)||inventory.Children==null||inventory.Files==null)continue;
   var index=new IndexedRoot{Files=inventory.Files.OrderBy(p=>p,StringComparer.OrdinalIgnoreCase).ToArray(),Direct=snapshot.Games.Where(g=>!inventory.Children.Any(c=>UnderDirectory(g.Path,c))).ToList(),Branches=new Dictionary<string,IndexedBranch>(StringComparer.OrdinalIgnoreCase)};
   foreach(string child in inventory.Children){DirectoryInventory old;index.Branches[child]=new IndexedBranch{Stamp=snapshot.Directories.TryGetValue(child,out old)?old.Stamp:long.MinValue,Games=snapshot.Games.Where(g=>UnderDirectory(g.Path,child)).ToList()};}
   discoveryIndex[root]=index;
  }
 }
 List<FoundGame> DiscoverIndexedRoot(string root){
  root=NormalizeScanRoot(root);
  lock(discoveryLock){
   string[] changed;lock(dirtyLock){changed=dirtyPaths.Where(p=>UnderDirectory(p,root)).ToArray();foreach(string p in changed)dirtyPaths.Remove(p);}
   IndexedRoot index;string[] files=EntryFiles(root);string[] children=SafeDirectories(root);
   bool standalone=!ContainerDirectory(root)&&!root.Equals(Path.GetPathRoot(root),StringComparison.OrdinalIgnoreCase)&&files.Any(p=>p.EndsWith(".exe",StringComparison.OrdinalIgnoreCase)&&!UtilityExe(p));
   if(forceFullDiscovery||standalone||!discoveryIndex.TryGetValue(root,out index)||index.Branches==null||index.Files==null||!index.Files.SequenceEqual(files,StringComparer.OrdinalIgnoreCase)){
    var found=DiscoverGames(root,options.ScanShortcuts);indexScannedBranches++;
    index=new IndexedRoot{Files=files,Direct=found.Where(g=>!children.Any(c=>UnderDirectory(g.Path,c))).ToList(),Branches=new Dictionary<string,IndexedBranch>(StringComparer.OrdinalIgnoreCase)};
    foreach(string child in children)index.Branches[child]=new IndexedBranch{Stamp=Directory.GetLastWriteTimeUtc(child).Ticks,Games=found.Where(g=>UnderDirectory(g.Path,child)).ToList()};
    discoveryIndex[root]=index;return found;
   }
   var result=new List<FoundGame>(index.Direct??new List<FoundGame>());
   var next=new Dictionary<string,IndexedBranch>(StringComparer.OrdinalIgnoreCase);
   foreach(string child in children){
    IndexedBranch branch;long stamp=Directory.GetLastWriteTimeUtc(child).Ticks;
    bool reuse=index.Branches.TryGetValue(child,out branch)&&branch!=null&&branch.Games!=null&&branch.Stamp==stamp&&!changed.Any(p=>UnderDirectory(p,child)||p.Equals(root,StringComparison.OrdinalIgnoreCase));
    if(reuse)foreach(var game in branch.Games){indexEntryChecks++;if(!File.Exists(game.Path)){reuse=false;break;}}
    if(!reuse){foreach(string key in directoryInventory.Keys.Where(k=>UnderDirectory(k,child)).ToArray())directoryInventory.Remove(key);branch=new IndexedBranch{Stamp=stamp,Games=DiscoverGames(root,options.ScanShortcuts,child)};indexScannedBranches++;}
    else indexReusedBranches++;
    next[child]=branch;result.AddRange(branch.Games);
   }
   index.Branches=next;return DedupeGames(result).OrderBy(g=>g.Path,StringComparer.OrdinalIgnoreCase).ToList();
  }
 }
 void QueueDiscoveryChange(string path){
  lock(dirtyLock){dirtyPaths.Add(path);lastDiscoveryChange=DateTime.UtcNow;if(discoveryNotificationPosted)return;discoveryNotificationPosted=true;}
  if(closing||Dispatcher.HasShutdownStarted)return;
  Dispatcher.BeginInvoke(new Action(()=>{if(closing)return;if(discoveryDebounce==null){discoveryDebounce=new DispatcherTimer{Interval=TimeSpan.FromSeconds(2)};discoveryDebounce.Tick+=(o,e)=>{discoveryDebounce.Stop();lock(dirtyLock){if(scanning||(DateTime.UtcNow-lastDiscoveryChange).TotalSeconds<2){discoveryDebounce.Start();return;}discoveryNotificationPosted=false;}nextDeepScan=DateTime.MinValue;Scan();};}discoveryDebounce.Start();}));
 }
 void StartDiscoveryWatchers(){
  StopDiscoveryWatchers();
  foreach(string root in MinimalScanRoots(LibraryFolders()))try{
   if(!Directory.Exists(root))continue;
   foreach(string filter in new[]{"", "*.exe", "*.lnk", "*.url"}){
    if(!options.ScanShortcuts&&(filter=="*.lnk"||filter=="*.url"))continue;
    var watcher=new FileSystemWatcher(root){IncludeSubdirectories=true,NotifyFilter=filter==""?NotifyFilters.DirectoryName:NotifyFilters.FileName,Filter=filter==""?"*":filter,InternalBufferSize=32768};
    watcher.Created+=(o,e)=>QueueDiscoveryChange(e.FullPath);watcher.Deleted+=(o,e)=>QueueDiscoveryChange(e.FullPath);
    watcher.Renamed+=(o,e)=>{QueueDiscoveryChange(e.OldFullPath);QueueDiscoveryChange(e.FullPath);};
    watcher.Error+=(o,e)=>QueueDiscoveryChange(root);
    watcher.EnableRaisingEvents=true;discoveryWatchers.Add(watcher);
   }
  }catch{lock(dirtyLock)dirtyPaths.Add(root);}
 }
 void StopDiscoveryWatchers(){if(discoveryDebounce!=null)discoveryDebounce.Stop();foreach(var watcher in discoveryWatchers)watcher.Dispose();discoveryWatchers.Clear();lock(dirtyLock)discoveryNotificationPosted=false;}
 static void AuditIncrementalDiscovery(){
  var app=new System.Windows.Application();var shelf=new Shelf(true);bool restored=shelf.RestoreLibrarySnapshot();
  var before=shelf.games.Select(g=>g.Path).OrderBy(p=>p,StringComparer.OrdinalIgnoreCase).ToArray();
  var clock=System.Diagnostics.Stopwatch.StartNew();var indexed=shelf.DiscoverLibraries(shelf.LibraryFolders());double indexedMs=clock.Elapsed.TotalMilliseconds;
  int reused=shelf.indexReusedBranches,scanned=shelf.indexScannedBranches,entryChecks=shelf.indexEntryChecks;
  clock.Restart();shelf.forceFullDiscovery=true;var full=shelf.DiscoverLibraries(shelf.LibraryFolders());double fullMs=clock.Elapsed.TotalMilliseconds;shelf.forceFullDiscovery=false;
  var indexedPaths=indexed.Select(g=>g.Path).OrderBy(p=>p,StringComparer.OrdinalIgnoreCase).ToArray();var fullPaths=full.Select(g=>g.Path).OrderBy(p=>p,StringComparer.OrdinalIgnoreCase).ToArray();
  File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"incremental-audit.json"),json.Serialize(new{Restored=restored,IndexedMilliseconds=indexedMs,FullMilliseconds=fullMs,ReusedInstallations=reused,RescannedInstallations=scanned,CheckedEntries=entryChecks,IndexedCount=indexed.Count,FullCount=full.Count,MatchesFull=indexedPaths.SequenceEqual(fullPaths,StringComparer.OrdinalIgnoreCase),MissingFromIndex=fullPaths.Except(indexedPaths,StringComparer.OrdinalIgnoreCase).ToArray(),RemovedFromPrevious=before.Except(fullPaths,StringComparer.OrdinalIgnoreCase).ToArray()}));
  shelf.Close();app.Shutdown();
 }

}
