// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketManager.ViewModels.AssemblyViewModel
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using Utils.AssemblyUtils;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.TicketTools.TicketManager.ViewModels;

public class AssemblyViewModel : INotifyPropertyChanged
{
  private ICollection<ViewSheet> _viewSheets;
  private string _releasedDate;
  private string _releasedDate_Initial;
  private string _releasedBy;
  private string _releasedBy_Initial;
  private string _markNumberBackgroundColor;
  private string _markNumberTextColor;
  private string _quantityBackgroundColor;
  private string _quantityTextColor;
  private bool _createdFlag;
  private bool _detailedFlag;
  private bool _releasedFlag;
  private bool _invalidQuantity;
  private string _warnings;
  public AssemblyInstance Assembly;

  public bool isInvalidQuantity { get; set; }

  public List<ViewSheet> AssociatedViewSheets { get; set; }

  public string MarkNumber { get; set; }

  public List<Autodesk.Revit.DB.ElementId> StructuralFramingElemIds { get; set; }

  public string Assemblied { get; set; }

  public bool isSelected { get; set; }

  public int Quantity { get; set; }

  public string ReinforcedDate { get; set; }

  public string ReinforcedDate_Initial { get; set; }

  public string ReinforcedBy { get; set; }

  public string ReinforcedBy_Initial { get; set; }

  public string CreatedDate { get; set; }

  public string CreatedDate_Initial { get; set; }

  public string CreatedBy { get; set; }

  public string CreatedBy_Initial { get; set; }

  public string DetailedDate { get; set; }

  public string DetailedDate_Initial { get; set; }

  public string DetailedBy { get; set; }

  public string DetailedBy_Initial { get; set; }

  public string DraftingDate { get; set; }

  public string DraftingDate_Initial { get; set; }

  public string DraftingChecked { get; set; }

  public string DraftingChecked_Initial { get; set; }

  public string EngineeringDate { get; set; }

  public string EngineeringDate_Initial { get; set; }

  public string EngineeringChecked { get; set; }

  public string EngineeringChecked_Initial { get; set; }

  public string NeedsRevision { get; set; }

  public string OnHold { get; set; }

  public string IdentityComment { get; set; }

  public string Custom1 { get; set; }

  public string Custom2 { get; set; }

  public string Custom3 { get; set; }

  public string Custom4 { get; set; }

  public string CustomColumnName1 { get; set; }

  public string CustomColumnName2 { get; set; }

  public string CustomColumnName3 { get; set; }

  public string CustomColumnName4 { get; set; }

  public string ElementId { get; set; }

  public List<string> FriendlyNames { get; set; }

  public List<Element> SFElementsOfThisControlMark { get; set; }

  public Document revitDoc { get; set; }

  [DebuggerHidden]
  public string ReleasedDate
  {
    get => this._releasedDate;
    set
    {
      this._releasedDate = value;
      this.OnPropertyChanged(nameof (ReleasedDate));
    }
  }

  public string ReleasedDate_Initial
  {
    get => this._releasedDate_Initial;
    set
    {
      this._releasedDate_Initial = value;
      this.OnPropertyChanged(nameof (ReleasedDate_Initial));
    }
  }

  [DebuggerHidden]
  public string ReleasedBy
  {
    get => this._releasedBy;
    set
    {
      this._releasedBy = value;
      this.OnPropertyChanged(nameof (ReleasedBy));
    }
  }

  public string ReleasedBy_Initial
  {
    get => this._releasedBy_Initial;
    set
    {
      this._releasedBy_Initial = value;
      this.OnPropertyChanged(nameof (ReleasedBy_Initial));
    }
  }

  public string Description { get; set; }

  public bool DateFlagged { get; set; }

  public bool SheetFlagged { get; set; }

  public bool SheetCountFlagged { get; set; }

  public bool ReleaseFlagged { get; set; }

  public bool IsFlagged { get; set; }

