// Decompiled with JetBrains decompiler
// Type: EDGE.GeometryTools.WarpedParameterUpdater
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
using Utils.Exceptions;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.GeometryTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class WarpedParameterUpdater : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    IList<ElementId> elementIdList = (IList<ElementId>) new List<ElementId>();
    List<string> parameterNames = new List<string>()
    {
      "Length_of_Drop",
      "High_Point_Elevation",
      "Low_Point_Elevation",
      "CL_DT",
      "Vertical_Offset_MarkEnd",
      "Vertical_Offset_OppEnd",
      "Warp_Angle_MarkEnd",
      "Warp_Angle_OppEnd",
      "Manual_Mark_End_Offset",
      "Manual_Mark_End_Warp_Angle",
      "Manual_Opp_End_Offset",
      "Manual_Opp_End_Warp_Angle"
    };
    List<string> yesNoParamNames = new List<string>()
    {
      "Warp_Mark_End",
      "Warp_Opp_End",
      "High_Point_Right"
    };
    if (!ModelLockingUtils.ShowPermissionsDialog(document, ModelLockingToolPermissions.WarpedParameter))
      return (Result) 1;
    using (Transaction transaction = new Transaction(document, "Warped Parameter Updater"))
    {
      try
      {
        Reference reference1 = activeUiDocument.Selection.PickObject((ObjectType) 1, "Pick the Element whose WARPED Parameters are to be copied.");
        if (!(document.GetElement(reference1) is FamilyInstance element1))
        {
          message = "Picked Element is not a Family Instance.  Please pick a warpable structural framing element.";
          return (Result) -1;
        }
        Element element2 = !element1.HasSuperComponent() ? (Element) element1 : element1.SuperComponent;
        elementIdList.Add(element2.Id);
        Dictionary<string, double> parametersToCopy1 = this.GetParametersToCopy(element2, (IEnumerable<string>) parameterNames);
        Dictionary<string, int> parametersToCopy2 = this.GetYesNoParametersToCopy(element2, yesNoParamNames);
        while (true)
        {
          Element element3;
          do
          {
            Reference reference2 = activeUiDocument.Selection.PickObject((ObjectType) 1, "Pick the next Element to be updated.");
            if (!(document.GetElement(reference2) is FamilyInstance element4))
              TaskDialog.Show("EDGE Error", "Picked Element is not a Structural Framing Family Instance");
            else
              element3 = !element4.HasSuperComponent() ? (Element) element4 : element4.SuperComponent;
          }
          while (document.GetElement(element3.AssemblyInstanceId) != null || elementIdList.Contains(element3.Id));
          int num1 = (int) transaction.Start();
          this.UpdateParameters(element3, parametersToCopy1);
          this.UpdateYesNoParameters(element3, parametersToCopy2);
          elementIdList.Add(element3.Id);
          int num2 = (int) transaction.Commit();
        }
      }
      catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
      {
        return (Result) 0;
      }
      catch (Autodesk.Revit.Exceptions.InvalidOperationException ex)
      {
        ExceptionMessages.ShowInvalidViewMessage((Exception) ex);
        return (Result) 1;
      }
      catch (Exception ex)
      {
        if (ex.ToString().Contains("Object reference not set to an instance of an object."))
        {
          message = "Error: The selected element is invalid for parameter updating.";
          return (Result) 0;
        }
        if (ex.ToString().Contains("The user aborted the pick operation."))
          return (Result) 0;
        message = $"Error: There was an error updating parameters for the selected element.{Environment.NewLine}{Environment.NewLine}{ex?.ToString()}";
        return (Result) -1;
      }
    }
  }

  private Dictionary<string, int> GetYesNoParametersToCopy(
    Element topLevelElem,
    List<string> yesNoParamNames)
  {
    Dictionary<string, int> parametersToCopy = new Dictionary<string, int>();
    foreach (string yesNoParamName in yesNoParamNames)
    {
      Parameter parameter = topLevelElem.LookupParameter(yesNoParamName);
      if (parameter != null)
        parametersToCopy.Add(yesNoParamName, parameter.AsInteger());
    }
    return parametersToCopy;
  }

  private Dictionary<string, double> GetParametersToCopy(
    Element element,
    IEnumerable<string> parameterNames)
  {
    Dictionary<string, double> parametersToCopy = new Dictionary<string, double>();
    foreach (string parameterName in parameterNames)
    {
      Parameter parameter = element.LookupParameter(parameterName);
      if (parameter != null)
        parametersToCopy.Add(parameterName, parameter.AsDouble());
    }
    return parametersToCopy;
  }

  private void UpdateParameters(Element element, Dictionary<string, double> parameterValuePairs)
  {
    foreach (KeyValuePair<string, double> parameterValuePair in parameterValuePairs)
    {
      Parameter parameter = element.LookupParameter(parameterValuePair.Key);
      if (parameter != null && !parameter.IsReadOnly)
        parameter.Set(parameterValuePair.Value);
    }
  }

  private void UpdateYesNoParameters(
    Element element,
    Dictionary<string, int> yesNoParameterValuePairs)
  {
    foreach (KeyValuePair<string, int> parameterValuePair in yesNoParameterValuePairs)
    {
      Parameter parameter = element.LookupParameter(parameterValuePair.Key);
      if (parameter != null && !parameter.IsReadOnly)
        parameter.Set(parameterValuePair.Value);
    }
  }
}
