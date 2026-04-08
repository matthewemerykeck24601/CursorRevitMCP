// Decompiled with JetBrains decompiler
// Type: EDGE.UserSettingTools.CAD_Export_Settings.CADExportSettingsDataViewModel
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.RebarTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

#nullable disable
namespace EDGE.UserSettingTools.CAD_Export_Settings;

public class CADExportSettingsDataViewModel : INotifyPropertyChanged
{
  private string _manufacturer = "TestManufacturer";
  private string _cadExportFolderPathString;
  private string _cadExportFileNameSuffixString;
  private string _insulationExportFolderPathString;
  private string _insulationExportFileNameSuffixString;
  private ObservableCollection<ComboBoxExportColorItem> _colorList = new ObservableCollection<ComboBoxExportColorItem>((IEnumerable<ComboBoxExportColorItem>) DefaultCADExportSettings.ColorDictionary.Values);
  private ObservableCollection<CADExportLayer> _cadExportLayersDataGridList = new ObservableCollection<CADExportLayer>();
  private ObservableCollection<CADExportLayer> _insulationExportLayersDataGridList = new ObservableCollection<CADExportLayer>();
  private ObservableCollection<CADExportLayer> _cadExportLayerComboBoxeslist;
  private ObservableCollection<CADExportLayer> _insulationExportLayerComboBoxesList;
  private CADExportLayer _tifLayer;
  private CADExportLayer _bifLayer;
  private CADExportLayer _sifLayer;
  private CADExportLayer _formLayer;
  private CADExportLayer _corbelLayer;
  private CADExportLayer _archLayer;
  private CADExportLayer _strandLayer;
  private CADExportLayer _insulationLayer;
  private CADExportLayer _cutoutsLayer;
  private CADExportLayer _pinsLayer;
  private CADExportLayer _slotLayer;
  private CADExportLayer _extraTiesLayer;
  private CADExportLayer _markSymbolLayer;
  private CADExportLayer _insulationMarkLayer;
  private bool _tifExportBoundingBoxBool = true;
  private bool _tifExportFixedCircleBool;
  private bool _bifExportBoundingBoxBool = true;
  private bool _bifExportFixedCircleBool;
  private bool _cadExportDWGFileExport = true;
  private bool _cadExportDXFFileExport;
  private bool _insulationExportDWGFileExport = true;
  private bool _insulationExportDXFFileExport;
  private bool _useEdgeCADLines;
  private string _fixedCircleRadius;
  private string _units = "in.";
  private string _tickMarkLength;

  public string Manufacturer
  {
    get => this._manufacturer;
    set
    {
      if (!(this._manufacturer != value))
        return;
      this._manufacturer = value;
      this.NotifyPropertyChanged(nameof (Manufacturer));
      this.NotifyPropertyChanged("ManufacturerDisplay");
    }
  }

  public string ManufacturerDisplay
  {
    get => "Manufacturer: " + this._manufacturer;
    set
    {
      if (!(this._manufacturer != value))
        return;
      this._manufacturer = value;
      this.NotifyPropertyChanged("Manufacturer");
      this.NotifyPropertyChanged(nameof (ManufacturerDisplay));
    }
  }

  public string CADExportFolderPathString
  {
    get => this._cadExportFolderPathString;
    set
    {
      if (!(this._cadExportFolderPathString != value))
        return;
      this._cadExportFolderPathString = !string.IsNullOrWhiteSpace(value) ? value.Trim() : DefaultCADExportSettings.CADExportFolderPath;
      this.NotifyPropertyChanged(nameof (CADExportFolderPathString));
      this.NotifyPropertyChanged("CADExportExampleFilePath");
    }
  }

  public string CADExportFileNameSuffixString
  {
    get => this._cadExportFileNameSuffixString;
    set
    {
      if (!(this._cadExportFileNameSuffixString != value))
        return;
      this._cadExportFileNameSuffixString = !string.IsNullOrWhiteSpace(value) ? value : DefaultCADExportSettings.CADExportFileNameSuffix;
      this.NotifyPropertyChanged(nameof (CADExportFileNameSuffixString));
      this.NotifyPropertyChanged("CADExportExampleFilePath");
    }
  }

  public string InsulationExportFolderPathString
  {
    get => this._insulationExportFolderPathString;
    set
    {
      if (!(this._insulationExportFolderPathString != value))
        return;
      this._insulationExportFolderPathString = !string.IsNullOrEmpty(value) ? value.Trim() : DefaultCADExportSettings.InsulationExportFolderPath;
      this.NotifyPropertyChanged(nameof (InsulationExportFolderPathString));
      this.NotifyPropertyChanged("InsulationExportExampleFilePath");
    }
  }

  public string InsulationExportFileNameSuffixString
  {
    get => this._insulationExportFileNameSuffixString;
    set
    {
      if (!(this._insulationExportFileNameSuffixString != value))
        return;
      this._insulationExportFileNameSuffixString = !string.IsNullOrWhiteSpace(value) ? value : DefaultCADExportSettings.InsulationExportFileNamesSuffix;
      this.NotifyPropertyChanged(nameof (InsulationExportFileNameSuffixString));
      this.NotifyPropertyChanged("InsulationExportExampleFilePath");
    }
  }

