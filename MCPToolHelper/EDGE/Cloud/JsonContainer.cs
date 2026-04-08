// Decompiled with JetBrains decompiler
// Type: EDGE.Cloud.JsonContainer
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System.Collections.Generic;
using System.Runtime.Serialization;

#nullable disable
namespace EDGE.Cloud;

[DataContract]
internal class JsonContainer
{
  [DataMember]
  public List<JsonGeometry> geometries;
  [DataMember]
  public List<JsonMaterial> materials;

  [DataMember]
  public JsonMetadata metadata { get; set; }

  [DataMember(Name = "object")]
  public JsonObject obj { get; set; }
}
