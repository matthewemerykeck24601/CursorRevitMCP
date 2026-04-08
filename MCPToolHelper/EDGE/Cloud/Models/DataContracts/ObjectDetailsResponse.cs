// Decompiled with JetBrains decompiler
// Type: EDGE.Cloud.Models.DataContracts.ObjectDetailsResponse
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;

#nullable disable
namespace EDGE.Cloud.Models.DataContracts;

public class ObjectDetailsResponse : ViewDataResponseBase
{
  [DisplayName("Bucket Key")]
  [JsonProperty(PropertyName = "bucketKey")]
  public string BucketKey { get; set; }

  [DisplayName("Objects")]
  public List<ObjectDetails> Objects { get; set; }

  [JsonProperty(PropertyName = "objectId")]
  public string ObjectId { get; set; }

  [JsonProperty(PropertyName = "objectKey")]
  public string ObjectKey { get; set; }

  [JsonProperty(PropertyName = "location")]
  public string Location { get; set; }

  [JsonProperty(PropertyName = "sha1")]
  public string SHA1 { get; set; }
}
