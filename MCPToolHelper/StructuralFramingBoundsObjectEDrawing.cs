// Decompiled with JetBrains decompiler
// Type: StructuralFramingBoundsObjectEDrawing
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System.Collections.Generic;

#nullable disable
internal class StructuralFramingBoundsObjectEDrawing
{
  public double xMin;
  public double xMax;
  public double xMid;
  public double yMin;
  public double yMax;
  public double yMid;
  public double Width;
  public double Height;
  public List<XYZ> outlinePoints;
  public Transform viewTransform;
  private Autodesk.Revit.DB.FamilyInstance _sfFamInst;
  private View _view;
}
