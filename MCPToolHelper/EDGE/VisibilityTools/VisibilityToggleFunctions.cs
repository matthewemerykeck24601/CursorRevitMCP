// Decompiled with JetBrains decompiler
// Type: EDGE.VisibilityTools.VisibilityToggleFunctions
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using EDGE.ApplicationRibbon;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.VisibilityTools;

internal static class VisibilityToggleFunctions
{
  public static Result ToggleCommandPrimer(
    ExternalCommandData commandData,
    string categoryName,
    ref string message)
  {
    bool visFlag = VisibilityToggles.visTogglesDict[categoryName].visFlag;
    string filterTag = VisibilityToggles.visTogglesDict[categoryName].filterTag;
    try
    {
      switch (categoryName)
      {
        case "Overrides":
          if (visFlag)
          {
            VisibilityToggleFunctions.SetVisibilityOverridesOff(commandData, ref message);
            break;
          }
          VisibilityToggleFunctions.SetVisibilityOverridesOn(commandData, ref message);
          break;
        case "Flat":
          if (visFlag)
          {
            VisibilityToggleFunctions.SetVisibilityFlatOff(commandData, ref message);
            break;
          }
          VisibilityToggleFunctions.SetVisibilityFlatOn(commandData, ref message);
          break;
        case "Warped":
          if (visFlag)
          {
            VisibilityToggleFunctions.SetVisibilityWarpOff(commandData, ref message);
            break;
          }
          VisibilityToggleFunctions.SetVisibilityWarpOn(commandData, ref message);
          break;
        default:
          VisibilityToggleFunctions.ToggleCategoryVisibility(commandData, filterTag, !visFlag, ref message);
          break;
      }
      VisibilityToggles.visTogglesDict[categoryName].visFlag = !visFlag;
      AppRibbonSetup.ToggleVisibilityButtonStatus(commandData, categoryName);
    }
    catch (Exception ex)
    {
      message = ex.Message;
      return (Result) -1;
    }
    return (Result) 0;
  }

