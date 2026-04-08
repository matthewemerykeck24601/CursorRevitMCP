// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.GeometryComparer.GeometryVerificationFamily
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.AssemblyTools.MarkVerification.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Utils.MiscUtils;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification.GeometryComparer;

public class GeometryVerificationFamily
{
  public List<TransformableSolid> Solids = new List<TransformableSolid>();
  public List<EDGEAddonComponent> Addons = new List<EDGEAddonComponent>();
  public List<EDGEEmbedComponent> Embeds = new List<EDGEEmbedComponent>();
  public List<EDGEFinishComponent> Finishes = new List<EDGEFinishComponent>();
  public Dictionary<ElementId, List<TransformableSolid>> SolidsByFinish = new Dictionary<ElementId, List<TransformableSolid>>();
  public List<RotationWarning> listofWarnings = new List<RotationWarning>();
  public bool embedsExcludedByCountMultiplier;
  public Dictionary<string, List<ElementId>> embedsExcluded = new Dictionary<string, List<ElementId>>();
  private MarkVerificationData _MVData;
  public bool failed;
  private MKTolerances _toleranceComparer;
  private ComparisonOption _compareOption;
  private XYZ _SFCentroid;
  private Document _revitDoc;

  public string UniqueId { get; private set; }

  public GeometryVerificationFamily(
    FamilyInstance familyInstance,
    MarkVerificationData data,
    bool bVisualize = false)
  {
    this.UniqueId = familyInstance.UniqueId;
    this._revitDoc = familyInstance.Document;
    this._compareOption = data.CompareOption;
    this._MVData = data;
    this._toleranceComparer = new MKTolerances(this._compareOption, this._revitDoc);
    this.FillSolids(familyInstance);
    this.FillAddons(familyInstance);
    this.FillPlates(familyInstance);
    if (this.failed)
      return;
    this.FillFinishes(familyInstance);
    this.FillFinishSolids();
    TransformableSolid transformableSolid = this.Solids.OrderByDescending<TransformableSolid, double>((Func<TransformableSolid, double>) (s => s.Volume)).FirstOrDefault<TransformableSolid>();
    this._SFCentroid = transformableSolid != null ? transformableSolid.Centroid : new XYZ(0.0, 0.0, 0.0);
    this.TransformGeometry(familyInstance.GetTransform().Inverse);
    this.TransformGeometry(Transform.CreateTranslation(XYZ.Zero - this._SFCentroid));
    if (!bVisualize)
      return;
    this.DrawVisualization(familyInstance.Document);
  }

  public void FillPlates(FamilyInstance familyInstance)
  {
    Dictionary<string, List<ElementId>> embedsExcluded;
    this.Embeds = this._MVData.GetEmbedComponentDatas(familyInstance, out embedsExcluded);
    if (this.Embeds == null)
    {
      this.failed = true;
    }
    else
    {
      if (embedsExcluded.Count <= 0)
        return;
      this.embedsExcludedByCountMultiplier = true;
      foreach (string key in embedsExcluded.Keys)
      {
        if (this.embedsExcluded.ContainsKey(key))
        {
          foreach (ElementId elementId in embedsExcluded[key])
          {
            if (!this.embedsExcluded[key].Contains(elementId))
              this.embedsExcluded[key].Add(elementId);
          }
        }
        else
          this.embedsExcluded.Add(key, embedsExcluded[key]);
      }
    }
  }

  private void FillAddons(FamilyInstance hostInstance)
  {
    this.Addons = this._MVData.GetAddonsForHost(hostInstance);
  }

  protected void FillSolids(FamilyInstance familyInstance)
  {
    this.Solids = new List<TransformableSolid>();
    foreach (Solid instanceSolid in Utils.GeometryUtils.Solids.GetInstanceSolids((Element) familyInstance))
      this.Solids.Add(new TransformableSolid(instanceSolid, this._toleranceComparer));
  }

  private void FillFinishes(FamilyInstance familyInstance)
  {
    this.Finishes = this._MVData.GetFinishesForHost(familyInstance);
  }

  private void FillFinishSolids()
  {
    foreach (EDGEFinishComponent finish1 in this.Finishes)
    {
      Wall finish2 = finish1._finish;
      List<TransformableSolid> transformableSolidList = new List<TransformableSolid>();
      foreach (Solid instanceSolid in Utils.GeometryUtils.Solids.GetInstanceSolids((Element) finish2))
        transformableSolidList.Add(new TransformableSolid(instanceSolid, this._toleranceComparer));
      this.SolidsByFinish.Add(finish1.Id, transformableSolidList);
    }
  }

