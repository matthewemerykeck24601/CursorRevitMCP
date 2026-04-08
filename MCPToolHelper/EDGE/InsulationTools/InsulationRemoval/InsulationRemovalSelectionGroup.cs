// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.InsulationRemoval.InsulationRemovalSelectionGroup
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.InsulationTools.InsulationRemoval.UtilityFunctions;
using System.Collections.Generic;
using System.Linq;
using Utils.AssemblyUtils;

#nullable disable
namespace EDGE.InsulationTools.InsulationRemoval;

public class InsulationRemovalSelectionGroup
{
  public static Result SelectionGroup(Document revitDoc, UIApplication uiApp, UIDocument uiDoc)
  {
    if (revitDoc.IsFamilyDocument)
    {
      new TaskDialog("Family Editor")
      {
        AllowCancellation = false,
        CommonButtons = ((TaskDialogCommonButtons) 1),
        MainInstruction = "Insulation Removal Tool must be run in the Project Environment",
        MainContent = "You are currently in the family editor, Insulation Removal Tool must be run in the project environment.  Please return to the project environment or open a project before running this tool."
      }.Show();
      return (Result) 1;
    }
    using (TransactionGroup transactionGroup = new TransactionGroup(revitDoc, "Insulation Removal - Whole Model"))
    {
      int num1 = (int) transactionGroup.Start();
      List<ElementId> selectionGroup = uiDoc.Selection.GetElementIds().ToList<ElementId>();
      if (selectionGroup.Count == 0)
      {
        try
        {
          selectionGroup = EDGE.InsulationTools.InsulationRemoval.UtilityFunctions.InsulationRemoval.SelectValidSFElements(revitDoc, uiDoc);
        }
        catch
        {
          int num2 = (int) transactionGroup.RollBack();
          return (Result) 1;
        }
      }
      if (selectionGroup.Count == 0)
      {
        EDGE.InsulationTools.InsulationRemoval.UtilityFunctions.InsulationRemoval.CallInsulationErrorMessage(InsulErrorType.NoElementsSelGroup);
        int num3 = (int) transactionGroup.RollBack();
        return (Result) 1;
      }
      List<ElementId> elementIdList = AssemblyInstances.RetrieveSFFromAssemblies(revitDoc, selectionGroup);
      ElementMulticategoryFilter filter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
      {
        BuiltInCategory.OST_StructuralFraming,
        BuiltInCategory.OST_GenericModel,
        BuiltInCategory.OST_SpecialityEquipment
      });
      int count = elementIdList.Count;
      List<Element> elements = EDGE.InsulationTools.InsulationRemoval.UtilityFunctions.InsulationRemoval.RetrieveValidSFPieces(new FilteredElementCollector(revitDoc, (ICollection<ElementId>) elementIdList).WherePasses((ElementFilter) filter).ToList<Element>());
      List<Element> roElements = new List<Element>();
      List<Element> sfElements = Utils.SelectionUtils.SelectionUtils.CheckForReadOnly(elements, "INSULATION_INCLUDED", out roElements);
      if (sfElements.Count == 0 && roElements.Count == 0)
      {
        EDGE.InsulationTools.InsulationRemoval.UtilityFunctions.InsulationRemoval.CallInsulationErrorMessage(InsulErrorType.NoElementsSelGroup);
        int num4 = (int) transactionGroup.RollBack();
        return (Result) 1;
      }
      if (roElements.Count > 0)
      {
        EDGE.InsulationTools.InsulationRemoval.UtilityFunctions.InsulationRemoval.CallInsulationErrorMessage(InsulErrorType.ReadOnly, roElements);
        if (sfElements.Count == 0)
        {
          int num5 = (int) transactionGroup.RollBack();
          return (Result) 1;
        }
      }
      if (count != sfElements.Count + roElements.Count)
        EDGE.InsulationTools.InsulationRemoval.UtilityFunctions.InsulationRemoval.CallInsulationErrorMessage(InsulErrorType.ElementsRemoved);
      RemovalStatus removalStatus;
      try
      {
        removalStatus = EDGE.InsulationTools.InsulationRemoval.UtilityFunctions.InsulationRemoval.RemoveInsulFromSF(revitDoc, sfElements);
      }
      catch
      {
        EDGE.InsulationTools.InsulationRemoval.UtilityFunctions.InsulationRemoval.CallInsulationErrorMessage(InsulErrorType.Failure);
        int num6 = (int) transactionGroup.RollBack();
        return (Result) -1;
      }
      if (removalStatus == RemovalStatus.Failed)
      {
        EDGE.InsulationTools.InsulationRemoval.UtilityFunctions.InsulationRemoval.CallInsulationErrorMessage(InsulErrorType.Failure);
        int num7 = (int) transactionGroup.RollBack();
        return (Result) -1;
      }
      int num8 = (int) transactionGroup.Commit();
    }
    EDGE.InsulationTools.InsulationRemoval.UtilityFunctions.InsulationRemoval.CallInsulationErrorMessage(InsulErrorType.Success);
    return (Result) 0;
  }
}
