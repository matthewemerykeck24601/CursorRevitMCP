// Decompiled with JetBrains decompiler
// Type: EDGE.VisibilityTools.EmbedVisibilityOn
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
internal class EmbedVisibilityOn : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    if (Utils.ViewUtils.ViewUtils.CurrentViewIsProjectBrowser(document))
      return (Result) 1;
    bool flag = false;
    using (Transaction transaction = new Transaction(document, "Turn ON Embeds"))
    {
      try
      {
        int num1 = (int) transaction.Start();
        foreach (ElementId filter in (IEnumerable<ElementId>) document.ActiveView.GetFilters())
        {
          if ((document.GetElement(filter) as ParameterFilterElement).Name.Contains("EMBED"))
          {
            flag = true;
            break;
          }
        }
        if (!flag)
          EmbedVisibilityOn.AddFilter(document);
        document.Regenerate();
        foreach (ElementId filter in (IEnumerable<ElementId>) document.ActiveView.GetFilters())
        {
          if ((document.GetElement(filter) as ParameterFilterElement).Name.ToUpper().Contains("EMBED"))
          {
            document.ActiveView.SetFilterVisibility(filter, true);
            int num2 = (int) transaction.Commit();
            return (Result) 0;
          }
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

  private static void AddFilter(Document doc)
  {
    foreach (ParameterFilterElement parameterFilterElement in new FilteredElementCollector(doc).OfClass(typeof (ParameterFilterElement)))
    {
      if (parameterFilterElement.Name.Contains("EMBED"))
      {
        doc.ActiveView.AddFilter(parameterFilterElement.Id);
        return;
      }
    }
    new TaskDialog("Warning")
    {
      FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
      MainInstruction = "Warning:  There is no filter for \"EMBED\" available to turn on. Make sure that the filter is both created and enabled in your Visibility/Graphics settings and try again."
    }.Show();
  }
}
