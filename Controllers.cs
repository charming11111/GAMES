using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
partial class Shelf {
 [DllImport("kernel32",CharSet=CharSet.Unicode)] static extern IntPtr LoadLibrary(string p);
 [DllImport("SDL2.dll",CallingConvention=CallingConvention.Cdecl)] static extern int SDL_Init(uint f);
 [DllImport("SDL2.dll",CallingConvention=CallingConvention.Cdecl)] static extern void SDL_PumpEvents();
 [DllImport("SDL2.dll",CallingConvention=CallingConvention.Cdecl)] static extern int SDL_NumJoysticks();
 [DllImport("SDL2.dll",CallingConvention=CallingConvention.Cdecl)] static extern int SDL_IsGameController(int i);
 [DllImport("SDL2.dll",CallingConvention=CallingConvention.Cdecl)] static extern IntPtr SDL_GameControllerOpen(int i);
 [DllImport("SDL2.dll",CallingConvention=CallingConvention.Cdecl)] static extern void SDL_GameControllerClose(IntPtr p);
 [DllImport("SDL2.dll",CallingConvention=CallingConvention.Cdecl)] static extern int SDL_GameControllerGetAttached(IntPtr p);
 [DllImport("SDL2.dll",CallingConvention=CallingConvention.Cdecl)] static extern byte SDL_GameControllerGetButton(IntPtr p,int b);
 [DllImport("SDL2.dll",CallingConvention=CallingConvention.Cdecl)] static extern short SDL_GameControllerGetAxis(IntPtr p,int a);
 [DllImport("SDL2.dll",CallingConvention=CallingConvention.Cdecl)] static extern int SDL_GameControllerGetType(IntPtr p);
 [DllImport("SDL2.dll",CallingConvention=CallingConvention.Cdecl)] static extern void SDL_Quit();
 bool padsEnabled=false;int padCount=-1;List<IntPtr> pads=new List<IntPtr>();IntPtr activePad=IntPtr.Zero;bool[] lastButtons=new bool[21];int lastDirection=0;DateTime repeat=DateTime.MinValue;
 void InitPads(){try{string p=System.IO.Path.Combine(home,"SDL2.dll");if(!File.Exists(p))using(var src=typeof(Shelf).Assembly.GetManifestResourceStream("SDL2.dll"))using(var dst=File.Create(p))src.CopyTo(dst);if(LoadLibrary(p)==IntPtr.Zero)throw new Exception();if(SDL_Init(0x2000|0x4000)!=0)throw new Exception();padsEnabled=true;}catch{SetFamily("keyboard");}}
 void ClosePads(){if(!padsEnabled)return;foreach(var p in pads)SDL_GameControllerClose(p);pads.Clear();SDL_Quit();}
 void SetFamily(string f){if(padFamily==f)return;padFamily=f;DrawHints();}
 bool IsPs(int type){return type==3||type==4||type==7;}
 [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
 bool padInputArmed=false;
 bool AcceptPadInput(bool foreground,bool blocked,bool neutral){if(!foreground||blocked){padInputArmed=false;return false;}if(!padInputArmed){if(neutral)padInputArmed=true;return false;}return true;}
 bool PadForeground(){var h=new System.Windows.Interop.WindowInteropHelper(this).Handle;return h!=IntPtr.Zero&&IsVisible&&WindowState!=WindowState.Minimized&&GetForegroundWindow()==h;}
 void PollPads(){if(!padsEnabled)return;SDL_PumpEvents();int n=SDL_NumJoysticks();if(n!=padCount||pads.Any(p=>SDL_GameControllerGetAttached(p)==0)){foreach(var p in pads)SDL_GameControllerClose(p);pads.Clear();for(int i=0;i<n;i++)if(SDL_IsGameController(i)!=0){var p=SDL_GameControllerOpen(i);if(p!=IntPtr.Zero)pads.Add(p);}padCount=n;Array.Clear(lastButtons,0,lastButtons.Length);lastDirection=0;repeat=DateTime.MinValue;}
  if(pads.Count==0){SetFamily("keyboard");return;}IntPtr active=pads.Contains(activePad)?activePad:pads[0];foreach(var p in pads)if(Enumerable.Range(0,15).Any(b=>SDL_GameControllerGetButton(p,b)!=0)||Math.Abs((int)SDL_GameControllerGetAxis(p,0))>18000||Math.Abs((int)SDL_GameControllerGetAxis(p,1))>18000)active=p;
  if(active!=activePad){for(int i=0;i<lastButtons.Length;i++)lastButtons[i]=SDL_GameControllerGetButton(active,i)!=0;activePad=active;repeat=DateTime.MinValue;}SetFamily(IsPs(SDL_GameControllerGetType(active))?"ps":"xbox");bool[] now=new bool[21];for(int i=0;i<now.Length;i++)now[i]=SDL_GameControllerGetButton(active,i)!=0;
  bool foreground=PadForeground();int axisX=SDL_GameControllerGetAxis(active,0),axisY=SDL_GameControllerGetAxis(active,1);
  bool accept=AcceptPadInput(foreground,launching,!now.Any(v=>v)&&Math.Abs(axisX)<=10000&&Math.Abs(axisY)<=10000);
  if(!accept){lastButtons=now;lastDirection=0;repeat=DateTime.MinValue;return;}
  if(accept){if(now.Where((v,i)=>v&&!lastButtons[i]).Any()||Math.Abs((int)SDL_GameControllerGetAxis(active,0))>18000||Math.Abs((int)SDL_GameControllerGetAxis(active,1))>18000)LockPointer();if(now[6]&&!lastButtons[6]){if(modal=="settings")CloseModal();else OpenSettings();}else if(now[1]&&!lastButtons[1])Back();else if(now[0]&&!lastButtons[0]){Keyboard.ClearFocus();Confirm();}else if(modal==""){
    if(now[2]&&!lastButtons[2])OpenFilters();if(now[3]&&!lastButtons[3]&&visible.Count>selected){var g=visible[selected];State(g).Favorite=!State(g).Favorite;PlayUi(State(g).Favorite?"favorite":"remove");Save();Render(g.Path);}if(now[9]&&!lastButtons[9])Move(-columns*2);if(now[10]&&!lastButtons[10])Move(columns*2);if(now[4]&&!lastButtons[4]){if(filter=="recent")filter="all";else if(filter=="all")filter="favorite";else filter="recent";selected=0;Render();}}
   int direction=ReadDirection(now,SDL_GameControllerGetAxis(active,0),SDL_GameControllerGetAxis(active,1),lastDirection);int dx=Math.Abs(direction)==1?direction:0,dy=Math.Abs(direction)==100?direction/100:0;
   if(direction==0){repeat=DateTime.MinValue;lastDirection=0;}else if(direction!=lastDirection||DateTime.Now>=repeat){bool first=direction!=lastDirection;lastDirection=direction;Navigate(dx,dy);repeat=DateTime.Now.AddMilliseconds(first?180:(Math.Abs(direction)==100&&modal==""&&navZone=="games"?160:90));}}
  lastButtons=now;
 }
 static int ReadDirection(bool[] buttons,int x,int y,int previous){if(buttons[14])return 1;if(buttons[13])return -1;if(buttons[12])return 100;if(buttons[11])return -100;int threshold=previous==0?15000:10000;if(Math.Max(Math.Abs(x),Math.Abs(y))<=threshold)return 0;return Math.Abs(x)>=Math.Abs(y)?(x>0?1:-1):(y>0?100:-100);}
 FrameworkElement KeyCap(string code){bool ps=padFamily=="ps";var grid=new Grid{Width=code=="shoulders"?64:code=="menu"?35:28,Height=27};string shape=null,letter=null;Brush color=B("#BEC8DF");if(code=="a"){if(ps){shape="M6,6 L18,18 M18,6 L6,18";color=B("#94B9FF");}else{letter="A";color=B("#79DDA6");}}if(code=="b"){if(ps){shape="M20,12 A8,8 0 1 1 4,12 A8,8 0 1 1 20,12";color=B("#FF8B9F");}else{letter="B";color=B("#FF8B9F");}}if(code=="x"){if(ps){shape="M5,5 H19 V19 H5 Z";color=B("#DE9DF2");}else{letter="X";color=B("#94B9FF");}}if(code=="y"){if(ps){shape="M12,4 L21,20 H3 Z";color=B("#79DDA6");}else{letter="Y";color=B("#F2CE79");}}if(code=="menu")shape=paths["menu"];if(code=="shoulders")letter=ps?"L1  R1":"LB  RB";
  var outline=new Border{CornerRadius=new CornerRadius(code=="menu"||code=="shoulders"?7:14),BorderBrush=color,BorderThickness=new Thickness(1.2),Background=B("#122E3854")};grid.Children.Add(outline);if(shape!=null)grid.Children.Add(new System.Windows.Shapes.Path{Data=Geometry.Parse(shape),Stroke=color,StrokeThickness=1.8,Width=code=="menu"?15:14,Height=14,Stretch=Stretch.Uniform,VerticalAlignment=VerticalAlignment.Center,HorizontalAlignment=HorizontalAlignment.Center,StrokeStartLineCap=PenLineCap.Round,StrokeEndLineCap=PenLineCap.Round});if(letter!=null){var t=Txt(letter,code=="shoulders"?10:14,color);t.FontWeight=FontWeights.Bold;t.HorizontalAlignment=HorizontalAlignment.Center;grid.Children.Add(t);}return grid;}
 void Hint(string code,string text){var row=new StackPanel{Orientation=Orientation.Horizontal,Margin=new Thickness(6,2,6,2)};row.Children.Add(KeyCap(code));var label=Label(text,11,"Muted");label.Margin=new Thickness(6,0,0,0);row.Children.Add(label);hintBar.Children.Add(row);}
 void KeyboardHint(string key,string label){var row=new StackPanel{Orientation=Orientation.Horizontal,Margin=new Thickness(5,2,5,2)};var t=Label(key,10,"Fg");t.HorizontalAlignment=HorizontalAlignment.Center;var b=Box(t,"Surface",6);b.Padding=new Thickness(7,5,7,5);row.Children.Add(b);var l=Label(label,11,"Muted");l.Margin=new Thickness(6,0,0,0);row.Children.Add(l);hintBar.Children.Add(row);}
 string hintsKey="";
 void DrawHints(){string key=padFamily+"|"+(modal==""?navZone=="games"?"games":"controls":"modal");if(hintsKey==key)return;hintsKey=key;hintBar.Children.Clear();if(padFamily=="keyboard"){KeyboardHint("↑↓←→","导航");KeyboardHint("Enter",modal==""?(navZone=="games"?"菜单":"选择"):"确认");KeyboardHint("Esc","返回");KeyboardHint("F1","设置");return;}Hint("a",modal==""?(navZone=="games"?"菜单":"选择"):"确认");Hint("b","返回");if(modal==""){Hint("x","筛选");Hint("y","收藏");Hint("shoulders","翻页");}Hint("menu","设置");}
}







