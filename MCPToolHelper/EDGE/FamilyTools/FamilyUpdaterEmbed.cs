// Decompiled with JetBrains decompiler
// Type: EDGE.FamilyTools.FamilyUpdaterEmbed
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
internal class FamilyUpdaterEmbed : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    Application application = activeUiDocument.Application.Application;
    string str1 = "Embed Family Parameter Updater";
    string str2 = "Error:  Run inside the Family Editor.";
    string str3 = "NA";
    if (document.OwnerFamily != null)
      str3 = document.OwnerFamily.FamilyCategory.Name;
    string parametersFilename = application.SharedParametersFilename;
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
    using (Transaction transaction = new Transaction(document, "Update Embed Family Parameters"))
    {
      if (!str3.Equals("Structural Framing"))
      {
        try
        {
          TaskDialog taskDialog = new TaskDialog("Family Updater");
          taskDialog.Id = "ID_Embed_Family_Updater";
          taskDialog.MainIcon = (TaskDialogIcon) (int) ushort.MaxValue;
          taskDialog.Title = "Embed Family Updater";
          taskDialog.TitleAutoPrefix = true;
          taskDialog.AllowCancellation = true;
          taskDialog.MainInstruction = "Embed Family Updater";
          taskDialog.MainContent = "Choose the type of Embed family to update";
          taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
          taskDialog.ExpandedContent = "This tool updates existing Embed families with previous version parameters to the current version of EDGE^R.  This tool is also useful to create a new family from a blank template with the proper parameters, or to convert a family from one Manufacture Component type to that of Embed.";
          taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "This is a Precaster Standard Embed Family.");
          taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "This is a Custom Embed Family for my project.");
          taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1003, "This is a Manufactured Part Embed Family.");
          taskDialog.CommonButtons = (TaskDialogCommonButtons) 8;
          taskDialog.DefaultButton = (TaskDialogResult) 2;
          TaskDialogResult taskDialogResult = taskDialog.Show();
          if (taskDialogResult == 1001)
          {
            int num1 = (int) transaction.Start();
            string aManufactureComponent = "EMBED STANDARD";
            if (new TaskDialog("Finishes")
            {
              MainInstruction = "Does this family use Finishes?",
              CommonButtons = ((TaskDialogCommonButtons) 6),
              DefaultButton = ((TaskDialogResult) 7),
              FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
            }.Show() == 6)
            {
              FamUpdaterControl.Updater(application, aFilePath, true, aManufactureComponent);
              int num2 = (int) transaction.Commit();
              return (Result) 0;
            }
            FamUpdaterControl.Updater(application, aFilePath, false, aManufactureComponent);
            int num3 = (int) transaction.Commit();
            return (Result) 0;
          }
          if (taskDialogResult == 1002)
          {
            int num4 = (int) transaction.Start();
            string aManufactureComponent = "EMBED CUSTOM";
            if (new TaskDialog("Finishes")
            {
              MainInstruction = "Does this family use Finishes?",
              CommonButtons = ((TaskDialogCommonButtons) 6),
              DefaultButton = ((TaskDialogResult) 7),
              FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
            }.Show() == 6)
            {
              FamUpdaterControl.Updater(application, aFilePath, true, aManufactureComponent);
              int num5 = (int) transaction.Commit();
              return (Result) 0;
            }
            FamUpdaterControl.Updater(application, aFilePath, false, aManufactureComponent);
            int num6 = (int) transaction.Commit();
            return (Result) 0;
          }
          if (taskDialogResult != 1003)
            return (Result) 1;
          int num7 = (int) transaction.Start();
          string aManufactureComponent1 = "EMBED MANUFACTURED PRODUCT";
          if (new TaskDialog("Finishes")
          {
            MainInstruction = "Does this family use Finishes?",
            CommonButtons = ((TaskDialogCommonButtons) 6),
            DefaultButton = ((TaskDialogResult) 7),
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
          }.Show() == 6)
          {
            FamUpdaterControl.Updater(application, aFilePath, true, aManufactureComponent1);
            int num8 = (int) transaction.Commit();
            return (Result) 0;
          }
          FamUpdaterControl.Updater(application, aFilePath, false, aManufactureComponent1);
          int num9 = (int) transaction.Commit();
          return (Result) 0;
        }
        catch (Exception ex)
        {
          if (ex.ToString().Contains("System.IndexOutOfRangeException: Index was outside the bounds of the array."))
          {
            message = "The family you are attempting to update contains more than 50 Types and is not supported by EDGE. Please reduce the number of types or manually update the family.";
            if (transaction.GetStatus() == TransactionStatus.Started)
            {
              int num = (int) transaction.RollBack();
            }
            if (!string.IsNullOrEmpty(parametersFilename))
              application.SharedParametersFilename = parametersFilename;
            return (Result) -1;
          }
          if (transaction.GetStatus() == TransactionStatus.Started)
          {
            int num = (int) transaction.RollBack();
            message = "Update Family error. \n" + ex?.ToString();
            if (!string.IsNullOrEmpty(parametersFilename))
              application.SharedParametersFilename = parametersFilename;
            return (Result) -1;
          }
        }
      }
      new TaskDialog(str1)
      {
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
        MainInstruction = "This is a Structural Framing Category Family and the tool you selected is not intended to update a Structural Framing Category family."
      }.Show();
      return (Result) -1;
    }
  }
}
