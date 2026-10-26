using System;
using System.Windows;
using System.Windows.Threading;
partial class Shelf {
 static System.Threading.Mutex instanceMutex;static bool ownsInstance=false;
 [System.Runtime.InteropServices.DllImport("user32.dll",CharSet=System.Runtime.InteropServices.CharSet.Unicode)]static extern uint RegisterWindowMessage(string name);
 [System.Runtime.InteropServices.DllImport("user32.dll")]static extern bool PostMessage(IntPtr h,uint message,IntPtr w,IntPtr l);
 static uint restoreMessage=RegisterWindowMessage("GAMES.GameShelf.Restore.v1");
 static uint coverDiagnosticsMessage=RegisterWindowMessage("GAMES.GameShelf.CoverState.v1");
 static bool AcquireInstance(){bool created;instanceMutex=new System.Threading.Mutex(true,@"Local\GAMES.GameShelf.Instance",out created);ownsInstance=created;if(created)return true;PostMessage((IntPtr)0xffff,restoreMessage,IntPtr.Zero,IntPtr.Zero);instanceMutex.Dispose();instanceMutex=null;return false;}
 static void ReleaseInstance(){if(instanceMutex==null)return;if(ownsInstance){instanceMutex.ReleaseMutex();ownsInstance=false;}instanceMutex.Dispose();instanceMutex=null;}
 void BringBack(){if(inTray)RestoreFromTray();else {Show();if(WindowState==WindowState.Minimized)WindowState=WindowState.Normal;Activate();main.Focus();}}
 System.Windows.Forms.NotifyIcon tray;System.Drawing.Icon trayArtwork;WindowState trayState=WindowState.Normal;bool inTray=false;
 void InitTray(){if(testMode)return;try{using(var stream=typeof(Shelf).Assembly.GetManifestResourceStream("app.ico"))using(var icon=new System.Drawing.Icon(stream))trayArtwork=(System.Drawing.Icon)icon.Clone();tray=new System.Windows.Forms.NotifyIcon{Icon=trayArtwork,Text="GAMES · 游戏启动器",Visible=false};var menu=new System.Windows.Forms.ContextMenuStrip();menu.Items.Add("打开 GAMES",null,(o,e)=>Dispatcher.BeginInvoke(new Action(RestoreFromTray)));menu.Items.Add(new System.Windows.Forms.ToolStripSeparator());menu.Items.Add("退出 GAMES",null,(o,e)=>Dispatcher.BeginInvoke(new Action(()=>Close())));tray.ContextMenuStrip=menu;tray.MouseClick+=(o,e)=>{if(e.Button==System.Windows.Forms.MouseButtons.Left)Dispatcher.BeginInvoke(new Action(RestoreFromTray));};StateChanged+=(o,e)=>{if(WindowState==WindowState.Minimized&&!inTray)Dispatcher.BeginInvoke(new Action(MinimizeToTray));else if(WindowState!=WindowState.Minimized&&!inTray)trayState=WindowState;};}catch(Exception ex){status.Text="托盘初始化失败 · "+ex.Message;}}
 void MinimizeToTray(){if(inTray)return;if(tray==null&&!testMode){WindowState=WindowState.Minimized;return;}if(WindowState!=WindowState.Minimized)trayState=WindowState;inTray=true;PlayUi("switch");if(tray!=null)tray.Visible=true;Hide();ShowInTaskbar=false;if(WindowState==WindowState.Minimized)WindowState=trayState;}
 void RestoreFromTray(){if(!inTray)return;inTray=false;ShowInTaskbar=true;Show();WindowState=trayState;if(tray!=null)tray.Visible=false;Activate();main.Focus();RefreshNavFocus();}
 void CloseTray(){if(tray!=null){tray.Visible=false;var menu=tray.ContextMenuStrip;tray.Dispose();if(menu!=null)menu.Dispose();tray=null;}if(trayArtwork!=null){trayArtwork.Dispose();trayArtwork=null;}}
}
