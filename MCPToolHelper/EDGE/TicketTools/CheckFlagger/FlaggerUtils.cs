// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.CheckFlagger.FlaggerUtils
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.ElementUtils;
using Utils.WorkSharingUtils;

#nullable disable
namespace EDGE.TicketTools.CheckFlagger;

public class FlaggerUtils
{
  public const string _format = "yyyy-MM-dd-HH-mm-ss";
  public const string _friendlyformat = "yyyy-MM-dd";
  public const string _titleStr = "Ticket Check Utility: ";
  public const string _yesStr = "YES";
  public const string _noStr = "NO";
  public const string _ticketStr = "TICKET";
  public const string _isStr = "_IS";
  public const string _engStr = "_ENGINEERING";
  public const string _draftStr = "_DRAFTING";
  public const string _checkedStr = "_CHECKED";
  public const string _dateStr = "_DATE";
  public const string _userStr = "_USER";
  public const string _initialStr = "_INITIAL";
  public const string _currentStr = "_CURRENT";
  public const string _needsRevStr = "_NEEDS_REVISION";

  public static string FormatUsername(string s)
  {
    string str = s;
    if (s.Contains("@"))
      str = s.Substring(0, s.IndexOf("@"));
    return str;
  }

  public static bool Check(AssemblyInstance assembly, InfoType infoType, string username)
  {
    Document document = assembly.Document;
    string str1 = infoType != InfoType.EngCheck ? "_DRAFTING" : "_ENGINEERING";
    string paramName1 = $"TICKET_IS{str1}_CHECKED";
    string paramName2 = $"TICKET{str1}_CHECKED_USER_CURRENT";
    string paramName3 = $"TICKET{str1}_CHECKED_USER_INITIAL";
    string paramName4 = $"TICKET{str1}_CHECKED_DATE_CURRENT";
    string paramName5 = $"TICKET{str1}_CHECKED_DATE_INITIAL";
    string str2 = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
    string name = $"{assembly.AssemblyTypeName}- Check Ticket{str1}";
    using (Transaction transaction = new Transaction(document, name))
    {
      int num1 = (int) transaction.Start();
      Parameters.LookupParameter((Element) assembly, paramName1)?.Set(1);
      Parameters.LookupParameter((Element) assembly, paramName2)?.Set(username);
      Parameters.LookupParameter((Element) assembly, paramName4)?.Set(str2);
      if (string.IsNullOrWhiteSpace(Parameters.GetParameterAsString((Element) assembly, paramName3)))
        Parameters.LookupParameter((Element) assembly, paramName3)?.Set(username);
      if (string.IsNullOrWhiteSpace(Parameters.GetParameterAsString((Element) assembly, paramName5)))
        Parameters.LookupParameter((Element) assembly, paramName5)?.Set(str2);
      int num2 = (int) transaction.Commit();
    }
    return true;
  }

  public static bool CheckAll(
    UIDocument uiDoc,
    InfoType infoType,
    string username,
    List<AssemblyInstance> assemblies)
  {
    Document document = uiDoc.Document;
    if (document.IsWorkshared)
    {
      try
      {
        if (!CheckElementsOwnership.CheckOwnership("Check Flag Utility", assemblies.Select<AssemblyInstance, ElementId>((Func<AssemblyInstance, ElementId>) (e => e.Id)).ToList<ElementId>(), document, uiDoc, out List<ElementId> _))
          return false;
      }
      catch (Exception ex)
      {
        return false;
      }
    }
    string str = infoType != InfoType.EngCheck ? "_DRAFTING" : "_ENGINEERING";
    using (TransactionGroup transactionGroup = new TransactionGroup(document, "Check All" + str))
    {
      int num1 = (int) transactionGroup.Start();
      foreach (AssemblyInstance assembly in assemblies)
        FlaggerUtils.Check(assembly, infoType, username);
      int num2 = (int) transactionGroup.Assimilate();
    }
    return true;
  }

  public static bool ToggleNeedsRev(AssemblyInstance assembly, bool? bYesNo = null)
  {
    bool flag1 = false;
    Document document = assembly.Document;
    string paramName = "TICKET_NEEDS_REVISION";
    string upper = Parameters.GetParameterAsString((Element) assembly, paramName).ToUpper();
    bool flag2 = bYesNo.HasValue ? bYesNo.GetValueOrDefault() : !upper.Equals("YES");
    string name = assembly.AssemblyTypeName + (flag2 ? "- Set Needs Revision " : "- Issue Revision");
    using (Transaction transaction = new Transaction(document, name))
    {
      int num1 = (int) transaction.Start();
      Parameter parameter = Parameters.LookupParameter((Element) assembly, paramName);
      if (parameter != null)
      {
        if (!flag2 && upper.Equals("YES"))
        {
          parameter.Set(DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
          flag1 = true;
        }
        else if (flag2)
        {
          parameter.Set("YES");
          flag1 = true;
        }
      }
      int num2 = (int) transaction.Commit();
    }
    return flag1;
  }

  public static bool SetNeedsRevAll(
    UIDocument uiDoc,
    bool bYesNo,
    List<AssemblyInstance> assemblies)
  {
    return FlaggerUtils.SetNeedsRevAll(uiDoc, bYesNo, assemblies, out List<AssemblyInstance> _);
  }

  public static bool SetNeedsRevAll(
    UIDocument uiDoc,
    bool bYesNo,
    List<AssemblyInstance> assemblies,
    out List<AssemblyInstance> affected)
  {
    Document document = uiDoc.Document;
    affected = new List<AssemblyInstance>();
    if (document.IsWorkshared)
    {
      try
      {
        if (!CheckElementsOwnership.CheckOwnership("Check Flag Utility", assemblies.Select<AssemblyInstance, ElementId>((Func<AssemblyInstance, ElementId>) (e => e.Id)).ToList<ElementId>(), document, uiDoc, out List<ElementId> _))
          return false;
      }
      catch (Exception ex)
      {
        return false;
      }
    }
    using (TransactionGroup transactionGroup = new TransactionGroup(document, (bYesNo ? "Set Needs Revision " : "Issue Revision") + " for all"))
    {
      int num1 = (int) transactionGroup.Start();
      foreach (AssemblyInstance assembly in assemblies)
      {
        if (FlaggerUtils.ToggleNeedsRev(assembly, new bool?(bYesNo)))
          affected.Add(assembly);
      }
      int num2 = (int) transactionGroup.Assimilate();
    }
    return true;
  }
}
