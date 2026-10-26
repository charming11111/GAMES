using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
partial class Shelf {
 string navZone="games";int navIndex=0;List<Button> toolNav=new List<Button>(),tabNav=new List<Button>();Border searchBox;Button navButton;Game glowingGame;TextBlock focusCaption;
 DateTime lastVerticalKey=DateTime.MinValue;bool controllerNavigation=false;HwndSource inputSource;TextBox virtualInput;
 [DllImport("user32.dll")] static extern short GetKeyState(int key);
 [StructLayout(LayoutKind.Sequential)]struct CursorPoint {public int X,Y;}
 [DllImport("user32.dll")]static extern bool GetCursorPos(out CursorPoint p);
 CursorPoint pointerAnchor;bool pointerKnown=false;
 void LockPointer(){if(GetCursorPos(out pointerAnchor))pointerKnown=true;controllerNavigation=true;}
 bool PointerMoved(int x,int y){if(!pointerKnown){pointerAnchor.X=x;pointerAnchor.Y=y;pointerKnown=true;return false;}if(Math.Abs(pointerAnchor.X-x)+Math.Abs(pointerAnchor.Y-y)<3)return false;pointerAnchor.X=x;pointerAnchor.Y=y;controllerNavigation=false;return true;}
 void ObservePointer(object sender,MouseEventArgs e){CursorPoint p;if(GetCursorPos(out p))PointerMoved(p.X,p.Y);}
 void InstallKeyboard(){main.Focusable=true;Focusable=true;PreviewMouseMove+=ObservePointer;Loaded+=(o,e)=>{inputSource=PresentationSource.FromVisual(this) as HwndSource;if(inputSource!=null){inputSource.AddHook(NativeKeyboard);ComponentDispatcher.ThreadPreprocessMessage+=ThreadKeyboard;}};Activated+=(o,e)=>{if(!testMode)RecoverCachedCoverCards();if(modal==""&&!search.IsKeyboardFocused)main.Focus();};Closed+=(o,e)=>{ComponentDispatcher.ThreadPreprocessMessage-=ThreadKeyboard;if(inputSource!=null)inputSource.RemoveHook(NativeKeyboard);};}
 void ThreadKeyboard(ref MSG message,ref bool handled){if(handled||inputSource==null||message.hwnd!=inputSource.Handle)return;NativeKeyboard(message.hwnd,message.message,message.wParam,message.lParam,ref handled);}
 IntPtr NativeKeyboard(IntPtr hwnd,int message,IntPtr wParam,IntPtr lParam,ref bool handled){if(message==coverDiagnosticsMessage){DumpRunningCoverState();handled=true;return IntPtr.Zero;}if(message==restoreMessage){Dispatcher.BeginInvoke(new Action(BringBack));handled=true;return IntPtr.Zero;}if(message!=0x100||(!IsActive&&!testMode))return IntPtr.Zero;var key=KeyInterop.KeyFromVirtualKey(wParam.ToInt32());ModifierKeys mods=ModifierKeys.None;if((GetKeyState(0x11)&0x8000)!=0)mods|=ModifierKeys.Control;if((GetKeyState(0x10)&0x8000)!=0)mods|=ModifierKeys.Shift;
  bool repeating=(lParam.ToInt64()&(1L<<30))!=0;if((key==Key.Up||key==Key.Down)&&modal==""&&navZone=="games"){if(repeating&&(DateTime.UtcNow-lastVerticalKey).TotalMilliseconds<160){handled=true;return IntPtr.Zero;}lastVerticalKey=DateTime.UtcNow;}if(repeating&&(key==Key.F5||key==Key.F1||key==Key.F11||key==Key.Enter||key==Key.Escape)){handled=true;return IntPtr.Zero;}handled=ProcessKey(key,mods);return IntPtr.Zero;}
 bool ProcessKey(Key key,ModifierKeys mods){if(launching){if(key==Key.Escape)launchWaitCancelled=true;return key==Key.Enter||key==Key.Escape;}if(key==Key.F5){ManualScan();return true;}if(key==Key.F11){ToggleFull();if(modal=="settings")OpenSettings();return true;}if(key==Key.F1){if(modal=="settings")CloseModal();else OpenSettings();return true;}if(key==Key.Escape){Back();return true;}if(key==Key.F&&(mods&ModifierKeys.Control)!=0){CloseModal();navZone="search";ExpandSearch(true);search.Focus();RefreshNavFocus();return true;}
  if(Keyboard.FocusedElement is TextBox){if(key==Key.Enter){if(modal=="search"){CloseModal();navZone="games";main.Focus();RefreshNavFocus();return true;}if(modal=="appid"){Confirm();return true;}navZone="games";main.Focus();RefreshNavFocus();return true;}if(key==Key.Down&&modal==""){navZone="tabs";navIndex=0;main.Focus();RefreshNavFocus();return true;}if(key!=Key.Tab)return false;}
  if(key==Key.Tab&&modal==""){string[] zones={"tools","search","tabs","games"};int index=Array.IndexOf(zones,navZone);index=(index+((mods&ModifierKeys.Shift)!=0?3:1))%4;navZone=zones[index];navIndex=0;main.Focus();RefreshNavFocus();return true;}
  if(key==Key.Enter||key==Key.Space){Confirm();return true;}int dx=key==Key.Left?-1:key==Key.Right?1:0,dy=key==Key.Up?-1:key==Key.Down?1:0;if(dx!=0||dy!=0){Navigate(dx,dy);return true;}return false;}
 void Navigate(int dx,int dy){LockPointer();PlayUi("move");main.Focus();if(modal!=""){if(modal=="search")MoveModal(dy!=0?dy*10:dx);else MoveModal(dy!=0?dy:dx);return;}controllerNavigation=true;
  if(navZone=="games"){if(visible.Count==0){navZone="tabs";navIndex=0;}else if(dy<0&&selected<columns){navZone="tabs";navIndex=NearestTab(selected);}else {int rowStart=(selected/columns)*columns,rowEnd=Math.Min(rowStart+columns-1,visible.Count-1);if(dx!=0)selected=Math.Max(rowStart,Math.Min(rowEnd,selected+dx));else if(dy>0)selected=Math.Min(visible.Count-1,selected+columns);else selected=Math.Max(0,selected-columns);RevealSelected();}}
  else if(navZone=="tabs"){if(dx!=0)navIndex=Math.Max(0,Math.Min(tabNav.Count-1,navIndex+dx));if(dy<0)navZone="search";if(dy>0&&visible.Count>0){selected=Math.Min(visible.Count-1,navIndex<3?navIndex:columns-1);navZone="games";RevealSelected();}}
  else if(navZone=="search"){if(dy<0){navZone="tools";navIndex=Math.Max(0,toolNav.Count-2);}else if(dy>0){navZone="tabs";navIndex=Math.Max(0,tabNav.Count-2);}}
  else if(navZone=="tools"){if(dx!=0)navIndex=Math.Max(0,Math.Min(toolNav.Count-1,navIndex+dx));if(dy>0)navZone="search";}
  RefreshNavFocus();DrawHints();}
 int NearestTab(int col){if(columns<=1)return 0;double x=(double)col/(columns-1);if(x>.83)return tabNav.Count-1;if(x>.65)return tabNav.Count-2;return Math.Min(2,(int)Math.Round(x*4));}
 void RefreshNavFocus(){string style=navZone+"|"+navIndex+"|"+modal;if(focusStyleKey==style){Highlight(false);return;}focusStyleKey=style;foreach(var b in toolNav.Concat(tabNav)){b.BorderBrush=Brushes.Transparent;b.BorderThickness=new Thickness(1);b.Effect=null;PaintIcon(b);}navButton=null;if(searchBox!=null){searchBox.BorderThickness=new Thickness(1);searchBox.SetResourceReference(Border.BorderBrushProperty,"Line");searchBox.Effect=null;}
  if(modal==""){if(navZone=="tools"&&toolNav.Count>0)navButton=toolNav[Math.Max(0,Math.Min(navIndex,toolNav.Count-1))];if(navZone=="tabs"&&tabNav.Count>0)navButton=tabNav[Math.Max(0,Math.Min(navIndex,tabNav.Count-1))];if(navButton!=null){PaintIcon(navButton,true);selectedName.Text=navButton.ToolTip.ToString();}if(navZone=="search"&&searchBox!=null){searchBox.BorderBrush=B(Mint);searchBox.BorderThickness=new Thickness(2);selectedName.Text="搜索游戏 · 确认打开屏幕键盘";}}
  Highlight(false);if(focusCaption!=null)focusCaption.Text=modal!=""?"COMMAND PANEL":navZone=="games"?"LIBRARY / 游戏选择":navZone=="tabs"?"FILTER / 分类导航":navZone=="tools"?"SYSTEM / 工具导航":"SEARCH / 搜索";}
 void ActivateNavigation(){if(navZone=="tools"||navZone=="tabs"){if(navButton!=null)navButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));return;}if(navZone=="search"){OpenSearch();return;}if(visible.Count>selected)OpenGame(visible[selected]);}
 void PointerGame(Game g){controllerNavigation=false;if(navZone=="games"&&selected==visible.IndexOf(g))return;if(navZone!="games"||selected!=visible.IndexOf(g))PlayUi("move");controllerNavigation=false;navZone="games";selected=visible.IndexOf(g);RefreshNavFocus();}
 void Breathe(Game g){if(glowingGame==g)return;StopGlow();glowingGame=g;var brush=new LinearGradientBrush{StartPoint=new Point(0,0),EndPoint=new Point(1,1)};foreach(var stop in new[]{new GradientStop((Color)ColorConverter.ConvertFromString("#65FFE3"),0),new GradientStop((Color)ColorConverter.ConvertFromString("#67C7FF"),.28),new GradientStop((Color)ColorConverter.ConvertFromString("#BA83FF"),.55),new GradientStop((Color)ColorConverter.ConvertFromString("#FF93D8"),.78),new GradientStop((Color)ColorConverter.ConvertFromString("#65FFE3"),1)})brush.GradientStops.Add(stop);var spin=new RotateTransform(0,.5,.5);brush.RelativeTransform=spin;Animate(spin,RotateTransform.AngleProperty,new DoubleAnimation(0,360,TimeSpan.FromSeconds(5.5)){RepeatBehavior=RepeatBehavior.Forever});g.Card.BorderBrush=brush;Animate(brush,Brush.OpacityProperty,new DoubleAnimation(.25,1,TimeSpan.FromMilliseconds(110)));
  var glow=new DropShadowEffect{Color=(Color)ColorConverter.ConvertFromString("#7ADFFF"),BlurRadius=32,ShadowDepth=0,Opacity=.45};g.Halo.BorderBrush=B("#7ADFFF");g.Halo.Opacity=1;g.Halo.Effect=glow;g.Halo.CacheMode=new BitmapCache(1);g.Card.Effect=null;Animate(g.Halo,UIElement.OpacityProperty,new DoubleAnimation(.35,.95,TimeSpan.FromSeconds(1.15)){AutoReverse=true,RepeatBehavior=RepeatBehavior.Forever});g.Card.BorderThickness=new Thickness(4);}
 void StopGlow(){if(glowingGame==null)return;var c=glowingGame.Card;if(c!=null){var brush=c.BorderBrush as LinearGradientBrush;if(brush!=null){var transform=brush.RelativeTransform as RotateTransform;if(transform!=null)transform.BeginAnimation(RotateTransform.AngleProperty,null);}glowingGame.FocusMark.Opacity=0;if(glowingGame.SelectionBadge!=null)glowingGame.SelectionBadge.Visibility=Visibility.Collapsed;glowingGame.Lift.Y=0;glowingGame.Scale.ScaleX=glowingGame.Scale.ScaleY=1;var halo=glowingGame.Halo;var glow=halo!=null?halo.Effect as DropShadowEffect:null;if(glow!=null){glow.BeginAnimation(DropShadowEffect.OpacityProperty,null);glow.BeginAnimation(DropShadowEffect.BlurRadiusProperty,null);}c.BeginAnimation(Border.BorderThicknessProperty,null);c.BorderThickness=new Thickness(4);c.BorderBrush=Brushes.Transparent;c.Effect=null;if(halo!=null){halo.BeginAnimation(UIElement.OpacityProperty,null);halo.Effect=null;halo.Opacity=0;halo.BorderBrush=Brushes.Transparent;}}glowingGame=null;}
 void OpenSearch(){ExpandSearch(true,false);NewModal("search","搜索游戏","方向键选择字符 · 确认输入 · B / ○ 返回",660);virtualInput=new TextBox{Text=search.Text,Padding=new Thickness(14),FontSize=16,Margin=new Thickness(0,0,0,18)};virtualInput.SetResourceReference(Control.ForegroundProperty,"Fg");virtualInput.SetResourceReference(Control.BackgroundProperty,"Track");virtualInput.SetResourceReference(TextBox.CaretBrushProperty,"Fg");virtualInput.TextChanged+=(o,e)=>search.Text=virtualInput.Text;modalList.Children.Add(virtualInput);var keys=new UniformGrid{Columns=10};string[] chars="ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".Select(c=>c.ToString()).Concat(new[]{"空格","退格","清空","完成"}).ToArray();foreach(string ch in chars){string value=ch;var b=new Button{Content=Label(value,value.Length>1?11:15,"Fg"),Height=44,Margin=new Thickness(3),HorizontalContentAlignment=HorizontalAlignment.Center,ToolTip=value};int index=modalButtons.Count;b.Click+=(o,e)=>{PlayUi("move");if(value=="完成"){CloseModal();navZone="games";main.Focus();RefreshNavFocus();return;}if(value=="退格"){if(virtualInput.Text.Length>0)virtualInput.Text=virtualInput.Text.Substring(0,virtualInput.Text.Length-1);}else if(value=="清空")virtualInput.Clear();else virtualInput.Text+=value=="空格"?" ":value;};b.MouseEnter+=(o,e)=>{if(!controllerNavigation){modalSelected=index;MarkModal();}};modalButtons.Add(b);keys.Children.Add(b);}modalList.Children.Add(keys);MarkModal();main.Focus();}
}













