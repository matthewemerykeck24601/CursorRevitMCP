// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.InsulationDrawing.InsulationDrawingExternalEvent
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.ElementUtils;
using Utils.IEnumerable_Extensions;

#nullable disable
namespace EDGE.InsulationTools.InsulationDrawing;

public class InsulationDrawingExternalEvent : IExternalEventHandler
{
  public List<InsulationDetail> details = new List<InsulationDetail>();

  public void Execute(UIApplication app)
  {
    UIDocument activeUiDocument = app.ActiveUIDocument;
    View activeView = activeUiDocument.ActiveView;
    Document revitDoc = activeUiDocument.Document;
    if (!InsulationDrawingUtils.ValidInsulationDrawingView(activeView))
    {
      new TaskDialog("Insulation Drawing - Mark")
      {
        MainInstruction = "Invalid View.",
        MainContent = "Insulation Drawing - Mark will only place insulation details on sheets, details and legends. Please change the active view and try again."
      }.Show();
    }
    else
    {
      int num1 = 1;
      int num2 = 0;
      List<string> source = new List<string>();
      for (int index = 0; index < this.details.Count; ++index)
      {
        try
        {
          XYZ location = activeUiDocument.Selection.PickPoint((ObjectSnapTypes) 0, "Pick Placement Location for Insulation Mark: " + this.details[index].InsulationMark);
          this.details[index].DrawDetail(activeView, location);
          source.Add(this.details[index].InsulationMark);
          ++num1;
        }
        catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
        {
          TaskDialog taskDialog = new TaskDialog("Operation Canceled");
          taskDialog.MainInstruction = "There are still insulation marks queued for drawing.\n\nAre you sure you want to cancel?";
          taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Cancel", "Abort the insulation drawing operation. Already placed drawings will be preserved.");
          taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Continue", $"Resume insulation drawing operation starting with current mark {this.details[index].InsulationMark}. ({this.details.Count - index}) marks remaining.");
          TaskDialogResult taskDialogResult = taskDialog.Show();
          bool flag;
          if (taskDialogResult != 1001)
          {
            if (taskDialogResult == 1002)
            {
              --index;
              continue;
            }
            flag = true;
          }
          else
            flag = true;
          if (flag)
            break;
        }
        catch (Exception ex)
        {
          if (ex.Message.Contains("The user aborted the pick operation.") || ex.Message.Contains("Insulation Drawing Window Closed"))
          {
            if (App.insulationDrawingWindow == null)
              return;
            App.insulationDrawingWindow.Close();
            return;
          }
          TaskDialog taskDialog = new TaskDialog("Insulation Drawing - Mark");
          taskDialog.MainInstruction = "Failed to Create Insulation Drawing";
          taskDialog.MainContent = $"Unable to create Insulation Drawing for the insulation mark {this.details[index].InsulationMark}. ";
          if (num1 != this.details.Count)
          {
            taskDialog.CommonButtons = (TaskDialogCommonButtons) 6;
            taskDialog.MainContent += "Would you like to continue placing Insulation Drawings?";
            taskDialog.ExpandedContent = "Exception Message: " + ex.Message;
            if (taskDialog.Show() != 6)
              return;
          }
          taskDialog.Show();
          continue;
        }
        ++num2;
      }
      if (source.Count > 0)
      {
        string str1 = "";
        string str2 = "";
        if (activeView.Name.StartsWith("INSULATION DETAIL - "))
        {
          string[] strArray = activeView.Name.Split('-');
          if (strArray.Length > 2)
          {
            str2 = strArray[1].Trim();
            str1 = strArray[2].Trim();
          }
          else if (strArray.Length == 2)
          {
            str2 = strArray[1].Trim();
            str1 = strArray[1].Trim();
          }
        }
        if (!string.IsNullOrWhiteSpace(str1) && !string.IsNullOrWhiteSpace(str2))
        {
          ViewSheet elem = new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_Sheets).Cast<ViewSheet>().Where<ViewSheet>((Func<ViewSheet, bool>) (x =>
          {
            if (!x.Name.StartsWith("INS - "))
              return false;
            foreach (ElementId allViewport in (IEnumerable<ElementId>) x.GetAllViewports())
            {
              if ((revitDoc.GetElement(allViewport) as Viewport).ViewId.IntegerValue == activeView.Id.IntegerValue)
                return true;
            }
            return false;
          })).FirstOrDefault<ViewSheet>();
          if (elem != null)
          {
            source.Add(str1);
            source.Add(str2);
            List<string> me1 = new List<string>();
            List<string> me2 = new List<string>();
            foreach (string s in source)
            {
              if (double.TryParse(s, out double _))
                me1.Add(s);
              else
                me2.Add(s);
            }
            List<string> list1 = me1.NaturalSort().ToList<string>();
            List<string> list2 = me2.NaturalSort().ToList<string>();
            source.Clear();
            source.AddRange((IEnumerable<string>) list1);
            source.AddRange((IEnumerable<string>) list2);
            if (source[0] != str2 || source.LastOrDefault<string>() != str1)
            {
              string str3 = source[0];
              string str4 = source.LastOrDefault<string>();
              string str5 = "INSULATION DETAIL - ";
              string str6 = !(str3 != str4) ? str5 + str3 : $"{str5}{str3}-{str4}";
              string str7 = "MASTER INSULATION SHEET - ";
              string str8 = !(str3 != str4) ? str7 + str3 : $"{str7}{str3}-{str4}";
              if (new TaskDialog("Insulation Drawing - Mark")
              {
                MainInstruction = "Adjust master drawing range for new marks?",
                MainContent = "Some or all of the drawings generated by the tool would change the mark range of a master insulation sheet. Expand for details.\n\nNote that the upper bound of this range may determine which marks get drawn when running Master Insulation Drawing",
                ExpandedContent = $"{activeView.Name} -> {str6}",
                CommonButtons = ((TaskDialogCommonButtons) 6)
              }.Show() == 6)
              {
                using (Transaction transaction = new Transaction(revitDoc, "Update Master Insualtion Legend"))
                {
                  int num3 = (int) transaction.Start();
                  string str9 = "";
                  int num4 = 1;
                  while (activeView.Name != (str6 + str9).Trim())
                  {
                    try
                    {
                      activeView.Name = (str6 + str9).Trim();
                    }
                    catch
                    {
                      str9 = $" - ({num4++.ToString()})";
                    }
                  }
                  Parameters.LookupParameter((Element) elem, "SHT_DRAWING_COVERS")?.Set(str8);
                  int num5 = (int) transaction.Commit();
                }
              }
            }
          }
        }
      }
      TaskDialog taskDialog1 = new TaskDialog("Insulation Drawing - Mark");
      if (num2 == this.details.Count)
      {
        taskDialog1.MainInstruction = "Insulation Drawings Placed";
        taskDialog1.MainContent = "Successfully placed Insulation Drawings for each insulation mark selected.";
      }
      else if (num2 != 0)
      {
        taskDialog1.MainInstruction = "Insulation Drawings Placed";
        taskDialog1.MainContent = "Successfully placed Insulation Drawings. Failed to place Insulation Drawings for one or more of the selcted marks.";
      }
      if (!string.IsNullOrEmpty(taskDialog1.MainInstruction))
        taskDialog1.Show();
      activeView.Document.Application.WriteJournalComment("Insulation Drawing - Message Shown", true);
      if (App.insulationDrawingWindow != null)
        App.insulationDrawingWindow.Close();
      activeView.Document.Application.WriteJournalComment("Insulation Drawing - Mark window closed in Event", true);
    }
  }

  public string GetName() => "Insulation Drawing - Mark Event";
}
