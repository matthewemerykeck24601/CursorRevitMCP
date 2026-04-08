// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.GeometryComparer.TransformableSolid
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.AssemblyTools.MarkVerification.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification.GeometryComparer;

public class TransformableSolid : IEquatable<TransformableSolid>
{
  private List<ComparableFace> _faces;
  private MKTolerances _toleranceComparer;

  public double Volume { get; private set; }

  public XYZ Centroid { get; private set; }

  public Dictionary<Type, int> FaceTypeCounts { get; private set; }

  public int FaceCount { get; private set; }

  public TransformableSolid(Solid solid, MKTolerances toleranceComparer)
  {
    this._toleranceComparer = toleranceComparer;
    this.Centroid = solid.ComputeCentroid();
    this.Volume = solid.Volume;
    this._faces = new List<ComparableFace>();
    foreach (Face face in solid.Faces)
    {
      if (face.Area > 1E-05)
        this._faces.Add(new ComparableFace(face, toleranceComparer));
    }
    this.FaceTypeCounts = new Dictionary<Type, int>();
    this.FillFaceTypeCounts();
    this.FaceCount = this._faces.Count;
  }

  public bool FinishEquals(TransformableSolid other)
  {
    return this._toleranceComparer.NewValuesAreEqual(this.Volume, other.Volume, MKToleranceAspect.AddonVolume) && this._toleranceComparer.NewPointsAreEqual(this.Centroid, other.Centroid, MKToleranceAspect.Geometry) && this.CompareFaceTypeCounts(this.FaceTypeCounts, other.FaceTypeCounts) && this.CompareFaces(this.GetFaces(), other.GetFaces());
  }

  public bool Equals(TransformableSolid other)
  {
    return this._toleranceComparer.NewValuesAreEqual(this.Volume, other.Volume, MKToleranceAspect.Volume) && this._toleranceComparer.NewPointsAreEqual(this.Centroid, other.Centroid, MKToleranceAspect.Geometry) && this.CompareFaceTypeCounts(this.FaceTypeCounts, other.FaceTypeCounts) && this.CompareFaces(this.GetFaces(), other.GetFaces());
  }

  private List<ComparableFace> GetFaces() => this._faces.ToList<ComparableFace>();

  private bool CompareFaceTypeCounts(
    Dictionary<Type, int> faceTypes,
    Dictionary<Type, int> otherTypes)
  {
    foreach (Type key in faceTypes.Keys)
    {
      if (!otherTypes.ContainsKey(key) || faceTypes[key] != otherTypes[key])
        return false;
    }
    return true;
  }

  private bool CompareFaces(List<ComparableFace> standardFaces, List<ComparableFace> otherFaces)
  {
    if (standardFaces.Count != otherFaces.Count)
      return false;
    List<TransformableSolid.FaceMatchData> list1 = standardFaces.Select<ComparableFace, TransformableSolid.FaceMatchData>((Func<ComparableFace, TransformableSolid.FaceMatchData>) (s => new TransformableSolid.FaceMatchData()
    {
      Face = s,
      Matched = false
    })).ToList<TransformableSolid.FaceMatchData>();
    List<TransformableSolid.FaceMatchData> list2 = otherFaces.Select<ComparableFace, TransformableSolid.FaceMatchData>((Func<ComparableFace, TransformableSolid.FaceMatchData>) (s => new TransformableSolid.FaceMatchData()
    {
      Face = s,
      Matched = false
    })).ToList<TransformableSolid.FaceMatchData>();
    foreach (TransformableSolid.FaceMatchData faceMatchData1 in list1)
    {
      IEnumerable<TransformableSolid.FaceMatchData> faceMatchDatas = list2.Where<TransformableSolid.FaceMatchData>((Func<TransformableSolid.FaceMatchData, bool>) (s => !s.Matched));
      bool flag = false;
      foreach (TransformableSolid.FaceMatchData faceMatchData2 in faceMatchDatas)
      {
        if (faceMatchData1.Face.Equals(faceMatchData2.Face))
        {
          faceMatchData2.Matched = true;
          flag = true;
          break;
        }
      }
      if (!flag)
        return false;
    }
    return true;
  }

  public bool TransformSolid(Transform transform)
  {
    this.Centroid = transform.OfPoint(this.Centroid);
    foreach (ComparableFace face in this._faces)
      face.TransformFace(transform);
    return true;
  }

  private void FillFaceTypeCounts()
  {
    this.FaceTypeCounts.Clear();
    this.FaceTypeCounts.Add(typeof (ConicalFace), 0);
    this.FaceTypeCounts.Add(typeof (CylindricalFace), 0);
    this.FaceTypeCounts.Add(typeof (HermiteFace), 0);
    this.FaceTypeCounts.Add(typeof (PlanarFace), 0);
    this.FaceTypeCounts.Add(typeof (RevolvedFace), 0);
    this.FaceTypeCounts.Add(typeof (RuledFace), 0);
    foreach (ComparableFace face in this._faces)
    {
      int num = QA.SpecialPermission ? 1 : 0;
      this.FaceTypeCounts[face.FaceType]++;
    }
  }

  public void DrawSolidVisualization(Document revitDoc)
  {
    foreach (ComparableFace face in this._faces)
      face.DrawFaceVisualization(revitDoc);
  }

  private class FaceMatchData
  {
    public ComparableFace Face;
    public bool Matched;
  }
}
