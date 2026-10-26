using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
partial class Shelf {
 bool launchWaitCancelled=false;
 [DllImport("user32.dll")]static extern bool EnumWindows(EnumWindow callback,IntPtr param);
 delegate bool EnumWindow(IntPtr window,IntPtr param);
 [DllImport("user32.dll")]static extern bool IsWindowVisible(IntPtr h);
 [DllImport("user32.dll")]static extern uint GetWindowThreadProcessId(IntPtr h,out uint pid);
 [DllImport("user32.dll")]static extern bool GetWindowRect(IntPtr h,out WindowRect r);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)]static extern int GetWindowText(IntPtr h,StringBuilder text,int count);
 [DllImport("dwmapi.dll")]static extern int DwmGetWindowAttribute(IntPtr h,int attribute,out int value,int size);
 [StructLayout(LayoutKind.Sequential)]struct WindowRect{public int Left,Top,Right,Bottom;}
 [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)]struct ProcessEntry{public uint Size,Usage,Pid;public IntPtr Heap;public uint Module,Threads,Parent;public int Priority;public uint Flags;[MarshalAs(UnmanagedType.ByValTStr,SizeConst=260)]public string Name;}
 [DllImport("kernel32.dll")]static extern IntPtr CreateToolhelp32Snapshot(uint flags,uint pid);
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode)]static extern bool Process32FirstW(IntPtr snapshot,ref ProcessEntry entry);
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode)]static extern bool Process32NextW(IntPtr snapshot,ref ProcessEntry entry);
 [DllImport("kernel32.dll")]static extern bool CloseHandle(IntPtr handle);
 class LaunchTarget {public string Exe,Directory;public HashSet<int> Before=new HashSet<int>(),Tracked=new HashSet<int>();public HashSet<string> Titles=new HashSet<string>();}
 static bool IsBootstrap(string path){string n=System.IO.Path.GetFileNameWithoutExtension(path??"").ToLowerInvariant();return new[]{"steam","epicgameslauncher","epicwebhelper","explorer","eadesktop","upc","ubisoftconnect","galaxyclient","crashreporter","crashpad_handler"}.Contains(n)||n.Contains("launcher")||n.Contains("启动")||n.Contains("crash");}
 LaunchTarget ResolveLaunch(Game game,bool snapshot=true){var target=new LaunchTarget();string exe=game.Path,working=null;
  if(System.IO.Path.GetExtension(game.Path).Equals(".lnk",StringComparison.OrdinalIgnoreCase)){object shell=null,link=null;try{var type=Type.GetTypeFromProgID("WScript.Shell");shell=Activator.CreateInstance(type);link=type.InvokeMember("CreateShortcut",BindingFlags.InvokeMethod,null,shell,new object[]{game.Path});exe=(string)link.GetType().InvokeMember("TargetPath",BindingFlags.GetProperty,null,link,null);working=(string)link.GetType().InvokeMember("WorkingDirectory",BindingFlags.GetProperty,null,link,null);}catch{}finally{if(link!=null&&Marshal.IsComObject(link))Marshal.ReleaseComObject(link);if(shell!=null&&Marshal.IsComObject(shell))Marshal.ReleaseComObject(shell);}}
  else if(System.IO.Path.GetExtension(game.Path).Equals(".url",StringComparison.OrdinalIgnoreCase)){foreach(string line in File.ReadAllLines(game.Path)){if(line.StartsWith("IconFile=",StringComparison.OrdinalIgnoreCase)){string icon=Environment.ExpandEnvironmentVariables(line.Substring(9).Trim().Trim('"'));if(System.IO.Path.GetExtension(icon).Equals(".exe",StringComparison.OrdinalIgnoreCase))exe=icon;}}}
  if(!string.IsNullOrEmpty(exe)&&System.IO.Path.GetExtension(exe).Equals(".exe",StringComparison.OrdinalIgnoreCase)){target.Exe=exe;target.Directory=System.IO.Path.GetDirectoryName(exe);}else if(!string.IsNullOrEmpty(working))target.Directory=working;
  foreach(string n in new[]{game.Name,game.OriginalName,aliases.ContainsKey(game.OriginalName)?aliases[game.OriginalName]:null})if(!string.IsNullOrWhiteSpace(n)&&Norm(n).Length>=4)target.Titles.Add(Norm(n));
  if(snapshot)foreach(var p in Process.GetProcesses()){try{target.Before.Add(p.Id);if(target.Exe!=null&&string.Equals(p.ProcessName,System.IO.Path.GetFileNameWithoutExtension(target.Exe),StringComparison.OrdinalIgnoreCase)&&!IsBootstrap(target.Exe))target.Tracked.Add(p.Id);}catch{}finally{p.Dispose();}}return target;
 }
 static void TrackChildren(LaunchTarget target){var snapshot=CreateToolhelp32Snapshot(2,0);if(snapshot==IntPtr.Zero||snapshot==(IntPtr)(-1))return;try{var list=new List<ProcessEntry>();var e=new ProcessEntry{Size=(uint)Marshal.SizeOf(typeof(ProcessEntry))};if(Process32FirstW(snapshot,ref e))do{list.Add(e);}while(Process32NextW(snapshot,ref e));for(int pass=0;pass<4;pass++)foreach(var p in list)if(target.Tracked.Contains((int)p.Parent))target.Tracked.Add((int)p.Pid);
   foreach(var p in list){if(target.Before.Contains((int)p.Pid)||IsBootstrap(p.Name))continue;if(target.Exe!=null&&string.Equals(p.Name,System.IO.Path.GetFileName(target.Exe),StringComparison.OrdinalIgnoreCase)){target.Tracked.Add((int)p.Pid);continue;}if(target.Directory==null||target.Directory.Length<5||IsBootstrap(target.Exe))continue;try{using(var process=Process.GetProcessById((int)p.Pid)){string path=process.MainModule.FileName;if(path.StartsWith(target.Directory.TrimEnd('\\')+"\\",StringComparison.OrdinalIgnoreCase))target.Tracked.Add((int)p.Pid);}}catch{}}
  }finally{CloseHandle(snapshot);}}
 static IntPtr FindGameWindow(LaunchTarget target){IntPtr found=IntPtr.Zero;int own=Process.GetCurrentProcess().Id;EnumWindows((h,p)=>{if(!IsWindowVisible(h))return true;uint pid;GetWindowThreadProcessId(h,out pid);if(pid==own)return true;WindowRect r;if(!GetWindowRect(h,out r)||r.Right-r.Left<320||r.Bottom-r.Top<200)return true;int cloaked=0;try{DwmGetWindowAttribute(h,14,out cloaked,4);}catch{}if(cloaked!=0)return true;var text=new StringBuilder(512);GetWindowText(h,text,512);string title=text.ToString();if(title.IndexOf("launcher",StringComparison.OrdinalIgnoreCase)>=0||title.Contains("启动器")||title.IndexOf("crash",StringComparison.OrdinalIgnoreCase)>=0)return true;
   bool match=target.Tracked.Contains((int)pid);if(match){try{using(var process=Process.GetProcessById((int)pid))if(IsBootstrap(process.ProcessName))return true;}catch{return true;}}
   if(!match&&!target.Before.Contains((int)pid)){string normalized=Norm(title);match=target.Titles.Any(n=>normalized.Contains(n));}if(match){found=h;return false;}return true;},IntPtr.Zero);return found;}
 async Task<bool> WaitForGameWindow(LaunchTarget target,TimeSpan timeout){var clock=Stopwatch.StartNew();IntPtr last=IntPtr.Zero;long firstSeen=0;while(!closing&&!launchWaitCancelled&&clock.Elapsed<timeout){IntPtr window=await Task.Run(()=>{TrackChildren(target);return FindGameWindow(target);});if(window!=IntPtr.Zero){if(window!=last){last=window;firstSeen=clock.ElapsedMilliseconds;}else if(clock.ElapsedMilliseconds-firstSeen>=600)return true;}else last=IntPtr.Zero;await Task.Delay(150);}return false;}
 static void RunWindowProbe(){var app=new Application{ShutdownMode=ShutdownMode.OnExplicitShutdown};var window=new Window{Title="GAMES Window Detection Probe",Width=440,Height=300,ShowInTaskbar=false};var show=new DispatcherTimer{Interval=TimeSpan.FromMilliseconds(350)};show.Tick+=(o,e)=>{show.Stop();window.Show();};var end=new DispatcherTimer{Interval=TimeSpan.FromSeconds(2)};end.Tick+=(o,e)=>{end.Stop();app.Shutdown();};show.Start();end.Start();app.Run();}
}