  private void TransformGeometry(Transform transform)
  {
    foreach (TransformableSolid solid in this.Solids)
      solid.TransformSolid(transform);
    foreach (EDGEAssemblyComponent_Base addon in this.Addons)
      addon.TransformLocation(transform);
    foreach (EDGEAssemblyComponent_Base embed in this.Embeds)
      embed.TransformLocation(transform);
    foreach (EDGEFinishComponent finish in this.Finishes)
    {
      finish.TransformLocation(transform);
      foreach (TransformableSolid transformableSolid in this.SolidsByFinish[finish.Id])
        transformableSolid.TransformSolid(transform);
    }
    this._SFCentroid = transform.OfPoint(this._SFCentroid);
  }

  public void Rotate90ForTest() => this.Rotate90ForTest(GeometryVerificationFamily.RotAxis.Z);

  private void Rotate90ForTest(GeometryVerificationFamily.RotAxis axis)
  {
    XYZ axis1 = XYZ.Zero;
    switch (axis)
    {
      case GeometryVerificationFamily.RotAxis.X:
        axis1 = XYZ.BasisX;
        break;
      case GeometryVerificationFamily.RotAxis.Y:
        axis1 = XYZ.BasisY;
        break;
      case GeometryVerificationFamily.RotAxis.Z:
        axis1 = XYZ.BasisZ;
        break;
    }
    this.TransformGeometry(Transform.CreateRotation(axis1, Math.PI / 2.0));
  }

