// Decompiled with JetBrains decompiler
// Type: EDGE.FamilyTools.FamilyUpdaterProducts
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
using Utils.Forms;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.FamilyTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class FamilyUpdaterProducts : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    Application application = activeUiDocument.Application.Application;
    string str = "NA";
    if (document.OwnerFamily != null)
      str = document.OwnerFamily.FamilyCategory.Name;
    int index1 = 0;
    string[] strArray1 = new string[50];
    string[] strArray2 = new string[50];
    string[] strArray3 = new string[50];
    string[] strArray4 = new string[50];
    string[] strArray5 = new string[50];
    string[] strArray6 = new string[50];
    string[] strArray7 = new string[50];
    string afilePath = !EdgeBuildInformation.IsDebugCheck ? EdgeBuildInformation.GetSharedParametersPath() : EdgeBuildInformation.GetSharedParametersPath();
    if (!document.IsFamilyDocument)
    {
      new TaskDialog("Structural Framing Family (PRODUCS) Parameter Updater")
      {
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
        MainInstruction = "Error:  Run inside the Family Editor."
      }.Show();
      return (Result) 1;
    }
    FamilyManager familyManager = document.FamilyManager;
    string parametersFilename = application.SharedParametersFilename;
    using (Transaction transaction = new Transaction(document, "Update Product Family Parameters"))
    {
      if (str.Equals("Structural Framing"))
      {
        try
        {
          int num1 = (int) transaction.Start();
          if (document.FamilyManager.Types.Size == 0)
            document.FamilyManager.NewType("API Generated Family Type");
          string name = familyManager.CurrentType.Name;
          foreach (FamilyType type in familyManager.Types)
          {
            familyManager.CurrentType = type;
            strArray1[index1] = familyManager.CurrentType.Name;
            foreach (FamilyParameter parameter in familyManager.Parameters)
            {
              switch (parameter.Definition.Name)
              {
                case "CONTROL_MARK":
                  strArray2[index1] = SharedParameterCollectorUpdater.Collector(application, parameter);
                  continue;
                case "IDENTITY_DESCRIPTION_SHORT":
                  strArray3[index1] = SharedParameterCollectorUpdater.Collector(application, parameter);
                  continue;
                case "IDENTITY_DESCRIPTION":
                  strArray4[index1] = SharedParameterCollectorUpdater.Collector(application, parameter);
                  continue;
                case "IDENTITY_COMMENT":
                  strArray5[index1] = SharedParameterCollectorUpdater.Collector(application, parameter);
                  continue;
                case "MANUFACTURER_PLANT_DESCRIPTION":
                  strArray6[index1] = SharedParameterCollectorUpdater.Collector(application, parameter);
                  continue;
                case "MANUFACTURER_PLANT_ID":
                  strArray7[index1] = SharedParameterCollectorUpdater.Collector(application, parameter);
                  continue;
                default:
                  continue;
              }
            }
            ++index1;
          }
          int num2 = (int) new ProductSelectionForm(application, afilePath).ShowDialog();
          foreach (FamilyType type in familyManager.Types)
          {
            familyManager.CurrentType = type;
            for (int index2 = 0; index2 <= index1; ++index2)
            {
              if (type.Name.Equals(strArray1[index2]))
              {
                if (string.IsNullOrWhiteSpace(strArray2[index2]))
                  strArray2[index2] = "edit";
                if (string.IsNullOrWhiteSpace(strArray3[index2]))
                  strArray3[index2] = "edit";
                if (string.IsNullOrWhiteSpace(strArray4[index2]))
                  strArray4[index2] = "edit";
                if (string.IsNullOrWhiteSpace(strArray5[index2]))
                  strArray5[index2] = " ";
                if (string.IsNullOrWhiteSpace(strArray6[index2]))
                  strArray6[index2] = " ";
                if (string.IsNullOrWhiteSpace(strArray7[index2]))
                  strArray7[index2] = " ";
                SharedParameterCollectorUpdater.Updater(application, true, strArray2[index2], strArray3[index2], strArray4[index2], strArray5[index2], strArray6[index2], strArray7[index2]);
                break;
              }
            }
          }
          foreach (FamilyType type in familyManager.Types)
          {
            if (type.Name == name)
            {
              familyManager.CurrentType = type;
              break;
            }
          }
          if (!string.IsNullOrEmpty(parametersFilename))
            application.SharedParametersFilename = parametersFilename;
          int num3 = (int) transaction.Commit();
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
            return (Result) 1;
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
      new TaskDialog("Structural Framing Family (PRODUCS) Parameter Updater")
      {
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
        MainInstruction = "This is NOT a Structural Framing Category Family and the tool you selected is intended to update ONLY Structural Framing Category families."
      }.Show();
      return (Result) -1;
    }
  }
}
