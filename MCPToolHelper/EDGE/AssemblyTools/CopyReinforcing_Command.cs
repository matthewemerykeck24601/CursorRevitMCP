// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.CopyReinforcing_Command
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
using System.Windows;
using Utils.AssemblyUtils;
using Utils.ElementUtils;
using Utils.Exceptions;
using Utils.SelectionUtils;

#nullable disable
namespace EDGE.AssemblyTools;

[Regeneration(RegenerationOption.Manual)]
[Transaction(TransactionMode.Manual)]
internal class CopyReinforcing_Command : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    try
    {
      UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
      Document document = activeUiDocument.Document;
      if (!ModelLockingUtils.ShowPermissionsDialog(document, ModelLockingToolPermissions.CopyReinforcing))
        return (Result) 1;
      using (Transaction transaction = new Transaction(activeUiDocument.Document, "Copy Reinforcing"))
      {
        if (transaction.Start() != TransactionStatus.Started)
        {
          message = "EDGE: Unable to start transaction.";
          return (Result) -1;
        }
        View activeView = document.ActiveView;
        if (activeView is ViewSheet)
        {
          int num = (int) MessageBox.Show("Copy Reinforcing Tool cannot be ran in the assembly sheet, transaction will be cancelled.", "Warning");
          return (Result) 1;
        }
        if (activeView.ViewType == ViewType.ProjectBrowser)
        {
          int num = (int) MessageBox.Show("Copy Reinforcing Tool cannot be ran in the project browser, transaction will be cancelled.", "Warning");
          return (Result) 1;
        }
        if (activeView.ViewType == ViewType.Schedule)
        {
          int num = (int) MessageBox.Show("Copy Reinforcing Tool cannot be ran in schedule, transaction will be cancelled.", "Warning");
          return (Result) 1;
        }
        foreach (ElementId filter in (IEnumerable<ElementId>) document.ActiveView.GetFilters())
        {
          if ((document.GetElement(filter) as ParameterFilterElement).Name.ToUpper().Contains("WARPED PRODUCT"))
          {
            if (document.ActiveView.GetFilterVisibility(filter))
            {
              ICollection<ElementId> filters = document.ActiveView.GetFilters();
              bool flag = false;
              foreach (ElementId elementId in (IEnumerable<ElementId>) filters)
              {
                if ((document.GetElement(elementId) as ParameterFilterElement).Name.ToUpper().Contains("FLAT PRODUCT"))
                {
                  flag = true;
                  if (!document.ActiveView.GetFilterVisibility(elementId))
                  {
                    int num = (int) MessageBox.Show("The Flat element for this assembly is not visible in this view. To properly copy reinforcing, the flat element must be visible for selection. Enable flat element visibility from the EDGE^R Visibility panel and re-start this tool.", "Warning");
                    return (Result) -1;
                  }
                  break;
                }
              }
              if (!flag)
              {
                int num = (int) MessageBox.Show("The Flat element for this assembly is not visible in this view. To properly copy reinforcing, the flat element must be visible for selection. Enable flat element visibility from the EDGE^R Visibility panel and re-start this tool.", "Warning");
                return (Result) -1;
              }
              break;
            }
            break;
          }
        }
        Reference reference1;
        try
        {
          reference1 = activeUiDocument.Selection.PickObject((ObjectType) 1, (ISelectionFilter) new OnlyAssemblyInstances(), "Please select the assembly for which to copy reinforcing from.");
        }
        catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
        {
          return (Result) 1;
        }
        catch (Autodesk.Revit.Exceptions.InvalidOperationException ex)
        {
          ExceptionMessages.ShowInvalidViewMessage((Exception) ex);
          return (Result) 1;
        }
        Element element1 = document.GetElement(reference1);
        AssemblyInstance assemblyInstance = element1 as AssemblyInstance;
        ICollection<ElementId> memberIds = assemblyInstance.GetMemberIds();
        memberIds.Remove(assemblyInstance.GetStructuralFramingElement().Id);
        ICollection<ElementId> elementsToCopy = (ICollection<ElementId>) new List<ElementId>();
        foreach (ElementId id in (IEnumerable<ElementId>) memberIds)
        {
          Element topLevelElement = document.GetElement(id).GetTopLevelElement();
          string manufactureComponent = topLevelElement.GetManufactureComponent();
          string familyName = topLevelElement is FamilyInstance ? (topLevelElement as FamilyInstance).Symbol.FamilyName : "";
          if (new ModelLockingOracle().RebarHandlingComponents.Contains(manufactureComponent) || manufactureComponent.Contains("REBAR") || manufactureComponent.Contains("LIFTING") || familyName.ToUpper().Contains("LIFT"))
            elementsToCopy.Add(id);
        }
        ICollection<Reference> references;
        try
        {
          references = (ICollection<Reference>) activeUiDocument.Selection.PickObjects((ObjectType) 1, (ISelectionFilter) new RestAssemblyInstances(element1), "Please select the assembly for which to copy reinforcing to.");
        }
        catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
        {
          return (Result) 1;
        }
        catch (Autodesk.Revit.Exceptions.InvalidOperationException ex)
        {
          ExceptionMessages.ShowInvalidViewMessage((Exception) ex);
          return (Result) 1;
        }
        if (elementsToCopy.Count == 0)
        {
          int num1 = (int) MessageBox.Show($"There is no reinforcing in the selected assembly with mark number {Utils.ElementUtils.Parameters.GetParameterAsString((Element) assemblyInstance, "ASSEMBLY_MARK_NUMBER")}, nothing will be copied to the destination.", "Warning");
        }
        else
        {
          if (references.Count == 0)
          {
            int num2 = (int) MessageBox.Show("There is no assembly selected to copy reinforcing to, transaction will be cancelled.", "Warning");
          }
          foreach (Reference reference2 in (IEnumerable<Reference>) references)
          {
            AssemblyInstance element2 = document.GetElement(reference2) as AssemblyInstance;
            Transform transform1 = assemblyInstance.GetTransform();
            Transform transform2 = element2.GetTransform().Multiply(transform1.Inverse);
            CopyPasteOptions options = new CopyPasteOptions();
            ElementTransformUtils.CopyElements(document, elementsToCopy, document, transform2, options);
          }
        }
        return transaction.Commit() != TransactionStatus.Committed ? (Result) -1 : (Result) 0;
      }
    }
    catch (Exception ex)
    {
      if (ex.ToString().Contains("View does not belong to a project document."))
      {
        int num3 = (int) MessageBox.Show("Copy Reinforcing cannot be ran in the family view, transaction will be cancelled.", "Warning");
      }
      else
      {
        int num4 = (int) MessageBox.Show("Could not complete Copy Reinforcing operation. Transaction will be cancelled.", "Warning");
      }
      return (Result) -1;
    }
  }
}
