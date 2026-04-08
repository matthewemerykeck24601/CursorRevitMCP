// Decompiled with JetBrains decompiler
// Type: EDGE.SchedulingTools.ControlNumberIncrementor
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using EDGE.IUpdaters.ModelLocking;
using EDGE.SchedulingTools.ControlNumberIncrementorTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Utils.ElementUtils;
using Utils.Exceptions;
using Utils.WorkSharingUtils;

#nullable disable
namespace EDGE.SchedulingTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class ControlNumberIncrementor : IExternalCommand
{
  private Document revitDoc;
  private UIDocument uidoc;
  private bool is4Digit;
  private string cnName = "CONTROL_NUMBER";
  private const string appName = "Control Number Incrementor";
  private const string appError = "Error:  There was a problem updating the selected element's CONTROl_NUMBER parameter value.";
  private const string nullExError = "Error:  The selected element is invalid for CONTROL_NUMBER incrementing.";

  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    this.uidoc = commandData.Application.ActiveUIDocument;
    this.revitDoc = this.uidoc.Document;
    string str1 = "Control Number Incrementor Tool";
    if (this.revitDoc.IsFamilyDocument)
    {
      new TaskDialog("Family Editor")
      {
        AllowCancellation = false,
        CommonButtons = ((TaskDialogCommonButtons) 1),
        MainInstruction = (str1 + " Must be run in the Project Environment"),
        MainContent = $"You are currently in the family editor, {str1} must be run in the project environment.  Please return to the project environment or open a project before running this tool.",
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
      }.Show();
      return (Result) 1;
    }
    string startingNum = "";
    string str2 = "";
    string str3 = "";
    Element pickedElement = (Element) null;
    ICollection<ElementId> referenceIdList = (ICollection<ElementId>) new List<ElementId>();
    if (!ModelLockingUtils.ShowPermissionsDialog(this.revitDoc, ModelLockingToolPermissions.ControlNumberIncrementor))
      return (Result) 1;
    List<Element> list = new FilteredElementCollector(this.revitDoc).OfClass(typeof (FamilyInstance)).OfCategory(BuiltInCategory.OST_StructuralFraming).ToList<Element>();
    if (list.Count == 0)
    {
      new TaskDialog("Control Number Incrementor")
      {
        MainInstruction = "No Pieces Within Model",
        MainContent = "Control Number Incrementor requires strucural framing elements in the model."
      }.Show();
      return (Result) 1;
    }
    if (Parameters.LookupParameter(list.First<Element>(), "CONTROL_NUMBER") == null)
    {
      new TaskDialog("Control Number Incrementor")
      {
        MainInstruction = "Control Number Parameter Does Not Exist",
        MainContent = "Please run project shared parameters tool to add the control number parameter to structural framing elements and run the tool again."
      }.Show();
      return (Result) 1;
    }
    TaskDialog taskDialog1 = new TaskDialog("Control Number Incrementor");
    taskDialog1.MainInstruction = "Control Number Incrementor";
    taskDialog1.MainContent = "Select the scope for Control Number Incrementor.";
    taskDialog1.AddCommandLink((TaskDialogCommandLinkId) 1001, "Whole Model");
    taskDialog1.AddCommandLink((TaskDialogCommandLinkId) 1002, "Active View");
    taskDialog1.AddCommandLink((TaskDialogCommandLinkId) 1003, "Selection Group");
    taskDialog1.AddCommandLink((TaskDialogCommandLinkId) 1004, "Manually Assign");
    taskDialog1.CommonButtons = (TaskDialogCommonButtons) 8;
    TaskDialogResult taskDialogResult1 = taskDialog1.Show();
    if (taskDialogResult1 != 1001 && taskDialogResult1 != 1002 && taskDialogResult1 != 1003 && taskDialogResult1 != 1004)
      return (Result) 1;
    bool flag1 = taskDialogResult1 == 1004;
    List<Element> elements1 = new List<Element>();
    List<Element> assignedElements = new List<Element>();
    List<Element> unassignedElements = new List<Element>();
    if (!flag1)
    {
      if (taskDialogResult1 == 1001)
      {
        elements1 = new List<Element>((IEnumerable<Element>) list);
        if (!this.ContainsValidSFElements(elements1, out unassignedElements, out assignedElements))
        {
          new TaskDialog("Control Number Incrementor")
          {
            MainInstruction = "No Valid Pieces Within Scope",
            MainContent = "There are no pieces within the selected scope that are valid for assigning control number."
          }.Show();
          return (Result) 1;
        }
        elements1.Clear();
        elements1.AddRange((IEnumerable<Element>) assignedElements);
        elements1.AddRange((IEnumerable<Element>) unassignedElements);
      }
      else if (taskDialogResult1 == 1002)
      {
        View activeView = this.revitDoc.ActiveView;
        if (activeView == null)
        {
          new TaskDialog("Control Number Incrementor")
          {
            MainInstruction = "Invalid Active View",
            MainContent = "Unable acess active view. Make sure a valid view is active and try again."
          }.Show();
          return (Result) 1;
        }
        elements1 = new FilteredElementCollector(this.revitDoc, activeView.Id).OfClass(typeof (FamilyInstance)).OfCategory(BuiltInCategory.OST_StructuralFraming).ToList<Element>();
        if (!this.ContainsValidSFElements(elements1, out unassignedElements, out assignedElements))
        {
          new TaskDialog("Control Number Incrementor")
          {
            MainInstruction = "No Valid Pieces Within Scope",
            MainContent = "There are no pieces within the selected scope that are valid for assigning control number."
          }.Show();
          return (Result) 1;
        }
        elements1.Clear();
        elements1.AddRange((IEnumerable<Element>) assignedElements);
        elements1.AddRange((IEnumerable<Element>) unassignedElements);
      }
      else if (taskDialogResult1 == 1003)
      {
        ICollection<ElementId> elementIds = this.uidoc.Selection.GetElementIds();
        List<Element> elements2 = new List<Element>();
        foreach (ElementId id in (IEnumerable<ElementId>) elementIds)
        {
          Element element = this.revitDoc.GetElement(id);
          if (element != null)
            elements2.Add(element);
        }
        if (!this.ContainsValidSFElements(elements2, out unassignedElements, out assignedElements))
        {
          IList<Reference> referenceList;
          try
          {
            referenceList = this.uidoc.Selection.PickObjects((ObjectType) 1, (ISelectionFilter) new OnlySructuralFramingControlNumberFilter());
          }
          catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
          {
            return (Result) 1;
          }
          List<Element> elements3 = new List<Element>();
          foreach (Reference reference in (IEnumerable<Reference>) referenceList)
          {
            ElementId elementId = reference.ElementId;
            if (!(elementId == (ElementId) null))
            {
              Element element = this.revitDoc.GetElement(elementId);
              if (element != null)
                elements3.Add(element);
            }
          }
          if (!this.ContainsValidSFElements(elements3, out unassignedElements, out assignedElements))
          {
            new TaskDialog("Control Number Incrementor")
            {
              MainInstruction = "No Valid Pieces Selected",
              MainContent = "Pieces selected are not valid for assigning control number."
            }.Show();
            return (Result) 1;
          }
        }
        elements1.Clear();
        elements1.AddRange((IEnumerable<Element>) assignedElements);
        elements1.AddRange((IEnumerable<Element>) unassignedElements);
      }
      if (elements1.Count == 0)
      {
        new TaskDialog("Control Number Incrementor")
        {
          MainInstruction = "No Valid Pieces Within Scope",
          MainContent = "There are no pieces within the selected scope that are valid for assigning control numbers."
        }.Show();
        return (Result) 1;
      }
      if (assignedElements.Count > 0)
      {
        string str4 = "the selection";
        if (taskDialogResult1 == 1001)
          str4 = "the model";
        if (taskDialogResult1 == 1002)
          str4 = "the active view";
        TaskDialogResult taskDialogResult2 = new TaskDialog("Control Number Incrementor")
        {
          MainInstruction = "Clear Control Numbers?",
          MainContent = $"Would you like to clear the control number values for all pieces within {str4}?",
          CommonButtons = ((TaskDialogCommonButtons) 14)
        }.Show();
        if (taskDialogResult2 == 2)
          return (Result) 1;
        if (taskDialogResult2 == 6)
        {
          using (Transaction transaction = new Transaction(this.revitDoc, "Clear Control Numbers"))
          {
            int num1 = (int) transaction.Start();
            bool flag2 = true;
            if (this.revitDoc.IsWorkshared)
            {
              List<ElementId> elementIdList = new List<ElementId>();
              foreach (Element element1 in assignedElements)
              {
                elementIdList.Add(element1.Id);
                FamilyInstance familyInstance = element1 as FamilyInstance;
                if (familyInstance.IsWarpableProduct() && familyInstance.HasSubComponents())
                {
                  foreach (ElementId id in familyInstance.GetAllSubcomponents().ToList<ElementId>())
                  {
                    Element element2 = this.revitDoc.GetElement(id);
                    if (element2 != null && element2.Category.Id.IntegerValue == -2001320)
                      elementIdList.Add(element2.Id);
                  }
                }
              }
              if (CheckElementsOwnership.CheckOwnership(this.revitDoc, (ICollection<ElementId>) assignedElements.Select<Element, ElementId>((Func<Element, ElementId>) (x => x.Id)).ToList<ElementId>(), out ICollection<ElementId> _, out ICollection<ElementId> _, out ICollection<ElementId> _))
              {
                new TaskDialog("Control Number Incrementor")
                {
                  MainInstruction = "Unable to Clear Control Numbers",
                  MainContent = "One or more pieces are owned by another user. Coordinate with other project members to relinquish elements and reload latest from the central model."
                }.Show();
                int num2 = (int) transaction.RollBack();
                flag2 = false;
              }
            }
            if (flag2)
            {
              foreach (Element element in assignedElements)
              {
                bool readOnly;
                if (this.AssignControlNumber(element, "", out readOnly))
                  unassignedElements.Add(element);
                else if (!readOnly)
                {
                  new TaskDialog("Control Number Incrementor")
                  {
                    MainInstruction = "Unable to Clear Control Numbers",
                    MainContent = "Unable to clear control number value. Error occured clearing values."
                  }.Show();
                  if (transaction.HasStarted() && !transaction.HasEnded())
                  {
                    int num3 = (int) transaction.RollBack();
                  }
                  flag2 = false;
                  break;
                }
              }
              if (flag2)
              {
                int num4 = (int) transaction.Commit();
              }
            }
          }
        }
      }
      if (unassignedElements.Count == 0)
      {
        new TaskDialog("Control Number Incrementor")
        {
          MainInstruction = "No Valid Piece in Scope.",
          MainContent = "There are currently no pieces in the scope that do not already have a control number value. Tool cancelled."
        }.Show();
        return (Result) 0;
      }
      int num = int.MinValue;
      List<ElementId> source = new List<ElementId>();
      foreach (Element elem in list)
      {
        string parameterAsString = Parameters.GetParameterAsString(elem, "CONTROL_NUMBER");
        if (!string.IsNullOrWhiteSpace(parameterAsString))
        {
          string controlNumber1;
          string prefix;
          string suffix;
          int controlNumber2 = this.ParseControlNumber(parameterAsString, out controlNumber1, out prefix, out suffix);
          if (controlNumber2 == -1)
            source.Add(elem.Id);
          else if (controlNumber2 > num)
          {
            num = controlNumber2;
            startingNum = controlNumber1;
            str2 = prefix;
            str3 = suffix;
          }
        }
      }
      if (source.Count > 0)
      {
        TaskDialog taskDialog2 = new TaskDialog("Control Number Incrementor");
        taskDialog2.MainInstruction = "Incorrect Control Number Values";
        taskDialog2.MainContent = "One or more pieces have incorrectly formatted control number values.";
        TaskDialog taskDialog3 = taskDialog2;
        string expandedContent1 = taskDialog3.ExpandedContent;
        int integerValue = source.First<ElementId>().IntegerValue;
        string str5 = integerValue.ToString();
        taskDialog3.ExpandedContent = $"{expandedContent1}Element Ids:\n{str5}";
        for (int index = 1; index < source.Count<ElementId>(); ++index)
        {
          TaskDialog taskDialog4 = taskDialog2;
          string expandedContent2 = taskDialog4.ExpandedContent;
          integerValue = source[index].IntegerValue;
          string str6 = integerValue.ToString();
          taskDialog4.ExpandedContent = $"{expandedContent2};\n{str6}";
        }
        taskDialog2.Show();
      }
    }
    using (Transaction transaction = new Transaction(this.revitDoc, "Control Number Incrementor"))
    {
      try
      {
        ControlNumberIncrementorWindow incrementorWindow = new ControlNumberIncrementorWindow(Process.GetCurrentProcess().MainWindowHandle, startingNum, str2, str3);
        incrementorWindow.ShowDialog();
        if (string.IsNullOrWhiteSpace(incrementorWindow.controlNumber))
          return (Result) 0;
        int int32 = Convert.ToInt32(incrementorWindow.controlNumber);
        if (!string.IsNullOrWhiteSpace(incrementorWindow.prefix))
          str2 = incrementorWindow.prefix;
        if (!string.IsNullOrWhiteSpace(incrementorWindow.suffix))
          str3 = incrementorWindow.suffix;
        if (incrementorWindow.controlNumber.Trim().Length > 3)
          this.is4Digit = true;
        if (flag1)
          return this.ManualSelectionIncrementation(referenceIdList, pickedElement, int32, str2, str3);
        if (this.revitDoc.IsWorkshared)
        {
          List<ElementId> ElementIds = new List<ElementId>();
          foreach (Element element3 in unassignedElements)
          {
            ElementIds.Add(element3.Id);
            FamilyInstance familyInstance = element3 as FamilyInstance;
            if (familyInstance.IsWarpableProduct() && familyInstance.HasSubComponents())
            {
              foreach (ElementId id in familyInstance.GetAllSubcomponents().ToList<ElementId>())
              {
                Element element4 = this.revitDoc.GetElement(id);
                if (element4 != null && element4.Category.Id.IntegerValue == -2001320)
                  ElementIds.Add(element4.Id);
              }
            }
          }
          if (CheckElementsOwnership.CheckOwnership(this.revitDoc, (ICollection<ElementId>) ElementIds, out ICollection<ElementId> _, out ICollection<ElementId> _, out ICollection<ElementId> _))
          {
            new TaskDialog("Control Number Incrementor")
            {
              MainInstruction = "Unable to Automatically Assign Control Numbers",
              MainContent = "One or more pieces are owned by another user. Coordinate with other project members to relinquish elements and reload latest from the central model."
            }.Show();
            int num = (int) transaction.RollBack();
          }
        }
        this.AutomaticSelectionIncrementation(unassignedElements, int32, str2, str3);
        return (Result) 0;
      }
      catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
      {
        return (Result) 0;
      }
      catch (Exception ex)
      {
        if (pickedElement is AssemblyInstance)
        {
          message = "Error: The selected Element is an Assembly. The CONTROL_NUMBER Parameter cannot be updated for Assemblies.";
          return (Result) 0;
        }
        if (ex.ToString().Contains("Object reference not set to an instance of an object."))
        {
          if (transaction.GetStatus() == TransactionStatus.Started)
          {
            int num = (int) transaction.RollBack();
          }
          message = "Error:  The selected element is invalid for CONTROL_NUMBER incrementing.";
          return (Result) 0;
        }
        if (ex.ToString().Contains("The parameter is read"))
        {
          message = "Error: The CONTROL_NUMBER Parameter is read-only.";
          return (Result) 0;
        }
        if (transaction.GetStatus() == TransactionStatus.Started)
        {
          int num5 = (int) transaction.RollBack();
        }
        message = $"Error:  There was a problem updating the selected element's CONTROl_NUMBER parameter value.{Environment.NewLine}{Environment.NewLine}{ex?.ToString()}";
        return (Result) 0;
      }
    }
  }

  private void AutomaticSelectionIncrementation(
    List<Element> scopeList,
    int newNumber,
    string prefixValue,
    string suffixValue)
  {
    List<ElementId> elementIdList = new List<ElementId>();
    scopeList = scopeList.OrderBy<Element, int>((Func<Element, int>) (e => e.Id.IntegerValue)).ToList<Element>();
    --newNumber;
    using (Transaction transaction = new Transaction(this.revitDoc, "Increment Control Number"))
    {
      int num1 = (int) transaction.Start();
      HashSet<int> currentControlNumbers = this.GetCurrentControlNumbers();
      List<ElementId> source = new List<ElementId>();
      bool flag = true;
      foreach (Element scope in scopeList)
      {
        if (elementIdList.Contains(scope.Id))
        {
          new TaskDialog("Control Number Incrementor")
          {
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
            MainInstruction = "Member Already Incremented",
            MainContent = "You have already selected this member once during this command.  Control Number will not be incremented."
          }.Show();
        }
        else
        {
          ++newNumber;
          if (flag && currentControlNumbers.Contains(newNumber))
          {
            if (new TaskDialog("Control Number Incrementor")
            {
              MainInstruction = "Control Number Warning",
              MainContent = "The selected starting value resulted in the tool attempting to assign a control number that is already present in the model. Would you like to use the next available control number instead?",
              CommonButtons = ((TaskDialogCommonButtons) 10)
            }.Show() != 6)
            {
              int num2 = (int) transaction.RollBack();
              return;
            }
            flag = false;
          }
          while (currentControlNumbers.Contains(newNumber))
            ++newNumber;
          string format = this.is4Digit ? "{0:0000}" : "{0:000}";
          bool readOnly;
          if (!this.AssignControlNumber(scope, prefixValue + string.Format(format, (object) newNumber) + suffixValue, out readOnly))
          {
            if (!readOnly)
              source.Add(scope.Id);
            else
              --newNumber;
          }
          if (!elementIdList.Contains(scope.Id))
            elementIdList.Add(scope.Id);
        }
      }
      TaskDialog taskDialog1 = new TaskDialog("Control Number Incrementor");
      if (source.Count == 0)
      {
        taskDialog1.MainInstruction = "Control Numbers Successfully Assigned";
        taskDialog1.MainContent = "All pieces in the selected scope that did not have a control number assigned have been assigned a new control number value";
        int num3 = (int) transaction.Commit();
      }
      else if (source.Count == scopeList.Count)
      {
        taskDialog1.MainContent = "Control Number Assignment Failed";
        taskDialog1.MainContent = "No pieces have been edited.";
        int num4 = (int) transaction.RollBack();
      }
      else
      {
        taskDialog1.MainInstruction = "Control Numbers Successfully Assigned";
        taskDialog1.MainInstruction = "Control number assigned to some pieces in the selected scope. One or more pieces in the scope failed to assign a control number value. Expend for more information.";
        TaskDialog taskDialog2 = taskDialog1;
        string expandedContent1 = taskDialog2.ExpandedContent;
        int integerValue = source.First<ElementId>().IntegerValue;
        string str1 = integerValue.ToString();
        taskDialog2.ExpandedContent = $"{expandedContent1}Failed Element Ids:\n{str1}";
        for (int index = 1; index < source.Count<ElementId>(); ++index)
        {
          TaskDialog taskDialog3 = taskDialog1;
          string expandedContent2 = taskDialog3.ExpandedContent;
          integerValue = source[index].IntegerValue;
          string str2 = integerValue.ToString();
          taskDialog3.ExpandedContent = $"{expandedContent2};\n{str2}";
        }
        int num5 = (int) transaction.Commit();
      }
      taskDialog1.Show();
    }
  }

  private Result ManualSelectionIncrementation(
    ICollection<ElementId> referenceIdList,
    Element pickedElement,
    int startingNumber,
    string prefixValue,
    string suffixValue)
  {
    HashSet<int> currentControlNumbers = this.GetCurrentControlNumbers();
    int num1 = startingNumber;
    string format = this.is4Digit ? "{0:0000}" : "{0:000}";
    while (true)
    {
      string newValue;
      List<ElementId> ElementIds;
      do
      {
        do
        {
          newValue = prefixValue + string.Format(format, (object) num1) + suffixValue;
          ISelectionFilter iselectionFilter = (ISelectionFilter) new OnlySructuralFramingControlNumberFilter(referenceIdList);
          Reference reference;
          try
          {
            reference = this.uidoc.Selection.PickObject((ObjectType) 1, iselectionFilter, "Assign Control Number - " + newValue);
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
          pickedElement = this.revitDoc.GetElement(reference);
        }
        while (pickedElement == null);
        if (pickedElement.Document.IsWorkshared)
          ElementIds = new List<ElementId>();
        else
          goto label_21;
      }
      while (!(pickedElement is FamilyInstance familyInstance));
      if (familyInstance.IsWarpableProduct())
      {
        if (familyInstance.HasSuperComponent() && familyInstance.SuperComponent is FamilyInstance superComponent)
        {
          familyInstance = superComponent;
          ElementIds.Add(superComponent.Id);
        }
        ElementIds.Add(familyInstance.Id);
        if (familyInstance.HasSubComponents())
        {
          foreach (ElementId id in familyInstance.GetAllSubcomponents().ToList<ElementId>())
          {
            Element element = this.revitDoc.GetElement(id);
            if (element != null && element.Category.Id.IntegerValue == -2001320)
              ElementIds.Add(element.Id);
          }
        }
      }
      else
        ElementIds.Add(pickedElement.Id);
      if (CheckElementsOwnership.CheckOwnership(pickedElement.Document, (ICollection<ElementId>) ElementIds, out ICollection<ElementId> _, out ICollection<ElementId> _, out ICollection<ElementId> _))
      {
        new TaskDialog("Control Number Incrementor")
        {
          MainInstruction = "Unable to Assign Control Number",
          MainContent = "The selected piece is owned by another user. Coordinate with other project members to relinquish the element and reload latest from the central model."
        }.Show();
        continue;
      }
label_21:
      if (currentControlNumbers.Contains(num1))
      {
        int num2 = num1;
        while (currentControlNumbers.Contains(num1))
          ++num1;
        new TaskDialog("Control Number Incrementor")
        {
          MainContent = $"Model already contains an element with the next control number: {num2.ToString()}.  The next available number, {num1.ToString()}, will be used."
        }.Show();
        newValue = prefixValue + string.Format(format, (object) num1) + suffixValue;
      }
      using (Transaction transaction = new Transaction(this.revitDoc, "Control Number Assignment - " + newValue))
      {
        int num3 = (int) transaction.Start();
        if (this.AssignControlNumber(pickedElement, newValue, out bool _))
        {
          ++num1;
          referenceIdList.Add(pickedElement.Id);
          int num4 = (int) transaction.Commit();
        }
        else
        {
          new TaskDialog("Control Number Incrementor")
          {
            MainInstruction = "Failed Update Control Number",
            MainContent = "Unable to update the control number parameter for the selected value."
          }.Show();
          int num5 = (int) transaction.RollBack();
        }
      }
    }
  }

  private HashSet<int> GetCurrentControlNumbers()
  {
    HashSet<int> currentControlNumbers = new HashSet<int>();
    if (this.revitDoc == null)
      return currentControlNumbers;
    foreach (string str in new FilteredElementCollector(this.revitDoc).OfCategory(BuiltInCategory.OST_StructuralFraming).OfClass(typeof (FamilyInstance)).ToElements().Select<Element, string>((Func<Element, string>) (s => Parameters.GetParameterAsString(s, "CONTROL_NUMBER"))))
    {
      if (!string.IsNullOrWhiteSpace(str) && !string.IsNullOrWhiteSpace(str))
      {
        int controlNumber = this.ParseControlNumber(str, out string _, out string _, out string _);
        if (controlNumber != -1 && !currentControlNumbers.Contains(controlNumber))
          currentControlNumbers.Add(controlNumber);
      }
    }
    return currentControlNumbers;
  }

  private bool ContainsValidSFElements(
    List<Element> elements,
    out List<Element> unassignedElements,
    out List<Element> assignedElements)
  {
    unassignedElements = new List<Element>();
    assignedElements = new List<Element>();
    foreach (Element element in elements)
    {
      if (element.Category.Id.IntegerValue == -2001320 && element is FamilyInstance familyInstance1)
      {
        if (familyInstance1.IsWarpableProduct() && familyInstance1.HasSuperComponent())
        {
          Element superComponent = familyInstance1.SuperComponent;
          if (superComponent == null || superComponent.Category.Id.IntegerValue == -2001320 && !(superComponent is FamilyInstance familyInstance1))
            continue;
        }
        if (string.IsNullOrWhiteSpace(Parameters.GetParameterAsString((Element) familyInstance1, "CONTROL_NUMBER")))
        {
          if (!unassignedElements.Select<Element, ElementId>((Func<Element, ElementId>) (x => x.Id)).Contains<ElementId>(familyInstance1.Id))
            unassignedElements.Add((Element) familyInstance1);
        }
        else if (!assignedElements.Select<Element, ElementId>((Func<Element, ElementId>) (x => x.Id)).Contains<ElementId>(familyInstance1.Id))
          assignedElements.Add((Element) familyInstance1);
      }
    }
    return unassignedElements.Count + assignedElements.Count > 0;
  }

  private bool AssignControlNumber(Element element, string newValue, out bool readOnly)
  {
    readOnly = false;
    if (!(element is FamilyInstance familyInstance))
      return false;
    newValue = newValue.Trim();
    List<Element> elementList = new List<Element>();
    if (familyInstance.IsWarpableProduct())
    {
      if (familyInstance.HasSuperComponent() && familyInstance.SuperComponent is FamilyInstance superComponent)
      {
        familyInstance = superComponent;
        elementList.Add((Element) superComponent);
      }
      elementList.Add((Element) familyInstance);
      if (familyInstance.HasSubComponents())
      {
        foreach (ElementId id in familyInstance.GetAllSubcomponents().ToList<ElementId>())
        {
          Element element1 = this.revitDoc.GetElement(id);
          if (element1 != null && element1.Category.Id.IntegerValue == -2001320)
            elementList.Add(element1);
        }
      }
    }
    else
      elementList.Add((Element) familyInstance);
    try
    {
      foreach (Element element2 in elementList)
      {
        Parameter parameter = Parameters.LookupParameter(element, "CONTROL_NUMBER");
        if (parameter == null)
          return false;
        if (parameter.IsReadOnly)
        {
          readOnly = true;
          return false;
        }
        parameter.Set(newValue);
      }
    }
    catch (Exception ex)
    {
      return false;
    }
    return true;
  }

  private int ParseControlNumber(
    string value,
    out string controlNumber,
    out string prefix,
    out string suffix)
  {
    controlNumber = string.Empty;
    prefix = string.Empty;
    suffix = string.Empty;
    Match match = new Regex("^([^0-9]*)([0-9]*)([^0-9]*)$").Match(value);
    if (match.Success)
    {
      prefix = match.Groups[1].Value;
      suffix = match.Groups[3].Value;
      controlNumber = match.Groups[2].Value;
      int result = -1;
      if (int.TryParse(controlNumber, out result))
        return result;
    }
    return -1;
  }
}
