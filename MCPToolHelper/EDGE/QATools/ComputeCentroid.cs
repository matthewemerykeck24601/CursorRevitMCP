// Decompiled with JetBrains decompiler
// Type: EDGE.QATools.ComputeCentroid
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Utils.GeometryUtils;

#nullable disable
namespace EDGE.QATools;

[Regeneration(RegenerationOption.Manual)]
[Transaction(TransactionMode.Manual)]
internal class ComputeCentroid : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    Document document = commandData.Application.ActiveUIDocument.Document;
    Transaction transaction = new Transaction(document, "place centroid");
    try
    {
      int num1 = (int) transaction.Start();
      IEnumerable<ElementId> elementIds = (IEnumerable<ElementId>) commandData.Application.ActiveUIDocument.Selection.GetElementIds();
      if (elementIds.Any<ElementId>())
      {
        if (elementIds.Count<ElementId>() > 1)
        {
          TaskDialog.Show("EDGE: Selection Error", "More than one element selected");
          return (Result) 1;
        }
        Element element1 = document.GetElement(elementIds.First<ElementId>());
        List<Solid> instanceSolids = Solids.GetInstanceSolids(element1);
        ElementMulticategoryFilter filter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
        {
          BuiltInCategory.OST_GenericModel
        });
        FilteredElementCollector elementCollector = new FilteredElementCollector(document).WherePasses((ElementFilter) filter);
        FamilySymbol symbol = (FamilySymbol) null;
        foreach (Element element2 in elementCollector)
        {
          if (element2 is FamilySymbol && element2.Name.Equals("centroid"))
          {
            symbol = element2 as FamilySymbol;
            break;
          }
        }
        if (document.ActiveView is View3D)
          document.Create.NewFamilyInstance(instanceSolids[0].ComputeCentroid(), symbol, element1, StructuralType.NonStructural);
      }
      int num2 = (int) transaction.Commit();
      return (Result) 0;
    }
    catch (Exception ex)
    {
      int num = (int) MessageBox.Show(ex.ToString());
      return (Result) -1;
    }
  }
}
