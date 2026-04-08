// Decompiled with JetBrains decompiler
// Type: EDGE.GeometryTools.VoidHostingUpdater
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using EDGE.IUpdaters.ModelLocking;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Utils.AdminUtils;
using Utils.AssemblyUtils;
using Utils.CollectionUtils;
using Utils.ElementUtils;
using Utils.Exceptions;
using Utils.SelectionUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.GeometryTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class VoidHostingUpdater : IExternalCommand
{
  private List<Element> unprocessedVoids = new List<Element>();

  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document revitDoc = activeUiDocument.Document;
    Application application = activeUiDocument.Application.Application;
    if (revitDoc.IsFamilyDocument)
    {
      new TaskDialog("Family Editor")
      {
        AllowCancellation = false,
        CommonButtons = ((TaskDialogCommonButtons) 1),
        MainInstruction = "Void Hosting Updater must be run in the Project Environment",
        MainContent = "You are currently in the family editor, Void Hosting Updater must be run in the project environment.  Please return to the project environment or open a project before running this tool.",
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
      }.Show();
      return (Result) 1;
    }
    string str1 = "Void Hosting Updater";
    string str2 = "Success:  Voids have been updated with the appropriate parameter values";
    string str3 = "Error:  There was an error updating parameters for the elements.";
    HashSet<string> stringSet1 = new HashSet<string>();
    if (!ModelLockingUtils.ShowPermissionsDialog(revitDoc, ModelLockingToolPermissions.VoidHosting))
      return (Result) 1;
    using (Transaction transaction = new Transaction(revitDoc, "Update Void Parameters"))
    {
      try
      {
        App.DialogSwitches.SuspendModelLockingforOperation = true;
        TaskDialog taskDialog = new TaskDialog("Void Hosting Updater");
        List<ElementId> list = Components.GetVoidIds(revitDoc).ToList<ElementId>();
        if (list.Count<ElementId>() == 0)
        {
          new TaskDialog("No Voids Found")
          {
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
            MainInstruction = "There are currently no voids in this model. The Void Hosting Updater function will not do anything."
          }.Show();
          return (Result) 1;
        }
        int voidCount = 0;
        taskDialog.Id = "ID_VoidHosting_Updater";
        taskDialog.MainIcon = (TaskDialogIcon) (int) ushort.MaxValue;
        taskDialog.Title = "Void Hosting Updater";
        taskDialog.TitleAutoPrefix = true;
        taskDialog.AllowCancellation = true;
        taskDialog.MainInstruction = "Void Hosting Updater";
        taskDialog.MainContent = "Select the scope of the Void Hosting Update.";
        taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
        taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Update Hosting for Voids in the Whole Model.");
        taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Update Hosting for Voids in the Active View.");
        taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1003, "Update Hosting for Selected Voids");
        taskDialog.CommonButtons = (TaskDialogCommonButtons) 8;
        taskDialog.DefaultButton = (TaskDialogResult) 2;
        TaskDialogResult taskDialogResult = taskDialog.Show();
        if (taskDialogResult == 1001)
        {
          int num = (int) transaction.Start();
          HashSet<string> stringSet2;
          try
          {
            stringSet2 = VoidHostingUpdater.ProcessAndUpdateHostedElements(revitDoc, false, (ICollection<ElementId>) null, out this.unprocessedVoids, out voidCount);
          }
          catch (Exception ex)
          {
            if (ex.Message.Contains("Multiple Elements"))
              return (Result) 1;
            new TaskDialog("EDGE Error")
            {
              FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
              AllowCancellation = false,
              MainInstruction = "Unexpected Exception Running Void Hosting",
              MainContent = $"EDGE encountered an unexpected exception: {ex.GetType().Name} please contact EDGE support if necessary.  See expanded content for exception detail",
              ExpandedContent = ex.Message
            }.Show();
            return (Result) 1;
          }
          revitDoc.Regenerate();
          if (this.unprocessedVoids.Count > 0)
          {
            StringBuilder stringBuilder1 = new StringBuilder("");
            foreach (Element unprocessedVoid in this.unprocessedVoids)
            {
              string str4 = stringBuilder1.ToString();
              string str5 = unprocessedVoid.Name.ToString();
              int integerValue = unprocessedVoid.Id.IntegerValue;
              string str6 = integerValue.ToString();
              string str7 = $"{str5} {str6}";
              if (!str4.Contains(str7))
              {
                StringBuilder stringBuilder2 = stringBuilder1;
                string str8 = unprocessedVoid.Name.ToString();
                integerValue = unprocessedVoid.Id.IntegerValue;
                string str9 = integerValue.ToString();
                string str10 = $"{str8} {str9}\n";
                stringBuilder1 = stringBuilder2.Append(str10);
              }
            }
            string str11 = stringBuilder1.ToString();
            new TaskDialog("Unprocessed Voids")
            {
              FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
              AllowCancellation = false,
              MainInstruction = "Voids were not processed",
              MainContent = ("The following void(s) will not be processed because the family origin is not intersecting any structural framing element: \n" + str11)
            }.Show();
          }
          if (this.unprocessedVoids.Count<Element>() == voidCount)
          {
            this.unprocessedVoids.Clear();
            return (Result) 1;
          }
          this.unprocessedVoids.Clear();
          TransactionStatus transactionStatus = transaction.Commit();
          bool flag = stringSet2.Count > 0;
          if (!flag)
          {
            new TaskDialog("No Voids Processed")
            {
              FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
              MainInstruction = "No Voids were able to be processed and hosted."
            }.Show();
            return (Result) 1;
          }
          if (transactionStatus == TransactionStatus.Committed & flag)
            new TaskDialog(str1)
            {
              FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
              MainInstruction = (str2 + " in the Whole Model")
            }.Show();
          return (Result) 0;
        }
        if (taskDialogResult == 1002)
        {
          int num = (int) transaction.Start();
          HashSet<string> stringSet3;
          try
          {
            stringSet3 = VoidHostingUpdater.ProcessAndUpdateHostedElements(revitDoc, true, (ICollection<ElementId>) null, out this.unprocessedVoids, out voidCount);
          }
          catch (Exception ex)
          {
            if (!ex.ToString().Contains("No Voids Found."))
              return (Result) -1;
            new TaskDialog("No Voids Found")
            {
              FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
              MainInstruction = "No Voids found. The Void Hosting Updater function will not do anything."
            }.Show();
            return (Result) 1;
          }
          revitDoc.Regenerate();
          if (this.unprocessedVoids.Count > 0)
          {
            StringBuilder stringBuilder3 = new StringBuilder("");
            foreach (Element unprocessedVoid in this.unprocessedVoids)
            {
              string str12 = stringBuilder3.ToString();
              string str13 = unprocessedVoid.Name.ToString();
              int integerValue = unprocessedVoid.Id.IntegerValue;
              string str14 = integerValue.ToString();
              string str15 = $"{str13} {str14}";
              if (!str12.Contains(str15))
              {
                StringBuilder stringBuilder4 = stringBuilder3;
                string str16 = unprocessedVoid.Name.ToString();
                integerValue = unprocessedVoid.Id.IntegerValue;
                string str17 = integerValue.ToString();
                string str18 = $"{str16} {str17}\n";
                stringBuilder3 = stringBuilder4.Append(str18);
              }
            }
            string str19 = stringBuilder3.ToString();
            new TaskDialog("Unprocessed Voids")
            {
              FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
              AllowCancellation = false,
              MainInstruction = "Voids were not processed",
              MainContent = ("The following void(s) will not be processed because the family origin is not intersecting any structural framing element: \n" + str19)
            }.Show();
          }
          if (this.unprocessedVoids.Count<Element>() == voidCount)
          {
            this.unprocessedVoids.Clear();
            return (Result) 1;
          }
          this.unprocessedVoids.Clear();
          TransactionStatus transactionStatus = transaction.Commit();
          bool flag = stringSet3.Count > 0;
          if (!flag)
          {
            new TaskDialog("No Voids Processed")
            {
              FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
              MainInstruction = "No Voids were able to be processed and hosted."
            }.Show();
            return (Result) 1;
          }
          if (transactionStatus == TransactionStatus.Committed & flag)
            new TaskDialog(str1)
            {
              FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
              MainInstruction = (str2 + " in the Active View")
            }.Show();
          return (Result) 0;
        }
        if (taskDialogResult == 1003)
        {
          ICollection<ElementId> elementIds = activeUiDocument.Selection.GetElementIds();
          if (elementIds.Count == 0)
            elementIds = References.PickNewReferences(activeUiDocument, (ISelectionFilter) new OnlyVoids(list), "Select Void to be updated.");
          if (elementIds.Select<ElementId, Element>((Func<ElementId, Element>) (id => revitDoc.GetElement(id))).Any<Element>((Func<Element, bool>) (e => Utils.ElementUtils.Parameters.GetParameterAsBool(e, "HARDWARE_DETAIL"))))
          {
            TaskDialog.Show("Void Hosting Updater", "One or more of your selected voids were Hardware Detail elements. These are not valid for Void Hosting Updater. Please try again with valid void elements.");
            return (Result) 1;
          }
          if (elementIds == null || elementIds.Count == 0)
            return (Result) 1;
          new List<ElementId>().AddRange((IEnumerable<ElementId>) elementIds);
          int num = (int) transaction.Start();
          HashSet<string> stringSet4;
          try
          {
            stringSet4 = VoidHostingUpdater.ProcessAndUpdateHostedElements(revitDoc, false, elementIds, out this.unprocessedVoids, out voidCount);
          }
          catch (Exception ex)
          {
            if (!ex.ToString().Contains("No Voids Found."))
              return (Result) -1;
            new TaskDialog("No Voids Found")
            {
              FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
              MainInstruction = "No Voids found. The Void Hosting Updater function will not do anything."
            }.Show();
            return (Result) 1;
          }
          revitDoc.Regenerate();
          if (this.unprocessedVoids.Count > 0)
          {
            StringBuilder stringBuilder5 = new StringBuilder("");
            foreach (Element unprocessedVoid in this.unprocessedVoids)
            {
              string str20 = stringBuilder5.ToString();
              string str21 = unprocessedVoid.Name.ToString();
              int integerValue = unprocessedVoid.Id.IntegerValue;
              string str22 = integerValue.ToString();
              string str23 = $"{str21} {str22}";
              if (!str20.Contains(str23))
              {
                StringBuilder stringBuilder6 = stringBuilder5;
                string str24 = unprocessedVoid.Name.ToString();
                integerValue = unprocessedVoid.Id.IntegerValue;
                string str25 = integerValue.ToString();
                string str26 = $"{str24} {str25}\n";
                stringBuilder5 = stringBuilder6.Append(str26);
              }
            }
            string str27 = stringBuilder5.ToString();
            new TaskDialog("Unprocessed Voids")
            {
              FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
              AllowCancellation = false,
              MainInstruction = "Voids were not processed",
              MainContent = ("The following void(s) will not be processed because the family origin is not intersecting any structural framing element: \n" + str27)
            }.Show();
          }
          if (this.unprocessedVoids.Count<Element>() == voidCount)
          {
            this.unprocessedVoids.Clear();
            return (Result) 1;
          }
          this.unprocessedVoids.Clear();
          TransactionStatus transactionStatus = transaction.Commit();
          bool flag = stringSet4.Count > 0;
          if (!flag)
          {
            new TaskDialog("No Voids Processed")
            {
              FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
              MainInstruction = "No Voids were able to be processed and hosted."
            }.Show();
            return (Result) 1;
          }
          if (transactionStatus == TransactionStatus.Committed & flag)
            new TaskDialog(str1)
            {
              FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
              MainInstruction = (str2 + ".")
            }.Show();
          return (Result) 0;
        }
      }
      catch (Exception ex)
      {
        if (transaction.HasStarted())
        {
          int num = (int) transaction.RollBack();
        }
        if (ex.ToString().Contains("The parameter is read-only."))
        {
          new TaskDialog("Read-only Parameter Issue")
          {
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
            MainInstruction = "One or more families had read-only parameters and could not be updated. Please check relevant parameters and try again."
          }.Show();
          return (Result) -1;
        }
        if (ex.ToString().Contains("No Voids Found."))
        {
          new TaskDialog("No Voids Found")
          {
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
            MainInstruction = "No Voids found. The Void Hosting Updater function will not do anything."
          }.Show();
          return (Result) 1;
        }
        if (ex is EdgeException)
        {
          message = ex.Message;
          return (Result) -1;
        }
        message = str3 + ex?.ToString();
        return (Result) -1;
      }
      finally
      {
        App.DialogSwitches.SuspendModelLockingforOperation = false;
      }
    }
    return (Result) 1;
  }

  public static HashSet<string> ProcessAndUpdateHostedElements(
    Document revitDoc,
    bool bActiveViewOnly,
    ICollection<ElementId> selectedElements,
    out List<Element> unprocessedVoids,
    out int voidCount)
  {
    unprocessedVoids = new List<Element>();
    HashSet<string> stringSet = new HashSet<string>();
    ICollection<string> strings = (ICollection<string>) new Collection<string>()
    {
      "PROD_ERECT_SEQUENCE",
      "CONTROL_MARK",
      "BOM_PRODUCT_HOST",
      "HOST_GUID"
    };
    ICollection<ElementId> voidComponents = Components.GetVoidIds(revitDoc);
    new FilteredElementCollector(revitDoc).OfClass(typeof (FamilyInstance)).OfCategory(BuiltInCategory.OST_StructuralFraming);
    View activeView = revitDoc.ActiveView;
    Dictionary<string, Dictionary<string, Element>> dictionary = new Dictionary<string, Dictionary<string, Element>>();
    FilteredElementCollector elementCollector = new FilteredElementCollector(revitDoc);
    List<Element> source1 = new List<Element>();
    if (selectedElements == null)
    {
      source1 = (bActiveViewOnly ? (IEnumerable<Element>) new FilteredElementCollector(revitDoc, revitDoc.ActiveView.Id).OfCategory(BuiltInCategory.OST_GenericModel).OfClass(typeof (FamilyInstance)) : (IEnumerable<Element>) new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_GenericModel).OfClass(typeof (FamilyInstance))).Where<Element>((Func<Element, bool>) (voidElem => voidComponents.Contains(voidElem.Id))).ToList<Element>();
    }
    else
    {
      foreach (ElementId selectedElement in (IEnumerable<ElementId>) selectedElements)
      {
        Element element = revitDoc.GetElement(selectedElement);
        if (voidComponents.Contains(selectedElement))
          source1.Add(element);
      }
    }
    voidCount = source1.Count<Element>() != 0 ? source1.Count : throw new Exception("No Voids Found.");
    foreach (Element element1 in source1)
    {
      if (element1 is FamilyInstance)
      {
        FamilyInstance familyInstance1 = element1 as FamilyInstance;
        Util.getTransformedBoundingBox(familyInstance1.GetTransform(), element1);
        XYZ xyz = new XYZ();
        BoundingBoxXYZ boundingBoxXyz = familyInstance1.get_BoundingBox((View) null);
        Outline outline1 = new Outline(boundingBoxXyz.Min, boundingBoxXyz.Max);
        LogicalOrFilter logicalOrFilter = new LogicalOrFilter((ElementFilter) new BoundingBoxIsInsideFilter(outline1), (ElementFilter) new BoundingBoxIntersectsFilter(outline1));
        List<Element> source2 = new List<Element>();
        List<Element> elementList = new List<Element>();
        if (InstanceVoidCutUtils.GetElementsBeingCut((Element) familyInstance1).Count != 0)
        {
          foreach (ElementId id in (IEnumerable<ElementId>) InstanceVoidCutUtils.GetElementsBeingCut((Element) familyInstance1))
          {
            Element element2 = revitDoc.GetElement(id);
            if (element2 is FamilyInstance familyInstance2)
            {
              Transform transform = familyInstance2.GetTransform();
              BoundingBoxXYZ transformedBoundingBox = Util.getTransformedBoundingBox(transform, element2);
              XYZ p = transform.Inverse.OfPoint((familyInstance1.Location as LocationPoint).Point);
              Outline outline2 = new Outline(new XYZ(transformedBoundingBox.Min.X, transformedBoundingBox.Min.Y, transformedBoundingBox.Min.Z), new XYZ(transformedBoundingBox.Max.X, transformedBoundingBox.Max.Y, transformedBoundingBox.Max.Z));
              ElementIntersectsElementFilter intersectsElementFilter = new ElementIntersectsElementFilter(revitDoc.GetElement(id));
              if (Util.BoundingBoxXyzContains(transformedBoundingBox, p))
                source2.Add(revitDoc.GetElement(id));
            }
          }
          try
          {
            List<Element> source3 = new List<Element>();
            if (source2.Count<Element>() > 0)
            {
              ElementIntersectsElementFilter intersectsElementFilter = new ElementIntersectsElementFilter((Element) familyInstance1);
              foreach (Element element3 in source2)
              {
                Element element4 = element3;
                Element element5 = (Element) null;
                bool flag = false;
                do
                {
                  if (element5 != null)
                    element4 = element5;
                  if (element4 is FamilyInstance)
                    element5 = (element4 as FamilyInstance).SuperComponent;
                  else
                    break;
                }
                while (element5 != null);
                foreach (Element element6 in source3)
                {
                  if (element6.Id.IntegerValue == element4.Id.IntegerValue)
                  {
                    flag = true;
                    break;
                  }
                }
                if (!flag)
                  source3.Add(element4);
              }
              if (source3.Count<Element>() > 1)
              {
                List<Element> source4 = new List<Element>();
                foreach (Element structFramingElem in source3)
                {
                  Element flatElement = (Element) AssemblyInstances.GetFlatElement(revitDoc, structFramingElem as FamilyInstance);
                  if (!source4.Select<Element, ElementId>((Func<Element, ElementId>) (s => s.Id)).Contains<ElementId>(flatElement.Id))
                    source4.Add(flatElement);
                }
                source3 = source4;
              }
              if (source3.Count<Element>() > 1)
              {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (Element elem1 in source3)
                {
                  string str1 = "null";
                  string str2;
                  string str3;
                  if (elem1 is FamilyInstance)
                  {
                    FamilyInstance elem2 = elem1 as FamilyInstance;
                    str1 = elem2.Symbol.Family.Name;
                    str2 = Utils.ElementUtils.Parameters.GetParameterAsString((Element) elem2, "CONTROL_MARK");
                    str3 = elem2.Symbol.Name;
                  }
                  else
                  {
                    str2 = Utils.ElementUtils.Parameters.GetParameterAsString(elem1, "CONTROL_MARK");
                    str3 = elem1.Name;
                  }
                  if (string.IsNullOrEmpty(str2))
                    str2 = "null";
                  if (string.IsNullOrEmpty(str3))
                    str3 = "null";
                  stringBuilder.AppendLine($"{str1}, {str3}, {str2}");
                }
                TaskDialog.Show("Error", $"Void Component {familyInstance1.Name}: {familyInstance1.Id?.ToString()} intersects multiple elements.  Please investigate and resolve multiple intersection.{Environment.NewLine}Intersected Elements:(Family, Type, Control Mark) {Environment.NewLine}{stringBuilder?.ToString()}");
                throw new Exception("Multiple Elements");
              }
            }
            Element structFramingElem1 = source3.Count<Element>() <= 1 ? source3.SingleOrDefault<Element>() : throw new Exception("Actual Intersectors Greater than 1");
            if (structFramingElem1 == null)
            {
              unprocessedVoids.Add(element1);
            }
            else
            {
              Element flatElement = (Element) AssemblyInstances.GetFlatElement(revitDoc, structFramingElem1 as FamilyInstance);
              foreach (string name in (IEnumerable<string>) strings)
              {
                Parameter parameter = familyInstance1.LookupParameter(name);
                if (parameter != null)
                {
                  switch (name)
                  {
                    case "HOST_GUID":
                      string str4 = "";
                      parameter.Set(str4);
                      continue;
                    case "BOM_PRODUCT_HOST":
                      string str5 = "";
                      parameter.Set(str5);
                      continue;
                    default:
                      string str6 = "";
                      parameter.Set(str6);
                      continue;
                  }
                }
              }
              if (flatElement != null)
              {
                Geometry.GetIntersectedMaterialId(familyInstance1, flatElement as FamilyInstance);
                foreach (string name in (IEnumerable<string>) strings)
                {
                  Parameter parameter1 = familyInstance1.LookupParameter(name);
                  if (parameter1 != null)
                  {
                    switch (name)
                    {
                      case "HOST_GUID":
                        string uniqueId = flatElement.UniqueId;
                        parameter1.Set(uniqueId);
                        continue;
                      case "BOM_PRODUCT_HOST":
                        string controlMark = flatElement.GetControlMark();
                        parameter1.Set(controlMark);
                        continue;
                      default:
                        Parameter parameter2 = flatElement.LookupParameter(name);
                        if (parameter2 != null)
                        {
                          string str = parameter2.AsString();
                          parameter1.Set(str);
                          continue;
                        }
                        string str7 = "";
                        parameter1.Set(str7);
                        continue;
                    }
                  }
                }
                stringSet.Add(flatElement.UniqueId);
              }
            }
          }
          catch (InvalidOperationException ex)
          {
            throw new EdgeException($"Error: Element {familyInstance1.Name} (ID: {familyInstance1.Id}) is intersecting more than one Structural Framing Element.", (Exception) ex);
          }
        }
      }
    }
    return stringSet;
  }
}
