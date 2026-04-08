// Decompiled with JetBrains decompiler
// Type: EDGE.AutoTicketSettingsReader
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Utils.MiscUtils;
using Utils.SettingsUtils;

#nullable disable
namespace EDGE;

public class AutoTicketSettingsReader
{
  public static Document revitDoc;
  public static string manufacturerName = "";
  public static string path = "";
  public const string prefsFileName = "Ticket_Settings.txt";
  public bool bTicketGenerationActive;
  public bool bAutoDimensionActive;
  public bool bAutoDimensionEDrawingActive;

  public static List<AutoTicketSettingsTools> ReaderforAutoTicketSettings(
    Document doc,
    string tool,
    bool bSettingsWindow,
    out List<AutoTicketAppendStringParameterData> appendStringData)
  {
    AutoTicketSettingsReader.revitDoc = doc;
    Parameter parameter = AutoTicketSettingsReader.revitDoc.ProjectInformation.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER");
    if (parameter != null)
      AutoTicketSettingsReader.manufacturerName = parameter.AsString();
    AutoTicketSettingsReader.path = string.IsNullOrEmpty(App.AutoTicketFolderPath) ? $"C:/EDGEforREVIT/{AutoTicketSettingsReader.manufacturerName}_Ticket_Settings.txt" : $"{App.AutoTicketFolderPath}\\{AutoTicketSettingsReader.manufacturerName}_Ticket_Settings.txt";
    try
    {
      if (File.Exists(AutoTicketSettingsReader.path))
      {
        using (StreamReader streamReader = new StreamReader(AutoTicketSettingsReader.path))
        {
          string callouts1 = "";
          string odimensions1 = "";
          string gdimensions1 = "";
          string texts1 = "";
          bool flag1 = true;
          string appendStringV1 = "";
          string sample1 = "";
          string contour = "";
          string overall = "";
          string blockout = "";
          string ft1 = "";
          string inches1 = "";
          string odimensions2 = "";
          string texts2 = "";
          string appendStringV2 = "";
          string sample2 = "";
          string ft2 = "";
          string inches2 = "";
          string odimensions3 = "";
          string texts3 = "";
          string appendStringV3 = "";
          string sample3 = "";
          string ft3 = "";
          string inches3 = "";
          string callouts2 = "";
          string odimensions4 = "";
          string gdimensions2 = "";
          string texts4 = "";
          string ft4 = "";
          string inches4 = "";
          bool flag2 = true;
          string appendStringV4 = "";
          string sample4 = "";
          bool flag3 = false;
          appendStringData = new List<AutoTicketAppendStringParameterData>();
          string str1;
          while ((str1 = streamReader.ReadLine()) != null)
          {
            if (!string.IsNullOrEmpty(str1))
            {
              string[] source = str1.Split(new char[1]
              {
                ','
              }, StringSplitOptions.None);
              string input = source[0];
              Regex regex = new Regex("(?<=\\\").*(?=\\\")");
              foreach (Capture match in regex.Matches(input))
                input = match.Value;
              if (((IEnumerable<string>) source).Count<string>() <= 1)
              {
                if (input.Equals("PARAMETERS"))
                  flag3 = true;
              }
              else if (!flag3)
              {
                if (tool.Equals("AUTO-TICKET GENERATION") && tool.Equals(input))
                {
                  for (int index = 0; index < source.Length; ++index)
                  {
                    if (index != 0)
                    {
                      string str2 = "";
                      foreach (Match match in regex.Matches(source[index]))
                        str2 += match.Value;
                      switch (index)
                      {
                        case 1:
                          callouts1 = str2;
                          continue;
                        case 2:
                          odimensions1 = str2;
                          continue;
                        case 3:
                          gdimensions1 = str2;
                          continue;
                        case 4:
                          texts1 = str2;
                          continue;
                        case 5:
                          appendStringV1 = str2;
                          continue;
                        case 6:
                          sample1 = str2;
                          continue;
                        case 7:
                          string[] strArray = FeetAndInchesRounding.readFeetInchValue(source[7]);
                          ft1 = strArray[0];
                          inches1 = strArray[1];
                          continue;
                        case 8:
                          overall = str2;
                          continue;
                        case 9:
                          contour = str2;
                          continue;
                        case 10:
                          blockout = str2;
                          continue;
                        case 11:
                          if (str2 == "FALSE")
                          {
                            flag1 = false;
                            continue;
                          }
                          continue;
                        default:
                          continue;
                      }
                    }
                  }
                }
                else if (tool.Equals("AUTO-DIMENSION") && tool.Equals(input))
                {
                  for (int index = 0; index < source.Length; ++index)
                  {
                    if (index != 0)
                    {
                      string str3 = "";
                      foreach (Match match in regex.Matches(source[index]))
                        str3 += match.Value;
                      switch (index)
                      {
                        case 1:
                          odimensions2 = str3;
                          continue;
                        case 2:
                          texts2 = str3;
                          continue;
                        case 3:
                          appendStringV2 = str3;
                          continue;
                        case 4:
                          sample2 = str3;
                          continue;
                        case 5:
                          string[] strArray = FeetAndInchesRounding.readFeetInchValue(source[5]);
                          ft2 = strArray[0];
                          inches2 = strArray[1];
                          continue;
                        default:
                          continue;
                      }
                    }
                  }
                }
                else if (tool.Equals("AUTO-DIMENSION E-DRAWING") && tool.Equals(input))
                {
                  for (int index = 0; index < source.Length; ++index)
                  {
                    if (index != 0)
                    {
                      string str4 = "";
                      foreach (Match match in regex.Matches(source[index]))
                        str4 += match.Value;
                      switch (index)
                      {
                        case 1:
                          odimensions3 = str4;
                          continue;
                        case 2:
                          texts3 = str4;
                          continue;
                        case 3:
                          appendStringV3 = str4;
                          continue;
                        case 4:
                          sample3 = str4;
                          continue;
                        default:
                          continue;
                      }
                    }
                  }
                }
                else if (tool.Equals("CLONE TICKET") && tool.Equals(input))
                {
                  for (int index = 0; index < source.Length; ++index)
                  {
                    if (index != 0 && index == 1)
                    {
                      string[] strArray = FeetAndInchesRounding.readFeetInchValue(source[1]);
                      ft3 = strArray[0];
                      inches3 = strArray[1];
                    }
                  }
                }
                else if (tool.Equals("HARDWARE DETAIL") && tool.Equals(input))
                {
                  for (int index = 0; index < source.Length; ++index)
                  {
                    if (index != 0)
                    {
                      string str5 = "";
                      foreach (Match match in regex.Matches(source[index]))
                        str5 += match.Value;
                      switch (index)
                      {
                        case 1:
                          callouts2 = str5;
                          continue;
                        case 2:
                          odimensions4 = str5;
                          continue;
                        case 3:
                          gdimensions2 = str5;
                          continue;
                        case 4:
                          texts4 = str5;
                          continue;
                        case 5:
                          appendStringV4 = str5;
                          continue;
                        case 6:
                          sample4 = str5;
                          continue;
                        case 7:
                          string[] strArray = FeetAndInchesRounding.readFeetInchValue(source[7]);
                          ft4 = strArray[0];
                          inches4 = strArray[1];
                          continue;
                        case 8:
                          if (str5 == "FALSE")
                          {
                            flag2 = false;
                            continue;
                          }
                          continue;
                        default:
                          continue;
                      }
                    }
                  }
                }
              }
              else
              {
                string displayValue = "";
                string valueToLookUp = "";
                string pre1 = "";
                string post1 = "";
                string pre2 = "";
                string post2 = "";
                string pre3 = "";
                string post3 = "";
                string pre4 = "";
                string post4 = "";
                for (int index = 0; index < source.Length; ++index)
                {
                  string str6 = "";
                  if (!string.IsNullOrWhiteSpace(source[index]))
                  {
                    foreach (Match match in regex.Matches(source[index]))
                      str6 += match.Value;
                    switch (index)
                    {
                      case 0:
                        displayValue = str6;
                        continue;
                      case 1:
                        valueToLookUp = str6;
                        continue;
                      case 2:
                        pre1 = str6;
                        continue;
                      case 3:
                        post1 = str6;
                        continue;
                      case 4:
                        pre2 = str6;
                        continue;
                      case 5:
                        post2 = str6;
                        continue;
                      case 6:
                        pre3 = str6;
                        continue;
                      case 7:
                        post3 = str6;
                        continue;
                      case 8:
                        pre4 = str6;
                        continue;
                      case 9:
                        post4 = str6;
                        continue;
                      default:
                        continue;
                    }
                  }
                }
                switch (tool)
                {
                  case "AUTO-TICKET GENERATION":
                    appendStringData.Add(new AutoTicketAppendStringParameterData(pre1, post1, displayValue, valueToLookUp, tool));
                    continue;
                  case "AUTO-DIMENSION":
                    appendStringData.Add(new AutoTicketAppendStringParameterData(pre2, post2, displayValue, valueToLookUp, tool));
                    continue;
                  case "AUTO-DIMENSION E-DRAWING":
                    if (displayValue != "{{LIF}}")
                    {
                      appendStringData.Add(new AutoTicketAppendStringParameterData(pre3, post3, displayValue, valueToLookUp, tool));
                      continue;
                    }
                    continue;
                  case "HARDWARE DETAIL":
                    if (displayValue != "{{LIF}}")
                    {
                      appendStringData.Add(new AutoTicketAppendStringParameterData(pre4, post4, displayValue, valueToLookUp, tool));
                      continue;
                    }
                    continue;
                  default:
                    continue;
                }
              }
            }
          }
          streamReader.Close();
          switch (tool)
          {
            case "AUTO-TICKET GENERATION":
              List<AutoTicketSettingsTools> ticketSettingsToolsList1 = new List<AutoTicketSettingsTools>();
              AutoTicketCalloutAndDimensionTexts andDimensionTexts1 = new AutoTicketCalloutAndDimensionTexts(callouts1, odimensions1, gdimensions1, texts1, new bool?(flag1));
              AutoTicketAppendString ticketAppendString1 = new AutoTicketAppendString(appendStringV1, sample1);
              AutoTicketCustomValues ticketCustomValues = new AutoTicketCustomValues(overall, contour, blockout);
              AutoTicketMinimumDimension minimumDimension1 = new AutoTicketMinimumDimension(ft1, inches1);
              ticketSettingsToolsList1.Add((AutoTicketSettingsTools) andDimensionTexts1);
              ticketSettingsToolsList1.Add((AutoTicketSettingsTools) ticketAppendString1);
              ticketSettingsToolsList1.Add((AutoTicketSettingsTools) ticketCustomValues);
              ticketSettingsToolsList1.Add((AutoTicketSettingsTools) minimumDimension1);
              return ticketSettingsToolsList1;
            case "AUTO-DIMENSION":
              List<AutoTicketSettingsTools> ticketSettingsToolsList2 = new List<AutoTicketSettingsTools>();
              AutoTicketCalloutAndDimensionTexts andDimensionTexts2 = new AutoTicketCalloutAndDimensionTexts("", odimensions2, "", texts2);
              AutoTicketAppendString ticketAppendString2 = new AutoTicketAppendString(appendStringV2, sample2);
              AutoTicketMinimumDimension minimumDimension2 = new AutoTicketMinimumDimension(ft2, inches2);
              ticketSettingsToolsList2.Add((AutoTicketSettingsTools) andDimensionTexts2);
              ticketSettingsToolsList2.Add((AutoTicketSettingsTools) ticketAppendString2);
              ticketSettingsToolsList2.Add((AutoTicketSettingsTools) minimumDimension2);
              return ticketSettingsToolsList2;
            case "AUTO-DIMENSION E-DRAWING":
              List<AutoTicketSettingsTools> ticketSettingsToolsList3 = new List<AutoTicketSettingsTools>();
              AutoTicketCalloutAndDimensionTexts andDimensionTexts3 = new AutoTicketCalloutAndDimensionTexts("", odimensions3, "", texts3);
              AutoTicketAppendString ticketAppendString3 = new AutoTicketAppendString(appendStringV3, sample3);
              ticketSettingsToolsList3.Add((AutoTicketSettingsTools) andDimensionTexts3);
              ticketSettingsToolsList3.Add((AutoTicketSettingsTools) ticketAppendString3);
              return ticketSettingsToolsList3;
            case "CLONE TICKET":
              return new List<AutoTicketSettingsTools>()
              {
                (AutoTicketSettingsTools) new CloneTicketSamePointTolerance(ft3, inches3)
              };
            case "HARDWARE DETAIL":
              List<AutoTicketSettingsTools> ticketSettingsToolsList4 = new List<AutoTicketSettingsTools>();
              AutoTicketCalloutAndDimensionTexts andDimensionTexts4 = new AutoTicketCalloutAndDimensionTexts(callouts2, odimensions4, gdimensions2, texts4, new bool?(flag2));
              AutoTicketAppendString ticketAppendString4 = new AutoTicketAppendString(appendStringV4, sample4);
              AutoTicketMinimumDimension minimumDimension3 = new AutoTicketMinimumDimension(ft4, inches4);
              ticketSettingsToolsList4.Add((AutoTicketSettingsTools) andDimensionTexts4);
              ticketSettingsToolsList4.Add((AutoTicketSettingsTools) ticketAppendString4);
              ticketSettingsToolsList4.Add((AutoTicketSettingsTools) minimumDimension3);
              return ticketSettingsToolsList4;
            default:
              return new List<AutoTicketSettingsTools>();
          }
        }
      }
      else
      {
        appendStringData = AutoTicketSettingsReader.returnDefaultParameters();
        List<AutoTicketSettingsTools> ticketSettingsToolsList = new List<AutoTicketSettingsTools>();
        switch (tool)
        {
          case "AUTO-TICKET GENERATION":
            List<AutoTicketSettingsTools> collection = AutoTicketSettingsReader.defaultReturnValue(tool, bSettingsWindow);
            ticketSettingsToolsList.AddRange((IEnumerable<AutoTicketSettingsTools>) collection);
            break;
          case "AUTO-DIMENSION":
            AutoTicketAppendString ticketAppendString5 = new AutoTicketAppendString("{{QTY}} {{DESC}} {{LIF}}", "(QTY) DESC (LIF)");
            ticketSettingsToolsList.Add((AutoTicketSettingsTools) ticketAppendString5);
            break;
          case "HARDWARE DETAIL":
            AutoTicketAppendString ticketAppendString6 = new AutoTicketAppendString("{{QTY}} {{DESC}}", "(QTY) DESC");
            ticketSettingsToolsList.Add((AutoTicketSettingsTools) ticketAppendString6);
            break;
          default:
            AutoTicketAppendString ticketAppendString7 = new AutoTicketAppendString("{{QTY}} {{DESC}}", "(QTY) DESC");
            ticketSettingsToolsList.Add((AutoTicketSettingsTools) ticketAppendString7);
            break;
        }
        return ticketSettingsToolsList;
      }
    }
    catch (IOException ex)
    {
      if (ex.Message.Contains("process"))
        new TaskDialog("EDGE^R")
        {
          AllowCancellation = false,
          MainInstruction = "Auto Ticket and Dimension Settings File Error.",
          MainContent = (string.IsNullOrEmpty(AutoTicketSettingsReader.manufacturerName) ? "Check Auto Ticket and Dimension Settings File: Auto_Ticket_Settings.txt. Please ensure the file is not in use by another application and try again." : $"Check Auto Ticket and Dimension Settings File: {AutoTicketSettingsReader.manufacturerName}_Auto_Ticket_Settings.txt. Please ensure the file is not in use by another application and try again.")
        }.Show();
      appendStringData = AutoTicketSettingsReader.returnDefaultParameters();
      return AutoTicketSettingsReader.defaultReturnValue(tool, bSettingsWindow);
    }
  }

