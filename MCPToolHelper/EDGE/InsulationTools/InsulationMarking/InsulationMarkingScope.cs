// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.InsulationMarking.InsulationMarkingScope
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

#nullable disable
namespace EDGE.InsulationTools.InsulationMarking;

public class InsulationMarkingScope
{
  public static List<AssemblyInstance> WholeModel(Document revitDoc, string toolName)
  {
    using (Transaction transaction = new Transaction(revitDoc, "Whole Model - " + toolName))
    {
      int num1 = (int) transaction.Start();
      List<AssemblyInstance> list = new FilteredElementCollector(revitDoc).OfClass(typeof (AssemblyInstance)).ToElements().Cast<AssemblyInstance>().ToList<AssemblyInstance>();
      int num2 = (int) transaction.Commit();
      return list;
    }
  }

  public static List<AssemblyInstance> ActiveModel(Document revitDoc, UIDocument uiDoc)
  {
    using (Transaction transaction = new Transaction(revitDoc, "Active Model - Insulation Marking"))
    {
      int num1 = (int) transaction.Start();
      List<AssemblyInstance> list = new FilteredElementCollector(revitDoc, uiDoc.ActiveView.Id).OfClass(typeof (AssemblyInstance)).ToElements().Cast<AssemblyInstance>().ToList<AssemblyInstance>();
      int num2 = (int) transaction.Commit();
      return list;
    }
  }

  public static List<AssemblyInstance> SelectionList(
    Document revitDoc,
    UIDocument uiDoc,
    string toolName)
  {
    using (Transaction transaction = new Transaction(revitDoc, "Selection List - " + toolName))
    {
      int num1 = (int) transaction.Start();
      List<ElementId> list1 = uiDoc.Selection.GetElementIds().ToList<ElementId>();
      if (list1.Count == 0)
      {
        try
        {
          List<Reference> list2 = uiDoc.Selection.PickObjects((ObjectType) 1, (ISelectionFilter) new AssemblyInstancesAndStructuralFraming(), "Pick Assemblies to export.").ToList<Reference>();
          if (list2.Count > 0)
          {
            foreach (Reference reference in list2)
            {
              if (revitDoc.GetElement(reference) != null)
                list1.Add(revitDoc.GetElement(reference).Id);
            }
          }
          else
          {
            int num2 = (int) MessageBox.Show("There were no Assemblies selected to be processed.");
            return (List<AssemblyInstance>) null;
          }
        }
        catch (Exception ex)
        {
          return (List<AssemblyInstance>) null;
        }
      }
      List<AssemblyInstance> list3 = new FilteredElementCollector(revitDoc, (ICollection<ElementId>) list1).OfClass(typeof (AssemblyInstance)).ToElements().Cast<AssemblyInstance>().ToList<AssemblyInstance>();
      int num3 = (int) transaction.Commit();
      return list3;
    }
  }
}