  [DebuggerHidden]
  public string MarkNumberBackgroundColor
  {
    get => this._markNumberBackgroundColor;
    set
    {
      this._markNumberBackgroundColor = value;
      this.OnPropertyChanged(nameof (MarkNumberBackgroundColor));
    }
  }

  [DebuggerHidden]
  public string MarkNumberTextColor
  {
    get => this._markNumberTextColor;
    set
    {
      this._markNumberTextColor = value;
      this.OnPropertyChanged(nameof (MarkNumberTextColor));
    }
  }

  [DebuggerHidden]
  public string QuantityBackgroundColor
  {
    get => this._quantityBackgroundColor;
    set
    {
      this._quantityBackgroundColor = value;
      this.OnPropertyChanged(nameof (QuantityBackgroundColor));
    }
  }

  [DebuggerHidden]
  public string QuantityTextColor
  {
    get => this._quantityTextColor;
    set
    {
      this._quantityTextColor = value;
      this.OnPropertyChanged(nameof (QuantityTextColor));
    }
  }

  public string Warnings
  {
    get => this._warnings;
    set
    {
      this._warnings = value;
      this.OnPropertyChanged(nameof (Warnings));
    }
  }

  public AssemblyViewModel()
  {
    this.Assembly = (AssemblyInstance) null;
    this.MarkNumber = "";
    this.isSelected = false;
    this.Assemblied = "";
    this.Quantity = 0;
    this.ReinforcedDate = "";
    this.ReinforcedDate_Initial = "";
    this.ReinforcedBy = "";
    this.ReinforcedBy_Initial = "";
    this.CreatedDate = "";
    this.CreatedDate_Initial = "";
    this.CreatedBy = "";
    this.CreatedBy_Initial = "";
    this.DetailedDate = "";
    this.DetailedDate_Initial = "";
    this.DetailedBy = "";
    this.DetailedBy_Initial = "";
    this.DraftingDate = "";
    this.DraftingDate_Initial = "";
    this.DraftingChecked = "";
    this.DraftingChecked_Initial = "";
    this.EngineeringDate = "";
    this.EngineeringDate_Initial = "";
    this.EngineeringChecked = "";
    this.EngineeringChecked_Initial = "";
    this.NeedsRevision = "";
    this.ReleasedDate = "";
    this.ReleasedDate_Initial = "";
    this.ReleasedBy = "";
    this.ReleasedBy_Initial = "";
    this.OnHold = (string) null;
    this.IdentityComment = (string) null;
    this.Custom1 = "";
    this.Custom2 = "";
    this.Custom3 = "";
    this.Custom4 = "";
    this.CustomColumnName1 = "";
    this.CustomColumnName2 = "";
    this.CustomColumnName3 = "";
    this.CustomColumnName4 = "";
    this.ElementId = "";
    this.MarkNumberBackgroundColor = "Transparent";
    this.MarkNumberTextColor = "Black";
    this.Description = "";
    this.Warnings = "";
    this.QuantityBackgroundColor = "Transparent";
    this.QuantityTextColor = "Black";
    this.StructuralFramingElemIds = new List<Autodesk.Revit.DB.ElementId>();
  }

