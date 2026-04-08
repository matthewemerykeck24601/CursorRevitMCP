// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.InsulationDrawing.InsulationDrawing_Command
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.InsulationTools.InsulationDrawing.Views;
using EDGE.UserSettingTools.Insulation_Drawing_Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.InsulationTools.InsulationDrawing;

[Transaction(TransactionMode.Manual)]
public class InsulationDrawing_Command : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    if (App.insulationDrawingWindow != null)
      return (Result) 1;
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    Document document = activeUiDocument.Document;
    string str1 = "Insulation Drawing - Mark";
    if (document.IsFamilyDocument)
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
    string manufacturer = "";
    Parameter parameter = document.ProjectInformation.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER");
    if (parameter != null && parameter.StorageType == StorageType.String)
      manufacturer = parameter.AsString();
    if (string.IsNullOrWhiteSpace(manufacturer))
    {
      TaskDialog.Show("Insulaion Drawing Settings", "Please ensure that the project has a valid PROJECT_CLIENT_PRECAST_MANUFACTURER parameter value and try to run Insulation Drawing - Mark again.");
      return (Result) 1;
    }
    InsulationDrawingSettings insulationDrawingSettings = new InsulationDrawingSettings(manufacturer);
    InsulationDrawingSettingsObject settingsObject;
    if (!insulationDrawingSettings.SettingsRead())
    {
      settingsObject = new InsulationDrawingSettingsObject(document);
      bool flag = true;
      if (settingsObject.InsulationDetailLineStyle == null)
        flag = false;
      if (settingsObject.MarkCircleDetailLineStyle == null)
        flag = false;
      if (settingsObject.RecessCalloutsTextStyle == null)
        flag = false;
      if (settingsObject.InsulationMarkTextStyle == null)
        flag = false;
      if (settingsObject.OverallDimensionStyle == null)
        flag = false;
      if (settingsObject.GeneralDimensionStyle == null)
        flag = false;
      if (settingsObject.TitleBlockFamily == null)
        flag = false;
      if (flag)
      {
        new TaskDialog("Insulation Drawing Settings")
        {
          MainInstruction = "Insulation Drawing Settings file not found.",
          MainContent = "Insulation Drawing Settings file not found. Insulation Drawing - Mark will use default settings."
        }.Show();
      }
      else
      {
        new TaskDialog("Insulation Drawing Settings")
        {
          MainInstruction = "Insulation Drawing Settings file not found.",
          MainContent = "Insulation Drawing Settings file not found. The default settings styles were not found in the project. Please update settings and try again."
        }.Show();
        return (Result) 1;
      }
    }
    else
      settingsObject = insulationDrawingSettings.GetSettings(document);
    if (settingsObject == null)
    {
      new TaskDialog("Insulation Drawing Settings")
      {
        MainInstruction = "Insulation Drawing Settings not found.",
        MainContent = "One or more insulation drawing settings are not present the project. Please update settings and try again."
      }.Show();
      return (Result) 1;
    }
    ElementMulticategoryFilter filter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
    {
      BuiltInCategory.OST_GenericModel,
      BuiltInCategory.OST_SpecialityEquipment
    });
    ICollection<ElementId> elementIds = activeUiDocument.Selection.GetElementIds();
    List<Element> elementList = new List<Element>();
    if (elementIds.Count > 0)
      elementList = new FilteredElementCollector(document, elementIds).OfClass(typeof (FamilyInstance)).WherePasses((ElementFilter) filter).Where<Element>((Func<Element, bool>) (i => InsulationDrawingUtils.ValidForInsulationDrawing(i) && InsulationDrawingUtils.InsulationLocked(i))).ToList<Element>();
    Dictionary<string, FamilyInstance> dictionary = new Dictionary<string, FamilyInstance>();
    List<string> stringList1 = new List<string>();
    List<string> details;
    if (elementList.Count == 0)
    {
      List<Element> list1 = new FilteredElementCollector(document).OfClass(typeof (FamilyInstance)).WherePasses((ElementFilter) filter).Where<Element>((Func<Element, bool>) (i => Utils.SelectionUtils.SelectionUtils.CheckforInsulationManufactureComponent(i))).ToList<Element>();
      List<string> stringList2 = new List<string>();
      foreach (Element element in list1)
      {
        string parameterAsString = Parameters.GetParameterAsString(element, "INSULATION_MARK");
        if (!string.IsNullOrWhiteSpace(parameterAsString) && !stringList2.Contains(parameterAsString) && InsulationDrawingUtils.InsulationLocked(element))
        {
          stringList2.Add(parameterAsString);
          if (InsulationDrawingUtils.ValidForInsulationDrawing(element) && !dictionary.ContainsKey(parameterAsString))
            dictionary.Add(parameterAsString, element as FamilyInstance);
        }
      }
      List<string> list2 = dictionary.Keys.ToList<string>();
      list2.Sort();
      InsulationDrawingSelectInsulationWindow insulationWindow = new InsulationDrawingSelectInsulationWindow(Process.GetCurrentProcess().MainWindowHandle, list2);
      insulationWindow.ShowDialog();
      if (insulationWindow.cancel)
        return (Result) 1;
      details = insulationWindow.GetSelectedMarks();
      if (details.Count == 0)
        return (Result) 1;
    }
    else
    {
      foreach (Element elem in elementList)
      {
        string parameterAsString = Parameters.GetParameterAsString(elem, "INSULATION_MARK");
        if (!string.IsNullOrWhiteSpace(parameterAsString) && !dictionary.ContainsKey(parameterAsString))
          dictionary.Add(parameterAsString, elem as FamilyInstance);
      }
      details = dictionary.Keys.ToList<string>();
      details.Sort();
    }
    View activeView = activeUiDocument.ActiveView;
    List<InsulationDetail> collection = new List<InsulationDetail>();
    foreach (string str2 in details)
    {
      try
      {
        InsulationDetail insulationDetail = new InsulationDetail(dictionary[str2], settingsObject, str2);
        collection.Add(insulationDetail);
        activeView.Name.StartsWith("INS -");
      }
      catch (Exception ex)
      {
        return (Result) 1;
      }
    }
    InsulationDrawingExternalEvent handler = new InsulationDrawingExternalEvent();
    handler.details.AddRange((IEnumerable<InsulationDetail>) collection);
    ExternalEvent journalable = ExternalEvent.CreateJournalable((IExternalEventHandler) handler);
    App.insulationDrawingWindow = new InsulationDrawingWindow(Process.GetCurrentProcess().MainWindowHandle, activeUiDocument, details, journalable, handler);
    App.insulationDrawingWindow.Show();
    return (Result) 0;
  }
}
