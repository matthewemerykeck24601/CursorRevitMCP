// Decompiled with JetBrains decompiler
// Type: EDGE.SchedulingTools.ErectionSequenceTool.ErectionSequenceZoneWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.SchedulingTools.ErectionSequenceTool;

public class ErectionSequenceZoneWindow : Window, IComponentConnector, IStyleConnector
{
  public ZoneSettings ListItems = new ZoneSettings();
  public ErectionSequenceZone SelectedZone = new ErectionSequenceZone(string.Empty, 0);
  private int rowIndex = -1;
  private int currRowCount = -1;
  private List<ErectionSequenceZone> startingList = new List<ErectionSequenceZone>();
  private Document revitDoc;
  public bool cancel = true;
  private bool changesMade;
  private List<ErectionSequenceZone> OGZones = new List<ErectionSequenceZone>();
  internal DataGrid Zone_List;
  internal Button AssignButton;
  internal Button AddRow;
  internal Button DelRow;
  internal Button ResetZone;
  internal Button ResetSequence;
  internal Button SaveButton;
  internal Button CloseButton;
  private bool _contentLoaded;

  public ErectionSequenceZoneWindow(
    Document doc,
    List<ErectionSequenceZone> ZoneList,
    IntPtr parentWindowHandler)
  {
    this.InitializeComponent();
    WindowInteropHelper windowInteropHelper = new WindowInteropHelper((Window) this);
    windowInteropHelper.EnsureHandle();
    windowInteropHelper.Owner = parentWindowHandler;
    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    this.revitDoc = doc;
    if (ZoneList != null)
    {
      this.ListItems.ZoneEntryList = ZoneList;
      this.Zone_List.ItemsSource = (IEnumerable) this.ListItems.ZoneEntryList;
      this.OGZones = ZoneList.ToList<ErectionSequenceZone>();
      for (int index = 0; index < ZoneList.Count; ++index)
        this.startingList.Add(new ErectionSequenceZone(ZoneList[index].ZoneName, ZoneList[index].ZoneIndex, ZoneList[index].ZoneCount, ZoneList[index].IsDuplicateName));
    }
    this.currRowCount = this.Zone_List.Items.Count;
    if (this.currRowCount != 0)
      return;
    this.AddRow_Click(new object(), new RoutedEventArgs());
  }

  private void SaveButton_Click(object sender, RoutedEventArgs e)
  {
    if (!this.saveButtons())
      return;
    this.displayNewChanges();
    this.changesMade = false;
    this.startingList = new List<ErectionSequenceZone>();
    for (int index = 0; index < this.ListItems.ZoneEntryList.Count; ++index)
      this.startingList.Add(new ErectionSequenceZone((this.Zone_List.Items[index] as ErectionSequenceZone).ZoneName, (this.Zone_List.Items[index] as ErectionSequenceZone).ZoneIndex));
    for (int index = 0; index < this.ListItems.ZoneEntryList.Count; ++index)
      this.ListItems.ZoneEntryList[index].ZoneCount = ErectionSequence_Command.GetZoneElementCount(this.revitDoc, this.ListItems.ZoneEntryList[index].ZoneIndex);
  }

  private bool ValidateZones()
  {
    if (this.ListItems.ZoneEntryList.Count == 0 || this.ListItems.ZoneEntryList.Count == 1 && this.ListItems.ZoneEntryList.Where<ErectionSequenceZone>((Func<ErectionSequenceZone, bool>) (z => string.IsNullOrEmpty(z.ZoneName))).Count<ErectionSequenceZone>() > 0)
    {
      new TaskDialog("Erection Sequence - Invalid Zones Exist")
      {
        MainInstruction = "Duplicate names and empty zones must be resolved.",
        MainContent = "Duplicate zone names should be highlighted in red. Please make sure that all names are unique and no empty zones exist before saving."
      }.Show();
      return false;
    }
    if (this.ListItems.ZoneEntryList.Where<ErectionSequenceZone>((Func<ErectionSequenceZone, bool>) (z => z.IsDuplicateName)).Count<ErectionSequenceZone>() <= 0 && this.ListItems.ZoneEntryList.Where<ErectionSequenceZone>((Func<ErectionSequenceZone, bool>) (z => string.IsNullOrEmpty(z.ZoneName))).Count<ErectionSequenceZone>() <= 0)
      return true;
    new TaskDialog("Erection Sequence - Invalid Zones Exist")
    {
      MainInstruction = "Duplicate names and empty zones must be resolved.",
      MainContent = "Duplicate zone names should be highlighted in red. Please make sure that all names are unique and no empty zones exist before saving."
    }.Show();
    return false;
  }