  public string CADExportExampleFilePath
  {
    get
    {
      string str = ".dwg";
      if (this.CADExportDWGFileExport)
        str = ".dwg";
      else if (this.CADExportDXFFileExport)
        str = ".dxf";
      return $"{this._cadExportFolderPathString}\\#### - Project\\####-MKXX{this._cadExportFileNameSuffixString}{str}";
    }
  }

  public string InsulationExportExampleFilePath
  {
    get
    {
      string str = ".dwg";
      if (this.InsulationExportDWGFileExport)
        str = ".dwg";
      else if (this.InsulationExportDXFFileExport)
        str = ".dxf";
      return $"{this._insulationExportFolderPathString}\\#### - Project\\####-MKXX{this._insulationExportFileNameSuffixString}{str}";
    }
  }

  public ObservableCollection<ComboBoxExportColorItem> ColorList => this._colorList;

  public ObservableCollection<CADExportLayer> CADExportLayersDataGridList
  {
    get => this._cadExportLayersDataGridList;
    set
    {
      if (value == null || this.SameContents(this._cadExportLayersDataGridList, value))
        return;
      this._cadExportLayersDataGridList = value;
      this.CADExportLayerComboBoxesList = this.getComboBoxList(value);
      this.NotifyPropertyChanged(nameof (CADExportLayersDataGridList));
    }
  }

  public ObservableCollection<CADExportLayer> InsulationExportLayersDataGridList
  {
    get => this._insulationExportLayersDataGridList;
    set
    {
      if (value == null || this.SameContents(this._insulationExportLayersDataGridList, value))
        return;
      this._insulationExportLayersDataGridList = value;
      this.InsulationExportLayerComboBoxesList = this.getComboBoxList(value);
      this.NotifyPropertyChanged(nameof (InsulationExportLayersDataGridList));
    }
  }

  public ObservableCollection<CADExportLayer> CADExportLayerComboBoxesList
  {
    get => this._cadExportLayerComboBoxeslist;
    set
    {
      if (value == null || this.SameContents(this._cadExportLayerComboBoxeslist, value))
        return;
      this._cadExportLayerComboBoxeslist = value;
      this.NotifyPropertyChanged(nameof (CADExportLayerComboBoxesList));
      this.NotifyPropertyChanged("TIFLayer");
      this.NotifyPropertyChanged("BIFLayer");
      this.NotifyPropertyChanged("SIFLayer");
      this.NotifyPropertyChanged("FormLayer");
      this.NotifyPropertyChanged("CorbelLayer");
      this.NotifyPropertyChanged("ArchLayer");
      this.NotifyPropertyChanged("StrandLayer");
    }
  }

  public ObservableCollection<CADExportLayer> InsulationExportLayerComboBoxesList
  {
    get => this._insulationExportLayerComboBoxesList;
    set
    {
      if (value == null || this.SameContents(this._insulationExportLayerComboBoxesList, value))
        return;
      this._insulationExportLayerComboBoxesList = value;
      this.NotifyPropertyChanged(nameof (InsulationExportLayerComboBoxesList));
      this.NotifyPropertyChanged("InsulationLayer");
      this.NotifyPropertyChanged("CutoutsLayer");
      this.NotifyPropertyChanged("PinsLayer");
      this.NotifyPropertyChanged("SlotLayer");
      this.NotifyPropertyChanged("ExtraTiesLayer");
      this.NotifyPropertyChanged("MarkSymbolLayer");
      this.NotifyPropertyChanged("InsulationMarkLayer");
    }
  }

  public CADExportLayer TIFLayer
  {
    get => this._tifLayer;
    set
    {
      if (value == null)
      {
        this._tifLayer = DefaultCADExportSettings.disabledLayer;
        this.NotifyPropertyChanged(nameof (TIFLayer));
      }
      else if (this._tifLayer == null)
      {
        this._tifLayer = value;
        this.NotifyPropertyChanged(nameof (TIFLayer));
      }
      else
      {
        if (value == null || this._tifLayer.Equals(value))
          return;
        this._tifLayer = value;
        this.NotifyPropertyChanged(nameof (TIFLayer));
      }
    }
  }

  public CADExportLayer BIFLayer
  {
    get => this._bifLayer;
    set
    {
      if (value == null)
      {
        this._bifLayer = DefaultCADExportSettings.disabledLayer;
        this.NotifyPropertyChanged(nameof (BIFLayer));
      }
      else if (this._bifLayer == null)
      {
        this._bifLayer = value;
        this.NotifyPropertyChanged(nameof (BIFLayer));
      }
      else
      {
        if (value == null || this._bifLayer.Equals(value))
          return;
        this._bifLayer = value;
        this.NotifyPropertyChanged(nameof (BIFLayer));
      }
    }
  }

