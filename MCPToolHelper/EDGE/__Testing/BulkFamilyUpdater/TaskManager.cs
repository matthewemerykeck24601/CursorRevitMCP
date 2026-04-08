// Decompiled with JetBrains decompiler
// Type: EDGE.__Testing.BulkFamilyUpdater.TaskManager
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Utils.ElementUtils;
using Utils.ExcelUtils;
using Utils.IEnumerable_Extensions;
using Utils.MiscUtils;

#nullable disable
namespace EDGE.__Testing.BulkFamilyUpdater;

public class TaskManager
{
  public bool runDimLinesBool;
  private List<FamilyUpdaterTask> deleteTasks = new List<FamilyUpdaterTask>();
  private List<FamilyUpdaterTask> addTasks = new List<FamilyUpdaterTask>();
  private List<FamilyUpdaterTask> modifyTasks = new List<FamilyUpdaterTask>();
  private Dictionary<string, List<FamilyUpdaterTask>> outputDictionary = new Dictionary<string, List<FamilyUpdaterTask>>();
  private DefinitionGroups sharedParameterDefintions;
  public static FamilyUpdaterTask currentTaskCopy;

  public void addTaskToLists(FamilyUpdaterTask task)
  {
    if (task.action == BulkFamilyUpdatersUtils.taskActions.Delete)
      this.deleteTasks.Add(task);
    else if (task.action == BulkFamilyUpdatersUtils.taskActions.Add)
    {
      this.addTasks.Add(task);
    }
    else
    {
      if (task.action != BulkFamilyUpdatersUtils.taskActions.Modify)
        return;
      this.modifyTasks.Add(task);
    }
  }

  public void addTasksToLists(List<FamilyUpdaterTask> tasks)
  {
    foreach (FamilyUpdaterTask task in tasks)
      this.addTaskToLists(task);
  }

  public Dictionary<string, List<FamilyUpdaterTask>> getOutputDictionary() => this.outputDictionary;

  public void setSharedParameterDefinition(DefinitionGroups sharedParameters)
  {
    this.sharedParameterDefintions = sharedParameters;
  }

  public void runTasks(
    FileInfo[] familyFiles,
    Document revitDoc,
    Autodesk.Revit.ApplicationServices.Application revitApp,
    List<string> openFamilies)
  {
    if (this.getTasksCount() == 0 && !this.runDimLinesBool)
      return;
    foreach (FileInfo familyFile in familyFiles)
    {
      bool saveModified = false;
      Document currentFamDoc;
      try
      {
        currentFamDoc = revitApp.OpenDocumentFile(familyFile.FullName);
      }
      catch (CorruptModelException ex)
      {
        string errorMessage = "Task not performed. This family was created in an incompatible year of Revit.";
        this.addFailMessageToTasks(this.deleteTasks, familyFile.Name, errorMessage);
        this.addFailMessageToTasks(this.addTasks, familyFile.Name, errorMessage);
        this.addFailMessageToTasks(this.modifyTasks, familyFile.Name, errorMessage);
        if (this.runDimLinesBool)
        {
          this.addToOutputDictionary(familyFile.Name, new FamilyUpdaterTask(BulkFamilyUpdatersUtils.taskActions.AddDimLines)
          {
            messType = BulkFamilyUpdatersUtils.messageType.fail,
            outputMessage = errorMessage
          });
          continue;
        }
        continue;
      }
      catch (Exception ex)
      {
        string errorMessage = "Task not performed. Unable to open the revit file.";
        this.addFailMessageToTasks(this.deleteTasks, familyFile.Name, errorMessage);
        this.addFailMessageToTasks(this.addTasks, familyFile.Name, errorMessage);
        this.addFailMessageToTasks(this.modifyTasks, familyFile.Name, errorMessage);
        if (this.runDimLinesBool)
        {
          this.addToOutputDictionary(familyFile.Name, new FamilyUpdaterTask(BulkFamilyUpdatersUtils.taskActions.AddDimLines)
          {
            messType = BulkFamilyUpdatersUtils.messageType.fail,
            outputMessage = errorMessage
          });
          continue;
        }
        continue;
      }
      if (familyFile.Attributes.HasFlag((Enum) FileAttributes.ReadOnly))
      {
        string warningMessage = "Task not performed. File is read only";
        this.addWarningMessageToTasks(this.deleteTasks, familyFile.Name, warningMessage);
        this.addWarningMessageToTasks(this.addTasks, familyFile.Name, warningMessage);
        this.addWarningMessageToTasks(this.modifyTasks, familyFile.Name, warningMessage);
        if (this.runDimLinesBool)
          this.addToOutputDictionary(familyFile.Name, new FamilyUpdaterTask(BulkFamilyUpdatersUtils.taskActions.AddDimLines)
          {
            messType = BulkFamilyUpdatersUtils.messageType.warning,
            outputMessage = warningMessage
          });
      }
      else
      {
        FamilyManager familyManager = currentFamDoc.FamilyManager;
        if (currentFamDoc.IsReadOnlyFile)
        {
          string warningMessage = "Task not performed. File is read only";
          this.addWarningMessageToTasks(this.deleteTasks, familyFile.Name, warningMessage);
          this.addWarningMessageToTasks(this.addTasks, familyFile.Name, warningMessage);
          this.addWarningMessageToTasks(this.modifyTasks, familyFile.Name, warningMessage);
          if (this.runDimLinesBool)
            this.addToOutputDictionary(familyFile.Name, new FamilyUpdaterTask(BulkFamilyUpdatersUtils.taskActions.AddDimLines)
            {
              outputMessage = warningMessage,
              messType = BulkFamilyUpdatersUtils.messageType.warning
            });
        }
        else if (currentFamDoc == null)
        {
          string errorMessage = "Task not performed. Unable to open the revit file.";
          this.addFailMessageToTasks(this.deleteTasks, familyFile.Name, errorMessage);
          this.addFailMessageToTasks(this.addTasks, familyFile.Name, errorMessage);
          this.addFailMessageToTasks(this.modifyTasks, familyFile.Name, errorMessage);
          if (this.runDimLinesBool)
            this.addToOutputDictionary(familyFile.Name, new FamilyUpdaterTask(BulkFamilyUpdatersUtils.taskActions.AddDimLines)
            {
              messType = BulkFamilyUpdatersUtils.messageType.fail,
              outputMessage = errorMessage
            });
        }
        else
        {
          foreach (FamilyUpdaterTask deleteTask in this.deleteTasks)
          {
            TaskManager.currentTaskCopy = new FamilyUpdaterTask(deleteTask);
            this.runDeleteTask(familyManager, currentFamDoc, deleteTask);
            if (TaskManager.currentTaskCopy.messType == BulkFamilyUpdatersUtils.messageType.success)
              saveModified = true;
            this.addToOutputDictionary(familyFile.Name, TaskManager.currentTaskCopy);
            TaskManager.currentTaskCopy = (FamilyUpdaterTask) null;
          }
          foreach (FamilyUpdaterTask addTask in this.addTasks)
          {
            TaskManager.currentTaskCopy = new FamilyUpdaterTask(addTask);
            this.runAddTask(familyManager, currentFamDoc, addTask, revitDoc);
            if (TaskManager.currentTaskCopy.messType == BulkFamilyUpdatersUtils.messageType.success)
              saveModified = true;
            this.addToOutputDictionary(familyFile.Name, TaskManager.currentTaskCopy);
            TaskManager.currentTaskCopy = (FamilyUpdaterTask) null;
          }
          foreach (FamilyUpdaterTask modifyTask in this.modifyTasks)
          {
            TaskManager.currentTaskCopy = new FamilyUpdaterTask(modifyTask);
            this.runModifyTask(familyManager, currentFamDoc, modifyTask, revitDoc);
            if (TaskManager.currentTaskCopy.messType == BulkFamilyUpdatersUtils.messageType.success)
              saveModified = true;
            this.addToOutputDictionary(familyFile.Name, TaskManager.currentTaskCopy);
            TaskManager.currentTaskCopy = (FamilyUpdaterTask) null;
          }
          if (this.runDimLinesBool)
          {
            TaskManager.currentTaskCopy = new FamilyUpdaterTask(BulkFamilyUpdatersUtils.taskActions.AddDimLines);
            this.addDimLines(familyManager, currentFamDoc);
            if (TaskManager.currentTaskCopy.messType == BulkFamilyUpdatersUtils.messageType.success)
              saveModified = true;
            this.addToOutputDictionary(familyFile.Name, TaskManager.currentTaskCopy);
            TaskManager.currentTaskCopy = (FamilyUpdaterTask) null;
          }
          if (revitDoc.PathName.ToUpper() == familyFile.FullName.ToUpper() || openFamilies.Contains(currentFamDoc.Title))
          {
            if (currentFamDoc != null & saveModified)
              currentFamDoc.Save();
          }
          else
            currentFamDoc?.Close(saveModified);
        }
      }
    }
  }