  private void AddRow_Click(object sender, RoutedEventArgs e)
  {
    this.currRowCount = this.Zone_List.Items.Count + 1;
    this.ListItems.ZoneEntryList.Add(new ErectionSequenceZone(string.Empty, this.currRowCount));
    this.changesMade = true;
    this.Zone_List.ItemsSource = (IEnumerable) null;
    this.Zone_List.ItemsSource = (IEnumerable) this.ListItems.ZoneEntryList;
    this.Zone_List.SelectedItem = this.Zone_List.Items[this.Zone_List.Items.Count - 1];
    this.Zone_List.Items.Refresh();
  }

  private bool CheckForDuplicates()
  {
    this.Zone_List.Items.Refresh();
    List<ErectionSequenceZone> itemsSource = this.Zone_List.ItemsSource as List<ErectionSequenceZone>;
    bool flag = false;
    List<ErectionSequenceZone> source = new List<ErectionSequenceZone>();
    if (itemsSource != null)
    {
      int count = itemsSource.Count;
      List<IGrouping<string, ErectionSequenceZone>> list = itemsSource.GroupBy<ErectionSequenceZone, string>((Func<ErectionSequenceZone, string>) (z => z.ZoneName)).Where<IGrouping<string, ErectionSequenceZone>>((Func<IGrouping<string, ErectionSequenceZone>, bool>) (g => g.Count<ErectionSequenceZone>() > 1)).ToList<IGrouping<string, ErectionSequenceZone>>();
      if (list != null)
        source = list.SelectMany<IGrouping<string, ErectionSequenceZone>, ErectionSequenceZone>((Func<IGrouping<string, ErectionSequenceZone>, IEnumerable<ErectionSequenceZone>>) (group => (IEnumerable<ErectionSequenceZone>) group)).ToList<ErectionSequenceZone>();
      foreach (ErectionSequenceZone erectionSequenceZone in itemsSource)
      {
        ErectionSequenceZone z = erectionSequenceZone;
        z.IsDuplicateName = source.Where<ErectionSequenceZone>((Func<ErectionSequenceZone, bool>) (d => d.ZoneName == z.ZoneName)).Count<ErectionSequenceZone>() > 0 || string.IsNullOrEmpty(z.ZoneName);
      }
    }
    return flag;
  }

  private void DeleteRow_Click(object sender, RoutedEventArgs e)
  {
    foreach (ErectionSequenceZone selectedItem in (IEnumerable) this.Zone_List.SelectedItems)
      this.ListItems.ZoneEntryList.Remove(selectedItem);
    this.Zone_List.ItemsSource = (IEnumerable) null;
    this.ReorderZones();
    this.changesMade = false;
  }

  private void ReorderZones()
  {
    this.ListItems.ZoneEntryList.OrderBy<ErectionSequenceZone, int>((Func<ErectionSequenceZone, int>) (z => z.ZoneIndex));
    for (int index = 0; index < this.ListItems.ZoneEntryList.Count; ++index)
      this.ListItems.ZoneEntryList[index].ZoneIndex = index + 1;
    this.Zone_List.ItemsSource = (IEnumerable) this.ListItems.ZoneEntryList;
  }

  private void zoneDataGrid_Drop(object sender, DragEventArgs e)
  {
    if (this.rowIndex < 0)
      return;
    int currentRowIndex = this.GetCurrentRowIndex(new ErectionSequenceZoneWindow.GetPosition(e.GetPosition));
    if (currentRowIndex < 0 || currentRowIndex == this.rowIndex)
      return;
    if (currentRowIndex == this.Zone_List.Items.Count)
      return;
    try
    {
      this.Zone_List.Items.Refresh();
    }
    catch (Exception ex)
    {
      if (!ex.Message.Contains("EditItem"))
        return;
      this.Zone_List.CommitEdit();
      this.Zone_List.CommitEdit();
    }
    ErectionSequenceZone zoneEntry = this.ListItems.ZoneEntryList[this.rowIndex];
    this.ListItems.ZoneEntryList.RemoveAt(this.rowIndex);
    zoneEntry.ZoneIndex = currentRowIndex + 1;
    this.ListItems.ZoneEntryList.Insert(currentRowIndex, zoneEntry);
    this.ReorderZones();
    this.changesMade = true;
    this.Zone_List.Items.Refresh();
  }

