// Decompiled with JetBrains decompiler
// Type: EDGE.Cloud.JsonMetadata
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System.Collections.Generic;
using System.Runtime.Serialization;

#nullable disable
namespace EDGE.Cloud;

internal class JsonMetadata
{
  [DataMember]
  public string type { get; set; }

  [DataMember]
  public double version { get; set; }

  [DataMember]
  public string generator { get; set; }

  [DataMember]
  public string file { get; set; }

  public JsonChart chart { get; set; }

  public string notes { get; set; }

  public IDictionary<string, string> structuralFraming { get; set; }

  public IDictionary<string, string> parentOfChildren { get; set; }

  public Dictionary<string, Dictionary<string, string>> nodeRebars { get; set; }

  public Dictionary<string, Dictionary<string, string>> nodeMeshes { get; set; }

  public Dictionary<string, Dictionary<string, string>> nodeEmbeds { get; set; }

  public Dictionary<string, Dictionary<string, string>> nodeLiftings { get; set; }

  public Dictionary<string, Dictionary<string, string>> nodeStrands { get; set; }

  public Dictionary<string, Dictionary<string, string>> nodeConcretes { get; set; }

  public Dictionary<string, Dictionary<string, string>> nodeFraming { get; set; }

  public Dictionary<string, Dictionary<string, string>> nodeWallFinishes { get; set; }

  public Dictionary<string, Dictionary<string, string>> nodeFormliners { get; set; }

  public Dictionary<string, Dictionary<string, string>> nodeOthers { get; set; }
}
