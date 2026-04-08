// Decompiled with JetBrains decompiler
// Type: EDGE.Cloud.Models.Project
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;

#nullable disable
namespace EDGE.Cloud.Models;

public class Project
{
  public int Id { get; set; }

  public int Id2 { get; set; }

  public int PlantId { get; set; }

  public string Guid { get; set; }

  public string Name { get; set; }

  public string Number { get; set; }

  public bool Active { get; set; }

  public string EmpName { get; set; }

  public DateTime CreationDate { get; set; }

  public string HostName { get; set; }

  public string IpAddress { get; set; }

  public byte[] BlobData { get; set; }

  public ProjectType ProjectType { get; set; }

  public static string GetProjectName(string projectName)
  {
    switch (Project.ProjecTypeFromProjectName(projectName))
    {
      case ProjectType.Export:
      case ProjectType.Upload:
        projectName = projectName.Substring(0, projectName.Length - 1);
        break;
    }
    return projectName;
  }

  public static string CharacterFromProjectType(ProjectType projectType)
  {
    return ((char) projectType).ToString();
  }

  public static ProjectType ProjecTypeFromProjectName(string projectName)
  {
    ProjectType projectType = ProjectType.None;
    if (!string.IsNullOrEmpty(projectName) && projectName.Length > 0)
      projectType = Project.ProjectTypeFromCharacter(projectName.Substring(projectName.Length - 1, 1));
    return projectType;
  }

  private static ProjectType ProjectTypeFromCharacter(string character)
  {
    return (ProjectType) character.ToCharArray()[0];
  }
}
