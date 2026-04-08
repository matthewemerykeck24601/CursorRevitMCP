// Decompiled with JetBrains decompiler
// Type: EDGE.Cloud.JsonGeometryData
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System.Collections.Generic;
using System.Runtime.Serialization;

#nullable disable
namespace EDGE.Cloud;

[DataContract]
internal class JsonGeometryData
{
  [DataMember]
  public List<double> vertices { get; set; }

  [DataMember]
  public List<double> normals { get; set; }

  [DataMember]
  public List<double> uvs { get; set; }

  [DataMember]
  public List<int> faces { get; set; }

  [DataMember]
  public double scale { get; set; }

  [DataMember]
  public bool visible { get; set; }

  [DataMember]
  public bool castShadow { get; set; }

  [DataMember]
  public bool receiveShadow { get; set; }

  [DataMember]
  public bool doubleSided { get; set; }
}
