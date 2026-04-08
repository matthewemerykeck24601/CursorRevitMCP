// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.CheckFlagger.CheckWin
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.TicketTools.CheckFlagger;

public class CheckWin : Window, IComponentConnector
{
  private Document _revitDoc;
  private UIDocument _uiDoc;
  private string _username;
  private Dictionary<InfoType, InfoStruct> currInfo = new Dictionary<InfoType, InfoStruct>();
  private List<InfoType> _typesToGet = new List<InfoType>()
  {
    InfoType.EngCheck,
    InfoType.DraftCheck,
    InfoType.NeedsRev
  };
  private List<AssemblyInstance> Assemblies;
  private int assemblyIDX;
  internal Label TitleLbl;
  internal Button LeftArrowBtn;
  internal Button RightArrowBtn;
  internal System.Windows.Controls.Grid CheckAllGrid;
  internal Button EngChkAllBtn;
  internal Button DraftChkAllBtn;
  internal Button NeedsRevAll;
  internal Button IssueRevAll;
  internal TextBlock EngYesNo;
  internal Button EngChkBtn;
  internal Label EngCurrentUser;
  internal Label EngCurrentDate;
  internal Label EngInitialUser;
  internal Label EngInitialDate;
  internal TextBlock DraftingYesNo;
  internal Button DraftChkBtn;
  internal Label DraftCurrentUser;
  internal Label DraftCurrentDate;
  internal Label DraftInitialUser;
  internal Label DraftInitialDate;
  internal TextBlock NeedsRevYesNo;
  internal TextBlock RevIssuedDate;
  internal Button NeedsRevBtn;
  internal Button IssueRevBtn;
  internal Button DoneBtn;
  private bool _contentLoaded;

  private AssemblyInstance CurrentAssembly => this.Assemblies[this.assemblyIDX];

  public CheckWin(UIDocument uiDoc, List<AssemblyInstance> assemblies)
  {
    this._uiDoc = uiDoc;
    this._revitDoc = uiDoc.Document;
    this.Assemblies = assemblies.OrderBy<AssemblyInstance, string>((Func<AssemblyInstance, string>) (a => a.AssemblyTypeName)).ToList<AssemblyInstance>();
    this.assemblyIDX = 0;
    this._username = FlaggerUtils.FormatUsername(this._revitDoc.Application.Username);
    this.InitializeComponent();
    this.SetupWin(this.Assemblies.Count > 1);
  }

  private void SetupWin(bool bMultiple)
  {
    if (bMultiple)
    {
      this.LeftArrowBtn.Visibility = System.Windows.Visibility.Visible;
      this.RightArrowBtn.Visibility = System.Windows.Visibility.Visible;
      this.CheckAllGrid.Visibility = System.Windows.Visibility.Visible;
    }
    else
    {
      this.LeftArrowBtn.Visibility = System.Windows.Visibility.Collapsed;
      this.RightArrowBtn.Visibility = System.Windows.Visibility.Collapsed;
      this.CheckAllGrid.Visibility = System.Windows.Visibility.Collapsed;
    }
    this.UpdateArrows();
    this.UpdateInfoForAssembly();
  }

  private void UpdateArrows()
  {
    if (this.assemblyIDX == 0)
      this.LeftArrowBtn.IsEnabled = false;
    else
      this.LeftArrowBtn.IsEnabled = true;
    if (this.assemblyIDX == this.Assemblies.Count - 1)
      this.RightArrowBtn.IsEnabled = false;
    else
      this.RightArrowBtn.IsEnabled = true;
  }

  private bool UpdateInfoForAssembly()
  {
    this.currInfo.Clear();
    foreach (InfoType infoType in this._typesToGet)
      this.currInfo.Add(infoType, this.GetInfo(infoType));
    this.UpdateUILabels();
    return true;
  }

  private void SetYesNo(TextBlock ynLbl, InfoStruct info)
  {
    ynLbl.FontWeight = FontWeights.Normal;
    ynLbl.Text = "NO";
    ynLbl.Foreground = (Brush) Brushes.Red;
    if (info.yesNo)
    {
      ynLbl.Text = "YES";
      ynLbl.Foreground = (Brush) Brushes.Green;
    }
    else
    {
      if (string.IsNullOrEmpty(info.initialDate) || info.initialDate.Equals("-"))
        return;
      ynLbl.FontWeight = FontWeights.Bold;
      ynLbl.Text = "YES*";
      ynLbl.Foreground = (Brush) Brushes.Goldenrod;
    }
  }