  public CADExportLayer SIFLayer
  {
    get => this._sifLayer;
    set
    {
      if (value == null)
      {
        this._sifLayer = DefaultCADExportSettings.disabledLayer;
        this.NotifyPropertyChanged(nameof (SIFLayer));
      }
      else if (this._sifLayer == null)
      {
        this._sifLayer = value;
        this.NotifyPropertyChanged(nameof (SIFLayer));
      }
      else
      {
        if (value == null || this._sifLayer.Equals(value))
          return;
        this._sifLayer = value;
        this.NotifyPropertyChanged(nameof (SIFLayer));
      }
    }
  }

  public CADExportLayer FormLayer
  {
    get => this._formLayer;
    set
    {
      if (value == null)
      {
        this._formLayer = DefaultCADExportSettings.disabledLayer;
        this.NotifyPropertyChanged(nameof (FormLayer));
      }
      else if (this._formLayer == null)
      {
        this._formLayer = value;
        this.NotifyPropertyChanged(nameof (FormLayer));
      }
      else
      {
        if (value == null || this._formLayer.Equals(value))
          return;
        this._formLayer = value;
        this.NotifyPropertyChanged(nameof (FormLayer));
      }
    }
  }

  public CADExportLayer CorbelLayer
  {
    get => this._corbelLayer;
    set
    {
      if (value == null)
      {
        this._corbelLayer = DefaultCADExportSettings.disabledLayer;
        this.NotifyPropertyChanged(nameof (CorbelLayer));
      }
      else if (this._corbelLayer == null)
      {
        this._corbelLayer = value;
        this.NotifyPropertyChanged(nameof (CorbelLayer));
      }
      else
      {
        if (value == null || this._corbelLayer.Equals(value))
          return;
        this._corbelLayer = value;
        this.NotifyPropertyChanged(nameof (CorbelLayer));
      }
    }
  }

  public CADExportLayer ArchLayer
  {
    get => this._archLayer;
    set
    {
      if (value == null)
      {
        this._archLayer = DefaultCADExportSettings.disabledLayer;
        this.NotifyPropertyChanged(nameof (ArchLayer));
      }
      else if (this._archLayer == null)
      {
        this._archLayer = value;
        this.NotifyPropertyChanged(nameof (ArchLayer));
      }
      else
      {
        if (value == null || this._archLayer.Equals(value))
          return;
        this._archLayer = value;
        this.NotifyPropertyChanged(nameof (ArchLayer));
      }
    }
  }

  public CADExportLayer StrandLayer
  {
    get => this._strandLayer;
    set
    {
      if (value == null)
      {
        this._strandLayer = DefaultCADExportSettings.disabledLayer;
        this.NotifyPropertyChanged(nameof (StrandLayer));
      }
      else if (this._strandLayer == null)
      {
        this._strandLayer = value;
        this.NotifyPropertyChanged(nameof (StrandLayer));
      }
      else
      {
        if (value == null || this.StrandLayer.Equals(value))
          return;
        this._strandLayer = value;
        this.NotifyPropertyChanged(nameof (StrandLayer));
      }
    }
  }

  public CADExportLayer InsulationLayer
  {
    get => this._insulationLayer;
    set
    {
      if (value == null)
      {
        this._insulationLayer = DefaultCADExportSettings.disabledLayer;
        this.NotifyPropertyChanged(nameof (InsulationLayer));
      }
      else if (this._insulationLayer == null)
      {
        this._insulationLayer = value;
        this.NotifyPropertyChanged(nameof (InsulationLayer));
      }
      else
      {
        if (value == null || this.InsulationLayer.Equals(value))
          return;
        this._insulationLayer = value;
        this.NotifyPropertyChanged(nameof (InsulationLayer));
      }
    }
  }

  public CADExportLayer CutoutsLayer
  {
    get => this._cutoutsLayer;
    set
    {
      if (value == null)
      {
        this._cutoutsLayer = DefaultCADExportSettings.disabledLayer;
        this.NotifyPropertyChanged(nameof (CutoutsLayer));
      }
      else if (this._cutoutsLayer == null)
      {
        this._cutoutsLayer = value;
        this.NotifyPropertyChanged(nameof (CutoutsLayer));
      }
      else
      {
        if (value == null || this.CutoutsLayer.Equals(value))
          return;
        this._cutoutsLayer = value;
        this.NotifyPropertyChanged(nameof (CutoutsLayer));
      }
    }
  }

  public CADExportLayer PinsLayer
  {
    get => this._pinsLayer;
    set
    {
      if (value == null)
      {
        this._pinsLayer = DefaultCADExportSettings.disabledLayer;
        this.NotifyPropertyChanged(nameof (PinsLayer));
      }
      else if (this._pinsLayer == null)
      {
        this._pinsLayer = value;
        this.NotifyPropertyChanged(nameof (PinsLayer));
      }
      else
      {
        if (value == null || this.PinsLayer.Equals(value))
          return;
        this._pinsLayer = value;
        this.NotifyPropertyChanged(nameof (PinsLayer));
      }
    }
  }