  public static Result ToggleCategoryVisibility(
    ExternalCommandData commandData,
    string type,
    bool showOrHide,
    ref string message)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    if (Utils.ViewUtils.ViewUtils.CurrentViewIsProjectBrowser(document))
      return (Result) 1;
    bool flag = false;
    using (Transaction transaction = new Transaction(document, "Turn OFF " + type))
    {
      try
      {
        int num1 = (int) transaction.Start();
        foreach (ElementId filter in (IEnumerable<ElementId>) document.ActiveView.GetFilters())
        {
          if ((document.GetElement(filter) as ParameterFilterElement).Name.Contains(type))
          {
            flag = true;
            break;
          }
        }
        if (!flag)
          VisibilityToggleFunctions.AddFilter(document, type);
        document.Regenerate();
        foreach (ElementId filter in (IEnumerable<ElementId>) document.ActiveView.GetFilters())
        {
          if ((document.GetElement(filter) as ParameterFilterElement).Name.ToUpper().Contains(type))
          {
            document.ActiveView.SetFilterVisibility(filter, showOrHide);
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

  public static Result SetVisibilityOverridesOn(ExternalCommandData commandData, ref string message)
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
          VisibilityToggleFunctions.AddFilter(filterName, filters, filterCollector);
        document.Regenerate();
        foreach (ElementId filter in (IEnumerable<ElementId>) document.ActiveView.GetFilters())
        {
          ParameterFilterElement element = document.GetElement(filter) as ParameterFilterElement;
          string upper = element.Name.ToUpper();
          if (upper.Contains("CIP"))
            VisibilityToggleFunctions.SetFilterOverrides(element, new Autodesk.Revit.DB.Color((byte) 64 /*0x40*/, (byte) 128 /*0x80*/, (byte) 128 /*0x80*/));
          if (upper.Contains("EMBED"))
            VisibilityToggleFunctions.SetFilterOverrides(element, new Autodesk.Revit.DB.Color((byte) 0, (byte) 0, byte.MaxValue));
          if (upper.Contains("ERECTION"))
            VisibilityToggleFunctions.SetFilterOverrides(element, new Autodesk.Revit.DB.Color(byte.MaxValue, (byte) 0, byte.MaxValue));
          if (upper.Contains("FOUNDATION"))
            VisibilityToggleFunctions.SetFilterOverrides(element, new Autodesk.Revit.DB.Color((byte) 64 /*0x40*/, (byte) 128 /*0x80*/, (byte) 128 /*0x80*/));
          if (upper.Contains("GROUT"))
            VisibilityToggleFunctions.SetFilterOverrides(element, new Autodesk.Revit.DB.Color(byte.MaxValue, (byte) 128 /*0x80*/, (byte) 0));
          if (upper.Contains("LIFTING"))
            VisibilityToggleFunctions.SetFilterOverrides(element, new Autodesk.Revit.DB.Color(byte.MaxValue, byte.MaxValue, (byte) 0));
          if (upper.Contains("MESH"))
            VisibilityToggleFunctions.SetFilterOverrides(element, new Autodesk.Revit.DB.Color((byte) 0, byte.MaxValue, (byte) 0));
          if (upper.Contains("PRECAST PRODUCT"))
            VisibilityToggleFunctions.SetFilterOverrides(element, new Autodesk.Revit.DB.Color((byte) 215, (byte) 215, (byte) 215));
          if (upper.Contains("REBAR"))
            VisibilityToggleFunctions.SetFilterOverrides(element, new Autodesk.Revit.DB.Color((byte) 0, byte.MaxValue, (byte) 0));
          if (upper.Contains("VOID"))
            VisibilityToggleFunctions.SetFilterOverrides(element, new Autodesk.Revit.DB.Color((byte) 0, byte.MaxValue, byte.MaxValue));
          if (upper.Contains("WWF"))
            VisibilityToggleFunctions.SetFilterOverrides(element, new Autodesk.Revit.DB.Color((byte) 0, byte.MaxValue, (byte) 0));
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
        message = ex.Message;
        return (Result) -1;
      }
    }
  }

  public static Result SetVisibilityOverridesOff(
    ExternalCommandData commandData,
    ref string message)
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
        message = ex.Message;
        return (Result) -1;
      }
    }
  }

  public static Result SetVisibilityFlatOn(ExternalCommandData commandData, ref string message)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    if (Utils.ViewUtils.ViewUtils.CurrentViewIsProjectBrowser(document))
      return (Result) 1;
    bool flag1 = false;
    bool flag2 = false;
    using (Transaction transaction = new Transaction(document, "Turn ON Flat"))
    {
      try
      {
        int num1 = (int) transaction.Start();
        ICollection<ElementId> filters = document.ActiveView.GetFilters();
        foreach (ElementId id in (IEnumerable<ElementId>) filters)
        {
          if ((document.GetElement(id) as ParameterFilterElement).Name.Contains("FLAT PRODUCT"))
          {
            flag1 = true;
            break;
          }
        }
        foreach (ElementId id in (IEnumerable<ElementId>) filters)
        {
          if ((document.GetElement(id) as ParameterFilterElement).Name.Contains("WARPED_ELEMENTS"))
          {
            flag2 = true;
            break;
          }
        }
        if (!flag1)
          VisibilityToggleFunctions.AddFlatFilter(document);
        if (!flag2)
          VisibilityToggleFunctions.AddWarpFilter(document);
        document.Regenerate();
        foreach (ElementId filter in (IEnumerable<ElementId>) document.ActiveView.GetFilters())
        {
          ParameterFilterElement element = document.GetElement(filter) as ParameterFilterElement;
          if (element.Name.ToUpper().Contains("FLAT PRODUCT") || element.Name.ToUpper().Contains("WARPED_ELEMENTS"))
            document.ActiveView.SetFilterVisibility(filter, true);
        }
        int num2 = (int) transaction.Commit();
        return (Result) 0;
      }
      catch (Exception ex)
      {
        message = ex.ToString();
        return (Result) -1;
      }
    }
  }

  public static Result SetVisibilityFlatOff(ExternalCommandData commandData, ref string message)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    if (Utils.ViewUtils.ViewUtils.CurrentViewIsProjectBrowser(document))
      return (Result) 1;
    bool flag1 = false;
    bool flag2 = false;
    using (Transaction transaction = new Transaction(document, "Turn OFF Flat"))
    {
      try
      {
        int num1 = (int) transaction.Start();
        ICollection<ElementId> filters = document.ActiveView.GetFilters();
        foreach (ElementId id in (IEnumerable<ElementId>) filters)
        {
          if ((document.GetElement(id) as ParameterFilterElement).Name.Contains("FLAT PRODUCT"))
          {
            flag1 = true;
            break;
          }
        }
        foreach (ElementId id in (IEnumerable<ElementId>) filters)
        {
          if ((document.GetElement(id) as ParameterFilterElement).Name.Contains("WARPED_ELEMENTS"))
          {
            flag2 = true;
            break;
          }
        }
        if (!flag1)
          VisibilityToggleFunctions.AddFlatFilter(document);
        if (!flag2)
          VisibilityToggleFunctions.AddWarpFilter(document);
        document.Regenerate();
        foreach (ElementId filter in (IEnumerable<ElementId>) document.ActiveView.GetFilters())
        {
          ParameterFilterElement element = document.GetElement(filter) as ParameterFilterElement;
          if (element.Name.ToUpper().Contains("FLAT PRODUCT") || element.Name.ToUpper().Contains("WARPED_ELEMENTS"))
            document.ActiveView.SetFilterVisibility(filter, false);
        }
        int num2 = (int) transaction.Commit();
        return (Result) 0;
      }
      catch (Exception ex)
      {
        message = ex.ToString();
        return (Result) -1;
      }
    }
  }

  public static Result SetVisibilityWarpOn(ExternalCommandData commandData, ref string message)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    if (Utils.ViewUtils.ViewUtils.CurrentViewIsProjectBrowser(document))
      return (Result) 1;
    bool flag1 = false;
    bool flag2 = false;
    using (Transaction transaction = new Transaction(document, "Turn ON Warped"))
    {
      try
      {
        int num1 = (int) transaction.Start();
        ICollection<ElementId> filters = document.ActiveView.GetFilters();
        foreach (ElementId id in (IEnumerable<ElementId>) filters)
        {
          if ((document.GetElement(id) as ParameterFilterElement).Name.Contains("WARPED PRODUCT"))
          {
            flag1 = true;
            break;
          }
        }
        foreach (ElementId id in (IEnumerable<ElementId>) filters)
        {
          if ((document.GetElement(id) as ParameterFilterElement).Name.Contains("ENTOURAGE"))
          {
            flag2 = true;
            break;
          }
        }
        if (!flag1)
          VisibilityToggleFunctions.AddWarpedProductFilter(document);
        if (!flag2)
          VisibilityToggleFunctions.AddEntourageFilter(document);
        document.Regenerate();
        foreach (ElementId filter in (IEnumerable<ElementId>) document.ActiveView.GetFilters())
        {
          ParameterFilterElement element = document.GetElement(filter) as ParameterFilterElement;
          if (element.Name.ToUpper().Contains("WARPED PRODUCT") || element.Name.ToUpper().Contains("ENTOURAGE"))
            document.ActiveView.SetFilterVisibility(filter, true);
        }
        int num2 = (int) transaction.Commit();
        return (Result) 0;
      }
      catch (Exception ex)
      {
        message = ex.ToString();
        return (Result) -1;
      }
    }
  }

  public static Result SetVisibilityWarpOff(ExternalCommandData commandData, ref string message)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    if (Utils.ViewUtils.ViewUtils.CurrentViewIsProjectBrowser(document))
      return (Result) 1;
    bool flag1 = false;
    bool flag2 = false;
    using (Transaction transaction = new Transaction(document, "Turn OFF Warped"))
    {
      try
      {
        int num1 = (int) transaction.Start();
        ICollection<ElementId> filters = document.ActiveView.GetFilters();
        foreach (ElementId id in (IEnumerable<ElementId>) filters)
        {
          if ((document.GetElement(id) as ParameterFilterElement).Name.Contains("WARPED PRODUCT"))
          {
            flag1 = true;
            break;
          }
        }
        foreach (ElementId id in (IEnumerable<ElementId>) filters)
        {
          if ((document.GetElement(id) as ParameterFilterElement).Name.Contains("ENTOURAGE"))
          {
            flag2 = true;
            break;
          }
        }
        if (!flag1)
          VisibilityToggleFunctions.AddWarpedProductFilter(document);
        if (!flag2)
          VisibilityToggleFunctions.AddEntourageFilter(document);
        document.Regenerate();
        foreach (ElementId filter in (IEnumerable<ElementId>) document.ActiveView.GetFilters())
        {
          ParameterFilterElement element = document.GetElement(filter) as ParameterFilterElement;
          if (element.Name.ToUpper().Contains("WARPED PRODUCT") || element.Name.ToUpper().Contains("ENTOURAGE"))
            document.ActiveView.SetFilterVisibility(filter, false);
        }
        int num2 = (int) transaction.Commit();
        return (Result) 0;
      }
      catch (Exception ex)
      {
        message = ex.ToString();
        return (Result) -1;
      }
    }
  }

  public static Result SetPrecastOpacity(
    ExternalCommandData commandData,
    ref string message,
    int opacity)
  {
    Document document = commandData.Application.ActiveUIDocument.Document;
    if (Utils.ViewUtils.ViewUtils.CurrentViewIsProjectBrowser(document))
      return (Result) 1;
    bool flag = false;
    using (Transaction transaction = new Transaction(document, "PRECAST PRODUCT - Opacity"))
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
          VisibilityToggleFunctions.AddFilter(document, "PRECAST PRODUCT");
        document.Regenerate();
        foreach (ElementId filter in (IEnumerable<ElementId>) document.ActiveView.GetFilters())
        {
          ParameterFilterElement element = document.GetElement(filter) as ParameterFilterElement;
          if (element.Name.ToUpper().Contains("PRECAST PRODUCT"))
          {
            OverrideGraphicSettings filterOverrides = document.ActiveView.GetFilterOverrides(element.Id);
            filterOverrides.SetSurfaceTransparency(100 - opacity);
            document.ActiveView.SetFilterOverrides(element.Id, filterOverrides);
            int num2 = (int) transaction.Commit();
            return (Result) 0;
          }
        }
        return (Result) 0;
      }
      catch (Exception ex)
      {
        message = ex.Message;
        int num = (int) transaction.RollBack();
        return (Result) -1;
      }
    }
  }

  public static Result getPrecastOpacity(
    ExternalCommandData commandData,
    ref string message,
    out int opacity)
  {
    opacity = 100;
    Document document = commandData.Application.ActiveUIDocument.Document;
    if (Utils.ViewUtils.ViewUtils.CurrentViewIsProjectBrowser(document))
      return (Result) 1;
    bool flag = false;
    try
    {
      foreach (ElementId filter in (IEnumerable<ElementId>) document.ActiveView.GetFilters())
      {
        if ((document.GetElement(filter) as ParameterFilterElement).Name.ToUpper().Contains("PRECAST PRODUCT"))
        {
          flag = true;
          break;
        }
      }
      if (!flag)
      {
        opacity = 100;
        return (Result) 0;
      }
      foreach (ElementId filter in (IEnumerable<ElementId>) document.ActiveView.GetFilters())
      {
        ParameterFilterElement element = document.GetElement(filter) as ParameterFilterElement;
        if (element.Name.ToUpper().Contains("PRECAST PRODUCT"))
        {
          OverrideGraphicSettings filterOverrides = document.ActiveView.GetFilterOverrides(element.Id);
          opacity = 100 - filterOverrides.Transparency;
          return (Result) 0;
        }
      }
      return (Result) 0;
    }
    catch (Exception ex)
    {
      message = ex.Message;
      return (Result) -1;
    }
  }

  public static void CheckVisibilityFilters(UIApplication uiapp, ViewActivatedEventArgs args)
  {
    UIDocument activeUiDocument = uiapp.ActiveUIDocument;
    if (activeUiDocument == null)
      return;
    Document document = activeUiDocument.Document;
    if (document.IsFamilyDocument)
      return;
    List<string> stringList1 = new List<string>()
    {
      "CIP",
      "Embed",
      "Erection",
      "Foundation",
      "Grout",
      "Insulation",
      "Lifting",
      "Mesh",
      "Rebar",
      "WWF",
      "Flat",
      "WARPED PRODUCT",
      "Void"
    };
    List<string> stringList2 = new List<string>()
    {
      "CIP",
      "Embed",
      "Erection",
      "Foundation",
      "Grout",
      "Insulation",
      "Lifting",
      "Mesh",
      "Rebar",
      "WWF",
      "Flat",
      "WARPED PRODUCT",
      "Void"
    };
    List<string> stringList3 = new List<string>();
    ICollection<ElementId> filters = document.ActiveView.GetFilters();
    List<string> stringList4 = new List<string>();
    foreach (ElementId elementId in (IEnumerable<ElementId>) filters)
    {
      ParameterFilterElement element = document.GetElement(elementId) as ParameterFilterElement;
      string str1 = "";
      stringList4.Add(element.Name);
      foreach (string str2 in stringList1)
      {
        str1 = "";
        if (element.Name.Contains(str2.ToUpper()) && !document.ActiveView.GetFilterVisibility(elementId))
        {
          stringList3.Add(str2);
          str1 = str2;
          break;
        }
      }
      if (stringList1.Contains(str1))
        stringList1.Remove(str1);
    }
    string str3 = "EDGE^R";
    RibbonPanel ribbonPanel1 = (RibbonPanel) null;
    foreach (RibbonPanel ribbonPanel2 in uiapp.GetRibbonPanels(str3))
    {
      if (ribbonPanel2.Name == "Visibility")
      {
        ribbonPanel1 = ribbonPanel2;
        break;
      }
    }
    if (ribbonPanel1 == null)
      return;
    foreach (string str4 in stringList2)
    {
      string key = str4;
      if (key == "Embed" || key == "Void")
        key += "s";
      if (key == "WARPED PRODUCT")
        key = "Warped";
      if (stringList1.Contains(str4))
        VisibilityToggles.visTogglesDict[key].visFlag = true;
      else if (stringList3.Contains(str4))
        VisibilityToggles.visTogglesDict[key].visFlag = false;
      foreach (RibbonItem ribbonItem in (IEnumerable<RibbonItem>) ribbonPanel1.GetItems())
      {
        if (ribbonItem.ItemText == key)
        {
          PushButton pushButton = ribbonItem as PushButton;
          string currentImage = VisibilityToggles.visTogglesDict[key].getCurrentImage();
          ((RibbonButton) pushButton).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage(currentImage);
          ((RibbonButton) pushButton).Image = (ImageSource) AppRibbonSetup.GetBitmapImage($"{currentImage.Substring(0, currentImage.Length - 4)}_Small{currentImage.Substring(currentImage.Length - 4, 4)}");
          break;
        }
      }
    }
  }

  private static void SetFilterOverrides(ParameterFilterElement filter, Autodesk.Revit.DB.Color color)
  {
    Document document = ActiveModel.Document;
    OverrideGraphicSettings filterOverrides = document.ActiveView.GetFilterOverrides(filter.Id);
    filterOverrides.SetSurfaceForegroundPatternColor(color);
    filterOverrides.SetSurfaceForegroundPatternId(new ElementId(3));
    document.ActiveView.SetFilterOverrides(filter.Id, filterOverrides);
  }

  private static void AddFilter(Document doc, string type)
  {
    foreach (ParameterFilterElement parameterFilterElement in new FilteredElementCollector(doc).OfClass(typeof (ParameterFilterElement)))
    {
      if (parameterFilterElement.Name.Contains(type))
      {
        doc.ActiveView.AddFilter(parameterFilterElement.Id);
        return;
      }
    }
    new TaskDialog("Warning")
    {
      FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
      MainInstruction = $"Warning:  There is no filter for \"{type}\" available to turn on. Make sure that the filter is both created and enabled in your Visibility/Graphics settings and try again."
    }.Show();
  }

  private static void AddFilter(
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

  private static void AddFlatFilter(Document doc)
  {
    foreach (ParameterFilterElement parameterFilterElement in new FilteredElementCollector(doc).OfClass(typeof (ParameterFilterElement)))
    {
      if (parameterFilterElement.Name.Contains("FLAT PRODUCT"))
      {
        doc.ActiveView.AddFilter(parameterFilterElement.Id);
        return;
      }
    }
    new TaskDialog("Warning")
    {
      FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
      MainInstruction = "Warning:  There is no filter for \"FLAT PRODUCT\" available to turn off. Make sure that the filter is both created and enabled in your Visibility/Graphics settings and try again."
    }.Show();
  }

  private static void AddWarpFilter(Document doc)
  {
    foreach (ParameterFilterElement parameterFilterElement in new FilteredElementCollector(doc).OfClass(typeof (ParameterFilterElement)))
    {
      if (parameterFilterElement.Name.Contains("WARPED_ELEMENTS"))
      {
        doc.ActiveView.AddFilter(parameterFilterElement.Id);
        return;
      }
    }
    new TaskDialog("Warning")
    {
      FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
      MainInstruction = "Warning:  There is no filter for \"WARPED_ELEMENTS\" available. Please run Project Shared Parameters to automatically create the filter and try again."
    }.Show();
  }

  private static void AddWarpedProductFilter(Document doc)
  {
    foreach (ParameterFilterElement parameterFilterElement in new FilteredElementCollector(doc).OfClass(typeof (ParameterFilterElement)))
    {
      if (parameterFilterElement.Name.Contains("WARPED PRODUCT"))
      {
        doc.ActiveView.AddFilter(parameterFilterElement.Id);
        return;
      }
    }
    new TaskDialog("Warning")
    {
      FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
      MainInstruction = "Warning:  There is no filter for \"WARPED PRODUCT\" available to turn on. Make sure that the filter is both created and enabled in your Visibility/Graphics settings and try again."
    }.Show();
  }

  private static void AddEntourageFilter(Document doc)
  {
    foreach (ParameterFilterElement parameterFilterElement in new FilteredElementCollector(doc).OfClass(typeof (ParameterFilterElement)))
    {
      if (parameterFilterElement.Name.Contains("ENTOURAGE"))
      {
        doc.ActiveView.AddFilter(parameterFilterElement.Id);
        return;
      }
    }
    new TaskDialog("Warning")
    {
      FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
      MainInstruction = "Warning:  There is no filter for \"ENTOURAGE\" available. Please run Project Shared Parameters to automatically create the filter and try again."
    }.Show();
  }
}
