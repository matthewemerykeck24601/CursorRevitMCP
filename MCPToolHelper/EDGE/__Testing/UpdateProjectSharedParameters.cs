// Decompiled with JetBrains decompiler
// Type: EDGE.__Testing.UpdateProjectSharedParameters
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using Utils.AdminUtils;
using Utils.Forms;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.__Testing;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class UpdateProjectSharedParameters : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    Application application = commandData.Application.Application;
    EdgeBuildInformation.GetSharedParametersPath();
    if (document.IsFamilyDocument)
    {
      new TaskDialog("Shared Parameters")
      {
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
        MainInstruction = "Error: Run inside the Project Environment and not within the Family Editor"
      }.Show();
      return (Result) 1;
    }
    ProjectParametersManufacturerForm manufacturerForm = new ProjectParametersManufacturerForm();
    int num1 = (int) manufacturerForm.ShowDialog();
    string precastmanufacturer = manufacturerForm.Precastmanufacturer;
    using (Transaction transaction = new Transaction(document, "Update Project Shared Parameters"))
    {
      try
      {
        int num2 = (int) transaction.Start();
        ProjectParameters.AddProjectParameters(precastmanufacturer);
        int num3 = (int) transaction.Commit();
        return (Result) 0;
      }
      catch (Exception ex)
      {
        if (transaction.HasStarted())
        {
          int num4 = (int) transaction.RollBack();
        }
        message = ex.ToString();
        return (Result) -1;
      }
    }
  }
}
