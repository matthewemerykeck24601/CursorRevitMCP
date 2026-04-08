// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TemplateToolsBase.TitleBlockGeometryAnalyzer
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.TicketTools.TicketPopulator;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace EDGE.TicketTools.TemplateToolsBase;

internal class TitleBlockGeometryAnalyzer
{
  public static Outline Analyze(
    ViewSheet sheet,
    out XYZ titleBlockLocation,
    out string titleBlockName,
    out string message)
  {
    titleBlockLocation = new XYZ(0.0, 0.0, 0.0);
    message = "All Ok";
    titleBlockName = "";
    Document document = sheet.Document;
    List<ElementId> list = new FilteredElementCollector(document, sheet.Id).OfCategory(BuiltInCategory.OST_TitleBlocks).ToElementIds().ToList<ElementId>();
    if (list.Count > 1)
    {
      message = "Multiple titleblocks detected on this sheet.  Template creation can only work with one titleblock.";
      return TitleBlockGeometryAnalyzer.GetEmptyOutline();
    }
    if (list.Count == 0)
    {
      message = "No titleblocks detected.  Will anchor to zero point (0,0)";
      return TitleBlockGeometryAnalyzer.GetEmptyOutline();
    }
    if (!list.Any<ElementId>())
    {
      message = "No titleblocks detected.  Will anchor to zero point (0,0)";
      return TitleBlockGeometryAnalyzer.GetEmptyOutline();
    }
    if (!(document.GetElement(list.First<ElementId>()) is FamilyInstance element))
    {
      message = "Unable to get titleBlock FamilyInstance: " + list.First<ElementId>().ToString();
      return TitleBlockGeometryAnalyzer.GetEmptyOutline();
    }
    titleBlockName = element.Name;
    double sheetHeight = element.get_Parameter(BuiltInParameter.SHEET_HEIGHT).AsDouble();
    double sheetWidth = element.get_Parameter(BuiltInParameter.SHEET_WIDTH).AsDouble();
    IEnumerable<Line> source1 = element.GetOriginalGeometry(new Options()
    {
      DetailLevel = ViewDetailLevel.Coarse
    }).Where<GeometryObject>((Func<GeometryObject, bool>) (s => s is Line)).Cast<Line>();
    XYZ titleLoc = (element.Location as LocationPoint).Point;
    titleBlockLocation = titleLoc;
    double num1 = source1.Select<Line, double>((Func<Line, double>) (s => Math.Max(s.GetEndPoint(0).X, s.GetEndPoint(1).X))).Max() + titleLoc.X;
    double num2 = source1.Select<Line, double>((Func<Line, double>) (s => Math.Min(s.GetEndPoint(0).X, s.GetEndPoint(1).X))).Min() + titleLoc.X;
    double num3 = source1.Select<Line, double>((Func<Line, double>) (s => Math.Max(s.GetEndPoint(0).Y, s.GetEndPoint(1).Y))).Max() + titleLoc.Y;
    double num4 = source1.Select<Line, double>((Func<Line, double>) (s => Math.Min(s.GetEndPoint(0).Y, s.GetEndPoint(1).Y))).Min() + titleLoc.Y;
    double midX = (num1 + num2) / 2.0;
    double midY = (num3 + num4) / 2.0;
    Line leftLine = (Line) null;
    Line rightLine = (Line) null;
    Line topLine = (Line) null;
    Line bottomLine = (Line) null;
    IEnumerable<Line> source2 = source1.Where<Line>((Func<Line, bool>) (s => TitleBlockGeometryAnalyzer.isVertical(s) && s.GetEndPoint(0).X + titleLoc.X < midX && s.Length > 0.7 * sheetHeight)).OrderByDescending<Line, double>((Func<Line, double>) (o => o.GetEndPoint(0).X)).Select<Line, Line>((Func<Line, Line>) (s => Line.CreateBound(s.GetEndPoint(0) + titleLoc, s.GetEndPoint(1) + titleLoc)));
    if (source2.Any<Line>())
      leftLine = source2.First<Line>();
    IEnumerable<Line> source3 = source1.Where<Line>((Func<Line, bool>) (s => TitleBlockGeometryAnalyzer.isVertical(s) && s.GetEndPoint(0).X + titleLoc.X > midX && s.Length > 0.7 * sheetHeight)).OrderByDescending<Line, double>((Func<Line, double>) (o => o.GetEndPoint(0).X)).Select<Line, Line>((Func<Line, Line>) (s => Line.CreateBound(s.GetEndPoint(0) + titleLoc, s.GetEndPoint(1) + titleLoc)));
    if (source3.Any<Line>())
      rightLine = source3.Last<Line>();
    IEnumerable<Line> source4 = source1.Where<Line>((Func<Line, bool>) (s => TitleBlockGeometryAnalyzer.isHorizontal(s) && s.GetEndPoint(0).Y + titleLoc.Y > midY && s.Length > 0.7 * sheetWidth)).OrderByDescending<Line, double>((Func<Line, double>) (o => o.GetEndPoint(0).Y)).Select<Line, Line>((Func<Line, Line>) (s => Line.CreateBound(s.GetEndPoint(0) + titleLoc, s.GetEndPoint(1) + titleLoc)));
    if (source4.Any<Line>())
      topLine = source4.Last<Line>();
    IEnumerable<Line> source5 = source1.Where<Line>((Func<Line, bool>) (s => TitleBlockGeometryAnalyzer.isHorizontal(s) && s.GetEndPoint(0).Y + titleLoc.Y < midY && s.Length > 0.7 * sheetWidth)).OrderByDescending<Line, double>((Func<Line, double>) (o => o.GetEndPoint(0).Y)).Select<Line, Line>((Func<Line, Line>) (s => Line.CreateBound(s.GetEndPoint(0) + titleLoc, s.GetEndPoint(1) + titleLoc)));
    if (source5.Any<Line>())
      bottomLine = source5.First<Line>();
    XYZ xyz1 = (GeometryObject) leftLine == (GeometryObject) null || (GeometryObject) bottomLine == (GeometryObject) null ? new XYZ(0.0, 0.0, 0.0) : TitleBlockGeometryAnalyzer.GetLowerLeft(leftLine, bottomLine);
    XYZ xyz2 = (GeometryObject) topLine == (GeometryObject) null || (GeometryObject) rightLine == (GeometryObject) null ? new XYZ(0.0, 0.0, 0.0) : TitleBlockGeometryAnalyzer.GetUpperRight(topLine, rightLine);
    if (PopulatorQA.inHouse)
    {
      Transaction transaction = new Transaction(document, "debug TitleBlock Analyzer");
      if (transaction.Start() == TransactionStatus.Started)
      {
        document.Create.NewDetailCurve((View) sheet, (Curve) Line.CreateBound(xyz1, new XYZ(xyz1.X, xyz2.Y, xyz2.Z)));
        document.Create.NewDetailCurve((View) sheet, (Curve) Line.CreateBound(new XYZ(xyz1.X, xyz2.Y, xyz2.Z), xyz2));
        document.Create.NewDetailCurve((View) sheet, (Curve) Line.CreateBound(xyz2, new XYZ(xyz2.X, xyz1.Y, xyz1.Z)));
        document.Create.NewDetailCurve((View) sheet, (Curve) Line.CreateBound(new XYZ(xyz2.X, xyz1.Y, xyz1.Z), xyz1));
        int num5 = (int) transaction.Commit();
      }
    }
    return new Outline(xyz1, xyz2);
  }

