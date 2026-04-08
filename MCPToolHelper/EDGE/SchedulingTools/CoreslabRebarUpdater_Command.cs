// Decompiled with JetBrains decompiler
// Type: EDGE.SchedulingTools.CoreslabRebarUpdater_Command
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CNCExport.Utils.AssemblyUtils;
using EDGE.IUpdaters.ModelLocking;
using EDGE.RebarTools;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.DocumentUtils;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.SchedulingTools;

[Transaction(TransactionMode.Manual)]
public class CoreslabRebarUpdater_Command : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    Document document = commandData.Application.ActiveUIDocument.Document;
    if (!App.RebarManager.IsAutomatedDocument(document) || !document.MfgName().Equals("CORESLAB OKC"))
    {
      TaskDialog.Show("Error", "This project is not a Coreslab OKC project, please check settings in Manage > Project Information");
      return (Result) 1;
    }
    if (!ModelLockingUtils.ShowPermissionsDialog(document, ModelLockingToolPermissions.MarkRebarByProduct))
      return (Result) 1;
    IEnumerable<AssemblyInstance> assemblies = new FilteredElementCollector(document).OfClass(typeof (AssemblyInstance)).Cast<AssemblyInstance>().Where<AssemblyInstance>((Func<AssemblyInstance, bool>) (s => s.GetStructuralFramingElement() != null && !Parameters.GetParameterAsBool((Element) s, "HARDWARE_DETAIL")));
    try
    {
      using (Transaction transaction = new Transaction(document, "Update Rebar Control Mark"))
      {
        if (transaction.Start() != TransactionStatus.Started)
        {
          message = "Failed to start transaction";
          return (Result) -1;
        }
        CoreSlabMarkingUtil.MarkAssembledRebar(document, assemblies);
        CoreSlabMarkingUtil.ProcessRebarByMarkNotAssembled(document);
        if (transaction.Commit() != TransactionStatus.Committed)
        {
          message = "Failed to commit transaction";
          return (Result) -1;
        }
      }
      return (Result) 0;
    }
    catch (Exception ex)
    {
      message = ex.Message;
      return (Result) -1;
    }
  }
}