  private void addToOutputDictionary(string family, FamilyUpdaterTask task)
  {
    if (this.outputDictionary.ContainsKey(family))
    {
      this.outputDictionary[family].Add(task);
    }
    else
    {
      List<FamilyUpdaterTask> familyUpdaterTaskList = new List<FamilyUpdaterTask>()
      {
        task
      };
      this.outputDictionary.Add(family, familyUpdaterTaskList);
    }
  }

  private void addFailMessageToTasks(
    List<FamilyUpdaterTask> tasks,
    string familyName,
    string errorMessage)
  {
    foreach (FamilyUpdaterTask task in tasks)
      this.addToOutputDictionary(familyName, new FamilyUpdaterTask(task)
      {
        messType = BulkFamilyUpdatersUtils.messageType.fail,
        outputMessage = errorMessage
      });
  }

  private void addWarningMessageToTasks(
    List<FamilyUpdaterTask> tasks,
    string familyName,
    string warningMessage)
  {
    foreach (FamilyUpdaterTask task in tasks)
      this.addToOutputDictionary(familyName, new FamilyUpdaterTask(task)
      {
        messType = BulkFamilyUpdatersUtils.messageType.warning,
        outputMessage = warningMessage
      });
  }

  private int getTasksCount()
  {
    return this.addTasks.Count + this.modifyTasks.Count + this.deleteTasks.Count;
  }

  private int getOutputDictionaryTasksCount()
  {
    int dictionaryTasksCount = 0;
    foreach (KeyValuePair<string, List<FamilyUpdaterTask>> output in this.outputDictionary)
      dictionaryTasksCount += output.Value.Count;
    return dictionaryTasksCount;
  }

