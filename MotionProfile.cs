using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Media;
partial class Shelf {
 bool profileMode=false;
 async void RunMotionProfile(){try{Title="GAMES · 滚动诊断（自动结束）";Scan();UpdateLayout();Activate();await Task.Delay(250);var reports=new List<object>();
  foreach(int rate in new[]{90,144,144,90}){animationRate=rate;SmoothScrollViewer.DiagnosticRate=rate;StopGlow();Highlight(false);scroll.MoveTo(0);await Task.Delay(400);var times=new List<double>();var offsets=new List<double>();var model=new List<double>();var watch=Stopwatch.StartNew();TimeSpan prior=TimeSpan.MinValue;EventHandler capture=(o,e)=>{var frame=(RenderingEventArgs)e;if(frame.RenderingTime==prior)return;prior=frame.RenderingTime;times.Add(watch.Elapsed.TotalMilliseconds);offsets.Add(scroll.VerticalOffset);model.Add(scroll.DisplayOffset);};CompositionTarget.Rendering+=capture;
   for(int i=1;i<=3;i++){scroll.MoveTo(Math.Min(scroll.ScrollableHeight,i*tiles.ItemHeight));await Task.Delay(300);}await Task.Delay(200);CompositionTarget.Rendering-=capture;var gaps=times.Skip(1).Select((t,i)=>t-times[i]).OrderBy(x=>x).ToArray();int movingFrames=0,heldFrames=0,lagFrames=0;for(int i=1;i<times.Count;i++){if(Math.Abs(model[i]-model[i-1])>.2){movingFrames++;if(Math.Abs(offsets[i]-offsets[i-1])<.2)heldFrames++;if(Math.Abs(model[i]-offsets[i])>10)lagFrames++;}}reports.Add(new{TargetRate=rate,Frames=times.Count,MedianMs=gaps.Length>0?gaps[gaps.Length/2]:0,P95Ms=gaps.Length>0?gaps[(int)((gaps.Length-1)*.95)]:0,Over25Ms=gaps.Count(x=>x>25),MovingFrames=movingFrames,HeldFrames=heldFrames,LayoutLagFrames=lagFrames,Samples=times.Select((t,i)=>new{Ms=Math.Round(t,2),Native=Math.Round(offsets[i],2),Motion=Math.Round(model[i],2)}).ToArray()});}
  File.WriteAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"motion-profile.json"),json.Serialize(new{RateTarget=animationRate,RenderTier=RenderCapability.Tier>>16,Reports=reports}));Close();}catch(Exception ex){File.WriteAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"motion-profile.json"),ex.ToString());Close();}}
}


