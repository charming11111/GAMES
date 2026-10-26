using System;
using System.IO;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
partial class Shelf {
 static void WriteYuzuArtwork(string path){var visual=new DrawingVisual();using(var d=visual.RenderOpen()){
 d.DrawRectangle(new LinearGradientBrush(Color.FromRgb(24,31,56),Color.FromRgb(8,13,26),70),null,new Rect(0,0,600,900));
 var gridPen=new Pen(new SolidColorBrush(Color.FromArgb(18,125,162,199)),1);for(int x=0;x<600;x+=50)d.DrawLine(gridPen,new Point(x,0),new Point(x,900));for(int y=0;y<900;y+=50)d.DrawLine(gridPen,new Point(0,y),new Point(600,y));
 var red=new SolidColorBrush(Color.FromRgb(255,102,133));var blue=new SolidColorBrush(Color.FromRgb(84,219,239));
 d.DrawRoundedRectangle(new SolidColorBrush(Color.FromArgb(24,102,156,218)),new Pen(new SolidColorBrush(Color.FromArgb(50,102,156,218)),1),new Rect(90,237,420,355),46,46);
 var left=Geometry.Parse("M278,294 L229,294 C180,294 159,326 159,370 L159,447 C159,492 184,520 229,520 L278,520 Z");
 var right=Geometry.Parse("M322,318 L371,318 C416,318 441,346 441,391 L441,469 C441,514 415,544 371,544 L322,544 Z");
 d.DrawGeometry(null,new Pen(red,13){LineJoin=PenLineJoin.Round},left);d.DrawGeometry(null,new Pen(blue,13){LineJoin=PenLineJoin.Round},right);d.DrawEllipse(red,null,new Point(222,361),23,23);d.DrawEllipse(blue,null,new Point(378,470),23,23);
 DrawCoverText(d,"E M U L A T I O N",18,106,Color.FromRgb(136,161,195));DrawCoverText(d,"yuzu",94,633,Colors.White);DrawCoverText(d,"SWITCH EMULATOR",19,751,Color.FromRgb(148,170,200));
 d.DrawRoundedRectangle(new LinearGradientBrush(red.Color,blue.Color,0),null,new Rect(247,815,106,4),2,2);
 }var bitmap=new RenderTargetBitmap(600,900,96,96,PixelFormats.Pbgra32);bitmap.Render(visual);var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(bitmap));using(var f=File.Create(path))png.Save(f);}
 static void DrawCoverText(DrawingContext context,string text,double size,double top,Color color){var formatted=new FormattedText(text,CultureInfo.InvariantCulture,FlowDirection.LeftToRight,new Typeface("Segoe UI Light"),size,new SolidColorBrush(color),1);context.DrawText(formatted,new Point((600-formatted.Width)/2,top));}
 static bool BuiltinCover(Game g){if(!Norm(g.OriginalName).StartsWith("yuzu"))return false;using(var source=typeof(Shelf).Assembly.GetManifestResourceStream("yuzu-cover.png")){if(source==null)return false;using(var bytes=new MemoryStream()){source.CopyTo(bytes);WriteCover(g.Cover,bytes.ToArray());}}File.WriteAllText(g.Cover+".title","yuzu模拟器");File.Delete(g.Cover+".retry");return true;}
}
