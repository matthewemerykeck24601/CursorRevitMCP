// Decompiled with JetBrains decompiler
// Type: EDGE.QA
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.__Testing;
using System;
using System.Text;
using System.Windows.Forms;
using Utils;

#nullable disable
namespace EDGE;

internal class QA
{
  private static string path = "";
  public static bool inHouse = false;
  public static bool inHouseShowDebugMessages = false;
  private static bool _bShowRebarDebugging = false;
  private static bool _bAskToShowRebarDebugging = false;
  private static bool _bShowViewSectionRotationDebugInfo = false;
  private static bool _bTraceRebarLists = false;
  private static bool _bLogTrace = false;
  private static bool _bLogInHouse = false;
  private static bool _bRebarWorkSharingAutomation = false;
  private static bool _bLogAutoDimensionTrace = false;
  public static bool _bDrawAutoWarpDebuggingModelLines = false;
  private static bool _showRebarControlCompare = false;
  private static bool _bVerboseReloadLatestCatalogging = false;
  private static bool _bVerboseRebarLogging = false;
  private static bool _bVerboseRebarSWCLogging = false;
  private static bool _bShowCloudDebugging = false;
  private static bool _bShortCircuitCloudPush = false;
  private static bool _bDrawVisualizationForMarkVerification = false;
  private static bool _bPickMarkVerificationFaces = false;
  internal static bool ShowSendToCloudOnAdmin = false;

  internal static void LogDemarcation(string v)
  {
    if (!QA.inHouse)
      return;
    string shortTimeString = DateTime.Now.ToShortTimeString();
    string str = $"{v}: {shortTimeString}";
    QA.Trace("**************************************************************************************************************************************");
    QA.Trace("**************************************************************************************************************************************");
    QA.Trace("**************************************************************************************************************************************");
    QA.Trace("*                                                                                                                                    *");
    QA.Trace($"*                                    {str}{new string(' ', 95 - str.Length)}*");
    QA.Trace("*                                                                                                                                    *");
    QA.Trace("**************************************************************************************************************************************");
    QA.Trace("**************************************************************************************************************************************");
    QA.Trace("**************************************************************************************************************************************");
  }

  public static bool RebarMarkAutomation_Worksharing_Enabled
  {
    get
    {
      if (!QA.inHouse)
        return App.RebarMarkAutomation_WorksharingSupport;
      return QA.inHouse && QA._bRebarWorkSharingAutomation;
    }
  }

  public static bool Worksharing_VerboseCatalogging
  {
    get => QA.inHouse && QA._bVerboseReloadLatestCatalogging;
  }

  public static bool SpecialPermission => QA.inHouse;

  public static bool LogVerboseRebarInfo => QA.inHouse && QA._bVerboseRebarLogging;

  public static bool CloudDebuggingInfo => QA.inHouse && QA._bShowCloudDebugging;

  public static bool UseInHouseCloudSettings => QA.inHouse && QA._bShortCircuitCloudPush;

  public static bool DrawVisualizationForMarkVerification
  {
    get => QA.inHouse && QA._bDrawVisualizationForMarkVerification;
  }

  public static bool LogAutodimTrace => QA.inHouse && QA._bLogAutoDimensionTrace;

  public static bool SelectCompareFaces => QA.inHouse && QA._bPickMarkVerificationFaces;

  public static bool DrawAutoWarpModelLines => QA.inHouse && QA._bDrawAutoWarpDebuggingModelLines;

  public static bool PushToDemoSiteAnyway { get; set; }

  public static void SetVerboseRebarLogging(bool setting) => QA._bVerboseRebarLogging = setting;

  public static void SetTraceLogging(bool setting) => QA._bLogTrace = setting;

  public static void Trace(string message)
  {
    if (!QA._bLogTrace)
      return;
    QA.LogLine(message);
  }

  public static void DebugMessage(string message)
  {
    if (!QA.inHouse)
      return;
    int num = (int) MessageBox.Show(message);
  }

  public static void InHouseMessage(string message)
  {
    if (QA.inHouse && QA.inHouseShowDebugMessages)
    {
      int num = (int) MessageBox.Show(message);
    }
    if (!QA.inHouse || !QA._bLogInHouse)
      return;
    QA.LogLine(message);
  }