  public static ElementId solidComparison(
    FamilyInstance standard,
    FamilyInstance incoming,
    UIDocument doc,
    AddInId uidoc)
  {
    Solid solid1 = (Solid) null;
    Solid solid2 = (Solid) null;
    List<Solid> instanceSolids1 = Utils.GeometryUtils.Solids.GetInstanceSolids((Element) incoming);
    List<Solid> instanceSolids2 = Utils.GeometryUtils.Solids.GetInstanceSolids((Element) standard);
    foreach (Solid solid1_1 in instanceSolids1)
      solid1 = !((GeometryObject) solid1 == (GeometryObject) null) ? BooleanOperationsUtils.ExecuteBooleanOperation(solid1, solid1_1, BooleanOperationsType.Union) : solid1_1;
    foreach (Solid solid1_2 in instanceSolids2)
      solid2 = !((GeometryObject) solid2 == (GeometryObject) null) ? BooleanOperationsUtils.ExecuteBooleanOperation(solid2, solid1_2, BooleanOperationsType.Union) : solid1_2;
    SolidComparison solidComparison1 = new SolidComparison(solid2, standard, (MKTolerances) null);
    SolidComparison solidComparison2 = new SolidComparison(solid1, incoming, (MKTolerances) null);
    List<Solid> source1 = new List<Solid>();
    Solid solid3 = (Solid) null;
    double num1 = double.MaxValue;
    for (double rad = Math.PI / 2.0; rad <= 2.0 * Math.PI; rad += Math.PI / 2.0)
    {
      XYZ centerPoint1 = solidComparison2.CenterPoint;
      XYZ centerPoint2 = solidComparison1.CenterPoint;
      bool flag = false;
      if (incoming.Mirrored && standard.Mirrored)
        flag = false;
      else if (incoming.Mirrored || standard.Mirrored)
        flag = true;
      Transform translation = Transform.CreateTranslation(!flag ? new XYZ(centerPoint2.X - centerPoint1.X, centerPoint2.Y - centerPoint1.Y, centerPoint2.Z - centerPoint1.Z) : new XYZ(centerPoint2.X - centerPoint1.X, -centerPoint2.Y - centerPoint1.Y, centerPoint2.Z - centerPoint1.Z));
      Solid transformed = SolidUtils.CreateTransformed(solidComparison2.zeroSolid, translation.Inverse);
      Solid subtractedSolid1;
      double me1 = solidComparison1.subtractSolid(transformed, out subtractedSolid1);
      Solid subtractedSolid2;
      double me2 = solidComparison2.subtractSolid(solidComparison1.zeroSolid, out subtractedSolid2, transformed);
      if (!((GeometryObject) subtractedSolid1 == (GeometryObject) null) || !((GeometryObject) subtractedSolid2 == (GeometryObject) null))
      {
        if (me1.ApproximatelyEquals(0.0) && me2.ApproximatelyEquals(0.0))
          return ElementId.InvalidElementId;
        try
        {
          solid3 = BooleanOperationsUtils.ExecuteBooleanOperation(subtractedSolid2, subtractedSolid1, BooleanOperationsType.Union);
          solid3 = SolidUtils.CreateTransformed(solid3, translation);
          if (solid3.Volume < num1)
          {
            num1 = solid3.Volume;
            source1.Add(solid3);
          }
        }
        catch (Exception ex)
        {
          continue;
        }
        solidComparison1.zeroSolid = solidComparison1.RotateSolid(XYZ.BasisZ, rad);
      }
    }
    List<Solid> source2;
    if (source1.Count == 0)
    {
      source2 = new List<Solid>();
    }
    else
    {
      bool flag = false;
      foreach (Solid solid4 in source1.OrderBy<Solid, double>((Func<Solid, double>) (e => e.Volume)).ToList<Solid>())
      {
        if (solid4.Volume != 0.0)
        {
          solid3 = solid4;
          flag = true;
          break;
        }
      }
      source2 = new List<Solid>();
      Transform transform = incoming.GetTransform();
      if (flag)
      {
        Solid transformed = SolidUtils.CreateTransformed(SolidUtils.CreateTransformed(solid3, Transform.CreateTranslation(solidComparison2.CenterPoint)), transform);
        source2.Add(transformed);
      }
    }
    DirectShape element = DirectShape.CreateElement(incoming.Document, new ElementId(BuiltInCategory.OST_Entourage));
    if (source2.Count > 0)
    {
      if (!element.IsValidGeometry(source2.First<Solid>()))
      {
        TessellatedShapeBuilder tessellatedShapeBuilder = new TessellatedShapeBuilder();
        tessellatedShapeBuilder.OpenConnectedFaceSet(true);
        foreach (Face face1 in source2.First<Solid>().Faces)
        {
          Mesh mesh = face1.Triangulate();
          for (int idx = 0; idx < mesh.NumTriangles; ++idx)
          {
            MeshTriangle meshTriangle = mesh.get_Triangle(idx);
            TessellatedFace face2 = new TessellatedFace((IList<XYZ>) new List<XYZ>()
            {
              meshTriangle.get_Vertex(0),
              meshTriangle.get_Vertex(1),
              meshTriangle.get_Vertex(2)
            }, ElementId.InvalidElementId);
            if (tessellatedShapeBuilder.DoesFaceHaveEnoughLoopsAndVertices(face2))
              tessellatedShapeBuilder.AddFace(face2);
          }
        }
        tessellatedShapeBuilder.CloseConnectedFaceSet();
        tessellatedShapeBuilder.Target = TessellatedShapeBuilderTarget.AnyGeometry;
        tessellatedShapeBuilder.Fallback = TessellatedShapeBuilderFallback.Mesh;
        tessellatedShapeBuilder.Build();
        element.SetShape(tessellatedShapeBuilder.GetBuildResult().GetGeometricalObjects());
      }
      else
        element.SetShape((IList<GeometryObject>) source2.Cast<GeometryObject>().ToList<GeometryObject>());
    }
    else if (source2.Count == 0)
    {
      int num2 = (int) MessageBox.Show("The solid geometry for analysis could not be generated.");
      return (ElementId) null;
    }
    return element.Id;
  }

  public void DrawVisualization(Document revitDoc)
  {
    foreach (TransformableSolid solid in this.Solids)
      solid.DrawSolidVisualization(revitDoc);
  }

  public bool AddOnLocationMatches(
    GeometryVerificationFamily other,
    out LocationCompareResult locationComparisonResult)
  {
    locationComparisonResult = LocationCompareResult.Failed;
    if (this.Addons.Count != other.Addons.Count)
      return false;
    for (int index = 0; index < 4; ++index)
    {
      locationComparisonResult = this.CompareAddonLocations(other);
      if (locationComparisonResult != LocationCompareResult.Failed)
        return true;
      other.Rotate90ForTest();
    }
    locationComparisonResult = LocationCompareResult.Failed;
    return false;
  }

