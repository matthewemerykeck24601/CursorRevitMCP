// Decompiled with JetBrains decompiler
// Type: EDGE.__Testing.PopulateAssemblyFields
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.__Testing;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class PopulateAssemblyFields : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    using (Transaction transaction = new Transaction(document, "Populate Assembly Fields"))
    {
      try
      {
        int num1 = (int) transaction.Start();
        foreach (Element element in new FilteredElementCollector(document).OfClass(typeof (AssemblyInstance)))
        {
          Utils.AssemblyUtils.Parameters.UpdateDescription(element);
          List<string> source = new List<string>()
          {
            "Keith",
            "Greg",
            "JWatkins",
            "Heather",
            "Shannon",
            "Heath",
            "John"
          };
          Random random = new Random();
          Parameter parameter1 = Utils.ElementUtils.Parameters.LookupParameter(element, "TICKET_REINFORCED_DATE_CURRENT");
          Parameter parameter2 = Utils.ElementUtils.Parameters.LookupParameter(element, "TICKET_CREATED_DATE_CURRENT");
          Parameter parameter3 = Utils.ElementUtils.Parameters.LookupParameter(element, "TICKET_DETAILED_DATE_CURRENT");
          Parameter parameter4 = Utils.ElementUtils.Parameters.LookupParameter(element, "TICKET_RELEASED_DATE_CURRENT");
          Parameter parameter5 = Utils.ElementUtils.Parameters.LookupParameter(element, "TICKET_REINFORCED_USER_CURRENT");
          Parameter parameter6 = Utils.ElementUtils.Parameters.LookupParameter(element, "TICKET_CREATED_USER_CURRENT");
          Parameter parameter7 = Utils.ElementUtils.Parameters.LookupParameter(element, "TICKET_DETAILED_USER_CURRENT");
          Parameter parameter8 = Utils.ElementUtils.Parameters.LookupParameter(element, "TICKET_RELEASED_USER_CURRENT");
          string str1 = new DateTime(random.Next(2014, 2016), random.Next(1, 13), random.Next(1, 28), random.Next(0, 24), random.Next(0, 60), random.Next(0, 60)).ToString("yyyy-MM-dd-HH-mm-ss");
          string str2 = new DateTime(random.Next(2014, 2016), random.Next(1, 13), random.Next(1, 28), random.Next(0, 24), random.Next(0, 60), random.Next(0, 60)).ToString("yyyy-MM-dd-HH-mm-ss");
          string str3 = new DateTime(random.Next(2014, 2016), random.Next(1, 13), random.Next(1, 28), random.Next(0, 24), random.Next(0, 60), random.Next(0, 60)).ToString("yyyy-MM-dd-HH-mm-ss");
          string str4 = new DateTime(random.Next(2014, 2016), random.Next(1, 13), random.Next(1, 28), random.Next(0, 24), random.Next(0, 60), random.Next(0, 60)).ToString("yyyy-MM-dd-HH-mm-ss");
          parameter1.Set(str1);
          parameter2.Set(str2);
          parameter3.Set(str3);
          parameter4.Set(str4);
          parameter5.Set(source.ElementAt<string>(random.Next(0, source.Count)));
          parameter6.Set(source.ElementAt<string>(random.Next(0, source.Count)));
          parameter7.Set(source.ElementAt<string>(random.Next(0, source.Count)));
          string str5 = source.ElementAt<string>(random.Next(0, source.Count));
          parameter8.Set(str5);
        }
        int num2 = (int) transaction.Commit();
        return (Result) 0;
      }
      catch (Exception ex)
      {
        int num = (int) transaction.RollBack();
        message = ex.ToString();
        return (Result) -1;
      }
    }
  }
}
