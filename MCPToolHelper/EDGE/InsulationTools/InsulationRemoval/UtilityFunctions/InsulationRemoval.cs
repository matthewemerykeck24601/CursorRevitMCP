// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.InsulationRemoval.UtilityFunctions.InsulationRemoval
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.InsulationTools.InsulationRemoval.UtilityFunctions;

public class InsulationRemoval
{
  public static List<Element> RetrieveValidSFPieces(List<Element> selectedElements)
  {
    List<Element> elementList = new List<Element>();
    List<Element> list = selectedElements.Where<Element>((Func<Element, bool>) (e => Utils.SelectionUtils.SelectionUtils.CheckConstructionProduct(e) && Parameters.LookupParameter(e, "INSULATION_INCLUDED") != null)).ToList<Element>();
    list.AddRange((IEnumerable<Element>) EDGE.InsulationTools.InsulationRemoval.UtilityFunctions.InsulationRemoval.CheckSuperComponentInsulRemoval(selectedElements));
    return list;
  }

  public static List<Element> CheckSuperComponentInsulRemoval(List<Element> selectedElems)
  {
    List<Element> elementList = new List<Element>();
    foreach (Element selectedElem in selectedElems)
    {
      if (selectedElem.HasSuperComponent())
      {
        Element superComponent = selectedElem.GetSuperComponent();
        if (superComponent.Category.Id.Equals((object) new ElementId(BuiltInCategory.OST_StructuralFraming)) && Utils.SelectionUtils.SelectionUtils.CheckConstructionProduct(superComponent) && Parameters.LookupParameter(superComponent, "INSULATION_INCLUDED") != null)
          elementList.Add(superComponent);
      }
    }
    return elementList;
  }

  public static RemovalStatus RemoveInsulFromSF(Document revitDoc, List<Element> sfElements)
  {
    int num1 = 0;
    using (Transaction transaction = new Transaction(revitDoc, "Remove Insulation"))
    {
      int num2 = (int) transaction.Start();
      foreach (Element sfElement in sfElements)
      {
        if (Parameters.LookupParameter(sfElement, "INSULATION_INCLUDED") != null)
        {
          EDGE.InsulationTools.InsulationRemoval.UtilityFunctions.InsulationRemoval.TurnOffVisibility(revitDoc, sfElement);
          ++num1;
        }
      }
      int num3 = (int) transaction.Commit();
    }
    return num1 == 0 ? RemovalStatus.Failed : RemovalStatus.Removed;
  }

  public static void TurnOffVisibility(Document revitDoc, Element insulSF)
  {
    Parameters.LookupParameter(insulSF, "INSULATION_INCLUDED").Set(0);
  }

  public static List<ElementId> SelectValidSFElements(Document revitDoc, UIDocument uiDoc)
  {
    List<Reference> list = uiDoc.Selection.PickObjects((ObjectType) 1, (ISelectionFilter) new InsulationSFFilter(), "Pick Structural Framing Elements To Remove Insulation From").ToList<Reference>();
    List<ElementId> elementIdList = new List<ElementId>();
    foreach (Reference reference in list)
    {
      if (revitDoc.GetElement(reference) != null)
        elementIdList.Add(revitDoc.GetElement(reference).Id);
    }
    return elementIdList;
  }

  public static void CallInsulationErrorMessage(InsulErrorType errType, List<Element> readOnlyElems = null)
  {
    switch (errType)
    {
      case InsulErrorType.Failure:
        new TaskDialog("Insulation Removal")
        {
          AllowCancellation = false,
          CommonButtons = ((TaskDialogCommonButtons) 1),
          MainInstruction = "Unable to Remove Insulation",
          MainContent = "Unable to Remove Insulation. Please ensure that the selection contained viable Structural Framing Elements and that the INSULATION_INCLUDED parameter exists."
        }.Show();
        break;
      case InsulErrorType.NoElements:
        new TaskDialog("Insulation Removal")
        {
          AllowCancellation = false,
          CommonButtons = ((TaskDialogCommonButtons) 1),
          MainInstruction = "No Elements Found To Remove Insulation From.",
          MainContent = "No Structural Framing Elements were found in your selection that fit the criteria to remove Insulation. Please ensure that eligible Structural Framing Elements contain \"WALL\" and \"INSULATED\" in their CONSTRUCTION_PRODUCT parameter and also have the INSULATION_INCLUDED parameter."
        }.Show();
        break;
      case InsulErrorType.ElementsRemoved:
        new TaskDialog("Insulation Removal")
        {
          AllowCancellation = false,
          CommonButtons = ((TaskDialogCommonButtons) 1),
          MainInstruction = "Elements Removed from Selection Group",
          MainContent = "Ineligible Elements have been removed from your selection group. Eligible Structural Framing Elements must contain \"WALL\" and \"INSULATED\" in their CONSTRUCTION_PRODUCT parameter and also have the INSULATION_INCLUDED parameter."
        }.Show();
        break;
      case InsulErrorType.ReadOnly:
        new TaskDialog("Insulation Removal")
        {
          AllowCancellation = false,
          CommonButtons = ((TaskDialogCommonButtons) 1),
          MainInstruction = "Elements Removed from Selection Group",
          MainContent = "The INSULATION_INCLUDED parameter is read only for the following Structural Framing Element(s). Please ensure that the parameter is not marked as read only in order for Insulation to be removed for the Element(s).",
          ExpandedContent = ErrorMessageUtils.RetrieveIDAndName(readOnlyElems).ToString()
        }.Show();
        break;
      case InsulErrorType.NoElementsSelGroup:
        new TaskDialog("Insulation Removal")
        {
          AllowCancellation = false,
          CommonButtons = ((TaskDialogCommonButtons) 1),
          MainInstruction = "No Elements Selected To Remove Insulation From.",
          MainContent = "No Structural Framing Elements were selected that fit the criteria to remove Insulation. Please ensure that eligible Structural Framing Elements contain \"WALL\" and \"INSULATED\" in their CONSTRUCTION_PRODUCT parameter and also have the INSULATION_INCLUDED parameter."
        }.Show();
        break;
      case InsulErrorType.Success:
        new TaskDialog("Insulation Removal")
        {
          AllowCancellation = false,
          CommonButtons = ((TaskDialogCommonButtons) 1),
          MainInstruction = "Insulation Successfully Removed",
          MainContent = "Insulation has been successfully removed from your selection group."
        }.Show();
        break;
    }
  }
}
