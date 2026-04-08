// Decompiled with JetBrains decompiler
// Type: EDGE.SchedulingTools.ScheduleSequenceByView
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.IUpdaters.ModelLocking;
using System;
using Utils.Forms;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.SchedulingTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class ScheduleSequenceByView : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    string name = "PROD_ERECT_SEQUENCE";
    FilteredElementCollector elementCollector = new FilteredElementCollector(document, document.ActiveView.Id).OfClass(typeof (FamilyInstance));
    using (Transaction transaction = new Transaction(document, "Schedule Sequence By View"))
    {
      try
      {
        if (!ModelLockingUtils.ShowPermissionsDialog(document, ModelLockingToolPermissions.ScheduleByView))
          return (Result) 1;
        App.DialogSwitches.SuspendModelLockingforOperation = true;
        DataCollectorForm dataCollectorForm = new DataCollectorForm();
        dataCollectorForm.Text = "Sequence by View";
        dataCollectorForm.label.Text = $"Enter the Schedule Sequence value to append to all Elements in the Active View.{Environment.NewLine}{Environment.NewLine}Enter the text \"RESET\" to empty all Schedule Sequence values from the elements visible on the screen.";
        int num1 = (int) dataCollectorForm.ShowDialog();
        if (!dataCollectorForm.isContinue)
          return (Result) 1;
        string upper = dataCollectorForm.textBox.Text.ToUpper();
        int num2 = (int) transaction.Start();
        foreach (Element element in elementCollector)
        {
          Parameter parameter = element.LookupParameter(name);
          if (upper.Equals("RESET"))
          {
            if (parameter != null)
            {
              parameter.AsString();
              parameter.Set("");
            }
          }
          else if (parameter != null)
          {
            string str1 = parameter.AsString() ?? "";
            if (!str1.Equals(""))
            {
              if (upper == "")
              {
                parameter.Set(str1);
              }
              else
              {
                bool flag = false;
                string str2 = str1;
                char[] chArray = new char[1]{ ',' };
                foreach (string str3 in str2.Split(chArray))
                {
                  if (str3.Trim().Equals(upper.Trim()))
                  {
                    flag = true;
                    break;
                  }
                }
                if (flag)
                  parameter.Set(str1);
                else
                  parameter.Set($"{str1}, {upper}");
              }
            }
            else if (upper != "")
              parameter.Set(upper);
          }
        }
        int num3 = (int) transaction.Commit();
        return (Result) 0;
      }
      catch (Exception ex)
      {
        if (transaction.HasStarted())
        {
          int num = (int) transaction.RollBack();
        }
        message = ex.ToString();
        return (Result) -1;
      }
      finally
      {
        App.DialogSwitches.SuspendModelLockingforOperation = false;
      }
    }
  }
}
