// Decompiled with JetBrains decompiler
// Type: EDGE.IUpdaters.ModelLocking.ModelLockingUtils
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

#nullable disable
namespace EDGE.IUpdaters.ModelLocking;

public class ModelLockingUtils
{
  public static List<string> GetFriendlyNamesForPermissions(ModelPermissionsCategory permissions)
  {
    List<string> namesForPermissions = new List<string>();
    if ((permissions & ModelPermissionsCategory.Geometry) != ModelPermissionsCategory.None)
      namesForPermissions.Add("Geometry");
    if ((permissions & ModelPermissionsCategory.ConnectionsHardware) != ModelPermissionsCategory.None)
      namesForPermissions.Add("Connections");
    if ((permissions & ModelPermissionsCategory.RebarHandling) != ModelPermissionsCategory.None)
      namesForPermissions.Add("Rebar");
    if ((permissions & ModelPermissionsCategory.Assemblies) != ModelPermissionsCategory.None)
      namesForPermissions.Add("Assemblies");
    if ((permissions & ModelPermissionsCategory.Admin) != ModelPermissionsCategory.None)
      namesForPermissions.Add("Admin");
    return namesForPermissions;
  }

  public static bool ModelLockingEnabled(Document revitDoc)
  {
    if (revitDoc.IsFamilyDocument || !App.ModelLocking)
      return false;
    Parameter parameter = revitDoc.ProjectInformation.LookupParameter("MODEL_LOCKING_ENABLED");
    return parameter != null && parameter.HasValue && parameter.AsInteger() == 1;
  }

  public static bool ShowPermissionsDialog(
    Document revitDoc,
    ModelPermissionsCategory requestedCategories,
    string uname_optional = "")
  {
    if (!ModelLockingUtils.ModelLockingEnabled(revitDoc))
      return true;
    string strUserName = !string.IsNullOrEmpty(uname_optional) ? uname_optional : revitDoc.Application.Username;
    ModelPermissionsCategory permissionsForUser = ModelLockingPermissionsSchema.GetModelPermissionsForUser(strUserName, revitDoc.ProjectInformation);
    TaskDialog taskDialog = new TaskDialog("Model Locking");
    ModelPermissionsCategory permissionsCategory = permissionsForUser & requestedCategories;
    if (permissionsCategory == requestedCategories)
      return true;
    string str1;
    if (permissionsCategory > ModelPermissionsCategory.None)
    {
      str1 = "This feature requested one or more model locking permissions that you do not have. Choosing to proceed may cause unexpected results. Do you want to continue?";
      taskDialog.CommonButtons = (TaskDialogCommonButtons) 6;
    }
    else
    {
      str1 = "You have none of the model locking permissions that this tool requested. Please contact your administrator for more information.";
      taskDialog.CommonButtons = (TaskDialogCommonButtons) 1;
    }
    string str2 = "Requested Permissions:" + Environment.NewLine;
    foreach (string namesForPermission in ModelLockingUtils.GetFriendlyNamesForPermissions(requestedCategories))
      str2 = str2 + namesForPermission + Environment.NewLine;
    string str3;
    if (permissionsForUser == ModelPermissionsCategory.None || permissionsForUser == ModelPermissionsCategory.NonTracked)
    {
      str3 = $"{str2}{Environment.NewLine}User {strUserName.ToUpper()} has no Model Locking permissions assigned.";
    }
    else
    {
      str3 = $"{str2}{Environment.NewLine}Permissions for {strUserName.ToUpper()}:{Environment.NewLine}";
      foreach (string namesForPermission in ModelLockingUtils.GetFriendlyNamesForPermissions(permissionsForUser))
        str3 = str3 + namesForPermission + Environment.NewLine;
    }
    taskDialog.MainContent = str1;
    taskDialog.ExpandedContent = str3;
    return taskDialog.Show() == 6;
  }
}
