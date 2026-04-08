// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.InsulationPlacement.ManualPlacement
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.InsulationTools.InsulationPlacement.UtilityFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.AssemblyUtils;
using Utils.CollectionUtils;
using Utils.FamilyUtils;
using Utils.WorkSharingUtils;

#nullable disable
namespace EDGE.InsulationTools.InsulationPlacement;

[Transaction(TransactionMode.Manual)]
public class ManualPlacement : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIApplication application = commandData.Application;
    Document document = commandData.Application.ActiveUIDocument.Document;
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    if (document.IsFamilyDocument)
    {
      new TaskDialog("Family Editor")
      {
        AllowCancellation = false,
        CommonButtons = ((TaskDialogCommonButtons) 1),
        MainInstruction = "Manual Insulation Placement must be run in the Project Environment",
        MainContent = "You are currently in the family editor, Manual Insulation Placement must be run in the project environment.  Please return to the project environment or open a project before running this tool."
      }.Show();
      return (Result) 1;
    }
    if (document.ActiveView.AssociatedAssemblyInstanceId != ElementId.InvalidElementId)
    {
      new TaskDialog("Assembly View")
      {
        AllowCancellation = false,
        CommonButtons = ((TaskDialogCommonButtons) 1),
        MainInstruction = "Manual Insulation Placement cannot be run in an Assesmbly View",
        MainContent = "You are currently in an Assembly View, Manual Insulation Placement cannot be ran in this view.  Please select a non-assembly 2D or 3D view in order to run Insulation Placement."
      }.Show();
      return (Result) 1;
    }
    if (document.GetElement(document.ActiveView.Id) is View)
    {
      View element = document.GetElement(document.ActiveView.Id) as View;
      if (element.Title.Contains("Project") && element.Title.Contains("Browser"))
      {
        new TaskDialog("Project Browser")
        {
          AllowCancellation = false,
          CommonButtons = ((TaskDialogCommonButtons) 1),
          MainInstruction = "Manual Insulation Placement cannot be run in the Project Browser",
          MainContent = "You are currently in the Project Browser, Manual Insulation Placement cannot be ran in this view.  Please select a 2D or 3D view in order to run Insulation Placement."
        }.Show();
        return (Result) 1;
      }
    }
    bool bMetric = Utils.MiscUtils.MiscUtils.CheckMetricLengthUnit(document);
    List<ElementId> list1 = activeUiDocument.Selection.GetElementIds().ToList<ElementId>();
    Element element1 = (Element) null;
    ManualPlacementErrorType mpe1 = ManualPlacementErrorType.NoMessage;
    try
    {
      Insulation.Addons = Components.GetAddonsForInsulationPlacement(document);
      ElementMulticategoryFilter filter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
      {
        BuiltInCategory.OST_StructuralFraming
      });
      if (new FilteredElementCollector(document, document.ActiveView.Id).WherePasses((ElementFilter) filter).Where<Element>((Func<Element, bool>) (e => Utils.SelectionUtils.SelectionUtils.CheckConstructionProduct(e))).ToList<Element>().Count == 0)
      {
        PlacementUtilities.CallManualPlacementErrorMessage(ManualPlacementErrorType.NoValidElement);
        return (Result) 1;
      }
      if (list1.Count == 0)
        element1 = PlacementUtilities.SelectWallPanel(document, activeUiDocument);
      else if (list1.Count > 1)
      {
        List<Element> list2 = new FilteredElementCollector(document, (ICollection<ElementId>) list1).WherePasses((ElementFilter) filter).ToList<Element>();
        if (list2.Count == 0)
          element1 = PlacementUtilities.SelectWallPanel(document, activeUiDocument);
        else if (list2.Count == 1)
        {
          element1 = PlacementUtilities.FilterWallPanel(list2[0], out mpe1);
          if (element1 == null)
          {
            element1 = PlacementUtilities.SelectWallPanel(document, activeUiDocument);
            if (element1 == null)
              return (Result) 1;
          }
        }
        else
          mpe1 = ManualPlacementErrorType.TooManySelected;
      }
      else
      {
        List<ElementId> elementIdList = AssemblyInstances.RetrieveSFFromAssemblies(document, list1);
        if (elementIdList.Count > 0)
        {
          List<Element> list3 = new FilteredElementCollector(document, (ICollection<ElementId>) elementIdList).WherePasses((ElementFilter) filter).ToList<Element>();
          if (list3.Count > 0)
            element1 = PlacementUtilities.FilterWallPanel(list3[0], out mpe1);
        }
        if (element1 == null)
        {
          element1 = PlacementUtilities.SelectWallPanel(document, activeUiDocument);
          if (element1 == null)
            return (Result) 1;
        }
      }
      if (document.IsWorkshared)
      {
        if (!CheckElementsOwnership.CheckOwnership("Manual Insulation Placement", new List<ElementId>()
        {
          (element1 as FamilyInstance).Id
        }, document, activeUiDocument, out List<ElementId> _))
          return (Result) 1;
      }
    }
    catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
    {
      return (Result) 1;
    }
    if (mpe1 != ManualPlacementErrorType.NoMessage)
    {
      PlacementUtilities.CallManualPlacementErrorMessage(mpe1);
      return (Result) 1;
    }
    if (element1 == null)
      return (Result) 1;
    bool flag1 = false;
    string str1 = "";
    Parameter parameter1 = Utils.ElementUtils.Parameters.LookupParameter(element1, "DIM_WYTHE_INSULATION");
    if (parameter1 == null || !parameter1.HasValue)
    {
      flag1 = true;
      str1 += "DIM_WYTHE_INSULATION\n";
    }
    Parameter parameter2 = Utils.ElementUtils.Parameters.LookupParameter(element1, "DIM_WYTHE_INNER");
    if (parameter2 == null || !parameter2.HasValue)
    {
      flag1 = true;
      str1 += "DIM_WYTHE_INNER\n";
    }
    Parameter parameter3 = Utils.ElementUtils.Parameters.LookupParameter(element1, "DIM_LENGTH");
    if (parameter3 == null || !parameter3.HasValue)
    {
      flag1 = true;
      str1 += "DIM_LENGTH\n";
    }
    Parameter parameter4 = Utils.ElementUtils.Parameters.LookupParameter(element1, "DIM_WIDTH");
    if (parameter4 == null || !parameter4.HasValue)
    {
      flag1 = true;
      str1 += "DIM_WIDTH\n";
    }
    if (flag1)
    {
      string str2 = $"{(element1 as FamilyInstance).Symbol.FamilyName} - {element1.Name} - {element1.Id?.ToString()}";
      new TaskDialog("Manual Insulation Placement")
      {
        Title = "Manual Insulation Placement - Parameters Warning",
        MainInstruction = $"The {str2} could not be processed because it is missing the following parameter(s). Please update the family to include this/these parameter(s) and try again.",
        ExpandedContent = str1,
        CommonButtons = ((TaskDialogCommonButtons) 32 /*0x20*/)
      }.Show();
      return (Result) 1;
    }
    using (TransactionGroup transactionGroup = new TransactionGroup(document, "Manual Insulation Placement"))
    {
      int num1 = (int) transactionGroup.Start();
      FamilySymbol familySymbol = (FamilySymbol) null;
      try
      {
        Family insulFamily = PlacementUtilities.GetInsulFamily(document);
        if (insulFamily == null)
          return (Result) 1;
        Dictionary<string, ParameterValues> instanceParameter = FamilyUtilities.getInstanceParameter(document, insulFamily, new List<string>()
        {
          "DIM_THICKNESS",
          "DIM_LENGTH",
          "DIM_WIDTH"
        });
        List<string> stringList = new List<string>();
        int num2 = 0;
        ParameterValues parameterValues = instanceParameter["DIM_THICKNESS"];
        if (parameterValues.parameterType == parameterExistence.NULL)
        {
          new TaskDialog("Manual Insulation Placement")
          {
            Title = "Manual Insulation Placement - Parameter Error",
            MainContent = $"Please check the DIM_THICKNESS parameter on {insulFamily.Name} family as it does not exist and try again."
          }.Show();
          return (Result) 1;
        }
        if (!PlacementUtilities.checkInstanceParam("DIM_LENGTH", insulFamily, instanceParameter["DIM_LENGTH"]) || !PlacementUtilities.checkInstanceParam("DIM_WIDTH", insulFamily, instanceParameter["DIM_WIDTH"]))
          return (Result) 1;
        List<Element> insulationTypesForFamily = PlacementUtilities.GetInsulationTypesForFamily(insulFamily);
        if (parameterValues.parameterType == parameterExistence.INSTANCE)
        {
          if (!parameterValues.readOnly)
          {
            familySymbol = PlacementUtilities.getTypesForManual(activeUiDocument, insulFamily, insulationTypesForFamily, true, element1);
          }
          else
          {
            stringList.Add(insulFamily.Name);
            int num3 = num2 + 1;
          }
        }
        else
          familySymbol = PlacementUtilities.getTypesForManual(activeUiDocument, insulFamily, insulationTypesForFamily, false, element1);
      }
      catch (Exception ex)
      {
        return (Result) 1;
      }
      if (familySymbol == null)
        return (Result) 1;
      Transform transform = (element1 as FamilyInstance).GetTransform();
      if (PlacementUtilities.IsMirrored(element1))
        transform.BasisX = transform.BasisX.Negate();
      PlacementUtilities.RunInsulationRemoval(element1, document);
      bool bSymbol;
      PlanarFace frontFace = PlacementUtilities.GetFrontFace(element1, out bSymbol);
      if ((GeometryObject) frontFace == (GeometryObject) null)
      {
        TaskDialog.Show("Manual Insulation Placement", "The geometry of the selected wall could not be processed for insulation placement.");
        return (Result) 1;
      }
      ManualPlacementErrorType errType = PlacementUtilities.CheckView(document, activeUiDocument, element1, frontFace, bSymbol);
      if (errType != ManualPlacementErrorType.NoMessage)
      {
        PlacementUtilities.CallManualPlacementErrorMessage(errType);
        return (Result) 1;
      }
      if (!PlacementUtilities.IsolateWall(element1, activeUiDocument, document))
        return (Result) 1;
      double num4 = 8.0;
      double num5 = 0.0;
      double num6 = 8.0;
      double num7 = 0.0;
      Insulation insul = new Insulation(document, element1);
      PlacementUtilities.GetParamsForInsul(insul, familySymbol, element1);
      if (insul.MaxLength == 0.0)
        insul.MaxLength = num4;
      if (insul.MinLength == 0.0)
        insul.MinLength = num5;
      if (insul.MaxWidth == 0.0)
        insul.MaxWidth = num6;
      if (insul.MinWidth == 0.0)
        insul.MinWidth = num7;
      insul.Face = frontFace;
      if ((GeometryObject) PlacementUtilities.GetOuterCurveLoop(element1, insul) == (GeometryObject) null)
      {
        TaskDialog.Show("Manual Placement Error", "The geometry of the selected wall could not be processed for insulation placement tool.");
        return (Result) 1;
      }
      PlacementUtilities.UnionSolids(insul);
      PlacementUtilities.DrawUnionedFaceLines(insul, document);
      bool flag2 = true;
      int num8 = 0;
      while (flag2)
      {
        insul.ExpandedToMin = false;
        insul.ExpandedToMax = false;
        try
        {
          Tuple<XYZ, XYZ> clickPoints1 = PlacementUtilities.RetrieveClickPoints(activeUiDocument);
          if (clickPoints1 == null)
          {
            PlacementUtilities.RevertIsolation(document);
            if (transactionGroup.HasStarted())
            {
              int num9 = (int) transactionGroup.Commit();
            }
            return num8 == 0 ? (Result) 1 : (Result) 0;
          }
          ManualPlacementErrorType mpe2;
          Tuple<XYZ, XYZ> transformedClickPoints1 = PlacementUtilities.VerifyClickPoints(clickPoints1, insul.UnionedFace, insul, out mpe2);
          if (mpe2 == ManualPlacementErrorType.PointsOutsideLoop || mpe2 == ManualPlacementErrorType.InvalidDimensions)
          {
            if (mpe2 == ManualPlacementErrorType.InvalidDimensions)
              PlacementUtilities.CallManualPlacementErrorMessage(ManualPlacementErrorType.InvalidDimensions);
            else
              PlacementUtilities.CallManualPlacementErrorMessage(ManualPlacementErrorType.PointsOutsideLoop);
          }
          else
          {
            switch (mpe2)
            {
              case ManualPlacementErrorType.InsulationCutIntoMultiple:
                PlacementUtilities.CallManualPlacementErrorMessage(mpe2);
                continue;
              case ManualPlacementErrorType.InvalidDimensions:
                PlacementUtilities.CallManualPlacementErrorMessage(mpe2);
                continue;
              default:
                double len = 0.0;
                double wid = 0.0;
                PointOrientation pointOrientation = PointOrientation.UpperLeft;
                MaxMinStatus mms1;
                XYZ lengthWidthAndOrigin = PlacementUtilities.GetLengthWidthAndOrigin(transformedClickPoints1.Item1, transformedClickPoints1, insul, transform, bMetric, out len, out wid, out mms1, out ManualPlacementErrorType _, out pointOrientation);
                if (mms1 == MaxMinStatus.Reselect)
                {
                  bool flag3 = false;
                  while (!flag3)
                  {
                    try
                    {
                      XYZ xyz = PlacementUtilities.RetrieveClickPoint(activeUiDocument);
                      Tuple<XYZ, XYZ> clickPoints2 = new Tuple<XYZ, XYZ>(transformedClickPoints1.Item1, xyz);
                      if (clickPoints2.Item2 == null)
                      {
                        PlacementUtilities.RevertIsolation(document);
                        return num8 == 0 ? (Result) 1 : (Result) 0;
                      }
                      Tuple<XYZ, XYZ> transformedClickPoints2 = PlacementUtilities.VerifyClickPoints(clickPoints2, insul.UnionedFace, insul, out mpe2);
                      switch (mpe2)
                      {
                        case ManualPlacementErrorType.PointsOutsideLoop:
                          goto label_107;
                        case ManualPlacementErrorType.InvalidDimensions:
                          PlacementUtilities.CallManualPlacementErrorMessage(mpe2);
                          continue;
                        default:
                          MaxMinStatus mms2;
                          lengthWidthAndOrigin = PlacementUtilities.GetLengthWidthAndOrigin(transformedClickPoints1.Item1, transformedClickPoints2, insul, transform, bMetric, out len, out wid, out mms2, out ManualPlacementErrorType _, out pointOrientation);
                          if (mms2 != MaxMinStatus.Reselect)
                          {
                            flag3 = true;
                            continue;
                          }
                          continue;
                      }
                    }
                    catch (System.OperationCanceledException ex)
                    {
                      int num10 = (int) transactionGroup.Commit();
                      return num8 == 0 ? (Result) 1 : (Result) 0;
                    }
                  }
label_107:
                  if (mpe2 == ManualPlacementErrorType.PointsOutsideLoop)
                  {
                    PlacementUtilities.CallManualPlacementErrorMessage(mpe2);
                    continue;
                  }
                }
                if (len == 0.0 || wid == 0.0)
                {
                  PlacementUtilities.CallManualPlacementErrorMessage(ManualPlacementErrorType.InvalidDimensions);
                  continue;
                }
                insul.InsulLength = len;
                insul.InsulWidth = wid;
                if (insul.ExpandedToMin)
                {
                  ManualPlacementErrorType m;
                  if (!PlacementUtilities.ValidateFinalDimensions(transformedClickPoints1.Item1, insul.InsulLength, insul.InsulWidth, insul, pointOrientation, out m))
                  {
                    PlacementUtilities.CallManualPlacementErrorMessage(ManualPlacementErrorType.LessThanLenMin);
                    continue;
                  }
                  if (m == ManualPlacementErrorType.InsulationCutIntoMultiple)
                  {
                    PlacementUtilities.CallManualPlacementErrorMessage(m);
                    continue;
                  }
                }
                ManualPlacementErrorType m1;
                PlacementUtilities.CheckSplitInsulation(transformedClickPoints1.Item1, insul.InsulLength, insul.InsulWidth, insul, pointOrientation, out m1);
                if (m1 == ManualPlacementErrorType.InsulationCutIntoMultiple)
                {
                  PlacementUtilities.CallManualPlacementErrorMessage(m1);
                  continue;
                }
                if (!insul.ExpandedToMin && !insul.ExpandedToMax && mpe2 == ManualPlacementErrorType.InsulationCutIntoMultiple)
                {
                  PlacementUtilities.CallManualPlacementErrorMessage(mpe2);
                  continue;
                }
                num8 += PlacementUtilities.SetInsulDimensionsandPlace(insul, element1, lengthWidthAndOrigin, familySymbol, frontFace);
                switch (num8)
                {
                  case -3:
                    new TaskDialog("Warning")
                    {
                      MainContent = "The DIM_LENGTH parameter on the insulation family cannot be modified by the tool. Please ensure that this parameter is set to be a length type parameter and that it is not read-only."
                    }.Show();
                    return (Result) 1;
                  case -2:
                    new TaskDialog("Warning")
                    {
                      MainContent = "The DIM_WIDTH parameter on the insulation family cannot be modified by the tool. Please ensure that this parameter is set to be a length type parameter and that it is not read-only."
                    }.Show();
                    return (Result) 1;
                  case -1:
                    new TaskDialog("Warning")
                    {
                      MainContent = "The DIM_LENGTH parameter on the insulation family cannot be modified by the tool. Please ensure that this parameter is set to be a length type parameter and that it is not read-only."
                    }.Show();
                    return (Result) 1;
                  default:
                    continue;
                }
            }
          }
        }
        catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
        {
          flag2 = false;
        }
      }
      if (num8 > 0)
        PlacementUtilities.CallManualPlacementErrorMessage(ManualPlacementErrorType.Success);
      PlacementUtilities.RevertIsolation(document);
      int num11 = (int) transactionGroup.Commit();
      return num8 == 0 ? (Result) 1 : (Result) 0;
    }
  }
}
