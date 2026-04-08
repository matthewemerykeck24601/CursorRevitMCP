// Decompiled with JetBrains decompiler
// Type: EDGE.Cloud.Models.AssemblyDTO
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;

#nullable disable
namespace EDGE.Cloud.Models;

public class AssemblyDTO
{
  public int Id { get; set; }

  public int ProjectId { get; set; }

  public string Guid { get; set; }

  public string Baseurl { get; set; }

  public string Name { get; set; }

  public int Active { get; set; }

  public string EmpName { get; set; }

  public DateTime CreationDate { get; set; }

  public string HostName { get; set; }

  public string IpAddress { get; set; }

  public string JsonData { get; set; }

  public byte[] BlobData { get; set; }

  public AssemblyDTO() => this.CreationDate = DateTime.Now;
}