  private static List<AutoTicketSettingsTools> defaultReturnValue(string tool, bool bSettingsWindow)
  {
    switch (tool)
    {
      case "AUTO-TICKET GENERATION":
        List<AutoTicketSettingsTools> ticketSettingsToolsList1 = new List<AutoTicketSettingsTools>();
        ticketSettingsToolsList1.Add((AutoTicketSettingsTools) new AutoTicketCalloutAndDimensionTexts("AUTO_TICKET_CALLOUT", "PTAC - TICKET (GAP TO ELEMENT)", "PTAC - TICKET (FIXED TO DIM. LINE)", "PTAC - TICKET TEXT"));
        AutoTicketAppendString ticketAppendString1 = new AutoTicketAppendString("{{QTY}} {{DESC}} {{LIF}}", "(QTY) DESC (LIF)");
        AutoTicketCustomValues ticketCustomValues = new AutoTicketCustomValues("OVERALL", "CONTOUR", "BLOCKOUT");
        ticketSettingsToolsList1.Add((AutoTicketSettingsTools) ticketAppendString1);
        ticketSettingsToolsList1.Add((AutoTicketSettingsTools) ticketCustomValues);
        return ticketSettingsToolsList1;
      case "AUTO-DIMENSION":
        List<AutoTicketSettingsTools> ticketSettingsToolsList2 = new List<AutoTicketSettingsTools>();
        ticketSettingsToolsList2.Add((AutoTicketSettingsTools) new AutoTicketCalloutAndDimensionTexts("", "PTAC - TICKET (GAP TO ELEMENT)", "", "PTAC - TICKET TEXT"));
        if (bSettingsWindow)
        {
          AutoTicketAppendString ticketAppendString2 = new AutoTicketAppendString("{{QTY}} {{DESC}} {{LIF}}", "(QTY) DESC (LIF)");
          ticketSettingsToolsList2.Add((AutoTicketSettingsTools) ticketAppendString2);
        }
        return ticketSettingsToolsList2;
      case "AUTO-DIMENSION E-DRAWING":
        List<AutoTicketSettingsTools> ticketSettingsToolsList3 = new List<AutoTicketSettingsTools>();
        ticketSettingsToolsList3.Add((AutoTicketSettingsTools) new AutoTicketCalloutAndDimensionTexts("", "PTAC - TICKET (GAP TO ELEMENT)", "", "PTAC - TICKET TEXT"));
        if (bSettingsWindow)
        {
          AutoTicketAppendString ticketAppendString3 = new AutoTicketAppendString("{{QTY}} {{DESC}}", "(QTY) DESC");
          ticketSettingsToolsList3.Add((AutoTicketSettingsTools) ticketAppendString3);
        }
        return ticketSettingsToolsList3;
      case "HARDWAE DETAIL":
        List<AutoTicketSettingsTools> ticketSettingsToolsList4 = new List<AutoTicketSettingsTools>();
        ticketSettingsToolsList4.Add((AutoTicketSettingsTools) new AutoTicketCalloutAndDimensionTexts("", "PTAC - TICKET (GAP TO ELEMENT)", "PTAC - TICKET (FIXED TO DIM. LINE)", "PTAC - TICKET TEXT"));
        if (bSettingsWindow)
        {
          AutoTicketAppendString ticketAppendString4 = new AutoTicketAppendString("{{QTY}} {{DESC}}", "(QTY) DESC");
          ticketSettingsToolsList4.Add((AutoTicketSettingsTools) ticketAppendString4);
        }
        return ticketSettingsToolsList4;
      default:
        return new List<AutoTicketSettingsTools>();
    }
  }

