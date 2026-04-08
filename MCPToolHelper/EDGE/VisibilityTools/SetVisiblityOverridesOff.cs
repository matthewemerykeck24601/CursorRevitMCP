// Decompiled with JetBrains decompiler
// Type: EDGE.VisibilityTools.SetVisiblityOverridesOff
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
internal class SetVisiblityOverridesOff : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    if (Utils.ViewUtils.ViewUtils.CurrentViewIsProjectBrowser(document))
      return (Result) 1;
    using (Transaction transaction = new Transaction(document, "Set Visibility Overrides - OFF"))
    {
      try
      {
        int num1 = (int) transaction.Start();
        foreach (ElementId filter in (IEnumerable<ElementId>) document.ActiveView.GetFilters())
        {
          ParameterFilterElement element = document.GetElement(filter) as ParameterFilterElement;
          OverrideGraphicSettings overrideGraphicSettings = new OverrideGraphicSettings();
          overrideGraphicSettings.SetSurfaceTransparency(document.ActiveView.GetFilterOverrides(filter).Transparency);
          document.ActiveView.SetFilterOverrides(element.Id, overrideGraphicSettings);
        }
        int num2 = (int) transaction.Commit();
        return (Result) 0;
      }
      catch (Exception ex)
      {
        if (transaction.HasStarted())
        {
          int num = (int) transaction.RollBack();
        }
        message = ex.ToString();
        return (Result) -1;
      }
    }
  }
}