  public AssemblyViewModel(
    string controlMark,
    List<Element> sfElementsOfThisControlMark,
    ICollection<ViewSheet> viewSheets,
    Dictionary<string, Autodesk.Revit.DB.ElementId> assembliesByControlMark,
    List<string> friendlyNames,
    List<string> paramNames,
    Document doc)
  {
    AssemblyInstance assemblyInstance = (AssemblyInstance) null;
    Element elem1 = sfElementsOfThisControlMark.FirstOrDefault<Element>();
    this.SFElementsOfThisControlMark = sfElementsOfThisControlMark;
    if (assembliesByControlMark.ContainsKey(controlMark))
      assemblyInstance = doc.GetElement(assembliesByControlMark[controlMark]) as AssemblyInstance;
    this.Assembly = assemblyInstance;
    this.revitDoc = doc;
    if (this.Assembly != null)
    {
      this.MarkNumber = Utils.ElementUtils.Parameters.GetParameterAsString((Element) assemblyInstance, "ASSEMBLY_MARK_NUMBER");
      this.Assemblied = "Yes";
      this._viewSheets = viewSheets;
      this.GetAssociatedViewSheets(doc);
      this.Quantity = sfElementsOfThisControlMark.Count;
      this.ParseDateFromParameter(assemblyInstance, "TICKET_REINFORCED_DATE_CURRENT");
      this.ReinforcedDate_Initial = this.ParseDateFromParameter(assemblyInstance, "TICKET_REINFORCED_DATE_INITIAL");
      Utils.ElementUtils.Parameters.GetParameterAsString((Element) assemblyInstance, "TICKET_REINFORCED_USER_CURRENT");
      this.ReinforcedBy_Initial = Utils.ElementUtils.Parameters.GetParameterAsString((Element) assemblyInstance, "TICKET_REINFORCED_USER_INITIAL");
      this.ParseDateFromParameter(assemblyInstance, "TICKET_CREATED_DATE_CURRENT");
      this.CreatedDate_Initial = this.ParseDateFromParameter(assemblyInstance, "TICKET_CREATED_DATE_INITIAL");
      Utils.ElementUtils.Parameters.GetParameterAsString((Element) assemblyInstance, "TICKET_CREATED_USER_CURRENT");
      this.CreatedBy_Initial = Utils.ElementUtils.Parameters.GetParameterAsString((Element) assemblyInstance, "TICKET_CREATED_USER_INITIAL");
      this.ParseDateFromParameter(assemblyInstance, "TICKET_DETAILED_DATE_CURRENT");
      this.DetailedDate_Initial = this.ParseDateFromParameter(assemblyInstance, "TICKET_DETAILED_DATE_INITIAL");
      Utils.ElementUtils.Parameters.GetParameterAsString((Element) assemblyInstance, "TICKET_DETAILED_USER_CURRENT");
      this.DetailedBy_Initial = Utils.ElementUtils.Parameters.GetParameterAsString((Element) assemblyInstance, "TICKET_DETAILED_USER_INITIAL");
      this.ParseDateFromParameter(assemblyInstance, "TICKET_DRAFTING_CHECKED_DATE_CURRENT");
      this.DraftingDate_Initial = this.ParseDateFromParameter(assemblyInstance, "TICKET_DRAFTING_CHECKED_DATE_INITIAL");
      this.DraftingChecked_Initial = Utils.ElementUtils.Parameters.GetParameterAsString((Element) assemblyInstance, "TICKET_DRAFTING_CHECKED_USER_INITIAL");
      Utils.ElementUtils.Parameters.GetParameterAsString((Element) assemblyInstance, "TICKET_DRAFTING_CHECKED_USER_CURRENT");
      this.ParseDateFromParameter(assemblyInstance, "TICKET_ENGINEERING_CHECKED_DATE_CURRENT");
      this.EngineeringDate_Initial = this.ParseDateFromParameter(assemblyInstance, "TICKET_ENGINEERING_CHECKED_DATE_INITIAL");
      this.EngineeringChecked_Initial = Utils.ElementUtils.Parameters.GetParameterAsString((Element) assemblyInstance, "TICKET_ENGINEERING_CHECKED_USER_INITIAL");
      Utils.ElementUtils.Parameters.GetParameterAsString((Element) assemblyInstance, "TICKET_ENGINEERING_CHECKED_USER_CURRENT");
      string parameterAsString = Utils.ElementUtils.Parameters.GetParameterAsString((Element) assemblyInstance, "TICKET_NEEDS_REVISION");
      this.NeedsRevision = !(parameterAsString != "") || parameterAsString.Length <= 10 ? parameterAsString : parameterAsString.Substring(0, 10);
      this.ParseDateFromParameter(assemblyInstance, "TICKET_RELEASED_DATE_CURRENT");
      this.ReleasedDate_Initial = this.ParseDateFromParameter(assemblyInstance, "TICKET_RELEASED_DATE_INITIAL");
      Utils.ElementUtils.Parameters.GetParameterAsString((Element) assemblyInstance, "TICKET_RELEASED_USER_CURRENT");
      this.ReleasedBy_Initial = Utils.ElementUtils.Parameters.GetParameterAsString((Element) assemblyInstance, "TICKET_RELEASED_USER_INITIAL");
      string asStringTmVariant1 = AssemblyViewModel.GetParameterAsStringTMVariant((Element) assemblyInstance, "ON_HOLD");
      this.OnHold = AssemblyViewModel.checkParamValueConsistency(sfElementsOfThisControlMark, asStringTmVariant1, "ON_HOLD", this.Assembly) ? asStringTmVariant1 : "Parameter values are not the same across mark numbers.";
      string asStringTmVariant2 = AssemblyViewModel.GetParameterAsStringTMVariant((Element) assemblyInstance, "IDENTITY_COMMENT");
      this.IdentityComment = AssemblyViewModel.checkParamValueConsistency(sfElementsOfThisControlMark, asStringTmVariant2, "IDENTITY_COMMENT", this.Assembly) ? asStringTmVariant2 : "Parameter values are not the same across mark numbers.";
      if (paramNames.Count > 0)
      {
        for (int index = 0; index < paramNames.Count; ++index)
        {
          string asStringTmVariant3 = AssemblyViewModel.GetParameterAsStringTMVariant((Element) assemblyInstance, paramNames[index]);
          string str = AssemblyViewModel.checkParamValueConsistency(sfElementsOfThisControlMark, asStringTmVariant3, paramNames[index], this.Assembly) ? asStringTmVariant3 : "Parameter values are not the same across mark numbers.";
          switch (index)
          {
            case 0:
              this.CustomColumnName1 = paramNames[index];
              this.Custom1 = str;
              break;
            case 1:
              this.CustomColumnName2 = paramNames[index];
              this.Custom2 = str;
              break;
            case 2:
              this.CustomColumnName3 = paramNames[index];
              this.Custom3 = str;
              break;
            case 3:
              this.CustomColumnName4 = paramNames[index];
              this.Custom4 = str;
              break;
          }
        }
      }
      this.ElementId = assemblyInstance.Id.ToString();
      this.Description = Utils.ElementUtils.Parameters.GetParameterAsString(elem1, "CONSTRUCTION_PRODUCT");
      this.StructuralFramingElemIds = new List<Autodesk.Revit.DB.ElementId>();
      this.ReinforcedDate = this.ReinforcedDate_Initial;
      this.ReinforcedBy = this.ReinforcedBy_Initial;
      this.CreatedDate = this.CreatedDate_Initial;
      this.CreatedBy = this.CreatedBy_Initial;
      this.DetailedDate = this.DetailedDate_Initial;
      this.DetailedBy = this.DetailedBy_Initial;
      this.ReleasedDate = this.ReleasedDate_Initial;
      this.ReleasedBy = this.ReleasedBy_Initial;
    }
    else
    {
      this.MarkNumber = controlMark;
      this.Assemblied = "No";
      this.Quantity = sfElementsOfThisControlMark.Count;
      this.ReinforcedDate = "";
      this.ReinforcedBy = "";
      this.CreatedDate = "";
      this.CreatedBy = "";
      this.DetailedDate = "";
      this.DetailedBy = "";
      this.ReleasedDate = "";
      this.ReleasedBy = "";
      this.OnHold = "";
      this.IdentityComment = "";
      string asStringTmVariant4 = AssemblyViewModel.GetParameterAsStringTMVariant(elem1, "ON_HOLD");
      this.OnHold = AssemblyViewModel.checkParamValueConsistency(sfElementsOfThisControlMark, asStringTmVariant4, "ON_HOLD") ? asStringTmVariant4 : "Parameter values are not the same across mark numbers.";
      string asStringTmVariant5 = AssemblyViewModel.GetParameterAsStringTMVariant(elem1, "IDENTITY_COMMENT");
      this.IdentityComment = AssemblyViewModel.checkParamValueConsistency(sfElementsOfThisControlMark, asStringTmVariant5, "IDENTITY_COMMENT") ? asStringTmVariant5 : "Parameter values are not the same across mark numbers.";
      this.Custom1 = "";
      this.Custom2 = "";
      this.Custom3 = "";
      this.Custom4 = "";
      if (paramNames.Count > 0)
      {
        for (int index = 0; index < paramNames.Count; ++index)
        {
          string asStringTmVariant6 = AssemblyViewModel.GetParameterAsStringTMVariant(elem1, paramNames[index]);
          string str = AssemblyViewModel.checkParamValueConsistency(sfElementsOfThisControlMark, asStringTmVariant6, paramNames[index]) ? asStringTmVariant6 : "Parameter values are not the same across mark numbers.";
          switch (index)
          {
            case 0:
              this.CustomColumnName1 = paramNames[index];
              this.Custom1 = str;
              break;
            case 1:
              this.CustomColumnName2 = paramNames[index];
              this.Custom2 = str;
              break;
            case 2:
              this.CustomColumnName3 = paramNames[index];
              this.Custom3 = str;
              break;
            case 3:
              this.CustomColumnName4 = paramNames[index];
              this.Custom4 = str;
              break;
          }
        }
      }
      this.ElementId = elem1.Id.ToString();
      this.Description = Utils.ElementUtils.Parameters.GetParameterAsString(sfElementsOfThisControlMark.First<Element>(), "CONSTRUCTION_PRODUCT");
      this.StructuralFramingElemIds = sfElementsOfThisControlMark.Select<Element, Autodesk.Revit.DB.ElementId>((Func<Element, Autodesk.Revit.DB.ElementId>) (elem => elem.Id)).ToList<Autodesk.Revit.DB.ElementId>();
    }
    this.SetFlagStatus();
    this.SetWarnings();
    this.SetMarkNumberCellColors();
    this.SetQuantityCellColors();
  }