  public CADExportLayer SlotLayer
  {
    get => this._slotLayer;
    set
    {
      if (value == null)
      {
        this._slotLayer = DefaultCADExportSettings.disabledLayer;
        this.NotifyPropertyChanged(nameof (SlotLayer));
      }
      else if (this._slotLayer == null)
      {
        this._slotLayer = value;
        this.NotifyPropertyChanged(nameof (SlotLayer));
      }
      else
      {
        if (value == null || this.SlotLayer.Equals(value))
          return;
        this._slotLayer = value;
        this.NotifyPropertyChanged(nameof (SlotLayer));
      }
    }
  }

  public CADExportLayer ExtraTiesLayer
  {
    get => this._extraTiesLayer;
    set
    {
      if (value == null)
      {
        this._extraTiesLayer = DefaultCADExportSettings.disabledLayer;
        this.NotifyPropertyChanged(nameof (ExtraTiesLayer));
      }
      else if (this._extraTiesLayer == null)
      {
        this._extraTiesLayer = value;
        this.NotifyPropertyChanged(nameof (ExtraTiesLayer));
      }
      else
      {
        if (value == null || this.ExtraTiesLayer.Equals(value))
          return;
        this._extraTiesLayer = value;
        this.NotifyPropertyChanged(nameof (ExtraTiesLayer));
      }
    }
  }

  public CADExportLayer MarkSymbolLayer
  {
    get => this._markSymbolLayer;
    set
    {
      if (value == null)
      {
        this._markSymbolLayer = DefaultCADExportSettings.disabledLayer;
        this.NotifyPropertyChanged(nameof (MarkSymbolLayer));
      }
      else if (this._markSymbolLayer == null)
      {
        this._markSymbolLayer = value;
        this.NotifyPropertyChanged(nameof (MarkSymbolLayer));
      }
      else
      {
        if (value == null || this.MarkSymbolLayer.Equals(value))
          return;
        this._markSymbolLayer = value;
        this.NotifyPropertyChanged(nameof (MarkSymbolLayer));
      }
    }
  }

  public CADExportLayer InsulationMarkLayer
  {
    get => this._insulationMarkLayer;
    set
    {
      if (value == null)
      {
        this._insulationMarkLayer = DefaultCADExportSettings.disabledLayer;
        this.NotifyPropertyChanged(nameof (InsulationMarkLayer));
      }
      else if (this._insulationMarkLayer == null)
      {
        this._insulationMarkLayer = value;
        this.NotifyPropertyChanged(nameof (InsulationMarkLayer));
      }
      else
      {
        if (value == null || this.InsulationMarkLayer.Equals(value))
          return;
        this._insulationMarkLayer = value;
        this.NotifyPropertyChanged(nameof (InsulationMarkLayer));
      }
    }
  }

  public bool TIFExportBoundingBoxBool
  {
    get => this._tifExportBoundingBoxBool;
    set
    {
      if (this._tifExportBoundingBoxBool == value)
        return;
      this._tifExportBoundingBoxBool = value;
      this.NotifyPropertyChanged(nameof (TIFExportBoundingBoxBool));
    }
  }

  public bool TIFExportFixedCircleBool
  {
    get => this._tifExportFixedCircleBool;
    set
    {
      if (this._tifExportFixedCircleBool == value)
        return;
      this._tifExportFixedCircleBool = value;
      this.NotifyPropertyChanged(nameof (TIFExportFixedCircleBool));
    }
  }

  public bool BIFExportBoundingBoxBool
  {
    get => this._bifExportBoundingBoxBool;
    set
    {
      if (this._bifExportBoundingBoxBool == value)
        return;
      this._bifExportBoundingBoxBool = value;
      this.NotifyPropertyChanged(nameof (BIFExportBoundingBoxBool));
    }
  }

  public bool BIFExportFixedCircleBool
  {
    get => this._bifExportFixedCircleBool;
    set
    {
      if (this._bifExportFixedCircleBool == value)
        return;
      this._bifExportFixedCircleBool = value;
      this.NotifyPropertyChanged(nameof (BIFExportFixedCircleBool));
    }
  }

  public bool CADExportDWGFileExport
  {
    get => this._cadExportDWGFileExport;
    set
    {
      if (this._cadExportDWGFileExport == value)
        return;
      this._cadExportDWGFileExport = value;
      this.NotifyPropertyChanged(nameof (CADExportDWGFileExport));
      this.NotifyPropertyChanged("CADExportExampleFilePath");
      this.NotifyPropertyChanged("CADFileExportTypeError");
    }
  }

  public bool CADExportDXFFileExport
  {
    get => this._cadExportDXFFileExport;
    set
    {
      if (this._cadExportDXFFileExport == value)
        return;
      this._cadExportDXFFileExport = value;
      this.NotifyPropertyChanged(nameof (CADExportDXFFileExport));
      this.NotifyPropertyChanged("CADExportExampleFilePath");
      this.NotifyPropertyChanged("CADFileExportTypeError");
    }
  }