  public static void ToggleRebarDebug()
  {
    if (!QA.inHouse || !QA._bAskToShowRebarDebugging)
      return;
    QA._bShowRebarDebugging = MessageBox.Show("Turn on Rebar Debugging? ", "debug", MessageBoxButtons.YesNo) == DialogResult.Yes;
  }

  public static void DisplayRebarDebuggingInfo(string text)
  {
    if (QA.inHouse && QA._bShowRebarDebugging)
    {
      QA.DisplayTextInfo(text);
    }
    else
    {
      if (!QA._bTraceRebarLists || !QA._bLogTrace)
        return;
      QA.LogLine(text);
    }
  }

  public static void DisplayViewSectionRotationInfo(string text)
  {
    if (!QA.inHouse || !QA._bShowViewSectionRotationDebugInfo)
      return;
    QA.DisplayTextInfo(text);
  }

  public static void LogTaskDialog(TaskDialog td)
  {
    if (!QA.inHouse)
      return;
    QA.LogLine("------------------------------------------------------------");
    QA.LogLine("*                 Edge Dialog                              *");
    QA.LogLine("------------------------------------------------------------");
    QA.LogLine("* " + td.Title);
    QA.LogLine("* " + td.MainInstruction);
    QA.LogLine("* " + td.MainContent);
    QA.LogLine("* " + td.ExpandedContent);
    QA.LogLine("* " + td.FooterText);
    QA.LogLine("------------------------------------------------------------");
  }

  internal static void DisplayTextInfo(string text)
  {
    if (!QA.inHouse)
      return;
    int num = (int) new DisplayDebugInfoForm()
    {
      displayText = text
    }.ShowDialog();
  }

  public static string WriteBoundingBox(BoundingBoxXYZ bbox)
  {
    StringBuilder stringBuilder = new StringBuilder();
    stringBuilder.AppendLine("bbox Max: " + bbox.Max.ToString());
    stringBuilder.AppendLine("bbox Min: " + bbox.Min.ToString());
    stringBuilder.AppendLine("Transform:");
    stringBuilder.AppendLine("     Origin: " + bbox.Transform.Origin.ToString());
    stringBuilder.AppendLine("     BasisX: " + bbox.Transform.BasisX.ToString());
    stringBuilder.AppendLine("     BasisY: " + bbox.Transform.BasisY.ToString());
    stringBuilder.AppendLine("     BasisZ: " + bbox.Transform.BasisZ.ToString());
    return stringBuilder.ToString();
  }

  public static string WriteTransform(Transform trans)
  {
    StringBuilder stringBuilder = new StringBuilder();
    stringBuilder.AppendLine("Transform:");
    stringBuilder.AppendLine("     Origin: " + trans.Origin.ToString());
    stringBuilder.AppendLine("     BasisX: " + trans.BasisX.ToString());
    stringBuilder.AppendLine("     BasisY: " + trans.BasisY.ToString());
    stringBuilder.AppendLine("     BasisZ: " + trans.BasisZ.ToString());
    return stringBuilder.ToString();
  }

  internal static void InitLog(string journalPath) => QAUtils.InitLog(journalPath);

  public static void LogLine(string lineToLog)
  {
    if (!QA.inHouse)
      return;
    QAUtils.LogLine(lineToLog);
  }

  public static void LogError(string functionOrFile, string lineToLog)
  {
    QAUtils.LogError(functionOrFile, lineToLog);
  }

  public static void LogLine(Document revitDoc, string LineToLog)
  {
    if (!QA.inHouse)
      return;
    StringBuilder stringBuilder = new StringBuilder();
    stringBuilder.AppendLine(revitDoc.PathName);
    stringBuilder.Append(LineToLog);
    QA.LogLine(stringBuilder.ToString());
  }

  public static void LogVerboseRebarTrace(string LineToLog)
  {
    if (!QA._bVerboseRebarLogging)
      return;
    QA.LogLine(LineToLog);
  }

  public static void LogMinimalRebarTrace(string LineToLog) => QA.LogLine(LineToLog);

  public static void LogVerboseRebarSWCTrace(string LineToLog)
  {
    if (!QA._bVerboseRebarSWCLogging)
      return;
    QA.LogLine(LineToLog);
  }
}
