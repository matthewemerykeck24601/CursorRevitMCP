// Decompiled with JetBrains decompiler
// Type: EDGE.SchedulingTools.ErectionSequenceTool.ErectionSequenceZone
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System.ComponentModel;

#nullable disable
namespace EDGE.SchedulingTools.ErectionSequenceTool;

public class ErectionSequenceZone : INotifyPropertyChanged
{
  private string _originalZoneName = string.Empty;
  private int _originalZoneIndex;
  private string _zoneName = string.Empty;
  private int _zoneIndex;
  private int _zoneCount;
  private bool _isDuplicateName;

  public string OriginalZoneName => this._originalZoneName;

  public int OriginalZoneIndex => this._originalZoneIndex;

  public bool Edited
  {
    get
    {
      return (this.OriginalZoneIndex != this.ZoneIndex || this.OriginalZoneName != this.ZoneName) && !string.IsNullOrWhiteSpace(this.OriginalZoneName);
    }
  }

  public string ZoneName
  {
    get => this._zoneName;
    set
    {
      this._zoneName = value;
      this.NotifyPropertyChanged(nameof (ZoneName));
      this.NotifyPropertyChanged("IsDuplicateName");
    }
  }

  public int ZoneIndex { get; set; }

  public bool IsDuplicateName
  {
    get => string.IsNullOrWhiteSpace(this._zoneName) || this._isDuplicateName;
    set
    {
      this._isDuplicateName = value;
      this.NotifyPropertyChanged(nameof (IsDuplicateName));
    }
  }

  public int ZoneCount
  {
    get => this._zoneCount;
    set
    {
      this._zoneCount = value;
      this.NotifyPropertyChanged(nameof (ZoneCount));
    }
  }

  public ErectionSequenceZone()
  {
    this.ZoneName = string.Empty;
    this.ZoneIndex = 0;
    this.IsDuplicateName = false;
    this.ZoneCount = 0;
  }

  public ErectionSequenceZone(string zoneName, int zoneIndex, int zoneCount = 0, bool isDuplicateName = false)
  {
    this.ZoneName = zoneName.Trim();
    this.ZoneIndex = zoneIndex;
    this.ZoneCount = zoneCount;
    this._originalZoneName = zoneName.Trim();
    this._originalZoneIndex = zoneIndex;
  }

  public event PropertyChangedEventHandler PropertyChanged;

  private void NotifyPropertyChanged(string propertyName)
  {
    if (this.PropertyChanged == null)
      return;
    this.PropertyChanged((object) this, new PropertyChangedEventArgs(propertyName));
  }
}
