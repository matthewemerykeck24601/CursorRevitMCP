// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.ResultsPresentation.TestResult
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System.Collections.Generic;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification.ResultsPresentation;

public class TestResult
{
  private bool _notUsed;
  private string _testName;
  private string _testDescription;
  private bool _bPassed;
  private ElementId _standardStructuralFramingId;
  private ElementId _failingStructuralFramingId;
  private string _actualsfelementid;
  private string _expectedfailingreason;
  private bool _baseLine;
  public Dictionary<string, List<string>> DetailsforFaults = new Dictionary<string, List<string>>();
  private Dictionary<string, List<ElementId>> _detailsforLocation;
  private MVEEnhancedDetails _mVE;
  private CompareVolume _cv;
  private List<EDGE.AssemblyTools.MarkVerification.ResultsPresentation.PlateTypes> _plateTypes;
  private List<dataForAddonTest> _dataForAddon;
  private string _actualresult;
  private List<AddonLocation> _locationList;
  private string _imageDecider;
  public Dictionary<string, List<ElementId>> Allthewarnings = new Dictionary<string, List<ElementId>>();

  public bool NotUsed
  {
    get => this._notUsed;
    set => this._notUsed = value;
  }

  public string TestName
  {
    get => this._testName;
    set => this._testName = value;
  }

  public string TestDescription
  {
    get => this._testDescription;
    set => this._testDescription = value;
  }

  public bool Passed
  {
    get => this._bPassed;
    set => this._bPassed = value;
  }

  public ElementId StandardStructuralFramingId
  {
    get => this._standardStructuralFramingId;
    set => this._standardStructuralFramingId = value;
  }

  public ElementId FailingStructuralFramingId
  {
    get => this._failingStructuralFramingId;
    set => this._failingStructuralFramingId = value;
  }

  public string ActualSFelementid
  {
    get => this._actualsfelementid;
    set => this._actualsfelementid = value;
  }

  public string Expectedfailingreason
  {
    get => this._expectedfailingreason;
    set => this._expectedfailingreason = value;
  }

  public bool BaseLine
  {
    get => this._baseLine;
    set => this._baseLine = value;
  }

  public Dictionary<string, List<ElementId>> DetailsforLocation
  {
    get => this._detailsforLocation;
    set => this._detailsforLocation = value;
  }

  public MVEEnhancedDetails MVE
  {
    get => this._mVE;
    set => this._mVE = value;
  }

  public CompareVolume CV
  {
    get => this._cv;
    set => this._cv = value;
  }

  public List<EDGE.AssemblyTools.MarkVerification.ResultsPresentation.PlateTypes> PlateTypes
  {
    get => this._plateTypes;
    set => this._plateTypes = value;
  }

  public List<dataForAddonTest> DataForAddOn
  {
    get => this._dataForAddon;
    set => this._dataForAddon = value;
  }

  public string ActualResult
  {
    get => this._actualresult;
    set => this._actualresult = value;
  }

  public List<AddonLocation> Locations
  {
    get => this._locationList;
    set => this._locationList = value;
  }

  public bool FailedPlateRotationTest { get; set; }

  public Dictionary<string, List<ElementId>> FailedRotationPlates { get; set; }

  public bool EmbedsExcludedBool { get; set; }

  public Dictionary<string, List<ElementId>> EmbedsExcludedDictionary { get; set; }

  public TestResult(MKTest testType, bool notused, int i)
  {
    this._notUsed = notused;
    this.Init(testType);
  }

  public string ImageDecider
  {
    get
    {
      if (this._bPassed && !this._notUsed)
        this._imageDecider = "pass";
      else if (!this._bPassed && !this._notUsed)
        this._imageDecider = "fail";
      else if (!this._bPassed && this._notUsed)
        this._imageDecider = "neutral";
      return this._imageDecider;
    }
  }

  public TestResult(
    MKTest testType,
    bool Pass,
    ElementId standardElementId = null,
    ElementId failElementId = null,
    FamilyInstance fistandard = null,
    FamilyInstance failingInstance = null,
    Dictionary<string, List<ElementId>> solidcomp = null)
  {
    this._bPassed = Pass;
    this._standardStructuralFramingId = standardElementId;
    this._failingStructuralFramingId = failElementId;
    this.Init(testType);
    this.FailedPlateRotationTest = false;
    string str1 = "";
    string str2 = "";
    if (fistandard == null || !testType.Equals((object) MKTest.Geometry))
      return;
    if (solidcomp.Count > 0)
    {
      this.DetailsforLocation = solidcomp;
      foreach (string key in solidcomp.Keys)
      {
        str1 = key;
        str2 = string.Join<ElementId>(";", (IEnumerable<ElementId>) solidcomp[key]);
      }
    }
    this._actualsfelementid = "Family: " + str1;
    this._actualresult = "Failing ID: " + str2;
  }

  public TestResult()
  {
    this._testName = "<undetermined test>";
    this._testDescription = "<undetermined test>";
    this._bPassed = false;
    this._standardStructuralFramingId = ElementId.InvalidElementId;
    this._failingStructuralFramingId = ElementId.InvalidElementId;
    this.FailedPlateRotationTest = false;
  }

  private void Init(MKTest testType)
  {
    switch (testType)
    {
      case MKTest.FamilyType:
        this._testName = "Family Types Comparison";
        this._testDescription = "Compare the family and type of each structural framing element which share the same control mark.";
        break;
      case MKTest.ParameterComparison:
        this._testName = "Compare Family Parameters";
        this._testDescription = "Compare parameter values between each member of the group.";
        break;
      case MKTest.MainMaterialVolume:
        this._testName = "Compare Main Material Volumes";
        this._testDescription = "Compares volumes of individual materials in the main structural framing element.";
        break;
      case MKTest.AddonFamilyVolumeCount:
        this._testName = "Addon Family Types, Counts, and Material Volumes";
        this._testDescription = "Compares the number of addon components associated with each structural framing element.  Counts of each family type and the material volumes of the addons will be compared.";
        break;
      case MKTest.PlateNamesCounts:
        this._testName = "Compare Plate Family Types and Counts";
        this._testDescription = "Compares counts of embedded plates or other connector components between elements.";
        break;
      case MKTest.AddonLocation:
        this._testName = "Compare Addon Locations";
        this._testDescription = "Compares precise location of addon components between elements.";
        break;
      case MKTest.PlateLocation:
        this._testName = "Compare Plate Locations";
        this._testDescription = "Compares precise location of plates/embed components between elements.";
        break;
      case MKTest.Geometry:
        this._testName = "Compare Member Geometry";
        this._testDescription = "Compares precise location of geometric faces between elements.";
        break;
      case MKTest.MirrorState:
        this._testName = "Compare Mirror State of Elements";
        this._testDescription = "Compares the mirrored state of the main structural framing element.";
        break;
      default:
        this._testName = "<undetermined test>";
        this._testDescription = "<undetermined test>";
        break;
    }
  }
}