  public event PropertyChangedEventHandler PropertyChanged;

  protected void OnPropertyChanged(string name)
  {
    PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
    if (propertyChanged == null)
      return;
    propertyChanged((object) this, new PropertyChangedEventArgs(name));
  }

  private string ParseDateFromParameter(AssemblyInstance assembly, string parameter)
  {
    string empty = string.Empty;
    string parameterAsString = Utils.ElementUtils.Parameters.GetParameterAsString((Element) assembly, parameter);
    return !(parameterAsString != "") || parameterAsString.Length <= 10 ? parameterAsString : parameterAsString.Substring(0, 10);
  }

  public static bool checkParamValueConsistency(
    List<Element> sameMarkSF,
    string paramValue,
    string paramName,
    AssemblyInstance assembly = null)
  {
    if (assembly != null && assembly.LookupParameter(paramName) != null)
      return true;
    foreach (Element elem in sameMarkSF)
    {
      if (!AssemblyViewModel.GetParameterAsStringTMVariant(elem, paramName).Equals(paramValue))
        return false;
    }
    return true;
  }

  public static int GetQuantity(
    string assemblyMarkNumber,
    IEnumerable<IGrouping<string, Element>> structFramingGroups)
  {
    using (IEnumerator<IGrouping<string, Element>> enumerator = structFramingGroups.Where<IGrouping<string, Element>>((Func<IGrouping<string, Element>, bool>) (group => group.Key.Equals(assemblyMarkNumber))).GetEnumerator())
    {
      if (enumerator.MoveNext())
        return enumerator.Current.ToList<Element>().Count;
    }
    return 0;
  }

