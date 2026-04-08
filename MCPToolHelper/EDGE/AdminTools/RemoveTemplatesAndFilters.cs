// Decompiled with JetBrains decompiler
// Type: EDGE.AdminTools.RemoveTemplatesAndFilters
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.AdminTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class RemoveTemplatesAndFilters : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    using (Transaction transaction = new Transaction(document, "Remove View Filters and View Templates"))
    {
      try
      {
        if (new TaskDialog("Remove View Filters and View Templates")
        {
          CommonButtons = ((TaskDialogCommonButtons) 6),
          MainContent = "Are you sure you want to remove all View Filters and Templates in the Model?",
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
          DefaultButton = ((TaskDialogResult) 7)
        }.Show() == 6)
        {
          int num1 = (int) transaction.Start();
          List<View> list = new FilteredElementCollector(document).OfClass(typeof (View)).Cast<View>().Where<View>((Func<View, bool>) (view => view.IsTemplate)).ToList<View>().ToList<View>();
          ICollection<ElementId> elementIds = new FilteredElementCollector(document).OfClass(typeof (FilterElement)).ToElementIds();
          foreach (ElementId elementId in list.Select<View, ElementId>((Func<View, ElementId>) (template => template.Id)).Concat<ElementId>((IEnumerable<ElementId>) elementIds))
            document.Delete(elementId);
          int num2 = (int) transaction.Commit();
        }
        return (Result) 0;
      }
      catch (Exception ex)
      {
        message = ex.ToString();
        return (Result) -1;
      }
    }
  }
}
