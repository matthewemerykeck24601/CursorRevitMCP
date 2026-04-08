// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.InsulationDrawing.InsulationDrawingPerPieceObject
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

#nullable disable
namespace EDGE.InsulationTools.InsulationDrawing;

public class InsulationDrawingPerPieceObject : INotifyPropertyChanged
{
  private string sheetName;

  public event PropertyChangedEventHandler PropertyChanged;

  public string AssemblyName
  {
    get
    {
      return this.assemblyInstance != null && this.assemblyInstance.Name != null ? this.assemblyInstance.Name : "";
    }
  }

  public AssemblyInstance assemblyInstance { get; set; }

  public string SheetName
  {
    get => this.sheetName;
    set
    {
      this.sheetName = value;
      this.OnPropertyChanged(nameof (SheetName));
    }
  }

  public List<string> SheetList { get; set; }

  public Dictionary<string, ViewSheet> SheetsDictionary { get; set; }

  public Dictionary<string, FamilyInstance> insulationMarkToElementDictionary { get; set; }

  public bool ContainsUnmarked { get; set; }

  public InsulationDrawingPerPieceObject()
  {
    this.assemblyInstance = (AssemblyInstance) null;
    this.SheetName = string.Empty;
    this.SheetList = new List<string>();
    this.ContainsUnmarked = false;
    this.insulationMarkToElementDictionary = new Dictionary<string, FamilyInstance>();
  }

  public InsulationDrawingPerPieceObject(
    AssemblyInstance assembly,
    List<ViewSheet> sheets,
    Dictionary<string, FamilyInstance> insulationDict,
    bool containsUnmarked = false)
  {
    this.assemblyInstance = assembly;
    if (this.SheetsDictionary == null)
      this.SheetsDictionary = new Dictionary<string, ViewSheet>();
    else if (this.SheetsDictionary.Count == 0)
      this.SheetsDictionary = new Dictionary<string, ViewSheet>();
    if (this.SheetList == null)
      this.SheetList = new List<string>();
    else if (this.SheetList.Count == 0)
      this.SheetList = new List<string>();
    foreach (ViewSheet sheet in sheets)
    {
      string key = $"{sheet.SheetNumber}-{sheet.Name}";
      if (!this.SheetsDictionary.ContainsKey(key))
        this.SheetsDictionary.Add(key, sheet);
    }
    List<string> list = this.SheetsDictionary.Keys.ToList<string>();
    list.Sort();
    this.SheetList.AddRange((IEnumerable<string>) list);
    this.SheetName = this.SheetList.Count <= 0 ? string.Empty : this.SheetList[0];
    this.insulationMarkToElementDictionary = insulationDict;
    this.ContainsUnmarked = containsUnmarked;
  }

  protected void OnPropertyChanged([CallerMemberName] string name = null)
  {
    PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
    if (propertyChanged == null)
      return;
    propertyChanged((object) this, new PropertyChangedEventArgs(name));
  }
}
