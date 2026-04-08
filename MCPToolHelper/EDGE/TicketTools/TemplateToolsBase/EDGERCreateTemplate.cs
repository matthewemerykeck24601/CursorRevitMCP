// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TemplateToolsBase.EDGERCreateTemplate
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.TicketTools.TicketTemplateTools.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.TicketTools.TemplateToolsBase;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public class EDGERCreateTemplate : IExternalCommand
{
  private List<TicketViewportInfo> viewportInfos;
  private List<AnchorGroup> anchorGroups;
  private TicketTemplate ticketTemplate;
  private string TemplateName = "";
  private int overallTemplateScale = -1;
  private BOMJustification BOMJustification;
  private TicketTemplateCreatorWindow templateCreatorWindow;
  public bool templateWindowOpen;
  private double viewportAlignmentTolerance = 0.1;
  public static bool windowExists;

  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    if (EDGERCreateTemplate.windowExists)
      return (Result) 1;
    Document document = commandData.Application.ActiveUIDocument.Document;
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    View activeView = commandData.Application.ActiveUIDocument.ActiveView;
    this.ticketTemplate = new TicketTemplate();
    this.anchorGroups = new List<AnchorGroup>();
    ActiveModel.GetInformation(activeUiDocument);
    if (activeUiDocument.Document.IsFamilyDocument)
    {
      new TaskDialog("Family Editor")
      {
        AllowCancellation = false,
        CommonButtons = ((TaskDialogCommonButtons) 1),
        MainInstruction = "Ticket Template Manager must be run in the Project Environment",
        MainContent = "You are currently in the family editor, Ticket Template Manager must be run in the project environment.  Please return to the project environment or open a project before running this tool.",
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
      }.Show();
      return (Result) 1;
    }
    if (activeView == null)
    {
      message = "Null current view!  Run from an assembly sheet view.";
      return (Result) -1;
    }
    assembly = (AssemblyInstance) null;
    if (activeView is ViewSheet && activeView.IsAssemblyView)
    {
      if (document.GetElement(activeView.AssociatedAssemblyInstanceId) is AssemblyInstance assembly)
        assembly.GetTransform();
      if (!(activeView is ViewSheet))
      {
        message = "Unexpected error: ActiveView cannot be cast to ViewSheet.";
        return (Result) -1;
      }
    }
    IntPtr mainWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
    this.templateCreatorWindow = new TicketTemplateCreatorWindow(assembly, activeView, document, activeUiDocument, commandData.Application, mainWindowHandle);
    this.templateCreatorWindow.Show();
    return (Result) 0;
  }

  public static double GetSectionViewCropRotation(
    ViewSection associatedView,
    Transform AssemblyTransform,
    bool bCompatibility = false)
  {
    if (associatedView == null)
      return 0.0;
    double viewCropRotation = 0.0;
    XYZ zero1 = XYZ.Zero;
    XYZ zero2 = XYZ.Zero;
    XYZ zero3 = XYZ.Zero;
    XYZ zero4 = XYZ.Zero;
    XYZ zero5 = XYZ.Zero;
    XYZ zero6 = XYZ.Zero;
    XYZ zero7 = XYZ.Zero;
    XYZ zero8 = XYZ.Zero;
    XYZ zero9 = XYZ.Zero;
    XYZ zero10 = XYZ.Zero;
    XYZ zero11 = XYZ.Zero;
    XYZ zero12 = XYZ.Zero;
    XYZ source1;
    XYZ source2;
    XYZ basisX1;
    XYZ source3;
    XYZ source4;
    XYZ basisX2;
    XYZ source5;
    XYZ basisX3;
    XYZ source6;
    XYZ basisY;
    XYZ source7;
    XYZ source8;
    if (!bCompatibility)
    {
      source1 = AssemblyTransform.BasisY.Negate();
      source2 = AssemblyTransform.BasisY;
      basisX1 = AssemblyTransform.BasisX;
      source3 = AssemblyTransform.BasisX.Negate();
      source4 = AssemblyTransform.BasisX;
      basisX2 = AssemblyTransform.BasisX;
      source5 = AssemblyTransform.BasisX.Negate();
      basisX3 = AssemblyTransform.BasisX;
      source6 = AssemblyTransform.BasisY.Negate();
      basisY = AssemblyTransform.BasisY;
      source7 = AssemblyTransform.BasisZ;
      source8 = AssemblyTransform.BasisZ.Negate();
    }
    else if (AssemblyTransform.BasisY.IsAlmostEqualTo(new XYZ(0.0, 0.0, 1.0)))
    {
      source1 = AssemblyTransform.BasisZ;
      source2 = AssemblyTransform.BasisZ.Negate();
      basisX1 = AssemblyTransform.BasisX;
      source3 = AssemblyTransform.BasisX;
      source4 = AssemblyTransform.BasisX.Negate();
      basisX2 = AssemblyTransform.BasisX;
      source5 = AssemblyTransform.BasisX.Negate();
      basisX3 = AssemblyTransform.BasisX;
      source6 = AssemblyTransform.BasisY.Negate();
      basisY = AssemblyTransform.BasisY;
      source7 = AssemblyTransform.BasisZ.Negate();
      source8 = AssemblyTransform.BasisZ;
    }
    else if (AssemblyTransform.BasisZ.IsAlmostEqualTo(new XYZ(0.0, 0.0, 1.0)))
    {
      source1 = AssemblyTransform.BasisY.Negate();
      source2 = AssemblyTransform.BasisY;
      basisX1 = AssemblyTransform.BasisX;
      source3 = AssemblyTransform.BasisX.Negate();
      source4 = AssemblyTransform.BasisX;
      basisX2 = AssemblyTransform.BasisX;
      source5 = AssemblyTransform.BasisX.Negate();
      basisX3 = AssemblyTransform.BasisX;
      source6 = AssemblyTransform.BasisY.Negate();
      basisY = AssemblyTransform.BasisY;
      source7 = AssemblyTransform.BasisZ;
      source8 = AssemblyTransform.BasisZ.Negate();
    }
    else
    {
      if (App.DialogSwitches.AssemblyOriginOrientationWarning)
      {
        TaskDialog taskDialog = new TaskDialog("Edge Warning");
        taskDialog.AllowCancellation = false;
        taskDialog.MainInstruction = "Assembly Origin Orientation is not blue up or green up.  Assembly view rotations on sheet may not be interpreted properly.";
        taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "OK");
        taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Don't show this dialog again for this session");
        if (taskDialog.Show() == 1002)
          App.DialogSwitches.AssemblyOriginOrientationWarning = false;
      }
      return 0.0;
    }
    BoundingBoxXYZ cropBox = associatedView.CropBox;
    StringBuilder stringBuilder1 = new StringBuilder();
    if (AssemblyTransform.BasisZ.IsAlmostEqualTo(new XYZ(0.0, 0.0, 1.0)))
    {
      double num;
      if (cropBox.Transform.BasisZ.IsAlmostEqualTo(source5))
      {
        viewCropRotation = (cropBox.Transform.BasisX.DotProduct(new XYZ(0.0, 0.0, 1.0)) > 0.0 ? 1.0 : -1.0) * cropBox.Transform.BasisX.AngleTo(source1);
        stringBuilder1.AppendLine("This is a LeftView by basisZ");
        StringBuilder stringBuilder2 = stringBuilder1;
        num = viewCropRotation * 360.0 / (2.0 * Math.PI);
        string str = " Angle CropBasisX to standard LeftBasisX : " + num.ToString();
        stringBuilder2.AppendLine(str);
      }
      if (cropBox.Transform.BasisZ.IsAlmostEqualTo(basisX3))
      {
        viewCropRotation = (cropBox.Transform.BasisX.DotProduct(new XYZ(0.0, 0.0, 1.0)) > 0.0 ? 1.0 : -1.0) * cropBox.Transform.BasisX.AngleTo(source2);
        stringBuilder1.AppendLine("This is a RightView by basisZ");
        StringBuilder stringBuilder3 = stringBuilder1;
        num = viewCropRotation * 360.0 / (2.0 * Math.PI);
        string str = " Angle CropBasisX to standard RightBasisX : " + num.ToString();
        stringBuilder3.AppendLine(str);
      }
      if (cropBox.Transform.BasisZ.IsAlmostEqualTo(source6))
      {
        viewCropRotation = (cropBox.Transform.BasisX.DotProduct(new XYZ(0.0, 0.0, 1.0)) > 0.0 ? 1.0 : -1.0) * cropBox.Transform.BasisX.AngleTo(basisX1);
        stringBuilder1.AppendLine("This is a FrontView by basisZ");
        StringBuilder stringBuilder4 = stringBuilder1;
        num = viewCropRotation * 360.0 / (2.0 * Math.PI);
        string str = " Angle CropBasisX to standard FrontBasisX : " + num.ToString();
        stringBuilder4.AppendLine(str);
      }
      if (cropBox.Transform.BasisZ.IsAlmostEqualTo(basisY))
      {
        viewCropRotation = (cropBox.Transform.BasisX.DotProduct(new XYZ(0.0, 0.0, 1.0)) > 0.0 ? 1.0 : -1.0) * cropBox.Transform.BasisX.AngleTo(source3);
        stringBuilder1.AppendLine("This is a BackView by basisZ");
        StringBuilder stringBuilder5 = stringBuilder1;
        num = viewCropRotation * 360.0 / (2.0 * Math.PI);
        string str = " Angle CropBasisX to standard BackBasisX : " + num.ToString();
        stringBuilder5.AppendLine(str);
      }
      if (cropBox.Transform.BasisZ.IsAlmostEqualTo(source7))
      {
        viewCropRotation = (cropBox.Transform.BasisX.DotProduct(AssemblyTransform.BasisY) > 0.0 ? 1.0 : -1.0) * cropBox.Transform.BasisX.AngleTo(source4);
        stringBuilder1.AppendLine("This is a TopView by basisZ");
        StringBuilder stringBuilder6 = stringBuilder1;
        num = viewCropRotation * 360.0 / (2.0 * Math.PI);
        string str = " Angle CropBasisX to standard TopBasisX : " + num.ToString();
        stringBuilder6.AppendLine(str);
      }
      if (cropBox.Transform.BasisZ.IsAlmostEqualTo(source8))
      {
        viewCropRotation = (cropBox.Transform.BasisX.DotProduct(AssemblyTransform.BasisY.Negate()) > 0.0 ? 1.0 : -1.0) * cropBox.Transform.BasisX.AngleTo(basisX2);
        stringBuilder1.AppendLine("This is a BottomView by basisZ");
        StringBuilder stringBuilder7 = stringBuilder1;
        num = viewCropRotation * 360.0 / (2.0 * Math.PI);
        string str = " Angle CropBasisX to standard BottomBasisX : " + num.ToString();
        stringBuilder7.AppendLine(str);
      }
    }
    else
    {
      if (cropBox.Transform.BasisZ.IsAlmostEqualTo(source7))
      {
        stringBuilder1.AppendLine("This is a TopView by basisZ");
        XYZ xyz = source4.CrossProduct(cropBox.Transform.BasisX);
        double num1;
        if (xyz.IsZeroLength())
        {
          num1 = !source4.IsAlmostEqualTo(cropBox.Transform.BasisX.Negate()) ? 0.0 : 1.0;
        }
        else
        {
          double num2 = xyz.DotProduct(source7);
          num1 = num2 / Math.Abs(num2);
        }
        stringBuilder1.AppendLine(" Angle CropBasisX to standard TopBasisX : " + (num1 * source4.AngleTo(cropBox.Transform.BasisX) * 360.0 / (2.0 * Math.PI)).ToString());
        viewCropRotation = num1 * source4.AngleTo(cropBox.Transform.BasisX);
      }
      if (cropBox.Transform.BasisZ.IsAlmostEqualTo(source8))
      {
        stringBuilder1.AppendLine("This is a BottomView by basisZ");
        XYZ xyz = basisX2.CrossProduct(cropBox.Transform.BasisX);
        double num3;
        if (xyz.IsZeroLength())
        {
          num3 = !basisX2.IsAlmostEqualTo(cropBox.Transform.BasisX.Negate()) ? 0.0 : 1.0;
        }
        else
        {
          double num4 = xyz.DotProduct(source8);
          num3 = num4 / Math.Abs(num4);
        }
        stringBuilder1.AppendLine(" Angle CropBasisX to standard BottomBasisX : " + (num3 * basisX2.AngleTo(cropBox.Transform.BasisX) * 360.0 / (2.0 * Math.PI)).ToString());
        viewCropRotation = num3 * basisX2.AngleTo(cropBox.Transform.BasisX);
      }
      if (cropBox.Transform.BasisZ.IsAlmostEqualTo(source5))
      {
        stringBuilder1.AppendLine("This is a LeftView by basisZ");
        XYZ xyz = source1.CrossProduct(cropBox.Transform.BasisX);
        double num5;
        if (xyz.IsZeroLength())
        {
          num5 = !source1.IsAlmostEqualTo(cropBox.Transform.BasisX.Negate()) ? 0.0 : 1.0;
        }
        else
        {
          double num6 = xyz.DotProduct(source5);
          num5 = num6 / Math.Abs(num6);
        }
        stringBuilder1.AppendLine(" Angle CropBasisX to standard LeftBasisX : " + (num5 * source1.AngleTo(cropBox.Transform.BasisX) * 360.0 / (2.0 * Math.PI)).ToString());
        viewCropRotation = num5 * source1.AngleTo(cropBox.Transform.BasisX);
      }
      if (cropBox.Transform.BasisZ.IsAlmostEqualTo(basisX3))
      {
        stringBuilder1.AppendLine("This is a RightView by basisZ");
        XYZ xyz = source2.CrossProduct(cropBox.Transform.BasisX);
        double num7;
        if (xyz.IsZeroLength())
        {
          num7 = !source2.IsAlmostEqualTo(cropBox.Transform.BasisX.Negate()) ? 0.0 : 1.0;
        }
        else
        {
          double num8 = xyz.DotProduct(basisX3);
          num7 = num8 / Math.Abs(num8);
        }
        stringBuilder1.AppendLine(" Angle CropBasisX to standard RightBasisX : " + (num7 * source2.AngleTo(cropBox.Transform.BasisX) * 360.0 / (2.0 * Math.PI)).ToString());
        viewCropRotation = num7 * source2.AngleTo(cropBox.Transform.BasisX);
      }
      if (cropBox.Transform.BasisZ.IsAlmostEqualTo(source6))
      {
        stringBuilder1.AppendLine("This is a FrontView by basisZ");
        XYZ xyz = basisX1.CrossProduct(cropBox.Transform.BasisX);
        double num9;
        if (xyz.IsZeroLength())
        {
          num9 = !basisX1.IsAlmostEqualTo(cropBox.Transform.BasisX.Negate()) ? 0.0 : 1.0;
        }
        else
        {
          double num10 = xyz.DotProduct(source6);
          num9 = num10 / Math.Abs(num10);
        }
        stringBuilder1.AppendLine(" Angle CropBasisX to standard FrontBasisX : " + (num9 * basisX1.AngleTo(cropBox.Transform.BasisX) * 360.0 / (2.0 * Math.PI)).ToString());
        viewCropRotation = num9 * basisX1.AngleTo(cropBox.Transform.BasisX);
      }
      if (cropBox.Transform.BasisZ.IsAlmostEqualTo(basisY))
      {
        stringBuilder1.AppendLine("This is a BackView by basisZ");
        XYZ xyz = source3.CrossProduct(cropBox.Transform.BasisX);
        double num11;
        if (xyz.IsZeroLength())
        {
          num11 = !source3.IsAlmostEqualTo(cropBox.Transform.BasisX.Negate()) ? 0.0 : 1.0;
        }
        else
        {
          double num12 = xyz.DotProduct(basisY);
          num11 = num12 / Math.Abs(num12);
        }
        stringBuilder1.AppendLine(" Angle CropBasisX to standard BackBasisX : " + (num11 * source3.AngleTo(cropBox.Transform.BasisX) * 360.0 / (2.0 * Math.PI)).ToString());
        viewCropRotation = num11 * source3.AngleTo(cropBox.Transform.BasisX);
      }
    }
    return viewCropRotation;
  }

  public bool WithinViewAlignmentTolerance(XYZ point1, XYZ point2, double tolerance)
  {
    return point1.DistanceTo(point2) < tolerance;
  }

  public static XYZ GetOutlineUpperLeftCorner(Outline outline)
  {
    return outline.IsEmpty ? new XYZ(0.0, 0.0, 0.0) : new XYZ(outline.MinimumPoint.X < outline.MaximumPoint.X ? outline.MinimumPoint.X : outline.MaximumPoint.X, outline.MinimumPoint.Y > outline.MaximumPoint.Y ? outline.MinimumPoint.Y : outline.MaximumPoint.Y, outline.MinimumPoint.Z);
  }
}
