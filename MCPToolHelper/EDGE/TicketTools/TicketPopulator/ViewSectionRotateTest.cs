// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketPopulator.ViewSectionRotateTest
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

#nullable disable
namespace EDGE.TicketTools.TicketPopulator;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public class ViewSectionRotateTest : IExternalCommand
{
  Result IExternalCommand.Execute(
    ExternalCommandData commandData,
    ref string message,
    ElementSet elements)
  {
    FilteredElementCollector elementCollector = new FilteredElementCollector(commandData.Application.ActiveUIDocument.Document);
    elementCollector.OfCategory(BuiltInCategory.OST_Viewers);
    IEnumerable<Element> source1 = elementCollector.ToElements().Where<Element>((Func<Element, bool>) (s => s.get_Parameter(BuiltInParameter.ID_PARAM).AsElementId() == commandData.Application.ActiveUIDocument.ActiveView.Id));
    if (source1.Any<Element>())
    {
      Element element1 = source1.First<Element>();
      AssemblyInstance element2 = commandData.Application.ActiveUIDocument.Document.GetElement(commandData.Application.ActiveUIDocument.ActiveView.AssociatedAssemblyInstanceId) as AssemblyInstance;
      Transform transform = element2.GetTransform();
      StringBuilder stringBuilder1 = new StringBuilder();
      stringBuilder1.AppendLine("AssInstance Transform");
      stringBuilder1.AppendLine(QA.WriteTransform(element2.GetTransform()));
      XYZ source2 = XYZ.Zero;
      XYZ source3 = XYZ.Zero;
      XYZ source4 = XYZ.Zero;
      XYZ source5 = XYZ.Zero;
      XYZ source6 = XYZ.Zero;
      XYZ source7 = XYZ.Zero;
      XYZ source8 = XYZ.Zero;
      XYZ source9 = XYZ.Zero;
      XYZ source10 = XYZ.Zero;
      XYZ source11 = XYZ.Zero;
      XYZ source12 = XYZ.Zero;
      XYZ source13 = XYZ.Zero;
      if (element2.GetTransform().BasisY.IsAlmostEqualTo(new XYZ(0.0, 0.0, 1.0)))
      {
        source6 = transform.BasisX.Negate();
        source7 = transform.BasisX;
        source2 = transform.BasisZ;
        source3 = transform.BasisZ.Negate();
        source4 = transform.BasisX;
        source5 = transform.BasisX;
        source12 = transform.BasisZ.Negate();
        source13 = transform.BasisZ;
        source8 = transform.BasisX.Negate();
        source9 = transform.BasisX;
        source10 = transform.BasisY.Negate();
        source11 = transform.BasisY;
      }
      else if (element2.GetTransform().BasisZ.IsAlmostEqualTo(new XYZ(0.0, 0.0, 1.0)))
      {
        source2 = transform.BasisY.Negate();
        source3 = transform.BasisY;
        source4 = transform.BasisX;
        source5 = transform.BasisX.Negate();
        source6 = transform.BasisX;
        source7 = transform.BasisX;
        source8 = transform.BasisX.Negate();
        source9 = transform.BasisX;
        source10 = transform.BasisY.Negate();
        source11 = transform.BasisY;
        source12 = transform.BasisZ;
        source13 = transform.BasisZ.Negate();
      }
      else
      {
        int num1 = (int) MessageBox.Show("Assembly orientation issue.  Expected Green Arrow to be vertical or horizontal");
      }
      stringBuilder1.AppendLine("TRANSFORM TRANSPOSED:");
      stringBuilder1.AppendLine("LeftBasisX: " + source2.ToString());
      stringBuilder1.AppendLine("rightBasisX: " + source3.ToString());
      stringBuilder1.AppendLine("frontBasisX: " + source4.ToString());
      stringBuilder1.AppendLine("backBasisX: " + source5.ToString());
      stringBuilder1.AppendLine("topBasisX: " + source6.ToString());
      stringBuilder1.AppendLine("bottomBasisX: " + source7.ToString());
      stringBuilder1.AppendLine("");
      stringBuilder1.AppendLine("LeftBasisZ: " + source8.ToString());
      stringBuilder1.AppendLine("rightBasisZ: " + source9.ToString());
      stringBuilder1.AppendLine("frontBasisZ: " + source10.ToString());
      stringBuilder1.AppendLine("backBasisZ: " + source11.ToString());
      stringBuilder1.AppendLine("topBasisZ: " + source12.ToString());
      stringBuilder1.AppendLine("bottomBasisZ: " + source13.ToString());
      stringBuilder1.AppendLine("");
      Transaction transaction = new Transaction(commandData.Application.ActiveUIDocument.Document, "test");
      int num2 = (int) transaction.Start();
      BoundingBoxXYZ cropBox1 = commandData.Application.ActiveUIDocument.ActiveView.CropBox;
      stringBuilder1.AppendLine("VIEW: " + commandData.Application.ActiveUIDocument.ActiveView.Name);
      stringBuilder1.AppendLine(QA.WriteBoundingBox(cropBox1));
      if (cropBox1.Transform.BasisZ.IsAlmostEqualTo(source12))
      {
        stringBuilder1.AppendLine("This is a TopView by basisZ");
        XYZ xyz = source6.CrossProduct(cropBox1.Transform.BasisX);
        double num3;
        if (xyz.IsZeroLength())
        {
          num3 = 0.0;
        }
        else
        {
          double num4 = xyz.DotProduct(source12);
          num3 = num4 / Math.Abs(num4);
        }
        stringBuilder1.AppendLine(" Angle CropBasisX to standard TopBasisX : " + (num3 * source6.AngleTo(cropBox1.Transform.BasisX) * 360.0 / (2.0 * Math.PI)).ToString());
      }
      if (cropBox1.Transform.BasisZ.IsAlmostEqualTo(source13))
      {
        stringBuilder1.AppendLine("This is a BottomView by basisZ");
        XYZ xyz = source7.CrossProduct(cropBox1.Transform.BasisX);
        double num5;
        if (xyz.IsZeroLength())
        {
          num5 = 0.0;
        }
        else
        {
          double num6 = xyz.DotProduct(source13);
          num5 = num6 / Math.Abs(num6);
        }
        stringBuilder1.AppendLine(" Angle CropBasisX to standard BottomBasisX : " + (num5 * source7.AngleTo(cropBox1.Transform.BasisX) * 360.0 / (2.0 * Math.PI)).ToString());
      }
      if (cropBox1.Transform.BasisZ.IsAlmostEqualTo(source8))
      {
        stringBuilder1.AppendLine("This is a LeftView by basisZ");
        XYZ xyz = source2.CrossProduct(cropBox1.Transform.BasisX);
        double num7;
        if (xyz.IsZeroLength())
        {
          num7 = 0.0;
        }
        else
        {
          double num8 = xyz.DotProduct(source8);
          num7 = num8 / Math.Abs(num8);
        }
        stringBuilder1.AppendLine(" Angle CropBasisX to standard LeftBasisX : " + (num7 * source2.AngleTo(cropBox1.Transform.BasisX) * 360.0 / (2.0 * Math.PI)).ToString());
      }
      if (cropBox1.Transform.BasisZ.IsAlmostEqualTo(source9))
      {
        stringBuilder1.AppendLine("This is a RightView by basisZ");
        XYZ xyz = source3.CrossProduct(cropBox1.Transform.BasisX);
        double num9;
        if (xyz.IsZeroLength())
        {
          num9 = 0.0;
        }
        else
        {
          double num10 = xyz.DotProduct(source9);
          num9 = num10 / Math.Abs(num10);
        }
        stringBuilder1.AppendLine(" Angle CropBasisX to standard RightBasisX : " + (num9 * source3.AngleTo(cropBox1.Transform.BasisX) * 360.0 / (2.0 * Math.PI)).ToString());
      }
      if (cropBox1.Transform.BasisZ.IsAlmostEqualTo(source10))
      {
        stringBuilder1.AppendLine("This is a FrontView by basisZ");
        XYZ xyz = source4.CrossProduct(cropBox1.Transform.BasisX);
        double num11;
        if (xyz.IsZeroLength())
        {
          num11 = 0.0;
        }
        else
        {
          double num12 = xyz.DotProduct(source10);
          num11 = num12 / Math.Abs(num12);
        }
        stringBuilder1.AppendLine(" Angle CropBasisX to standard FrontBasisX : " + (num11 * source4.AngleTo(cropBox1.Transform.BasisX) * 360.0 / (2.0 * Math.PI)).ToString());
      }
      if (cropBox1.Transform.BasisZ.IsAlmostEqualTo(source11))
      {
        stringBuilder1.AppendLine("This is a BackView by basisZ");
        XYZ xyz = source5.CrossProduct(cropBox1.Transform.BasisX);
        double num13;
        if (xyz.IsZeroLength())
        {
          num13 = 0.0;
        }
        else
        {
          double num14 = xyz.DotProduct(source11);
          num13 = num14 / Math.Abs(num14);
        }
        stringBuilder1.AppendLine(" Angle CropBasisX to standard BackBasisX : " + (num13 * source5.AngleTo(cropBox1.Transform.BasisX) * 360.0 / (2.0 * Math.PI)).ToString());
      }
      XYZ xyz1 = new XYZ((cropBox1.Max.X - cropBox1.Min.X) / 2.0, (cropBox1.Max.Y - cropBox1.Min.Y) / 2.0, 0.0);
      XYZ center;
      XYZ endpoint2 = (center = element2.GetCenter()) + commandData.Application.ActiveUIDocument.ActiveView.ViewDirection;
      Line bound = Line.CreateBound(center, endpoint2);
      ElementTransformUtils.RotateElement(commandData.Application.ActiveUIDocument.Document, element1.Id, bound, Math.PI / 2.0);
      int num15 = (int) transaction.Commit();
      BoundingBoxXYZ cropBox2 = commandData.Application.ActiveUIDocument.ActiveView.CropBox;
      stringBuilder1.AppendLine(QA.WriteBoundingBox(cropBox2));
      double num16;
      if (cropBox2.Transform.BasisZ.IsAlmostEqualTo(source8))
      {
        stringBuilder1.AppendLine("This is a LeftView AFTER by basisZ");
        double num17 = cropBox2.Transform.BasisX.DotProduct(new XYZ(0.0, 0.0, 1.0)) > 0.0 ? 1.0 : -1.0;
        StringBuilder stringBuilder2 = stringBuilder1;
        num16 = num17 * cropBox2.Transform.BasisX.AngleTo(source2) * 360.0 / (2.0 * Math.PI);
        string str = " Angle CropBasisX to standard LeftBasisX : " + num16.ToString();
        stringBuilder2.AppendLine(str);
      }
      if (cropBox2.Transform.BasisZ.IsAlmostEqualTo(source9))
      {
        stringBuilder1.AppendLine("This is a RightView AFTER by basisZ");
        double num18 = cropBox2.Transform.BasisX.DotProduct(new XYZ(0.0, 0.0, 1.0)) > 0.0 ? 1.0 : -1.0;
        StringBuilder stringBuilder3 = stringBuilder1;
        num16 = num18 * cropBox2.Transform.BasisX.AngleTo(source3) * 360.0 / (2.0 * Math.PI);
        string str = " Angle CropBasisX to standard RightBasisX : " + num16.ToString();
        stringBuilder3.AppendLine(str);
      }
      if (cropBox2.Transform.BasisZ.IsAlmostEqualTo(source10))
      {
        stringBuilder1.AppendLine("This is a FrontView AFTER by basisZ");
        double num19 = cropBox2.Transform.BasisX.DotProduct(new XYZ(0.0, 0.0, 1.0)) > 0.0 ? 1.0 : -1.0;
        StringBuilder stringBuilder4 = stringBuilder1;
        num16 = num19 * cropBox2.Transform.BasisX.AngleTo(source4) * 360.0 / (2.0 * Math.PI);
        string str = " Angle CropBasisX to standard FrontBasisX : " + num16.ToString();
        stringBuilder4.AppendLine(str);
      }
      if (cropBox2.Transform.BasisZ.IsAlmostEqualTo(source11))
      {
        stringBuilder1.AppendLine("This is a BackView AFTER by basisZ");
        double num20 = cropBox2.Transform.BasisX.DotProduct(new XYZ(0.0, 0.0, 1.0)) > 0.0 ? 1.0 : -1.0;
        StringBuilder stringBuilder5 = stringBuilder1;
        num16 = num20 * cropBox2.Transform.BasisX.AngleTo(source5) * 360.0 / (2.0 * Math.PI);
        string str = " Angle CropBasisX to standard BackBasisX : " + num16.ToString();
        stringBuilder5.AppendLine(str);
      }
      if (cropBox2.Transform.BasisZ.IsAlmostEqualTo(source12))
      {
        double num21 = cropBox2.Transform.BasisX.DotProduct(element2.GetTransform().BasisY) > 0.0 ? 1.0 : -1.0;
        stringBuilder1.AppendLine("This is a TopView by basisZ");
        StringBuilder stringBuilder6 = stringBuilder1;
        num16 = num21 * cropBox2.Transform.BasisX.AngleTo(source6) * 360.0 / (2.0 * Math.PI);
        string str = " Angle CropBasisX to standard TopBasisX : " + num16.ToString();
        stringBuilder6.AppendLine(str);
      }
      if (cropBox2.Transform.BasisZ.IsAlmostEqualTo(source13))
      {
        double num22 = cropBox2.Transform.BasisX.DotProduct(element2.GetTransform().BasisY.Negate()) > 0.0 ? 1.0 : -1.0;
        stringBuilder1.AppendLine("This is a BottomView by basisZ");
        StringBuilder stringBuilder7 = stringBuilder1;
        num16 = num22 * cropBox2.Transform.BasisX.AngleTo(source7) * 360.0 / (2.0 * Math.PI);
        string str = " Angle CropBasisX to standard BottomBasisX : " + num16.ToString();
        stringBuilder7.AppendLine(str);
      }
      QA.DisplayTextInfo(stringBuilder1.ToString());
      return (Result) 0;
    }
    QA.InHouseMessage("didn't find a matching section mark for SectionId" + commandData.Application.ActiveUIDocument.ActiveView.Id.ToString());
    return (Result) 0;
  }
}
