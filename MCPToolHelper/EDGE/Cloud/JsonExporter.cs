// Decompiled with JetBrains decompiler
// Type: EDGE.Cloud.JsonExporter
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.TicketTools.TicketManager.ViewModels;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Utils.AssemblyUtils;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.Cloud;

public class JsonExporter
{
  private static AssemblyInstance _assemblyInstance;

  public List<ElementId> NonExportedElements { get; set; }

  public void Export(View3D view3D, string fileName)
  {
    if (view3D == null)
      return;
    Document document = view3D.Document;
    using (CustomExporter customExporter = new CustomExporter(document, (IExportContext) new JsonExportContext(document, fileName)))
    {
      customExporter.IncludeGeometricObjects = false;
      customExporter.ShouldStopOnError = false;
      customExporter.Export((View) view3D);
    }
  }

  public void Export(
    AssemblyInstance assemblyInstance,
    Func<string, byte[][], IRestResponse> saveModel)
  {
    this.Export(AssemblyViewUtils.Create3DOrthographic(assemblyInstance.Document, assemblyInstance.Id), saveModel);
  }

  public void Export(View3D view3D, Func<string, byte[][], IRestResponse> saveModel)
  {
    try
    {
      if (view3D == null)
        return;
      Document document = view3D.Document;
      JsonExporter._assemblyInstance = document.GetElement(view3D.AssociatedAssemblyInstanceId) as AssemblyInstance;
      using (CustomExporter customExporter = new CustomExporter(document, (IExportContext) new JsonExportContext(JsonExporter._assemblyInstance, saveModel)
      {
        NotToExportElements = this.NonExportedElements
      }))
      {
        customExporter.IncludeGeometricObjects = false;
        customExporter.ShouldStopOnError = false;
        customExporter.Export((View) view3D);
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine($"{ex.Message}\n{ex.StackTrace}");
      throw;
    }
  }

  public static Autodesk.Revit.DB.Parameter LookupParameter(Element elem, string paramName)
  {
    Element element = (Element) null;
    try
    {
      element = elem.Document.GetElement(elem.GetTypeId());
    }
    catch
    {
    }
    Autodesk.Revit.DB.Parameter parameter = elem.LookupParameter(paramName);
    if (parameter == null && element != null)
      parameter = element.LookupParameter(paramName);
    return parameter;
  }

  public static string GetParameterAsString(Element elem, string paramName)
  {
    Autodesk.Revit.DB.Parameter parameter = JsonExporter.LookupParameter(elem, paramName);
    string str = "";
    if (parameter != null && parameter.HasValue)
    {
      str = parameter.AsString();
      if (string.IsNullOrEmpty(str))
        str = parameter.AsValueString();
    }
    return str ?? "";
  }

  public static JsonChart GetChart(Document document)
  {
    using (new FilteredElementCollector(document))
    {
      MainViewModel mainViewModel = new MainViewModel();
      mainViewModel.ExecuteExport();
      int piecesReinforced = mainViewModel.PiecesReinforced;
      int piecesDetailed = mainViewModel.PiecesDetailed;
      int piecesReleased = mainViewModel.PiecesReleased;
      int totalPieces = mainViewModel.TotalPieces;
      string assemblyControlMark = JsonExporter.GetParameterAsString(JsonExporter.GetStructuralFraming(JsonExporter._assemblyInstance), "CONTROL_MARK");
      string str = string.Join(",", (IEnumerable<string>) new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_StructuralFraming).OfClass(typeof (FamilyInstance)).Where<Element>((Func<Element, bool>) (elem => JsonExporter.GetParameterAsString(elem, "CONTROL_MARK") == assemblyControlMark)).Select<Element, string>((Func<Element, string>) (elem => JsonExporter.GetParameterAsString(elem, "CONTROL_NUMBER"))).Distinct<string>().OrderBy<string, int>((Func<string, int>) (s => s.Length)).ThenBy<string, string>((Func<string, string>) (s => s)).ToList<string>());
      using (FilteredElementCollector elementCollector = new FilteredElementCollector(document))
      {
        elementCollector.OfCategory(BuiltInCategory.OST_StructuralFraming).OfClass(typeof (FamilyInstance)).GroupBy<Element, string>((Func<Element, string>) (elem => JsonExporter.GetParameterAsString(elem, "CONTROL_MARK"))).Where<IGrouping<string, Element>>((Func<IGrouping<string, Element>, bool>) (group => group.Where<Element>((Func<Element, bool>) (elem => elem.AssemblyInstanceId != ElementId.InvalidElementId)).Any<Element>())).Sum<IGrouping<string, Element>>((Func<IGrouping<string, Element>, int>) (Group => 1));
        return new JsonChart()
        {
          Detailed = piecesDetailed,
          Reinforced = piecesReinforced,
          Released = piecesReleased,
          TotalAssemblies = totalPieces,
          AssociatedControlNumbers = str
        };
      }
    }
  }

  private static AssemblyInstance GetAssemblyInstance(Document document)
  {
    using (FilteredElementCollector source = new FilteredElementCollector(document))
    {
      source.OfClass(typeof (AssemblyInstance));
      return source.Cast<AssemblyInstance>().ToList<AssemblyInstance>().First<AssemblyInstance>();
    }
  }

