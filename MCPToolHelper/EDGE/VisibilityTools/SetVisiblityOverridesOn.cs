// Decompiled with JetBrains decompiler
// Type: EDGE.VisibilityTools.SetVisiblityOverridesOn
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
internal class SetVisiblityOverridesOn : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    if (Utils.ViewUtils.ViewUtils.CurrentViewIsProjectBrowser(document))
      return (Result) 1;
    ICollection<string> strings = (ICollection<string>) new List<string>()
    {
      "CIP",
      "EMBED",
      "ERECTION",
      "FOUNDATION",
      "GROUT",
      "LIFTING",
      "MESH",
      "PRECAST PRODUCT",
      "REBAR",
      "VOID",
      "WWF"
    };
    using (Transaction transaction = new Transaction(document, "Set Visibility Overrides - ON"))
    {
      try
      {
        int num1 = (int) transaction.Start();
        ICollection<ElementId> filters = document.ActiveView.GetFilters();
        FilteredElementCollector filterCollector = new FilteredElementCollector(document).OfClass(typeof (ParameterFilterElement));
        foreach (string filterName in (IEnumerable<string>) strings)
          this.AddFilter(filterName, filters, filterCollector);
        document.Regenerate();
        foreach (ElementId filter in (IEnumerable<ElementId>) document.ActiveView.GetFilters())
        {
          ParameterFilterElement element = document.GetElement(filter) as ParameterFilterElement;
          string upper = element.Name.ToUpper();
          if (upper.Contains("CIP"))
            this.SetFilterOverrides(element, new Color((byte) 64 /*0x40*/, (byte) 128 /*0x80*/, (byte) 128 /*0x80*/));
          if (upper.Contains("EMBED"))
            this.SetFilterOverrides(element, new Color((byte) 0, (byte) 0, byte.MaxValue));
          if (upper.Contains("ERECTION"))
            this.SetFilterOverrides(element, new Color(byte.MaxValue, (byte) 0, byte.MaxValue));
          if (upper.Contains("FOUNDATION"))
            this.SetFilterOverrides(element, new Color((byte) 64 /*0x40*/, (byte) 128 /*0x80*/, (byte) 128 /*0x80*/));
          if (upper.Contains("GROUT"))
            this.SetFilterOverrides(element, new Color(byte.MaxValue, (byte) 128 /*0x80*/, (byte) 0));
          if (upper.Contains("LIFTING"))
            this.SetFilterOverrides(element, new Color(byte.MaxValue, byte.MaxValue, (byte) 0));
          if (upper.Contains("MESH"))
            this.SetFilterOverrides(element, new Color((byte) 0, byte.MaxValue, (byte) 0));
          if (upper.Contains("PRECAST PRODUCT"))
            this.SetFilterOverrides(element, new Color((byte) 215, (byte) 215, (byte) 215));
          if (upper.Contains("REBAR"))
            this.SetFilterOverrides(element, new Color((byte) 0, byte.MaxValue, (byte) 0));
          if (upper.Contains("VOID"))
            this.SetFilterOverrides(element, new Color((byte) 0, byte.MaxValue, byte.MaxValue));
          if (upper.Contains("WWF"))
            this.SetFilterOverrides(element, new Color((byte) 0, byte.MaxValue, (byte) 0));
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

  private void SetFilterOverrides(ParameterFilterElement filter, Color color)
  {
    Document document = ActiveModel.Document;
    OverrideGraphicSettings filterOverrides = document.ActiveView.GetFilterOverrides(filter.Id);
    filterOverrides.SetSurfaceForegroundPatternColor(color);
    filterOverrides.SetSurfaceForegroundPatternId(new ElementId(3));
    document.ActiveView.SetFilterOverrides(filter.Id, filterOverrides);
  }

  private void AddFilter(
    string filterName,
    ICollection<ElementId> existingFilters,
    FilteredElementCollector filterCollector)
  {
    Document document = ActiveModel.UIDoc.Document;
    foreach (ParameterFilterElement parameterFilterElement in filterCollector)
    {
      if (parameterFilterElement.Name.Contains(filterName) && !existingFilters.Contains(parameterFilterElement.Id))
      {
        document.ActiveView.AddFilter(parameterFilterElement.Id);
        break;
      }
    }
  }
}
