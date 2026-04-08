// Decompiled with JetBrains decompiler
// Type: EDGE.Cloud.Util
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System.Collections.Generic;

#nullable disable
namespace EDGE.Cloud;

internal class Util
{
  public static string RealString(double a) => a.ToString("0.##");

  public static string PointString(XYZ p)
  {
    return $"({Util.RealString(p.X)},{Util.RealString(p.Y)},{Util.RealString(p.Z)})";
  }

  public static int ColorToInt(Color color)
  {
    return (int) color.Red << 16 /*0x10*/ | (int) color.Green << 8 | (int) color.Blue;
  }

  public static string ElementDescription(Element e)
  {
    if (e == null)
      return "<null>";
    FamilyInstance familyInstance = e as FamilyInstance;
    return $"{e.GetType().Name} {(e.Category == null ? string.Empty : e.Category.Name + " ")}{(familyInstance == null ? string.Empty : familyInstance.Symbol.Family.Name + " ")}{(familyInstance == null || e.Name.Equals(familyInstance.Symbol.Name) ? string.Empty : familyInstance.Symbol.Name + " ")}<{e.Id.IntegerValue} {e.Name}>";
  }

  private static void AddToDictionary(
    string key,
    string value,
    IDictionary<string, string> dictionary)
  {
    if (dictionary.ContainsKey(key))
      return;
    dictionary.Add(key, value);
  }

  public static void AddParameterValue(
    IDictionary<string, string> paramDict,
    Parameter value,
    string prepend = "")
  {
    if (value == null)
      return;
    string key = prepend + value.Definition.Name;
    string str1 = "";
    if (paramDict.ContainsKey(key))
      return;
    if (value.StorageType == StorageType.String)
    {
      str1 = value.AsString();
    }
    else
    {
      string str2 = string.Empty;
      string str3 = value.AsValueString();
      if (!string.IsNullOrEmpty(str3))
      {
        string[] strArray = str3.Split(' ');
        if (strArray.Length > 1)
        {
          string str4 = strArray[0];
          for (int index = 1; index < strArray.Length; ++index)
            str2 = strArray[index] + " ";
          str2 = str2.Trim();
        }
        str1 = value.StorageType != StorageType.Integer ? value.AsValueString() : value.AsInteger().ToString() + (string.IsNullOrEmpty(str2) ? "" : " " + str2);
      }
    }
    if (!string.IsNullOrEmpty(str1))
    {
      if (str1.Contains("Ø"))
        str1 = str1.Replace("Ø", "^O^");
      if (str1.Contains("ø"))
        str1 = str1.Replace("ø", "^o^");
      if (str1.Contains("°"))
        str1 = str1.Replace("°", "^0^");
      if (str1.Contains("\u00B2"))
        str1 = str1.Replace("\u00B2", "^2^");
      if (str1.Contains("\u00B3"))
        str1 = str1.Replace("\u00B3", "^3^");
      if (str1.Contains("·"))
        str1 = str1.Replace("·", "^.^");
    }
    if (paramDict.ContainsKey(key))
      return;
    paramDict.Add(key, str1);
  }

  public static void AddElementProperties(Element element, IDictionary<string, string> paramDict)
  {
    foreach (Parameter orderedParameter in (IEnumerable<Parameter>) element.GetOrderedParameters())
      Util.AddParameterValue(paramDict, orderedParameter);
  }

  public static Dictionary<string, string> GetElementProperties(Element element, bool includeType)
  {
    Dictionary<string, string> paramDict = new Dictionary<string, string>();
    Util.AddToDictionary("Name", element.Name, (IDictionary<string, string>) paramDict);
    Util.AddElementProperties(element, (IDictionary<string, string>) paramDict);
    if (element.Category != null)
      Util.AddToDictionary("Category Name", element.Category.Name, (IDictionary<string, string>) paramDict);
    if (includeType)
    {
      ElementId typeId = element.GetTypeId();
      if (typeId != ElementId.InvalidElementId)
      {
        Element element1 = element.Document.GetElement(typeId);
        string empty = string.Empty;
        if (paramDict.ContainsKey("Name"))
          empty = paramDict["Name"];
        if (!string.IsNullOrEmpty(empty) && empty == element1.Name)
        {
          paramDict.Remove("Name");
          Util.AddToDictionary("Type Name", element1.Name, (IDictionary<string, string>) paramDict);
        }
        Util.AddElementProperties(element1, (IDictionary<string, string>) paramDict);
        if (element1 is ElementType elementType)
          Util.AddToDictionary("Family Name", elementType.FamilyName, (IDictionary<string, string>) paramDict);
      }
    }
    if (element is FamilyInstance familyInstance)
      Util.AddElementProperties((Element) familyInstance, (IDictionary<string, string>) paramDict);
    return paramDict;
  }
}
