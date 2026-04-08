// Decompiled with JetBrains decompiler
// Type: EDGE.Cloud.Models.View
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Newtonsoft.Json;

#nullable disable
namespace EDGE.Cloud.Models;

public class View
{
  public int Id { get; set; }

  public int Id2 { get; set; }

  public int AssemblyId { get; set; }

  public string Guid { get; set; }

  public string Name { get; set; }

  public string Urn { get; set; }

  public bool Is3d { get; set; }

  public byte[] PropertyData { get; set; }

  public object Properties { get; set; }

  [JsonIgnore]
  public string FileName { get; set; }

  [JsonIgnore]
  public bool AddToExisting { get; set; }
}
