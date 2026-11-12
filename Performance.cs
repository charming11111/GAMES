using System.Windows.Media.Imaging;
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Input;
using System.Diagnostics;
partial class Shelf {
 class BitmapEntry {public string Key;public BitmapImage Bitmap;}
 static Dictionary<string,LinkedListNode<BitmapEntry>> bitmaps=new Dictionary<string,LinkedListNode<BitmapEntry>>();static LinkedList<BitmapEntry> bitmapOrder=new LinkedList<BitmapEntry>();
 static string BitmapKey(string path,int width){
  var file=new FileInfo(path);
  return path+"|"+file.LastWriteTimeUtc.Ticks+"|"+file.Length+"|"+width;
 }
 static BitmapImage FindBitmap(string key){
  LinkedListNode<BitmapEntry> hit;
  if(!bitmaps.TryGetValue(key,out hit))return null;
  bitmapOrder.Remove(hit);
  bitmapOrder.AddLast(hit);
  return hit.Value.Bitmap;
 }
 static BitmapImage DecodeBitmap(string path,int width){
  var bitmap=new BitmapImage();
  using(var file=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete)){
   bitmap.BeginInit();
   bitmap.CacheOption=BitmapCacheOption.OnLoad;
   bitmap.CreateOptions=BitmapCreateOptions.IgnoreColorProfile;
   bitmap.DecodePixelWidth=width;
   bitmap.StreamSource=file;
   bitmap.EndInit();
   bitmap.Freeze();
  }
  return bitmap;
 }
 static void CacheBitmap(string key,BitmapImage bitmap){
  if(FindBitmap(key)!=null)return;
  bitmaps[key]=bitmapOrder.AddLast(new BitmapEntry{Key=key,Bitmap=bitmap});
  while(bitmaps.Count>8){
   var oldest=bitmapOrder.First;
   bitmapOrder.RemoveFirst();
   bitmaps.Remove(oldest.Value.Key);
  }
 }
 static BitmapImage ReadBitmap(string path,int width){
  string key=BitmapKey(path,width);
  var bitmap=FindBitmap(key);
  if(bitmap!=null)return bitmap;
  bitmap=DecodeBitmap(path,width);
  CacheBitmap(key,bitmap);
  return bitmap;
 }
 bool coverDecodeRunning;
 async void DecodeViewportCover(Game game,string key,int width){
  coverDecodeRunning=true;
  var image=game.Image;
  string path=game.Cover;
  try{
   var bitmap=await System.Threading.Tasks.Task.Run(()=>DecodeBitmap(path,width));
   if(closing||!IsVisible||!viewportCovers.Contains(game)||game.Image!=image)return;
   if(!File.Exists(path)||BitmapKey(path,width)!=key)return;
   CacheBitmap(key,bitmap);
   SetViewportCover(game,bitmap);
  }catch(Exception ex){
   if(File.Exists(path)&&BitmapKey(path,width)==key)ReportCoverError(game,ex);
  }finally{
   coverDecodeRunning=false;
   QueueViewportCovers();
  }
 }
 void SetViewportCover(Game game,BitmapImage bitmap){
  SetCoverImage(game,bitmap);
  game.CoverPlaceholder.Visibility=Visibility.Collapsed;
  if(game.BackdropImage!=null)game.BackdropImage.Source=bitmap;
 }
 readonly HashSet<Game> viewportCovers=new HashSet<Game>();
 bool viewportUpdateQueued;
 void QueueViewportCovers(){
  if(viewportUpdateQueued||closing)return;
  viewportUpdateQueued=true;
  Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,new Action(()=>{
   viewportUpdateQueued=false;
   if(!closing)UpdateViewportCovers();
  }));
 }
 int coverFirst=-1,coverLast=-1;
 void GetCoverRange(out int first,out int last){
  first=last=0;
  double rowHeight=tiles.ItemHeight;
  if(!IsVisible||!(rowHeight>0)||!(scroll.ViewportHeight>0))return;
  first=Math.Min(visible.Count,Math.Max(0,(int)Math.Floor(scroll.VerticalOffset/rowHeight)*columns));
  last=Math.Min(visible.Count,((int)Math.Ceiling((scroll.VerticalOffset+scroll.ViewportHeight)/rowHeight)+1)*columns);
 }
 void QueueCoversWhenRowsChange(){
  int first,last;
  GetCoverRange(out first,out last);
  if(first!=coverFirst||last!=coverLast)QueueViewportCovers();
 }
 void UpdateViewportCovers(){
  int first,last;
  GetCoverRange(out first,out last);
  coverFirst=first;
  coverLast=last;
  var nearby=new HashSet<Game>();
  for(int i=first;i<last;i++)nearby.Add(visible[i]);
  foreach(var g in viewportCovers)if(!nearby.Contains(g)){
   g.Image.Source=null;
   if(g.BackdropImage!=null)g.BackdropImage.Source=null;
   g.CoverPlaceholder.Visibility=Visibility.Visible;
  }
  viewportCovers.Clear();
  viewportCovers.UnionWith(nearby);
  if(!IsVisible){bitmaps.Clear();bitmapOrder.Clear();return;}
  for(int i=first;i<last;i++){
   var game=visible[i];
   if(game.Image.Source!=null||!HasCover(game.Cover))continue;
   if(testMode&&!profileMode){
    if(LoadImage(game))SetViewportCover(game,(BitmapImage)game.Image.Source);
    continue;
   }
   int width=CoverDecodeWidth(game);
   try{
    string key=BitmapKey(game.Cover,width);
    var bitmap=FindBitmap(key);
    if(bitmap!=null)SetViewportCover(game,bitmap);
    else if(!coverDecodeRunning)DecodeViewportCover(game,key,width);
   }catch(Exception ex){ReportCoverError(game,ex);}
  }
 }
 string CardFingerprint(Game g){var state=State(g);long stamp=File.Exists(g.Cover)?File.GetLastWriteTimeUtc(g.Cover).Ticks:0;return g.Name+"|"+stamp+"|"+state.Favorite+"|"+state.Hidden+"|"+state.LastPlayed+"|"+fullscreen+"|"+dark;}
 void RevealSelected(){if(selected<0||selected>=visible.Count)return;double top=(selected/Math.Max(1,columns))*tiles.ItemHeight+10,height=visible[selected].Card.Height;double bottom=top+height+12,view=scroll.ViewportHeight;if(double.IsNaN(top)||view<=0)return;double target=scroll.TargetOffset;if(top<target+8)target=top-12;else if(bottom>target+view-8)target=bottom-view+12;if(!testMode)scroll.MoveTo(target);else scroll.ScrollToVerticalOffset(target);}
 int? animationRate=SmoothScrollViewer.AnimationLimit;string focusStyleKey="";
 void SyncRefreshRate(){}
 void Animate(IAnimatable target,DependencyProperty property,AnimationTimeline animation){if(animation!=null)Timeline.SetDesiredFrameRate(animation,animationRate);target.BeginAnimation(property,animation);}
}







