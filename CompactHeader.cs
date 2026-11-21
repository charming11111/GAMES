using System;
using System.Windows;
using System.Windows.Controls;
partial class Shelf {
 const double LeftRailWidth=42;
 bool searchExpanded=false;
 void ExpandSearch(bool expanded,bool focus=true){
  searchExpanded=expanded;
  double size=toolNav[0].Height;
  searchBox.Height=size;
  searchBox.Width=expanded?Math.Max(180,Math.Min(260,ActualWidth*.23)):size;
  var searchGlyph=(FrameworkElement)((Grid)searchBox.Child).Children[1];
  searchGlyph.HorizontalAlignment=expanded?HorizontalAlignment.Left:HorizontalAlignment.Center;
  searchGlyph.Margin=expanded?new Thickness(14,0,0,0):new Thickness();
  search.Visibility=expanded?Visibility.Visible:Visibility.Collapsed;
  searchHint.Visibility=expanded&&search.Text.Length==0?Visibility.Visible:Visibility.Collapsed;
  searchBox.ToolTip=expanded?null:"搜索游戏 · Ctrl+F";
  if(expanded&&focus)search.Focus();
 }
 void InstallCompactSearch(){var input=searchBox.Child as Grid;if(input!=null)input.Width=double.NaN;search.Padding=new Thickness(39,0,10,0);search.VerticalContentAlignment=VerticalAlignment.Center;searchBox.PreviewMouseLeftButtonDown+=(o,e)=>{if(!searchExpanded){ExpandSearch(true);e.Handled=true;}};search.LostKeyboardFocus+=(o,e)=>Dispatcher.BeginInvoke(new Action(()=>{if(search.Text.Length==0&&!search.IsKeyboardFocused&&modal!="search")ExpandSearch(false,false);}));ExpandSearch(false,false);}
 void InstallHeaderDrag(Grid header,FrameworkElement logoArea){
  canvas.PreviewMouseLeftButtonDown+=(o,e)=>{
   if(fullscreen||launching||modal!="")return;
   double y=e.GetPosition(header).Y;
   if((y>=header.ActualHeight&&!logoArea.IsMouseOver)||e.GetPosition(this).Y<6)return;
   var node=e.OriginalSource as DependencyObject;
   while(node!=null&&node!=canvas){
    if(node==searchBox||node is Button)return;
    var visual=node as System.Windows.Media.Visual;
    if(visual==null)return;
    node=System.Windows.Media.VisualTreeHelper.GetParent(visual);
   }
   e.Handled=true;
   if(e.ClickCount==2)WindowState=WindowState==WindowState.Maximized?WindowState.Normal:WindowState.Maximized;
   else if(WindowState==WindowState.Normal)DragMove();
  };
 }

