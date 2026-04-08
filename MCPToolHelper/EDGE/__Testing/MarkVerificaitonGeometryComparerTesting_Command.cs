// Decompiled with JetBrains decompiler
// Type: EDGE.__Testing.MarkVerificaitonGeometryComparerTesting_Command
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using EDGE.AssemblyTools.MarkVerification;
using EDGE.AssemblyTools.MarkVerification.GeometryComparer;
using EDGE.AssemblyTools.MarkVerification.Tools;
using System.Linq;
using Utils.AssemblyUtils;

#nullable disable
namespace EDGE.__Testing;

[Transaction(TransactionMode.Manual)]
public class MarkVerificaitonGeometryComparerTesting_Command : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    using (Transaction transaction = new Transaction(commandData.Application.ActiveUIDocument.Document, "testingMarkGeometry"))
    {
      GeometryVerificationFamily verificationFamily = (GeometryVerificationFamily) null;
      GeometryVerificationFamily other = (GeometryVerificationFamily) null;
      Document document = commandData.Application.ActiveUIDocument.Document;
      int num1 = (int) transaction.Start();
      MarkVerificationData data = new MarkVerificationData(document, ComparisonOption.DoNotRound);
      ElementId id = commandData.Application.ActiveUIDocument.Selection.GetElementIds().FirstOrDefault<ElementId>();
      if ((commandData.Application.ActiveUIDocument.Document.GetElement(id) as AssemblyInstance).GetStructuralFramingElement() is FamilyInstance structuralFramingElement1)
        verificationFamily = new GeometryVerificationFamily(structuralFramingElement1, data);
      Reference reference = commandData.Application.ActiveUIDocument.Selection.PickObject((ObjectType) 1, "Pick Assembly");
      if ((commandData.Application.ActiveUIDocument.Document.GetElement(reference.ElementId) as AssemblyInstance).GetStructuralFramingElement() is FamilyInstance structuralFramingElement2)
        other = new GeometryVerificationFamily(structuralFramingElement2, data);
      if (verificationFamily.Matches(other, out LocationCompareResult _, true, true, true, true))
        TaskDialog.Show("Success", "Elements are Equal");
      else
        TaskDialog.Show("Failure", "Elements are *NOT* Equal");
      int num2 = (int) transaction.Commit();
    }
    return (Result) 0;
  }
}