  private void runDeleteTask(
    FamilyManager familyManager,
    Document currentFamDoc,
    FamilyUpdaterTask task)
  {
    List<FamilyParameter> list = familyManager.GetParameters().ToList<FamilyParameter>();
    FamilyParameter famParam = (FamilyParameter) null;
    foreach (FamilyParameter familyParameter in list)
    {
      if (familyParameter.Definition.Name.Equals(task.parameterName.Trim()))
      {
        famParam = familyParameter;
        break;
      }
    }
    if (famParam == null)
    {
      TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.warning, "Unable to find parameter that was requested to be deleted in this family.");
    }
    else
    {
      using (Transaction transaction = new Transaction(currentFamDoc, "Delete Parameter Transaction"))
      {
        FailureHandlingOptions failureHandlingOptions = transaction.GetFailureHandlingOptions();
        FailurePreproccessor preprocessor = new FailurePreproccessor();
        failureHandlingOptions.SetFailuresPreprocessor((IFailuresPreprocessor) preprocessor);
        transaction.SetFailureHandlingOptions(failureHandlingOptions);
        int num1 = (int) transaction.Start();
        int num2 = this.deleteParameter(familyManager, famParam) ? 1 : 0;
        TransactionStatus transactionStatus = num2 == 0 ? transaction.RollBack() : transaction.Commit();
        if (num2 == 0 || transactionStatus == TransactionStatus.RolledBack)
          return;
        TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.success);
      }
    }
  }

  private bool deleteParameter(FamilyManager famManager, FamilyParameter famParam)
  {
    try
    {
      famManager.RemoveParameter(famParam);
    }
    catch (Exception ex)
    {
      string output = ("Unable to delete parameter. " + ex.Message).Replace("\r\n", " ");
      TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.fail, output);
      return false;
    }
    return true;
  }

  private void runAddTask(
    FamilyManager familyManager,
    Document currentFamDoc,
    FamilyUpdaterTask task,
    Document primaryRevitDoc)
  {
    List<FamilyParameter> list = familyManager.GetParameters().ToList<FamilyParameter>();
    FamilyParameter famParam = (FamilyParameter) null;
    foreach (FamilyParameter familyParameter in list)
    {
      if (familyParameter.Definition.Name.Equals(task.parameterName.Trim()))
      {
        famParam = familyParameter;
        break;
      }
    }
    bool flag = false;
    bool modifyNeeded = true;
    using (Transaction transaction = new Transaction(currentFamDoc, "Add Parameter Transaction"))
    {
      FailureHandlingOptions failureHandlingOptions = transaction.GetFailureHandlingOptions();
      FailurePreproccessor preprocessor = new FailurePreproccessor();
      failureHandlingOptions.SetFailuresPreprocessor((IFailuresPreprocessor) preprocessor);
      transaction.SetFailureHandlingOptions(failureHandlingOptions);
      int num = (int) transaction.Start();
      if (famParam == null)
      {
        FamilyParameter famParameter = (FamilyParameter) null;
        flag = !task.isSharedParameter.ToUpper().Contains("SHARED") ? this.addFamilyParameter(familyManager, task, primaryRevitDoc, currentFamDoc, out famParameter) : this.addSharedParameter(familyManager, task, primaryRevitDoc, currentFamDoc, out famParameter);
      }
      else
      {
        ForgeTypeId dataType = famParam.Definition.GetDataType();
        ForgeTypeId disc = BulkFamilyUpdatersUtils.disciplineToString2223.FirstOrDefault<KeyValuePair<ForgeTypeId, string>>((Func<KeyValuePair<ForgeTypeId, string>, bool>) (x => x.Value == task.discipline)).Key;
        ForgeTypeId key = BulkFamilyUpdatersUtils.specTypeIdToString2223.FirstOrDefault<KeyValuePair<ForgeTypeId, string>>((Func<KeyValuePair<ForgeTypeId, string>, bool>) (x => x.Value == task.specType && BulkFamilyUpdatersUtils.disciplineForgeTypeIdToParameterSpecTypeIdDictionary[disc].Contains(x.Key))).Key;
        if (task.isSharedParameter.ToUpper().Contains("SHARED"))
        {
          if (!famParam.IsShared)
          {
            TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.fail, "Parameter with same name exists in the family but it is a family parameter. Unable to add or modify parameter to match request.");
            return;
          }
        }
        else if (famParam.IsShared)
        {
          TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.fail, "Parameter with same name exists in the family but it is a shared parameter. Unable to add or modify parameter to match request.");
          return;
        }
        if (dataType.Equals((object) key))
          flag = this.AddTaskModifyParameter(familyManager, famParam, task, primaryRevitDoc, currentFamDoc, out modifyNeeded);
        else
          TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.warning, "Parameter with same name and of a different type already exists in this family. Unable to add or modify parameter to match request.");
      }
      TransactionStatus transactionStatus;
      if (flag)
      {
        transactionStatus = transaction.Commit();
      }
      else
      {
        transactionStatus = transaction.RollBack();
        if ((TaskManager.currentTaskCopy.messType != BulkFamilyUpdatersUtils.messageType.fail || TaskManager.currentTaskCopy.messType != BulkFamilyUpdatersUtils.messageType.warning) && string.IsNullOrEmpty(TaskManager.currentTaskCopy.outputMessage))
        {
          TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.fail, "Unable to add parameter.");
          return;
        }
      }
      if (((!flag ? 0 : (transactionStatus != TransactionStatus.RolledBack ? 1 : 0)) & (modifyNeeded ? 1 : 0)) == 0)
        return;
      TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.success);
    }
  }

  private bool addSharedParameter(
    FamilyManager famManager,
    FamilyUpdaterTask task,
    Document primaryRevitDoc,
    Document familyDocument,
    out FamilyParameter famParameter,
    bool? instanceBool = null)
  {
    famParameter = (FamilyParameter) null;
    bool isInstance = !instanceBool.HasValue ? !task.isTypeParameter.ToUpper().Contains("TYPE") : instanceBool.GetValueOrDefault();
    ExternalDefinition familyDefinition = (ExternalDefinition) null;
    foreach (DefinitionGroup parameterDefintion in this.sharedParameterDefintions)
    {
      foreach (ExternalDefinition definition in parameterDefintion.Definitions)
      {
        if (definition.Name.Equals(task.parameterName))
        {
          familyDefinition = definition;
          break;
        }
      }
      if (familyDefinition != null)
        break;
    }
    if (familyDefinition == null)
    {
      TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.fail, "Unable to add Shared Parameter");
      return false;
    }
    ForgeTypeId key = BulkFamilyUpdatersUtils.GroupTypeIdToString2223.FirstOrDefault<KeyValuePair<ForgeTypeId, string>>((Func<KeyValuePair<ForgeTypeId, string>, bool>) (x => x.Value == task.paramGroup)).Key;
    try
    {
      famParameter = famManager.AddParameter(familyDefinition, key, isInstance);
    }
    catch (Exception ex)
    {
      string output = ex.Message.Replace("\r\n", " ");
      TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.fail, output);
      return false;
    }
    if (famParameter != null)
    {
      bool flag = false;
      if (string.IsNullOrWhiteSpace(task.value))
      {
        flag = true;
      }
      else
      {
        foreach (FamilyType type in famManager.Types)
        {
          if (task.isValueFormula.GetValueOrDefault())
          {
            flag = this.modifyParameterValueFormula(famManager, famParameter, task.value, familyDocument);
            break;
          }
          flag = this.modifyParameterValue(primaryRevitDoc, familyDocument, famManager, famParameter, task.value, type);
        }
      }
      return flag;
    }
    TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.fail, "Shared Parameter not created");
    return false;
  }

  private bool addFamilyParameter(
    FamilyManager famManager,
    FamilyUpdaterTask task,
    Document primaryRevitDoc,
    Document familyDocument,
    out FamilyParameter famParameter,
    bool? instanceBool = null)
  {
    famParameter = (FamilyParameter) null;
    bool isInstance = !instanceBool.HasValue ? !task.isTypeParameter.ToUpper().Contains("TYPE") : instanceBool.GetValueOrDefault();
    try
    {
      KeyValuePair<ForgeTypeId, string> keyValuePair = BulkFamilyUpdatersUtils.disciplineToString2223.FirstOrDefault<KeyValuePair<ForgeTypeId, string>>((Func<KeyValuePair<ForgeTypeId, string>, bool>) (x => x.Value == task.discipline));
      ForgeTypeId disc = keyValuePair.Key;
      keyValuePair = BulkFamilyUpdatersUtils.specTypeIdToString2223.FirstOrDefault<KeyValuePair<ForgeTypeId, string>>((Func<KeyValuePair<ForgeTypeId, string>, bool>) (x => x.Value == task.specType && BulkFamilyUpdatersUtils.disciplineForgeTypeIdToParameterSpecTypeIdDictionary[disc].Contains(x.Key)));
      ForgeTypeId key1 = keyValuePair.Key;
      keyValuePair = BulkFamilyUpdatersUtils.GroupTypeIdToString2223.FirstOrDefault<KeyValuePair<ForgeTypeId, string>>((Func<KeyValuePair<ForgeTypeId, string>, bool>) (x => x.Value == task.paramGroup));
      ForgeTypeId key2 = keyValuePair.Key;
      famParameter = famManager.AddParameter(task.parameterName.Trim(), key2, key1, isInstance);
    }
    catch (Exception ex)
    {
      string output = ex.Message.Replace("\r\n", "");
      TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.fail, output);
      return false;
    }
    if (famParameter != null)
    {
      bool flag = false;
      if (string.IsNullOrWhiteSpace(task.value))
      {
        flag = true;
      }
      else
      {
        foreach (FamilyType type in famManager.Types)
        {
          if (task.isValueFormula.GetValueOrDefault())
          {
            flag = this.modifyParameterValueFormula(famManager, famParameter, task.value, familyDocument);
            break;
          }
          flag = this.modifyParameterValue(primaryRevitDoc, familyDocument, famManager, famParameter, task.value, type);
        }
        if (famManager.Types.IsEmpty)
        {
          if (task.value.ToUpper() == "UNASSIGNED")
          {
            flag = true;
          }
          else
          {
            TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.fail, "Unable to add Family Parameter. No types found within the family.");
            return false;
          }
        }
      }
      return flag;
    }
    TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.fail, "Parameter not created");
    return false;
  }

  private bool AddTaskModifyParameter(
    FamilyManager famManager,
    FamilyParameter famParam,
    FamilyUpdaterTask task,
    Document primaryRevitDoc,
    Document familyRevitDoc,
    out bool modifyNeeded)
  {
    TaskManager.currentTaskCopy.oldIsTypeParameter = !famParam.IsInstance ? "Type" : "Instance";
    ForgeTypeId key = BulkFamilyUpdatersUtils.GroupTypeIdToString2223.FirstOrDefault<KeyValuePair<ForgeTypeId, string>>((Func<KeyValuePair<ForgeTypeId, string>, bool>) (x => x.Value == task.paramGroup)).Key;
    ForgeTypeId groupTypeId = famParam.Definition.GetGroupTypeId();
    TaskManager.currentTaskCopy.oldParamGroup = BulkFamilyUpdatersUtils.GroupTypeIdToString2223[groupTypeId];
    bool flag1 = false;
    if (key != (ForgeTypeId) null && groupTypeId != (ForgeTypeId) null)
      flag1 = groupTypeId.NameEquals(key);
    modifyNeeded = false;
    modifyNeeded = !flag1 || this.checkIfModificationsNeeded(TaskManager.currentTaskCopy.oldIsTypeParameter, TaskManager.currentTaskCopy.isTypeParameter, TaskManager.currentTaskCopy.value, TaskManager.currentTaskCopy.isValueFormula.GetValueOrDefault(), famParam, primaryRevitDoc, familyRevitDoc, famManager);
    if (!modifyNeeded)
    {
      TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.warning, "Parameter exactly matches an existing parameter. No changes made.");
      return true;
    }
    bool flag2 = false;
    if (!string.IsNullOrWhiteSpace(task.isTypeParameter))
    {
      if (task.isTypeParameter.ToUpper().Contains("INSTANCE"))
        flag2 = this.modifyTypeInstance(famManager, famParam, true);
      else if (task.isTypeParameter.ToUpper().Contains("TYPE"))
        flag2 = this.modifyTypeInstance(famManager, famParam, false);
      if (!flag2)
        return false;
    }
    if (!groupTypeId.Equals((object) key))
    {
      if (!this.modifyGroupParameterUnder(famManager, famParam, task, familyRevitDoc, primaryRevitDoc))
        return false;
      foreach (FamilyParameter familyParameter in famManager.GetParameters().ToList<FamilyParameter>())
      {
        if (familyParameter.Definition.Name.Equals(task.parameterName.Trim()))
        {
          famParam = familyParameter;
          break;
        }
      }
    }
    try
    {
      if (task.value.Trim().ToUpper().Equals("UNASSIGNED") || string.IsNullOrWhiteSpace(task.value))
      {
        foreach (FamilyType type in famManager.Types)
          this.clearParameterValue(famManager, famParam, type);
        if (famManager.Types.IsEmpty)
        {
          TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.fail, "Unable to add Parameter. No types found within the family.");
          return false;
        }
      }
      else
      {
        foreach (FamilyType type in famManager.Types)
          this.modifyParameterValue(primaryRevitDoc, familyRevitDoc, famManager, famParam, task.value, type);
        if (famManager.Types.IsEmpty)
        {
          TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.fail, "Unable to add Parameter. No types found within the family.");
          return false;
        }
      }
    }
    catch (Exception ex)
    {
      string output = ("The parameter already exists. Unable to modify value. " + ex.Message).Replace("\r\n", "");
      TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.fail, output);
      return false;
    }
    return true;
  }

  private void runModifyTask(
    FamilyManager familyManager,
    Document currentFamDoc,
    FamilyUpdaterTask task,
    Document primaryRevitDoc)
  {
    List<FamilyParameter> list = familyManager.GetParameters().ToList<FamilyParameter>();
    FamilyParameter famParam = (FamilyParameter) null;
    foreach (FamilyParameter familyParameter in list)
    {
      if (familyParameter.Definition.Name.Equals(task.oldParameterName.Trim()))
      {
        famParam = familyParameter;
        break;
      }
    }
    if (famParam == null)
    {
      TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.warning, "Unable to find parameter that was requested to be modified in this family.");
    }
    else
    {
      using (Transaction transaction = new Transaction(currentFamDoc, "Modify Parameter Transaction"))
      {
        FailureHandlingOptions failureHandlingOptions = transaction.GetFailureHandlingOptions();
        FailurePreproccessor preprocessor = new FailurePreproccessor();
        failureHandlingOptions.SetFailuresPreprocessor((IFailuresPreprocessor) preprocessor);
        transaction.SetFailureHandlingOptions(failureHandlingOptions);
        int num1 = (int) transaction.Start();
        bool flag1 = true;
        if (!famParam.Definition.Name.Equals(task.parameterName) && !string.IsNullOrWhiteSpace(task.parameterName))
        {
          int num2 = this.modifyParameterName(familyManager, famParam, task.parameterName) ? 1 : 0;
          flag1 = false;
          if (num2 == 0)
          {
            int num3 = (int) transaction.RollBack();
            return;
          }
        }
        TaskManager.currentTaskCopy.oldIsTypeParameter = !famParam.IsInstance ? "Type" : "Instance";
        ForgeTypeId key = BulkFamilyUpdatersUtils.GroupTypeIdToString2223.FirstOrDefault<KeyValuePair<ForgeTypeId, string>>((Func<KeyValuePair<ForgeTypeId, string>, bool>) (x => x.Value == task.paramGroup)).Key;
        ForgeTypeId groupTypeId = famParam.Definition.GetGroupTypeId();
        TaskManager.currentTaskCopy.oldParamGroup = BulkFamilyUpdatersUtils.GroupTypeIdToString2223[groupTypeId];
        bool flag2 = false;
        if (key != (ForgeTypeId) null && groupTypeId != (ForgeTypeId) null)
          flag2 = groupTypeId.NameEquals(key);
        if (task.paramGroup.Equals("unchanged"))
          flag2 = true;
        if (flag2 & flag1 && !this.checkIfModificationsNeeded(TaskManager.currentTaskCopy.oldIsTypeParameter, TaskManager.currentTaskCopy.isTypeParameter, TaskManager.currentTaskCopy.value, TaskManager.currentTaskCopy.isValueFormula.GetValueOrDefault(), famParam, primaryRevitDoc, currentFamDoc, familyManager))
        {
          int num4 = (int) transaction.RollBack();
          TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.warning, "Parameter already matched desired modifications. No changes made.");
        }
        else
        {
          if (!string.IsNullOrWhiteSpace(task.isTypeParameter) && !task.isTypeParameter.Trim().ToUpper().Equals("UNCHANGED"))
          {
            bool flag3 = false;
            if (task.isTypeParameter.ToUpper().Contains("INSTANCE"))
              flag3 = this.modifyTypeInstance(familyManager, famParam, true);
            else if (task.isTypeParameter.ToUpper().Contains("TYPE"))
              flag3 = this.modifyTypeInstance(familyManager, famParam, false);
            if (!flag3)
            {
              int num5 = (int) transaction.RollBack();
              return;
            }
          }
          if (!task.paramGroup.Trim().ToUpper().Equals("UNCHANGED") && !groupTypeId.Equals((object) key))
          {
            if (this.modifyGroupParameterUnder(familyManager, famParam, task, currentFamDoc, primaryRevitDoc))
            {
              foreach (FamilyParameter familyParameter in familyManager.GetParameters().ToList<FamilyParameter>())
              {
                if (familyParameter.Definition.Name.Equals(task.parameterName.Trim()))
                {
                  famParam = familyParameter;
                  break;
                }
              }
            }
            else
            {
              int num6 = (int) transaction.RollBack();
              return;
            }
          }
          if (!task.value.Trim().ToUpper().Equals("UNCHANGED"))
          {
            bool flag4 = false;
            if (task.clearValue.GetValueOrDefault())
            {
              foreach (FamilyType type in familyManager.Types)
                flag4 = this.clearParameterValue(familyManager, famParam, type);
            }
            else if (task.isValueFormula.GetValueOrDefault())
            {
              flag4 = this.modifyParameterValueFormula(familyManager, famParam, task.value, currentFamDoc);
            }
            else
            {
              foreach (FamilyType type in familyManager.Types)
                flag4 = this.modifyParameterValue(primaryRevitDoc, currentFamDoc, familyManager, famParam, task.value, type);
            }
            if (!flag4)
            {
              int num7 = (int) transaction.RollBack();
              return;
            }
          }
          int num8 = (int) transaction.Commit();
          if (transaction.GetStatus() != TransactionStatus.Committed)
            return;
          TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.success);
        }
      }
    }
  }

  private bool modifyParameterName(
    FamilyManager famManager,
    FamilyParameter famParam,
    string value)
  {
    try
    {
      famManager.RenameParameter(famParam, value);
    }
    catch (Exception ex)
    {
      string output = ("Unable to change name of the parameter. " + ex.Message).Replace("\r\n", "");
      TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.fail, output);
      return false;
    }
    return true;
  }

  private bool modifyTypeInstance(
    FamilyManager famManager,
    FamilyParameter famParam,
    bool isInstance)
  {
    try
    {
      if (isInstance)
        famManager.MakeInstance(famParam);
      else
        famManager.MakeType(famParam);
    }
    catch (Exception ex)
    {
      string output = ("Parameter already exists. Error updateing the parameter's type/instance property. " + ex.Message).Replace("\r\n", "");
      TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.fail, output);
      return false;
    }
    return true;
  }

  private bool modifyGroupParameterUnder(
    FamilyManager famManager,
    FamilyParameter famParam,
    FamilyUpdaterTask task,
    Document familyRevitDoc,
    Document primaryRevitDoc)
  {
    ForgeTypeId key = BulkFamilyUpdatersUtils.GroupTypeIdToString2223.FirstOrDefault<KeyValuePair<ForgeTypeId, string>>((Func<KeyValuePair<ForgeTypeId, string>, bool>) (x => x.Value == task.paramGroup)).Key;
    try
    {
      string formula = (string) null;
      Dictionary<FamilyType, string> dictionary1 = new Dictionary<FamilyType, string>();
      Dictionary<FamilyType, ElementId> dictionary2 = new Dictionary<FamilyType, ElementId>();
      if (famParam.IsDeterminedByFormula)
      {
        formula = famParam.Formula;
      }
      else
      {
        foreach (FamilyType type in famManager.Types)
        {
          if (famParam.StorageType == StorageType.ElementId)
            dictionary2.Add(type, type.AsElementId(famParam));
          else if (famParam.StorageType == StorageType.Integer)
            dictionary1.Add(type, type.AsInteger(famParam).ToString());
          else if (famParam.StorageType == StorageType.Double)
            dictionary1.Add(type, type.AsDouble(famParam).ToString());
          else
            dictionary1.Add(type, type.AsValueString(famParam));
        }
      }
      bool isShared = famParam.IsShared;
      bool? instanceBool = new bool?();
      instanceBool = !famParam.IsInstance ? new bool?(false) : new bool?(true);
      if (!this.deleteParameter(famManager, famParam))
      {
        TaskManager.currentTaskCopy.outputMessage = "Error while updating group parameter is grouped under. " + TaskManager.currentTaskCopy.outputMessage;
        return false;
      }
      FamilyParameter famParameter = (FamilyParameter) null;
      if (!(!isShared ? this.addFamilyParameter(famManager, task, primaryRevitDoc, familyRevitDoc, out famParameter, instanceBool) : this.addSharedParameter(famManager, task, primaryRevitDoc, familyRevitDoc, out famParameter, instanceBool)))
      {
        TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.fail, "Error while updating group parameter is grouped under. " + TaskManager.currentTaskCopy.outputMessage);
        return false;
      }
      if (!string.IsNullOrWhiteSpace(task.value) && !task.value.Trim().ToUpper().Equals("UNCHANGED"))
      {
        if (task.clearValue.GetValueOrDefault())
          goto label_40;
      }
      bool flag = false;
      if (string.IsNullOrEmpty(formula))
      {
        if (famParameter.StorageType == StorageType.ElementId)
        {
          if (famParameter.Definition.GetDataType() != SpecTypeId.Reference.FillPattern)
          {
            foreach (KeyValuePair<FamilyType, ElementId> keyValuePair in dictionary2)
            {
              flag = this.modifyParameterValue(familyRevitDoc, familyRevitDoc, famManager, famParameter, "", keyValuePair.Key, keyValuePair.Value);
              if (!flag)
                break;
            }
          }
          else
            flag = true;
        }
        else
        {
          foreach (KeyValuePair<FamilyType, string> keyValuePair in dictionary1)
          {
            flag = this.modifyParameterValue(familyRevitDoc, familyRevitDoc, famManager, famParameter, keyValuePair.Value, keyValuePair.Key);
            if (!flag)
              break;
          }
        }
      }
      else
        flag = this.modifyParameterValueFormula(famManager, famParameter, formula, familyRevitDoc);
      if (!flag)
      {
        TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.fail, "Unable to modify parameter group.");
        return false;
      }
    }
    catch (Exception ex)
    {
      string output = ("Error updating the parameter's group. " + ex.Message).Replace("\r\n", "");
      TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.fail, output);
      return false;
    }
