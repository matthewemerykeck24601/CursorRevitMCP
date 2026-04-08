// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.SetControlmarkEvent
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.AssemblyTools.MarkVerification.InitialPresentation;
using System;
using System.Collections.Generic;
using System.Windows;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification;

public class SetControlmarkEvent : IExternalEventHandler
{
  public void Execute(UIApplication app)
  {
    App.DialogSwitches.SuspendModelLockingforOperation = true;
    using (Transaction transaction = new Transaction(app.ActiveUIDocument.Document, "set user defined control mark"))
    {
      int num1 = (int) transaction.Start();
      try
      {
        Dictionary<string, MarkGroupResultData> dictionary = new Dictionary<string, MarkGroupResultData>();
        foreach (KeyValuePair<string, MarkGroupResultData> keyValuePair in App.MarkVerificationInitialWindow.Dictionary)
          dictionary.Add(keyValuePair.Key, keyValuePair.Value);
        bool flag = false;
        foreach (KeyValuePair<string, MarkGroupResultData> keyValuePair in dictionary)
        {
          if (keyValuePair.Key.Equals(App.MarkVerificationInitialWindow.userDefinedControlMark))
          {
            flag = true;
            int num2 = (int) MessageBox.Show("The Control Mark you entered already Exists.", "Warning");
            break;
          }
        }
        if (flag)
          return;
        foreach (KeyValuePair<string, MarkGroupResultData> keyValuePair in dictionary)
        {
          if (keyValuePair.Key.Equals(App.MarkVerificationInitialWindow.resultToshow.Bucket))
          {
            App.MarkVerificationInitialWindow.resultToshow.Bucket = App.MarkVerificationInitialWindow.userDefinedControlMark;
            List<InitialResult.DetailResult> detailResultList = new List<InitialResult.DetailResult>();
            foreach (FamilyInstance groupMember in keyValuePair.Value.GroupMembers)
            {
              if (groupMember.SuperComponent != null)
              {
                Parameter parameter = groupMember.SuperComponent.LookupParameter("CONTROL_MARK");
                if (parameter != null && !parameter.IsReadOnly)
                  parameter.Set(App.MarkVerificationInitialWindow.userDefinedControlMark);
              }
              else
              {
                Parameter parameter = groupMember.LookupParameter("CONTROL_MARK");
                if (parameter != null && !parameter.IsReadOnly)
                  parameter.Set(App.MarkVerificationInitialWindow.userDefinedControlMark);
              }
              InitialResult.DetailResult detailResult = new InitialResult.DetailResult(groupMember.Id, groupMember.Name, Parameters.GetParameterAsString((Element) groupMember, "CONTROL_NUMBER"), App.MarkVerificationInitialWindow.userDefinedControlMark, Parameters.GetParameterAsString((Element) groupMember, "CONSTRUCTION_PRODUCT"));
              detailResultList.Add(detailResult);
            }
            App.MarkVerificationInitialWindow.Dictionary.Add(App.MarkVerificationInitialWindow.resultToshow.Bucket, keyValuePair.Value);
            App.MarkVerificationInitialWindow.Dictionary.Remove(keyValuePair.Key);
            App.MarkVerificationInitialWindow.resultToshow.DetailedResults = detailResultList;
          }
        }
        App.MarkVerificationInitialWindow.MarkNumberBox.Text = "";
      }
      catch (Exception ex)
      {
        int num3 = (int) MessageBox.Show(ex.ToString());
      }
      finally
      {
        if (transaction.Commit() != TransactionStatus.Committed)
        {
          int num4 = (int) MessageBox.Show("transaction not committed!");
        }
        App.DialogSwitches.SuspendModelLockingforOperation = false;
      }
    }
  }

  public string GetName() => "set user defined control mark event.";
}