  private void Zone_List_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
  {
    this.rowIndex = this.GetCurrentRowIndex(new ErectionSequenceZoneWindow.GetPosition(((MouseEventArgs) e).GetPosition));
    if (this.rowIndex < 0)
      return;
    this.Zone_List.SelectedIndex = this.rowIndex;
    if (!(this.Zone_List.Items[this.rowIndex] is ErectionSequenceZone data))
      return;
    DragDropEffects allowedEffects = DragDropEffects.Move;
    if (DragDrop.DoDragDrop((DependencyObject) this.Zone_List, (object) data, allowedEffects) == DragDropEffects.None)
      return;
    this.Zone_List.SelectedItem = (object) data;
  }

  private bool GetMouseTargetRow(Visual theTarget, ErectionSequenceZoneWindow.GetPosition position)
  {
    return theTarget != null && VisualTreeHelper.GetDescendantBounds(theTarget).Contains(position((IInputElement) theTarget));
  }

  private DataGridRow GetRowItem(int index)
  {
    return this.Zone_List.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated ? (DataGridRow) null : this.Zone_List.ItemContainerGenerator.ContainerFromIndex(index) as DataGridRow;
  }

  private int GetCurrentRowIndex(ErectionSequenceZoneWindow.GetPosition pos)
  {
    int currentRowIndex = -1;
    for (int index = 0; index < this.Zone_List.Items.Count; ++index)
    {
      if (this.GetMouseTargetRow((Visual) this.GetRowItem(index), pos))
      {
        currentRowIndex = index;
        break;
      }
    }
    return currentRowIndex;
  }

  private void displayNewChanges()
  {
    List<ErectionSequenceZone> ogZones = this.OGZones;
    if (ogZones == null)
      return;
    this.ListItems.ZoneEntryList = ogZones;
    this.Zone_List.ItemsSource = (IEnumerable) this.ListItems.ZoneEntryList;
  }

  private void ResetModel_Click(object sender, RoutedEventArgs e1)
  {
    Dictionary<string, int> dictionary = new Dictionary<string, int>();
    string empty = string.Empty;
    if (new TaskDialog("Erection Sequence - Reset Model")
    {
      MainInstruction = "Are you sure you want to reset all erection sequence parameter values in the model?",
      MainContent = "This will clear all current values for the ERECTION_SEQUENCE_NUMBER, ERECTION_SEQUENCE_ZONE_NAME, and ERECTION_SEQUENCE_ZONE_NUMBER parameters on all pieces in the model.",
      CommonButtons = ((TaskDialogCommonButtons) 6)
    }.Show() != 6)
      return;
    ICollection<Element> elements = (ICollection<Element>) new FilteredElementCollector(this.revitDoc).OfCategory(BuiltInCategory.OST_StructuralFraming).OfClass(typeof (FamilyInstance)).ToElements();
    using (Transaction transaction = new Transaction(this.revitDoc, "Reset Erection Sequence Parameters"))
    {
      try
      {
        List<Element> source = new List<Element>();
        int num1 = (int) transaction.Start();
        foreach (Element element1 in (IEnumerable<Element>) elements)
        {
          List<Element> collection = new List<Element>();
          if (element1 is FamilyInstance elem)
          {
            if (elem.HasSuperComponent())
            {
              Element superComponent = elem.GetSuperComponent();
              collection.Add(superComponent);
              foreach (ElementId id in superComponent.GetSubComponentIds().ToList<ElementId>())
              {
                Element element2 = elem.Document.GetElement(id);
                if (element2 != null && element2.Category.Id == new ElementId(BuiltInCategory.OST_StructuralFraming))
                  collection.Add(element2);
              }
            }
            else
              collection.Add((Element) elem);
          }
          else
            collection.Add(element1);
          source.AddRange((IEnumerable<Element>) collection);
        }
        if (this.revitDoc.IsWorkshared && !ErectionSequence_Command.checkElementOwnershipCondition((ICollection<ElementId>) source.Select<Element, ElementId>((Func<Element, ElementId>) (e2 => e2.Id)).ToList<ElementId>(), this.revitDoc, true))
        {
          TaskDialog.Show("Erection Sequence Update", "The elements necessary for updating the Erection Sequence Zones are not currently editable. Please coordinate with project members to allow for ownership of the elements and then reload the latest from central.");
        }
        else
        {
          foreach (Element elem in source)
          {
            Parameters.LookupParameter(elem, "ERECTION_SEQUENCE_NUMBER")?.Set(0);
            Parameters.LookupParameter(elem, "ERECTION_SEQUENCE_ZONE_NAME")?.Set("");
            Parameters.LookupParameter(elem, "ERECTION_SEQUENCE_ZONE_NUMBER")?.Set(0);
          }
          for (int index = 0; index < this.ListItems.ZoneEntryList.Count; ++index)
            this.ListItems.ZoneEntryList[index].ZoneCount = ErectionSequence_Command.GetZoneElementCount(this.revitDoc, this.ListItems.ZoneEntryList[index].ZoneIndex);
          int num2 = (int) transaction.Commit();
        }
      }
      catch (Exception ex)
      {
        TaskDialog.Show("Erection Sequence", "Unable to clear Erection Sequence parameters for pieces in the model. No pieces were edited.");
        if (transaction.HasEnded())
          return;
        int num = (int) transaction.RollBack();
      }
    }
  }

