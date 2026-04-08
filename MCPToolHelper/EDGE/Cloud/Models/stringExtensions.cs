// Decompiled with JetBrains decompiler
// Type: EDGE.Cloud.Models.stringExtensions
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;
using System.Text;

#nullable disable
namespace EDGE.Cloud.Models;

public static class stringExtensions
{
  public static string ToBase64(this string str)
  {
    return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
  }

  public static string FromBase64(this string str)
  {
    try
    {
      return Encoding.UTF8.GetString(Convert.FromBase64String(str));
    }
    catch
    {
      return "Invalid base64 string: " + str;
    }
  }
}
