// Decompiled with JetBrains decompiler
// Type: EDGE.VisibilityTools.VisibilityToggles
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System.Collections.Generic;

#nullable disable
namespace EDGE.VisibilityTools;

public class VisibilityToggles
{
  public static Dictionary<string, VisibilityToggleButtonData> visTogglesDict = new Dictionary<string, VisibilityToggleButtonData>()
  {
    {
      "CIP",
      new VisibilityToggleButtonData("CIPON.png", "CIPOFF.png", "CIP")
    },
    {
      "Embeds",
      new VisibilityToggleButtonData("EmbedON.png", "EmbedOFF.png", "EMBED")
    },
    {
      "Erection",
      new VisibilityToggleButtonData("ErectionON.png", "ErectionOFF.png", "ERECTION")
    },
    {
      "Foundation",
      new VisibilityToggleButtonData("FootingON.png", "FootingOFF.png", "FOUNDATION")
    },
    {
      "Grout",
      new VisibilityToggleButtonData("GroutON.png", "GroutOFF.png", "GROUT")
    },
    {
      "Insulation",
      new VisibilityToggleButtonData("InsulationON.png", "InsulationOFF.png", "INSULATION")
    },
    {
      "Lifting",
      new VisibilityToggleButtonData("LiftingON.png", "LiftingOFF.png", "LIFTING")
    },
    {
      "Mesh",
      new VisibilityToggleButtonData("MeshON.png", "MeshOFF.png", "MESH")
    },
    {
      "Rebar",
      new VisibilityToggleButtonData("RebarON.png", "RebarOFF.png", "REBAR")
    },
    {
      "WWF",
      new VisibilityToggleButtonData("WWFON.png", "WWFOFF.png", "WWF")
    },
    {
      "Flat",
      new VisibilityToggleButtonData("FlatON.png", "FlatOFF.png", "FLAT PRODUCT")
    },
    {
      "Warped",
      new VisibilityToggleButtonData("WarpedON.png", "WarpedOFF.png", "WARPED PRODUCT")
    },
    {
      "Voids",
      new VisibilityToggleButtonData("VoidON.png", "VoidOFF.png", "VOID (ALL)")
    },
    {
      "Overrides",
      new VisibilityToggleButtonData("VisibilityOverridesON.png", "VisibilityOverridesOFF.png", "OVERRIDES")
    }
  };
}
