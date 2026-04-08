// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.Tools.MKUtils
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System.Collections.Generic;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification.Tools;

public class MKUtils
{
  public static List<string> LengthParameterNameStrings = new List<string>()
  {
    "DIM_LENGTH",
    "DIM_DEPTH",
    "DIM_THICKNESS",
    "DIM_WIDTH",
    "DIM_HEIGHT",
    "DIM_DIAGONAL",
    "DIM_DIAMETER",
    "DIM_ARC_LENGTH",
    "DIM_DEPTH_ACTUAL",
    "DIM_WYTHE_OUTER",
    "DIM_DEPTH_FORM",
    "DIM_WYTHE_INSULATION",
    "DIM_WIDTH_INCHES",
    "DIM_WYTHE_INNER",
    "DIM_STEM_DEPTH",
    "DIM_HEIGHT_INCHES",
    "DIM_DEPTH_INCHES"
  };
  public static List<string> WeightParameterNameStrings = new List<string>()
  {
    "MEMBER_WEIGHT_CAST",
    "WEIGHT_PER_UNIT"
  };
  public static List<string> VolumeParameterNameStrings = new List<string>()
  {
    "MEMBER_VOLUME_CAST"
  };
  public static List<string> AreaParameterNameStrings = new List<string>()
  {
    "ARCH_SF_1",
    "ARCH_SF_2",
    "ARCH_SF_3",
    "ARCH_SF_4",
    "DIM_SQFT"
  };
  internal static List<string> TextParameterNameStrings = new List<string>()
  {
    "CONSTRUCTION_PRODUCT",
    "DESIGN_NUMBER",
    "BOM_PRODUCT_HOST",
    "HANDLING_CODE"
  };
  public static List<string> StrengthParameterNameStrings = new List<string>()
  {
    "FINAL_STRENGTH",
    "RELEASE_STRENGTH"
  };
}
