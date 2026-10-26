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
 static BitmapImage ReadBitmap(string path,int width){var info=new FileInfo(path);string key=path+"|"+info.LastWriteTimeUtc.Ticks+"|"+info.Length+"|"+width;LinkedListNode<BitmapEntry> hit;if(bitmaps.TryGetValue(key,out hit)){bitmapOrder.Remove(hit);bitmapOrder.AddLast(hit);return hit.Value.Bitmap;}var b=new BitmapImage();using(var file=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete)){b.BeginInit();b.CacheOption=BitmapCacheOption.OnLoad;b.CreateOptions=BitmapCreateOptions.IgnoreColorProfile;b.DecodePixelWidth=width;b.StreamSource=file;b.EndInit();b.Freeze();}var node=bitmapOrder.AddLast(new BitmapEntry{Key=key,Bitmap=b});bitmaps[key]=node;while(bitmaps.Count>96){var first=bitmapOrder.First;bitmapOrder.RemoveFirst();bitmaps.Remove(first.Value.Key);}return b;}
 string CardFingerprint(Game g){var state=State(g);long stamp=File.Exists(g.Cover)?File.GetLastWriteTimeUtc(g.Cover).Ticks:0;return g.Name+"|"+stamp+"|"+state.Favorite+"|"+state.Hidden+"|"+state.LastPlayed+"|"+fullscreen+"|"+dark;}
 void RevealSelected(){if(selected<0||selected>=visible.Count)return;double top=(selected/Math.Max(1,columns))*tiles.ItemHeight+10,height=visible[selected].Card.Height;double bottom=top+height+12,view=scroll.ViewportHeight;if(double.IsNaN(top)||view<=0)return;double target=scroll.TargetOffset;if(top<target+8)target=top-12;else if(bottom>target+view-8)target=bottom-view+12;if(!testMode)scroll.MoveTo(target);else scroll.ScrollToVerticalOffset(target);}
 int? animationRate=SmoothScrollViewer.AnimationLimit;string focusStyleKey="";
 void SyncRefreshRate(){}
 void Animate(IAnimatable target,DependencyProperty property,AnimationTimeline animation){if(animation!=null)Timeline.SetDesiredFrameRate(animation,animationRate);target.BeginAnimation(property,animation);}
}







