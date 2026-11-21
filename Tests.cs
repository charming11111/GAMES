using System;
using System.IO;
using System.Windows;
partial class Shelf {
 [System.Runtime.InteropServices.DllImport("user32.dll")]static extern IntPtr SendMessage(IntPtr h,int m,IntPtr w,IntPtr l);
 void TestKey(int virtualKey){SendMessage(new System.Windows.Interop.WindowInteropHelper(this).Handle,0x100,(IntPtr)virtualKey,IntPtr.Zero);}
 void ColorPreview(string name){FitModal();UpdateLayout();modalCard.BeginAnimation(OpacityProperty,null);modalCard.Opacity=1;var bmp=new System.Windows.Media.Imaging.RenderTargetBitmap((int)ActualWidth,(int)ActualHeight,96,96,System.Windows.Media.PixelFormats.Pbgra32);bmp.Render(this);var enc=new System.Windows.Media.Imaging.PngBitmapEncoder();enc.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bmp));using(var f=File.Create(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),name)))enc.Save(f);}
 int checks=0;
 void Check(bool value,string name){checks++;if(!value)throw new Exception("Regression failed: "+name);}
 void TestPortableLibraryDiscovery(){string root=System.IO.Path.Combine(home,"portable-library");string first=System.IO.Path.Combine(root,"Cyberpunk 2077","bin","x64");string second=System.IO.Path.Combine(root,"Cuphead");Directory.CreateDirectory(first);Directory.CreateDirectory(second);string firstExe=System.IO.Path.Combine(first,"Cyberpunk2077.exe"),secondExe=System.IO.Path.Combine(second,"Cuphead.exe");File.WriteAllText(firstExe,"test");File.WriteAllText(secondExe,"test");Check(DiscoverGames(root).Count==2,"portable library finds independent games before adding launcher");File.WriteAllText(System.IO.Path.Combine(root,"GAMES.exe"),"launcher");directoryInventory.Clear();var found=DiscoverGames(root);Check(found.Count==2,"launcher beside installed games does not collapse the library into one game");Check(found.Exists(g=>g.Path==firstExe)&&found.Exists(g=>g.Path==secondExe),"portable library preserves both game executable paths");string container=System.IO.Path.Combine(root,"Games");Directory.CreateDirectory(System.IO.Path.Combine(container,"Game Alpha"));Directory.CreateDirectory(System.IO.Path.Combine(container,"Game Beta"));File.WriteAllText(System.IO.Path.Combine(container,"Game Alpha","Alpha.exe"),"test");File.WriteAllText(System.IO.Path.Combine(container,"Game Beta","Beta.exe"),"test");File.WriteAllText(System.IO.Path.Combine(container,"Standalone.exe"),"test");Check(DiscoverGames(container).Count==3,"library container executable does not merge separate game installations");}
 void TestConfiguredDirectoryStatus(){var previous=options.Folders;string oldFolder=folder;try{folder=@"E:\";options.Folders=new System.Collections.Generic.List<string>{@"D:\",@"C:\youxi"};Render();Check(status.Text.Contains(@"D:\")&&status.Text.Contains(@"C:\youxi")&&!status.Text.Contains(@"E:\"),"footer reflects configured multi-directory roots instead of stale legacy E drive");options.Folders=new System.Collections.Generic.List<string>();Render();Check(!status.Text.Contains(@"E:\"),"empty scan directory list does not show legacy E drive");}finally{options.Folders=previous;folder=oldFolder;Render();}}
 void TestFullscreenRows(){var priorGames=games;bool priorFull=fullscreen;double priorWidth=Width,priorHeight=Height;string priorFilter=filter;var priorDpi=System.Windows.Media.VisualTreeHelper.GetDpi(this);try{games=new System.Collections.Generic.List<Game>();for(int i=0;i<27;i++)games.Add(new Game{Path=System.IO.Path.Combine(home,"layout"+i+".exe"),OriginalName="Layout "+i,Name="Layout "+i,Cover=System.IO.Path.Combine(home,"layout"+i+".jpg")});filter="all";fullscreen=true;foreach(double dpi in new[]{1.0,1.25,1.5,2.0}){System.Windows.Media.VisualTreeHelper.SetRootDpi(this,new DpiScale(dpi,dpi));foreach(int pixels in new[]{1920,2560,3440}){Width=pixels/dpi;Height=1080/dpi;Render();UpdateLayout();UpdateTileSizes();UpdateLayout();Check(visible[0].NameLabel.FontSize==16,"card title retains intended size after fullscreen layout");double first=visible[0].Host.TranslatePoint(new Point(),tiles).Y,ninth=visible[8].Host.TranslatePoint(new Point(),tiles).Y;Check(Math.Abs(first-ninth)<1,"nine cards occupy one physical row at "+pixels+" pixels; DPI="+dpi+"; positions="+first+","+ninth);}}}finally{System.Windows.Media.VisualTreeHelper.SetRootDpi(this,priorDpi);games=priorGames;fullscreen=priorFull;Width=priorWidth;Height=priorHeight;filter=priorFilter;Render();UpdateLayout();}}
 void TestPortableCoverNames(){foreach(var sample in new[]{new[]{"DARK.SOULS.III.Deluxe.Edition","黑暗之魂3"},new[]{"Darksiders.III.Digital.Edition.v1.11.Incl.Keepers.of.the.Void.Dlc","暗黑血统3"},new[]{"FISTGame","暗影火炬城"},new[]{"Forza Horizon 4","极限竞速：地平线4"},new[]{"Grounded.2.Founders.Edition-InsaneRamZes","禁闭求生2"},new[]{"NFS18.Rivals.CHT.Green.Edition-ALI213","极品飞车18：宿敌"},new[]{"Nioh 2 Complete Edition","仁王2：完全版"},new[]{"TW3GOTY132S","巫师3：狂猎"}})Check(InstalledName(sample[0],"game.exe")==sample[1],"portable folder resolves cover search title: "+sample[0]);}
 void TestCoverRetryAfterRename(){var previousGames=games;var previousFolders=options.Folders;string previousFolder=folder,previousSignature=signature;try{string root=System.IO.Path.Combine(home,"retry-library"),install=System.IO.Path.Combine(root,"FISTGame");Directory.CreateDirectory(install);File.WriteAllText(System.IO.Path.Combine(install,"FIST.exe"),"test");options.Folders=new System.Collections.Generic.List<string>{root};folder=root;signature="";Scan();Check(games.Count==1,"retry fixture discovers game");var game=games[0];game.OriginalName="FISTGame";File.WriteAllText(game.Cover+".retry","old failed lookup");signature="";Scan();Check(!File.Exists(game.Cover+".retry"),"corrected game title retries missing artwork without twelve-hour wait");}finally{games=previousGames;options.Folders=previousFolders;folder=previousFolder;signature=previousSignature;Render();}}
 async System.Threading.Tasks.Task TestPlayHistory(){
  var game=new Game{Path=Path.Combine(home,"play-history.exe"),Name="Play history",OriginalName="Play history",Cover=Path.Combine(home,"play-history.jpg")};
  var probe=System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(System.Reflection.Assembly.GetExecutingAssembly().Location,"--window-probe --long-probe"){UseShellExecute=false});
  try{
   var state=State(game);
   state.PlayTicks=59*TimeSpan.TicksPerSecond;
   var target=new LaunchTarget();
   target.Tracked.Add(probe.Id);
   Check(await WaitForGameWindow(target,TimeSpan.FromSeconds(5)),"play history finds probe window");
   var handle=target.GameWindow;
   BeginPlayTracking(game,handle);
   playTimer.Stop();
   Check(PlayStatus(game)=="游玩中"&&state.PlayCount==1,"confirmed launch sets play status and count");
   await System.Threading.Tasks.Task.Delay(1100);
   SamplePlaySessions();
   await System.Threading.Tasks.Task.Delay(1100);
   SamplePlaySessions();
   long active=state.PlayTicks;
   Check(active>=61*TimeSpan.TicksPerSecond&&active<66*TimeSpan.TicksPerSecond,"two real foreground samples count");
   await System.Threading.Tasks.Task.Delay(1100);
   SamplePlaySessions();
   await System.Threading.Tasks.Task.Delay(1100);
   SamplePlaySessions();
   Check(state.PlayTicks>active&&PlayStatus(game)=="游玩中","elapsed process time continues without foreground input");
   SamplePlaySessions();
   await System.Threading.Tasks.Task.Delay(1100);
   SamplePlaySessions();
   Check(state.PlayTicks>active,"returning to foreground resumes counting");
   StopPlayTracking();
   var previousGames=games;
   try{
    games=new System.Collections.Generic.List<Game>{game};
    options.Games[Hash(game.Path)]=json.Deserialize<Options>(File.ReadAllText(Path.Combine(home,"settings.json"))).Games[Hash(game.Path)];
    RestorePlayTracking();
    playTimer.Stop();
    state=State(game);
    Check(PlayStatus(game)=="游玩中"&&state.PlayCount==1,"restart resumes the same process and does not count another launch");
   } finally{games=previousGames;}
   ShowLaunch(game);
   Check(launchStatus.FontSize==28&&launchDotsTimer.IsEnabled,"launch status uses larger text");
   var runningScreen=launchLayer;
   HoldLaunchScreen(playSessions[0]);
   Check(launchLayer==runningScreen&&main.Visibility==Visibility.Hidden&&stopLaunchAnimation==null&&!launchDotsTimer.IsEnabled&&CardContains(launchLayer,"游 玩 中"),"running game keeps the static launch screen and hides the library");
   var session=playSessions[0];
   session.Window=new IntPtr(-1);
   SamplePlaySessions();
   Check(CardContains(launchLayer,"正 在 退 出")&&launchStatus.FontSize==28&&launchDotsTimer.IsEnabled&&!session.Process.HasExited,"closed game window shows exit status while process is alive");
   session.Window=handle;
   SamplePlaySessions();
   Check(CardContains(launchLayer,"游 玩 中"),"valid game window shows playing status");
   TestKey(0x1B);
   Check(launchLayer==null&&main.IsVisible&&PlayStatus(game)=="游玩中","Esc returns to the library while the game keeps running");
   ShowLaunch(game);
   HoldLaunchScreen(playSessions[0]);
   probe.Kill();
   probe.WaitForExit(2000);
   SamplePlaySessions();
   Check(PlayStatus(game)==null,"closing a game clears its status");
   Check(launchLayer==null&&parkedLaunchSession==null&&main.Visibility==Visibility.Visible,"tracked process exit restores game library");
   var saved=json.Deserialize<Options>(File.ReadAllText(Path.Combine(home,"settings.json")));
   Check(saved.Games[Hash(game.Path)].PlayTicks==state.PlayTicks,"closed session persists exact duration");
   tiles.Children.Clear();
   game.CardKey=null;
   BuildCard(game);
   game.Host.Measure(new Size(200,350));
   game.Host.Arrange(new Rect(0,0,200,350));
   Check(CardContains(game.Host,"游玩时长")&&CardContains(game.Host,"1 分钟"),"card displays persisted whole minutes: "+PlayDuration(state.PlayTicks));
   tiles.Children.Remove(game.Host);
   File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"play-history-test.txt"),
    "First foreground: "+TimeSpan.FromTicks(active-59*TimeSpan.TicksPerSecond).TotalSeconds.ToString("F2")+"s; background: 0s; resumed: "+TimeSpan.FromTicks(state.PlayTicks-active).TotalSeconds.ToString("F2")+"s; card: "+PlayDuration(state.PlayTicks));
  }finally{if(!probe.HasExited)probe.Kill();probe.Dispose();}
 }

 static bool CardContains(DependencyObject node,string text){
  var label=node as System.Windows.Controls.TextBlock;
  if(label!=null&&label.Text==text)return true;
  var panel=node as System.Windows.Controls.Panel;
  if(panel!=null){
   foreach(UIElement child in panel.Children)if(CardContains(child,text))return true;
   return false;
  }
  var viewbox=node as System.Windows.Controls.Viewbox;
  if(viewbox!=null&&viewbox.Child!=null)return CardContains(viewbox.Child,text);
  var decorator=node as System.Windows.Controls.Decorator;
  if(decorator!=null&&decorator.Child!=null)return CardContains(decorator.Child,text);
  for(int i=0;i<System.Windows.Media.VisualTreeHelper.GetChildrenCount(node);i++)
   if(CardContains(System.Windows.Media.VisualTreeHelper.GetChild(node,i),text))return true;
  return false;
 }
 async void RunTests(){try{
  Check(WrapCardTitle("漫威蜘蛛侠：迈尔斯·莫拉莱斯",9,t=>t.Length)=="漫威蜘蛛侠：\n迈尔斯·莫拉莱斯","title wraps after subtitle separator");
  Check(WrapCardTitle("乐高-地平线大冒险",8,t=>t.Length)=="乐高-\n地平线大冒险","Chinese title supports compact hyphen");
  Check(WrapCardTitle("勇者斗恶龙 HD-2D 重制版",13,t=>t.Length)=="勇者斗恶龙 HD-2D\n重制版","edition stays together and HD-2D is preserved");
  Check(WrapCardTitle("鬼泣5",9,t=>t.Length)=="鬼泣5","short title remains one line");
  await TestPlayHistory();TestCoverRetryAfterRename();TestPortableCoverNames();TestFullscreenRows();TestPortableLibraryDiscovery();TestConfiguredDirectoryStatus();options.ScanShortcuts=true;Check(Norm("Assassin’s Creed® Origins")==Norm("Assassins Creed Origins"),"possessive apostrophe matches equivalent game query");Check(Norm("Dragon's Dogma 2")==Norm("Dragons Dogma 2"),"dragon possessive title preserves the letter s");Check(GameDirectory(new Game{Path=System.Reflection.Assembly.GetExecutingAssembly().Location,Name="probe",OriginalName="probe"})==System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),"game executable folder resolution");
  padInputArmed=true;Check(!AcceptPadInput(false,false,false)&&!padInputArmed,"background gamepad blocked even with held input");Check(!AcceptPadInput(true,false,false),"return to launcher requires button release");Check(!AcceptPadInput(true,false,true)&&padInputArmed,"neutral frame rearms without dispatching");Check(AcceptPadInput(true,false,false),"fresh foreground input accepted");Check(!AcceptPadInput(true,true,true)&&!padInputArmed,"launch transition disarms controller");Check(!AcceptPadInput(false,false,true)&&!padInputArmed,"background neutral cannot rearm");Check(!AcceptPadInput(true,false,false),"held game button cannot activate launcher on return");

  var freshSample=new PadSnapshot{SampleTicks=System.Diagnostics.Stopwatch.GetTimestamp(),Count=1,Identity=(IntPtr)123,Type=3};Check(FreshPadSnapshot(freshSample),"fresh controller snapshot is accepted");Check(!FreshPadSnapshot(new PadSnapshot{SampleTicks=System.Diagnostics.Stopwatch.GetTimestamp()-System.Diagnostics.Stopwatch.Frequency}),"stale controller snapshot is discarded instead of replayed");consumedPad=freshSample.Identity;padInputArmed=true;lastDirection=0;navZone="tabs";navIndex=0;freshSample.Buttons[11]=true;ApplyPadSnapshot(freshSample,true);Check(navIndex==1,"new dpad press dispatches immediately from sampled state");ApplyPadSnapshot(freshSample,true);Check(navIndex==1,"held dpad does not repeat before its deadline");var neutralSample=new PadSnapshot{Count=1,Identity=freshSample.Identity,Type=3};ApplyPadSnapshot(neutralSample,true);ApplyPadSnapshot(freshSample,true);Check(navIndex==2,"release and fresh dpad press bypass repeat delay");ApplyPadSnapshot(neutralSample,false);Check(!padInputArmed,"background sampled state disarms input");navZone="games";navIndex=0;lastDirection=0;repeat=DateTime.MinValue;OpenExitConfirmation();Check(modal=="exit-confirm"&&modalButtons.Count==2&&modalSelected==0,"exit confirmation defaults to cancel");Confirm();Check(modal==""&&!closing,"default confirmation cancels without exiting");var triggers=new PadSnapshot{Count=1,Identity=consumedPad,Type=3,LeftTrigger=25000,RightTrigger=25000};padInputArmed=true;exitChordHeld=false;ApplyPadSnapshot(triggers,true);Check(modal=="exit-confirm","L2 and R2 chord opens exit confirmation");modalSelected=1;ApplyPadSnapshot(triggers,true);Check(modalSelected==1,"held trigger chord does not reopen or reset confirmation");modalSelected=0;Confirm();ApplyPadSnapshot(triggers,true);Check(modal=="","held chord after cancel does not reopen");ApplyPadSnapshot(neutralSample,true);ApplyPadSnapshot(triggers,false);Check(modal==""&&!padInputArmed,"background trigger chord cannot open exit prompt");ApplyPadSnapshot(triggers,true);Check(modal=="","held triggers on focus return require release");ApplyPadSnapshot(neutralSample,true);ApplyPadSnapshot(triggers,true);Check(modal=="exit-confirm","released and repressed trigger chord works again");Confirm();ApplyPadSnapshot(neutralSample,true);toolNav[toolNav.Count-1].RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));Check(modal=="exit-confirm","top right close icon opens same confirmation");ColorPreview("exit-confirmation.png");Confirm();navZone="games";string discoveryRoot=System.IO.Path.Combine(home,"discovery");string gameInstall=System.IO.Path.Combine(discoveryRoot,"Example Adventure");string bin=System.IO.Path.Combine(gameInstall,"Binaries","Win64");Directory.CreateDirectory(bin);File.WriteAllText(System.IO.Path.Combine(bin,"Adventure-Win64-Shipping.exe"),"test");File.WriteAllText(System.IO.Path.Combine(gameInstall,"unins000.exe"),"test");File.WriteAllText(System.IO.Path.Combine(gameInstall,"CrashReportClient.exe"),"test");var discovered=DiscoverGames(discoveryRoot);Check(discovered.Count==1&&discovered[0].Path.EndsWith("Shipping.exe"),"recursive install scan excludes uninstall and crash tools");Check(discovered[0].Name=="Example Adventure","installed game uses installation folder title");int inspected=refreshedDirectories;Check(DiscoverGames(discoveryRoot).Count==1&&refreshedDirectories==inspected,"unchanged directory inventory avoids repeated file enumeration");string secondInstall=System.IO.Path.Combine(discoveryRoot,"New Game");Directory.CreateDirectory(secondInstall);File.WriteAllText(System.IO.Path.Combine(secondInstall,"NewGame.exe"),"test");Check(DiscoverGames(discoveryRoot).Count==2,"new installation folder is discovered incrementally");Directory.Delete(secondInstall,true);Check(DiscoverGames(discoveryRoot).Count==1,"removed installation folder drops cached game");string nestedInstall=System.IO.Path.Combine(gameInstall,"NewNested");Directory.CreateDirectory(nestedInstall);File.WriteAllText(System.IO.Path.Combine(nestedInstall,"Nested.exe"),"test");int beforeNested=refreshedDirectories;DiscoverGames(discoveryRoot);Check(refreshedDirectories>beforeNested,"nested directory changes invalidate only affected inventory");Directory.Delete(nestedInstall,true);File.Delete(System.IO.Path.Combine(bin,"Adventure-Win64-Shipping.exe"));Check(DiscoverGames(discoveryRoot).Count==0,"removed executable disappears from discovery");Check(UtilityExe("vcredist_x64.exe")&&!UtilityExe("Adventure.exe"),"runtime utility exclusion");string profileInstall=System.IO.Path.Combine(discoveryRoot,"Darksiders Genesis");Directory.CreateDirectory(profileInstall);string profileExe=System.IO.Path.Combine(profileInstall,"DarksidersGenesis.exe");File.WriteAllText(profileExe,"test");File.WriteAllText(System.IO.Path.Combine(profileInstall,"Other-Win64-Shipping.exe"),"test");var profiled=DiscoverGames(profileInstall);Check(profiled.Count==1&&profiled[0].Path==profileExe,"verified relative entry wins without any shortcut present");File.Delete(profileExe);Check(DiscoverGames(profileInstall)[0].Path.EndsWith("Shipping.exe"),"missing known entry falls back to actual installed executable");File.Delete(System.IO.Path.Combine(profileInstall,"Other-Win64-Shipping.exe"));
  Check(!UtilityExe("Crashlands2.exe")&&UtilityExe("CrashReportClient.exe"),"game names containing crash are preserved");File.WriteAllText(System.IO.Path.Combine(bin,"Adventure-Win64-Shipping.exe"),"test");var priorRoots=options.Folders;options.Folders=new System.Collections.Generic.List<string>{discoveryRoot,gameInstall};Check(DiscoverLibraries(LibraryFolders()).Count==1,"overlapping scan directories deduplicate executables");Check(MinimalScanRoots(new[]{discoveryRoot,gameInstall,bin}).Length==1,"nested scan roots do not select different executables twice");Check(NormalizeScanRoot(gameInstall+"\\")==gameInstall,"trailing separator retains install identity");string palRoot=System.IO.Path.Combine(discoveryRoot,"Sword and Fairy 7");string palBin=System.IO.Path.Combine(palRoot,"Pal7","Binaries","Win64");Directory.CreateDirectory(palBin);File.WriteAllText(System.IO.Path.Combine(palRoot,"Pal7.exe"),"test");File.WriteAllText(System.IO.Path.Combine(palBin,"Pal7-Win64-Shipping.exe"),"test");Check(DiscoverGames(palRoot+"\\")[0].Path==System.IO.Path.Combine(palRoot,"Pal7.exe"),"verified launcher wins over competing internal rendering executable");File.Delete(System.IO.Path.Combine(palRoot,"Pal7.exe"));File.Delete(System.IO.Path.Combine(palBin,"Pal7-Win64-Shipping.exe"));options.Folders=new System.Collections.Generic.List<string>();Check(LibraryFolders().Length==0,"empty directory list does not restore legacy folder");options.Folders=priorRoots;ExpandSearch(false,false);Check(!searchExpanded&&search.Visibility==Visibility.Collapsed&&Math.Abs(searchBox.Height-toolNav[0].Height)<.1,"collapsed search matches icon height");ExpandSearch(true,false);Check(searchExpanded&&search.Visibility==Visibility.Visible&&searchBox.Width>150,"search icon expands input");ExpandSearch(false,false);
  var indexedFirst=DiscoverIndexedRoot(discoveryRoot);int indexRefreshed=refreshedDirectories;
  var indexedAgain=DiscoverIndexedRoot(discoveryRoot);
  Check(indexedFirst.Count==indexedAgain.Count&&refreshedDirectories==indexRefreshed,"indexed scan reuses unchanged installations without walking subdirectories");
  string incrementalInstall=Path.Combine(discoveryRoot,"Incremental Adventure");Directory.CreateDirectory(incrementalInstall);File.WriteAllText(Path.Combine(incrementalInstall,"Play.exe"),"fixture");
  Check(DiscoverIndexedRoot(discoveryRoot).Exists(x=>x.Path==Path.Combine(incrementalInstall,"Play.exe")),"indexed scan discovers a new installation directory");
  File.Delete(Path.Combine(incrementalInstall,"Play.exe"));
  Check(!DiscoverIndexedRoot(discoveryRoot).Exists(x=>x.Path==Path.Combine(incrementalInstall,"Play.exe")),"indexed scan drops a deleted launch executable");
  Directory.Delete(incrementalInstall);
  string revived=Path.Combine(bin,"启动-Revived-Win64-Shipping.exe");File.WriteAllText(revived,"fixture");lock(dirtyLock)dirtyPaths.Add(revived);
  int affectedBefore=indexScannedBranches;var changedIndex=DiscoverIndexedRoot(discoveryRoot);
  Check(changedIndex.Exists(x=>x.Path==revived)&&indexScannedBranches==affectedBefore+1,"nested entry notification rescans only its installation: "+json.Serialize(new{Before=affectedBefore,After=indexScannedBranches,Entries=changedIndex}));
  string renamed=Path.Combine(bin,"启动-Renamed-Win64-Shipping.exe");File.Move(revived,renamed);lock(dirtyLock){dirtyPaths.Add(revived);dirtyPaths.Add(renamed);}
  Check(DiscoverIndexedRoot(discoveryRoot).Exists(x=>x.Path==renamed),"renamed executable replaces the indexed launch entry");
  var indexSnapshot=json.Deserialize<LibrarySnapshot>(json.Serialize(new LibrarySnapshot{Roots=new[]{discoveryRoot},Games=changedIndex,Index=discoveryIndex}));discoveryIndex.Clear();SeedDiscoveryIndex(indexSnapshot);
  int restoredScans=indexScannedBranches;Check(DiscoverIndexedRoot(discoveryRoot).Exists(x=>x.Path==renamed)&&indexScannedBranches==restoredScans,"persisted installation index survives serialization without deep traversal");
  File.Delete(renamed);

  string dir=System.IO.Path.Combine(home,"games");Directory.CreateDirectory(dir);File.WriteAllText(System.IO.Path.Combine(dir,"Alpha.url"),"[InternetShortcut]\nURL=https://example.com");File.WriteAllText(System.IO.Path.Combine(dir,"Beta.url"),"[InternetShortcut]\nURL=https://example.com");ChangeFolder(dir);Check(games.Count==2,"scan initial directory");Check((string)toolNav[0].Resources["ActionIcon"]=="scan"&&(string)toolNav[1].Resources["ActionIcon"]=="refresh","scan and cover buttons are separate");File.WriteAllText(System.IO.Path.Combine(dir,"Manual.url"),"[InternetShortcut]");TestKey(0x74);Check(games.Count==3,"F5 manually rescans game library");File.Delete(System.IO.Path.Combine(dir,"Manual.url"));ManualScan();Check(games.Count==2,"manual scan removes deleted entry");
  File.WriteAllText(System.IO.Path.Combine(dir,"Gamma.url"),"[InternetShortcut]\nURL=https://example.com");ManualScan();Check(games.Count==3,"manual addition");File.Delete(System.IO.Path.Combine(dir,"Gamma.url"));ManualScan();Check(games.Count==2,"manual removal");
  var g=games[0];State(g).Favorite=true;filter="favorite";Render();Check(visible.Count==1,"favorite filtering");State(g).Hidden=true;Render();Check(visible.Count==0,"hide excludes from normal library");filter="hidden";Render();Check(visible.Count==1,"hidden games recoverable");Check(File.Exists(g.Path),"hide preserves original game entry");State(g).Hidden=false;State(g).PlayCount=4;State(g).LastPlayed=DateTime.UtcNow.Ticks;filter="recent";Render();Check(visible.Count==1,"recent filter");filter="unplayed";Render();Check(visible.Count==1,"unplayed filter");filter="missing";Render();Check(visible.Count==2,"missing cover filter");
  filter="all";options.Sort="name-desc";Render();Check(visible[0].OriginalName=="Beta","name descending");options.Sort="count";Render();Check(visible[0]==g,"launch count sorting");Save();LoadOptions();Check(options.Folder==dir&&State(g).Favorite,"folder and favorite persistence");OpenGame(g);Check(modalButtons.Count==8,"game action menu");Check(MenuAccent(0)!=MenuAccent(1)&&MenuAccent(1)!=MenuAccent(2),"game menu semantic accent colors");MoveModal(1);Check(modalButtons[1].Background is System.Windows.Media.LinearGradientBrush,"controller selection uses colored gradient");MoveModal(-1);UpdateLayout();var preview=new System.Windows.Media.Imaging.RenderTargetBitmap((int)ActualWidth,(int)ActualHeight,96,96,System.Windows.Media.PixelFormats.Pbgra32);preview.Render(this);var png=new System.Windows.Media.Imaging.PngBitmapEncoder();png.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(preview));using(var output=File.Create(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),"menu-preview.png")))png.Save(output);MoveModal(5);Check(modalSelected==5,"controller menu movement");CloseModal();OpenSettings();Check(modalButtons.Count==9,"settings and exit actions");Check(modalCard.Opacity==0,"new modal stays invisible until final layout is ready");await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.ApplicationIdle);Check(modalCard.ActualHeight>0&&modalViewport.ActualHeight<=ActualHeight-30,"menu fits before its first visible fade frame");double stableMenuHeight=modalViewport.ActualHeight;MoveModal(1);UpdateLayout();Check(Math.Abs(modalViewport.ActualHeight-stableMenuHeight)<.1,"changing menu selection does not resize the menu");MoveModal(-1);Check(modalButtons[0].Resources.Contains("ActionColor"),"settings semantic colors");ColorPreview("settings-dark.png");double menuTestWidth=Width,menuTestHeight=Height;Width=800;Height=760;UpdateLayout();FitModal();UpdateLayout();Check(modalViewport.ActualHeight>=ActualHeight-40,"medium window menu uses available height: actual="+modalViewport.ActualHeight+", window="+ActualHeight+", natural="+modalCard.DesiredSize.Height+", row="+modalButtons[0].MinHeight);Check(modalButtons[0].MinHeight>48&&modalButtons[0].MinHeight<68,"menu spacing interpolates instead of two fixed density modes");ColorPreview("settings-medium.png");Width=800;Height=560;UpdateLayout();FitModal();UpdateLayout();var lastMenuButton=modalButtons[modalButtons.Count-1];var menuBottom=lastMenuButton.TranslatePoint(new Point(0,lastMenuButton.ActualHeight),overlay);Check(menuBottom.Y<=ActualHeight-12,"settings last action fits minimum window without scrolling");Check(modalViewport.ActualHeight<=ActualHeight-30&&modalViewport.ActualWidth<=ActualWidth-30,"menu fits available window bounds");ColorPreview("settings-compact.png");CloseModal();OpenGame(g);UpdateLayout();FitModal();UpdateLayout();var gameMenuBottom=modalButtons[modalButtons.Count-1].TranslatePoint(new Point(0,modalButtons[modalButtons.Count-1].ActualHeight),overlay);Check(gameMenuBottom.Y<=ActualHeight-12,"all eight game actions fit minimum window");Width=menuTestWidth;Height=menuTestHeight;UpdateLayout();FitModal();UpdateLayout();Check(modalViewport.ActualHeight<=ActualHeight-30,"open menu adapts when window is resized");CloseModal();OpenSettings();bool priorDark=dark;dark=false;Check(ActionColor("heart")=="#AD3C78","light theme uses readable darker accents");dark=priorDark;CloseModal();ToggleFull();Check(fullscreen,"fullscreen enter");UpdateLayout();OpenSettings();UpdateLayout();FitModal();UpdateLayout();Check(modalViewport.ActualHeight>=ActualHeight*.90,"fullscreen settings uses at least ninety percent of window height");Check(modalViewport.ActualHeight<=ActualHeight-30,"tall fullscreen settings stays within screen");ColorPreview("settings-fullscreen.png");CloseModal();UpdateTileSizes();Check(WindowState==WindowState.Normal,"fullscreen uses explicit monitor bounds");Check(tiles.ItemWidth>0&&Math.Abs(tiles.Width-tiles.ItemWidth*columns)<.1,"fullscreen rows fill exact centered grid width");Check(Math.Abs(visible[0].Card.Width+20-tiles.ItemWidth)<.1,"card width and gutters consume full cell");Check(columns==9&&Math.Abs(scroll.ActualWidth-(main.ActualWidth-LeftRailWidth))<2,"fullscreen preserves nine columns beside the left sidebar");ToggleFull();Check(!fullscreen,"fullscreen restore");options.StartupMode="全屏";Save();options.StartupMode="窗口";LoadOptions();Check(options.StartupMode=="全屏","startup fullscreen preference persists");OpenStartupMode();Check(modalButtons.Count==2,"startup display has window and fullscreen choices");CloseModal();Check(!fullscreen,"changing startup preference leaves current display intact");startupDisplayApplied=false;ApplyStartupDisplay();Check(fullscreen,"startup preference enters fullscreen");ToggleFull();ApplyStartupDisplay();Check(!fullscreen,"startup mode applies only once per application run");options.StartupMode="窗口";Save();startupDisplayApplied=false;ApplyStartupDisplay();Check(!fullscreen,"window startup preference keeps normal window");var snapshotGames=games;SaveLibrarySnapshot();games=new System.Collections.Generic.List<Game>();Check(RestoreLibrarySnapshot()&&games.Count==snapshotGames.Count,"startup restores persisted library before scanning");Check(games[0].Path==snapshotGames[0].Path,"snapshot preserves executable identity");SaveLibrarySnapshot();File.WriteAllText(SnapshotPath(),"corrupt");games.Clear();Check(RestoreLibrarySnapshot()&&games.Count==snapshotGames.Count,"corrupt primary snapshot recovers atomic backup");games=snapshotGames;Render();
  theme="浅色";ApplyTheme(true);Check(!dark,"light theme");theme="深色";ApplyTheme(true);Check(dark,"dark theme");Check(Norm("God of War Ragnarök")==Norm("God of War Ragnarok"),"localized accented title matching");string original=games[0].OriginalName;games[0].Name="中文测试";search.Text=original;Render();Check(visible.Count==1,"original English title search after translation");
  Check(IsPs(3)&&IsPs(4)&&IsPs(7)&&!IsPs(1),"PS versus Xbox glyph selection");SetFamily("ps");Check(hintBar.Children.Count==8,"PS shortcut hints");SetFamily("xbox");Check(hintBar.Children.Count==8,"Xbox shortcut hints");OpenSettings();Check(hintBar.Children.Count==4,"modal controller hints");CloseModal();
  search.Clear();filter="all";navZone="games";selected=0;Render();selected=0;int edgeGame=selected;Navigate(-1,0);Check(navZone=="tools"&&navIndex==toolNav.Count-1,"left edge enters first sidebar control");for(int item=0;item<tabNav.Count+toolNav.Count;item++){Navigate(0,1);Check(HeaderIndex()==item+1,"every sidebar control is reachable vertically "+item);}Check(navZone=="search","sidebar navigation reaches search at the bottom");for(int item=tabNav.Count+toolNav.Count;item>0;item--)Navigate(0,-1);Check(navZone=="tools"&&navIndex==toolNav.Count-1,"sidebar navigation returns to the first button");Navigate(1,0);Check(navZone=="games"&&selected==edgeGame,"right returns to previously selected game");navZone="tabs";navIndex=4;RefreshNavFocus();Confirm();Check(modal=="sort","gamepad opens sort button");CloseModal();navZone="tabs";navIndex=3;RefreshNavFocus();Confirm();Check(modal=="filters","gamepad opens filter button");CloseModal();navZone="search";RefreshNavFocus();Confirm();Check(modal=="search"&&modalButtons.Count==40,"controller screen keyboard");modalSelected=0;Confirm();Check(search.Text=="A","screen keyboard types");modalSelected=38;Confirm();Check(search.Text=="","screen keyboard clears");modalSelected=39;Confirm();Check(modal==""&&navZone=="games","screen keyboard returns to games");selected=0;Render();Check(glowingGame==visible[0]&&glowingGame.Card.BorderBrush is System.Windows.Media.LinearGradientBrush,"animated gradient border");Check(glowingGame.Card.Effect==null,"no blur effect on card content");Check(glowingGame.Scale.ScaleX==1&&glowingGame.Scale.ScaleY==1,"selected text remains unscaled");Check(glowingGame.Halo.Effect is System.Windows.Media.Effects.DropShadowEffect,"glow runs on separate halo layer");selected=0;Navigate(-1,0);Check(glowingGame==null,"only game focus runs breathing effect");
  main.Focus();TestKey(0x70);Check(modal=="settings","native F1 opens settings");TestKey(0x70);Check(modal=="","native F1 closes settings");TestKey(0x7A);Check(fullscreen,"native F11 enters fullscreen");TestKey(0x7A);Check(!fullscreen,"native F11 exits fullscreen");navZone="games";selected=0;RefreshNavFocus();TestKey(0x0D);Check(modal=="game","native Enter opens game menu");TestKey(0x1B);Check(modal=="","native Escape closes menu");selected=0;TestKey(0x25);Check(navZone=="tools","native left enters sidebar from grid edge");
  Check(IsSupplement("Bloodstained Succubus Cosmetic Pack","Bloodstained"),"cosmetic DLC is rejected as base game cover");Check(!IsSupplement("The Jackbox Party Pack","The Jackbox Party Pack"),"base game pack title remains eligible");var duplicateResult=DedupeGames(new[]{new FoundGame{Name="Riverbond",Path=@"E:\Old\Riverbond.exe"},new FoundGame{Name="Riverbond",Path=@"C:\Epic\Riverbond.exe",Official=true},new FoundGame{Name="Riverbond 2",Path=@"E:\Next\Riverbond2.exe"}});Check(duplicateResult.Count==2&&duplicateResult[0].Official&&duplicateResult[0].AlternatePaths.Count==1,"duplicate game copies merge and prefer registered platform installation");byte[] crystalTone=Synthesize("favorite",.55,"水晶清音"),woodTone=Synthesize("favorite",.55,"温暖木音");Check(crystalTone.Length!=woodTone.Length,"sound presets have different envelopes and timing");Check(BuiltinCover(new Game{OriginalName="yuzu模拟器",Cover=System.IO.Path.Combine(home,"yuzu.jpg")})&&HasCover(System.IO.Path.Combine(home,"yuzu.jpg")),"builtin yuzu artwork loads without network");
  byte[] tone=Synthesize("favorite",.55);Check(System.Text.Encoding.ASCII.GetString(tone,0,4)=="RIFF"&&tone.Length>10000,"synthesized PCM sound");OpenAudio();Check(modalButtons.Count==9,"audio controls");CloseModal();
  navZone="games";selected=0;Render();Navigate(1,0);Check(selected==1,"gamepad moves right");visible[0].Card.RaiseEvent(new System.Windows.Input.MouseEventArgs(System.Windows.Input.Mouse.PrimaryDevice,Environment.TickCount){RoutedEvent=System.Windows.Input.Mouse.MouseMoveEvent});Check(selected==1,"layout mouse move cannot steal game focus");Navigate(-1,0);Check(selected==0,"gamepad moves left");navZone="tools";navIndex=0;RefreshNavFocus();Navigate(0,-1);toolNav[0].RaiseEvent(new System.Windows.Input.MouseEventArgs(System.Windows.Input.Mouse.PrimaryDevice,Environment.TickCount){RoutedEvent=System.Windows.Input.Mouse.MouseMoveEvent});Check(navIndex==1&&navZone=="tools","layout mouse move cannot steal toolbar focus");pointerKnown=true;pointerAnchor.X=100;pointerAnchor.Y=100;controllerNavigation=true;Check(!PointerMoved(100,100)&&controllerNavigation,"stationary physical pointer stays suppressed");Check(PointerMoved(104,100)&&!controllerNavigation,"real mouse movement restores pointer control");
  var pad=new bool[21];pad[14]=true;Check(ReadDirection(pad,0,30000,0)==1,"D-pad right beats analog vertical drift");pad[14]=false;pad[13]=true;Check(ReadDirection(pad,0,30000,0)==-1,"D-pad left beats analog vertical drift");pad[13]=false;Check(ReadDirection(pad,16000,0,0)==1,"analog right threshold");Check(ReadDirection(pad,11000,0,1)==1,"analog hysteresis preserves hold");Check(ReadDirection(pad,9000,0,1)==0,"analog release dead zone");Check(ExternalCover("血源诅咒")!=null&&ExternalCover("植物大战僵尸杂交版")!=null&&ExternalCover("植物大战僵尸")==null,"external covers target correct game editions");
  string cached=System.IO.Path.Combine(home,"covers","restart-cache.jpg");var pixels=new byte[64];for(int i=0;i<pixels.Length;i++)pixels[i]=128;var frame=System.Windows.Media.Imaging.BitmapSource.Create(4,4,96,96,System.Windows.Media.PixelFormats.Bgra32,null,pixels,16);var encoder=new System.Windows.Media.Imaging.PngBitmapEncoder();encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(frame));using(var ms=new MemoryStream()){encoder.Save(ms);WriteCover(cached,ms.ToArray());}var probe=new Game{Name="Cached game",OriginalName="Cached game",Path=g.Path,Cover=cached,Image=new System.Windows.Controls.Image()};int requestsBefore=networkRequests;var written=File.GetLastWriteTimeUtc(cached);Check(HasCover(cached)&&LoadImage(probe),"cached cover decodes locally");BadCover(cached);Check(!HasCover(cached)&&LoadImage(probe)&&HasCover(cached),"valid cached image recovers from stale failed-load state without download");Check(Fetch(probe)&&networkRequests==requestsBefore,"existing cover never makes network request");var reopened=new Game{Name=probe.Name,OriginalName=probe.OriginalName,Path=probe.Path,Cover=cached,Image=new System.Windows.Controls.Image()};Check(LoadImage(reopened)&&Fetch(reopened)&&networkRequests==requestsBefore,"fresh session reuses cache offline");Check(File.GetLastWriteTimeUtc(cached)==written,"restart does not rewrite cover");UpdateLayout();UpdateViewportCovers();var activeGame=visible[0];WriteCover(activeGame.Cover,File.ReadAllBytes(cached));Check(RecoverCachedCoverCards()&&activeGame.Image.Source!=null,"live placeholder reloads an externally available cached cover without rescan");Check(games.Count==2,"cover recovery leaves library discovery unchanged");File.Delete(activeGame.Cover);Render();SendMessage(new System.Windows.Interop.WindowInteropHelper(this).Handle,(int)coverDiagnosticsMessage,IntPtr.Zero,IntPtr.Zero);Check(File.Exists(System.IO.Path.Combine(home,"live-cover-state.json")),"diagnostic message records actual in-process cover states");File.WriteAllText(cached+".title","已缓存中文名");Check(Fetch(reopened,true)&&networkRequests==requestsBefore,"cached title needs no network");File.WriteAllText(cached+".unfinished.tmp","partial download");Check(HasCover(cached)&&LoadImage(reopened),"unfinished download preserves valid cache");File.WriteAllBytes(cached+".empty",new byte[0]);Check(!HasCover(cached+".empty"),"empty cache entry is rejected");
  MinimizeToTray();Check(inTray&&!IsVisible&&!ShowInTaskbar,"minimize hides window from taskbar");RestoreFromTray();Check(!inTray&&IsVisible&&ShowInTaskbar,"tray restore returns window");TestKey(0x70);Check(modal=="settings","keyboard still works after tray restore");TestKey(0x70);Check(modal=="","tray restore keeps a single keyboard hook");
  Check(GridColumns()==9,"default grid has nine columns");Check(reopened.Image.Source is System.Windows.Media.Imaging.BitmapSource,"cached cover is decoded before display");
  var probeGame=new Game{Name="Window probe",OriginalName="Window probe",Path=System.Reflection.Assembly.GetExecutingAssembly().Location};var watch=ResolveLaunch(probeGame);launchWaitCancelled=false;Check(!await WaitForGameWindow(watch,TimeSpan.FromMilliseconds(180)),"unrelated windows do not end launch animation");using(var process=System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(probeGame.Path,"--window-probe"){UseShellExecute=false})){watch.Tracked.Add(process.Id);var clock=System.Diagnostics.Stopwatch.StartNew();Check(await WaitForGameWindow(watch,TimeSpan.FromSeconds(5)),"real game process window detection");Check(clock.ElapsedMilliseconds>=600,"waits for stable game window instead of fixed delay");if(!process.HasExited)await System.Threading.Tasks.Task.Run(()=>process.WaitForExit(4000));}launchWaitCancelled=true;var cancelClock=System.Diagnostics.Stopwatch.StartNew();Check(!await WaitForGameWindow(watch,TimeSpan.FromSeconds(10))&&cancelClock.ElapsedMilliseconds<500,"cancel waiting without terminating game");launchWaitCancelled=false;
  OpenSettings();
  modalButtons[5].RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
  Check(modal=="grid-size"&&modalButtons.Count==9,"column picker is available from settings");
  modalButtons[2].RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
  Check(GridColumns()==6,"column selection applies immediately");
  Check(WindowGridColumns(1200)==5&&WindowGridColumns(2300)==6,"window columns fit the viewport and respect the configured limit");
  LoadOptions();Check(GridColumns()==6,"column selection persists");
  CloseModal();
  var menuPress=new PadSnapshot{Count=1,Identity=consumedPad,Type=3};
  menuPress.Buttons[4]=true;
  ApplyPadSnapshot(neutralSample,true);
  ApplyPadSnapshot(menuPress,true);
  Check(fullscreen&&columns==6,"left menu button toggles fullscreen with configured columns");
  ApplyPadSnapshot(menuPress,true);
  Check(fullscreen,"holding the left menu button does not toggle again");
  ApplyPadSnapshot(neutralSample,true);
  ApplyPadSnapshot(menuPress,true);
  UpdateLayout();UpdateTileSizes();
  Check(!fullscreen&&columns==WindowGridColumns(scroll.ActualWidth-scroll.Padding.Left-scroll.Padding.Right),"left button restores responsive window columns");
  var settingsPress=new PadSnapshot{Count=1,Identity=consumedPad,Type=3};
  settingsPress.Buttons[6]=true;
  ApplyPadSnapshot(neutralSample,true);
  ApplyPadSnapshot(settingsPress,true);
  Check(modal=="settings"&&!fullscreen,"right button opens settings");
  ApplyPadSnapshot(neutralSample,true);
  ApplyPadSnapshot(settingsPress,true);
  Check(modal==""&&!fullscreen,"right button closes settings");
  options.Columns=9;Save();UpdateTileSizes();
  ShowScanToast();UpdateLayout();
  Check(scanToast.IsVisible&&scanToast.HorizontalAlignment==HorizontalAlignment.Right&&scanToast.VerticalAlignment==VerticalAlignment.Bottom,"scan progress floats at bottom right");
  UpdateScanToast(new ScanProgress{Games=4,Completed=1,Total=2,Directory=@"E:\Games\Long folder name for scan progress wrapping"});UpdateLayout();
  Check(scanToastDetail.Text.Contains("游戏库现有 "+games.Count)&&scanToastDetail.Text.Contains("当前目录：E:"),"scan progress separates library count and current directory");
  Check(scanToastProgress.IsIndeterminate&&scanToastDetail.TextWrapping==TextWrapping.Wrap&&scanToast.ActualWidth<=320,"scan progress wraps details without misleading root percentage");
  Check(scanToastDetail.Text.Contains("封面 ")&&scanToastDetail.Text.Contains("缺少 "),"scan progress includes cover totals and missing count");
  int toastGeneration=++scanGeneration;
  FinishScanToast(toastGeneration,true);
  await System.Threading.Tasks.Task.Delay(3600);
  Check(!scanToast.IsVisible,"scan progress disappears after completion");
  var wonderRoot=Path.Combine(home,"Super Mario Bros. Wonder");
  Directory.CreateDirectory(Path.Combine(wonderRoot,"game-build","translator"));
  File.WriteAllText(Path.Combine(wonderRoot,"wonder.exe"),"fixture");
  File.WriteAllText(Path.Combine(wonderRoot,"game-build","translator","spirv_harness.exe"),"fixture");
  var wonderEntry=DiscoverGames(wonderRoot);
  Check(wonderEntry.Count==1&&wonderEntry[0].Path==Path.Combine(wonderRoot,"wonder.exe"),"Wonder scan selects game executable instead of shader tool");
  var wonderCard=new Game{OriginalName="Super Mario Bros. Wonder",Cover=Path.Combine(home,"wonder-cover.jpg")};
  Check(Fetch(wonderCard)&&DecodeBitmap(wonderCard.Cover,300).PixelHeight>300,"Wonder portrait cover is available without network");
  double railTestWidth=Width,railTestHeight=Height;
  string railTestFamily=padFamily;
  foreach(var size in new[]{new Size(800,560),new Size(1280,720)}){
   Width=size.Width;Height=size.Height;
   foreach(string family in new[]{"keyboard","ps","xbox"}){
    SetFamily(family);ExpandSearch(false,false);UpdateLayout();
    var searchTop=searchBox.TranslatePoint(new Point(),main);
    var searchBottom=searchBox.TranslatePoint(new Point(0,searchBox.ActualHeight),main);
    var searchRight=searchBox.TranslatePoint(new Point(searchBox.ActualWidth,0),main);
    Check(searchTop.X>=0&&searchRight.X<=LeftRailWidth&&Math.Abs((searchTop.X+searchRight.X)/2-LeftRailWidth/2)<1,"collapsed search is centered in sidebar "+family+" at "+size+" X="+searchTop.X+".."+searchRight.X);
    var hintsTop=hintBar.TranslatePoint(new Point(),main);
    var hintsBottom=hintBar.TranslatePoint(new Point(0,hintBar.ActualHeight),main);
    Check(searchTop.Y>0&&searchBottom.Y<=hintsTop.Y+1&&hintsBottom.Y<=main.ActualHeight+1,"sidebar search and all hints fit "+family+" at "+size+" X="+searchTop.X+".."+searchRight.X);
   }
  }
  Width=railTestWidth;Height=railTestHeight;SetFamily(railTestFamily);
  UpdateLayout();Check(System.Windows.Controls.Grid.GetColumn(scroll)==1&&searchBox.HorizontalAlignment==HorizontalAlignment.Left,"game grid and search occupy their designated columns");Check(!section.IsVisible&&!selectedName.IsVisible&&!status.IsVisible,"library heading and footer text are hidden");Check(toolNav[0].TranslatePoint(new Point(),main).X<LeftRailWidth&&hintBar.TranslatePoint(new Point(),main).X<LeftRailWidth,"buttons and hints share the left rail");
  var oldHost=games[0].Host;Render();Check(object.ReferenceEquals(oldHost,games[0].Host),"unchanged cards reuse existing UI tree");var cachedBitmap=reopened.Image.Source;LoadImage(reopened);Check(object.ReferenceEquals(cachedBitmap,reopened.Image.Source),"decoded cover bitmap reused");
  double originalTilesHeight=tiles.MinHeight;
  tiles.MinHeight=scroll.ActualHeight+600;
  UpdateLayout();
  var overlayThumb=(System.Windows.Controls.Primitives.ScrollBar)scroll.Template.FindName("PART_VerticalScrollBar",scroll);
  Check(Math.Abs(scroll.ViewportWidth-(scroll.ActualWidth-scroll.Padding.Left-scroll.Padding.Right))<2,"overlay scrollbar reserves no viewport width");
  scroll.ScrollToVerticalOffset(scroll.VerticalOffset<100?200:0);
  UpdateLayout();
  Check(overlayThumb.Opacity==.55&&overlayThumb.IsHitTestVisible,"scrolling shows translucent scrollbar");
  await System.Threading.Tasks.Task.Delay(3200);
  Check(overlayThumb.Opacity==0&&!overlayThumb.IsHitTestVisible,"scrollbar hides after three idle seconds");
  tiles.MinHeight=originalTilesHeight;
  UpdateLayout();
  var smooth=new SmoothScrollViewer{Width=180,Height=180,Content=new System.Windows.Controls.Border{Height=2000},VerticalScrollBarVisibility=System.Windows.Controls.ScrollBarVisibility.Auto};canvas.Children.Add(smooth);UpdateLayout();smooth.MoveTo(200);Check(smooth.HasAnimatedProperties,"row navigation uses smooth scrolling");Check(((System.Windows.Controls.Border)smooth.Content).RenderTransform.Value.IsIdentity,"viewport follows native scroll position without transform compensation");await System.Threading.Tasks.Task.Delay(60);double previousClock=(double)smooth.GetValue(SmoothScrollViewer.MotionClockProperty),previousVelocity=smooth.MotionVelocity;smooth.MoveTo(400);Check((double)smooth.GetValue(SmoothScrollViewer.MotionClockProperty)>=previousClock,"retarget does not restart motion clock");Check(Math.Abs(smooth.MotionVelocity-previousVelocity)<.001,"retarget preserves current velocity");await System.Threading.Tasks.Task.Delay(400);Check(Math.Abs(smooth.VerticalOffset-400)<2,"smooth scrolling reaches latest selection");smooth.WheelBy(-120);double wheelFirst=smooth.WheelTarget;smooth.WheelBy(-120);Check(smooth.WheelTarget>wheelFirst,"wheel notches accumulate destination instead of restarting");await System.Threading.Tasks.Task.Delay(600);Check(Math.Abs(smooth.VerticalOffset-smooth.WheelTarget)<2&&!smooth.WheelRunning,"wheel eases to target and releases rendering callback");smooth.WheelBy(120);Check(smooth.WheelTarget<smooth.VerticalOffset,"wheel reversal responds immediately");smooth.MoveTo(100);Check(!smooth.WheelRunning,"keyboard or scrollbar cancels wheel inertia");await System.Threading.Tasks.Task.Delay(250);smooth.ScrollToVerticalOffset(500);await System.Threading.Tasks.Task.Delay(40);smooth.MoveTo(100);await System.Threading.Tasks.Task.Delay(250);Check(Math.Abs(smooth.VerticalOffset-100)<2,"smooth navigation applies repeated destination after wheel input");canvas.Children.Remove(smooth);
  Check(animationRate==144,"animation target matches approved 144 Hz setting");var systemAnimation=new System.Windows.Media.Animation.DoubleAnimation(0,1,TimeSpan.FromMilliseconds(10));var transform=new System.Windows.Media.TranslateTransform();Animate(transform,System.Windows.Media.TranslateTransform.XProperty,systemAnimation);Check(System.Windows.Media.Animation.Timeline.GetDesiredFrameRate(systemAnimation)==144,"root animation timeline requests approved 144 frames");
  var motion=new ScrollMotion{Position=0,Target=500};motion.Advance(.03);double speed=motion.Velocity;motion.Target=900;Check(motion.Velocity==speed&&speed>0,"continuous target changes preserve velocity");motion.Advance(.015);Check(motion.Position>0&&motion.Position<900,"continuous motion progresses without jumping to target");double[] end=new double[3];int[] rates={60,90,144};for(int r=0;r<rates.Length;r++){var sample=new ScrollMotion{Target=700};for(int n=0;n<rates[r];n++)sample.Advance(1.0/rates[r]);end[r]=sample.Position;}Check(Math.Abs(end[0]-end[2])<.01&&Math.Abs(end[1]-700)<.2,"motion timing is stable across display cadences");Check(glowingGame==null||glowingGame.Lift.Y==0,"selection never jumps card position");
  var finiteSlide=new ScrollMotion{Target=500};finiteSlide.Advance(.35);Check(finiteSlide.Settled,"single page slide ends within a short fixed interval");var sameTarget=new ScrollMotion{Target=500};sameTarget.Advance(.10);sameTarget.Target=500;sameTarget.Advance(.23);Check(sameTarget.Settled,"repeated same target does not prolong page follow");
  ShowLaunch(g);Check(launchLayer!=null&&canvas.Children.Contains(launchLayer),"launch animation overlay");canvas.Children.Remove(launchLayer);launchLayer=null;
  File.WriteAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"test-result.txt"),"PASS: "+checks+" checks\nIsolated test workspace: "+home);Close();
 }catch(Exception ex){File.WriteAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"test-result.txt"),"FAIL: "+ex);Close();}}
}


















