// Decompiled with JetBrains decompiler
// Type: EDGE.AdminTools.ProjectSharedParameters
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.IUpdaters.ModelLocking;
using EDGE.UserSettingTools.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utils;
using Utils.ElementUtils;
using Utils.ProjectUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.AdminTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class ProjectSharedParameters : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    Application application = activeUiDocument.Application.Application;
    string path = "C:\\EDGEforRevit\\Shared_Params_2015_v01.txt";
    string parametersFilename = application.SharedParametersFilename;
    if (Utils.ViewUtils.ViewUtils.CurrentViewIsProjectBrowser(document))
      return (Result) 1;
    if (document.IsFamilyDocument)
    {
      new TaskDialog("Shared Parameters")
      {
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
        MainInstruction = "Error:  Run inside the Project Environment and not within the Family Editor."
      }.Show();
      return (Result) 1;
    }
    if (!ModelLockingUtils.ShowPermissionsDialog(document, ModelLockingToolPermissions.ProjectShared))
      return (Result) 1;
    if (!File.Exists(path))
    {
      new TaskDialog("Error")
      {
        MainContent = $"The required EDGE shared parameter File path is missing. Please check that the file exists and that your Revit shared parameter settings are correct and run this tool again.{Environment.NewLine}{Environment.NewLine}Expected Path: {path}",
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
      }.Show();
      return (Result) 1;
    }
    try
    {
      new StreamReader(path).Close();
    }
    catch (Exception ex)
    {
      new TaskDialog("EDGE^R")
      {
        AllowCancellation = false,
        MainInstruction = "Shared Parameter Settings File Error.",
        MainContent = $"Check Shared Parameter Settings File at: {path}. Please ensure the file is not in use by another application and try again."
      }.Show();
      return (Result) 1;
    }
    if (!application.SharedParametersFilename.ToUpper().Equals(path.ToUpper()))
    {
      new TaskDialog("Error")
      {
        MainContent = $"Your Revit shared parameter file path is incorrect for EDGE shared parameters. Please check your Revit shared parameter settings and run this tool again.{Environment.NewLine}{Environment.NewLine}Expected Path: {path}{Environment.NewLine}{Environment.NewLine}Current Path: {application.SharedParametersFilename}",
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
      }.Show();
      return (Result) 1;
    }
    Parameter parameter1 = document.ProjectInformation.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER");
    if (parameter1 != null && parameter1.AsString() != null)
    {
      string precastManufacturer = parameter1.AsString();
      using (Transaction transaction1 = new Transaction(document, "Update Project Shared Parameters"))
      {
        using (Transaction transaction2 = new Transaction(document, "Initialize Count Multiplier"))
        {
          try
          {
            int num1 = (int) transaction1.Start();
            bool flag1 = false;
            if (document.ProjectInformation.LookupParameter("INSULATION_VOLUME_TOLERANCE") != null)
              flag1 = true;
            bool flag2 = false;
            if (document.ProjectInformation.LookupParameter("INSULATION_VOLUME_TOLERANCE_PERCENTAGE") != null)
              flag2 = true;
            bool flag3 = false;
            if (document.ProjectInformation.LookupParameter("INSULATION_GEOMETRY_TOLERANCE") != null)
              flag3 = true;
            this.DeleteLineStyles(document);
            this.CreateNewLineStyle(document);
            if (App.Edge_Cloud_Id == -1)
              ProjectParameters2.ProjectParametersAdd(precastManufacturer, false);
            else
              ProjectParameters2.ProjectParametersAdd(precastManufacturer, true);
            ProjectParameters2.SetProjectInformationManufacturerName(precastManufacturer);
            if (!string.IsNullOrEmpty(parametersFilename))
              application.SharedParametersFilename = parametersFilename;
            ProjectParameters2.UpdateWarpingFilters(document);
            ProjectParameters2.CreateEntourageFilter(document);
            ProjectParameters2.CreateWarpedElementFilter(document);
            int num2 = ProjectParameters2.CreateErectionSequenceUnassignedFilter(document) ? 1 : 0;
            bool sequenceAssignedFilter = ProjectParameters2.CreateErectionSequenceAssignedFilter(document);
            if (num2 == 0 || !sequenceAssignedFilter)
              new TaskDialog("Project Shared Parameters")
              {
                MainInstruction = "Failed to create or update erection sequence filters.",
                MainContent = "Unable to create or update the ERECTION_SEQUENCE_UNASSIGNED and ERECTION_SEQUENCE_ASSIGNED filters. Ensure the ERECTION_SEQUENCE_NUMBER parameter is a shared parameter and run Project Shared Parameters again."
              }.Show();
            ProjectParameters2.CreateErectionSequenceViewTemplates(document);
            ProjectParameters2.CreateHardwareDetailFilter(document);
            Dictionary<ElementId, string> dictionary1 = new Dictionary<ElementId, string>();
            List<View> list = new FilteredElementCollector(document).OfClass(typeof (View)).Cast<View>().Where<View>((Func<View, bool>) (x => x.IsTemplate)).ToList<View>();
            FilteredElementCollector elementCollector1 = new FilteredElementCollector(document).OfClass(typeof (ParameterFilterElement));
            document.Regenerate();
            ElementId filterElementId = (ElementId) null;
            foreach (ParameterFilterElement parameterFilterElement in elementCollector1)
            {
              if (parameterFilterElement.Name == "HARDWARE_DETAIL")
              {
                filterElementId = parameterFilterElement.Id;
                break;
              }
            }
            if (filterElementId != (ElementId) null)
            {
              foreach (View view in list)
              {
                if (!view.Name.StartsWith("_HWD-"))
                {
                  try
                  {
                    if (!view.IsFilterApplied(filterElementId))
                    {
                      view.AddFilter(filterElementId);
                      view.SetFilterVisibility(filterElementId, false);
                    }
                  }
                  catch (Exception ex)
                  {
                    if (!ex.Message.Contains("The view type does not support Visibility/Graphics Overriddes."))
                      dictionary1.Add(view.Id, view.Name);
                  }
                }
              }
            }
            if (dictionary1.Count > 0)
            {
              TaskDialog taskDialog1 = new TaskDialog("Project Shared Parameters");
              taskDialog1.MainContent = "Project Shared Parameters was unable to add a Hardware Detail visibility filter to one or more view templates.";
              foreach (KeyValuePair<ElementId, string> keyValuePair in dictionary1)
              {
                TaskDialog taskDialog2 = taskDialog1;
                taskDialog2.ExpandedContent = $"{taskDialog2.ExpandedContent}{keyValuePair.Value} : {keyValuePair.Key?.ToString()}\n";
              }
              taskDialog1.Show();
            }
            Dictionary<ElementId, string> dictionary2 = new Dictionary<ElementId, string>();
            FilteredElementCollector elementCollector2 = new FilteredElementCollector(document).OfClass(typeof (ViewSchedule));
            SharedParameterElement parameterElement = new FilteredElementCollector(document).OfClass(typeof (SharedParameterElement)).WhereElementIsNotElementType().Cast<SharedParameterElement>().Where<SharedParameterElement>((Func<SharedParameterElement, bool>) (x => x.Name == "DO_NOT_SCHEDULE")).FirstOrDefault<SharedParameterElement>();
            string str1 = string.IsNullOrEmpty(App.TKTBOMFolderPath) ? $"C:/EDGEforREVIT/{precastManufacturer}_TICKET_BOM.xml" : $"{App.TKTBOMFolderPath}\\{precastManufacturer}_TICKET_BOM.xml";
            List<string> stringList = new List<string>();
            TKTBOMSettings tktbomSettings = new TKTBOMSettings();
            if (File.Exists(str1) && tktbomSettings.LoadTicketTemplateSettings(str1))
            {
              foreach (ItemEntry ticketBom in tktbomSettings.TicketBOMList)
              {
                if (!string.IsNullOrWhiteSpace(ticketBom.templateSchedule) && !string.IsNullOrWhiteSpace(ticketBom.scheduleSuffix))
                  stringList.Add(ticketBom.templateSchedule);
              }
            }
            if (parameterElement != null)
            {
              foreach (ViewSchedule elem in elementCollector2)
              {
                if (!(elem.ViewTemplateId != (ElementId) null) || elem.ViewTemplateId.Equals((object) ElementId.InvalidElementId))
                {
                  Parameter parameter2 = Parameters.LookupParameter((Element) elem, "HARDWARE_DETAIL");
                  if ((parameter2 == null || parameter2.AsInteger() != 1) && (!elem.IsTemplate || !elem.Name.StartsWith("_HWD-")))
                  {
                    try
                    {
                      ScheduleDefinition definition = elem.Definition;
                      if (stringList.Contains(elem.Name) && definition.GetFilterCount() > 6)
                      {
                        dictionary2.Add(elem.Id, elem.Name);
                      }
                      else
                      {
                        ScheduleField field;
                        if (!this.DoesScheduleFieldExist(definition, "DO_NOT_SCHEDULE", out field))
                          field = definition.AddField(ScheduleFieldType.Instance, parameterElement.Id);
                        field.IsHidden = true;
                        ScheduleFieldId fieldId = field.FieldId;
                        if (!this.DoesScheduleFilterExist(definition, fieldId))
                        {
                          ScheduleFilter filter = new ScheduleFilter(field.FieldId, ScheduleFilterType.NotEqual, 1);
                          definition.AddFilter(filter);
                        }
                      }
                    }
                    catch (Exception ex)
                    {
                      dictionary2.Add(elem.Id, elem.Name);
                    }
                  }
                }
              }
            }
            if (dictionary2.Count > 0)
            {
              TaskDialog taskDialog3 = new TaskDialog("Project Shared Parameters");
              taskDialog3.MainContent = "Project Shared Parameters was unable to add a DO_NOT_SCHEDULE parameter filter to one or more schedules.";
              foreach (ElementId key in dictionary2.Keys)
              {
                string str2 = dictionary2[key];
                TaskDialog taskDialog4 = taskDialog3;
                taskDialog4.ExpandedContent = $"{taskDialog4.ExpandedContent}{str2} : {key?.ToString()}\n";
              }
              taskDialog3.Show();
            }
            if (!flag1)
              document.ProjectInformation.LookupParameter("INSULATION_VOLUME_TOLERANCE")?.Set(0.01);
            if (!flag2)
              document.ProjectInformation.LookupParameter("INSULATION_VOLUME_TOLERANCE_PERCENTAGE")?.Set(1.0 / 80.0);
            if (!flag3)
              document.ProjectInformation.LookupParameter("INSULATION_GEOMETRY_TOLERANCE")?.Set(1.0 / 192.0);
            int num3 = (int) transaction1.Commit();
            if (transaction2.Start() == TransactionStatus.Started)
            {
              ProjectSharedParameters.InitializeCountMultiplierToOneForAllEdgeElements(document);
              if (transaction2.Commit() != TransactionStatus.Committed)
              {
                QAUtils.LogLine("  ERROR:  Unable to commit count multiplier initialization transaction.  Rolling back initialization transaction.");
                if (transaction2.HasStarted())
                {
                  int num4 = (int) transaction2.RollBack();
                }
              }
            }
            else
            {
              TaskDialog.Show("EDGE Error", "Unable to start transaction to initialize count multiplier.  Item counts will not be correct until BOM Product Hosting is run for the entire model.");
              QAUtils.LogLine("Unable to start transaction to initialize count multiplier.  Item counts will not be correct until BOM Product Hosting is run for the entire model.");
            }
            new TaskDialog("Shared Parameters")
            {
              FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
              MainInstruction = "All appropriate Shared Parameters have been added to the project"
            }.Show();
            return (Result) 0;
          }
          catch (Exception ex)
          {
            if (transaction1.HasStarted())
            {
              int num5 = (int) transaction1.RollBack();
            }
            if (transaction2.HasStarted())
            {
              int num6 = (int) transaction2.RollBack();
            }
            if (!string.IsNullOrEmpty(parametersFilename))
              application.SharedParametersFilename = parametersFilename;
            message = "Update Project Shared Parameters Error. \n" + ex?.ToString();
            return (Result) -1;
          }
        }
      }
    }
    else
    {
      if (parameter1 == null)
        TaskDialog.Show("Project Shared Parameters Error", "The PROJECT_CLIENT_PRECAST_MANUFACTURER is not configured in the project. Please add this as a Project Parameter applied to the Project Information category and assign a value to the parameter.");
      else if (parameter1.AsString() == null)
        TaskDialog.Show("Project Shared Parameters Error", "Please ensure that the PROJECT_CLIENT_PRECAST_MANUFACTURER parameter within Project Information has a value assigned then try again.");
      return (Result) -1;
    }
  }

  private bool DoesScheduleFieldExist(
    ScheduleDefinition schedule,
    string fieldName,
    out ScheduleField field)
  {
    field = (ScheduleField) null;
    for (int index = 0; index < schedule.GetFieldCount(); ++index)
    {
      field = schedule.GetField(index);
      if (field.GetName() == fieldName)
        return true;
    }
    return false;
  }

  private bool DoesScheduleFilterExist(ScheduleDefinition schedule, ScheduleFieldId fieldId)
  {
    foreach (ScheduleFilter filter in (IEnumerable<ScheduleFilter>) schedule.GetFilters())
    {
      if (filter.FieldId.Equals((object) fieldId))
        return true;
    }
    return false;
  }

  public static void InitializeCountMultiplierToOneForAllEdgeElements(Document doc)
  {
    List<string> readonlyvalue = (List<string>) null;
    ProjectSharedParameters.InitializeCountMultiplierToOneForAllEdgeElements(doc, out readonlyvalue);
    if (readonlyvalue.Count <= 0)
      return;
    string str = string.Join(", \n", readonlyvalue.ToArray());
    new TaskDialog("EDGE Error")
    {
      MainInstruction = "Count multiplier could not be initialized for the following elements due to their COUNT_MULTIPLIER parameter being read-only.",
      ExpandedContent = str
    }.Show();
    QAUtils.LogLine("Unable to start transaction to initialize count multiplier");
  }

  public static void InitializeCountMultiplierToOneForAllEdgeElements(
    Document doc,
    out List<string> readonlyvalue)
  {
    readonlyvalue = new List<string>();
    ElementMulticategoryFilter filter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
    {
      BuiltInCategory.OST_GenericModel,
      BuiltInCategory.OST_SpecialityEquipment,
      BuiltInCategory.OST_StructConnections
    }, false);
    foreach (Element wherePass in new FilteredElementCollector(doc).OfClass(typeof (FamilyInstance)).WherePasses((ElementFilter) filter))
    {
      Parameter parameter = wherePass.LookupParameter("COUNT_MULTIPLIER");
      if (parameter != null && parameter.AsInteger() < 1)
      {
        if (!parameter.IsReadOnly)
          parameter.Set(1);
        else
          readonlyvalue.Add($"{wherePass.Id.ToString()}- {(wherePass as FamilyInstance).Symbol.FamilyName}");
      }
    }
  }

  private void DeleteLineStyles(Document doc)
  {
    Category category1 = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Lines);
    List<Category> categoryList = new List<Category>();
    foreach (Category subCategory in category1.SubCategories)
    {
      if (subCategory.Name.Contains("EdgeRotationLine"))
        categoryList.Add(subCategory);
    }
    foreach (Category category2 in categoryList)
      doc.Delete(category2.Id);
  }

  private void CreateNewLineStyle(Document doc)
  {
    Categories categories = doc.Settings.Categories;
    Category parentCategory = categories.get_Item(BuiltInCategory.OST_Lines);
    bool flag = false;
    foreach (Category subCategory in parentCategory.SubCategories)
    {
      if (subCategory.Name.Contains("EdgeRotationLine"))
        flag = true;
    }
    if (flag)
      return;
    Category category = categories.NewSubcategory(parentCategory, "EdgeRotationLine");
    doc.Regenerate();
    category.SetLineWeight(8, GraphicsStyleType.Projection);
    category.LineColor = new Color(byte.MaxValue, (byte) 0, (byte) 0);
  }
}
