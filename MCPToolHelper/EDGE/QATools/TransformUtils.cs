// Decompiled with JetBrains decompiler
// Type: EDGE.QATools.TransformUtils
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;

#nullable disable
namespace EDGE.QATools;

internal class TransformUtils
{
  public double[] GetTransformRotationAngles(XYZ[] orig_vecs, XYZ[] cur_vecs)
  {
    foreach (XYZ origVec in orig_vecs)
      origVec.Normalize();
    foreach (XYZ curVec in cur_vecs)
      curVec.Normalize();
    XYZ[] xyzArray = new XYZ[3];
    double[] transformRotationAngles = new double[3];
    if (1.0 - Math.Abs(xyzArray[0].Z) < 1E-05)
    {
      transformRotationAngles[1] = xyzArray[0].Z <= 0.0 ? Math.PI / 2.0 : -1.0 * Math.PI / 2.0;
      transformRotationAngles[2] = 0.0;
      transformRotationAngles[0] = Math.Acos(xyzArray[1].Y);
      if (xyzArray[1].X < 0.0)
        transformRotationAngles[0] = -transformRotationAngles[0];
    }
    else
    {
      transformRotationAngles[1] = Math.Asin(-xyzArray[0].Z);
      transformRotationAngles[0] = Math.Acos(xyzArray[2].Z / Math.Cos(transformRotationAngles[1]));
      if (xyzArray[1].Z < 0.0)
        transformRotationAngles[0] = -transformRotationAngles[0];
      transformRotationAngles[2] = Math.Acos(xyzArray[0].X / Math.Cos(transformRotationAngles[1]));
      if (xyzArray[0].Y < 0.0)
        transformRotationAngles[2] = -transformRotationAngles[2];
    }
    for (int index = 0; index < 3; ++index)
      transformRotationAngles[index] = transformRotationAngles[index] * 180.0 / Math.PI;
    return transformRotationAngles;
  }
}
