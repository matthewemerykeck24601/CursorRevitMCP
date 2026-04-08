// Decompiled with JetBrains decompiler
// Type: EDGE.Cloud.Models.DataContracts.BucketDetailsResponse
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;

#nullable disable
namespace EDGE.Cloud.Models.DataContracts;

public class BucketDetailsResponse : ViewDataResponseBase
{
  [JsonProperty(PropertyName = "key")]
  public string BucketKey { get; private set; }

  public string Owner { get; private set; }

  [DisplayName("Created Date")]
  public DateTime CreatedDate { get; private set; }

  public List<ServicesAllowed> Permissions { get; private set; }

  [DisplayName("Policy")]
  [JsonProperty(PropertyName = "policykey")]
  public string Policy { get; private set; }

  public BucketDetailsResponse() => this.Permissions = new List<ServicesAllowed>();

  [JsonConstructor]
  public BucketDetailsResponse(
    string bucketKey,
    string owner,
    double createdDate,
    List<ServicesAllowed> permissions,
    string policy)
  {
    this.BucketKey = bucketKey;
    this.Owner = owner;
    this.CreatedDate = BucketDetailsResponse.FromUnixTime(createdDate);
    this.Permissions = permissions;
    this.Policy = policy;
  }

  public BucketDetailsResponse(
    string bucketKey,
    string owner,
    DateTime createdDate,
    List<ServicesAllowed> permissions,
    string policy)
  {
    this.BucketKey = bucketKey;
    this.Owner = owner;
    this.CreatedDate = createdDate;
    this.Permissions = permissions;
    this.Policy = policy;
  }

  private static DateTime FromUnixTime(double unixTime)
  {
    return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTime * 0.001);
  }
}
