// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketTemplateTools.Debugging.TemplateDebugUtils
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.TicketTools.TemplateToolsBase;

#nullable disable
namespace EDGE.TicketTools.TicketTemplateTools.Debugging;

public class TemplateDebugUtils
{
  public static void DebugDrawOutline(
    Outline outline,
    Document doc,
    View view,
    string message = "",
    bool needsTransaction = true)
  {
    Transaction transaction = (Transaction) null;
    if (needsTransaction)
    {
      transaction = new Transaction(doc, "debug drawCircleAtPoint");
      if (transaction.Start() != TransactionStatus.Started)
        return;
    }
    doc.Create.NewDetailCurve(view, (Curve) Line.CreateBound(new XYZ(outline.MinimumPoint.X, outline.MinimumPoint.Y, 0.0), new XYZ(outline.MinimumPoint.X, outline.MaximumPoint.Y, 0.0)));
    doc.Create.NewDetailCurve(view, (Curve) Line.CreateBound(new XYZ(outline.MinimumPoint.X, outline.MaximumPoint.Y, 0.0), new XYZ(outline.MaximumPoint.X, outline.MaximumPoint.Y, 0.0)));
    doc.Create.NewDetailCurve(view, (Curve) Line.CreateBound(new XYZ(outline.MaximumPoint.X, outline.MaximumPoint.Y, 0.0), new XYZ(outline.MaximumPoint.X, outline.MinimumPoint.Y, 0.0)));
    doc.Create.NewDetailCurve(view, (Curve) Line.CreateBound(new XYZ(outline.MaximumPoint.X, outline.MinimumPoint.Y, 0.0), new XYZ(outline.MinimumPoint.X, outline.MinimumPoint.Y, 0.0)));
    if (!needsTransaction || transaction == null)
      return;
    int num = (int) transaction.Commit();
  }

  public static void DebugDrawVector(
    XYZ start,
    XYZ end,
    Document doc,
    View view,
    string lable = "",
    bool needsTransaction = true)
  {
    Transaction transaction = (Transaction) null;
    if (needsTransaction)
    {
      transaction = new Transaction(doc, "debug drawCircleAtPoint");
      if (transaction.Start() != TransactionStatus.Started)
        return;
    }
    Line bound = Line.CreateBound(start, end);
    doc.Create.NewDetailCurve(view, (Curve) bound);
    XYZ point = bound.Evaluate(0.95, true);
    Transform rotationAtPoint1 = Transform.CreateRotationAtPoint(new XYZ(0.0, 0.0, 1.0), 0.1, bound.GetEndPoint(1));
    Transform rotationAtPoint2 = Transform.CreateRotationAtPoint(new XYZ(0.0, 0.0, 1.0), -0.1, bound.GetEndPoint(1));
    doc.Create.NewDetailCurve(view, (Curve) Line.CreateBound(rotationAtPoint1.OfPoint(point), rotationAtPoint2.OfPoint(point)));
    TextNote.Create(doc, view.Id, bound.GetEndPoint(1) + new XYZ(0.03, 0.03, 0.0), 0.5, lable, new TextNoteOptions()
    {
      HorizontalAlignment = HorizontalTextAlignment.Right,
      TypeId = doc.GetDefaultElementTypeId(ElementTypeGroup.TextNoteType)
    });
    if (!needsTransaction || transaction == null)
      return;
    int num = (int) transaction.Commit();
  }

  public static void DebugDrawOutline(
    BoundingBoxXYZ bbox,
    Document doc,
    View view,
    string message = "",
    bool needsTransaction = true)
  {
    TemplateDebugUtils.DebugDrawOutline(new Outline(bbox.Min, bbox.Max), doc, view);
  }

  public static void DebugDrawCircleAtPoint(
    XYZ point,
    Document doc,
    View view,
    string message = "",
    bool needsTransaction = true)
  {
    Transaction transaction = (Transaction) null;
    if (needsTransaction)
    {
      transaction = new Transaction(doc, "debug drawCircleAtPoint");
      if (transaction.Start() != TransactionStatus.Started)
        return;
    }
    Arc geometryCurve = Arc.Create(Plane.CreateByNormalAndOrigin(new XYZ(0.0, 0.0, 1.0), new XYZ(point.X, point.Y, 0.0)), 0.01, 0.0, 6.1831853071795866);
    doc.Create.NewDetailCurve(view, (Curve) geometryCurve);
    TextNote.Create(doc, view.Id, point + new XYZ(0.03, 0.03, 0.0), 0.5, message, new TextNoteOptions()
    {
      HorizontalAlignment = HorizontalTextAlignment.Right,
      TypeId = doc.GetDefaultElementTypeId(ElementTypeGroup.TextNoteType)
    });
    if (!needsTransaction || transaction == null)
      return;
    int num = (int) transaction.Commit();
  }

  public static void DebugDrawCircleAtPoint(
    SimpleVector point,
    Document doc,
    View view,
    string message = "",
    bool needsTransaction = true)
  {
    TemplateDebugUtils.DebugDrawCircleAtPoint(point.GetXYZ(), doc, view, message);
  }
}
