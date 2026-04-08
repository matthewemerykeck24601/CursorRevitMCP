// Decompiled with JetBrains decompiler
// Type: EDGE.Cloud.Models.DataContracts.ViewableMessage
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Newtonsoft.Json;

#nullable disable
namespace EDGE.Cloud.Models.DataContracts;

public class ViewableMessage
{
  public string Type { get; private set; }

  public string Code { get; private set; }

  public string Message { get; private set; }

  public ViewableMessage()
  {
  }

  [JsonConstructor]
  public ViewableMessage(string type, string code, string message)
  {
    this.Type = type;
    this.Code = code;
    this.Message = message;
  }
}
