// Decompiled with JetBrains decompiler
// Type: EDGE.UserSettingTools.Views.StandardRebarItem
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System.ComponentModel;

#nullable disable
namespace EDGE.UserSettingTools.Views;

public class StandardRebarItem : INotifyPropertyChanged
{
  public string rebarDiameter { set; get; }

  public string rebarShape { set; get; }

  public string rebarMark { set; get; }

  public string rebarA { set; get; }

  public string rebarB { set; get; }

  public string rebarC { set; get; }

  public string rebarD { set; get; }

  public string rebarE { set; get; }

  public string rebarF { set; get; }

  public string rebarG { set; get; }

  public string rebarH { set; get; }

  public string rebarJ { set; get; }

  public string rebarK { set; get; }

  public string rebarO { set; get; }

  public StandardRebarItem(
    string diameter,
    string shape,
    string mark,
    string a,
    string b,
    string c,
    string d,
    string e,
    string f,
    string g,
    string h,
    string j,
    string k,
    string o)
  {
    this.rebarDiameter = diameter;
    this.rebarShape = shape;
    this.rebarMark = mark;
    this.rebarA = a;
    this.rebarB = b;
    this.rebarC = c;
    this.rebarD = d;
    this.rebarE = e;
    this.rebarF = f;
    this.rebarG = g;
    this.rebarH = h;
    this.rebarJ = j;
    this.rebarK = k;
    this.rebarO = o;
  }

  public event PropertyChangedEventHandler PropertyChanged;

  private void NotifyPropertyChanged(string v)
  {
    if (this.PropertyChanged == null)
      return;
    this.PropertyChanged((object) this, new PropertyChangedEventArgs(v));
  }
}
