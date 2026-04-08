// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.CreateAssemblyViews.ViewModel
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace EDGE.AssemblyTools.CreateAssemblyViews;

internal class ViewModel
{
  public UIDocument uiDoc;

  public List<AssemblyDataModel> Assemblies { get; set; }

  public AssemblyDataModel SelectedAssemblyModel { get; set; }

  public ICollection<ElementId> SelectedIdList { get; set; }

  public ViewModel(UIDocument revitUIDoc)
  {
    this.uiDoc = revitUIDoc;
    this.Assemblies = this.GetAllAssemblyDataModels();
  }

  private List<AssemblyDataModel> GetAllAssemblyDataModels()
  {
    List<AssemblyDataModel> list = new FilteredElementCollector(this.uiDoc.Document).OfClass(typeof (AssemblyInstance)).Cast<AssemblyInstance>().Select<AssemblyInstance, AssemblyDataModel>((Func<AssemblyInstance, AssemblyDataModel>) (ass => new AssemblyDataModel(ass))).ToList<AssemblyDataModel>();
    list.Sort((Comparison<AssemblyDataModel>) ((x, y) => string.Compare(x.Name, y.Name)));
    return list;
  }
}
