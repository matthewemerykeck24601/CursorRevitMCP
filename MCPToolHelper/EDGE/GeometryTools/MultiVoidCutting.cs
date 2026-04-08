// Decompiled with JetBrains decompiler
// Type: EDGE.GeometryTools.MultiVoidCutting
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using EDGE.IUpdaters.ModelLocking;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.CollectionUtils;
using Utils.Exceptions;
using Utils.Forms;
using Utils.SelectionUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.GeometryTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public class MultiVoidCutting : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    if (document.IsFamilyDocument)
    {
      new TaskDialog("Family Editor")
      {
        AllowCancellation = false,
        CommonButtons = ((TaskDialogCommonButtons) 1),
        MainInstruction = "Multi Void Cutting Must be run in the Project Environment",
        MainContent = "You are currently in the family editor, Multi Void Cutting must be run in the project environment.  Please return to the project environment or open a project before running this tool.",
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
      }.Show();
      return (Result) 1;
    }
    if (!ModelLockingUtils.ShowPermissionsDialog(document, ModelLockingToolPermissions.MultiVoid))
      return (Result) 1;
    bool flag = false;
    ICollection<ElementId> elementIds = (ICollection<ElementId>) new List<ElementId>();
    Options options = new Options();
    options.IncludeNonVisibleObjects = true;
    ICollection<Element> elements1 = (ICollection<Element>) new List<Element>();
    ICollection<Element> elements2 = (ICollection<Element>) new List<Element>();
    List<ElementId> elementIdList = new List<ElementId>();
    ICollection<ElementId> source1 = activeUiDocument.Selection.GetElementIds();
    using (Transaction transaction = new Transaction(document, "Multi Void Cutting"))
    {
      ICollection<ElementId> voidIds = Components.GetVoidIds(document);
      if (source1.Count == 0 || source1.Count > 0 && source1.Any<ElementId>((Func<ElementId, bool>) (id => !voidIds.Contains(id))))
      {
        ISelectionFilter selFilter = (ISelectionFilter) new OnlyVoids(voidIds.ToList<ElementId>());
        ICollection<ElementId> source2 = References.PickNewReferences(activeUiDocument, selFilter, "Select the voids to perform the cutting.", (ICollection<ElementId>) source1.Where<ElementId>((Func<ElementId, bool>) (id => voidIds.Contains(id))).ToList<ElementId>());
        if (source2 == null)
          return (Result) 1;
        source1 = (ICollection<ElementId>) source2.Select<ElementId, Element>(new Func<ElementId, Element>(document.GetElement)).SelectMany<Element, ElementId>((Func<Element, IEnumerable<ElementId>>) (elem => new List<ElementId>()
        {
          elem.Id
        }.Concat<ElementId>(elem.GetSubComponentIds()))).Where<ElementId>(new Func<ElementId, bool>(voidIds.Contains)).ToList<ElementId>();
      }
      if (source1.Count == 0)
        return (Result) 1;
      if (source1.Count > 0)
      {
        foreach (ElementId id in (IEnumerable<ElementId>) source1)
        {
          Element element1 = document.GetElement(id);
          Element element2 = (Element) null;
          if (element1 is FamilyInstance familyInstance && familyInstance.SuperComponent != null)
          {
            element2 = document.GetElement(familyInstance.SuperComponent.Id);
            flag = true;
          }
          GeometryElement geometryElement = element1.get_Geometry(options);
          if (!(element1 is AssemblyInstance) && element1 is FamilyInstance)
          {
            Element element3 = document.GetElement(element1.GetTypeId());
            Parameter parameter1 = element3.LookupParameter("CONSTRUCTION_PRODUCT");
            Parameter parameter2 = element3.LookupParameter("MANUFACTURE_COMPONENT");
            string str1 = "";
            string str2 = "";
            if (parameter1 != null || parameter2 != null)
            {
              if (parameter1 != null && parameter1.HasValue)
                str1 = parameter1.AsString().ToUpper();
              if (parameter2 != null && parameter2.HasValue)
                str2 = parameter2.AsString().ToUpper();
              if (str2.Contains("VOID") || str1.Contains("VOID"))
              {
                foreach (GeometryObject geometryObject in geometryElement)
                {
                  Solid solid = geometryObject as Solid;
                  if (!((GeometryObject) solid == (GeometryObject) null) && solid.Faces.Size != 0 && solid.Edges.Size != 0)
                  {
                    ElementId graphicsStyleId = solid.GraphicsStyleId;
                    Element element4 = document.GetElement(graphicsStyleId);
                    if (element4 == null)
                    {
                      if (flag && element2 != null)
                        elements1.Add(element2);
                      else
                        elements1.Add(element1);
                    }
                    else if (element4.Name.Equals("Cutting geometry"))
                    {
                      if (flag && element2 != null)
                        elements2.Add(element2);
                      else
                        elements2.Add(element1);
                    }
                  }
                }
              }
            }
          }
        }
      }
      try
      {
        while (true)
        {
          Element element5;
          do
          {
            ISelectionFilter iselectionFilter = (ISelectionFilter) new NoVoids(document);
            element5 = document.GetElement(activeUiDocument.Selection.PickObject((ObjectType) 1, iselectionFilter, "Select the next element to be cut."));
          }
          while (elementIds.Contains(element5.Id));
          if (element5 is AssemblyInstance)
          {
            new TaskDialog("Multi Void Cutting")
            {
              FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
              MainInstruction = "Error:  The selected element is an Assembly, which cannot be cut. Press \"Close\" below to continue cutting."
            }.Show();
          }
          else
          {
            int num1 = (int) transaction.Start();
            if (element5 is FamilyInstance familyInstance && familyInstance.GetSubComponentIds().Count > 0)
            {
              foreach (ElementId subComponentId in (IEnumerable<ElementId>) familyInstance.GetSubComponentIds())
              {
                Element element6 = document.GetElement(subComponentId);
                if (element6.Name.ToUpper().Contains("FLAT"))
                {
                  element5 = element6;
                  break;
                }
              }
            }
            foreach (Element cuttingSolid in (IEnumerable<Element>) elements1)
            {
              BoundingBoxXYZ boundingBoxXyz = cuttingSolid.get_BoundingBox(document.ActiveView);
              Outline outline = new Outline(boundingBoxXyz.Min, boundingBoxXyz.Max);
              if (new LogicalOrFilter((ElementFilter) new BoundingBoxIntersectsFilter(outline), (ElementFilter) new BoundingBoxIsInsideFilter(outline)).PassesFilter(document, element5.Id))
                SolidSolidCutUtils.AddCutBetweenSolids(document, element5, cuttingSolid);
            }
            foreach (Element cuttingInstance in (IEnumerable<Element>) elements2)
            {
              BoundingBoxXYZ boundingBoxXyz = cuttingInstance.get_BoundingBox(document.ActiveView);
              Outline outline = new Outline(boundingBoxXyz.Min, boundingBoxXyz.Max);
              if (new LogicalOrFilter((ElementFilter) new BoundingBoxIntersectsFilter(outline), (ElementFilter) new BoundingBoxIsInsideFilter(outline)).PassesFilter(document, element5.Id) && !InstanceVoidCutUtils.GetElementsBeingCut(cuttingInstance).Contains(element5.Id))
                InstanceVoidCutUtils.AddInstanceVoidCut(document, element5, cuttingInstance);
            }
            int num2 = (int) transaction.Commit();
          }
        }
      }
      catch (Autodesk.Revit.Exceptions.InvalidOperationException ex)
      {
        ExceptionMessages.ShowInvalidViewMessage((Exception) ex);
        if (transaction.HasStarted())
        {
          int num = (int) transaction.RollBack();
        }
        return (Result) 1;
      }
      catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
      {
        return (Result) 0;
      }
      catch (Exception ex)
      {
        if (ex.ToString().Contains("Object reference not set to an instance of an object."))
        {
          TaskDialog.Show("Multi Void Cutting", "Error:  The selected object is not valid for cutting.");
          if (transaction.HasStarted())
          {
            int num = (int) transaction.RollBack();
          }
          return (Result) 0;
        }
        int num3 = (int) transaction.RollBack();
        ErrorForm errorForm = new ErrorForm();
        errorForm.textBox1.Text = ex.ToString();
        int num4 = (int) errorForm.ShowDialog();
        return (Result) 0;
      }
    }
  }
}