  public static FamilyInstance GetStructuralFraming(Document document)
  {
    structuralFraming = (FamilyInstance) null;
    AssemblyInstance assemblyInstance = JsonExporter.GetAssemblyInstance(document);
    IEnumerable<Element> source = assemblyInstance.GetMemberIds().Select<ElementId, Element>(new Func<ElementId, Element>(assemblyInstance.Document.GetElement)).Where<Element>((Func<Element, bool>) (s => s.Category.Id.IntegerValue == -2001320));
    if ((source.Any<Element>() ? source.First<Element>() : (Element) null) != null && source is FamilyInstance structuralFraming)
      structuralFraming = structuralFraming.SuperComponent as FamilyInstance;
    return structuralFraming;
  }

  public static Element GetStructuralFraming(AssemblyInstance assemblyInstance)
  {
    return assemblyInstance.GetStructuralFramingElement().GetTopLevelElement();
  }

  public static IDictionary<string, string> GetParentOfElements(AssemblyInstance assemblyInstance)
  {
    IDictionary<string, string> parentOfElements1 = (IDictionary<string, string>) new Dictionary<string, string>();
    foreach (Element element in assemblyInstance.GetMemberIds().Select<ElementId, Element>(new Func<ElementId, Element>(assemblyInstance.Document.GetElement)))
    {
      if (element is FamilyInstance familyInstance)
      {
        IDictionary<string, string> parentOfElements2 = JsonExporter.GetParentOfElements(familyInstance);
        foreach (string key in (IEnumerable<string>) parentOfElements2.Keys)
        {
          if (!parentOfElements1.ContainsKey(key))
            parentOfElements1.Add(key, parentOfElements2[key]);
        }
      }
    }
    return parentOfElements1;
  }

  public static IDictionary<string, string> GetParentOfElements(
    FamilyInstance familyInstance,
    bool isUniqueId = true)
  {
    IDictionary<string, string> parentOfElements = (IDictionary<string, string>) new Dictionary<string, string>();
    foreach (Element elem in familyInstance.GetSubComponentIds().Select<ElementId, Element>(new Func<ElementId, Element>(familyInstance.Document.GetElement)))
    {
      bool flag = false;
      string paramName = "MANUFACTURE_COMPONENT";
      string parameterAsString = JsonExporter.GetParameterAsString(elem, paramName);
      if (!string.IsNullOrEmpty(parameterAsString))
        flag = parameterAsString.ToLower().Contains("raw consumable");
      if (flag)
      {
        Element superComponent = elem.GetSuperComponent();
        string str = string.Empty;
        if (superComponent != null)
          str = isUniqueId ? JsonExporter.GetGuid(superComponent.UniqueId) : superComponent.Id.ToString();
        string key = isUniqueId ? JsonExporter.GetGuid(elem.UniqueId) : elem.Id.ToString();
        parentOfElements.Add(key, str);
      }
    }
    return parentOfElements;
  }

  public static void GetLocationInForm(Document document)
  {
    foreach (ElementId memberId in (IEnumerable<ElementId>) JsonExporter.GetAssemblyInstance(document).GetMemberIds())
      ;
  }

  public static Dictionary<string, string> GetParamData(Element elem)
  {
    Dictionary<string, string> paramDict = new Dictionary<string, string>();
    switch (elem)
    {
      case null:
        return paramDict;
      case FamilyInstance familyInstance:
        foreach (Autodesk.Revit.DB.Parameter parameter in familyInstance.Parameters)
          Util.AddParameterValue((IDictionary<string, string>) paramDict, parameter);
        IEnumerator enumerator1 = familyInstance.Symbol.Parameters.GetEnumerator();
        try
        {
          while (enumerator1.MoveNext())
          {
            Autodesk.Revit.DB.Parameter current = (Autodesk.Revit.DB.Parameter) enumerator1.Current;
            Util.AddParameterValue((IDictionary<string, string>) paramDict, current, "TYPE_");
          }
          break;
        }
        finally
        {
          if (enumerator1 is IDisposable disposable)
            disposable.Dispose();
        }
      case Wall _:
        IEnumerator enumerator2 = (elem as Wall).Parameters.GetEnumerator();
        try
        {
          while (enumerator2.MoveNext())
          {
            Autodesk.Revit.DB.Parameter current = (Autodesk.Revit.DB.Parameter) enumerator2.Current;
            Util.AddParameterValue((IDictionary<string, string>) paramDict, current);
          }
          break;
        }
        finally
        {
          if (enumerator2 is IDisposable disposable)
            disposable.Dispose();
        }
      case AssemblyInstance _:
        IEnumerator enumerator3 = (elem as AssemblyInstance).Parameters.GetEnumerator();
        try
        {
          while (enumerator3.MoveNext())
          {
            Autodesk.Revit.DB.Parameter current = (Autodesk.Revit.DB.Parameter) enumerator3.Current;
            Util.AddParameterValue((IDictionary<string, string>) paramDict, current);
          }
          break;
        }
        finally
        {
          if (enumerator3 is IDisposable disposable)
            disposable.Dispose();
        }
    }
    return paramDict;
  }

  public static string GetGuid(string UniqueId)
  {
    Guid guid = new Guid(UniqueId.Substring(0, 36));
    int num1 = int.Parse(UniqueId.Substring(37), NumberStyles.AllowHexSpecifier);
    int num2 = int.Parse(UniqueId.Substring(28, 8), NumberStyles.AllowHexSpecifier) ^ num1;
    return UniqueId.Substring(0, 28) + num2.ToString("x8");
  }

  internal class ElementData
  {
    public ElementData(Element elem)
    {
      this.ElementId = elem.Id.IntegerValue;
      this.UniqueId = elem.UniqueId;
    }

    public int ElementId { get; set; }

    public string UniqueId { get; set; }

    public string LocationInForm { get; set; }

    public Dictionary<string, string> ParameterData { get; set; }
  }
}