  public bool EmbedLocationMatches(
    GeometryVerificationFamily other,
    out LocationCompareResult locationComparisonResult,
    out Dictionary<string, List<ElementId>> platesWarning,
    bool traditional)
  {
    Dictionary<string, List<ElementId>> dictionary = new Dictionary<string, List<ElementId>>();
    locationComparisonResult = LocationCompareResult.Failed;
    if (this.Embeds.Count != other.Embeds.Count)
    {
      platesWarning = (Dictionary<string, List<ElementId>>) null;
      return false;
    }
    for (int index = 0; index < 4; ++index)
    {
      locationComparisonResult = this.CompareEmbedLocations(other);
      if (locationComparisonResult == LocationCompareResult.Failed)
      {
        other.Rotate90ForTest();
      }
      else
      {
        if (this.listofWarnings.Count<RotationWarning>() > 0)
        {
          foreach (RotationWarning listofWarning in this.listofWarnings)
          {
            foreach (string key in listofWarning.Warnings.Keys)
            {
              if (dictionary.ContainsKey(key))
              {
                foreach (ElementId elementId in listofWarning.Warnings[key])
                {
                  if (!dictionary[key].Contains(elementId))
                    dictionary[key].Add(elementId);
                }
              }
              else
              {
                foreach (ElementId elementId in listofWarning.Warnings[key])
                {
                  if (dictionary.ContainsKey(key))
                    dictionary[key].Add(elementId);
                  else
                    dictionary.Add(key, new List<ElementId>()
                    {
                      elementId
                    });
                }
              }
            }
          }
        }
        platesWarning = dictionary;
        return true;
      }
    }
    platesWarning = (Dictionary<string, List<ElementId>>) null;
    locationComparisonResult = LocationCompareResult.Failed;
    return false;
  }

  public bool FinishMatches(
    GeometryVerificationFamily other,
    out LocationCompareResult locationComparisonResult)
  {
    locationComparisonResult = LocationCompareResult.Failed;
    if (this.Finishes.Count != other.Finishes.Count)
      return false;
    for (int index = 0; index < 4; ++index)
    {
      MVEfailedResults mvEfailedResults = new MVEfailedResults();
      if (!this.CompareFinishes(other))
      {
        locationComparisonResult = LocationCompareResult.Failed;
        other.Rotate90ForTest();
      }
      else
      {
        locationComparisonResult = LocationCompareResult.Success;
        return true;
      }
    }
    locationComparisonResult = LocationCompareResult.Failed;
    return false;
  }

  public bool SolidMatches(
    GeometryVerificationFamily other,
    out LocationCompareResult locationComparisonResult,
    bool traditional)
  {
    locationComparisonResult = LocationCompareResult.Failed;
    if (traditional && this.Solids.Count != other.Solids.Count)
      return false;
    for (int index = 0; index < 4; ++index)
    {
      MVEfailedResults mvEfailedResults = new MVEfailedResults();
      if (!this.CompareSolids(other))
      {
        locationComparisonResult = LocationCompareResult.Failed;
        other.Rotate90ForTest();
      }
      else
      {
        locationComparisonResult = LocationCompareResult.Success;
        return true;
      }
    }
    locationComparisonResult = LocationCompareResult.Failed;
    return false;
  }

  public bool Matches(
    GeometryVerificationFamily other,
    out LocationCompareResult locationComparisonResult,
    bool addonLocation,
    bool embededLocation,
    bool solid,
    bool finishLocation)
  {
    locationComparisonResult = LocationCompareResult.Failed;
    if (this.Solids.Count != other.Solids.Count || this.Addons.Count != other.Addons.Count)
      return false;
    for (int index = 0; index < 4; ++index)
    {
      bool flag = true;
      LocationCompareResult locationCompareResult1 = LocationCompareResult.Failed;
      if (addonLocation)
      {
        locationCompareResult1 = this.CompareAddonLocations(other);
        if (flag && locationCompareResult1 == LocationCompareResult.Failed)
          flag = false;
      }
      LocationCompareResult locationCompareResult2 = LocationCompareResult.Failed;
      if (embededLocation)
      {
        locationCompareResult2 = this.CompareEmbedLocations(other);
        if (flag && locationCompareResult2 == LocationCompareResult.Failed)
          flag = false;
      }
      if (solid && flag && !this.CompareSolids(other))
        flag = false;
      if (finishLocation && flag && !this.CompareFinishes(other))
        flag = false;
      if (flag)
      {
        locationComparisonResult = locationCompareResult1 == LocationCompareResult.PlateRotationWarning || locationCompareResult2 == LocationCompareResult.PlateRotationWarning ? LocationCompareResult.PlateRotationWarning : LocationCompareResult.Success;
        return true;
      }
      locationComparisonResult = LocationCompareResult.Failed;
      other.Rotate90ForTest();
    }
    locationComparisonResult = LocationCompareResult.Failed;
    return false;
  }