  private void AssignButton_Click(object sender, RoutedEventArgs e)
  {
    if (!(this.Zone_List.SelectedItem is ErectionSequenceZone selectedItem))
    {
      new TaskDialog("Erection Sequence")
      {
        MainInstruction = "Please select a zone in the the data grid.",
        MainContent = "In order to assign erection sequences you must select an erection sequence zone first."
      }.Show();
    }
    else
    {
      if (selectedItem.ZoneIndex < 1 || string.IsNullOrWhiteSpace(selectedItem.ZoneName) || !this.ValidateZones() || !this.SaveErectionSequenceZones())
        return;
      this.SelectedZone = selectedItem;
      this.cancel = false;
      this.Close();
    }
  }

  private bool SaveErectionSequenceZones()
  {
    List<ErectionSequenceZone> erectionSequenceZoneList = new List<ErectionSequenceZone>();
    List<ErectionSequenceZone> source1 = new List<ErectionSequenceZone>();
    foreach (object obj in (IEnumerable) this.Zone_List.Items)
    {
      if (obj is ErectionSequenceZone erectionSequenceZone)
      {
        erectionSequenceZoneList.Add(erectionSequenceZone);
        if (erectionSequenceZone.Edited)
          source1.Add(erectionSequenceZone);
      }
    }
    List<ErectionSequenceZone> source2 = new List<ErectionSequenceZone>();
    foreach (ErectionSequenceZone ogZone1 in this.OGZones)
    {
      ErectionSequenceZone ogZone = ogZone1;
      if (!erectionSequenceZoneList.Where<ErectionSequenceZone>((Func<ErectionSequenceZone, bool>) (x => x.OriginalZoneIndex == ogZone.OriginalZoneIndex && x.OriginalZoneName == ogZone.OriginalZoneName)).Any<ErectionSequenceZone>())
        source2.Add(ogZone);
    }
    TransactionGroup transactionGroup = new TransactionGroup(this.revitDoc);
    int num1 = (int) transactionGroup.Start();
    if (source1.Count > 0 || source2.Count > 0)
    {
      TaskDialog taskDialog1 = new TaskDialog("Erection Sequence");
      taskDialog1.MainInstruction = "Are you sure you want to save current erection sequence zones?";
      taskDialog1.MainContent = "Saving the current erection sequence zones will update erection sequence zone number and name for pieces in edited zones. All pieces in deleted zones will have their erection sequence zone name, erection sequence zone number, and erection sequence number parameters cleared.";
      int num2;
      if (source1.Count > 0)
      {
        TaskDialog taskDialog2 = taskDialog1;
        TaskDialog taskDialog3 = taskDialog2;
        string[] strArray1 = new string[9];
        strArray1[0] = taskDialog2.ExpandedContent;
        strArray1[1] = "Edited Zones: \n";
        num2 = source1[0].OriginalZoneIndex;
        strArray1[2] = num2.ToString();
        strArray1[3] = ": ";
        strArray1[4] = source1[0].OriginalZoneName;
        strArray1[5] = " --> ";
        num2 = source1[0].ZoneIndex;
        strArray1[6] = num2.ToString();
        strArray1[7] = ": ";
        strArray1[8] = source1[0].ZoneName;
        string str1 = string.Concat(strArray1);
        taskDialog3.ExpandedContent = str1;
        for (int index = 1; index < source1.Count; ++index)
        {
          TaskDialog taskDialog4 = taskDialog1;
          TaskDialog taskDialog5 = taskDialog4;
          string[] strArray2 = new string[9];
          strArray2[0] = taskDialog4.ExpandedContent;
          strArray2[1] = ",\n";
          num2 = source1[index].OriginalZoneIndex;
          strArray2[2] = num2.ToString();
          strArray2[3] = ": ";
          strArray2[4] = source1[index].OriginalZoneName;
          strArray2[5] = " --> ";
          num2 = source1[index].ZoneIndex;
          strArray2[6] = num2.ToString();
          strArray2[7] = ": ";
          strArray2[8] = source1[index].ZoneName;
          string str2 = string.Concat(strArray2);
          taskDialog5.ExpandedContent = str2;
        }
        if (source2.Count > 0)
          taskDialog1.ExpandedContent += "\n\n";
      }
      if (source2.Count > 0)
      {
        TaskDialog taskDialog6 = taskDialog1;
        TaskDialog taskDialog7 = taskDialog6;
        string[] strArray3 = new string[5]
        {
          taskDialog6.ExpandedContent,
          "Deleted Zones: \n",
          null,
          null,
          null
        };
        num2 = source2[0].OriginalZoneIndex;
        strArray3[2] = num2.ToString();
        strArray3[3] = ": ";
        strArray3[4] = source2[0].OriginalZoneName;
        string str3 = string.Concat(strArray3);
        taskDialog7.ExpandedContent = str3;
        for (int index = 1; index < source2.Count; ++index)
        {
          TaskDialog taskDialog8 = taskDialog1;
          TaskDialog taskDialog9 = taskDialog8;
          string[] strArray4 = new string[5]
          {
            taskDialog8.ExpandedContent,
            ",\n",
            null,
            null,
            null
          };
          num2 = source2[index].OriginalZoneIndex;
          strArray4[2] = num2.ToString();
          strArray4[3] = ": ";
          strArray4[4] = source2[index].OriginalZoneName;
          string str4 = string.Concat(strArray4);
          taskDialog9.ExpandedContent = str4;
        }
      }
      taskDialog1.CommonButtons = (TaskDialogCommonButtons) 6;
      if (taskDialog1.Show() != 6)
      {
        int num3 = (int) transactionGroup.RollBack();
        return false;
      }
      List<Element> list1 = new FilteredElementCollector(this.revitDoc).OfCategory(BuiltInCategory.OST_StructuralFraming).OfClass(typeof (FamilyInstance)).Where<Element>((Func<Element, bool>) (x => x is FamilyInstance familyInstance1 && (!familyInstance1.IsWarpableProduct() || !familyInstance1.HasSuperComponent()))).ToList<Element>();
      using (Transaction transaction = new Transaction(this.revitDoc, "Edit Erection Sequence Zones"))
      {
        try
        {
          int num4 = (int) transaction.Start();
          List<Element> source3 = new List<Element>();
          List<Element> source4 = new List<Element>();
          foreach (Element elem in list1)
          {
            Parameter parameter = Parameters.LookupParameter(elem, "ERECTION_SEQUENCE_ZONE_NUMBER");
            if (parameter != null)
            {
              int zoneNum = parameter.AsInteger();
              if (source2.Where<ErectionSequenceZone>((Func<ErectionSequenceZone, bool>) (x => x.OriginalZoneIndex == zoneNum)).Any<ErectionSequenceZone>())
                source3.Add(elem);
              if (source1.Where<ErectionSequenceZone>((Func<ErectionSequenceZone, bool>) (x => x.OriginalZoneIndex == zoneNum)).FirstOrDefault<ErectionSequenceZone>() != null)
                source4.Add(elem);
            }
          }
          if (this.revitDoc.IsWorkshared)
          {
            List<ElementId> list2 = source3.Select<Element, ElementId>((Func<Element, ElementId>) (e => e.Id)).ToList<ElementId>();
            list2.AddRange((IEnumerable<ElementId>) source4.Select<Element, ElementId>((Func<Element, ElementId>) (e => e.Id)).ToList<ElementId>());
            for (int index1 = 0; index1 < list2.Count; ++index1)
            {
              List<Element> warpableListToEdit = ErectionSequence_Command.GetWarpableListToEdit(list2[index1].GetElement() as FamilyInstance);
              for (int index2 = 0; index2 < warpableListToEdit.Count; ++index2)
              {
                if (!list2.Contains(warpableListToEdit[index2].Id))
                  list2.Add(warpableListToEdit[index2].Id);
              }
            }
            if (list2.Count > 0 && !ErectionSequence_Command.checkElementOwnershipCondition((ICollection<ElementId>) list2, this.revitDoc, true))
            {
              int num5 = (int) transactionGroup.RollBack();
              TaskDialog.Show("Erection Sequence Update", "The elements necessary for updating the Erection Sequence Zones are not currently editable. Please coordinate with project members to allow for ownership of the elements and then reload the latest from central.");
              return false;
            }
          }
          foreach (Element elem1 in source3)
          {
            Parameters.LookupParameter(elem1, "ERECTION_SEQUENCE_ZONE_NUMBER").Set(0);
            Parameters.LookupParameter(elem1, "ERECTION_SEQUENCE_NUMBER")?.Set(0);
            Parameters.LookupParameter(elem1, "ERECTION_SEQUENCE_ZONE_NAME")?.Set("");
            if (elem1 is FamilyInstance familyInstance2)
            {
              List<Element> elementList = new List<Element>();
              foreach (ElementId id in familyInstance2.GetSubComponentIds().ToList<ElementId>())
              {
                Element element = familyInstance2.Document.GetElement(id);
                if (element != null && element.Category.Id == new ElementId(BuiltInCategory.OST_StructuralFraming))
                  elementList.Add(element);
              }
              foreach (Element elem2 in elementList)
              {
                Parameters.LookupParameter(elem2, "ERECTION_SEQUENCE_ZONE_NUMBER")?.Set(0);
                Parameters.LookupParameter(elem2, "ERECTION_SEQUENCE_NUMBER")?.Set(0);
                Parameters.LookupParameter(elem2, "ERECTION_SEQUENCE_ZONE_NAME")?.Set("");
              }
            }
          }
          foreach (Element elem3 in source4)
          {
            Parameter ESZoneNumberParameter = Parameters.LookupParameter(elem3, "ERECTION_SEQUENCE_ZONE_NUMBER");
            ErectionSequenceZone erectionSequenceZone = source1.Where<ErectionSequenceZone>((Func<ErectionSequenceZone, bool>) (x => x.OriginalZoneIndex == ESZoneNumberParameter.AsInteger())).FirstOrDefault<ErectionSequenceZone>();
            ESZoneNumberParameter.Set(erectionSequenceZone.ZoneIndex);
            Parameters.LookupParameter(elem3, "ERECTION_SEQUENCE_ZONE_NAME")?.Set(erectionSequenceZone.ZoneName);
            if (elem3 is FamilyInstance structFramingInstance && structFramingInstance.IsWarpableProduct())
            {
              List<Element> elementList = new List<Element>();
              foreach (ElementId id in structFramingInstance.GetSubComponentIds().ToList<ElementId>())
              {
                Element element = structFramingInstance.Document.GetElement(id);
                if (element != null && element.Category.Id == new ElementId(BuiltInCategory.OST_StructuralFraming))
                  elementList.Add(element);
              }
              foreach (Element elem4 in elementList)
              {
                Parameters.LookupParameter(elem4, "ERECTION_SEQUENCE_ZONE_NUMBER")?.Set(erectionSequenceZone.ZoneIndex);
                Parameters.LookupParameter(elem4, "ERECTION_SEQUENCE_ZONE_NAME")?.Set(erectionSequenceZone.ZoneName);
              }
            }
          }
          int num6 = (int) transaction.Commit();
        }
        catch (Exception ex)
        {
          int num7 = (int) transactionGroup.RollBack();
          new TaskDialog("Erection Sequence")
          {
            MainInstruction = "Failed to Edit Zones",
            MainContent = "Error occurred editing erection sequence zones. No pieces were edited. Zone edits were not saved."
          }.Show();
          return false;
        }
      }
    }
    bool isException;
    if (!ErectionSequenceZoneExtensibleStorage.SaveErectionSequenceZones(this.revitDoc, erectionSequenceZoneList, out isException))
    {
      new TaskDialog("Erection Sequence")
      {
        MainInstruction = (isException ? "Failed to Save Zone Edits" : "Failed to Save Zone Edits (work sharing)"),
        MainContent = (isException ? "Error occurred saving erection sequence zones. No pieces were edited. Zone edits were not saved. " : "Could not access project information because of either user ownership or central model changes. Please coordinate with project members to allow for ownership, and then reload the latest from central. No pieces were edited. Zone edits were not saved.")
      }.Show();
      int num8 = (int) transactionGroup.RollBack();
      return false;
    }
    this.OGZones = erectionSequenceZoneList.Select<ErectionSequenceZone, ErectionSequenceZone>((Func<ErectionSequenceZone, ErectionSequenceZone>) (x => new ErectionSequenceZone(x.ZoneName, x.ZoneIndex, x.ZoneCount))).ToList<ErectionSequenceZone>();
    this.ListItems.ZoneEntryList = this.OGZones.ToList<ErectionSequenceZone>();
    this.Zone_List.ItemsSource = (IEnumerable) this.ListItems.ZoneEntryList;
    int num9 = (int) transactionGroup.Commit();
    return true;
  }

