using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

class ScrollMotion {
 public double Position,Velocity;public double DurationHint=.30;double target,start,startVelocity,elapsed,duration=.30;bool initialized=false;
 public double Target{get{return target;}set{if(initialized&&Math.Abs(target-value)<.05)return;target=value;start=Position;double distance=target-start;startVelocity=Velocity;duration=Math.Max(.12,DurationHint);double tangent=startVelocity*duration;if(distance>0)tangent=Math.Max(0,Math.Min(3*distance,tangent));else tangent=Math.Min(0,Math.Max(3*distance,tangent));startVelocity=tangent/duration;elapsed=0;initialized=true;}}
 public void Advance(double dt){elapsed+=Math.Max(0,dt);double t=Math.Min(1,elapsed/duration),t2=t*t,t3=t2*t;Position=(2*t3-3*t2+1)*start+(t3-2*t2+t)*startVelocity*duration+(-2*t3+3*t2)*target;Velocity=((6*t2-6*t)*start+(3*t2-4*t+1)*startVelocity*duration+(-6*t2+6*t)*target)/duration;if(t>=1){Position=target;Velocity=0;}}
 public bool Settled{get{return Position==target&&Velocity==0;}}
}
class SmoothScrollViewer:ScrollViewer {
 public const int AnimationLimit=144;public static int DiagnosticRate=0;
 public static readonly DependencyProperty MotionClockProperty=DependencyProperty.Register("MotionClock",typeof(double),typeof(SmoothScrollViewer),new PropertyMetadata(0.0,(o,e)=>((SmoothScrollViewer)o).AdvanceMotion()));
 readonly ScrollMotion motion=new ScrollMotion();long lastTick;bool running=false,wheel=false;
 public event EventHandler WheelStarted;
 public bool WheelRunning{get{return running&&wheel;}}public double WheelTarget{get{return motion.Target;}}public double TargetOffset{get{return running?motion.Target:VerticalOffset;}}public double DisplayOffset{get{return running?motion.Position:VerticalOffset;}}public double MotionVelocity{get{return motion.Velocity;}}
 public SmoothScrollViewer(){Unloaded+=(o,e)=>StopMotion(false);}
 void SetTarget(double offset,bool fromWheel){offset=Math.Max(0,Math.Min(ScrollableHeight,offset));if(!running&&Math.Abs(offset-VerticalOffset)<.1)return;motion.DurationHint=fromWheel?.20:.30;if(!running){motion.Position=VerticalOffset;motion.Velocity=0;motion.Target=offset;lastTick=Stopwatch.GetTimestamp();running=true;wheel=fromWheel;var clock=new DoubleAnimation(0,1,TimeSpan.FromSeconds(1)){RepeatBehavior=RepeatBehavior.Forever};Timeline.SetDesiredFrameRate(clock,DiagnosticRate>0?DiagnosticRate:AnimationLimit);BeginAnimation(MotionClockProperty,clock);}else{motion.Target=offset;wheel=fromWheel;}}
 void AdvanceMotion(){if(!running)return;long now=Stopwatch.GetTimestamp();double dt=(double)(now-lastTick)/Stopwatch.Frequency;lastTick=now;motion.Advance(dt);ScrollToVerticalOffset(motion.Position);if(motion.Settled){ScrollToVerticalOffset(motion.Target);StopMotion(false);}}
 void StopMotion(bool keepVisual){if(!running)return;double position=motion.Position;running=false;wheel=false;BeginAnimation(MotionClockProperty,null);motion.Velocity=0;if(keepVisual){ScrollToVerticalOffset(position);UpdateLayout();}}
 protected override void OnPreviewMouseDown(MouseButtonEventArgs e){StopMotion(true);base.OnPreviewMouseDown(e);}
 protected override void OnPreviewMouseWheel(MouseWheelEventArgs e){if(ScrollableHeight>0&&SystemParameters.WheelScrollLines!=0){WheelBy(e.Delta);e.Handled=true;return;}base.OnPreviewMouseWheel(e);}
 public void WheelBy(int delta){if(delta==0||ScrollableHeight<=0)return;double step=SystemParameters.WheelScrollLines<0?ViewportHeight*.85:Math.Max(40,Math.Min(180,SystemParameters.WheelScrollLines*40));bool wasWheel=WheelRunning;SetTarget(TargetOffset-delta/120.0*step,true);if(!wasWheel&&WheelRunning&&WheelStarted!=null)WheelStarted(this,EventArgs.Empty);}
 public void MoveTo(double offset){SetTarget(offset,false);}
}