  private LocationCompareResult CompareAddonLocations(GeometryVerificationFamily other)
  {
    return this.CompareGeometricLocations(((IEnumerable<EDGEAssemblyComponent_Base>) this.Addons).ToList<EDGEAssemblyComponent_Base>(), ((IEnumerable<EDGEAssemblyComponent_Base>) other.Addons).ToList<EDGEAssemblyComponent_Base>());
  }

  private LocationCompareResult CompareEmbedLocations(GeometryVerificationFamily other)
  {
    return this.CompareGeometricLocations(((IEnumerable<EDGEAssemblyComponent_Base>) this.Embeds).ToList<EDGEAssemblyComponent_Base>(), ((IEnumerable<EDGEAssemblyComponent_Base>) other.Embeds).ToList<EDGEAssemblyComponent_Base>());
  }

  private LocationCompareResult CompareGeometricLocations(
    List<EDGEAssemblyComponent_Base> standardComponents,
    List<EDGEAssemblyComponent_Base> otherComponents)
  {
    bool flag1 = false;
    StringBuilder stringBuilder = new StringBuilder();
    string str1 = "";
    RotationWarning rotationWarning = new RotationWarning();
    List<string> list1 = standardComponents.Select<EDGEAssemblyComponent_Base, string>((Func<EDGEAssemblyComponent_Base, string>) (s => s.FamilyTypeName)).Distinct<string>().ToList<string>();
    Dictionary<string, List<ElementId>> dictionary = new Dictionary<string, List<ElementId>>();
    foreach (string str2 in list1)
    {
      string key = str2;
      List<EDGEAssemblyComponent_Base> list2 = standardComponents.Where<EDGEAssemblyComponent_Base>((Func<EDGEAssemblyComponent_Base, bool>) (s => s.FamilyTypeName == key)).ToList<EDGEAssemblyComponent_Base>();
      List<EDGEAssemblyComponent_Base> list3 = otherComponents.Where<EDGEAssemblyComponent_Base>((Func<EDGEAssemblyComponent_Base, bool>) (s => s.FamilyTypeName == key)).ToList<EDGEAssemblyComponent_Base>();
      foreach (EDGEAssemblyComponent_Base assemblyComponentBase1 in list2)
      {
        int index = 0;
        bool flag2 = false;
        foreach (EDGEAssemblyComponent_Base other in list3)
        {
          LocationCompareResult locationCompareResult = assemblyComponentBase1.Matches(other);
          switch (locationCompareResult)
          {
            case LocationCompareResult.PlateRotationWarning:
            case LocationCompareResult.Success:
              if (locationCompareResult == LocationCompareResult.PlateRotationWarning)
              {
                flag1 = true;
                if (other.GetType().Name.Equals("EDGEEmbedComponent"))
                {
                  foreach (EDGEAssemblyComponent_Base assemblyComponentBase2 in list3)
                  {
                    string familyName = assemblyComponentBase2.inst.Symbol.FamilyName;
                    string name = assemblyComponentBase2.inst.Symbol.Name;
                    string key1 = !familyName.Equals(name) ? $"{familyName} - {name}" : familyName;
                    if (assemblyComponentBase2.FamilyTypeName.Equals(key))
                    {
                      if (dictionary.ContainsKey(key1))
                        dictionary[key1].Add(assemblyComponentBase2.Id);
                      else
                        dictionary.Add(key1, new List<ElementId>()
                        {
                          assemblyComponentBase2.Id
                        });
                    }
                  }
                  foreach (EDGEAssemblyComponent_Base assemblyComponentBase3 in list2)
                  {
                    string familyName = assemblyComponentBase3.inst.Symbol.FamilyName;
                    string name = assemblyComponentBase3.inst.Symbol.Name;
                    string key2 = !familyName.Equals(name) ? $"{familyName} - {name}" : familyName;
                    if (assemblyComponentBase3.FamilyTypeName.Equals(key))
                    {
                      if (dictionary.ContainsKey(key2))
                        dictionary[key2].Add(assemblyComponentBase3.Id);
                      else
                        dictionary.Add(key2, new List<ElementId>()
                        {
                          assemblyComponentBase3.Id
                        });
                    }
                  }
                }
              }
              list3.RemoveAt(index);
              flag2 = true;
              goto label_29;
            default:
              ++index;
              continue;
          }
        }
label_29:
        if (!flag2)
        {
          str1 += "Unable to find matching addon locations";
          return LocationCompareResult.Failed;
        }
      }
    }
    if (dictionary.Keys.Count<string>() > 0)
    {
      rotationWarning.Warnings = dictionary;
      this.listofWarnings.Add(rotationWarning);
    }
    return !flag1 ? LocationCompareResult.Success : LocationCompareResult.PlateRotationWarning;
  }