  private void CloseButton_Click(object sender, RoutedEventArgs e)
  {
    List<ErectionSequenceZone> source = new List<ErectionSequenceZone>();
    List<ErectionSequenceZone> erectionSequenceZoneList1 = new List<ErectionSequenceZone>();
    foreach (object obj in (IEnumerable) this.Zone_List.Items)
    {
      if (obj is ErectionSequenceZone erectionSequenceZone)
      {
        source.Add(erectionSequenceZone);
        if (erectionSequenceZone.Edited)
          erectionSequenceZoneList1.Add(erectionSequenceZone);
      }
    }
    List<ErectionSequenceZone> erectionSequenceZoneList2 = new List<ErectionSequenceZone>();
    foreach (ErectionSequenceZone ogZone1 in this.OGZones)
    {
      ErectionSequenceZone ogZone = ogZone1;
      if (!source.Where<ErectionSequenceZone>((Func<ErectionSequenceZone, bool>) (x => x.OriginalZoneIndex == ogZone.OriginalZoneIndex && x.OriginalZoneName == ogZone.OriginalZoneName)).Any<ErectionSequenceZone>())
        erectionSequenceZoneList2.Add(ogZone);
    }
    if (this.OGZones.Count == source.Count && erectionSequenceZoneList2.Count == 0 && erectionSequenceZoneList1.Count == 0)
      this.Close();
    else if (new TaskDialog("Erection Sequence")
    {
      MainInstruction = "Would you like to save erection sequence zone changes?",
      CommonButtons = ((TaskDialogCommonButtons) 6)
    }.Show() == 6)
    {
      if (!this.saveButtons())
        return;
      this.Close();
    }
    else
      this.Close();
  }

