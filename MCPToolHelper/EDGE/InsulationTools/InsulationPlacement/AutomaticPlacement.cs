// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.InsulationPlacement.AutomaticPlacement
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using EDGE.AdminTools;
using EDGE.InsulationTools.InsulationPlacement.UtilityFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.AssemblyUtils;
using Utils.CollectionUtils;
using Utils.FamilyUtils;
using Utils.MiscUtils;
using Utils.WorkSharingUtils;

#nullable disable
namespace EDGE.InsulationTools.InsulationPlacement;

[Transaction(TransactionMode.Manual)]
public class AutomaticPlacement : IExternalCommand
{
  private bool defaultIsSet = App.default_insulation_set;
  private bool showAspectRatioDialog = App.aspectRatioSet;
  private bool horizontal;
  private string _insulationFamily;

  public Result Execute(
    ExternalCommandData commandData,
    ref string refMessage,
    ElementSet elementSet)
  {
    UIApplication application = commandData.Application;
    UIDocument activeUiDocument = application.ActiveUIDocument;
    Document document = commandData.Application.ActiveUIDocument.Document;
    Autodesk.Revit.UI.Selection.Selection selection = application.ActiveUIDocument.Selection;
    if (document.IsFamilyDocument)
    {
      new TaskDialog("Family Editor")
      {
        AllowCancellation = false,
        CommonButtons = ((TaskDialogCommonButtons) 1),
        MainInstruction = "Automatic Insulation Placement must be run in the Project Environment",
        MainContent = "You are currently in the family editor, Automatic Insulation Placement must be run in the project environment. Please return to the project environment or open a project before running this tool."
      }.Show();
      return (Result) 1;
    }
    using (TransactionGroup transactionGroup = new TransactionGroup(document, "Insulation Placement"))
    {
      Insulation.Addons = Components.GetAddonsForInsulationPlacement(document);
      int num1 = (int) transactionGroup.Start();
      using (Transaction transaction = new Transaction(document, "revit"))
      {
        int num2 = (int) transaction.Start();
        document.Regenerate();
        int num3 = (int) transaction.Commit();
      }
      ICollection<ElementId> elementIds = application.ActiveUIDocument.Selection.GetElementIds();
      try
      {
        if (!this.selectionofWholeDocumentsorFew(document, elementIds, selection, activeUiDocument))
        {
          int num4 = (int) transactionGroup.RollBack();
          return (Result) 1;
        }
      }
      catch (Exception ex)
      {
        int num5 = (int) transactionGroup.RollBack();
        return (Result) 1;
      }
      int num6 = (int) transactionGroup.Assimilate();
    }
    return (Result) 0;
  }

