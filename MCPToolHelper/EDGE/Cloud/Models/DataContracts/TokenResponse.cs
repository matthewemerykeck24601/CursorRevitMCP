// Decompiled with JetBrains decompiler
// Type: EDGE.Cloud.Models.DataContracts.TokenResponse
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Newtonsoft.Json;

#nullable disable
namespace EDGE.Cloud.Models.DataContracts;

public class TokenResponse : ViewDataResponseBase
{
  [JsonProperty(PropertyName = "token_type")]
  public string TokenType { get; set; }

  [JsonProperty(PropertyName = "expires_in")]
  public int ExpirationTime { get; set; }

  [JsonProperty(PropertyName = "access_token")]
  public string AccessToken { get; set; }
}