  public bool CADFileExportTypeError
  {
    get => !this.CADExportDWGFileExport && !this.CADExportDXFFileExport;
  }

  public bool InsulationExportDWGFileExport
  {
    get => this._insulationExportDWGFileExport;
    set
    {
      if (this._insulationExportDWGFileExport == value)
        return;
      this._insulationExportDWGFileExport = value;
      this.NotifyPropertyChanged(nameof (InsulationExportDWGFileExport));
      this.NotifyPropertyChanged("InsulationExportExampleFilePath");
      this.NotifyPropertyChanged("InsulationFileExportTypeLayer");
    }
  }

  public bool InsulationExportDXFFileExport
  {
    get => this._insulationExportDXFFileExport;
    set
    {
      if (this._insulationExportDXFFileExport == value)
        return;
      this._insulationExportDXFFileExport = value;
      this.NotifyPropertyChanged(nameof (InsulationExportDXFFileExport));
      this.NotifyPropertyChanged("InsulationExportExampleFilePath");
      this.NotifyPropertyChanged("InsulationFileExportTypeLayer");
    }
  }

  public bool InsulationFileExportTypeLayer
  {
    get => !this.InsulationExportDWGFileExport && !this.InsulationExportDXFFileExport;
  }

  public bool UseEdgeCADLines
  {
    get => this._useEdgeCADLines;
    set
    {
      if (this._useEdgeCADLines == value)
        return;
      this._useEdgeCADLines = value;
      this.NotifyPropertyChanged(nameof (UseEdgeCADLines));
    }
  }

  public string FixedCircleRadius
  {
    get => this._fixedCircleRadius;
    set
    {
      if (!(this._fixedCircleRadius != value))
        return;
      if (string.IsNullOrWhiteSpace(value))
      {
        if (this.Units == "in.")
          this._fixedCircleRadius = UnitUtils.ConvertFromInternalUnits(DefaultCADExportSettings.FixedCircleRadius, UnitTypeId.Inches).ToString();
        else if (this.Units == "mm")
          this._fixedCircleRadius = UnitUtils.ConvertFromInternalUnits(DefaultCADExportSettings.FixedCircleRadius, UnitTypeId.Millimeters).ToString();
      }
      else
      {
        string stringToParse = value.Trim();
        if (UnitValidationParameters.imperialUnits)
        {
          ValueParsingOptions valueParsingOptions1 = new ValueParsingOptions();
          FormatOptions formatOptions1 = new FormatOptions(UnitTypeId.Inches, SymbolTypeId.InchDoubleQuote);
          valueParsingOptions1.SetFormatOptions(formatOptions1);
          ValueParsingOptions valueParsingOptions2 = new ValueParsingOptions();
          FormatOptions formatOptions2 = new FormatOptions(UnitTypeId.Inches, SymbolTypeId.In);
          valueParsingOptions2.SetFormatOptions(formatOptions2);
          double num;
          if (UnitFormatUtils.TryParse(UnitValidationParameters.units, SpecTypeId.Length, stringToParse, valueParsingOptions1, out num))
            stringToParse = Math.Round(UnitUtils.ConvertFromInternalUnits(num, UnitTypeId.Inches), 10).ToString();
          else if (UnitFormatUtils.TryParse(UnitValidationParameters.units, SpecTypeId.Length, stringToParse, valueParsingOptions2, out num))
            stringToParse = Math.Round(UnitUtils.ConvertFromInternalUnits(num, UnitTypeId.Inches), 10).ToString();
        }
        else
        {
          ValueParsingOptions valueParsingOptions = new ValueParsingOptions();
          FormatOptions formatOptions = new FormatOptions(UnitTypeId.Millimeters, SymbolTypeId.Mm);
          valueParsingOptions.SetFormatOptions(formatOptions);
          double num;
          if (UnitFormatUtils.TryParse(UnitValidationParameters.units, SpecTypeId.Length, stringToParse, valueParsingOptions, out num))
            stringToParse = Math.Round(UnitUtils.ConvertFromInternalUnits(num, UnitTypeId.Millimeters), 10).ToString();
        }
        this._fixedCircleRadius = stringToParse;
      }
      this.NotifyPropertyChanged(nameof (FixedCircleRadius));
    }
  }

  public string Units
  {
    get => this._units;
    set
    {
      if (!(this._units != value))
        return;
      this._units = value;
      this.NotifyPropertyChanged(nameof (Units));
    }
  }