  private bool CompareSolids(GeometryVerificationFamily other)
  {
    if (this.Solids.Count != other.Solids.Count)
      return false;
    bool flag1 = true;
    List<TransformableSolid> list1 = this.Solids.OrderByDescending<TransformableSolid, double>((Func<TransformableSolid, double>) (s => s.Volume)).ToList<TransformableSolid>();
    List<TransformableSolid> list2 = other.Solids.OrderByDescending<TransformableSolid, double>((Func<TransformableSolid, double>) (s => s.Volume)).ToList<TransformableSolid>();
    List<TransformableSolid> transformableSolidList1 = new List<TransformableSolid>();
    List<TransformableSolid> transformableSolidList2 = new List<TransformableSolid>();
    bool flag2 = false;
    foreach (TransformableSolid transformableSolid in list1)
    {
      foreach (TransformableSolid other1 in list2)
      {
        if (!transformableSolidList1.Contains(transformableSolid) && !transformableSolidList2.Contains(other1) && transformableSolid.Equals(other1))
        {
          transformableSolidList1.Add(transformableSolid);
          transformableSolidList2.Add(other1);
          flag2 = true;
          list2.Remove(other1);
          break;
        }
      }
      if (!flag2)
      {
        flag1 = false;
        break;
      }
    }
    return flag1;
  }

  private bool CompareFinishes(GeometryVerificationFamily other)
  {
    if (this.Finishes.Count != other.Finishes.Count)
      return false;
    foreach (string str in this.Finishes.Select<EDGEFinishComponent, string>((Func<EDGEFinishComponent, string>) (s => s.FamilyTypeName)).Distinct<string>())
    {
      string key = str;
      List<EDGEFinishComponent> list1 = this.Finishes.Where<EDGEFinishComponent>((Func<EDGEFinishComponent, bool>) (s => s.FamilyTypeName == key)).ToList<EDGEFinishComponent>();
      List<EDGEFinishComponent> list2 = other.Finishes.Where<EDGEFinishComponent>((Func<EDGEFinishComponent, bool>) (s => s.FamilyTypeName == key)).ToList<EDGEFinishComponent>();
      foreach (EDGEFinishComponent edgeFinishComponent in list1)
      {
        bool flag = false;
        using (List<EDGEFinishComponent>.Enumerator enumerator1 = list2.GetEnumerator())
        {
label_12:
          while (enumerator1.MoveNext())
          {
            EDGEFinishComponent current = enumerator1.Current;
            IEnumerable<TransformableSolid> transformableSolids = (IEnumerable<TransformableSolid>) this.SolidsByFinish[edgeFinishComponent.Id].OrderByDescending<TransformableSolid, double>((Func<TransformableSolid, double>) (i => i.Volume));
            IOrderedEnumerable<TransformableSolid> orderedEnumerable = other.SolidsByFinish[current.Id].OrderByDescending<TransformableSolid, double>((Func<TransformableSolid, double>) (i => i.Volume));
            IEnumerator<TransformableSolid> enumerator2 = transformableSolids.GetEnumerator();
            IEnumerator<TransformableSolid> enumerator3 = orderedEnumerable.GetEnumerator();
            do
            {
              if (!enumerator2.MoveNext() || !enumerator3.MoveNext())
                goto label_12;
            }
            while (!enumerator2.Current.FinishEquals(enumerator3.Current));
            flag = true;
          }
        }
        if (!flag)
          return false;
      }
    }
    return true;
  }

  public enum RotAxis
  {
    X,
    Y,
    Z,
  }
}