  public static void SaveFile(List<string> outputValue)
  {
    try
    {
      DirectoryInfo directoryInfo = new DirectoryInfo(Path.GetDirectoryName(AutoTicketSettingsReader.path));
      if (!directoryInfo.Exists)
        directoryInfo.Create();
      FileInfo fileInfo = new FileInfo(AutoTicketSettingsReader.path);
      if (!fileInfo.Exists)
        fileInfo.Create()?.Close();
      using (StreamWriter streamWriter = new StreamWriter(AutoTicketSettingsReader.path, false))
      {
        foreach (string str in outputValue)
          streamWriter.WriteLine(str);
      }
      int num = (int) MessageBox.Show("File saved successfully.");
    }
    catch (Exception ex)
    {
      if (ex.Message.Contains("process"))
        new TaskDialog("EDGE^R")
        {
          AllowCancellation = false,
          MainInstruction = "Auto Ticket and Dimension Settings File Error.",
          MainContent = (string.IsNullOrEmpty(AutoTicketSettingsReader.manufacturerName) ? "Check Auto Ticket and Dimension Settings File: Ticket_Settings.txt. Please ensure the file is not in use by another application and try again." : $"Check Auto Ticket and Dimension Settings File: {AutoTicketSettingsReader.manufacturerName}_Ticket_Settings.txt. Please ensure the file is not in use by another application and try again.")
        }.Show();
      if (!ex.Message.Contains("Access") || !ex.Message.Contains("denied"))
        return;
      int num = (int) MessageBox.Show($"Please check folder attributes. {AutoTicketSettingsReader.manufacturerName}_Ticket_Settings.txt is set to Read Only mode.", "Folder Access Warning", MessageBoxButton.OK, MessageBoxImage.None);
    }
  }

