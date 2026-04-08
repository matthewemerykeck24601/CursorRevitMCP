// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.CopyInsulation.CopyInsulationFamily
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.ElementUtils;
using Utils.MiscUtils;

#nullable disable
namespace EDGE.InsulationTools.CopyInsulation;

public class CopyInsulationFamily
{
  private FamilyInstance _familyInstance;
  private XYZ sourceLocation;
  private Transform _sourceTransform;
  private Transform _targetTransform;

  public CopyInsulationFamily(FamilyInstance famInst, Transform source, Transform target)
  {
    this._familyInstance = famInst;
    this._sourceTransform = source;
    this._targetTransform = target;
    if (famInst == null || !(famInst.Location is LocationPoint location) || location.Point == null)
      return;
    this.sourceLocation = location.Point;
  }

  public FamilyInstance Copy(Reference frontFaceReference)
  {
    if (frontFaceReference == null)
      return (FamilyInstance) null;
    XYZ targetLocation = this.calculateTargetLocation();
    if (targetLocation == null)
      return (FamilyInstance) null;
    if (this._familyInstance == null)
      return (FamilyInstance) null;
    Transform transform = this._familyInstance.GetTransform();
    if (transform == null)
      return (FamilyInstance) null;
    XYZ basisX = transform.BasisX;
    FamilySymbol symbol = this._familyInstance.Symbol;
    if (symbol == null)
      return (FamilyInstance) null;
    if (!symbol.IsActive)
      symbol.Activate();
    FamilyInstance elem = this._familyInstance.Document.Create.NewFamilyInstance(frontFaceReference, XYZ.Zero, basisX, symbol);
    if (elem == null)
      return (FamilyInstance) null;
    ElementTransformUtils.MoveElement(elem.Document, elem.Id, targetLocation);
    if (this._familyInstance.HostFace != null)
    {
      Parameter parameter1 = Parameters.LookupParameter((Element) this._familyInstance, "Offset from Host");
      if (parameter1 == null || !parameter1.HasValue)
        return elem;
      double num = parameter1.AsDouble();
      Parameter parameter2 = Parameters.LookupParameter((Element) elem, "Offset from Host");
      if (parameter2 == null)
        return elem;
      parameter2.Set(num);
    }
    else
    {
      Parameter parameter = Parameters.LookupParameter((Element) elem, "Offset from Host");
      if (parameter == null)
        return elem;
      parameter.Set(0);
    }
    return elem;
  }

  public FamilyInstance Copy()
  {
    XYZ targetLocation = this.calculateTargetLocation();
    if (targetLocation == null)
      return (FamilyInstance) null;
    if (this._familyInstance == null)
      return (FamilyInstance) null;
    Transform transform1 = this._familyInstance.GetTransform();
    if (transform1 == null)
      return (FamilyInstance) null;
    FamilySymbol symbol = this._familyInstance.Symbol;
    if (symbol == null)
      return (FamilyInstance) null;
    if (!symbol.IsActive)
      symbol.Activate();
    this.calculatNewTransform(transform1, transform1).Origin = targetLocation;
    Transform transform2 = this._sourceTransform.Multiply(this._sourceTransform.Inverse);
    CopyPasteOptions copyPasteOptions = new CopyPasteOptions();
    Document document1 = this._familyInstance.Document;
    List<ElementId> elementsToCopy = new List<ElementId>();
    elementsToCopy.Add(this._familyInstance.Id);
    Document document2 = this._familyInstance.Document;
    Transform transform3 = transform2;
    CopyPasteOptions options = copyPasteOptions;
    FamilyInstance element = this._familyInstance.Document.GetElement(ElementTransformUtils.CopyElements(document1, (ICollection<ElementId>) elementsToCopy, document2, transform3, options).First<ElementId>()) as FamilyInstance;
    if (!(element.Location is LocationPoint location))
      return (FamilyInstance) null;
    XYZ point = location.Point;
    if (!point.IsAlmostEqualTo(targetLocation))
      ElementTransformUtils.MoveElement(element.Document, element.Id, targetLocation - point);
    return element;
  }

  private XYZ calculateTargetLocation()
  {
    return this.sourceLocation == null || this._sourceTransform == null || this._targetTransform == null ? (XYZ) null : this._targetTransform.OfPoint(this._sourceTransform.Inverse.OfPoint(this.sourceLocation));
  }

  private Transform calculatNewTransform(Transform sourceTransform, Transform TargetTransform)
  {
    Transform transform = new Transform(TargetTransform);
    XYZ xyz1 = this.transformVector(sourceTransform.BasisX);
    if (xyz1 == null)
      return (Transform) null;
    XYZ xyz2 = this.transformVector(sourceTransform.BasisY);
    if (xyz2 == null)
      return (Transform) null;
    XYZ xyz3 = this.transformVector(sourceTransform.BasisZ);
    if (xyz3 == null)
      return (Transform) null;
    transform.BasisX = xyz1;
    transform.BasisY = xyz2;
    transform.BasisZ = xyz3;
    return transform;
  }

  private XYZ transformVector(XYZ source)
  {
    return source == null || this._sourceTransform == null || this._targetTransform == null ? (XYZ) null : this._targetTransform.OfVector(this._sourceTransform.Inverse.OfVector(source));
  }

  private double CalculateAngleBetweenVectors(XYZ v1, XYZ v2, XYZ axisOfRotation)
  {
    XYZ perpendicularComponent1 = this.GetPerpendicularComponent(v1, axisOfRotation);
    XYZ perpendicularComponent2 = this.GetPerpendicularComponent(v2, axisOfRotation);
    double num1 = perpendicularComponent1.DotProduct(v2);
    double num2 = this.VectorMagnitude(perpendicularComponent1);
    double num3 = this.VectorMagnitude(perpendicularComponent2);
    return num2 == 0.0 || num3 == 0.0 ? 0.0 : Math.Acos(num1 / (num2 * num3));
  }

  private double VectorMagnitude(XYZ vector)
  {
    return Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
  }

  private XYZ GetPerpendicularComponent(XYZ vector, XYZ axisOfRotation)
  {
    XYZ xyz = vector.DotProduct(axisOfRotation) * axisOfRotation;
    return vector - xyz;
  }

  private Transform RotateFamilyInstance(
    FamilyInstance targetFamilyInstance,
    XYZ vector1,
    XYZ vector2,
    XYZ axisOfRotation,
    XYZ Location)
  {
    Line bound = Line.CreateBound(Location, Location + 10.0 * axisOfRotation);
    double angleBetweenVectors = this.CalculateAngleBetweenVectors(vector1, vector2, axisOfRotation);
    if (angleBetweenVectors.ApproximatelyEquals(0.0, 0.0001) || angleBetweenVectors.ApproximatelyEquals(2.0 * Math.PI, 0.0001))
      return (Transform) null;
    ElementTransformUtils.RotateElement(targetFamilyInstance.Document, targetFamilyInstance.Id, bound, -angleBetweenVectors);
    return targetFamilyInstance.GetTransform();
  }
}
