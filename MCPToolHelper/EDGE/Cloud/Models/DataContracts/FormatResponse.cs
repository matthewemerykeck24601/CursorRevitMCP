// Decompiled with JetBrains decompiler
// Type: EDGE.Cloud.Models.DataContracts.FormatResponse
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

#nullable disable
namespace EDGE.Cloud.Models.DataContracts;

public class FormatResponse : ViewDataResponseBase
{
  [JsonProperty(PropertyName = "extensions")]
  public List<string> Extensions { get; private set; }

  [JsonProperty(PropertyName = "channelMapping")]
  public JObject ChannelMapping { get; private set; }

  [JsonProperty(PropertyName = "regExp")]
  public JObject RegularExpressions { get; private set; }
}
