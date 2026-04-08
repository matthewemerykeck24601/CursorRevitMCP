// Decompiled with JetBrains decompiler
// Type: EDGE.VisibilityTools.VisibilityToggleButtonData
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

#nullable disable
namespace EDGE.VisibilityTools;

public class VisibilityToggleButtonData
{
  public string onImage;
  public string offImage;
  public string filterTag;
  public bool visFlag;

  public VisibilityToggleButtonData(string onImage, string offImage, string filterTag)
  {
    this.onImage = onImage;
    this.offImage = offImage;
    this.filterTag = filterTag;
    this.visFlag = true;
  }

  public string getCurrentImage() => this.visFlag ? this.onImage : this.offImage;
}
