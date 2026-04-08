// Decompiled with JetBrains decompiler
// Type: EDGE.GeometryTools.NewReferencePoint
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils.AssemblyUtils;
using Utils.ElementUtils;
using Utils.SelectionUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.GeometryTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class NewReferencePoint : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    try
    {
      UIApplication application = commandData.Application;
      UIDocument activeUiDocument = application.ActiveUIDocument;
      ActiveModel.GetInformation(activeUiDocument);
      Document document = activeUiDocument.Document;
      View activeView1 = document.ActiveView;
      if (document.IsFamilyDocument)
      {
        new TaskDialog("Family Editor")
        {
          AllowCancellation = false,
          CommonButtons = ((TaskDialogCommonButtons) 1),
          MainInstruction = "New Reference Point must be run in the Project Environment",
          MainContent = "You are currently in the family editor, New Reference Point must be run in the project environment.  Please return to the project environment or open a project before running this tool.",
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
        }.Show();
        return (Result) 1;
      }
      if (activeView1.ViewType.Equals((object) ViewType.Schedule))
      {
        TaskDialog.Show("Invalid View", "New Reference Point cannot be used in Schedule View.");
        return (Result) 1;
      }
      if (activeView1.ViewType.Equals((object) ViewType.ProjectBrowser))
      {
        TaskDialog.Show("Invalid View", "New Reference Point cannot be used in the Project Browser.");
        return (Result) 1;
      }
      if (activeView1.ViewType.Equals((object) ViewType.Elevation) || activeView1.ViewType.Equals((object) ViewType.Legend) || activeView1.ViewType.Equals((object) ViewType.DraftingView) || activeView1.ViewType.Equals((object) ViewType.Section) || activeView1.ViewType.Equals((object) ViewType.DrawingSheet) || activeView1.ViewType.Equals((object) ViewType.Detail) || activeView1.ViewType.Equals((object) ViewType.FloorPlan) || activeView1.ViewType.Equals((object) ViewType.EngineeringPlan))
      {
        this.NewReferencePoint2D(document, activeUiDocument, application);
        return (Result) 0;
      }
      Plane plane = document.ActiveView.SketchPlane.GetPlane();
      View3D activeView2 = document.ActiveView as View3D;
      XYZ upDirection = plane.YVec.DotProduct(XYZ.BasisZ) < 0.0 ? new XYZ(-plane.YVec.X, -plane.YVec.Y, -plane.YVec.Z) : new XYZ(plane.YVec.X, plane.YVec.Y, plane.YVec.Z);
      ICollection<ElementId> elementIds = activeUiDocument.Selection.GetElementIds();
      Autodesk.Revit.UI.Selection.Selection selection = application.ActiveUIDocument.Selection;
      if (elementIds.Count == 0)
      {
        ICollection<ElementId> source = References.PickNewReferences("Please select elements to move.");
        if (source == null)
          return (Result) 1;
        elementIds = (ICollection<ElementId>) source.Select<ElementId, Element>(new Func<ElementId, Element>(document.GetElement)).SelectMany<Element, ElementId>((Func<Element, IEnumerable<ElementId>>) (elems => new List<ElementId>()
        {
          elems.Id
        }.Concat<ElementId>(elems.GetSubComponentIds()))).ToList<ElementId>();
      }
      if (elementIds.Count == 0)
        return (Result) 1;
      foreach (ElementId id in (IEnumerable<ElementId>) elementIds)
      {
        Element element = document.GetElement(id);
        int integerValue = element.Category.Id.IntegerValue;
        if (integerValue.Equals(-2000260))
        {
          TaskDialog.Show("Invalid Element", "Dimension Lines are invalid elements for New Reference Point. ");
          return (Result) 1;
        }
        integerValue = element.Category.Id.IntegerValue;
        if (integerValue.Equals(-2000264))
        {
          TaskDialog.Show("Invalid Element", "Spot Coordinates are invalid elements for New Reference Point.");
          return (Result) 1;
        }
        integerValue = element.Category.Id.IntegerValue;
        if (integerValue.Equals(-2000263))
        {
          TaskDialog.Show("Invalid Element", "Spot Elevations are invalid elements for New Reference Point.");
          return (Result) 1;
        }
        integerValue = element.Category.Id.IntegerValue;
        if (integerValue.Equals(-2000265))
        {
          TaskDialog.Show("Invalid Element", "Spot Slopes are invalid elements for New Reference Point.");
          return (Result) 1;
        }
      }
      ICollection<ElementId> elementId = elementIds;
      foreach (ElementId id in elementIds.ToList<ElementId>())
      {
        Element element1 = document.GetElement(id);
        if (element1 is AssemblyInstance)
        {
          AssemblyInstance assemblyInstance = element1 as AssemblyInstance;
          elementId.Remove(id);
          foreach (ElementId memberId in (IEnumerable<ElementId>) assemblyInstance.GetMemberIds())
          {
            bool flag = false;
            Element element2 = document.GetElement(memberId);
            if (element2 is FamilyInstance)
            {
              FamilyInstance famInst = element2 as FamilyInstance;
              this.CheckAssemblies(famInst, assemblyInstance.GetMemberIds());
              if (!this.CheckAssemblies(famInst, assemblyInstance.GetMemberIds()))
                continue;
            }
            foreach (object obj in (IEnumerable<ElementId>) elementId)
            {
              if (obj.Equals((object) memberId))
              {
                flag = true;
                break;
              }
            }
            if (!flag)
              elementId.Add(memberId);
          }
        }
      }
      XYZ elemIdCenter = this.GetElemIdCenter(elementId, document);
      List<UIView> list = activeUiDocument.GetOpenUIViews().ToList<UIView>();
      UIView uiView1 = (UIView) null;
      foreach (UIView uiView2 in list)
      {
        if (uiView2.ViewId.Equals((object) activeView2.Id))
          uiView1 = uiView2;
      }
      ViewOrientation3D newViewOrientation3D = new ViewOrientation3D(elemIdCenter + plane.Normal * 2.0, upDirection, -plane.Normal);
      activeView2.GetOrientation();
      if (activeView2.IsLocked)
      {
        TaskDialog.Show("Warning", "New Reference Point cannot be run while the 3d View is Locked");
        return (Result) 1;
      }
      if (!activeView2.IsLocked)
      {
        activeView2.SetOrientation(newViewOrientation3D);
        BoundingBoxXYZ boundingBox = this.GetBoundingBox(elementId, document);
        uiView1.ZoomAndCenterRectangle(boundingBox.Max, boundingBox.Min);
      }
      XYZ axisX = new XYZ(activeView2.RightDirection.X, activeView2.RightDirection.Y, activeView2.RightDirection.Z);
      XYZ axisY = new XYZ(activeView2.UpDirection.X, activeView2.UpDirection.Y, activeView2.UpDirection.Z);
      XYZ axisZ = new XYZ(activeView2.ViewDirection.X, activeView2.ViewDirection.Y, activeView2.ViewDirection.Z);
      List<string> stringList = new List<string>();
      foreach (ElementId id in (IEnumerable<ElementId>) elementIds)
      {
        Element element = document.GetElement(id);
        if (element != null && element.Pinned)
        {
          string name = (element as FamilyInstance).Symbol.Family.Name;
          stringList.Add($"{id?.ToString()} Family Name: {name} Control Mark: {element.GetControlMark()}");
        }
      }
      if (stringList.Count > 0)
      {
        StringBuilder stringBuilder = new StringBuilder();
        TaskDialog taskDialog = new TaskDialog("Pinned Elements Found");
        taskDialog.MainContent = "Pinned Elements Found";
        taskDialog.MainInstruction = "The following Pinned Elements were found in your selection group. Unpin the elements or deselect them in order to run New Reference Point. ";
        foreach (string str in stringList)
          stringBuilder.AppendLine(str.ToString());
        taskDialog.ExpandedContent = stringBuilder.ToString();
        taskDialog.Show();
        return (Result) 1;
      }
      Element elem = elementIds.ToList<ElementId>().Count != 1 ? document.GetElement(selection.PickObject((ObjectType) 1, (ISelectionFilter) new OnlySelectedElems(activeUiDocument, elementIds.ToList<ElementId>()), "Please select the element from selection group that will be your Z Axis reference.")) : document.GetElement(elementIds.ToList<ElementId>().First<ElementId>());
      XYZ xyz1 = selection.PickPoint("Pick starting reference point.");
      XYZ xyz2 = selection.PickPoint("Pick new reference point.");
      string planeName = "";
      if (document.ActiveView.SketchPlane != null)
        planeName = "Current Work Plane: " + document.ActiveView.SketchPlane.Name.ToString();
      bool imperialUnits = true;
      if (document.GetUnits().GetFormatOptions(SpecTypeId.Length).GetUnitTypeId().TypeId.ToUpper().Contains("METERS"))
        imperialUnits = false;
      NewRefPointForm newRefPointForm = new NewRefPointForm(false, planeName, imperialUnits);
      int num1 = (int) newRefPointForm.ShowDialog();
      bool xNullFlag = newRefPointForm.xNullFlag;
      bool yNullFlag = newRefPointForm.yNullFlag;
      bool zNullFlag = newRefPointForm.zNullFlag;
      XYZ xyz3 = new XYZ(newRefPointForm.newXYZ.X, newRefPointForm.newXYZ.Y, newRefPointForm.newXYZ.Z);
      XYZ offsetXYZ = new XYZ(xyz2.X - xyz1.X, xyz2.Y - xyz1.Y, xyz2.Z - xyz1.Z);
      using (Transaction transaction = new Transaction(document, "New Reference Point"))
      {
        int num2 = (int) transaction.Start();
        Transform translation1 = Transform.CreateTranslation(XYZ.Zero);
        translation1.BasisX = axisX;
        translation1.BasisY = axisY;
        translation1.BasisZ = axisZ;
        XYZ xyz4 = NewReferencePoint.offsetTransform(offsetXYZ, axisX, axisY, axisZ, true);
        XYZ referenceZ = this.GetReferenceZ(elem, plane, document, axisX, axisY, axisZ);
        if (!xNullFlag)
        {
          XYZ translation2 = translation1.OfPoint(new XYZ(xyz4.X, 0.0, 0.0)) + NewReferencePoint.offsetTransform(new XYZ(xyz3.X, 0.0, 0.0), axisX, axisY, axisZ);
          ElementTransformUtils.MoveElements(document, elementIds, translation2);
        }
        if (!yNullFlag)
        {
          XYZ translation3 = translation1.OfPoint(new XYZ(0.0, xyz4.Y, 0.0)) + NewReferencePoint.offsetTransform(new XYZ(0.0, xyz3.Y, 0.0), axisX, axisY, axisZ);
          ElementTransformUtils.MoveElements(document, elementIds, translation3);
        }
        if (!zNullFlag)
        {
          XYZ xyz5 = new XYZ(0.0, 0.0, referenceZ.Z);
          XYZ translation4 = NewReferencePoint.offsetTransform(new XYZ(0.0, 0.0, xyz3.Z - xyz5.Z), axisX, axisY, axisZ);
          ElementTransformUtils.MoveElements(document, elementIds, translation4);
        }
        int num3 = (int) transaction.Commit();
      }
    }
    catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
    {
      return (Result) 1;
    }
    catch (Exception ex)
    {
      message = ex.Message;
      return (Result) -1;
    }
    return (Result) 0;
  }

  private XYZ GetReferenceZ(
    Element elem,
    Plane plane,
    Document revitDoc,
    XYZ axisX,
    XYZ axisY,
    XYZ axisZ)
  {
    double z = plane.Origin.Z;
    Transform translation = Transform.CreateTranslation(plane.Origin);
    translation.BasisX = axisX;
    translation.BasisY = axisY;
    translation.BasisZ = axisZ;
    XYZ xyz1 = (XYZ) null;
    Element element = !(elem is AssemblyInstance) ? elem : (elem as AssemblyInstance).GetStructuralFramingElement();
    if (element.Location is LocationPoint)
    {
      XYZ xyz2 = new XYZ();
      XYZ point = !(element is FamilyInstance) ? (!(element.Location is LocationPoint) ? translation.Origin : (element.Location as LocationPoint).Point) : (element as FamilyInstance).GetTransform().Origin;
      XYZ xyz3 = translation.Inverse.OfPoint(point);
      xyz1 = new XYZ(xyz3.X, xyz3.Y, xyz3.Z);
    }
    return xyz1 ?? new XYZ(0.0, 0.0, 0.0);
  }

  private XYZ GetPoint(View3D view, UIDocument uidoc, string instruction = "")
  {
    XYZ xyz = uidoc.Selection.PickPoint(instruction);
    ViewOrientation3D orientation = view.GetOrientation();
    XYZ eyePosition = orientation.EyePosition;
    XYZ forwardDirection = orientation.ForwardDirection;
    double num1 = xyz.DotProduct(forwardDirection);
    double num2 = eyePosition.DotProduct(forwardDirection);
    XYZ origin = xyz + forwardDirection.Negate() * (num1 - num2);
    Line unbound = Line.CreateUnbound(origin, forwardDirection);
    ReferenceWithContext nearest = new ReferenceIntersector(view).FindNearest(origin, unbound.Direction);
    XYZ point = xyz;
    if (nearest != null)
      point = nearest.GetReference().GlobalPoint;
    return point;
  }

  private Result NewReferencePoint2D(Document doc, UIDocument uiDoc, UIApplication uiApp)
  {
    using (Transaction transaction = new Transaction(doc, "New Reference Point"))
    {
      int num1 = (int) transaction.Start();
      ICollection<ElementId> elementIds = uiDoc.Selection.GetElementIds();
      bool noZ = false;
      Plane plane = (Plane) null;
      string planeName = "";
      if (doc.ActiveView.ViewType.Equals((object) ViewType.Elevation) || doc.ActiveView.ViewType.Equals((object) ViewType.Section) || doc.ActiveView.ViewType.Equals((object) ViewType.Detail))
      {
        if (doc.ActiveView.Title.Contains("Detail View: Elevation"))
        {
          noZ = true;
        }
        else
        {
          if (doc.ActiveView.SketchPlane == null)
          {
            TaskDialog.Show("Please Select Valid Workplane", "Please select a valid workplane whose normal is parallel to the view direction and try running New Reference Point again.");
            return (Result) 1;
          }
          if (doc.ActiveView.SketchPlane.GetPlane().Normal.IsAlmostEqualTo(doc.ActiveView.ViewDirection.Negate()) || doc.ActiveView.SketchPlane.GetPlane().Normal.IsAlmostEqualTo(doc.ActiveView.ViewDirection))
          {
            plane = Plane.CreateByNormalAndOrigin(doc.ActiveView.ViewDirection, doc.ActiveView.SketchPlane.GetPlane().Origin);
            planeName = "Current Work Plane: " + doc.ActiveView.SketchPlane.Name.ToString();
          }
          else
          {
            TaskDialog.Show("Please Select Valid Workplane", "Please select a workplane whose normal is parallel to the view direction and try running New Reference Point again.");
            return (Result) 1;
          }
        }
      }
      else if (doc.ActiveView.ViewType.Equals((object) ViewType.EngineeringPlan))
      {
        if (doc.ActiveView.GenLevel != null)
        {
          plane = Plane.CreateByNormalAndOrigin(doc.ActiveView.ViewDirection, new XYZ(0.0, 0.0, doc.ActiveView.GenLevel.Elevation));
          planeName = "Current Level: " + doc.ActiveView.GenLevel.Name.ToString();
        }
        else
          noZ = true;
      }
      else
        noZ = true;
      Autodesk.Revit.UI.Selection.Selection selection = uiApp.ActiveUIDocument.Selection;
      if (elementIds.Count == 0)
      {
        ICollection<ElementId> source = References.PickNewReferences("Please select elements to move.");
        if (source == null)
          return (Result) 1;
        elementIds = (ICollection<ElementId>) source.Select<ElementId, Element>(new Func<ElementId, Element>(doc.GetElement)).SelectMany<Element, ElementId>((Func<Element, IEnumerable<ElementId>>) (elems => new List<ElementId>()
        {
          elems.Id
        }.Concat<ElementId>(elems.GetSubComponentIds()))).ToList<ElementId>();
      }
      if (elementIds.Count == 0)
        return (Result) 1;
      foreach (ElementId id in (IEnumerable<ElementId>) elementIds)
      {
        Element element = doc.GetElement(id);
        int integerValue = element.Category.Id.IntegerValue;
        if (integerValue.Equals(-2000260))
        {
          TaskDialog.Show("Invalid Element", "Dimension Lines are invalid elements for New Reference Point. ");
          return (Result) 1;
        }
        integerValue = element.Category.Id.IntegerValue;
        if (integerValue.Equals(-2000264))
        {
          TaskDialog.Show("Invalid Element", "Spot Coordinates are invalid elements for New Reference Point.");
          return (Result) 1;
        }
        integerValue = element.Category.Id.IntegerValue;
        if (integerValue.Equals(-2000263))
        {
          TaskDialog.Show("Invalid Element", "Spot Elevations are invalid elements for New Reference Point.");
          return (Result) 1;
        }
        integerValue = element.Category.Id.IntegerValue;
        if (integerValue.Equals(-2000265))
        {
          TaskDialog.Show("Invalid Element", "Spot Slopes are invalid elements for New Reference Point.");
          return (Result) 1;
        }
      }
      Element elem = !(elementIds.ToList<ElementId>().Count == 1 | noZ) ? doc.GetElement(selection.PickObject((ObjectType) 1, (ISelectionFilter) new OnlySelectedElems(uiDoc, elementIds.ToList<ElementId>()), "Please select the element from selection group that will be your Z Axis reference.")) : doc.GetElement(elementIds.ToList<ElementId>().First<ElementId>());
      XYZ xyz1 = selection.PickPoint("Please select starting reference point.");
      XYZ xyz2 = selection.PickPoint("Please select new reference point");
      bool imperialUnits = true;
      if (doc.GetUnits().GetFormatOptions(SpecTypeId.Length).GetUnitTypeId().TypeId.ToUpper().Contains("METERS"))
        imperialUnits = false;
      NewRefPointForm newRefPointForm = new NewRefPointForm(noZ, planeName, imperialUnits);
      int num2 = (int) newRefPointForm.ShowDialog();
      bool xNullFlag = newRefPointForm.xNullFlag;
      bool yNullFlag = newRefPointForm.yNullFlag;
      bool zNullFlag = newRefPointForm.zNullFlag;
      XYZ xyz3 = new XYZ(newRefPointForm.newXYZ.X, newRefPointForm.newXYZ.Y, newRefPointForm.newXYZ.Z);
      XYZ offsetXYZ = new XYZ(xyz2.X - xyz1.X, xyz2.Y - xyz1.Y, xyz2.Z - xyz1.Z);
      Transform translation1 = Transform.CreateTranslation(XYZ.Zero);
      translation1.BasisX = doc.ActiveView.RightDirection;
      translation1.BasisY = doc.ActiveView.UpDirection;
      translation1.BasisZ = doc.ActiveView.ViewDirection;
      XYZ xyz4 = NewReferencePoint.offsetTransform(offsetXYZ, doc.ActiveView.RightDirection, doc.ActiveView.UpDirection, doc.ActiveView.ViewDirection, true);
      if (!xNullFlag)
      {
        XYZ translation2 = translation1.OfPoint(new XYZ(xyz4.X, 0.0, 0.0)) + NewReferencePoint.offsetTransform(new XYZ(xyz3.X, 0.0, 0.0), doc.ActiveView.RightDirection, doc.ActiveView.UpDirection, doc.ActiveView.ViewDirection);
        ElementTransformUtils.MoveElements(doc, elementIds, translation2);
      }
      if (!yNullFlag)
      {
        XYZ translation3 = translation1.OfPoint(new XYZ(0.0, xyz4.Y, 0.0)) + NewReferencePoint.offsetTransform(new XYZ(0.0, xyz3.Y, 0.0), doc.ActiveView.RightDirection, doc.ActiveView.UpDirection, doc.ActiveView.ViewDirection);
        ElementTransformUtils.MoveElements(doc, elementIds, translation3);
      }
      if (!zNullFlag && !noZ)
      {
        XYZ xyz5 = new XYZ(0.0, 0.0, this.GetReferenceZ(elem, plane, doc, doc.ActiveView.RightDirection, doc.ActiveView.UpDirection, doc.ActiveView.ViewDirection).Z);
        XYZ translation4 = NewReferencePoint.offsetTransform(new XYZ(0.0, 0.0, xyz3.Z - xyz5.Z), doc.ActiveView.RightDirection, doc.ActiveView.UpDirection, doc.ActiveView.ViewDirection);
        ElementTransformUtils.MoveElements(doc, elementIds, translation4);
      }
      int num3 = (int) transaction.Commit();
      return (Result) 0;
    }
  }

  public XYZ GetElemCenter(Element elem)
  {
    BoundingBoxXYZ boundingBoxXyz = elem.get_BoundingBox((View) null);
    return (boundingBoxXyz.Max + boundingBoxXyz.Min) * 0.5;
  }

  public XYZ GetElemIdCenter(ICollection<ElementId> elementId, Document doc)
  {
    BoundingBoxXYZ boundingBox = this.GetBoundingBox(elementId, doc);
    return (boundingBox.Max + boundingBox.Min) * 0.5;
  }

  public BoundingBoxXYZ GetBoundingBox(ICollection<ElementId> elementId, Document doc)
  {
    double x1 = 0.0;
    double y1 = 0.0;
    double z1 = 0.0;
    double x2 = 0.0;
    double y2 = 0.0;
    double z2 = 0.0;
    bool flag = true;
    foreach (ElementId id in (IEnumerable<ElementId>) elementId)
    {
      BoundingBoxXYZ boundingBoxXyz = doc.GetElement(id).get_BoundingBox((View) null);
      if (flag)
      {
        x1 = boundingBoxXyz.Max.X;
        y1 = boundingBoxXyz.Max.Y;
        z1 = boundingBoxXyz.Max.Z;
        x2 = boundingBoxXyz.Min.X;
        y2 = boundingBoxXyz.Min.Y;
        z2 = boundingBoxXyz.Min.Z;
        flag = false;
      }
      else
      {
        if (boundingBoxXyz.Max.X >= x1)
          x1 = boundingBoxXyz.Max.X;
        if (boundingBoxXyz.Max.Y >= y1)
          y1 = boundingBoxXyz.Max.Y;
        if (boundingBoxXyz.Max.Z >= z1)
          z1 = boundingBoxXyz.Max.Z;
        if (boundingBoxXyz.Min.X < x2)
          x2 = boundingBoxXyz.Max.X;
        if (boundingBoxXyz.Min.Y < y2)
          y2 = boundingBoxXyz.Max.Y;
        if (boundingBoxXyz.Min.Z < z2)
          z2 = boundingBoxXyz.Max.Z;
      }
    }
    XYZ xyz1 = new XYZ(x1, y1, z1);
    XYZ xyz2 = new XYZ(x2, y2, z2);
    return new BoundingBoxXYZ() { Max = xyz1, Min = xyz2 };
  }

  public static XYZ offsetTransform(XYZ offsetXYZ, XYZ axisX, XYZ axisY, XYZ axisZ, bool inverse = false)
  {
    Transform transform = Transform.CreateTranslation(XYZ.Zero);
    transform.BasisX = axisX;
    transform.BasisY = axisY;
    transform.BasisZ = axisZ;
    if (inverse)
      transform = transform.Inverse;
    return transform.OfPoint(offsetXYZ);
  }

  private bool CheckAssemblies(FamilyInstance famInst, ICollection<ElementId> members)
  {
    bool flag = false;
    if (famInst.Host != null)
    {
      ElementId id = famInst.Host.Id;
      foreach (object member in (IEnumerable<ElementId>) members)
      {
        if (member.Equals((object) id))
        {
          flag = true;
          break;
        }
      }
      return !flag;
    }
    return famInst.SuperComponent == null || !(famInst.SuperComponent is FamilyInstance) || this.CheckAssemblies(famInst.SuperComponent as FamilyInstance, members);
  }
}
