// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.InitialPresentation.InitialResult
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.ComponentModel;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification.InitialPresentation;

public class InitialResult : INotifyPropertyChanged
{
  private string _bucket;
  private int _count;
  private bool _passInitialPlateRotate;
  private bool _useCountMultiplier;
  private List<InitialResult.DetailResult> _detailedResults;

  public string Bucket
  {
    get => this._bucket;
    set
    {
      this._bucket = value;
      this.OnPropertyChanged(new PropertyChangedEventArgs(nameof (Bucket)));
    }
  }

  public int Count
  {
    get => this._count;
    set
    {
      this._count = value;
      this.OnPropertyChanged(new PropertyChangedEventArgs(nameof (Count)));
    }
  }

  public bool InitialPlateRotated
  {
    get => this._passInitialPlateRotate;
    set
    {
      this._passInitialPlateRotate = value;
      this.OnPropertyChanged(new PropertyChangedEventArgs(nameof (InitialPlateRotated)));
    }
  }

  public bool UseCountMultiplier
  {
    get => this._useCountMultiplier;
    set
    {
      this._useCountMultiplier = value;
      this.OnPropertyChanged(new PropertyChangedEventArgs(nameof (UseCountMultiplier)));
    }
  }

  public List<InitialResult.DetailResult> DetailedResults
  {
    get => this._detailedResults;
    set
    {
      this._detailedResults = value;
      this.OnPropertyChanged(new PropertyChangedEventArgs(nameof (DetailedResults)));
    }
  }

  public InitialResult(InitialResult result)
  {
    this.Bucket = result.Bucket;
    this.Count = result.Count;
    this.DetailedResults = result.DetailedResults;
    this.InitialPlateRotated = result.InitialPlateRotated;
    this.UseCountMultiplier = result.UseCountMultiplier;
  }

  public InitialResult()
  {
  }

  public event PropertyChangedEventHandler PropertyChanged;

  public void OnPropertyChanged(PropertyChangedEventArgs e)
  {
    if (this.PropertyChanged == null)
      return;
    this.PropertyChanged((object) this, e);
  }

  public class DetailResult
  {
    private string _controlNumber;
    private string _controlNumId;
    private ElementId _ElementId;
    private string _name;
    private string _constructionProduct;

    public DetailResult(
      ElementId id,
      string name,
      string controlNumber,
      string controlMark,
      string constructionProduct)
    {
      this.controlNumberandID = $"{controlNumber}({id.IntegerValue.ToString()})";
      this.ElementId = id;
      this.Name = name;
      this.ConstructionProduct = constructionProduct;
      this.ControlNumber = controlNumber;
    }

    public DetailResult()
    {
    }

    public string ControlNumber
    {
      get => this._controlNumber;
      set => this._controlNumber = value;
    }

    public string controlNumberandID
    {
      get => this._controlNumId;
      set => this._controlNumId = value;
    }

    public ElementId ElementId
    {
      get => this._ElementId;
      set => this._ElementId = value;
    }

    public string Name
    {
      get => this._name;
      set => this._name = value;
    }

    public string ConstructionProduct
    {
      get => this._constructionProduct;
      set => this._constructionProduct = value;
    }
  }
}