  public string TickMarkLength
  {
    get => this._tickMarkLength;
    set
    {
      if (!(this._tickMarkLength != value))
        return;
      if (string.IsNullOrWhiteSpace(value))
      {
        if (this.Units == "in.")
          this._tickMarkLength = UnitUtils.ConvertFromInternalUnits(DefaultCADExportSettings.TickMarkLength, UnitTypeId.Inches).ToString();
        else if (this.Units == "mm")
          this._tickMarkLength = UnitUtils.ConvertFromInternalUnits(DefaultCADExportSettings.TickMarkLength, UnitTypeId.Millimeters).ToString();
      }
      else
      {
        string stringToParse = value.Trim();
        if (UnitValidationParameters.imperialUnits)
        {
          ValueParsingOptions valueParsingOptions1 = new ValueParsingOptions();
          FormatOptions formatOptions1 = new FormatOptions(UnitTypeId.Inches, SymbolTypeId.InchDoubleQuote);
          valueParsingOptions1.SetFormatOptions(formatOptions1);
          ValueParsingOptions valueParsingOptions2 = new ValueParsingOptions();
          FormatOptions formatOptions2 = new FormatOptions(UnitTypeId.Inches, SymbolTypeId.In);
          valueParsingOptions2.SetFormatOptions(formatOptions2);
          double num;
          if (UnitFormatUtils.TryParse(UnitValidationParameters.units, SpecTypeId.Length, stringToParse, valueParsingOptions1, out num))
            stringToParse = Math.Round(UnitUtils.ConvertFromInternalUnits(num, UnitTypeId.Inches), 10).ToString();
          else if (UnitFormatUtils.TryParse(UnitValidationParameters.units, SpecTypeId.Length, stringToParse, valueParsingOptions2, out num))
            stringToParse = Math.Round(UnitUtils.ConvertFromInternalUnits(num, UnitTypeId.Inches), 10).ToString();
        }
        else
        {
          ValueParsingOptions valueParsingOptions = new ValueParsingOptions();
          FormatOptions formatOptions = new FormatOptions(UnitTypeId.Millimeters, SymbolTypeId.Mm);
          valueParsingOptions.SetFormatOptions(formatOptions);
          double num;
          if (UnitFormatUtils.TryParse(UnitValidationParameters.units, SpecTypeId.Length, stringToParse, valueParsingOptions, out num))
            stringToParse = Math.Round(UnitUtils.ConvertFromInternalUnits(num, UnitTypeId.Millimeters), 10).ToString();
        }
        this._tickMarkLength = stringToParse;
      }
      this.NotifyPropertyChanged(nameof (TickMarkLength));
    }
  }

  public CADExportSettingsDataViewModel(CADExportSettings settings, bool imperialUnits)
  {
    this.UpdateSettings(settings, imperialUnits);
  }

  public void UpdateSettings(CADExportSettings settings, bool imperialUnits)
  {
    this.CADExportFolderPathString = settings.CADExportFolderPath;
    this.CADExportFileNameSuffixString = settings.CADFileNameSuffix;
    this.CADExportDWGFileExport = settings.CADExportDWGFiles;
    this.CADExportDXFFileExport = settings.CADExportDXFFiles;
    this.InsulationExportFolderPathString = settings.InsulationExportFolderPath;
    this.InsulationExportFileNameSuffixString = settings.InsulationFileNameSuffix;
    this.InsulationExportDWGFileExport = settings.InsulationExportDWGFiles;
    this.InsulationExportDXFFileExport = settings.InsulationExportDXFFiles;
    this.TIFExportBoundingBoxBool = settings.TIFExportBoundingBox;
    this.TIFExportFixedCircleBool = settings.TIFExportFixedCircle;
    this.BIFExportBoundingBoxBool = settings.BIFExportBoundingBox;
    this.BIFExportFixedCircleBool = settings.BIFExportFixedCircle;
    if (imperialUnits)
    {
      this.FixedCircleRadius = settings.fixedCircleRadiusInches.ToString();
      this.TickMarkLength = settings.tickMarkLengthInches.ToString();
      this.Units = "in.";
    }
    else
    {
      this.FixedCircleRadius = settings.fixedCircleRadiusMillimeter.ToString();
      this.TickMarkLength = settings.tickMarkLengthMillimeter.ToString();
      this.Units = "mm";
    }
    this.UseEdgeCADLines = settings.UseEdgeCadLines;
    this.CADExportLayersDataGridList = new ObservableCollection<CADExportLayer>(settings.GetCADExportLayers().OrderBy<CADExportLayer, string>((Func<CADExportLayer, string>) (x => x.LayerName)).ToList<CADExportLayer>());
    this.InsulationExportLayersDataGridList = new ObservableCollection<CADExportLayer>(settings.GetInsulationExportLayers().OrderBy<CADExportLayer, string>((Func<CADExportLayer, string>) (x => x.LayerName)).ToList<CADExportLayer>());
    this.TIFLayer = settings.GetSelectedLayer(CADExportSettings.ExportGroup.TIF);
    this.BIFLayer = settings.GetSelectedLayer(CADExportSettings.ExportGroup.BIF);
    this.SIFLayer = settings.GetSelectedLayer(CADExportSettings.ExportGroup.SIF);
    this.FormLayer = settings.GetSelectedLayer(CADExportSettings.ExportGroup.Form);
    this.CorbelLayer = settings.GetSelectedLayer(CADExportSettings.ExportGroup.Corbel);
    this.ArchLayer = settings.GetSelectedLayer(CADExportSettings.ExportGroup.Arch);
    this.StrandLayer = settings.GetSelectedLayer(CADExportSettings.ExportGroup.Strand);
    this.InsulationLayer = settings.GetSelectedLayer(CADExportSettings.ExportGroup.Insulation);
    this.CutoutsLayer = settings.GetSelectedLayer(CADExportSettings.ExportGroup.Cutout);
    this.PinsLayer = settings.GetSelectedLayer(CADExportSettings.ExportGroup.Pins);
    this.SlotLayer = settings.GetSelectedLayer(CADExportSettings.ExportGroup.Slots);
    this.ExtraTiesLayer = settings.GetSelectedLayer(CADExportSettings.ExportGroup.ExtraTies);
    this.MarkSymbolLayer = settings.GetSelectedLayer(CADExportSettings.ExportGroup.MarkSymbol);
    this.InsulationMarkLayer = settings.GetSelectedLayer(CADExportSettings.ExportGroup.MarkText);
  }