  private void SetRevLbls(bool yesNo, string date)
  {
    TextBlock needsRevYesNo = this.NeedsRevYesNo;
    TextBlock revIssuedDate = this.RevIssuedDate;
    if (!yesNo && date != null && date != "-" && !string.IsNullOrWhiteSpace(date))
    {
      needsRevYesNo.Text = "Last Issued:";
      revIssuedDate.Text = date;
      revIssuedDate.Visibility = System.Windows.Visibility.Visible;
    }
    else if (!yesNo)
    {
      needsRevYesNo.Text = "NO";
      revIssuedDate.Visibility = System.Windows.Visibility.Collapsed;
    }
    else if (yesNo)
    {
      needsRevYesNo.Text = "YES";
      revIssuedDate.Visibility = System.Windows.Visibility.Collapsed;
    }
    needsRevYesNo.Foreground = yesNo ? (Brush) Brushes.Red : (Brush) Brushes.Green;
  }

  private void UpdateUILabels()
  {
    this.TitleLbl.Content = (object) ("Ticket Check Utility: " + this.CurrentAssembly.AssemblyTypeName);
    foreach (InfoType key in this.currInfo.Keys)
    {
      InfoStruct info = this.currInfo[key];
      switch (key)
      {
        case InfoType.EngCheck:
          this.SetYesNo(this.EngYesNo, info);
          this.EngCurrentUser.Content = (object) info.currUser;
          this.EngCurrentDate.Content = (object) info.currDate;
          this.EngInitialUser.Content = (object) info.initialUser;
          this.EngInitialDate.Content = (object) info.initialDate;
          continue;
        case InfoType.DraftCheck:
          this.SetYesNo(this.DraftingYesNo, info);
          this.DraftCurrentUser.Content = (object) info.currUser;
          this.DraftCurrentDate.Content = (object) info.currDate;
          this.DraftInitialUser.Content = (object) info.initialUser;
          this.DraftInitialDate.Content = (object) info.initialDate;
          continue;
        case InfoType.NeedsRev:
          if (!info.yesNo)
            this.IssueRevBtn.IsEnabled = false;
          else
            this.IssueRevBtn.IsEnabled = true;
          this.SetRevLbls(info.yesNo, info.currDate);
          continue;
        default:
          continue;
      }
    }
  }

  private InfoStruct GetInfo(InfoType infoType)
  {
    InfoStruct info = new InfoStruct();
    string str = infoType != InfoType.EngCheck ? "_DRAFTING" : "_ENGINEERING";
    if (infoType == InfoType.EngCheck || infoType == InfoType.DraftCheck)
    {
      info.yesNo = Parameters.GetParameterAsBool((Element) this.CurrentAssembly, $"TICKET_IS{str}_CHECKED");
      info.currUser = Parameters.GetParameterAsString((Element) this.CurrentAssembly, $"TICKET{str}_CHECKED_USER_CURRENT");
      info.initialUser = Parameters.GetParameterAsString((Element) this.CurrentAssembly, $"TICKET{str}_CHECKED_USER_INITIAL");
      info.currDate = Parameters.GetParameterValueStringValue((Element) this.CurrentAssembly, $"TICKET{str}_CHECKED_DATE_CURRENT");
      info.initialDate = Parameters.GetParameterValueStringValue((Element) this.CurrentAssembly, $"TICKET{str}_CHECKED_DATE_INITIAL");
    }
    if (infoType == InfoType.NeedsRev)
    {
      string parameterAsString = Parameters.GetParameterAsString((Element) this.CurrentAssembly, "TICKET_NEEDS_REVISION");
      info.yesNo = parameterAsString.ToUpper().Equals("YES");
      if (!info.yesNo && !parameterAsString.ToUpper().Equals("NO") && !string.IsNullOrWhiteSpace(parameterAsString))
        info.currDate = parameterAsString;
    }
    return info;
  }

  private void Check(AssemblyInstance assembly, InfoType infoType)
  {
    FlaggerUtils.Check(assembly, infoType, this._username);
  }

  private void CheckAll(InfoType infoType)
  {
    FlaggerUtils.CheckAll(this._uiDoc, infoType, this._username, this.Assemblies);
    this.UpdateInfoForAssembly();
  }

  private void DraftChkAllBtn_Click(object sender, RoutedEventArgs e)
  {
    this.Hide();
    this.CheckAll(InfoType.DraftCheck);
    this.ShowDialog();
  }

  private void EngChkAllBtn_Click(object sender, RoutedEventArgs e)
  {
    this.Hide();
    this.CheckAll(InfoType.EngCheck);
    this.ShowDialog();
  }

  private void DraftChkBtn_Click(object sender, RoutedEventArgs e)
  {
    this.Hide();
    this.Check(this.CurrentAssembly, InfoType.DraftCheck);
    this.UpdateInfoForAssembly();
    this.ShowDialog();
  }

  private void EngChkBtn_Click(object sender, RoutedEventArgs e)
  {
    this.Hide();
    this.Check(this.CurrentAssembly, InfoType.EngCheck);
    this.UpdateInfoForAssembly();
    this.ShowDialog();
  }

  private void LeftArrowBtn_Click(object sender, RoutedEventArgs e) => this.arrowClick(false);

  private void RightArrowBtn_Click(object sender, RoutedEventArgs e) => this.arrowClick(true);

