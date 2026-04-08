// Decompiled with JetBrains decompiler
// Type: EDGE.Cloud.Models.Model
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System.Collections.Generic;

#nullable disable
namespace EDGE.Cloud.Models;

public class Model
{
  public int ManufactureId { get; set; }

  public int PlantId { get; set; }

  public int ProjectId { get; set; }

  public int AssemblyId { get; set; }

  public int ViewId { get; set; }

  public int SheetId { get; set; }

  public string ManufactureName { get; set; }

  public string PlantName { get; set; }

  public string ProjectName { get; set; }

  public string AssemblyName { get; set; }

  public string ViewName { get; set; }

  public string SheetName { get; set; }

  public bool IsForge { get; set; }

  public ProjectData ProjectData { get; set; }

  public AssemblyData AssemblyData { get; set; }

  public IEnumerable<Manufacture> Manufactures { get; set; }

  public IEnumerable<Plant> Plants { get; set; }

  public IEnumerable<Project> Projects { get; set; }

  public IEnumerable<Assembly> Assemblies { get; set; }

  public IEnumerable<View> Views { get; set; }

  public IEnumerable<View> Sheets { get; set; }

  public Model() => this.IsForge = true;
}