  public CADExportSettings updateSettings(
    CADExportSettings settings,
    Autodesk.Revit.DB.Units units,
    bool imperialUnits = true)
  {
    settings.CADExportFolderPath = string.IsNullOrWhiteSpace(this.CADExportFolderPathString) ? string.Empty : this.CADExportFolderPathString;
    settings.CADFileNameSuffix = string.IsNullOrWhiteSpace(this.CADExportFileNameSuffixString) ? string.Empty : this.CADExportFileNameSuffixString;
    settings.InsulationExportFolderPath = string.IsNullOrWhiteSpace(this.InsulationExportFolderPathString) ? string.Empty : this.InsulationExportFolderPathString;
    settings.InsulationFileNameSuffix = string.IsNullOrWhiteSpace(this.InsulationExportFileNameSuffixString) ? string.Empty : this.InsulationExportFileNameSuffixString;
    settings.CADExportDWGFiles = this.CADExportDWGFileExport;
    settings.CADExportDXFFiles = this.CADExportDXFFileExport;
    settings.InsulationExportDWGFiles = this.InsulationExportDWGFileExport;
    settings.InsulationExportDXFFiles = this.InsulationExportDXFFileExport;
    settings.TIFExportBoundingBox = this.TIFExportBoundingBoxBool;
    settings.BIFExportBoundingBox = this.BIFExportBoundingBoxBool;
    if (imperialUnits)
    {
      ValueParsingOptions valueParsingOptions1 = new ValueParsingOptions();
      FormatOptions formatOptions1 = new FormatOptions(UnitTypeId.Inches, SymbolTypeId.InchDoubleQuote);
      valueParsingOptions1.SetFormatOptions(formatOptions1);
      ValueParsingOptions valueParsingOptions2 = new ValueParsingOptions();
      FormatOptions formatOptions2 = new FormatOptions(UnitTypeId.Inches, SymbolTypeId.In);
      valueParsingOptions2.SetFormatOptions(formatOptions2);
      double dbl1_1;
      if (UnitFormatUtils.TryParse(units, SpecTypeId.Length, this.FixedCircleRadius, valueParsingOptions1, out dbl1_1))
      {
        if (!dbl1_1.ApproximatelyEqual(settings.fixedCircleRadiusInternalUnits))
          settings.fixedCircleRadiusInternalUnits = dbl1_1;
      }
      else if (UnitFormatUtils.TryParse(units, SpecTypeId.Length, this.FixedCircleRadius, valueParsingOptions2, out dbl1_1) && !dbl1_1.ApproximatelyEqual(settings.fixedCircleRadiusInternalUnits))
        settings.fixedCircleRadiusInternalUnits = dbl1_1;
      double dbl1_2;
      if (UnitFormatUtils.TryParse(units, SpecTypeId.Length, this.TickMarkLength, valueParsingOptions1, out dbl1_2))
      {
        if (!dbl1_2.ApproximatelyEqual(settings.tickMarkLengthInternalUnits))
          settings.tickMarkLengthInternalUnits = dbl1_2;
      }
      else if (UnitFormatUtils.TryParse(units, SpecTypeId.Length, this.TickMarkLength, valueParsingOptions1, out dbl1_2) && !dbl1_2.ApproximatelyEqual(settings.tickMarkLengthInternalUnits))
        settings.tickMarkLengthInternalUnits = dbl1_2;
    }
    else
    {
      ValueParsingOptions valueParsingOptions = new ValueParsingOptions();
      FormatOptions formatOptions = new FormatOptions(UnitTypeId.Millimeters, SymbolTypeId.Mm);
      valueParsingOptions.SetFormatOptions(formatOptions);
      double dbl1_3;
      if (UnitFormatUtils.TryParse(units, SpecTypeId.Length, this.FixedCircleRadius, valueParsingOptions, out dbl1_3) && !dbl1_3.ApproximatelyEqual(settings.fixedCircleRadiusInternalUnits))
        settings.fixedCircleRadiusInternalUnits = dbl1_3;
      double dbl1_4;
      if (UnitFormatUtils.TryParse(units, SpecTypeId.Length, this.TickMarkLength, valueParsingOptions, out dbl1_4) && !dbl1_4.ApproximatelyEqual(settings.tickMarkLengthInternalUnits))
        settings.tickMarkLengthInternalUnits = dbl1_4;
    }
    settings.UseEdgeCadLines = this.UseEdgeCADLines;
    settings.ClearLayerSettings();
    foreach (CADExportLayer exportLayersDataGrid in (Collection<CADExportLayer>) this.CADExportLayersDataGridList)
    {
      if (!string.IsNullOrWhiteSpace(exportLayersDataGrid.LayerName) && exportLayersDataGrid.LayerColor != null)
        settings.AddCADExportLayer(exportLayersDataGrid);
    }
    foreach (CADExportLayer exportLayersDataGrid in (Collection<CADExportLayer>) this.InsulationExportLayersDataGridList)
    {
      if (!string.IsNullOrWhiteSpace(exportLayersDataGrid.LayerName) && exportLayersDataGrid.LayerColor != null)
        settings.AddInsulationExportLayer(exportLayersDataGrid);
    }
    this.SetExportGroupLayer(CADExportSettings.ExportGroup.TIF, settings, this.TIFLayer);
    this.SetExportGroupLayer(CADExportSettings.ExportGroup.BIF, settings, this.BIFLayer);
    this.SetExportGroupLayer(CADExportSettings.ExportGroup.SIF, settings, this.SIFLayer);
    this.SetExportGroupLayer(CADExportSettings.ExportGroup.Form, settings, this.FormLayer);
    this.SetExportGroupLayer(CADExportSettings.ExportGroup.Corbel, settings, this.CorbelLayer);
    this.SetExportGroupLayer(CADExportSettings.ExportGroup.Arch, settings, this.ArchLayer);
    this.SetExportGroupLayer(CADExportSettings.ExportGroup.Strand, settings, this.StrandLayer);
    this.SetExportGroupLayer(CADExportSettings.ExportGroup.Insulation, settings, this.InsulationLayer);
    this.SetExportGroupLayer(CADExportSettings.ExportGroup.Cutout, settings, this.CutoutsLayer);
    this.SetExportGroupLayer(CADExportSettings.ExportGroup.Pins, settings, this.PinsLayer);
    this.SetExportGroupLayer(CADExportSettings.ExportGroup.Slots, settings, this.SlotLayer);
    this.SetExportGroupLayer(CADExportSettings.ExportGroup.ExtraTies, settings, this.ExtraTiesLayer);
    this.SetExportGroupLayer(CADExportSettings.ExportGroup.MarkSymbol, settings, this.MarkSymbolLayer);
    this.SetExportGroupLayer(CADExportSettings.ExportGroup.MarkText, settings, this.InsulationMarkLayer);
    return settings;
  }