  public static List<AutoTicketAppendStringParameterData> returnDefaultParameters()
  {
    return new List<AutoTicketAppendStringParameterData>()
    {
      new AutoTicketAppendStringParameterData("(", ")", "{{QTY}}", "QUANTITY", "AUTO-TICKET GENERATION"),
      new AutoTicketAppendStringParameterData("", "", "{{DESC}}", "IDENTITY_DESCRIPTION_SHORT", "AUTO-TICKET GENERATION"),
      new AutoTicketAppendStringParameterData("(", ")", "{{LIF}}", "LOCATION_IN_FORM", "AUTO-TICKET GENERATION"),
      new AutoTicketAppendStringParameterData("(", ")", "{{QTY}}", "QUANTITY", "AUTO-DIMENSION"),
      new AutoTicketAppendStringParameterData("", "", "{{DESC}}", "IDENTITY_DESCRIPTION_SHORT", "AUTO-DIMENSION"),
      new AutoTicketAppendStringParameterData("(", ")", "{{LIF}}", "LOCATION_IN_FORM", "AUTO-DIMENSION"),
      new AutoTicketAppendStringParameterData("(", ")", "{{QTY}}", "QUANTITY", "AUTO-DIMENSION E-DRAWING"),
      new AutoTicketAppendStringParameterData("", "", "{{DESC}}", "IDENTITY_DESCRIPTION_SHORT", "AUTO-DIMENSION E-DRAWING"),
      new AutoTicketAppendStringParameterData("(", ")", "{{QTY}}", "QUANTITY", "HARDWARE DETAIL"),
      new AutoTicketAppendStringParameterData("", "", "{{DESC}}", "IDENTITY_DESCRIPTION_SHORT", "HARDWARE DETAIL")
    };
  }
}
