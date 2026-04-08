// Decompiled with JetBrains decompiler
// Type: EDGE.Cloud.Models.DataContracts.BucketCreationData
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System.Collections.Generic;

#nullable disable
namespace EDGE.Cloud.Models.DataContracts;

public class BucketCreationData
{
  public BucketCreationData(string name, BucketPolicyEnum policy)
  {
    this.Name = name.ToLower();
    this.Policy = policy;
    this.servicesAllowed = new List<ServicesAllowed>();
  }

  public string Name { get; private set; }

  public List<ServicesAllowed> servicesAllowed { get; private set; }

  public BucketPolicyEnum Policy { get; private set; }

  public string ToJsonString()
  {
    string str1 = $"{{\"bucketKey\":\"{this.Name}\",\"servicesAllowed\":{{";
    foreach (ServicesAllowed servicesAllowed in this.servicesAllowed)
      ;
    string str2 = str1 + "},";
    string jsonString;
    switch (this.Policy)
    {
      case BucketPolicyEnum.kTransient:
        jsonString = str2 + "\"policyKey\":\"transient\"}";
        break;
      case BucketPolicyEnum.kTemporary:
        jsonString = str2 + "\"policyKey\":\"temporary\"}";
        break;
      case BucketPolicyEnum.kPersistent:
        jsonString = str2 + "\"policyKey\":\"persistent\"}";
        break;
      default:
        jsonString = str2 + "\"policyKey\":\"transient\"}";
        break;
    }
    return jsonString;
  }
}