  public void SetMarkNumberCellColors()
  {
    string[] strArray = new string[2]
    {
      "Black",
      "Transparent"
    };
    if (this.IsFlagged && this.DateFlagged || this.SheetFlagged || this.ReleaseFlagged && this.IsFlagged || this.SheetCountFlagged)
      strArray = new string[2]{ "White", "Red" };
    this.MarkNumberTextColor = strArray[0];
    this.MarkNumberBackgroundColor = strArray[1];
  }

  public void SetQuantityCellColors()
  {
    if (this.Assembly == null)
    {
      this.QuantityBackgroundColor = "Transparent";
      this.QuantityTextColor = "Black";
    }
    else
    {
      string[] strArray = new string[2]
      {
        "Black",
        "Transparent"
      };
      if (this.isInvalidQuantity && this._invalidQuantity)
        strArray = new string[2]{ "White", "Red" };
      this.QuantityTextColor = strArray[0];
      this.QuantityBackgroundColor = strArray[1];
    }
  }

  public void SetFlagStatus()
  {
    if (this.Assembly == null)
      return;
    this.IsFlagged = Utils.ElementUtils.Parameters.GetParameterAsInt((Element) this.Assembly, "TICKET_FLAGGED") == 1;
    string parameterAsString1 = Utils.ElementUtils.Parameters.GetParameterAsString((Element) this.Assembly, "TICKET_REINFORCED_DATE_CURRENT");
    string str = Utils.ElementUtils.Parameters.GetParameterAsString((Element) this.Assembly, "TICKET_CREATED_DATE_CURRENT");
    string parameterAsString2 = Utils.ElementUtils.Parameters.GetParameterAsString((Element) this.Assembly, "TICKET_DETAILED_DATE_CURRENT");
    string parameterAsString3 = Utils.ElementUtils.Parameters.GetParameterAsString((Element) this.Assembly, "TICKET_RELEASED_DATE_CURRENT");
    if (this.AssociatedViewSheets.Count == 0 && str != "")
    {
      App.GlobalValueForViewSheetIssue.Add(this.Assembly);
      str = "";
    }
    int num1 = Utils.MiscUtils.MiscUtils.CompareDateStrings(str, parameterAsString1, "yyyy-MM-dd-HH-mm-ss");
    int num2 = Utils.MiscUtils.MiscUtils.CompareDateStrings(parameterAsString2, parameterAsString1, "yyyy-MM-dd-HH-mm-ss");
    int num3 = Utils.MiscUtils.MiscUtils.CompareDateStrings(parameterAsString2, str, "yyyy-MM-dd-HH-mm-ss");
    int num4 = Utils.MiscUtils.MiscUtils.CompareDateStrings(parameterAsString3, parameterAsString1, "yyyy-MM-dd-HH-mm-ss");
    int num5 = Utils.MiscUtils.MiscUtils.CompareDateStrings(parameterAsString3, str, "yyyy-MM-dd-HH-mm-ss");
    int num6 = Utils.MiscUtils.MiscUtils.CompareDateStrings(parameterAsString3, parameterAsString2, "yyyy-MM-dd-HH-mm-ss");
    if (str == "" && parameterAsString2 != "")
    {
      this.SheetFlagged = true;
    }
    else
    {
      this.SheetFlagged = num5 == -2 || num6 == -2;
      this.ReleaseFlagged = num5 == -1 || num6 == -1;
    }
    this._createdFlag = num1 < 0;
    this._detailedFlag = num2 < 0 || num3 == -1 || num3 < 0 && this.AssociatedViewSheets.Count < 2;
    this._releasedFlag = num4 < 0;
    this.DateFlagged = this._createdFlag || this._detailedFlag || this._releasedFlag;
    try
    {
      string parameterAsString4 = Utils.ElementUtils.Parameters.GetParameterAsString((Element) this.Assembly, "TKT_TOTAL_RELEASED");
      int parameterAsInt1 = Utils.ElementUtils.Parameters.GetParameterAsInt((Element) this.Assembly, "TKT_TOTAL_CREATED");
      int parameterAsInt2 = Utils.ElementUtils.Parameters.GetParameterAsInt((Element) this.Assembly, "TKT_TOTAL_DETAILED");
      if (this.Quantity != 0)
      {
        int quantity;
        if (!string.IsNullOrWhiteSpace(parameterAsString4))
        {
          quantity = this.Quantity;
          if (!quantity.ToString().Equals(parameterAsString4))
            goto label_14;
        }
        if (parameterAsInt1 != -1)
        {
          quantity = this.Quantity;
          if (!quantity.Equals(parameterAsInt1))
            goto label_14;
        }
        if (parameterAsInt2 != -1)
        {
          quantity = this.Quantity;
          if (!quantity.Equals(parameterAsInt2))
            goto label_14;
        }
        this.isInvalidQuantity = false;
        this._invalidQuantity = false;
        return;
      }
label_14:
      App.GlobalValueForQuantityIssue.Add(this.Assembly);
      this.isInvalidQuantity = true;
      this._invalidQuantity = true;
    }
    catch (Exception ex)
    {
      int num7 = (int) MessageBox.Show(ex.ToString());
    }
  }