  private void OnCellLostFocus(object sender, RoutedEventArgs e)
  {
    TextBox textBox = new TextBox();
    int indexNum = 0;
    if (!(e.OriginalSource is TextBox) || string.IsNullOrEmpty((e.OriginalSource as TextBox).Text))
      return;
    if (sender is DataGridCell && (sender as DataGridCell).DataContext is ErectionSequenceZone)
      indexNum = ((sender as DataGridCell).DataContext as ErectionSequenceZone).ZoneIndex;
    this.Zone_List.CommitEdit();
    this.Zone_List.CommitEdit();
    List<ErectionSequenceZone> list = (this.Zone_List.ItemsSource as ICollection<ErectionSequenceZone>).ToList<ErectionSequenceZone>();
    if (indexNum > 0)
    {
      ErectionSequenceZone erectionSequenceZone = list.FirstOrDefault<ErectionSequenceZone>((Func<ErectionSequenceZone, bool>) (z => z.ZoneIndex == indexNum));
      erectionSequenceZone.ZoneName = erectionSequenceZone.ZoneName.Trim();
    }
    this.CheckForDuplicates();
    if (this.Zone_List.Items.Count == 0)
      this.DelRow.IsEnabled = false;
    else
      this.DelRow.IsEnabled = true;
    this.Zone_List.Items.Refresh();
  }