label_40:
    return true;
  }

  private bool modifyParameterValue(
    Document revitDoc,
    Document familyDocument,
    FamilyManager famManager,
    FamilyParameter famParam,
    string value,
    FamilyType famType,
    ElementId elemId = null)
  {
    if (value == null)
      value = string.Empty;
    if (value.Trim().ToUpper().Equals("UNASSIGNED") || value.Trim().ToUpper().Equals("UNCHANGED") || value.Trim().ToUpper().Equals("CLEARED"))
      return true;
    StorageType storageType = famParam.StorageType;
    Units units = revitDoc.GetUnits();
    ForgeTypeId dataType = famParam.Definition.GetDataType();
    FamilyType currentType = famManager.CurrentType;
    try
    {
      if (famParam.IsDeterminedByFormula && !this.modifyParameterValueFormula(famManager, famParam, (string) null, familyDocument))
      {
        TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.fail, "Unable to modify parameter. Unable to remove current formula value for parameter. " + TaskManager.currentTaskCopy.outputMessage);
        return false;
      }
      famManager.CurrentType = famType;
      if (elemId != (ElementId) null)
      {
        famManager.Set(famParam, elemId);
        famManager.CurrentType = currentType;
        return true;
      }
      famManager.CurrentType = famType;
      switch (storageType)
      {
        case StorageType.None:
          famManager.Set(famParam, value);
          break;
        case StorageType.Integer:
          if (value.ToUpper().Contains("YES"))
          {
            famManager.Set(famParam, 1);
            break;
          }
          if (value.ToUpper().Contains("NO"))
          {
            famManager.Set(famParam, 0);
            break;
          }
          double num1 = 0.0;
          if (UnitFormatUtils.TryParse(units, dataType, value, out num1))
          {
            int num2 = (int) num1;
            famManager.Set(famParam, num2);
            break;
          }
          TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.fail, "Unable to set new parameter value.");
          return false;
        case StorageType.Double:
          double num3 = 0.0;
          if (UnitFormatUtils.TryParse(units, dataType, value, out num3))
          {
            famManager.Set(famParam, num3);
            break;
          }
          TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.fail, "Unable to set new parameter value.");
          return false;
        case StorageType.String:
          famManager.Set(famParam, value);
          break;
        case StorageType.ElementId:
          ElementId reference = this.findReference(familyDocument, value, famParam);
          if (reference != (ElementId) null)
          {
            famManager.Set(famParam, reference);
            break;
          }
          TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.fail, "Unable to set new parameter value.");
          return false;
      }
    }
    catch (Exception ex)
    {
      string output = ("Unable to set new parameter value. " + ex.Message).Replace("\r\n", "");
      TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.fail, output);
      famManager.CurrentType = currentType;
      return false;
    }
    finally
    {
      famManager.CurrentType = currentType;
    }
    return true;
  }

  private bool modifyParameterValueFormula(
    FamilyManager famManager,
    FamilyParameter famParam,
    string formula,
    Document revitDoc)
  {
    try
    {
      famManager.SetFormula(famParam, formula);
      revitDoc.Regenerate();
      IList<FamilyParameter> familyParameterList = (IList<FamilyParameter>) null;
      if (revitDoc.IsFamilyDocument)
        familyParameterList = revitDoc.FamilyManager.GetParameters();
      foreach (FamilyParameter familyParameter in (IEnumerable<FamilyParameter>) familyParameterList)
      {
        if (familyParameter.Formula != null && familyParameter.Formula.Contains(famParam.Definition.Name))
          famManager.SetFormula(familyParameter, familyParameter.Formula);
      }
    }
    catch (Exception ex)
    {
      string output = ("Unable to set new formula for parameter. " + ex.Message).Replace("\r\n", " ");
      TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.fail, output);
      return false;
    }
    return true;
  }

  private bool clearParameterValue(
    FamilyManager famManager,
    FamilyParameter famParam,
    FamilyType famType)
  {
    StorageType storageType = famParam.StorageType;
    FamilyType currentType = famManager.CurrentType;
    try
    {
      famManager.CurrentType = famType;
      switch (storageType)
      {
        case StorageType.None:
          famManager.Set(famParam, "");
          break;
        case StorageType.Integer:
          famManager.Set(famParam, 0);
          break;
        case StorageType.Double:
          famManager.Set(famParam, 0.0);
          break;
        case StorageType.String:
          famManager.Set(famParam, "");
          break;
        case StorageType.ElementId:
          ElementId elementId = new ElementId(-1);
          famManager.Set(famParam, elementId);
          break;
      }
    }
    catch (Exception ex)
    {
      string output = ex.Message.Replace("\r\n", " ");
      TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.fail, output);
      return false;
    }
    finally
    {
      famManager.CurrentType = currentType;
    }
    return true;
  }

  private bool checkIfModificationsNeeded(
    string typeInstanceParam,
    string typeInstanceTask,
    string valueTask,
    bool determinedByFormula,
    FamilyParameter famParam,
    Document primaryRevitDoc,
    Document familyDocument,
    FamilyManager familyManager)
  {
    if (!typeInstanceParam.ToUpper().Equals(typeInstanceTask.ToUpper()) && !typeInstanceTask.Equals("unchanged"))
      return true;
    ForgeTypeId dataType = famParam.Definition.GetDataType();
    Units units = primaryRevitDoc.GetUnits();
    if (valueTask.Equals("unassigned") || valueTask.Equals("unchanged"))
      return false;
    if (determinedByFormula)
    {
      if (famParam.Formula == null || !valueTask.Trim().Equals(famParam.Formula.Trim()))
        return true;
    }
    else
    {
      foreach (FamilyType type in familyManager.Types)
      {
        if (famParam.StorageType == StorageType.ElementId)
        {
          ElementId reference = this.findReference(familyDocument, valueTask, famParam);
          if (!type.AsElementId(famParam).Equals((object) reference))
            return true;
        }
        else if (famParam.StorageType == StorageType.Double)
        {
          double num1;
          UnitFormatUtils.TryParse(units, dataType, valueTask, out num1);
          double num2 = type.AsDouble(famParam).Value;
          if (num1 != num2)
            return true;
        }
        else if (famParam.StorageType == StorageType.String)
        {
          string str = type.AsString(famParam);
          if (str != null && !str.Trim().Equals(valueTask.Trim()))
            return true;
        }
        else
        {
          string str = type.AsInteger(famParam).ToString();
          if (valueTask.ToUpper().Equals("YES"))
            valueTask = "1";
          else if (valueTask.ToUpper().Equals("NO"))
            valueTask = "0";
          if (!str.Trim().Equals(valueTask.Trim()))
            return true;
        }
      }
    }
    return false;
  }

  private ElementId findReference(Document familyDocument, string name, FamilyParameter famParam)
  {
    ElementId reference = (ElementId) null;
    FilteredElementCollector elementCollector = new FilteredElementCollector(familyDocument);
    List<Element> list;
    if (famParam.Definition.GetDataType() == SpecTypeId.Reference.Image)
      list = elementCollector.OfCategory(BuiltInCategory.OST_RasterImages).ToList<Element>();
    else if (famParam.Definition.GetDataType() == SpecTypeId.Reference.Material)
    {
      list = elementCollector.OfCategory(BuiltInCategory.OST_Materials).ToList<Element>();
    }
    else
    {
      if (famParam.Definition.GetDataType() == SpecTypeId.Reference.FillPattern)
        return (ElementId) null;
      list = elementCollector.WhereElementIsNotElementType().WhereElementIsElementType().ToList<Element>();
    }
    if (list == null)
      return (ElementId) null;
    foreach (Element element in list)
    {
      if (element.Name.Contains(name))
      {
        reference = element.Id;
        break;
      }
    }
    return reference;
  }

  private void addDimLines(FamilyManager familyManager, Document currentFamDoc)
  {
    FamilyType currentType = familyManager.CurrentType;
    using (Transaction transaction = new Transaction(currentFamDoc, "Add EDGE Dim Lines Transaction"))
    {
      TaskManager.FamilyOperation familyOperation = new TaskManager.FamilyOperation(this.DrawDimensionAxes);
      FailureHandlingOptions failureHandlingOptions = transaction.GetFailureHandlingOptions();
      FailurePreproccessor preprocessor = new FailurePreproccessor();
      failureHandlingOptions.SetFailuresPreprocessor((IFailuresPreprocessor) preprocessor);
      transaction.SetFailureHandlingOptions(failureHandlingOptions);
      TransactionStatus transactionStatus = transaction.Start();
      if (transactionStatus != TransactionStatus.Started)
      {
        TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.fail, "Failed to add EDGE dim lines to " + currentFamDoc.PathName);
      }
      else
      {
        if (!familyOperation(currentFamDoc, (object) null))
        {
          int num = (int) transaction.RollBack();
        }
        else
          transactionStatus = transaction.Commit();
        if (transactionStatus != TransactionStatus.Committed)
        {
          TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.fail, "Failed to add EDGE dim lines");
        }
        else
        {
          TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.success);
          FamilyParameter familyParameter = familyManager.get_Parameter("MANUFACTURE_COMPONENT");
          if (currentType == null || familyParameter == null || !currentType.HasValue(familyParameter) || !(currentType.AsString(familyParameter) == "CONNECTION"))
            return;
          this.HandleNested(new FilteredElementCollector(currentFamDoc).OfClass(typeof (FamilyInstance)).Cast<FamilyInstance>().DistinctBy<FamilyInstance, ElementId>((Func<FamilyInstance, ElementId>) (s => s.Symbol.Family.Id)).ToList<FamilyInstance>(), currentFamDoc);
        }
      }
    }
  }

  private bool DrawDimensionAxes(Document familyDoc, object data)
  {
    try
    {
      if (!familyDoc.IsFamilyDocument)
        return false;
      string str = "EdgeDimLines";
      double x = 0.0;
      double y = 0.0;
      foreach (Element elem in new FilteredElementCollector(familyDoc).OfClass(typeof (ReferencePlane)).ToList<Element>())
      {
        if (Parameters.GetParameterAsInt(elem, "Defines Origin").Equals(1))
        {
          ReferencePlane referencePlane = elem as ReferencePlane;
          if (referencePlane.GetPlane().Normal.X == 1.0 || referencePlane.GetPlane().Normal.X == -1.0)
            x = referencePlane.GetPlane().Origin.X;
          else if (referencePlane.GetPlane().Normal.Y == 1.0 || referencePlane.GetPlane().Normal.Y == -1.0)
            y = referencePlane.GetPlane().Origin.Y;
        }
      }
      XYZ origin = new XYZ(x, y, 0.0);
      SketchPlane sketchPlane1 = SketchPlane.Create(familyDoc, Plane.CreateByNormalAndOrigin(XYZ.BasisZ, origin));
      SketchPlane sketchPlane2 = SketchPlane.Create(familyDoc, Plane.CreateByNormalAndOrigin(XYZ.BasisY, origin));
      Category familyCategory = familyDoc.OwnerFamily.FamilyCategory;
      Category category = (Category) null;
      if (familyCategory.SubCategories.Contains(str))
      {
        category = familyCategory.SubCategories.get_Item(str);
        List<Element> list = new FilteredElementCollector(familyDoc).OfClass(typeof (CurveElement)).ToList<Element>();
        List<ModelCurve> modelCurveList = new List<ModelCurve>();
        foreach (Element element in list)
        {
          if (element is ModelCurve)
          {
            ModelCurve modelCurve = element as ModelCurve;
            if (familyDoc.GetElement(modelCurve.LineStyle.Id).Name == "EdgeDimLines")
              familyDoc.Delete(modelCurve.Id);
          }
        }
      }
      if (category == null && familyCategory != null && familyCategory.CanAddSubcategory)
        category = familyDoc.Settings.Categories.NewSubcategory(familyCategory, str);
      if (category == null)
        return false;
      XYZ xyz1 = XYZ.BasisX / 24.0;
      XYZ xyz2 = XYZ.BasisY / 24.0;
      XYZ xyz3 = XYZ.BasisZ / 24.0;
      familyDoc.FamilyCreate.NewModelCurve((Curve) Line.CreateBound(origin - xyz1, origin + xyz1), sketchPlane1).Subcategory = category.GetGraphicsStyle(GraphicsStyleType.Projection);
      familyDoc.FamilyCreate.NewModelCurve((Curve) Line.CreateBound(origin - xyz2, origin + xyz2), sketchPlane1).Subcategory = category.GetGraphicsStyle(GraphicsStyleType.Projection);
      familyDoc.FamilyCreate.NewModelCurve((Curve) Line.CreateBound(origin - xyz3, origin + xyz3), sketchPlane2).Subcategory = category.GetGraphicsStyle(GraphicsStyleType.Projection);
      return true;
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex.Message);
      return false;
    }
  }

  private bool HandleNested(List<FamilyInstance> nestedFamilies, Document currentFamDoc)
  {
    TaskManager.FamilyOperation familyOperation = new TaskManager.FamilyOperation(this.DrawDimensionAxes);
    foreach (FamilyInstance nestedFamily in nestedFamilies)
    {
      if (!nestedFamily.Name.Equals("CONNECTOR_COMPONENT"))
      {
        Family family = nestedFamily.Symbol.Family;
        if (family != null)
        {
          if (family.FamilyCategory.CategoryType == CategoryType.Model)
          {
            Document document;
            try
            {
              document = currentFamDoc.EditFamily(family);
            }
            catch (Exception ex)
            {
              TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.fail, "Failed to add EDGE dim lines to " + currentFamDoc.PathName);
              return false;
            }
            if (document != null)
            {
              using (Transaction transaction = new Transaction(document, "UpdateNestedFamily"))
              {
                FailureHandlingOptions failureHandlingOptions = transaction.GetFailureHandlingOptions();
                FailurePreproccessor preprocessor = new FailurePreproccessor();
                failureHandlingOptions.SetFailuresPreprocessor((IFailuresPreprocessor) preprocessor);
                transaction.SetFailureHandlingOptions(failureHandlingOptions);
                int num1 = (int) transaction.Start();
                if (!familyOperation(document, (object) null))
                {
                  TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.fail, $"Failed to add edge dim linees on {document.OwnerFamily.Name} in {currentFamDoc.PathName}");
                  return false;
                }
                Parameter parameter = nestedFamily.LookupParameter("MANUFACTURE_COMPONENT");
                parameter?.AsString();
                if (parameter != null && parameter.AsString() == "CONNECTION")
                  this.HandleNested(new FilteredElementCollector(document).OfClass(typeof (FamilyInstance)).Cast<FamilyInstance>().DistinctBy<FamilyInstance, ElementId>((Func<FamilyInstance, ElementId>) (s => s.Symbol.Family.Id)).ToList<FamilyInstance>(), document);
                if (transaction.Commit() != TransactionStatus.Committed)
                {
                  TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.warning, $"EDGE dim lines were added to family, but was unable to add EDGE dim lines nested family {family.Name} in {currentFamDoc.PathName}");
                  int num2 = (int) transaction.RollBack();
                  return false;
                }
                TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.success);
                string name = family.Name;
                string pathName = currentFamDoc.PathName;
                TaskManager.FamilyLoadOptions familyLoadOptions = new TaskManager.FamilyLoadOptions();
                try
                {
                  if (document.LoadFamily(currentFamDoc, (IFamilyLoadOptions) familyLoadOptions) == null)
                  {
                    TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.warning, $"EDGE dim lines were added to family and nested family, but was unable to re-load nested family {family.Name} in {currentFamDoc.PathName}");
                    return false;
                  }
                }
                catch (Exception ex)
                {
                  TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.warning, $"EDGE dim lines were added to family and nested family, but was unable to re-load nested family {name} in {pathName}");
                  return false;
                }
              }
            }
          }
        }
      }
    }
    return true;
  }

  public void createExcelSpreadSheet(string filePath)
  {
    if (this.getOutputDictionaryTasksCount() == 0)
      return;
    bool flag1 = false;
    bool flag2 = false;
    bool flag3 = false;
    string str1;
    try
    {
      List<string> stringList = new List<string>()
      {
        "Family Name",
        "Action",
        "Result",
        "Message",
        "Shared/Family",
        "Parameter Name",
        "New Parameter Name",
        "Discipline",
        "Type of Parameter",
        "Parameter Group",
        "New Parameter Group",
        "Type/Instance",
        "New Type/Instance",
        "Value"
      };
      string[,] strArray = new string[this.getOutputDictionaryTasksCount() + 1, stringList.Count];
      int num1 = 0;
      foreach (string str2 in stringList)
        strArray[0, num1++] = str2;
      string str3 = DateTime.Now.ToString("yyyy-MM-dd-HHmm");
      str1 = $"{filePath}BFUResult-{str3}";
      bool flag4 = true;
      DirectoryInfo directoryInfo = new DirectoryInfo(filePath);
      FileInfo fileInfo = new FileInfo(filePath);
      int num2 = 2;
      string str4 = str1;
      while (flag4)
      {
        FileInfo[] array = ((IEnumerable<FileInfo>) directoryInfo.GetFiles("*.*")).Where<FileInfo>((Func<FileInfo, bool>) (s => s.FullName.EndsWith(".xlsx"))).ToArray<FileInfo>();
        if (directoryInfo.Exists)
        {
          bool flag5 = false;
          foreach (FileSystemInfo fileSystemInfo in array)
          {
            if (fileSystemInfo.FullName.ToUpper().Equals(str1.ToUpper() + ".XLSX"))
              flag5 = true;
          }
          if (flag5)
            str1 = $"{str4} ({num2++.ToString()})";
          else
            flag4 = false;
        }
      }
      ExcelDocument excelDocument = new ExcelDocument(str1 + ".xlsx");
      if (excelDocument.ExcelFileOpen)
      {
        int index = 1;
        foreach (KeyValuePair<string, List<FamilyUpdaterTask>> output in this.outputDictionary)
        {
          string key = output.Key;
          foreach (FamilyUpdaterTask familyUpdaterTask in output.Value)
          {
            string str5 = "";
            string str6 = "";
            string str7 = "";
            if (familyUpdaterTask.messType == BulkFamilyUpdatersUtils.messageType.success)
              flag3 = true;
            else if (familyUpdaterTask.messType == BulkFamilyUpdatersUtils.messageType.fail)
              flag1 = true;
            else if (familyUpdaterTask.messType == BulkFamilyUpdatersUtils.messageType.warning)
              flag2 = true;
            string str8 = familyUpdaterTask.action.ToString() + " Parameter";
            string str9 = familyUpdaterTask.messType.ToString();
            string outputMessage = familyUpdaterTask.outputMessage;
            string isSharedParameter = familyUpdaterTask.isSharedParameter;
            string str10 = familyUpdaterTask.parameterName;
            string discipline = familyUpdaterTask.discipline;
            string specType = familyUpdaterTask.specType;
            string str11 = familyUpdaterTask.paramGroup;
            string str12 = familyUpdaterTask.isTypeParameter;
            string str13 = familyUpdaterTask.value;
            if (familyUpdaterTask.action == BulkFamilyUpdatersUtils.taskActions.Add)
            {
              if (!string.IsNullOrWhiteSpace(familyUpdaterTask.oldParamGroup))
              {
                str11 = familyUpdaterTask.oldParamGroup;
                str6 = familyUpdaterTask.paramGroup;
              }
              if (!string.IsNullOrWhiteSpace(familyUpdaterTask.oldIsTypeParameter))
              {
                str12 = familyUpdaterTask.oldIsTypeParameter;
                str7 = familyUpdaterTask.isTypeParameter;
              }
            }
            else if (familyUpdaterTask.action == BulkFamilyUpdatersUtils.taskActions.Modify)
            {
              if (!string.IsNullOrWhiteSpace(familyUpdaterTask.oldParameterName))
              {
                if (familyUpdaterTask.oldParameterName.Equals(familyUpdaterTask.parameterName))
                {
                  str10 = familyUpdaterTask.oldParameterName;
                  str5 = "unchanged";
                }
                else
                {
                  str10 = familyUpdaterTask.oldParameterName;
                  str5 = familyUpdaterTask.parameterName;
                }
              }
              if (!string.IsNullOrWhiteSpace(familyUpdaterTask.oldParamGroup))
              {
                str11 = familyUpdaterTask.oldParamGroup;
                str6 = familyUpdaterTask.paramGroup;
              }
              if (!string.IsNullOrWhiteSpace(familyUpdaterTask.oldIsTypeParameter))
              {
                str12 = familyUpdaterTask.oldIsTypeParameter;
                str7 = familyUpdaterTask.isTypeParameter;
              }
            }
            else if (familyUpdaterTask.action == BulkFamilyUpdatersUtils.taskActions.AddDimLines)
              str8 = "Add EDGE Dim Lines";
            strArray[index, 0] = key;
            strArray[index, 1] = str8;
            strArray[index, 2] = str9;
            strArray[index, 3] = outputMessage;
            strArray[index, 4] = isSharedParameter;
            strArray[index, 5] = str10;
            strArray[index, 6] = str5;
            strArray[index, 7] = discipline;
            strArray[index, 8] = specType;
            strArray[index, 9] = str11;
            strArray[index, 10] = str6;
            strArray[index, 11] = str12;
            strArray[index, 12] = str7;
            strArray[index++, 13] = str13;
          }
        }
        for (int rowId = 1; rowId < this.getOutputDictionaryTasksCount() + 2; ++rowId)
        {
          for (int columnId = 1; columnId < stringList.Count + 1; ++columnId)
            excelDocument.UpdateCellValue(columnId, rowId, strArray[rowId - 1, columnId - 1]);
        }
        excelDocument.SaveAndClose();
      }
      else
      {
        new TaskDialog("Bulk Family Updater")
        {
          MainContent = "Bulk Family Updater completed running. Error occured creating output Excel spreadsheet."
        }.Show();
        return;
      }
    }
    catch (Exception ex)
    {
      new TaskDialog("Bulk Family Updater")
      {
        MainContent = "Bulk Family Updater completed running. Error occured creating output Excel spreadsheet."
      }.Show();
      return;
    }
    if (flag3 && !flag1 && !flag2)
    {
      int num3 = (int) MessageBox.Show($"All families were successfully updated. For more information about what was processed, a detailed report can be found here: {str1}.xlsx", "Bulk Family Updater");
    }
    else if (flag3 && flag1 | flag2)
    {
      int num4 = (int) MessageBox.Show($"Only some families and/or updates were successfully processed. For more information about what was processed, a detailed report can be found here: {str1}.xlsx", "Bulk Family Updater");
    }
    else
    {
      if (!(flag1 | flag2) || flag3)
        return;
      int num5 = (int) MessageBox.Show($"No families were updated. For more information, a detailed report can be found here: {str1}.xlsx", "Bulk Family Updater");
    }
  }

  private delegate bool FamilyOperation(Document doc, object data);

  private class FamilyLoadOptions : IFamilyLoadOptions
  {
    public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
    {
      overwriteParameterValues = true;
      return true;
    }

    public bool OnSharedFamilyFound(
      Family sharedFamily,
      bool familyInUse,
      out FamilySource source,
      out bool overwriteParameterValues)
    {
      source = FamilySource.Family;
      overwriteParameterValues = true;
      return true;
    }
  }
}
