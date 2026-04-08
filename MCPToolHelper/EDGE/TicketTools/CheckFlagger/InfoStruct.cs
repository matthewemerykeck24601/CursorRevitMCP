// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.CheckFlagger.InfoStruct
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;
using System.Globalization;

#nullable disable
namespace EDGE.TicketTools.CheckFlagger;

public class InfoStruct
{
  public bool yesNo;
  public string currUser = "-";
  public string initialUser = "-";
  private string _currDate = "-";
  private string _initialDate = "-";

  public string currDate
  {
    get => this._currDate;
    set => this._currDate = this.DateConversion(value);
  }

  public string initialDate
  {
    get => this._initialDate;
    set => this._initialDate = this.DateConversion(value);
  }

  private string DateConversion(string s)
  {
    DateTime result;
    return DateTime.TryParseExact(s, "yyyy-MM-dd-HH-mm-ss", (IFormatProvider) CultureInfo.InvariantCulture, DateTimeStyles.None, out result) ? result.ToString("yyyy-MM-dd") : s;
  }
}
