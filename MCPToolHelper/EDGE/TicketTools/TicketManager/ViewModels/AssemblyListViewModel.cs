// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketManager.ViewModels.AssemblyListViewModel
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Utils;
using Utils.AssemblyUtils;
using Utils.CollectionUtils;
using Utils.ElementUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.TicketTools.TicketManager.ViewModels;

public class AssemblyListViewModel : INotifyPropertyChanged
{
  private ICollection<AssemblyViewModel> _assemblyList;

  public List<AssemblyViewModel> AssemblyList
  {
    get
    {
      List<AssemblyViewModel> list = this._assemblyList.ToList<AssemblyViewModel>();
      list.Sort((Comparison<AssemblyViewModel>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p.MarkNumber, q.MarkNumber)));
      return list;
    }
    set
    {
      this._assemblyList = (ICollection<AssemblyViewModel>) value;
      this.OnPropertyChanged(nameof (AssemblyList));
    }
  }

  public ICollection<AssemblyViewModel> AllAssemblyViewModels { get; set; }

  public int TotalPieces { get; set; }

  public int TotalMarks { get; set; }

  public int PiecesReinforced { get; set; }

  public int MarksDetailed { get; set; }

  public int PiecesDetailed { get; set; }

  public int MarksReleased { get; set; }

  public int PiecesReleased { get; set; }

  public static string customColumnName1 { get; set; }

  public static string customColumnName2 { get; set; }

  public static string customColumnName3 { get; set; }

  public static string customColumnName4 { get; set; }

  public event PropertyChangedEventHandler PropertyChanged;

  protected void OnPropertyChanged(string name)
  {
    PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
    if (propertyChanged == null)
      return;
    propertyChanged((object) this, new PropertyChangedEventArgs(name));
  }

  public AssemblyListViewModel()
  {
    DateTime now = DateTime.Now;
    List<Element> list = StructuralFraming.GetFilteredElementsForTicketManager().Where<Element>((Func<Element, bool>) (elem => (elem as FamilyInstance).SuperComponent == null)).ToList<Element>();
    Dictionary<string, List<Element>> SFElementsByControlMark = new Dictionary<string, List<Element>>();
    foreach (Element elem in list)
    {
      string controlMark = elem.GetControlMark();
      if (SFElementsByControlMark.ContainsKey(controlMark))
        SFElementsByControlMark[controlMark].Add(elem);
      else
        SFElementsByControlMark.Add(controlMark, new List<Element>()
        {
          elem
        });
    }
    this.AssemblyList = AssemblyListViewModel.GetAssemblies(SFElementsByControlMark).ToList<AssemblyViewModel>();
    this.AllAssemblyViewModels = (ICollection<AssemblyViewModel>) this.AssemblyList;
    this.TotalPieces = list.Count;
    this.TotalMarks = SFElementsByControlMark.Keys.Count;
    QAUtils.LogLine("-----------Time To Run AssemblyListViewModel(): " + (DateTime.Now - now).TotalMilliseconds.ToString());
  }

  private static ICollection<AssemblyViewModel> GetAssemblies(
    Dictionary<string, List<Element>> SFElementsByControlMark)
  {
    Document document = ActiveModel.Document;
    ICollection<ViewSheet> list = (ICollection<ViewSheet>) new FilteredElementCollector(document).OfClass(typeof (ViewSheet)).Cast<ViewSheet>().ToList<ViewSheet>();
    ICollection<AssemblyViewModel> source = (ICollection<AssemblyViewModel>) new Collection<AssemblyViewModel>();
    Dictionary<string, ElementId> assembliesByControlMark = new Dictionary<string, ElementId>();
    foreach (Element elem in new FilteredElementCollector(document).OfClass(typeof (AssemblyInstance)).Where<Element>((Func<Element, bool>) (e => (e as AssemblyInstance).GetStructuralFramingElement() != null)))
    {
      string assemblyMarkNumber = elem.GetAssemblyMarkNumber();
      if (!assembliesByControlMark.ContainsKey(assemblyMarkNumber))
        assembliesByControlMark.Add(assemblyMarkNumber, elem.Id);
    }
    string str1 = App.TMCFolderPath;
    if (str1.Equals(""))
      str1 = "C:\\EDGEforRevit";
    string str2 = "";
    Parameter parameter = document.ProjectInformation.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER");
    if (parameter != null && !string.IsNullOrEmpty(parameter.AsString()))
      str2 = parameter.AsString();
    List<string> friendlyNames = new List<string>();
    List<string> paramNames = new List<string>();
    if (!str2.Equals("") || str2 != null)
    {
      if (File.Exists($"{str1}\\{str2}_TicketManagerCustomizationSettings.txt"))
      {
        foreach (string readAllLine in File.ReadAllLines($"{str1}\\{str2}_TicketManagerCustomizationSettings.txt"))
        {
          char[] chArray = new char[1]{ ':' };
          string[] strArray = readAllLine.Split(chArray)[1].Split('|');
          friendlyNames.Add(strArray[0].Trim());
          paramNames.Add(strArray[1].Trim());
        }
      }
      else if (File.Exists(str1 + "\\TicketManagerCustomizationSettings.txt"))
      {
        foreach (string readAllLine in File.ReadAllLines(str1 + "\\TicketManagerCustomizationSettings.txt"))
        {
          char[] chArray = new char[1]{ ':' };
          string[] strArray = readAllLine.Split(chArray)[1].Split('|');
          friendlyNames.Add(strArray[0].Trim());
          paramNames.Add(strArray[1].Trim());
        }
      }
    }
    else if (File.Exists(str1 + "\\TicketManagerCustomizationSettings.txt"))
    {
      foreach (string readAllLine in File.ReadAllLines(str1 + "\\TicketManagerCustomizationSettings.txt"))
      {
        char[] chArray = new char[1]{ ':' };
        string[] strArray = readAllLine.Split(chArray)[1].Split('|');
        friendlyNames.Add(strArray[0].Trim());
        paramNames.Add(strArray[1].Trim());
      }
    }
    List<AssemblyViewModel> assemblyViewModelList = new List<AssemblyViewModel>();
    foreach (string key in SFElementsByControlMark.Keys)
      source.Add(new AssemblyViewModel(key, SFElementsByControlMark[key], list, assembliesByControlMark, friendlyNames, paramNames, document));
    source.OrderBy<AssemblyViewModel, string>((Func<AssemblyViewModel, string>) (e => e.MarkNumber));
    return source;
  }

  private static int GetTotalPieces(IEnumerable<Element> structFramingElems)
  {
    return structFramingElems.Count<Element>();
  }

  private int GetMarksReinforced()
  {
    return this.AllAssemblyViewModels.Where<AssemblyViewModel>((Func<AssemblyViewModel, bool>) (model => !string.IsNullOrWhiteSpace(model.ReinforcedDate))).ToList<AssemblyViewModel>().Count;
  }

  private int GetMarksDetailed()
  {
    return this.AllAssemblyViewModels.Where<AssemblyViewModel>((Func<AssemblyViewModel, bool>) (model => !string.IsNullOrWhiteSpace(model.DetailedDate))).ToList<AssemblyViewModel>().Count;
  }

  private int GetMarksReleased()
  {
    return this.AllAssemblyViewModels.Where<AssemblyViewModel>((Func<AssemblyViewModel, bool>) (model => !string.IsNullOrWhiteSpace(model.ReleasedDate))).ToList<AssemblyViewModel>().Count;
  }

  private int GetPiecesReinforced()
  {
    return this.AllAssemblyViewModels.Where<AssemblyViewModel>((Func<AssemblyViewModel, bool>) (model => !string.IsNullOrWhiteSpace(model.ReinforcedDate))).Select<AssemblyViewModel, int>((Func<AssemblyViewModel, int>) (model => model.Quantity)).Sum();
  }

  private int GetPiecesDetailed()
  {
    return this.AllAssemblyViewModels.Where<AssemblyViewModel>((Func<AssemblyViewModel, bool>) (model => !string.IsNullOrWhiteSpace(model.DetailedDate))).Select<AssemblyViewModel, int>((Func<AssemblyViewModel, int>) (model => model.Quantity)).Sum();
  }

  private int GetPiecesReleased()
  {
    return this.AllAssemblyViewModels.Where<AssemblyViewModel>((Func<AssemblyViewModel, bool>) (model => !string.IsNullOrWhiteSpace(model.ReleasedDate))).Select<AssemblyViewModel, int>((Func<AssemblyViewModel, int>) (model => model.Quantity)).Sum();
  }
}
