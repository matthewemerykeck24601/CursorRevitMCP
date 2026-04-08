// Decompiled with JetBrains decompiler
// Type: EDGE.AdminTools.WarningAnalyzer.Views.ResultsWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;

#nullable disable
namespace EDGE.AdminTools.WarningAnalyzer.Views;

public class ResultsWindow : Window, IComponentConnector
{
  internal TextBlock textBlock;
  private bool _contentLoaded;

  public ResultsWindow() => this.InitializeComponent();

  public ResultsWindow(int joinCount, int cutCount)
  {
    this.InitializeComponent();
    StringBuilder stringBuilder = new StringBuilder();
    bool flag1 = cutCount > 0;
    bool flag2 = joinCount > 0;
    bool flag3 = cutCount > 1;
    bool flag4 = joinCount > 1;
    if (!flag1 && !flag2)
    {
      stringBuilder.Append("There were no warnings of the specified type(s) to resolve.");
    }
    else
    {
      if (flag1 & flag2)
        stringBuilder.Append("There have been");
      else if (flag2)
      {
        if (flag4)
          stringBuilder.Append("There have been");
        else
          stringBuilder.Append("There has been");
      }
      else if (flag1)
      {
        if (flag3)
          stringBuilder.Append("There have been");
        else
          stringBuilder.Append("There has been");
      }
      if (flag2)
      {
        if (flag3)
          stringBuilder.Append($" {joinCount.ToString()} Join warnings");
        else
          stringBuilder.Append($" {joinCount.ToString()} Join warning");
      }
      if (flag2 & flag1)
        stringBuilder.Append(" and ");
      if (flag1)
      {
        if (flag3)
          stringBuilder.Append($" {cutCount.ToString()} Cut warnings");
        else
          stringBuilder.Append($" {cutCount.ToString()} Cut warning");
      }
      stringBuilder.Append(" resolved.");
    }
    this.textBlock.Text = stringBuilder.ToString();
  }

  private void Button_Click(object sender, RoutedEventArgs e) => this.Close();

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/admintools/warninganalyzer/views/resultswindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    if (connectionId != 1)
    {
      if (connectionId == 2)
        this.textBlock = (TextBlock) target;
      else
        this._contentLoaded = true;
    }
    else
      ((ButtonBase) target).Click += new RoutedEventHandler(this.Button_Click);
  }
}
