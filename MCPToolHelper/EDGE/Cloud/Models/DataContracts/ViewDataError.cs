// Decompiled with JetBrains decompiler
// Type: EDGE.Cloud.Models.DataContracts.ViewDataError
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net;

#nullable disable
namespace EDGE.Cloud.Models.DataContracts;

public class ViewDataError
{
  public HttpStatusCode StatusCode { get; set; }

  public string Reason { get; set; }

  public string DeveloperMessage { get; set; }

  public string ErrorCode { get; set; }

  [JsonProperty(PropertyName = "more info")]
  public Uri MoreInfo { get; set; }

  public List<ErrorEventArgs> JsonErrors { get; set; }

  public Exception Exception { get; set; }

  public ViewDataError()
  {
  }

  public ViewDataError(Exception ex) => this.Exception = ex;

  public ViewDataError(HttpStatusCode statusCode) => this.StatusCode = statusCode;

  public ViewDataError(List<ErrorEventArgs> jsonErrors) => this.JsonErrors = jsonErrors;
}