 void CompactHeader(){
  var header=(Grid)main.Children[0];
  var intro=(Grid)main.Children[1];
  var nav=(Grid)main.Children[2];
  var footer=(Grid)main.Children[4];
  var tools=(StackPanel)toolNav[0].Parent;
  var extra=(StackPanel)nav.Children[1];
  var info=(StackPanel)selectedName.Parent;

  header.Children.RemoveAt(0);header.Children.Remove(tools);
  intro.Children.Remove(searchBox);
  var oldHeading=(StackPanel)section.Parent;
  oldHeading.Children.Remove(section);oldHeading.Children.Remove(countText);
  nav.Children.Remove(filterButtons);nav.Children.Remove(extra);
  footer.Children.Remove(info);footer.Children.Remove(hintBar);
  intro.Visibility=nav.Visibility=footer.Visibility=Visibility.Collapsed;

  main.Margin=new Thickness(10,6,10,2);
  main.RowDefinitions[0].Height=new GridLength(6);
  main.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(LeftRailWidth)});
  main.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(1,GridUnitType.Star)});
  header.Margin=new Thickness();Grid.SetColumnSpan(header,2);
  Grid.SetColumn(scroll,1);scroll.Padding=new Thickness(3,0,3,0);

  var left=new Grid();
  left.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto});
  left.RowDefinitions.Add(new RowDefinition());
  var logoGroup=new StackPanel{Background=System.Windows.Media.Brushes.Transparent};
  var railLogo=Logo(38);
  railLogo.Margin=new Thickness(0,2,0,0);
  logoGroup.Children.Add(railLogo);
  var logoDivider=new Border{Width=28,Height=1,Margin=new Thickness(0,6,0,7)};
  logoDivider.SetResourceReference(Border.BackgroundProperty,"Line");
  logoGroup.Children.Add(logoDivider);
  left.Children.Add(logoGroup);
  var actions=new StackPanel();
  foreach(var group in new[]{tools,extra,filterButtons}){
   group.Orientation=Orientation.Vertical;
   group.HorizontalAlignment=HorizontalAlignment.Center;
   var buttons=System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Cast<Button>(group.Children));
   group.Children.Clear();
   for(int i=buttons.Length-1;i>=0;i--){
    var button=buttons[i];
    button.Width=button.Height=34;
    button.Margin=new Thickness(0,3,0,3);
    var glyph=(FrameworkElement)button.Content;
    glyph.Width=glyph.Height=18;
    group.Children.Add(button);
   }
   actions.Children.Add(group);
   if(group==tools){
    var divider=new Border{Width=28,Height=1,Margin=new Thickness(0,7,0,7)};
    divider.SetResourceReference(Border.BackgroundProperty,"Line");
    actions.Children.Add(divider);
   }
  }
  hintBar.Width=LeftRailWidth;
  hintBar.Margin=new Thickness(0,6,0,4);
  var railContents=new DockPanel();
  var shortcuts=new StackPanel();
  var searchSlot=new Border{Height=40};
  shortcuts.Children.Add(searchSlot);
  shortcuts.Children.Add(hintBar);
  DockPanel.SetDock(shortcuts,Dock.Bottom);
  railContents.Children.Add(shortcuts);
  actions.VerticalAlignment=VerticalAlignment.Top;
  railContents.Children.Add(actions);
  var railViewport=new Grid{ClipToBounds=true};
  var railScale=new System.Windows.Media.ScaleTransform(1,1);
  railContents.Width=LeftRailWidth;
  railContents.HorizontalAlignment=HorizontalAlignment.Center;
  railContents.VerticalAlignment=VerticalAlignment.Top;
  railContents.LayoutTransform=railScale;
  railViewport.Children.Add(railContents);
  Grid.SetRow(railViewport,1);
  left.Children.Add(railViewport);
  Grid.SetRow(left,3);
  main.Children.Add(left);

  searchBox.HorizontalAlignment=HorizontalAlignment.Left;
  searchBox.VerticalAlignment=VerticalAlignment.Top;
  searchBox.Margin=new Thickness();
  var searchPosition=new System.Windows.Media.TranslateTransform();
  var searchScale=new System.Windows.Media.ScaleTransform(1,1);
  var searchTransform=new System.Windows.Media.TransformGroup();
  searchTransform.Children.Add(searchScale);
  searchTransform.Children.Add(searchPosition);
  searchBox.RenderTransform=searchTransform;
  Grid.SetColumn(searchBox,0);
  Grid.SetRow(searchBox,3);
  Grid.SetColumnSpan(searchBox,2);
  Panel.SetZIndex(searchBox,1);
  main.Children.Add(searchBox);
  Action fitRail=()=>{
   if(railViewport.ActualHeight<=0)return;
   var constraint=new Size(LeftRailWidth,double.PositiveInfinity);
   actions.Measure(constraint);
   shortcuts.Measure(constraint);
   double needed=actions.DesiredSize.Height+shortcuts.DesiredSize.Height;
   double scale=Math.Min(1,railViewport.ActualHeight/Math.Max(1,needed+2));
   railScale.ScaleX=railScale.ScaleY=scale;
   railContents.Height=railViewport.ActualHeight/scale;
   searchScale.ScaleX=searchScale.ScaleY=scale;
  };
  railViewport.SizeChanged+=(o,e)=>fitRail();
  hintBar.SizeChanged+=(o,e)=>fitRail();
  searchSlot.LayoutUpdated+=(o,e)=>{
   var anchor=searchSlot.TranslatePoint(new Point((LeftRailWidth-34)/2,0),main);
   searchPosition.X=anchor.X;
   searchPosition.Y=anchor.Y-left.TranslatePoint(new Point(),main).Y;
  };
  InstallCompactSearch();InstallHeaderDrag(header,logoGroup);
 }
}
