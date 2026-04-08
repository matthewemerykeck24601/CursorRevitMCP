// Decompiled with JetBrains decompiler
// Type: EDGE.GeometryTools.MarkOppIndicatorsOnActiveView
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using Utils.CollectionUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.GeometryTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class MarkOppIndicatorsOnActiveView : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    string name1 = "Mark_Opp_Indicators";
    string str1 = "Mark_Opp_Indicators On";
    string str2 = "Failure: There was an error turning ON all Mark_Opp_Indicators.";
    FilteredElementCollector elementsInView = StructuralFraming.GetElementsInView();
    string name2 = str1;
    using (Transaction transaction = new Transaction(document, name2))
    {
      try
      {
        App.DialogSwitches.SuspendModelLockingforOperation = true;
        int num1 = (int) transaction.Start();
        foreach (Element element in elementsInView)
        {
          Parameter parameter = element.LookupParameter(name1);
          if (parameter != null && !parameter.IsReadOnly)
            parameter.Set(1);
        }
        int num2 = (int) transaction.Commit();
        return (Result) 0;
      }
      catch (Exception ex)
      {
        if (transaction.HasStarted())
        {
          int num = (int) transaction.RollBack();
        }
        message = str2 + Environment.NewLine + Environment.NewLine + ex?.ToString();
        return (Result) -1;
      }
      finally
      {
        App.DialogSwitches.SuspendModelLockingforOperation = false;
      }
    }
  }
}
