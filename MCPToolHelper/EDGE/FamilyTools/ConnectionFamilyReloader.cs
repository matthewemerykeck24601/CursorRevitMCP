// Decompiled with JetBrains decompiler
// Type: EDGE.FamilyTools.ConnectionFamilyReloader
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using Utils.FamilyUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.FamilyTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public class ConnectionFamilyReloader : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    string str = "NA";
    if (document.OwnerFamily != null)
      str = document.OwnerFamily.FamilyCategory.Name;
    string aManComponent = "";
    if (!document.IsFamilyDocument)
    {
      new TaskDialog("Connection Family Reloader")
      {
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
        MainInstruction = "Error:  Run inside the Family Editor."
      }.Show();
      return (Result) 1;
    }
    FamilyManager familyManager = document.FamilyManager;
    FamilyType currentType = document.FamilyManager.CurrentType;
    foreach (FamilyParameter parameter in familyManager.Parameters)
    {
      if (parameter.Definition.Name == "MANUFACTURE_COMPONENT")
        aManComponent = FamParameterGetValue.FamilyParamValueString(currentType, parameter, document);
    }
    using (Transaction transaction = new Transaction(document, "Reload Nested Families"))
    {
      if (!str.Equals("Structural Framing"))
      {
        try
        {
          int num = (int) transaction.Start();
          FamReloader.Reloader(aManComponent);
          return (Result) 0;
        }
        catch (Exception ex)
        {
          if (transaction.HasStarted())
          {
            int num = (int) transaction.RollBack();
          }
          message = "Reloading Nested Families error. \n" + ex?.ToString();
          return (Result) -1;
        }
      }
      else
      {
        new TaskDialog("Connection Family Reloader")
        {
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
          MainInstruction = "This is a Structural Framing Category Family and the tool you selected is not intended to update a Structural Framing Category family."
        }.Show();
        return (Result) -1;
      }
    }
  }
}
