// Decompiled with JetBrains decompiler
// Type: EDGE.FamilyTools.FamilyUpdaterBlaster
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
public class FamilyUpdaterBlaster : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    Application application = activeUiDocument.Application.Application;
    string str1 = "Family Update Blaster";
    string str2 = "Error:  Run inside the Family Editor.";
    string str3 = "NA";
    if (document.OwnerFamily != null)
      str3 = document.OwnerFamily.FamilyCategory.Name;
    string aManufactureComponent = "";
    bool flag = false;
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
    using (Transaction transaction = new Transaction(document, "Update Family Blaster"))
    {
      if (!str3.Equals("Structural Framing"))
      {
        try
        {
          FamilyManager familyManager = document.FamilyManager;
          FamilyType currentType = document.FamilyManager.CurrentType;
          foreach (FamilyParameter parameter in familyManager.Parameters)
          {
            switch (parameter.Definition.Name)
            {
              case "FINISH_BLACK":
                flag = true;
                continue;
              case "MANUFACTURE_COMPONENT":
                aManufactureComponent = FamParameterGetValue.FamilyParamValueString(currentType, parameter, document);
                continue;
              default:
                continue;
            }
          }
          int num1 = (int) transaction.Start();
          if (flag)
          {
            FamUpdaterControl.Updater(application, aFilePath, true, aManufactureComponent);
            int num2 = (int) transaction.Commit();
            return (Result) 0;
          }
          FamUpdaterControl.Updater(application, aFilePath, false, aManufactureComponent);
          int num3 = (int) transaction.Commit();
          return (Result) 0;
        }
        catch (Exception ex)
        {
          if (transaction.HasStarted())
          {
            int num = (int) transaction.RollBack();
          }
          message = "Update Family Blaster error. \n" + ex?.ToString();
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
}