  public void SetWarnings()
  {
    if (this.Assembly == null)
      return;
    string parameterAsString = Utils.ElementUtils.Parameters.GetParameterAsString((Element) this.Assembly, "TKT_TOTAL_RELEASED");
    int parameterAsInt1 = Utils.ElementUtils.Parameters.GetParameterAsInt((Element) this.Assembly, "TKT_TOTAL_CREATED");
    int parameterAsInt2 = Utils.ElementUtils.Parameters.GetParameterAsInt((Element) this.Assembly, "TKT_TOTAL_DETAILED");
    StringBuilder stringBuilder = new StringBuilder("");
    List<string> source1 = new List<string>();
    List<string> stringList1 = new List<string>();
    List<string> stringList2 = new List<string>();
    if (this.DateFlagged && this.IsFlagged || this.SheetFlagged || this.ReleaseFlagged && this.IsFlagged || this.SheetCountFlagged)
    {
      if (this._createdFlag && this.IsFlagged)
        source1.Add("Created");
      if (this._detailedFlag && this.IsFlagged)
        source1.Add("Detailed");
      if (this._releasedFlag || this.ReleaseFlagged)
        source1.Add("Released");
      if (this.SheetFlagged)
        stringList1.Add("delete before released");
      if (this.SheetCountFlagged)
        stringList2.Add("view sheet count changed");
      if (source1.Count > 0)
      {
        stringBuilder.Append("The Assembly associated with this Ticket has been modified since it was ");
        if (source1.Count == 1)
        {
          stringBuilder.Append(source1.First<string>() + ". Therefore the Ticket drawings may have errors and should be reviewed. ");
        }
        else
        {
          for (int index = 0; index < source1.Count - 1; ++index)
            stringBuilder.Append(source1.ElementAt<string>(index) + ", ");
          stringBuilder.Append($"and {source1.Last<string>()}. Therefore the Ticket drawings may have errors and should be reviewed. ");
        }
      }
      if (stringList1.Count > 0)
        stringBuilder.Append("The view sheets of the Assembly were deleted after the ticket has been detailed or released. Please review the create date.");
      if (stringList2.Count > 0)
        stringBuilder.Append("The number of the Assembly view sheets was changed after the ticket has been detailed or released. Please review the view sheet.");
    }
    if (this._invalidQuantity && this.isInvalidQuantity)
    {
      if (this.Quantity == 0)
      {
        stringBuilder.Append("The Assembly's ASSEMBLY_CONTROL_MARK parameter value does not match any Structural Framing Elements' CONTROL_MARK parameter value.  This piece needs to be reviewed and the Mark Number corrected for the Ticket (Assembly), or the Structural Framing element which will then have an effect on the Erection Drawings.");
      }
      else
      {
        int quantity;
        if (!string.IsNullOrWhiteSpace(parameterAsString))
        {
          quantity = this.Quantity;
          if (!quantity.ToString().Equals(parameterAsString))
            goto label_32;
        }
        if (parameterAsInt1 != -1)
        {
          quantity = this.Quantity;
          if (!quantity.Equals(parameterAsInt1))
            goto label_32;
        }
        if (parameterAsInt2 != -1)
        {
          quantity = this.Quantity;
          if (quantity.Equals(parameterAsInt2))
            goto label_48;
        }
        else
          goto label_48;
label_32:
        List<string> source2 = new List<string>();
        stringBuilder.Append("The Assembly's");
        if (parameterAsInt1 != -1)
        {
          quantity = this.Quantity;
          if (!quantity.Equals(parameterAsInt1))
            source2.Add(" created quantity");
        }
        if (parameterAsInt2 != -1)
        {
          quantity = this.Quantity;
          if (!quantity.Equals(parameterAsInt2))
            source2.Add(" detailed quantity");
        }
        if (!string.IsNullOrWhiteSpace(parameterAsString))
        {
          quantity = this.Quantity;
          if (!quantity.ToString().Equals(parameterAsString))
            source2.Add(" released quantity");
        }
        if (source2.Count == 1)
        {
          stringBuilder.Append(source2[0] + " no longer matches");
        }
        else
        {
          for (int index = 0; index < source2.Count - 1; ++index)
            stringBuilder.Append(source2.ElementAt<string>(index) + ", ");
          stringBuilder.Append($"and {source2.Last<string>()} no longer match");
        }
        stringBuilder.Append(" the quantity of Structural Framing Elements in the model with the same CONTROL_MARK parameter value.");
      }
    }
label_48:
    this.Warnings = stringBuilder.ToString();
  }