  private void arrowClick(bool bNext)
  {
    if (bNext)
      ++this.assemblyIDX;
    else
      --this.assemblyIDX;
    this.UpdateArrows();
    this.UpdateInfoForAssembly();
  }

  private void SetNeedsRevAll(bool yesNo)
  {
    FlaggerUtils.SetNeedsRevAll(this._uiDoc, yesNo, this.Assemblies);
  }

  private void NeedsRevAll_Click(object sender, RoutedEventArgs e)
  {
    this.Hide();
    this.SetNeedsRevAll(true);
    this.UpdateInfoForAssembly();
    this.ShowDialog();
  }

  private void IssueRevAll_Click(object sender, RoutedEventArgs e)
  {
    this.Hide();
    this.SetNeedsRevAll(false);
    this.UpdateInfoForAssembly();
    this.ShowDialog();
  }

  private void NeedsRevBtn_Click(object sender, RoutedEventArgs e)
  {
    this.Hide();
    FlaggerUtils.ToggleNeedsRev(this.CurrentAssembly, new bool?(true));
    this.UpdateInfoForAssembly();
    this.ShowDialog();
  }

  private void IssueRevBtn_Click(object sender, RoutedEventArgs e)
  {
    this.Hide();
    FlaggerUtils.ToggleNeedsRev(this.CurrentAssembly, new bool?(false));
    this.UpdateInfoForAssembly();
    this.ShowDialog();
  }

  private void DoneBtn_Click(object sender, RoutedEventArgs e) => this.Close();

  private void Window_MouseDown(object sender, MouseButtonEventArgs e)
  {
    Window window = sender as Window;
    if (e.ChangedButton != MouseButton.Left)
      return;
    try
    {
      window.DragMove();
    }
    catch (Exception ex)
    {
    }
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/tickettools/ticketmanager/checkflagger/checkwin.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        ((UIElement) target).MouseDown += new MouseButtonEventHandler(this.Window_MouseDown);
        break;
      case 2:
        this.TitleLbl = (Label) target;
        break;
      case 3:
        this.LeftArrowBtn = (Button) target;
        this.LeftArrowBtn.Click += new RoutedEventHandler(this.LeftArrowBtn_Click);
        break;
      case 4:
        this.RightArrowBtn = (Button) target;
        this.RightArrowBtn.Click += new RoutedEventHandler(this.RightArrowBtn_Click);
        break;
      case 5:
        this.CheckAllGrid = (System.Windows.Controls.Grid) target;
        break;
      case 6:
        this.EngChkAllBtn = (Button) target;
        this.EngChkAllBtn.Click += new RoutedEventHandler(this.EngChkAllBtn_Click);
        break;
      case 7:
        this.DraftChkAllBtn = (Button) target;
        this.DraftChkAllBtn.Click += new RoutedEventHandler(this.DraftChkAllBtn_Click);
        break;
      case 8:
        this.NeedsRevAll = (Button) target;
        this.NeedsRevAll.Click += new RoutedEventHandler(this.NeedsRevAll_Click);
        break;
      case 9:
        this.IssueRevAll = (Button) target;
        this.IssueRevAll.Click += new RoutedEventHandler(this.IssueRevAll_Click);
        break;
      case 10:
        this.EngYesNo = (TextBlock) target;
        break;
      case 11:
        this.EngChkBtn = (Button) target;
        this.EngChkBtn.Click += new RoutedEventHandler(this.EngChkBtn_Click);
        break;
      case 12:
        this.EngCurrentUser = (Label) target;
        break;
      case 13:
        this.EngCurrentDate = (Label) target;
        break;
      case 14:
        this.EngInitialUser = (Label) target;
        break;
      case 15:
        this.EngInitialDate = (Label) target;
        break;
      case 16 /*0x10*/:
        this.DraftingYesNo = (TextBlock) target;
        break;
      case 17:
        this.DraftChkBtn = (Button) target;
        this.DraftChkBtn.Click += new RoutedEventHandler(this.DraftChkBtn_Click);
        break;
      case 18:
        this.DraftCurrentUser = (Label) target;
        break;
      case 19:
        this.DraftCurrentDate = (Label) target;
        break;
      case 20:
        this.DraftInitialUser = (Label) target;
        break;
      case 21:
        this.DraftInitialDate = (Label) target;
        break;
      case 22:
        this.NeedsRevYesNo = (TextBlock) target;
        break;
      case 23:
        this.RevIssuedDate = (TextBlock) target;
        break;
      case 24:
        this.NeedsRevBtn = (Button) target;
        this.NeedsRevBtn.Click += new RoutedEventHandler(this.NeedsRevBtn_Click);
        break;
      case 25:
        this.IssueRevBtn = (Button) target;
        this.IssueRevBtn.Click += new RoutedEventHandler(this.IssueRevBtn_Click);
        break;
      case 26:
        this.DoneBtn = (Button) target;
        this.DoneBtn.Click += new RoutedEventHandler(this.DoneBtn_Click);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
