using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
partial class Shelf {
 Grid techBackdrop;
 void InstallTechBackdrop(){
  techBackdrop=new Grid{IsHitTestVisible=false,ClipToBounds=true};canvas.Children.Insert(0,techBackdrop);
  var lines=new GeometryGroup();lines.Children.Add(new LineGeometry(new Point(0,0),new Point(56,0)));lines.Children.Add(new LineGeometry(new Point(0,0),new Point(0,56)));var brush=new DrawingBrush(new GeometryDrawing(null,new Pen(B("#1A779BD0"),.6),lines)){TileMode=TileMode.Tile,Viewport=new Rect(0,0,56,56),ViewportUnits=BrushMappingMode.Absolute,Stretch=Stretch.None};techBackdrop.Children.Add(new Border{Background=brush,Opacity=.5});
  var cyan=new Ellipse{Width=680,Height=520,HorizontalAlignment=HorizontalAlignment.Right,VerticalAlignment=VerticalAlignment.Top,Margin=new Thickness(0,-270,-240,0),Fill=new RadialGradientBrush((Color)ColorConverter.ConvertFromString("#2432D9F4"),Colors.Transparent)};techBackdrop.Children.Add(cyan);
  techBackdrop.Children.Add(new Ellipse{Width=820,Height=650,HorizontalAlignment=HorizontalAlignment.Left,VerticalAlignment=VerticalAlignment.Bottom,Margin=new Thickness(-390,0,0,-350),Fill=new RadialGradientBrush((Color)ColorConverter.ConvertFromString("#284E3AAC"),Colors.Transparent)});
  var circuit=new System.Windows.Shapes.Path{Data=Geometry.Parse("M0,28 H160 L180,8 H300 M0,36 H110 L126,52 H245 M320,8 H520 L538,28 H650"),Stroke=B("#46577FA6"),StrokeThickness=1,Width=650,Height=54,HorizontalAlignment=HorizontalAlignment.Left,VerticalAlignment=VerticalAlignment.Top,Margin=new Thickness(230,32,0,0),Opacity=.6};techBackdrop.Children.Add(circuit);
  var header=main.Children[0] as Grid;if(header!=null){var rule=new Border{Height=1,VerticalAlignment=VerticalAlignment.Bottom,Margin=new Thickness(0,0,0,-13),Background=new LinearGradientBrush((Color)ColorConverter.ConvertFromString("#6671E3CB"),(Color)ColorConverter.ConvertFromString("#009581FF"),0),IsHitTestVisible=false};header.Children.Add(rule);Grid.SetColumnSpan(rule,2);}
  var intro=main.Children[1] as Grid;if(intro!=null){var heading=intro.Children[0] as StackPanel;if(heading!=null){var chips=new StackPanel{Orientation=Orientation.Horizontal,Margin=new Thickness(0,12,0,0)};chips.Children.Add(StatusChip("LOCAL LIBRARY",Mint));chips.Children.Add(StatusChip("AUTO SYNC",Accent));heading.Children.Add(chips);}}
  var nav=main.Children[2] as Grid;if(nav!=null){focusCaption=Label("LIBRARY / 游戏选择",9,"Muted");focusCaption.HorizontalAlignment=HorizontalAlignment.Center;focusCaption.IsHitTestVisible=false;focusCaption.Margin=new Thickness(0,0,70,0);nav.Children.Add(focusCaption);}
  var foot=main.Children[4] as Grid;if(foot!=null){var line=new Border{Height=1,VerticalAlignment=VerticalAlignment.Top,Margin=new Thickness(0,-12,0,0),Background=new LinearGradientBrush((Color)ColorConverter.ConvertFromString("#0071E3CB"),(Color)ColorConverter.ConvertFromString("#666BA8E0"),0),IsHitTestVisible=false};foot.Children.Add(line);Grid.SetColumnSpan(line,2);}
 }
 Border StatusChip(string text,string color){var row=new StackPanel{Orientation=Orientation.Horizontal};row.Children.Add(new Ellipse{Width=4,Height=4,Fill=B(color),VerticalAlignment=VerticalAlignment.Center,Margin=new Thickness(0,0,6,0)});var t=Txt(text,8,B(color));t.FontWeight=FontWeights.SemiBold;row.Children.Add(t);return new Border{Child=row,CornerRadius=new CornerRadius(5),BorderBrush=B("#374C6389"),BorderThickness=new Thickness(1),Background=B("#102C4564"),Padding=new Thickness(8,5,8,5),Margin=new Thickness(0,0,8,0)};}
}
