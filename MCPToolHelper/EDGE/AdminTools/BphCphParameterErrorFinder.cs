// Decompiled with JetBrains decompiler
// Type: EDGE.AdminTools.BphCphParameterErrorFinder
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.AdminTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public class BphCphParameterErrorFinder : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    Application application = activeUiDocument.Application.Application;
    string str1 = "";
    string str2 = "";
    string str3 = "";
    if (document.IsFamilyDocument)
    {
      TaskDialog.Show("BPH CPH Parameter Error Finder", "Error:  Run inside the Project Environment and not insuide the Family Editor.");
      return (Result) 1;
    }
    using (Transaction transaction = new Transaction(document, "Update Cast Family (CIP) Parameters"))
    {
      FilteredElementCollector elementCollector = new FilteredElementCollector(document);
      List<BuiltInCategory> categories = new List<BuiltInCategory>();
      categories.Add(BuiltInCategory.OST_StructConnections);
      categories.Add(BuiltInCategory.OST_StructuralFraming);
      categories.Add(BuiltInCategory.OST_GenericModel);
      categories.Add(BuiltInCategory.OST_SpecialityEquipment);
      ElementMulticategoryFilter filter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) categories);
      try
      {
        int num1 = (int) transaction.Start();
        foreach (Element wherePass in new FilteredElementCollector(document).OfClass(typeof (FamilySymbol)).WherePasses((ElementFilter) filter))
        {
          Parameter parameter1 = wherePass.LookupParameter("BOM_PRODUCT_HOST");
          Parameter parameter2 = wherePass.LookupParameter("CONSTRUCTION_PRODUCT_HOST");
          str3 = wherePass.Name;
          if (parameter1 != null || parameter2 != null)
          {
            if (parameter1 != null)
            {
              str1 = !string.IsNullOrEmpty(str1) ? $"{str1}, {wherePass.Name}" : wherePass.Name;
              if (parameter1.IsReadOnly)
                str1 = !string.IsNullOrEmpty(str1) ? $"{str1}, {wherePass.Name}" : wherePass.Name;
            }
            if (parameter2 != null)
            {
              str2 = !string.IsNullOrEmpty(str2) ? $"{str2}, {wherePass.Name}" : wherePass.Name;
              if (parameter2.IsReadOnly)
                str2 = !string.IsNullOrEmpty(str2) ? $"{str2}, {wherePass.Name}" : wherePass.Name;
            }
          }
        }
        if (string.IsNullOrEmpty(str1))
          str1 = "\t No families with BPH parameter errors.";
        if (string.IsNullOrEmpty(str2))
          str2 = "\t No families with CPH parameter errors.";
        new TaskDialog("BPH CPH Parameter Error Finder")
        {
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
          MainInstruction = $"Families with BPH Paremeter Errors: {Environment.NewLine}{str1}{Environment.NewLine}{Environment.NewLine}Families with CPH Parameter Errors: {Environment.NewLine}{str2}"
        }.Show();
        int num2 = (int) transaction.Commit();
        return (Result) 0;
      }
      catch (Exception ex)
      {
        if (transaction.HasStarted())
        {
          int num = (int) transaction.RollBack();
        }
        message = $"BPH/CPH Parameter Error Finder error in family: {Environment.NewLine}({str3}).{Environment.NewLine}{ex?.ToString()}";
        return (Result) -1;
      }
    }
  }
}