  private static XYZ GetUpperRight(Line topLine, Line rightLine)
  {
    if ((GeometryObject) topLine == (GeometryObject) null || (GeometryObject) rightLine == (GeometryObject) null)
      throw new ArgumentNullException("Null line in GetUpperRight: TitleBlockGeometryAnalyzer.cs");
    List<XYZ> source = new List<XYZ>();
    source.Add(topLine.GetEndPoint(0).X > topLine.GetEndPoint(1).X ? topLine.GetEndPoint(0) : topLine.GetEndPoint(1));
    source.Add(rightLine.GetEndPoint(0).Y > rightLine.GetEndPoint(1).Y ? rightLine.GetEndPoint(0) : rightLine.GetEndPoint(1));
    return new XYZ(source.Select<XYZ, double>((Func<XYZ, double>) (s => s.X)).Min<double>(), source.Select<XYZ, double>((Func<XYZ, double>) (o => o.Y)).Min<double>(), topLine.GetEndPoint(0).Z);
  }

  private static XYZ GetLowerLeft(Line leftLine, Line bottomLine)
  {
    if ((GeometryObject) leftLine == (GeometryObject) null || (GeometryObject) bottomLine == (GeometryObject) null)
      throw new ArgumentNullException("Null line in GetUpperRight: TitleBlockGeometryAnalyzer.cs");
    List<XYZ> source = new List<XYZ>();
    source.Add(bottomLine.GetEndPoint(0).X < bottomLine.GetEndPoint(1).X ? bottomLine.GetEndPoint(0) : bottomLine.GetEndPoint(1));
    source.Add(leftLine.GetEndPoint(0).Y < leftLine.GetEndPoint(1).Y ? leftLine.GetEndPoint(0) : leftLine.GetEndPoint(1));
    return new XYZ(source.Select<XYZ, double>((Func<XYZ, double>) (s => s.X)).Max<double>(), source.Select<XYZ, double>((Func<XYZ, double>) (o => o.Y)).Max<double>(), leftLine.GetEndPoint(0).Z);
  }

  private static bool isVertical(Line line)
  {
    return new XYZ(line.GetEndPoint(0).X, 0.0, 0.0).DistanceTo(new XYZ(line.GetEndPoint(1).X, 0.0, 0.0)) < 0.01;
  }

  private static bool isHorizontal(Line line)
  {
    return new XYZ(0.0, line.GetEndPoint(0).Y, 0.0).DistanceTo(new XYZ(0.0, line.GetEndPoint(1).Y, 0.0)) < 0.01;
  }

  private static Outline GetEmptyOutline()
  {
    return new Outline(new XYZ(0.0, 0.0, 0.0), new XYZ(0.0, 0.0, 0.0));
  }
}
