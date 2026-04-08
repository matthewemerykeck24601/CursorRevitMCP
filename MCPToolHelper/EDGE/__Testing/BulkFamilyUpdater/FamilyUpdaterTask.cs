// Decompiled with JetBrains decompiler
// Type: EDGE.__Testing.BulkFamilyUpdater.FamilyUpdaterTask
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Utils.MiscUtils;

#nullable disable
namespace EDGE.__Testing.BulkFamilyUpdater;

public class FamilyUpdaterTask
{
  public string oldParameterName;
  public string oldValue;
  public bool? isValueFormula;
  public bool? clearValue;
  public string oldParamGroup;
  public string oldIsTypeParameter;
  public string FamilyType;
  public BulkFamilyUpdatersUtils.messageType messType = BulkFamilyUpdatersUtils.messageType.success;
  public string outputMessage;

  public BulkFamilyUpdatersUtils.taskActions action { get; set; }

  public string parameterName { get; set; }

  public string isSharedParameter { get; set; }

  public string value { get; set; }

  public string discipline { get; set; }

  public string specType { get; set; }

  public string paramGroup { get; set; }

  public string isTypeParameter { get; set; }

  public bool canEdit { get; set; }

  public FamilyUpdaterTask()
  {
  }

  public FamilyUpdaterTask(BulkFamilyUpdatersUtils.taskActions actionArgument)
  {
    this.action = actionArgument;
    this.value = "";
    if (this.action == BulkFamilyUpdatersUtils.taskActions.Delete)
      this.canEdit = false;
    else
      this.canEdit = true;
  }

  public FamilyUpdaterTask(FamilyUpdaterTask original)
  {
    this.action = original.action;
    this.parameterName = original.parameterName;
    this.oldParameterName = original.oldParameterName;
    this.isSharedParameter = original.isSharedParameter;
    this.value = original.value;
    this.oldValue = original.oldValue;
    this.isValueFormula = original.isValueFormula;
    this.clearValue = original.clearValue;
    this.discipline = original.discipline;
    this.specType = original.specType;
    this.paramGroup = original.paramGroup;
    this.oldParamGroup = original.oldParamGroup;
    this.isTypeParameter = original.isTypeParameter;
    this.oldIsTypeParameter = original.oldIsTypeParameter;
    this.FamilyType = original.FamilyType;
    this.canEdit = original.canEdit;
    this.messType = original.messType;
    this.outputMessage = original.outputMessage;
  }

  public void addOutputMessage(
    BulkFamilyUpdatersUtils.messageType messageProperty,
    string output = "")
  {
    this.messType = messageProperty;
    if (messageProperty == BulkFamilyUpdatersUtils.messageType.success)
    {
      if (output == "")
        this.outputMessage = "Task Executed Successfully";
      else
        this.outputMessage = output;
    }
    else
      this.outputMessage = output;
  }
}
