using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Threading;

partial class Shelf {



 sealed class PlaySession {
  public Game Game;
  public Process Process;
  public int Pid;
  public GameState State;

 }

 string startingGamePath;
 long launchRequestedTicks;
 string PlayStatus(Game game){
  if(game.Path==startingGamePath)return "启动中";
  return playSessions.Any(session=>session.Game.Path==game.Path)?"游玩中":null;
 }

 readonly List<PlaySession> playSessions=new List<PlaySession>();
 readonly Stopwatch playClock=new Stopwatch();
 DispatcherTimer playTimer;
 long playSaved;
 bool playDirty;

 void BeginPlayTracking(Game game,IntPtr window){
  uint pid;
  GetWindowThreadProcessId(window,out pid);
  if(pid==0||playSessions.Any(s=>s.Pid==(int)pid&&!s.Process.HasExited))return;
  Process process;
  try{process=Process.GetProcessById((int)pid);if(process.HasExited){process.Dispose();return;}}
  catch(ArgumentException){return;}
  if(playSessions.Count>0)SamplePlaySessions();
  var state=State(game);
  state.LastPlayed=DateTime.UtcNow.Ticks;
  state.PlayCount++;
  state.PlayPid=process.Id;
  state.PlayProcessStart=process.StartTime.ToUniversalTime().Ticks;
  state.PlaySessionBase=state.PlayTicks;
  state.PlaySessionStart=Math.Max(state.PlayProcessStart,launchRequestedTicks);
  AttachPlaySession(game,state,process);
  Save();
 }

 void RestorePlayTracking(){
  foreach(var game in games){
   var state=State(game);
   if(state.PlayPid==0||state.PlaySessionStart==0)continue;
   Process process=null;
   try{
    process=Process.GetProcessById(state.PlayPid);
    if(!process.HasExited&&process.StartTime.ToUniversalTime().Ticks==state.PlayProcessStart){
     AttachPlaySession(game,state,process);
     continue;
    }
   }catch(ArgumentException){}catch(InvalidOperationException){}catch(System.ComponentModel.Win32Exception){}
   if(process!=null)process.Dispose();
   state.PlayPid=0;
   state.PlayProcessStart=0;
  }
  if(playSessions.Count>0){SamplePlaySessions(true);Render();}
 }

 void AttachPlaySession(Game game,GameState state,Process process){
  // Retain a handle so Windows can report the exit time after the process is gone.
  try{var handle=process.Handle;}catch(InvalidOperationException){}catch(System.ComponentModel.Win32Exception){}
  playSessions.Add(new PlaySession{Game=game,State=state,Process=process,Pid=process.Id});
  if(playTimer==null){
   playTimer=new DispatcherTimer(DispatcherPriority.Background){Interval=TimeSpan.FromSeconds(1)};
   playTimer.Tick+=(o,e)=>SamplePlaySessions();
  }
  if(!playTimer.IsEnabled){
   playClock.Restart();
   playSaved=0;
   playTimer.Start();
  }
 }

 void SamplePlaySessions(bool flush=false){
  long now=playClock.Elapsed.Ticks;
  long utc=DateTime.UtcNow.Ticks;
  bool ended=false;
  for(int i=playSessions.Count-1;i>=0;i--){
   var session=playSessions[i];
   bool exited=session.Process.HasExited;
   long until=utc;
   if(exited){
    try{until=session.Process.ExitTime.ToUniversalTime().Ticks;}
    catch(InvalidOperationException){}
    catch(System.ComponentModel.Win32Exception){}
   }
   var state=session.State;
   long total=state.PlaySessionBase+Math.Max(0,until-state.PlaySessionStart);
   if(total>state.PlayTicks){state.PlayTicks=total;playDirty=true;}
   if(!exited)continue;
   state.PlayPid=0;
   state.PlayProcessStart=0;
   state.PlaySessionStart=0;
   state.PlaySessionBase=0;
   playDirty=true;
   session.Process.Dispose();
   playSessions.RemoveAt(i);
   ended=true;
  }
  if(playDirty&&(flush||ended||closing||now-playSaved>=30*TimeSpan.TicksPerSecond)){
   try{Save();playDirty=false;playSaved=now;}
   catch(System.IO.IOException){}
   catch(UnauthorizedAccessException){}
  }
  if(playSessions.Count==0&&!playDirty){playTimer.Stop();playClock.Stop();}
  if(ended&&!closing){
   string keep=selected>=0&&selected<visible.Count?visible[selected].Path:null;
   Render(keep);
  }
 }

 void StopPlayTracking(){
  if(playTimer==null)return;
  SamplePlaySessions(true);
  playTimer.Stop();
  foreach(var session in playSessions)session.Process.Dispose();
  playSessions.Clear();
 }

 static string PlayDuration(long ticks){
  if(ticks<=0)return "—";
  long minutes=ticks/TimeSpan.TicksPerMinute;
  if(minutes==0)return "<1 分钟";
  return minutes<60?minutes+" 分钟":minutes/60+" 小时 "+(minutes%60)+" 分";
 }
}