  private void GetAssociatedViewSheets(Document doc)
  {
    this.AssociatedViewSheets = this._viewSheets.Where<ViewSheet>((Func<ViewSheet, bool>) (vs => vs.AssociatedAssemblyInstanceId == this.Assembly.Id)).ToList<ViewSheet>();
    if (this.AssociatedViewSheets.Count >= Utils.ElementUtils.Parameters.GetParameterAsInt((Element) this.Assembly, "TKT_VIEWSHEET_COUNT") || this.AssociatedViewSheets.Count == 0)
      return;
    string parameterAsString1 = Utils.ElementUtils.Parameters.GetParameterAsString((Element) this.Assembly, "TICKET_DETAILED_DATE_CURRENT");
    string parameterAsString2 = Utils.ElementUtils.Parameters.GetParameterAsString((Element) this.Assembly, "TICKET_RELEASED_DATE_CURRENT");
    if (!(parameterAsString1 != "") && !(parameterAsString2 != ""))
      return;
    App.GlobalValueForViewSheetCountIssue.Add(this.Assembly);
    this.SheetCountFlagged = true;
  }

  public static string GetParameterAsStringTMVariant(Element elem, string paramName)
  {
    if (elem.HasSuperComponent())
      elem = elem.GetTopLevelElement();
    if (elem == null)
      return "";
    Parameter parameter = Utils.ElementUtils.Parameters.LookupParameter(elem, paramName);
    string str1 = "";
    string str2 = "";
    if (parameter != null && parameter.HasValue)
    {
      str1 = parameter.AsString();
      if (parameter.AsValueString() != null)
        str2 = parameter.AsValueString();
    }
    if (str1 == null)
      str1 = "";
    if (str1.Equals("") && !str2.Equals(""))
      str1 = str2;
    if (parameter == null && elem is AssemblyInstance)
      str1 = AssemblyViewModel.GetParameterAsStringTMVariant((elem as AssemblyInstance).GetStructuralFramingElement(), paramName);
    return str1 ?? "";
  }
}