  private bool selectionofWholeDocumentsorFew(
    Document document,
    ICollection<ElementId> elementIds,
    Autodesk.Revit.UI.Selection.Selection sel,
    UIDocument uidoc)
  {
    TaskDialog taskDialog1 = new TaskDialog("Automatic Insulation Placement");
    taskDialog1.MainIcon = (TaskDialogIcon) 65533;
    taskDialog1.Title = "Automatic Insulation Placement";
    taskDialog1.MainInstruction = "Automatic Insulation Placement";
    taskDialog1.MainContent = "Select the scope for running the Insulation Placement";
    taskDialog1.AddCommandLink((TaskDialogCommandLinkId) 1001, "Run Automatic Insulation Placement for the Whole Project.");
    taskDialog1.AddCommandLink((TaskDialogCommandLinkId) 1002, "Run Automatic Insulation Placement for the Active View.");
    taskDialog1.AddCommandLink((TaskDialogCommandLinkId) 1003, "Run Automatic Insulation Placement for Selected Structural Framing Elements.");
    taskDialog1.CommonButtons = (TaskDialogCommonButtons) 8;
    taskDialog1.DefaultButton = (TaskDialogResult) 2;
    TaskDialogResult taskDialogResult1 = taskDialog1.Show();
    IEnumerable<Element> source = (IEnumerable<Element>) null;
    List<string> stringList1 = new List<string>();
    List<ElementId> elementIdList = new List<ElementId>();
    if (taskDialogResult1 == 7 || taskDialogResult1 == 2)
      return false;
    if (taskDialogResult1 == 1001)
      source = this.filterelements(document, AutomaticPlacement.OptionPicked.WholeModel);
    else if (taskDialogResult1 == 1002)
      source = this.filterelements(document, AutomaticPlacement.OptionPicked.ActiveView);
    else if (taskDialogResult1 == 1003)
    {
      if (elementIds.Count<ElementId>() == 0)
      {
        AutomaticPlacement.Filter filter = new AutomaticPlacement.Filter();
        elementIds = (ICollection<ElementId>) sel.PickObjects((ObjectType) 1, (ISelectionFilter) filter, "Please select structural framing elements for processing").ToList<Reference>().Select<Reference, ElementId>((Func<Reference, ElementId>) (r => r.ElementId)).ToList<ElementId>();
      }
      if (elementIds.Count<ElementId>() == 0)
      {
        new TaskDialog("Automatic Insulation Placement")
        {
          Title = "Automatic Insulation Placement - Warning",
          MainInstruction = "No Structural Framing elements were selected for Automatic Insulation Placement."
        }.Show();
        return false;
      }
      source = this.filterelements(document, AutomaticPlacement.OptionPicked.Selection, elementIds.ToList<ElementId>());
      if (source.Count<Element>() != elementIds.Count<ElementId>())
      {
        List<ElementId> list = source.Select<Element, ElementId>((Func<Element, ElementId>) (e => e.Id)).ToList<ElementId>();
        foreach (ElementId elementId in (IEnumerable<ElementId>) elementIds)
        {
          Element element = document.GetElement(elementId);
          if (element.Category.Id.IntegerValue == -2001320)
          {
            string str1 = "";
            if (!list.Contains(elementId))
            {
              FamilyInstance familyInstance = element as FamilyInstance;
              string str2 = $"{str1}{familyInstance.Symbol.FamilyName} {familyInstance.Name} {familyInstance.Id?.ToString()}";
              stringList1.Add(str2);
            }
          }
        }
        if (stringList1.Count<string>() > 0)
        {
          stringList1.Sort((Comparison<string>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p, q)));
          string str3 = string.Join("\n", (IEnumerable<string>) stringList1);
          string str4 = elementIds.Count<ElementId>() != stringList1.Count<string>() ? "Some Structural Framing element(s) are invalid for processing. Please ensure their CONSTRUCTION_PRODUCT parameter contains WALL and INSULATED and that they also have all other relevant parameters for processing." : "All Structural Framing element(s) are invalid for processing. Please ensure their CONSTRUCTION_PRODUCT parameter contains WALL and INSULATED and that they also have all other relevant parameters for processing.";
          new TaskDialog("Automatic Insulation Placement")
          {
            Title = "Automatic Insulation Placement - Warning",
            MainInstruction = str4,
            ExpandedContent = ("Offending Element(s): \n" + str3),
            CommonButtons = ((TaskDialogCommonButtons) 32 /*0x20*/)
          }.Show();
          if (elementIds.Count<ElementId>() == stringList1.Count<string>())
            return false;
        }
      }
    }
    List<ElementId> list1 = source.Select<Element, ElementId>((Func<Element, ElementId>) (e => e.Id)).ToList<ElementId>();
    Dictionary<FamilySymbol, List<Element>> dictionary1 = new Dictionary<FamilySymbol, List<Element>>();
    Dictionary<double, List<Element>> thicknessSeparatedElements = new Dictionary<double, List<Element>>();
    HashSet<double> doubleSet = new HashSet<double>();
    if (source.Count<Element>() == 0)
    {
      new TaskDialog("Automatic Insulation Placement")
      {
        MainInstruction = "No valid structural framing elements were found."
      }.Show();
      return false;
    }
    if (source.Count<Element>() > 0)
    {
      Dictionary<string, List<string>> dictionary2 = new Dictionary<string, List<string>>();
      List<string> stringList2 = new List<string>();
      List<Element> elementList = new List<Element>();
      Document document1 = source.First<Element>().Document;
      if (document1.IsWorkshared && !CheckElementsOwnership.CheckOwnership("Automatic Insulation Placement", list1, document1, uidoc, out List<ElementId> _))
        return false;
      foreach (Element elem in source)
      {
        bool flag1 = false;
        if (Utils.ElementUtils.Parameters.LookupParameter(elem, "DIM_LENGTH") == null)
        {
          flag1 = true;
          string key = "DIM_LENGTH";
          string str = $"{(elem as FamilyInstance).Symbol.FamilyName} - {elem.Name} - {elem.Id?.ToString()}";
          if (!dictionary2.ContainsKey(key))
            dictionary2.Add(key, new List<string>() { str });
          else
            dictionary2[key].Add(str);
        }
        if (Utils.ElementUtils.Parameters.LookupParameter(elem, "DIM_WIDTH") == null)
        {
          flag1 = true;
          string key = "DIM_WIDTH";
          string str = $"{(elem as FamilyInstance).Symbol.FamilyName} - {elem.Name} - {elem.Id?.ToString()}";
          if (!dictionary2.ContainsKey(key))
            dictionary2.Add(key, new List<string>() { str });
          else
            dictionary2[key].Add(str);
        }
        if (Utils.ElementUtils.Parameters.LookupParameter(elem, "DIM_WYTHE_INNER") == null)
        {
          flag1 = true;
          string key = "DIM_WYTHE_INNER";
          string str = $"{(elem as FamilyInstance).Symbol.FamilyName} - {elem.Name} - {elem.Id?.ToString()}";
          if (!dictionary2.ContainsKey(key))
            dictionary2.Add(key, new List<string>() { str });
          else
            dictionary2[key].Add(str);
        }
        Parameter parameter = Utils.ElementUtils.Parameters.LookupParameter(elem, "DIM_WYTHE_INSULATION");
        if (parameter == null)
        {
          flag1 = true;
          string key = "DIM_WYTHE_INSULATION";
          string str = $"{(elem as FamilyInstance).Symbol.FamilyName} - {elem.Name} - {elem.Id?.ToString()}";
          if (!dictionary2.ContainsKey(key))
            dictionary2.Add(key, new List<string>() { str });
          else
            dictionary2[key].Add(str);
        }
        if (!flag1)
        {
          elementList.Add(elem);
          double num = parameter.AsDouble();
          bool flag2 = false;
          foreach (double key in thicknessSeparatedElements.Keys)
          {
            if (num.ApproximatelyEquals(key))
            {
              thicknessSeparatedElements[key].Add(elem);
              flag2 = true;
            }
          }
          if (!flag2)
            thicknessSeparatedElements.Add(num, new List<Element>()
            {
              elem
            });
        }
      }
      if (dictionary2.Keys.Count<string>() > 0)
      {
        string str5 = "";
        foreach (string key in dictionary2.Keys)
        {
          string str6 = string.Join("\n", dictionary2[key].ToArray());
          str5 = $"{str5}{key}: \n{str6}";
          str5 += "\n\n";
        }
        new TaskDialog("Automatic Insulation Placement")
        {
          Title = "Automatic Insulation Placement - Parameters Warning",
          MainInstruction = "The following Structural Framing elements could not be processed because they are missing certain parameters defined below. Please update the families to include these parameters and try again.",
          ExpandedContent = str5,
          CommonButtons = ((TaskDialogCommonButtons) 32 /*0x20*/)
        }.Show();
      }
      if (elementList.Count<Element>() == 0)
      {
        new TaskDialog("Automatic Insulation Placement")
        {
          MainContent = "No elements were processed"
        }.Show();
        return false;
      }
      int num1 = (int) EDGE.InsulationTools.InsulationRemoval.UtilityFunctions.InsulationRemoval.RemoveInsulFromSF(document1, elementList);
      try
      {
        Family insulFamily = PlacementUtilities.GetInsulFamily(document1);
        if (insulFamily == null)
          return false;
        List<Element> insulationTypesForFamily = PlacementUtilities.GetInsulationTypesForFamily(insulFamily);
        List<string> stringList3 = new List<string>();
        int num2 = 0;
        Dictionary<string, ParameterValues> instanceParameter = FamilyUtilities.getInstanceParameter(document1, insulFamily, new List<string>()
        {
          "DIM_THICKNESS",
          "DIM_LENGTH",
          "DIM_WIDTH"
        });
        ParameterValues parameterValues = instanceParameter["DIM_THICKNESS"];
        if (parameterValues.parameterType == parameterExistence.NULL)
        {
          new TaskDialog("Automatic Insulation Placement")
          {
            Title = "Automatic Insulation Placement - Parameter Error",
            MainContent = $"Please check the DIM_THICKNESS parameter on {insulFamily.Name} family as it does not exist and try again."
          }.Show();
          return false;
        }
        if (!PlacementUtilities.checkInstanceParam("DIM_LENGTH", insulFamily, instanceParameter["DIM_LENGTH"]) || !PlacementUtilities.checkInstanceParam("DIM_WIDTH", insulFamily, instanceParameter["DIM_WIDTH"]))
          return false;
        if (parameterValues.parameterType.Equals((object) parameterExistence.INSTANCE))
        {
          if (!parameterValues.readOnly)
          {
            dictionary1 = PlacementUtilities.getTypes(uidoc, insulFamily, insulationTypesForFamily, true, alltheElements: elementList);
          }
          else
          {
            stringList3.Add(insulFamily.Name);
            ++num2;
          }
        }
        else
          dictionary1 = PlacementUtilities.getTypes(uidoc, insulFamily, insulationTypesForFamily, false, thicknessSeparatedElements);
        if (dictionary1.Keys.Count == 0)
          return false;
        if (!this.showAspectRatioDialog)
        {
          TaskDialog taskDialog2 = new TaskDialog("Automatic Insulation Placement");
          taskDialog2.Title = "Automatic Insulation Placement - Horizontal or Vertical Placement";
          taskDialog2.MainInstruction = "Please select if you would like the insulation to be placed horizontally or vertically in the structural framing members with regard to the length-wise direction of each structural framing piece.";
          taskDialog2.AddCommandLink((TaskDialogCommandLinkId) 1001, "Horizontal");
          taskDialog2.AddCommandLink((TaskDialogCommandLinkId) 1002, "Vertical");
          taskDialog2.ExtraCheckBoxText = "Do not show again";
          TaskDialogResult taskDialogResult2 = taskDialog2.Show();
          if (taskDialogResult2 == 2)
            return false;
          if (taskDialogResult2 == 1001)
            this.horizontal = true;
          if (taskDialog2.WasExtraCheckBoxChecked())
          {
            App.aspectRatioSet = true;
            App.horizontalPlacement = this.horizontal;
          }
        }
        else
          this.horizontal = App.horizontalPlacement;
        Dictionary<string, Dictionary<FamilySymbol, List<Element>>> dictionary3 = new Dictionary<string, Dictionary<FamilySymbol, List<Element>>>();
        List<string> stringList4 = new List<string>();
        bool duplicateErr = false;
        foreach (FamilySymbol key in dictionary1.Keys)
        {
          Parameter parameter1 = Utils.ElementUtils.Parameters.LookupParameter((Element) key, "DIM_LENGTH_MAX");
          Parameter parameter2 = Utils.ElementUtils.Parameters.LookupParameter((Element) key, "DIM_LENGTH_MIN");
          Parameter parameter3 = Utils.ElementUtils.Parameters.LookupParameter((Element) key, "DIM_WIDTH_MAX");
          Parameter parameter4 = Utils.ElementUtils.Parameters.LookupParameter((Element) key, "DIM_WIDTH_MIN");
          double maxLength = 8.0;
          double maxWidth = 4.0;
          double minLength = 0.6;
          double minWidth = 0.6;
          if (parameter1 != null && parameter1.HasValue)
            maxLength = parameter1.AsDouble();
          if (parameter2 != null && parameter2.HasValue)
            minLength = parameter2.AsDouble();
          if (parameter3 != null && parameter3.HasValue)
            maxWidth = parameter3.AsDouble();
          if (parameter4 != null && parameter4.HasValue)
            minWidth = parameter4.AsDouble();
          if (maxLength < 0.0)
            this.negativeError("DIM_LENGTH_MAX", key);
          else if (maxLength == 0.0)
            this.zeroError("DIM_LENGTH_MAX", key);
          else if (minLength == 0.0)
            this.zeroError("DIM_LENGTH_MIN", key);
          else if (minLength < 0.0)
            this.negativeError("DIM_LENGTH_MIN", key);
          else if (maxWidth < 0.0)
            this.negativeError("DIM_WIDTH_MAX", key);
          else if (maxWidth == 0.0)
            this.zeroError("DIM_WIDTH_MAX", key);
          else if (minWidth < 0.0)
            this.negativeError("DIM_WIDTH_MIN", key);
          else if (minWidth == 0.0)
            this.zeroError("DIM_WIDTH_MIN", key);
          else if (maxLength < minLength)
            new TaskDialog("Automatic Insulation Placement")
            {
              Title = "Automatic Insulation Placement - Length",
              MainInstruction = $"Please check the DIM_LENGTH_MIN parameter on {key.FamilyName} : {key.Name}. It should not be greater than DIM_LENGTH_MAX parameter."
            }.Show();
          else if (maxWidth < minWidth)
          {
            new TaskDialog("Automatic Insulation Placement")
            {
              Title = "Automatic Insulation Placement - Width",
              MainInstruction = $"Please check the DIM_WIDTH_MIN parameter on {key.FamilyName} : {key.Name}. It should not be greater than DIM_WIDTH_MAX parameter."
            }.Show();
          }
          else
          {
            Dictionary<string, List<Element>> dictionary4 = new Dictionary<string, List<Element>>();
            foreach (Element elem in dictionary1[key])
            {
              string problems;
              int counter;
              if (this.PlaceInsulation(document1, elem, uidoc, key, this.horizontal, maxLength, minLength, maxWidth, minWidth, out problems, out counter, out duplicateErr))
                ++num2;
              if (!string.IsNullOrEmpty(problems))
              {
                if (dictionary3.ContainsKey(problems))
                {
                  if (dictionary3[problems].ContainsKey(key))
                    dictionary3[problems][key].Add(elem);
                  else
                    dictionary3[problems].Add(key, new List<Element>()
                    {
                      elem
                    });
                }
                else
                  dictionary3.Add(problems, new Dictionary<FamilySymbol, List<Element>>()
                  {
                    {
                      key,
                      new List<Element>() { elem }
                    }
                  });
              }
              if (counter == -4)
                break;
            }
          }
        }
        if (duplicateErr)
        {
          if (new TaskDialog("Insulation Placement")
          {
            MainContent = "Insulation placement would create duplicate insulation instances in one or more locations in the model. Continue with insulation placement?",
            CommonButtons = ((TaskDialogCommonButtons) 6)
          }.Show() != 6)
            return false;
        }
        if (stringList3.Count<string>() > 0)
        {
          if (elementList.Count<Element>() == 0)
          {
            string str = string.Join(";\n", (IEnumerable<string>) stringList3);
            new TaskDialog("Automatic Insulation Placement")
            {
              Title = "Automatic Insulation Placement - Parameters Warning",
              MainInstruction = "The DIM_THICKNESS parameter for the all elements, it cannot find an insulation of matching thickness or it is set as read only",
              ExpandedContent = str,
              CommonButtons = ((TaskDialogCommonButtons) 32 /*0x20*/)
            }.Show();
          }
          else
          {
            string str = string.Join(";\n", (IEnumerable<string>) stringList3);
            new TaskDialog("Automatic Insulation Placement")
            {
              Title = "Automatic Insulation Placement - Parameters Warning",
              MainInstruction = "Please check the DIM_THICKNESS parameter for the following element(s) it cannot find an insulation of matching thickness or it is set as read only",
              ExpandedContent = str,
              CommonButtons = ((TaskDialogCommonButtons) 32 /*0x20*/)
            }.Show();
          }
        }
        if (dictionary3.Keys.Count<string>() > 0)
        {
          foreach (string key in dictionary3.Keys)
          {
            if (key != null)
            {
              switch (key.Length)
              {
                case 14:
                  if (key == "DIM_WIDTH-READ")
                  {
                    this.ReadError(dictionary3[key], "DIM_WIDTH");
                    continue;
                  }
                  continue;
                case 15:
                  switch (key[0])
                  {
                    case 'D':
                      if (key == "DIM_LENGTH-READ")
                      {
                        this.ReadError(dictionary3[key], "DIM_LENGTH");
                        continue;
                      }
                      continue;
                    case 'g':
                      if (key == "geometryFailure")
                      {
                        this.adjError(dictionary3[key], "GeometryFailure");
                        continue;
                      }
                      continue;
                    default:
                      continue;
                  }
                case 17:
                  switch (key[14])
                  {
                    case 'A':
                      if (key == "DIM_WIDTH_MIN-ADJ")
                      {
                        this.adjError(dictionary3[key], "DIM_WIDTH_MIN");
                        continue;
                      }
                      continue;
                    case 'I':
                      if (key == "DIM_WIDTH_MIN-INS")
                      {
                        this.GreaterError(dictionary3[key], "DIM_WIDTH_MIN", "DIM_WIDTH_MAX");
                        continue;
                      }
                      continue;
                    default:
                      continue;
                  }
                case 18:
                  switch (key[15])
                  {
                    case 'A':
                      if (key == "DIM_LENGTH_MIN-ADJ")
                      {
                        this.adjError(dictionary3[key], "DIM_LENGTH_MIN");
                        continue;
                      }
                      continue;
                    case 'E':
                      if (key == "DIM_THICKNESS_READ")
                      {
                        this.ReadError(dictionary3[key], "DIM_THICKNESS");
                        continue;
                      }
                      continue;
                    case 'I':
                      if (key == "DIM_LENGTH_MIN-INS")
                      {
                        this.GreaterError(dictionary3[key], "DIM_LENGTH_MIN", "DIM_LENGTH_MAX");
                        continue;
                      }
                      continue;
                    default:
                      continue;
                  }
                default:
                  continue;
              }
            }
          }
        }
        if (num2 > 0)
          new TaskDialog("Automatic Insulation Placement - Success")
          {
            MainContent = "Successfully placed all pieces of insulation. Please check insulation panelization for completeness."
          }.Show();
      }
      catch (Exception ex)
      {
        return false;
      }
    }
    return true;
  }

  private void CutIntoMultiple(
    Dictionary<FamilySymbol, List<Element>> information)
  {
    List<Element> elementList = new List<Element>();
    string str1 = "Offending elements: \n";
    foreach (FamilySymbol key in information.Keys)
    {
      string str2 = "";
      foreach (Element element in information[key])
      {
        FamilyInstance familyInstance = element as FamilyInstance;
        str2 = $"{str2}{familyInstance.Symbol.FamilyName} : {familyInstance.Name} {familyInstance.Id?.ToString()}\n";
      }
      string str3 = str2 + "\n";
      str1 += str3;
    }
    new TaskDialog("Automatic Insulation Placement")
    {
      Title = "Automatic Insulation Placement - Split Insulation Error",
      MainInstruction = "One or more walls in the placement group would have caused insulation being placed to be split into multiple parts, these insulation pieces were skipped.",
      ExpandedContent = str1
    }.Show();
  }

  private void adjError(
    Dictionary<FamilySymbol, List<Element>> information,
    string paramName)
  {
    string str1 = "Offending elements: \n";
    string str2 = "Offending elements: \n";
    string str3 = "";
    foreach (FamilySymbol key in information.Keys)
    {
      string str4 = $"\n{key.FamilyName} : {key.Name}";
      foreach (Element elem in information[key])
      {
        FamilyInstance familyInstance = elem as FamilyInstance;
        str4 += "\n";
        str4 = $"{str4}     {familyInstance.Symbol.FamilyName} {familyInstance.Name} {familyInstance.Id?.ToString()}";
        str3 += Utils.ElementUtils.Parameters.GetParameterAsString(elem, "CONTROL_MARK");
        str3 = $"{str3} - {familyInstance.Symbol.FamilyName}: {familyInstance.Symbol.Name} ({familyInstance.Id?.ToString()})\n";
      }
      string str5 = str4 + "\n\n";
      str1 += str5;
    }
    string str6 = str2 + str3;
    TaskDialog taskDialog = new TaskDialog("Automatic Insulation Placement");
    taskDialog.Title = "Automatic Insulation Placement - Insulation Dimension";
    if (paramName.Equals("GeometryFailure"))
    {
      taskDialog.MainInstruction = "The geometry of the following walls could not be processed for insulation placement tool.";
      taskDialog.ExpandedContent += str6;
    }
    else
    {
      taskDialog.MainInstruction = $"Please check {paramName} parameter for the following elements listed below. The adjusted value on insulation family types was found below this dimension";
      taskDialog.ExpandedContent = str1;
    }
    taskDialog.Show();
  }

  private void ReadError(
    Dictionary<FamilySymbol, List<Element>> information,
    string paramName)
  {
    string str1 = "Offending elements: \n";
    foreach (FamilySymbol key in information.Keys)
    {
      string str2 = $"\n{key.FamilyName} : {key.Name}";
      foreach (Element element in information[key])
      {
        FamilyInstance familyInstance = element as FamilyInstance;
        str2 += "\n";
        str2 = $"{str2}     {familyInstance.Symbol.FamilyName} {familyInstance.Name} {familyInstance.Id?.ToString()}";
      }
      string str3 = str2 + "\n\n";
      str1 += str3;
    }
    new TaskDialog("Automatic Insulation Placement")
    {
      Title = "Automatic Insulation Placement - Insulation Dimension",
      MainInstruction = $"The {paramName} parameter for the following elements listed below cannot be modified. Please ensure the insulation family type they belong to have this instance parameter set to be a length type parameter and it is not read only.",
      ExpandedContent = str1
    }.Show();
  }

  private void GreaterError(
    Dictionary<FamilySymbol, List<Element>> information,
    string paramName,
    string valueCorresponding)
  {
    string str1 = "Offending elements: \n";
    foreach (FamilySymbol key in information.Keys)
    {
      string str2 = $"\n{key.FamilyName} : {key.Name}";
      foreach (Element element in information[key])
      {
        FamilyInstance familyInstance = element as FamilyInstance;
        str2 += "\n";
        str2 = $"{str2}     {familyInstance.Symbol.FamilyName} {familyInstance.Name} {familyInstance.Id?.ToString()}";
      }
      string str3 = str2 + "\n\n";
      str1 += str3;
    }
    new TaskDialog("Automatic Insulation Placement")
    {
      Title = "Automatic Insulation Placement - Insulation Dimension",
      MainInstruction = $"Please check {valueCorresponding} for the following elements listed below.The {paramName} on the selected insulation family types was found greater than this dimension.",
      ExpandedContent = str1
    }.Show();
  }

  private bool PlaceInsulation(
    Document revitDoc,
    Element elem,
    UIDocument uidoc,
    FamilySymbol insulationFamiyName,
    bool horizontalOption,
    double maxLength,
    double minLength,
    double maxWidth,
    double minWidth,
    out string problems,
    out int counter,
    out bool duplicateErr)
  {
    try
    {
      bool bMetric = Utils.MiscUtils.MiscUtils.CheckMetricLengthUnit(revitDoc);
      duplicateErr = false;
      counter = 0;
      Insulation insul = new Insulation(revitDoc, elem);
      FamilyInstance wall = elem as FamilyInstance;
      Transform wallTransform = wall.GetTransform();
      problems = "";
      PlacementUtilities.RoundValue(Utils.ElementUtils.Parameters.LookupParameter(elem, "DIM_WYTHE_INSULATION").AsDouble(), bMetric);
      PlacementUtilities.GetParamsForInsul(insul, insulationFamiyName, elem);
      bool flag1 = PlacementUtilities.IsMirrored(elem);
      if (flag1)
        wallTransform.BasisX = wallTransform.BasisX.Negate();
      List<CurveLoop> faceCurves;
      if ((GeometryObject) PlacementUtilities.GetOuterCurveLoop(elem, insul, out faceCurves, false) == (GeometryObject) null)
      {
        problems = "geometryFailure";
        return false;
      }
      Solid extrusionGeometry = GeometryCreationUtilities.CreateExtrusionGeometry((IList<CurveLoop>) faceCurves.Select<CurveLoop, CurveLoop>((Func<CurveLoop, CurveLoop>) (cl => CurveLoop.CreateViaTransform(cl, wallTransform.Inverse))).ToList<CurveLoop>(), flag1 ? -XYZ.BasisY : XYZ.BasisY, 1.0);
      PlanarFace planarFace1 = (PlanarFace) null;
      XYZ source1 = PlacementUtilities.IsMirrored(elem) ? -XYZ.BasisY : XYZ.BasisY;
      foreach (Face face in extrusionGeometry.Faces)
      {
        if (face is PlanarFace)
        {
          PlanarFace planarFace2 = face as PlanarFace;
          if (planarFace2.ComputeNormal(new UV(0.0, 0.0)).IsAlmostEqualTo(source1))
          {
            if ((GeometryObject) planarFace1 == (GeometryObject) null)
              planarFace1 = planarFace2;
            double y1 = planarFace2.Origin.Y;
            double y2 = planarFace1.Origin.Y;
            if (flag1)
            {
              if (y1 < y2)
                planarFace1 = planarFace2;
            }
            else if (y1 > y2)
              planarFace1 = planarFace2;
          }
        }
      }
      List<CurveLoop> curveLoopList1 = this.projectedBoundingBox(planarFace1.GetEdgesAsCurveLoops().ToList<CurveLoop>(), revitDoc);
      List<CurveLoop> curveLoopList2 = new List<CurveLoop>();
      foreach (CurveLoop curveLoop2 in curveLoopList1)
      {
        bool flag2 = false;
        foreach (CurveLoop curveLoop in curveLoopList1)
        {
          if (CAM_Utils.CurveLoopContainsCurveLoop(curveLoop, curveLoop2))
          {
            flag2 = true;
            break;
          }
        }
        if (!flag2)
          curveLoopList2.Add(curveLoop2);
      }
      foreach (CurveLoop loop in curveLoopList2)
      {
        List<XYZ> source2 = new List<XYZ>();
        foreach (Curve curve in loop)
        {
          XYZ point1 = curve.GetEndPoint(0);
          XYZ point2c = curve.GetEndPoint(1);
          if (!source2.Any<XYZ>((Func<XYZ, bool>) (pt => pt.X.ApproximatelyEquals(point1.X, 1E-06) && pt.Y.ApproximatelyEquals(point1.Y, 1E-05) && pt.Z.ApproximatelyEquals(point1.Z, 1E-05))))
            source2.Add(curve.GetEndPoint(0));
          if (!source2.Any<XYZ>((Func<XYZ, bool>) (pt => pt.X.ApproximatelyEquals(point2c.X, 1E-06) && pt.Y.ApproximatelyEquals(point2c.Y, 1E-05) && pt.Z.ApproximatelyEquals(point2c.Z, 1E-05))))
            source2.Add(curve.GetEndPoint(1));
        }
        List<XYZ> list = source2.OrderBy<XYZ, double>((Func<XYZ, double>) (e => e.X)).ThenBy<XYZ, double>((Func<XYZ, double>) (p => p.Z)).ToList<XYZ>();
        double num1 = list.ElementAt<XYZ>(0).DistanceTo(list.ElementAt<XYZ>(2));
        double num2 = list.ElementAt<XYZ>(0).DistanceTo(list.ElementAt<XYZ>(1));
        XYZ xyz1 = this.LeftmostPointCopy(elem, loop, wallTransform);
        Parameter parameter = Utils.ElementUtils.Parameters.LookupParameter(elem, "TOLERANCE");
        double num3 = num2;
        double num4 = num1;
        double num5 = Utils.ElementUtils.Parameters.LookupParameter(elem, "DIM_LENGTH").AsDouble();
        Utils.ElementUtils.Parameters.LookupParameter(elem, "DIM_WIDTH").AsDouble();
        if (parameter != null)
        {
          double num6 = num5 - 2.0 * parameter.AsDouble();
        }
        double result1;
        double.TryParse($"{minLength:0.######0}", out result1);
        double result2;
        double.TryParse($"{maxLength:0.######0}", out result2);
        double result3;
        double.TryParse($"{minWidth:0.######0}", out result3);
        double result4;
        double.TryParse($"{maxWidth:0.######0}", out result4);
        insul.MinLength = double.IsNaN(result1) || result1 == 0.0 ? minLength : result1;
        insul.MaxLength = double.IsNaN(result2) || result2 == 0.0 ? maxLength : result2;
        insul.MaxWidth = double.IsNaN(result4) || result4 == 0.0 ? maxWidth : result4;
        insul.MinWidth = double.IsNaN(result3) || result3 == 0.0 ? minWidth : result3;
        string str1 = "";
        problems = str1;
        AutomaticPlacement.lengthAndWidthHolder lengthAndWidthHolder = new AutomaticPlacement.lengthAndWidthHolder();
        double totalWorkableLengthCopy1 = num3;
        double totalWorkableLengthCopy2 = num4;
        wall.Symbol.Family.Name.Contains("HORIZONTAL");
        AutomaticPlacement.lengthAndWidthHolder lengthAndWidth1_1 = new AutomaticPlacement.lengthAndWidthHolder();
        AutomaticPlacement.lengthAndWidthHolder lengthAndWidth1_2 = new AutomaticPlacement.lengthAndWidthHolder();
        string str2 = "";
        if (horizontalOption)
        {
          if (minLength > totalWorkableLengthCopy2)
          {
            string str3 = "DIM_LENGTH_MIN-INS";
            problems = str3;
            return false;
          }
          if (minWidth > totalWorkableLengthCopy1)
          {
            string str4 = "DIM_WIDTH_MIN-INS";
            problems = str4;
            return false;
          }
          if (!this.CalculationsForPlacement(totalWorkableLengthCopy1, maxWidth, minWidth, true, out lengthAndWidth1_2, out str2, bMetric))
          {
            problems = str2;
            return false;
          }
          if (!this.CalculationsForPlacement(totalWorkableLengthCopy2, maxLength, minLength, false, out lengthAndWidth1_1, out str2, bMetric))
          {
            problems = str2;
            return false;
          }
        }
        else
        {
          if (minLength > totalWorkableLengthCopy1)
          {
            string str5 = "DIM_LENGTH_MIN-INS";
            problems = str5;
            return false;
          }
          if (minWidth > totalWorkableLengthCopy2)
          {
            string str6 = "DIM_WIDTH_MIN-INS";
            problems = str6;
            return false;
          }
          if (!this.CalculationsForPlacement(totalWorkableLengthCopy2, maxWidth, minWidth, false, out lengthAndWidth1_1, out str2, bMetric))
          {
            problems = str2;
            return false;
          }
          if (!this.CalculationsForPlacement(totalWorkableLengthCopy1, maxLength, minLength, true, out lengthAndWidth1_2, out str2, bMetric))
          {
            problems = str2;
            return false;
          }
        }
        lengthAndWidthHolder.NumofLength = lengthAndWidth1_2.NumofLength;
        lengthAndWidthHolder.Lengths = lengthAndWidth1_2.Lengths;
        lengthAndWidthHolder.NumOfWidth = lengthAndWidth1_1.NumOfWidth;
        lengthAndWidthHolder.Widths = lengthAndWidth1_1.Widths;
        lengthAndWidthHolder.TotalLength = num3;
        lengthAndWidthHolder.TotalWidth = num4;
        XYZ xyz2 = xyz1;
        XYZ xyz3 = xyz1;
        PlacementUtilities.IsMirrored((Element) wall);
        double totalWidth = lengthAndWidthHolder.TotalWidth;
        double totalLength = lengthAndWidthHolder.TotalLength;
        PlanarFace frontFace = PlacementUtilities.GetFrontFace(elem);
        foreach (double width in lengthAndWidthHolder.Widths)
        {
          int num7 = 0;
          int num8 = 0;
          foreach (double length in lengthAndWidthHolder.Lengths)
          {
            insul.InsulLength = length;
            insul.InsulWidth = width;
            XYZ xyz4 = xyz3 - wallTransform.BasisX * insul.InsulWidth + wallTransform.BasisZ * insul.InsulLength;
            Tuple<XYZ, XYZ> points = new Tuple<XYZ, XYZ>(xyz3, xyz4);
            insul.Face = frontFace;
            ManualPlacementErrorType errors;
            counter = this.placingWithManualMethod(revitDoc, uidoc, insul, wallTransform, elem, insul.UnionedFace, insulationFamiyName, points, out duplicateErr, out errors);
            if ((errors & ManualPlacementErrorType.InsulationCutIntoMultiple) == ManualPlacementErrorType.InsulationCutIntoMultiple)
              problems = "CUT_INTO_MULTIPLE";
            if (counter == -1)
            {
              string str7 = "DIM_LENGTH-READ";
              problems = str7;
              return false;
            }
            if (counter == -2)
            {
              string str8 = "DIM_WIDTH-READ";
              problems = str8;
              return false;
            }
            if (counter == -3)
            {
              string str9 = "DIM_THICKNESS-READ";
              problems = str9;
              return false;
            }
            if (counter == -4)
              return false;
            totalLength -= length;
            if (totalLength > 0.0)
              xyz3 += wallTransform.BasisZ * length;
            ++num8;
          }
          totalLength = lengthAndWidthHolder.TotalLength;
          totalWidth -= width;
          if (totalWidth > 0.0)
          {
            double num9 = lengthAndWidthHolder.TotalWidth - totalWidth;
            xyz3 = xyz2 - wallTransform.BasisX * num9;
          }
          int num10 = num7 + 1;
        }
      }
    }
    catch (Exception ex)
    {
      throw ex;
    }
    return true;
  }

  private List<CurveLoop> projectedBoundingBox(List<CurveLoop> listOfCurveLoops, Document revitdoc)
  {
    List<CurveLoop> curveLoopList = new List<CurveLoop>();
    foreach (CurveLoop listOfCurveLoop in listOfCurveLoops)
    {
      BoundingBoxXYZ boundingBox = GeometryCreationUtilities.CreateExtrusionGeometry((IList<CurveLoop>) new List<CurveLoop>()
      {
        listOfCurveLoop
      }, XYZ.BasisY, 1.0).GetBoundingBox();
      XYZ xyz1 = boundingBox.Transform.OfPoint(boundingBox.Min);
      XYZ xyz2 = boundingBox.Transform.OfPoint(boundingBox.Max);
      double num1 = xyz2.X - xyz1.X;
      double num2 = xyz2.Z - xyz1.Z;
      XYZ xyz3 = xyz1 + num1 * XYZ.BasisX;
      XYZ xyz4 = xyz3 + num2 * XYZ.BasisZ;
      XYZ xyz5 = xyz1 + num2 * XYZ.BasisZ;
      XYZ xyz6 = xyz1;
      CurveLoop curveLoop = CurveLoop.Create((IList<Curve>) new List<Curve>()
      {
        (Curve) Line.CreateBound(xyz6, xyz5),
        (Curve) Line.CreateBound(xyz5, xyz4),
        (Curve) Line.CreateBound(xyz4, xyz3),
        (Curve) Line.CreateBound(xyz3, xyz6)
      });
      curveLoopList.Add(curveLoop);
    }
    return curveLoopList;
  }

  private void negativeError(string paramName, FamilySymbol fam)
  {
    new TaskDialog("Automatic Insulation Placement")
    {
      Title = "Automatic Insulation Placement - Negative Value",
      MainInstruction = $"Please check {paramName} on {fam.FamilyName} : {fam.Name}. It cannot be a negative value."
    }.Show();
  }

  private void zeroError(string paramName, FamilySymbol fam)
  {
    new TaskDialog("Automatic Insulation Placement")
    {
      Title = "Automatic Insulation Placement - Zero Value",
      MainInstruction = $"Please check {paramName} on {fam.FamilyName} : {fam.Name}. It cannot be zero."
    }.Show();
  }

  public bool CalculationsForPlacement(
    double totalWorkableLengthCopy,
    double maxWidth,
    double minWidth,
    bool length,
    out AutomaticPlacement.lengthAndWidthHolder lengthAndWidth1,
    out string value,
    bool bMetric = false)
  {
    AutomaticPlacement.lengthAndWidthHolder lengthAndWidthHolder = new AutomaticPlacement.lengthAndWidthHolder();
    double num1 = 0.0;
    int num2 = 0;
    List<double> doubleList = new List<double>();
    string str1 = "";
    double num3 = PlacementUtilities.RoundValue(totalWorkableLengthCopy, bMetric);
    double num4 = num3 % maxWidth;
    value = str1;
    int int32 = Convert.ToInt32(Math.Floor(num3 / maxWidth));
    if (num4 == 0.0)
    {
      for (int index = 0; index < int32; ++index)
      {
        doubleList.Add(PlacementUtilities.RoundValue(maxWidth, bMetric));
        ++num2;
      }
    }
    else if (num4 < minWidth)
    {
      int num5 = int32 - 1;
      for (int index = 0; index < num5; ++index)
      {
        doubleList.Add(PlacementUtilities.RoundValue(maxWidth, bMetric));
        ++num2;
      }
      double num6 = num1 + maxWidth * (double) num5;
      double num7 = num4 + maxWidth;
      if (num7 / 2.0 < minWidth)
      {
        string str2 = !length ? "DIM_WIDTH_MIN-ADJ" : "DIM_LENGTH_MIN-ADJ";
        value = str2;
        lengthAndWidth1 = lengthAndWidthHolder;
        return false;
      }
      doubleList.Add(PlacementUtilities.RoundValue(num3 - num6 - minWidth, bMetric));
      doubleList.Add(PlacementUtilities.RoundValue(minWidth, bMetric));
      num2 += 2;
      double num8 = num6 + num7;
    }
    else
    {
      for (int index = 0; index < int32; ++index)
      {
        doubleList.Add(PlacementUtilities.RoundValue(maxWidth, bMetric));
        ++num2;
      }
      double num9 = num1 + maxWidth * (double) int32;
      if (num3 - num9 >= minWidth)
      {
        doubleList.Add(PlacementUtilities.RoundValue(num3 - num9, bMetric));
        ++num2;
      }
    }
    if (length)
    {
      lengthAndWidthHolder.NumofLength = num2;
      lengthAndWidthHolder.Lengths = doubleList;
    }
    else
    {
      lengthAndWidthHolder.NumOfWidth = num2;
      lengthAndWidthHolder.Widths = doubleList;
    }
    lengthAndWidth1 = lengthAndWidthHolder;
    return true;
  }

  private int placingWithManualMethod(
    Document revitDoc,
    UIDocument uiDoc,
    Insulation insul,
    Transform wallTransform,
    Element wall,
    PlanarFace face,
    FamilySymbol insulSymbol,
    Tuple<XYZ, XYZ> points,
    out bool duplicateErr,
    out ManualPlacementErrorType errors)
  {
    bool bMetric = Utils.MiscUtils.MiscUtils.CheckMetricLengthUnit(revitDoc);
    errors = ManualPlacementErrorType.Success;
    duplicateErr = false;
    int num = 0;
    insul.ExpandedToMin = false;
    insul.ExpandedToMax = false;
    try
    {
      ManualPlacementErrorType mpe;
      Tuple<XYZ, XYZ> transformedClickPoints1 = PlacementUtilities.VerifyClickPoints(points, insul.UnionedFace, insul, out mpe);
      if (mpe == ManualPlacementErrorType.PointsOutsideLoop || mpe == ManualPlacementErrorType.InvalidDimensions)
        return num;
      double len = 0.0;
      double wid = 0.0;
      PointOrientation pointOrientation = PointOrientation.UpperLeft;
      MaxMinStatus mms1;
      XYZ lengthWidthAndOrigin = PlacementUtilities.GetLengthWidthAndOrigin(transformedClickPoints1.Item1, transformedClickPoints1, insul, wallTransform, bMetric, out len, out wid, out mms1, out ManualPlacementErrorType _, out pointOrientation);
      if (mms1 == MaxMinStatus.Reselect)
      {
        bool flag = false;
        while (!flag)
        {
          try
          {
            XYZ xyz = PlacementUtilities.RetrieveClickPoint(uiDoc);
            Tuple<XYZ, XYZ> clickPoints = new Tuple<XYZ, XYZ>(transformedClickPoints1.Item1, xyz);
            if (clickPoints.Item2 == null && num == 0)
              return num;
            Tuple<XYZ, XYZ> transformedClickPoints2 = PlacementUtilities.VerifyClickPoints(clickPoints, insul.UnionedFace, insul, out mpe);
            switch (mpe)
            {
              case ManualPlacementErrorType.PointsOutsideLoop:
                goto label_14;
              case ManualPlacementErrorType.InvalidDimensions:
                return num;
              default:
                MaxMinStatus mms2;
                lengthWidthAndOrigin = PlacementUtilities.GetLengthWidthAndOrigin(transformedClickPoints1.Item1, transformedClickPoints2, insul, wallTransform, bMetric, out len, out wid, out mms2, out ManualPlacementErrorType _, out pointOrientation);
                if (mms2 != MaxMinStatus.Reselect)
                {
                  flag = true;
                  continue;
                }
                continue;
            }
          }
          catch (OperationCanceledException ex)
          {
            if (num == 0)
              return num;
          }
        }
label_14:
        if (mpe == ManualPlacementErrorType.PointsOutsideLoop)
          return num;
      }
      if (len == 0.0 || wid == 0.0)
        return num;
      insul.InsulLength = len;
      insul.InsulWidth = wid;
      ManualPlacementErrorType m1;
      if (insul.ExpandedToMin && (!PlacementUtilities.ValidateFinalDimensions(transformedClickPoints1.Item1, insul.InsulLength, insul.InsulWidth, insul, pointOrientation, out m1) || m1 == ManualPlacementErrorType.InsulationCutIntoMultiple))
        return num;
      ManualPlacementErrorType m2;
      PlacementUtilities.CheckSplitInsulation(transformedClickPoints1.Item1, insul.InsulLength, insul.InsulWidth, insul, pointOrientation, out m2);
      return m2 == ManualPlacementErrorType.InsulationCutIntoMultiple || !insul.ExpandedToMin && !insul.ExpandedToMax && mpe == ManualPlacementErrorType.InsulationCutIntoMultiple ? num : num + PlacementUtilities.SetInsulDimensionsandPlaceCopyForAutomated(insul, wall, lengthWidthAndOrigin, insulSymbol, face, out FamilyInstance _, out duplicateErr, out errors);
    }
    catch (OperationCanceledException ex)
    {
      return num;
    }
  }

  private XYZ LeftmostPointCopy(Element elem, CurveLoop loop, Transform wallTransform)
  {
    Utils.ElementUtils.Parameters.LookupParameter(elem, "TOLERANCE");
    if (!PlacementUtilities.IsMirrored(elem))
    {
      XYZ basisY = XYZ.BasisY;
    }
    else
    {
      XYZ xyz1 = -XYZ.BasisY;
    }
    XYZ xyz2 = new XYZ();
    List<XYZ> source = new List<XYZ>();
    foreach (Curve curve in loop)
    {
      XYZ point1 = curve.GetEndPoint(0);
      XYZ point2c = curve.GetEndPoint(1);
      if (!source.Any<XYZ>((Func<XYZ, bool>) (pt => pt.X.Equals(point1.X) && pt.Y.Equals(point1.Y) && pt.Z.Equals(point1.Z))))
        source.Add(curve.GetEndPoint(0));
      if (!source.Any<XYZ>((Func<XYZ, bool>) (pt => pt.X.Equals(point2c.X) && pt.Y.Equals(point2c.Y) && pt.Z.Equals(point2c.Z))))
        source.Add(curve.GetEndPoint(1));
    }
    double x = double.MinValue;
    double y = double.MinValue;
    double z = double.MaxValue;
    foreach (XYZ xyz3 in source.ToList<XYZ>())
    {
      if (xyz3.X > x)
        x = xyz3.X;
      if (xyz3.Y > y)
        y = xyz3.Y;
      if (xyz3.Z < z)
        z = xyz3.Z;
    }
    XYZ point = new XYZ(x, y, z);
    return wallTransform.OfPoint(point);
  }

  private IEnumerable<Element> filterelements(
    Document revitdoc,
    AutomaticPlacement.OptionPicked scope,
    List<ElementId> elementIds = null)
  {
    IEnumerable<Element> second = (IEnumerable<Element>) null;
    ElementMulticategoryFilter filter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
    {
      BuiltInCategory.OST_StructuralFraming,
      BuiltInCategory.OST_Assemblies
    });
    FilteredElementCollector elementCollector;
    switch (scope)
    {
      case AutomaticPlacement.OptionPicked.WholeModel:
        elementCollector = new FilteredElementCollector(revitdoc);
        break;
      case AutomaticPlacement.OptionPicked.ActiveView:
        elementCollector = new FilteredElementCollector(revitdoc, revitdoc.ActiveView.Id);
        break;
      case AutomaticPlacement.OptionPicked.Selection:
        if (elementIds == null)
          return (IEnumerable<Element>) null;
        elementCollector = new FilteredElementCollector(revitdoc, (ICollection<ElementId>) elementIds);
        second = (IEnumerable<Element>) ((IEnumerable<Element>) new FilteredElementCollector(revitdoc, (ICollection<ElementId>) elementIds).OfCategory(BuiltInCategory.OST_Assemblies).OfType<AssemblyInstance>()).Select<Element, Element>((Func<Element, Element>) (aInst => (aInst as AssemblyInstance).GetStructuralFramingElement())).ToList<Element>();
        break;
      default:
        elementCollector = new FilteredElementCollector(revitdoc);
        break;
    }
    IEnumerable<Element> elements = (IEnumerable<Element>) elementCollector.WherePasses((ElementFilter) filter).OfClass(typeof (FamilyInstance));
    if (second != null)
      elements = elements.Union<Element>(second);
    return (IEnumerable<Element>) elements.Where<Element>((Func<Element, bool>) (e => Utils.SelectionUtils.SelectionUtils.CheckConstructionProduct(e))).ToList<Element>();
  }

  public string InsulationFamily
  {
    get => this._insulationFamily;
    set => this._insulationFamily = value;
  }

  public enum OptionPicked
  {
    WholeModel,
    ActiveView,
    Selection,
  }

  public class Filter : ISelectionFilter
  {
    public bool AllowElement(Element e)
    {
      return e is FamilyInstance familyInstance && familyInstance.Category.Id.Equals((object) new ElementId(BuiltInCategory.OST_StructuralFraming)) && Utils.SelectionUtils.SelectionUtils.CheckConstructionProduct(e);
    }

    public bool AllowReference(Reference refer, XYZ point) => false;
  }

  public class lengthAndWidthHolder
  {
    private int _numOfLength;
    private int _numOfWidth;
    private List<double> _lengths;
    private List<double> _widths;
    private double _totalLength;
    private double _totalWidth;

    public int NumofLength
    {
      get => this._numOfLength;
      set => this._numOfLength = value;
    }

    public int NumOfWidth
    {
      get => this._numOfWidth;
      set => this._numOfWidth = value;
    }

    public List<double> Lengths
    {
      get => this._lengths;
      set => this._lengths = value;
    }

    public List<double> Widths
    {
      get => this._widths;
      set => this._widths = value;
    }

    public double TotalLength
    {
      get => this._totalLength;
      set => this._totalLength = value;
    }

    public double TotalWidth
    {
      get => this._totalWidth;
      set => this._totalWidth = value;
    }
  }
}
