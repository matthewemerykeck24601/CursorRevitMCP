// Decompiled with JetBrains decompiler
// Type: EDGE.Cloud.Models.DataContracts.ViewableResponse
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;

#nullable disable
namespace EDGE.Cloud.Models.DataContracts;

public class ViewableResponse : ViewDataResponseBase
{
  public string Guid { get; set; }

  public int Order { get; private set; }

  public string Version { get; private set; }

  public string Type { get; private set; }

  public string Name { get; private set; }

  public bool HasThumbnail { get; private set; }

  public string Mime { get; private set; }

  public string Progress { get; private set; }

  public string Status { get; private set; }

  public string Success { get; private set; }

  public string StartedAt { get; private set; }

  public string Role { get; private set; }

  public string URN { get; set; }

  [DisplayName("File Id")]
  public string FileId => this.URN == null ? "" : this.URN.FromBase64();

  public string Result { get; private set; }

  [Browsable(false)]
  public List<ViewableResponse> Children { get; private set; }

  public List<double> Camera { get; private set; }

  public List<double> Resolution { get; private set; }

  public List<ViewableMessage> Messages { get; private set; }

  public ViewableResponse()
  {
    this.Camera = new List<double>();
    this.Resolution = new List<double>();
    this.Children = new List<ViewableResponse>();
    this.Messages = new List<ViewableMessage>();
  }

  [JsonConstructor]
  public ViewableResponse(
    string guid,
    int order,
    string version,
    string type,
    string name,
    bool hasThumbnail,
    string mime,
    string progress,
    string status,
    string success,
    string startedAt,
    string role,
    string urn,
    string result,
    List<ViewableResponse> children,
    List<double> camera,
    List<double> resolution,
    List<ViewableMessage> messages)
  {
    this.Guid = guid;
    this.Order = order;
    this.Version = version;
    this.Type = type;
    this.Name = name;
    this.HasThumbnail = hasThumbnail;
    this.Mime = mime;
    this.Progress = progress;
    this.Status = status;
    this.Success = success;
    this.StartedAt = startedAt;
    this.Role = role;
    this.URN = urn;
    this.Result = result;
    this.Children = children;
    this.Camera = camera;
    this.Resolution = resolution;
    this.Messages = messages;
  }
}
