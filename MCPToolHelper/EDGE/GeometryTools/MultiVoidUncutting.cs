// Decompiled with JetBrains decompiler
// Type: EDGE.GeometryTools.MultiVoidUncutting
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
public class MultiVoidUncutting : IExternalCommand
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
        MainInstruction = "Multi Void Uncutting Must be run in the Project Environment",
        MainContent = "You are currently in the family editor, Multi Void Uncutting must be run in the project environment.  Please return to the project environment or open a project before running this tool.",
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
      }.Show();
      return (Result) 1;
    }
    if (!ModelLockingUtils.ShowPermissionsDialog(document, ModelLockingToolPermissions.MultiVoid))
      return (Result) 1;
    bool flag1 = false;
    ICollection<ElementId> elementIds = (ICollection<ElementId>) new List<ElementId>();
    Options options = new Options()
    {
      IncludeNonVisibleObjects = true
    };
    ICollection<Element> solidVoidList = (ICollection<Element>) new List<Element>();
    ICollection<Element> voidVoidList = (ICollection<Element>) new List<Element>();
    List<ElementId> elementIdList = new List<ElementId>();
    ICollection<ElementId> source1 = activeUiDocument.Selection.GetElementIds();
    ICollection<ElementId> voidIds = Components.GetVoidIds(document);
    if (source1.Count == 0 || source1.Count > 0 && source1.Any<ElementId>((Func<ElementId, bool>) (id => !voidIds.Contains(id))))
    {
      ISelectionFilter selFilter = (ISelectionFilter) new OnlyVoids(voidIds.ToList<ElementId>());
      ICollection<ElementId> source2 = References.PickNewReferences(activeUiDocument, selFilter, "Select the voids to perform the un-cutting.", (ICollection<ElementId>) source1.Where<ElementId>((Func<ElementId, bool>) (id => voidIds.Contains(id))).ToList<ElementId>());
      if (source2 == null)
        return (Result) 1;
      source1 = (ICollection<ElementId>) source2.Select<ElementId, Element>(new Func<ElementId, Element>(document.GetElement)).SelectMany<Element, ElementId>((Func<Element, IEnumerable<ElementId>>) (elem => new List<ElementId>()
      {
        elem.Id
      }.Concat<ElementId>(elem.GetSubComponentIds()))).Where<ElementId>(new Func<ElementId, bool>(voidIds.Contains)).ToList<ElementId>();
    }
    if (source1.Count == 0)
      return (Result) 1;
    foreach (ElementId id in (IEnumerable<ElementId>) source1)
    {
      Element element1 = document.GetElement(id);
      Element element2 = (Element) null;
      if (element1 is FamilyInstance familyInstance && familyInstance.SuperComponent != null)
      {
        element2 = document.GetElement(familyInstance.SuperComponent.Id);
        flag1 = true;
      }
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
            foreach (GeometryObject geometryObject in element1.get_Geometry(options))
            {
              Solid solid = geometryObject as Solid;
              if (!((GeometryObject) solid == (GeometryObject) null) && (GeometryObject) solid != (GeometryObject) null && solid.Faces.Size != 0 && solid.Edges.Size != 0)
              {
                ElementId graphicsStyleId = solid.GraphicsStyleId;
                Element element4 = document.GetElement(graphicsStyleId);
                if (element4 == null)
                {
                  if (flag1 && element2 != null)
                    solidVoidList.Add(element2);
                  else
                    solidVoidList.Add(element1);
                }
                else if (element4.Name.Equals("Cutting geometry"))
                {
                  if (flag1 && element2 != null)
                    voidVoidList.Add(element2);
                  else
                    voidVoidList.Add(element1);
                }
              }
            }
          }
        }
      }
    }
    using (Transaction transaction = new Transaction(document, "Multi Void Uncutting"))
    {
      try
      {
        while (true)
        {
          Element elementToBeUncut1;
          do
          {
            ISelectionFilter iselectionFilter = (ISelectionFilter) new NoVoids(document);
            elementToBeUncut1 = document.GetElement(activeUiDocument.Selection.PickObject((ObjectType) 1, iselectionFilter, "Select the next element to be uncut."));
          }
          while (elementIds.Contains(elementToBeUncut1.Id));
          if (elementToBeUncut1 is AssemblyInstance)
          {
            new TaskDialog("Error")
            {
              FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
              MainInstruction = "Error:  The selected element is an Assembly, which cannot be used in the (un)cutting process. Press \"Close\" below to continue uncutting other elements."
            }.Show();
          }
          else
          {
            int num1 = (int) transaction.Start();
            if (elementToBeUncut1 is FamilyInstance familyInstance && familyInstance.GetSubComponentIds().Count > 0)
            {
              bool flag2 = false;
              bool flag3 = false;
              FamilyInstance elementToBeUncut2 = (FamilyInstance) null;
              FamilyInstance elementToBeUncut3 = (FamilyInstance) null;
              foreach (ElementId subComponentId in (IEnumerable<ElementId>) familyInstance.GetSubComponentIds())
              {
                Element element = document.GetElement(subComponentId);
                string upper = element.Name.ToUpper();
                if (upper.Contains("INSULATION"))
                {
                  elementToBeUncut2 = element as FamilyInstance;
                  flag2 = true;
                }
                if (upper.Contains("GROUT"))
                {
                  elementToBeUncut3 = element as FamilyInstance;
                  flag3 = true;
                }
                if (flag2)
                  MultiVoidUncutting.MultiUncutter(document, voidVoidList, solidVoidList, (Element) elementToBeUncut2);
                if (flag3)
                  MultiVoidUncutting.MultiUncutter(document, voidVoidList, solidVoidList, (Element) elementToBeUncut3);
                if ((upper.Contains("FLAT") || upper.Contains("WARPED")) && upper.Contains("FLAT"))
                {
                  elementToBeUncut1 = element;
                  break;
                }
              }
            }
            MultiVoidUncutting.MultiUncutter(document, voidVoidList, solidVoidList, elementToBeUncut1);
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
        if (transaction.HasStarted())
        {
          int num3 = (int) transaction.RollBack();
        }
        ErrorForm errorForm = new ErrorForm();
        errorForm.textBox1.Text = ex.ToString();
        int num4 = (int) errorForm.ShowDialog();
        return (Result) 0;
      }
    }
  }

  public static void MultiUncutter(
    Document doc,
    ICollection<Element> voidVoidList,
    ICollection<Element> solidVoidList,
    Element elementToBeUncut)
  {
    foreach (Element solidVoid in (IEnumerable<Element>) solidVoidList)
    {
      if (SolidSolidCutUtils.CutExistsBetweenElements(solidVoid, elementToBeUncut, out bool _))
        SolidSolidCutUtils.RemoveCutBetweenSolids(doc, elementToBeUncut, solidVoid);
    }
    foreach (Element voidVoid in (IEnumerable<Element>) voidVoidList)
    {
      if (InstanceVoidCutUtils.GetElementsBeingCut(voidVoid).Contains(elementToBeUncut.Id))
        InstanceVoidCutUtils.RemoveInstanceVoidCut(doc, elementToBeUncut, voidVoid);
    }
  }
}
