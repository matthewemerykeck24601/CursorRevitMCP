// Decompiled with JetBrains decompiler
// Type: EDGE.FamilyTools.FamilyFinishesRemover
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using Utils.AdminUtils;
using Utils.FamilyUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.FamilyTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public class FamilyFinishesRemover : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    Application application = activeUiDocument.Application.Application;
    string str1 = "Finish Parameter Remover";
    string str2 = "Error:  Run inside the Family Editor.";
    string str3 = "NA";
    if (document.OwnerFamily != null)
      str3 = document.OwnerFamily.FamilyCategory.Name;
    string aFilePath = !EdgeBuildInformation.IsDebugCheck ? EdgeBuildInformation.GetSharedParametersPath() : EdgeBuildInformation.GetSharedParametersPath();
    if (!document.IsFamilyDocument)
    {
      new TaskDialog(str1)
      {
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
        MainInstruction = str2
      }.Show();
      return (Result) 1;
    }
    FamilyManager familyManager = document.FamilyManager;
    FamilyType currentType = document.FamilyManager.CurrentType;
    FamilyParameter fp = familyManager.get_Parameter("MANUFACTURE_COMPONENT");
    string aManufactureComponent = fp == null ? "" : FamParameterGetValue.FamilyParamValueString(currentType, fp, document);
    using (Transaction transaction = new Transaction(document, "Remove Finish Parameters"))
    {
      if (!str3.Equals("Structural Framing"))
      {
        try
        {
          TaskDialog taskDialog = new TaskDialog("Finish Remover");
          taskDialog.Id = "ID_Family_Finish_Remover";
          taskDialog.MainIcon = (TaskDialogIcon) (int) ushort.MaxValue;
          taskDialog.Title = "Family Finish Remover";
          taskDialog.TitleAutoPrefix = true;
          taskDialog.AllowCancellation = true;
          taskDialog.MainInstruction = "Remove Finish Related Parameters";
          taskDialog.MainContent = "Remove Finish Related Parameters";
          taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
          taskDialog.ExpandedContent = "This tools removes all finish related parameters in the family.";
          taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Remove all finish related parameters from this family");
          taskDialog.CommonButtons = (TaskDialogCommonButtons) 8;
          taskDialog.DefaultButton = (TaskDialogResult) 2;
          if (taskDialog.Show() != 1001)
            return (Result) 1;
          int num1 = (int) transaction.Start();
          if (new TaskDialog("Confirmation of Deletion")
          {
            MainInstruction = "Are you sure you want to remove all finish related parameters?",
            CommonButtons = ((TaskDialogCommonButtons) 6),
            DefaultButton = ((TaskDialogResult) 7),
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
          }.Show() != 6)
            return (Result) 1;
          string paramCm = "";
          string paramIds = "";
          string paramId = "";
          string paramIc = "";
          string paramMpd = "";
          string paramMpid = "";
          foreach (FamilyParameter parameter in familyManager.Parameters)
          {
            switch (parameter.Definition.Name)
            {
              case "CONTROL_MARK":
                paramCm = currentType.AsString(parameter);
                continue;
              case "IDENTITY_DESCRIPTION_SHORT":
                paramIds = currentType.AsString(parameter);
                continue;
              case "IDENTITY_DESCRIPTION":
                paramId = currentType.AsString(parameter);
                continue;
              case "IDENTITY_COMMENT":
                paramIc = currentType.AsString(parameter);
                continue;
              case "MANUFACTURER_PLANT_DESCRIPTION":
                paramMpd = currentType.AsString(parameter);
                continue;
              case "MANUFACTURER_PLANT_ID":
                paramMpid = currentType.AsString(parameter);
                continue;
              default:
                continue;
            }
          }
          FamUpdaterFormulas.RemoveFinishFormulas();
          FamUpdaterControl.Updater(application, aFilePath, false, aManufactureComponent);
          this.addParamValues(paramCm, paramIds, paramId, paramIc, paramMpd, paramMpid);
          int num2 = (int) transaction.Commit();
          return (Result) 0;
        }
        catch (Exception ex)
        {
          if (transaction.HasStarted())
          {
            int num = (int) transaction.RollBack();
          }
          message = "Remove all finish related paramters and formulas error. \n" + ex?.ToString();
          return (Result) -1;
        }
      }
      else
      {
        new TaskDialog(str1)
        {
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
          MainInstruction = "This is a Structural Framing Category Family and the tool you selected is not intended to update a Structural Framing Category family."
        }.Show();
        return (Result) -1;
      }
    }
  }

  private void addParamValues(
    string paramCm,
    string paramIds,
    string paramId,
    string paramIc,
    string paramMpd,
    string paramMpid)
  {
    UIDocument uiDoc = ActiveModel.UIDoc;
    ActiveModel.GetInformation(uiDoc);
    FamilyManager familyManager = uiDoc.Document.FamilyManager;
    foreach (FamilyParameter parameter in familyManager.Parameters)
    {
      switch (parameter.Definition.Name)
      {
        case "CONTROL_MARK":
          familyManager.Set(parameter, paramCm);
          continue;
        case "IDENTITY_DESCRIPTION_SHORT":
          familyManager.Set(parameter, paramIds);
          continue;
        case "IDENTITY_DESCRIPTION":
          familyManager.Set(parameter, paramId);
          continue;
        case "IDENTITY_COMMENT":
          familyManager.Set(parameter, paramIc);
          continue;
        case "MANUFACTURER_PLANT_DESCRIPTION":
          familyManager.Set(parameter, paramMpd);
          continue;
        case "MANUFACTURER_PLANT_ID":
          familyManager.Set(parameter, paramMpid);
          continue;
        default:
          continue;
      }
    }
  }
}