  private void ResetZone_Click(object sender, RoutedEventArgs e)
  {
    bool flag = false;
    List<ErectionSequenceZone> source1 = new List<ErectionSequenceZone>();
    List<ErectionSequenceZone> erectionSequenceZoneList = new List<ErectionSequenceZone>();
    foreach (object obj in (IEnumerable) this.Zone_List.Items)
    {
      if (obj is ErectionSequenceZone erectionSequenceZone)
      {
        source1.Add(erectionSequenceZone);
        if (erectionSequenceZone.Edited)
        {
          flag = true;
          break;
        }
      }
    }
    if (!flag)
    {
      foreach (ErectionSequenceZone ogZone1 in this.OGZones)
      {
        ErectionSequenceZone ogZone = ogZone1;
        if (!source1.Where<ErectionSequenceZone>((Func<ErectionSequenceZone, bool>) (x => x.OriginalZoneIndex == ogZone.OriginalZoneIndex && x.OriginalZoneName == ogZone.OriginalZoneName)).Any<ErectionSequenceZone>())
        {
          flag = true;
          break;
        }
      }
    }
    if (!flag && !this.changesMade)
      return;
    if (new TaskDialog("Erection Sequence - Reset Zones")
    {
      MainInstruction = "Are you sure you want to reset zones?",
      MainContent = "Reseting zones will remove all changes made to the zone list since last save.",
      CommonButtons = ((TaskDialogCommonButtons) 6)
    }.Show() != 6)
      return;
    List<ErectionSequenceZone> source2 = ErectionSequenceZoneExtensibleStorage.ReadErectionSequenceZones(this.revitDoc);
    if (source2 == null)
      source2 = new List<ErectionSequenceZone>()
      {
        new ErectionSequenceZone(string.Empty, 1)
      };
    for (int index = 0; index < source2.Count; ++index)
      source2[index].ZoneCount = ErectionSequence_Command.GetZoneElementCount(this.revitDoc, source2[index].ZoneIndex);
    this.ListItems.ZoneEntryList = source2;
    this.Zone_List.ItemsSource = (IEnumerable) this.ListItems.ZoneEntryList;
    this.OGZones = source2.ToList<ErectionSequenceZone>();
    for (int index = 0; index < this.ListItems.ZoneEntryList.Count; ++index)
      this.ListItems.ZoneEntryList[index].ZoneCount = ErectionSequence_Command.GetZoneElementCount(this.revitDoc, this.ListItems.ZoneEntryList[index].ZoneIndex);
    this.changesMade = false;
  }

