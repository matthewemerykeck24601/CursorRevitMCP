// Decompiled with JetBrains decompiler
// Type: EDGE.QATools.RegTestGen_Command.Strings
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System.IO;

#nullable disable
namespace EDGE.QATools.RegTestGen_Command;

public class Strings
{
  public const string RegTestsFileName = "RegTests.txt";
  public const string BuildsToTestFolderPath = "_BuildsToTest";
  public const string TestsDirectoryName = "Tests";
  public const string RegTestGenCommand = "EDGE.QATools.RegTestGen_Command.RegTestGen_Command";
  public const string QuitRevitString = " Jrn.Command \"SystemMenu\" , \"Quit the application; prompts to save projects, ID_APP_EXIT\"\nJrn.Data \"TaskDialogResult\"  _\n, \"Do you want to save changes to TestRegressionCoverage.rvt?\",  _\n\"No\", \"IDNO\"";
  public const string StartPageFileClick = "Jrn.Data \"MRUFileName\"";
  public const string OpenFileBrowserSearchString = "Jrn.Data \"File Name\"";

  public static string GetRegTestsFilePath(string mainRegTestsFolderPath)
  {
    return mainRegTestsFolderPath + "\\RegTests.txt";
  }

  public static string GetBuildsToTestFolderPath(string mainRegTestsFolderPath)
  {
    return mainRegTestsFolderPath + "\\_BuildsToTest";
  }

  internal static string GetRegTestsFolderPath(string mainRegTestsFolderPath)
  {
    return mainRegTestsFolderPath + "\\Tests";
  }

  internal static string ResolveFilePath(string pathFromJournalFile, string recordingJournalPath)
  {
    switch (pathFromJournalFile.Substring(0, 1))
    {
      case ".":
        FileInfo fileInfo1 = new FileInfo(recordingJournalPath);
        if (!fileInfo1.Exists)
          return "Failed To Resolve Path";
        FileInfo fileInfo2 = new FileInfo(Path.Combine(fileInfo1.Directory.FullName, pathFromJournalFile));
        return fileInfo2.Exists ? fileInfo2.FullName : "failed to resolve Path";
      case "$":
        string str = "";
        string[] strArray = pathFromJournalFile.Split('\\');
        if (strArray.Length == 0)
          return "";
        for (int index = 1; index < strArray.Length; ++index)
          str = $"{str}\\{strArray[index]}";
        int num = strArray[0] == "$AllUsersAppData" ? 1 : 0;
        return str;
      default:
        return "";
    }
  }
}
