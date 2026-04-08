// Decompiled with JetBrains decompiler
// Type: EDGE.VisibilityTools.ProductVisibility_25Percent
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.VisibilityTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class ProductVisibility_25Percent : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    if (Utils.ViewUtils.ViewUtils.CurrentViewIsProjectBrowser(document))
      return (Result) 1;
    bool flag = false;
    using (Transaction transaction = new Transaction(document, "PRECAST PRODUCT - 25% Transparent"))
    {
      try
      {
        int num1 = (int) transaction.Start();
        foreach (ElementId filter in (IEnumerable<ElementId>) document.ActiveView.GetFilters())
        {
          if ((document.GetElement(filter) as ParameterFilterElement).Name.ToUpper().Contains("PRECAST PRODUCT"))
          {
            flag = true;
            break;
          }
        }
        if (!flag)
          ProductVisibility_25Percent.AddFilter();
        document.Regenerate();
        foreach (ElementId filter in (IEnumerable<ElementId>) document.ActiveView.GetFilters())
        {
          ParameterFilterElement element = document.GetElement(filter) as ParameterFilterElement;
          if (element.Name.ToUpper().Contains("PRECAST PRODUCT"))
          {
            OverrideGraphicSettings filterOverrides = document.ActiveView.GetFilterOverrides(element.Id);
            filterOverrides.SetSurfaceTransparency(25);
            document.ActiveView.SetFilterOverrides(element.Id, filterOverrides);
            int num2 = (int) transaction.Commit();
            return (Result) 0;
          }
        }
        return (Result) 0;
      }
      catch (Exception ex)
      {
        message = ex.ToString();
        int num = (int) transaction.RollBack();
        return (Result) -1;
      }
    }
  }

  private static void AddFilter()
  {
    Document document = ActiveModel.UIDoc.Document;
    foreach (ParameterFilterElement parameterFilterElement in new FilteredElementCollector(document).OfClass(typeof (ParameterFilterElement)))
    {
      if (parameterFilterElement.Name.Contains("PRECAST PRODUCT"))
      {
        document.ActiveView.AddFilter(parameterFilterElement.Id);
        break;
      }
    }
  }
}
