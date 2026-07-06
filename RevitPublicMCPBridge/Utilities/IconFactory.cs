using System.Windows;
using System.Windows.Media;

namespace RevitPublicMCPBridge.Utilities;

public static class IconFactory
{
    public static ImageSource CreatePlayIcon() => CreateIcon(
        Colors.SeaGreen,
        Geometry.Parse("M 8,6 L 26,16 L 8,26 Z"));

    public static ImageSource CreateStopIcon() => CreateIcon(
        Colors.IndianRed,
        Geometry.Parse("M 8,8 L 24,8 L 24,24 L 8,24 Z"));

    public static ImageSource CreateStatusIcon() => CreateIcon(
        Colors.SteelBlue,
        Geometry.Parse("M 6,24 L 12,16 L 18,20 L 26,8 L 28,10 L 18,24 L 12,20 L 8,26 Z"));

    public static ImageSource CreateCopyIcon() => CreateIcon(
        Colors.DarkSlateGray,
        Geometry.Parse("M 10,8 L 22,8 L 22,22 L 10,22 Z M 6,12 L 18,12 L 18,26 L 6,26 Z"));

    public static ImageSource CreateSettingsIcon() => CreateIcon(
        Colors.DimGray,
        Geometry.Parse("M16,6 L18,6 L19,9 L22,10 L24,8 L26,10 L24,13 L25,16 L28,17 L28,19 L25,20 L24,23 L26,26 L24,28 L22,26 L19,27 L18,30 L16,30 L15,27 L12,26 L10,28 L8,26 L10,23 L9,20 L6,19 L6,17 L9,16 L10,13 L8,10 L10,8 L12,10 L15,9 Z M17,13 A4,4 0 1 1 17,21 A4,4 0 1 1 17,13 Z"));

    public static ImageSource CreateHelpIcon() => CreateIcon(
        Colors.Goldenrod,
        Geometry.Parse("M 10,11 A 6,6 0 1 1 22,11 C 22,15 18,15 17,18 L 17,20 M 16,24 L 18,24 L 18,26 L 16,26 Z"));

    private static ImageSource CreateIcon(System.Windows.Media.Color color, Geometry geometry)
    {
        var drawing = new GeometryDrawing
        {
            Brush = new SolidColorBrush(color),
            Pen = new Pen(Brushes.WhiteSmoke, 1),
            Geometry = geometry,
        };

        drawing.Freeze();
        var group = new DrawingGroup();
        group.Children.Add(drawing);
        group.Freeze();

        return new DrawingImage(group);
    }
}
