// Decompiled with JetBrains decompiler
// Type: EDGE.Cloud.Models.DataContracts.ObjectDetails
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Text;

#nullable disable
namespace EDGE.Cloud.Models.DataContracts;

public class ObjectDetails
{
  [DisplayName("Location")]
  public Uri Location { get; private set; }

  [DisplayName("Size")]
  public int Size { get; private set; }

  [DisplayName("Object Key")]
  [JsonProperty(PropertyName = "key")]
  public string ObjectKey { get; private set; }

  [DisplayName("Bucket Key")]
  public string BucketKey => this.FileId.Replace("urn:adsk.objects:os.object:", "").Split('/')[0];

  [DisplayName("File Id")]
  [JsonProperty(PropertyName = "id")]
  public string FileId { get; private set; }

  [DisplayName("URN")]
  public string URN
  {
    get => Convert.ToBase64String(Encoding.UTF8.GetBytes(this.FileId));
    set
    {
    }
  }

  [DisplayName("Hash")]
  [JsonProperty(PropertyName = "sha-1")]
  public string Hash { get; private set; }

  [DisplayName("Content Type")]
  [JsonProperty(PropertyName = "content-type")]
  public Uri ContentType { get; private set; }

  public void SetFileId(string fileId) => this.FileId = fileId;

  [JsonConstructor]
  public ObjectDetails(
    Uri location,
    int size,
    string objectKey,
    string fileId,
    string hash,
    Uri contentType)
  {
    this.Location = location;
    this.Size = size;
    this.ObjectKey = objectKey;
    this.FileId = fileId;
    this.Hash = hash;
    this.ContentType = contentType;
  }
}
