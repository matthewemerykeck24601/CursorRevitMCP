// Decompiled with JetBrains decompiler
// Type: EDGE.Cloud.Models.AuthenticationModel
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

#nullable disable
namespace EDGE.Cloud.Models;

public class AuthenticationModel
{
  public string Token { get; set; }

  public string ForgeToken { get; set; }

  public int UserId { get; set; }

  public string Username { get; set; }

  public string FirstName { get; set; }

  public string LastName { get; set; }

  public string Email { get; set; }

  public string Roles { get; set; }

  public int ExpiredIn { get; set; }

  public int ErrorCode { get; set; }

  public string Message { get; set; }
}