  private void SetExportGroupLayer(
    CADExportSettings.ExportGroup group,
    CADExportSettings settings,
    CADExportLayer layer)
  {
    if (DefaultCADExportSettings.disabledLayer.Equals(layer) || layer == null)
      settings.DisableExportGroupLayer(group);
    else
      settings.setExportGroupLayer(group, layer);
  }

  public event PropertyChangedEventHandler PropertyChanged;

  private void NotifyPropertyChanged(string propertyName)
  {
    if (this.PropertyChanged == null)
      return;
    this.PropertyChanged((object) this, new PropertyChangedEventArgs(propertyName));
  }

  public void UpdatePropertyChangedEvent(string propertyName)
  {
    this.NotifyPropertyChanged(propertyName);
  }

  private bool SameContents(
    ObservableCollection<CADExportLayer> collection1,
    ObservableCollection<CADExportLayer> collection2)
  {
    if (collection1 == null && collection2 == null)
      return true;
    if (collection1 == null || collection2 == null || collection1.Count != collection2.Count)
      return false;
    List<int> intList = new List<int>();
    foreach (CADExportLayer cadExportLayer1 in (Collection<CADExportLayer>) collection1)
    {
      int num = -1;
      bool flag = true;
      foreach (CADExportLayer cadExportLayer2 in (Collection<CADExportLayer>) collection2)
      {
        ++num;
        if (!intList.Contains(num) && cadExportLayer1.Equals(cadExportLayer2))
        {
          intList.Add(num);
          flag = false;
          break;
        }
      }
      if (flag)
        return false;
    }
    return true;
  }

  public ObservableCollection<CADExportLayer> getComboBoxList(
    ObservableCollection<CADExportLayer> dataGridList)
  {
    List<CADExportLayer> list = new List<CADExportLayer>();
    list.Add(DefaultCADExportSettings.disabledLayer);
    foreach (CADExportLayer dataGrid in (Collection<CADExportLayer>) dataGridList)
    {
      if (!string.IsNullOrWhiteSpace(dataGrid.LayerName) && dataGrid.LayerColor != null)
        list.Add(dataGrid);
    }
    return new ObservableCollection<CADExportLayer>(list);
  }
}