  private bool saveButtons()
  {
    try
    {
      if (!this.ValidateZones())
        return false;
      if (!this.SaveErectionSequenceZones())
        return false;
    }
    catch (Exception ex)
    {
      TaskDialog.Show("Zone Save Issue", ex.Message);
    }
    new TaskDialog("Erection Sequence")
    {
      MainInstruction = "Saved Successfully",
      MainContent = "Erection sequence zone changes have been saved successfully."
    }.Show();
    return true;
  }

  private void Zone_List_UnloadingRow(object sender, DataGridRowEventArgs e)
  {
    if ((this.Zone_List.Items.Count > this.ListItems.ZoneEntryList.Count ? this.Zone_List.Items.Count : this.ListItems.ZoneEntryList.Count) == 0)
      this.DelRow.IsEnabled = false;
    else
      this.DelRow.IsEnabled = true;
  }

  private void Zone_List_PreviewKeyDown(object sender, KeyEventArgs e)
  {
    if (e.Key != Key.Delete)
      return;
    this.DeleteRow_Click(sender, new RoutedEventArgs());
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/schedulingtools/erectionsequencetool/erectionsequencezonewindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        this.Zone_List = (DataGrid) target;
        this.Zone_List.UnloadingRow += new EventHandler<DataGridRowEventArgs>(this.Zone_List_UnloadingRow);
        this.Zone_List.Drop += new DragEventHandler(this.zoneDataGrid_Drop);
        this.Zone_List.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.Zone_List_PreviewMouseLeftButtonDown);
        this.Zone_List.PreviewKeyDown += new KeyEventHandler(this.Zone_List_PreviewKeyDown);
        break;
      case 3:
        this.AssignButton = (Button) target;
        this.AssignButton.Click += new RoutedEventHandler(this.AssignButton_Click);
        break;
      case 4:
        this.AddRow = (Button) target;
        this.AddRow.Click += new RoutedEventHandler(this.AddRow_Click);
        break;
      case 5:
        this.DelRow = (Button) target;
        this.DelRow.Click += new RoutedEventHandler(this.DeleteRow_Click);
        break;
      case 6:
        this.ResetZone = (Button) target;
        this.ResetZone.Click += new RoutedEventHandler(this.ResetZone_Click);
        break;
      case 7:
        this.ResetSequence = (Button) target;
        this.ResetSequence.Click += new RoutedEventHandler(this.ResetModel_Click);
        break;
      case 8:
        this.SaveButton = (Button) target;
        this.SaveButton.Click += new RoutedEventHandler(this.SaveButton_Click);
        break;
      case 9:
        this.CloseButton = (Button) target;
        this.CloseButton.Click += new RoutedEventHandler(this.CloseButton_Click);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IStyleConnector.Connect(int connectionId, object target)
  {
    if (connectionId != 2)
      return;
    ((Style) target).Setters.Add((SetterBase) new EventSetter()
    {
      Event = UIElement.LostFocusEvent,
      Handler = (Delegate) new RoutedEventHandler(this.OnCellLostFocus)
    });
  }

  public delegate System.Windows.Point GetPosition(IInputElement element);
}
