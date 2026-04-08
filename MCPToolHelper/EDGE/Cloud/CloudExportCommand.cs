// Decompiled with JetBrains decompiler
// Type: EDGE.Cloud.CloudExportCommand
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.Cloud.Models;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utils.AssemblyUtils;
using Utils.GeometryUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.Cloud;

[Transaction(TransactionMode.Manual)]
public class CloudExportCommand : IExternalCommand
{
  public static UIApplication UIApplication;
  private UIDocument _uiDocument;
  private Document _document;
  private List<View3D> _exportedViews = new List<View3D>();
  private IDictionary<ElementId, List<ElementId>> _nonExportedElements = (IDictionary<ElementId, List<ElementId>>) new Dictionary<ElementId, List<ElementId>>();
  private AssemblyInfo _assemblyInfo;
  private IDictionary<string, AssemblyInfo> _assemblyInfos;
  private static CloudExportForm _exportForm;
  private static CloudUploadForm _uploadForm;
  private Dictionary<string, List<EDGE.Cloud.Models.View>> _views = new Dictionary<string, List<EDGE.Cloud.Models.View>>();
  private ICollection<ElementId> _selectedElementIds;
  private AsyncDialog _asyncDialog;
  private int _count;
  public static string Username;
  public static string Password;
  public static string Token;
  public static string AuthenticationUrl;
  public static string ServiceUrl;
  public static string WebUrl;
  public static string[] CloudNames = new string[8]
  {
    "Localhost",
    "Azure - Dev",
    "Azure - Demo",
    "Azure - Production",
    "Azure - Production 2",
    "Azure - Dev",
    "A2Hosting - Demo",
    "Azure - Production"
  };
  private static string[] _serviceUrls = new string[8]
  {
    "http://localhost:65279",
    "https://edgeservicedev.azurewebsites.net",
    "https://edgeservicedemo.azurewebsites.net",
    "https://edgeserviceprod.azurewebsites.net",
    "https://edgeserviceprod2.azurewebsites.net",
    "http://edgeservicedemo.azurewebsites.net",
    "http://servicedemo.edge.a2hosted.com",
    "https://edgeserviceprod.azurewebsites.net"
  };
  private static string[] _webUrls = new string[8]
  {
    "http://localhost:65298",
    "http://edgewebdev.azurewebsites.net",
    "http://edgewebdemo.azurewebsites.net",
    "http://edgeforcloud.com",
    "http://cloud.edge.ptac.com",
    "http://dev.edge.a2hosted.com",
    "http://demo.edge.a2hosted.com",
    "http://edgeforcloud.com"
  };

  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    CloudExportCommand.UIApplication = commandData.Application;
    return this.Execute(commandData.Application.ActiveUIDocument, commandData.Application.ActiveUIDocument.Document);
  }

  public Result Execute(UIDocument uiDocument, Document document)
  {
    try
    {
      this._uiDocument = uiDocument;
      this._document = document;
      ActiveModel.UIDoc = this._uiDocument;
      ActiveModel.Document = this._document;
      this.SendToCloud(this.GetSelectedAssemblies());
    }
    catch (Exception ex)
    {
      TaskDialog.Show("Error", $"{ex.Message}\n{ex.StackTrace}");
    }
    return (Result) 0;
  }

  private IEnumerable<Element> GetSelectedAssemblies()
  {
    this._uiDocument = ActiveModel.UIDoc;
    this._document = ActiveModel.Document;
    List<ElementId> list = this._uiDocument.Selection.GetElementIds().ToList<ElementId>();
    this._count = list.Count;
    return list.Select<ElementId, Element>(new Func<ElementId, Element>(this._document.GetElement)).Where<Element>((Func<Element, bool>) (e => e is AssemblyInstance assInst && assInst.GetStructuralFramingElement() != null && !Utils.ElementUtils.Parameters.GetParameterAsBool(e, "HARDWARE_DETAIL")));
  }

  private IEnumerable<Element> GetSelectedElements()
  {
    this._uiDocument = ActiveModel.UIDoc;
    this._document = ActiveModel.Document;
    return this._uiDocument.Selection.GetElementIds().ToList<ElementId>().Select<ElementId, Element>(new Func<ElementId, Element>(this._document.GetElement));
  }

  private View3D CreateView3DtoExport(Element selectedElement)
  {
    View3D view3DtoExport = (View3D) null;
    this._document = ActiveModel.Document;
    if (selectedElement == null)
    {
      view3DtoExport = this._document.ActiveView as View3D;
    }
    else
    {
      AssemblyInstance selectedAssembly = selectedElement as AssemblyInstance;
      if (selectedAssembly != null)
      {
        List<Autodesk.Revit.DB.View> list1 = new FilteredElementCollector(this._document).OfClass(typeof (Autodesk.Revit.DB.View)).Cast<Autodesk.Revit.DB.View>().ToList<Autodesk.Revit.DB.View>();
        List<Autodesk.Revit.DB.View> viewList = new List<Autodesk.Revit.DB.View>();
        Func<Autodesk.Revit.DB.View, bool> predicate = (Func<Autodesk.Revit.DB.View, bool>) (v => v.AssociatedAssemblyInstanceId.ToString().Equals(selectedAssembly.Id.ToString()));
        List<Autodesk.Revit.DB.View> list2 = list1.Where<Autodesk.Revit.DB.View>(predicate).ToList<Autodesk.Revit.DB.View>();
        bool flag = false;
        foreach (Autodesk.Revit.DB.View view in list2)
        {
          if (view.Name.Equals("3D-EDGECLOUDVIEW"))
          {
            flag = true;
            view3DtoExport = view as View3D;
            break;
          }
        }
        if (!flag)
        {
          using (Transaction transaction = new Transaction(this._document, "create 3D cloud view for selected assembly"))
          {
            int num1 = (int) transaction.Start();
            view3DtoExport = AssemblyViewUtils.Create3DOrthographic(this._document, selectedAssembly.Id);
            view3DtoExport.Name = "3D-EDGECLOUDVIEW";
            int num2 = (int) transaction.Commit();
          }
        }
      }
    }
    return view3DtoExport;
  }

  private AssemblyInfo GetAssemblyInfo(IEnumerable<Element> elements)
  {
    AssemblyInfo assemblyInfo = (AssemblyInfo) null;
    foreach (Element element in elements)
    {
      if (element is AssemblyInstance assemblyInstance)
      {
        assemblyInfo = this.GetAssemblyInfo(assemblyInstance);
        break;
      }
    }
    if (assemblyInfo == null)
      assemblyInfo = new AssemblyInfo();
    return assemblyInfo;
  }

  private AssemblyInfo GetAssemblyInfo(AssemblyInstance assemblyInstance)
  {
    AssemblyInfo assemblyInfo = new AssemblyInfo();
    assemblyInfo.AssemblyName = assemblyInstance.AssemblyTypeName;
    this._document = ActiveModel.Document;
    string empty = string.Empty;
    IList<Autodesk.Revit.DB.Parameter> orderedParameters = assemblyInstance.GetOrderedParameters();
    string str = orderedParameters.Where<Autodesk.Revit.DB.Parameter>((Func<Autodesk.Revit.DB.Parameter, bool>) (p => p.Definition.Name == "PIECE_PLANT")).FirstOrDefault<Autodesk.Revit.DB.Parameter>() == null ? JsonExporter.GetStructuralFraming(assemblyInstance).GetOrderedParameters().Where<Autodesk.Revit.DB.Parameter>((Func<Autodesk.Revit.DB.Parameter, bool>) (p => p.Definition.Name == "PIECE_PLANT")).Select<Autodesk.Revit.DB.Parameter, string>((Func<Autodesk.Revit.DB.Parameter, string>) (P => P.AsString())).FirstOrDefault<string>() : orderedParameters.Where<Autodesk.Revit.DB.Parameter>((Func<Autodesk.Revit.DB.Parameter, bool>) (p => p.Definition.Name == "PIECE_PLANT")).FirstOrDefault<Autodesk.Revit.DB.Parameter>().AsString();
    if (string.IsNullOrEmpty(str))
      str = CloudExportCommand.GetProjectInfoValue(this._document, "PROJ_PRODUCING_PLANT");
    string projectInfoValue1 = CloudExportCommand.GetProjectInfoValue(this._document, "PROJECT_CLIENT_PRECAST_MANUFACTURER");
    string projectInfoValue2 = CloudExportCommand.GetProjectInfoValue(this._document, "Project Name");
    string projectInfoValue3 = CloudExportCommand.GetProjectInfoValue(this._document, "Project Number");
    string uniqueId = this._document.ProjectInformation.UniqueId;
    assemblyInfo.ProducerName = projectInfoValue1;
    assemblyInfo.PlantName = str;
    assemblyInfo.ProjectName = projectInfoValue2;
    assemblyInfo.ProjectNumber = projectInfoValue3;
    assemblyInfo.ProjectGUID = uniqueId;
    return assemblyInfo;
  }

  private AssemblyInfo GetAssemblyInfo(View3D view3D = null)
  {
    return view3D != null ? this.GetAssemblyInfo(view3D.Document.GetElement(view3D.AssociatedAssemblyInstanceId) as AssemblyInstance) : new AssemblyInfo();
  }

  private static string GetAssemblyName(View3D view3D)
  {
    return view3D.Document.GetElement(view3D.AssociatedAssemblyInstanceId) is AssemblyInstance element ? element.Name : view3D.Name;
  }

  public void SendToCloud(IEnumerable<Element> elements)
  {
    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
    int edgeCloudId = App.Edge_Cloud_Id;
    CloudExportCommand.GetServerUrlsFromCloudId(edgeCloudId);
    if (elements.Count<Element>() == 0)
    {
      if (CloudExportCommand._uploadForm == null || CloudExportCommand._uploadForm.IsDisposed)
        CloudExportCommand._uploadForm = new CloudUploadForm()
        {
          SaveSheets = new Func<View3D, List<ElementId>, List<EDGE.Cloud.Models.View>, IRestResponse>(this.SaveSheets),
          ExportSheetsToPDF = new Action(this.ExportSheetsToPDF)
        };
      CloudUploadForm.CloudId = edgeCloudId;
      CloudExportCommand._uploadForm.HideCloudSelection();
      int num = (int) CloudExportCommand._uploadForm.ShowDialog();
    }
    else
    {
      this._assemblyInfo = this.GetAssemblyInfo(elements);
      if (string.IsNullOrEmpty(this._assemblyInfo.ProducerName) || string.IsNullOrEmpty(this._assemblyInfo.PlantName) || string.IsNullOrEmpty(this._assemblyInfo.ProjectName))
      {
        TaskDialog.Show("EDGE^Cloud", "Please check missing Producer, Plant or Project in the Project Information.");
      }
      else
      {
        if (CloudExportCommand._exportForm == null || CloudExportCommand._exportForm.IsDisposed)
          CloudExportCommand._exportForm = new CloudExportForm()
          {
            ExportModel = new Action<string, string, string, string, string>(this.ExportModel)
          };
        CloudExportCommand._exportForm.AssemblyInfo = this._assemblyInfo;
        CloudExportForm.CloudId = edgeCloudId;
        CloudExportCommand._exportForm.HideCloudSelection();
        if (!string.IsNullOrEmpty(CloudExportCommand._exportForm.Token))
        {
          int num1 = (int) CloudExportCommand._exportForm.ShowDialog();
        }
        else
        {
          int num2 = (int) CloudExportCommand._exportForm.ShowDialog();
        }
      }
    }
  }

  private void ExportModel(
    string serverUrl,
    string authenticationUrl,
    string username,
    string password,
    string option)
  {
    try
    {
      bool flag1 = false;
      this._assemblyInfo = CloudExportCommand._exportForm.AssemblyInfo;
      this._assemblyInfo.ServerUrl = serverUrl;
      this._assemblyInfo.AuthenticationUrl = authenticationUrl;
      this._assemblyInfo.Username = username;
      this._assemblyInfo.Password = password;
      IRestResponse tokenResponse = UploadAssembly.GetTokenResponse(authenticationUrl, username, password);
      if (tokenResponse.ResponseStatus != ResponseStatus.Completed)
      {
        string str = string.Empty;
        switch (tokenResponse.ResponseStatus)
        {
          case ResponseStatus.Error:
            str = "Please check your internet connection and try again.";
            break;
          case ResponseStatus.TimedOut:
            str = "Connection to EDGE Cloud is timeout. Please try again later.";
            break;
          case ResponseStatus.Aborted:
            str = "Connection to EDGE Cloud is aborted. Please try again later.";
            break;
        }
        if (string.IsNullOrEmpty(str))
          return;
        AsyncDialog.Done = true;
        TaskDialog.Show("EDGE^Cloud", str);
      }
      else
      {
        string content = tokenResponse.Content;
        if (!string.IsNullOrEmpty(content))
          CloudExportCommand.Token = UploadAssembly.ToObject<AuthenticationModel>(content).Token;
        if (string.IsNullOrEmpty(CloudExportCommand.Token))
        {
          TaskDialog.Show("EDGE^Cloud", "Username and Password are not valid. Please try again.");
          AsyncDialog.Done = true;
        }
        else
        {
          CloudExportCommand.Username = username;
          CloudExportCommand.Password = password;
          if (UploadAssembly.UploadAssemblyPermission(serverUrl, CloudExportCommand.Token, this._assemblyInfo.ProducerName, this._assemblyInfo.PlantName, this._assemblyInfo.ProjectName, this._assemblyInfo.ProjectNumber))
          {
            CloudExportCommand._exportForm.HideAuthentication(CloudExportCommand.Token);
            this._exportedViews.Clear();
            this._nonExportedElements.Clear();
            IEnumerable<Element> selectedElements = this.GetSelectedElements();
            this._count = this.GetSelectedAssemblies().Count<Element>();
            if (this._count == 0)
              AsyncDialog.Done = true;
            this._asyncDialog = new AsyncDialog(this._count)
            {
              AssemblyInfo = this._assemblyInfo,
              WebUrl = CloudExportCommand.WebUrl,
              Form = (System.Windows.Forms.Form) CloudExportCommand._exportForm,
              LockControls = new Action<bool>(CloudExportCommand._exportForm.LockControls)
            };
            this.DeselectElements();
            foreach (Element elem1 in selectedElements)
            {
              if (elem1 is AssemblyInstance selectedElement)
              {
                List<ElementId> elementIdList = new List<ElementId>();
                if (flag1)
                {
                  using (Transaction transaction = new Transaction(this._document, "Cut voids in assembly instance"))
                  {
                    int num1 = (int) transaction.Start();
                    this._document.Regenerate();
                    ICollection<ElementId> memberIds = selectedElement.GetMemberIds();
                    foreach (ElementId elementId in (IEnumerable<ElementId>) memberIds)
                    {
                      ElementId memberId = elementId;
                      Element element1 = this._document.GetElement(memberId);
                      if (InstanceVoidCutUtils.IsVoidInstanceCuttingElement(element1))
                      {
                        try
                        {
                          selectedElement.RemoveMemberIds((ICollection<ElementId>) new List<ElementId>()
                          {
                            memberId
                          });
                        }
                        catch
                        {
                        }
                        foreach (ElementId id in (IEnumerable<ElementId>) memberIds)
                        {
                          Element element2 = this._document.GetElement(id);
                          if (!InstanceVoidCutUtils.InstanceVoidCutExists(element2, element1) && InstanceVoidCutUtils.CanBeCutWithVoid(element2) && InstanceVoidCutUtils.IsVoidInstanceCuttingElement(element1))
                            InstanceVoidCutUtils.AddInstanceVoidCut(this._document, element2, element1);
                        }
                      }
                      IEnumerable<Element> source = new FilteredElementCollector(this._document).OfClass(typeof (FamilyInstance)).Where<Element>((Func<Element, bool>) (f => (f as FamilyInstance).Host != null && (f as FamilyInstance).Host.Id == memberId));
                      if (source.Count<Element>() > 0)
                      {
                        foreach (Element element3 in source)
                        {
                          FamilyInstance cuttingInstance = element3 as FamilyInstance;
                          if (!InstanceVoidCutUtils.InstanceVoidCutExists(element1, (Element) cuttingInstance) && InstanceVoidCutUtils.CanBeCutWithVoid(element1) && InstanceVoidCutUtils.IsVoidInstanceCuttingElement((Element) cuttingInstance))
                            InstanceVoidCutUtils.AddInstanceVoidCut(this._document, element1, (Element) cuttingInstance);
                          if (InstanceVoidCutUtils.IsVoidInstanceCuttingElement((Element) cuttingInstance))
                          {
                            try
                            {
                              selectedElement.RemoveMemberIds((ICollection<ElementId>) new List<ElementId>()
                              {
                                element3.Id
                              });
                            }
                            catch
                            {
                            }
                          }
                        }
                      }
                    }
                    int num2 = (int) transaction.Commit();
                  }
                }
                else
                {
                  foreach (ElementId memberId in (IEnumerable<ElementId>) selectedElement.GetMemberIds())
                  {
                    bool flag2 = false;
                    Element element = this._document.GetElement(memberId);
                    if (element is FamilyInstance elem2)
                    {
                      foreach (ElementId subComponentId in (IEnumerable<ElementId>) elem2.GetSubComponentIds())
                      {
                        if (InstanceVoidCutUtils.IsVoidInstanceCuttingElement(this._document.GetElement(subComponentId)))
                          flag2 = true;
                      }
                      if (!flag2 && InstanceVoidCutUtils.IsVoidInstanceCuttingElement(element))
                      {
                        Solid solid = Solids.GetInstanceSolids((Element) elem2).FirstOrDefault<Solid>();
                        if ((GeometryObject) solid != (GeometryObject) null && solid.Volume < 0.0)
                          elementIdList.Add(memberId);
                      }
                    }
                  }
                }
                View3D view3DtoExport = this.CreateView3DtoExport((Element) selectedElement);
                if (view3DtoExport != null)
                {
                  this._exportedViews.Add(view3DtoExport);
                  this._nonExportedElements.Add(view3DtoExport.Id, elementIdList);
                  if (elementIdList.Count > 0)
                  {
                    string str1 = string.Empty;
                    foreach (ElementId elementId in elementIdList)
                      str1 = $"{str1}{elementId.ToString()}, ";
                    if (!string.IsNullOrEmpty(str1))
                    {
                      string str2 = str1.Substring(0, str1.Length - 2);
                      this._asyncDialog.AddError($"{CloudExportCommand.GetAssemblyName(view3DtoExport)} has elements with IDs not being exported: {str2}");
                    }
                  }
                }
              }
              else
              {
                string str = elem1.Id.ToString();
                this._asyncDialog.NonAssemblies.Add($"{JsonExporter.GetParameterAsString(elem1, "CONTROL_MARK")} ({str})");
              }
            }
            if (this._exportedViews.Count > 0)
            {
              ForgeExporter forgeExporter = new ForgeExporter(serverUrl)
              {
                AsyncDialog = this._asyncDialog
              };
              this._assemblyInfos = (IDictionary<string, AssemblyInfo>) new Dictionary<string, AssemblyInfo>();
              foreach (View3D exportedView in this._exportedViews)
              {
                try
                {
                  AssemblyInstance element = this._document.GetElement(exportedView.AssociatedAssemblyInstanceId) as AssemblyInstance;
                  AssemblyInfo assemblyInfo = this.GetAssemblyInfo(element);
                  if (!this._assemblyInfos.ContainsKey(element.Name))
                    this._assemblyInfos.Add(element.Name, assemblyInfo);
                  if (!AsyncDialog.Error)
                  {
                    if (!AsyncDialog.Done)
                    {
                      CloudExportCommand.RepaintForm();
                      List<ElementId> elementIdList = forgeExporter.Export2D(exportedView, option, this._nonExportedElements[exportedView.Id], new Func<View3D, List<ElementId>, List<EDGE.Cloud.Models.View>, IRestResponse>(this.SaveSheets));
                      if (elementIdList != null)
                      {
                        if (elementIdList != null)
                        {
                          if (elementIdList.Count != 0)
                            continue;
                        }
                        else
                          continue;
                      }
                      this._asyncDialog.AddInfo("There are no views/sheets associated with this assembly.");
                    }
                  }
                }
                catch (Exception ex)
                {
                }
              }
            }
          }
          else
          {
            CloudExportCommand._exportForm.LockControls(false);
            AsyncDialog.Done = true;
            int num = (int) MessageBox.Show("This user does not have enough permissions or correct producer/plant to export.", "EDGE^Cloud");
          }
        }
        this.ReselectElements();
      }
    }
    catch (Exception ex)
    {
      TaskDialog.Show("EDGE^Cloud", $"{ex.Message}\n{ex.StackTrace}");
    }
  }

  public void GetElementsInFamily(FamilyInstance familyInstance)
  {
    if (familyInstance == null)
      return;
    Document document = familyInstance.Document.EditFamily(familyInstance.Symbol.Family);
    int count = new FilteredElementCollector(document).OfClass(typeof (GenericForm)).ToElementIds().Count;
    document.Close(false);
  }

  private void DeselectElements()
  {
    this._uiDocument = ActiveModel.UIDoc;
    this._selectedElementIds = this._uiDocument.Selection.GetElementIds();
    this._uiDocument.Selection.SetElementIds((ICollection<ElementId>) new List<ElementId>());
  }

  private void ReselectElements()
  {
    if (this._selectedElementIds == null)
      return;
    this._uiDocument = ActiveModel.UIDoc;
    this._uiDocument.Selection.SetElementIds(this._selectedElementIds);
  }

  private IRestResponse SaveSheets(
    View3D view3D,
    List<ElementId> nonExportedElements,
    List<EDGE.Cloud.Models.View> views)
  {
    IRestResponse restResponse = (IRestResponse) null;
    try
    {
      if (view3D == null)
      {
        this._asyncDialog = new AsyncDialog(1)
        {
          AssemblyInfo = this._assemblyInfo,
          WebUrl = CloudExportCommand.WebUrl,
          Form = (System.Windows.Forms.Form) CloudExportCommand._uploadForm,
          LockControls = new Action<bool>(CloudExportCommand._uploadForm.LockControls)
        };
        this._assemblyInfo = new AssemblyInfo()
        {
          AuthenticationUrl = CloudExportCommand.AuthenticationUrl,
          ServerUrl = CloudExportCommand.ServiceUrl,
          Username = CloudExportCommand._uploadForm.Username,
          Password = CloudExportCommand._uploadForm.Password,
          ProducerName = CloudExportCommand._uploadForm.ProducerName,
          PlantName = CloudExportCommand._uploadForm.PlantName,
          ProjectName = CloudExportCommand._uploadForm.ProjectName,
          ProjectNumber = CloudExportCommand._uploadForm.ProjectNumber,
          AssemblyName = CloudExportCommand._uploadForm.AssemblyName
        };
        string assemblyName = this._assemblyInfo.AssemblyName;
        if (this._views.Keys.Contains<string>(assemblyName))
          this._views[assemblyName] = views;
        else
          this._views.Add(assemblyName, views);
        this.SaveModel(assemblyName);
      }
      else
      {
        string name = view3D.Document.GetElement(view3D.AssociatedAssemblyInstanceId).Name;
        if (this._views.Keys.Contains<string>(name))
          this._views[name] = views;
        else
          this._views.Add(name, views);
        JsonExporter jsonExporter = new JsonExporter()
        {
          NonExportedElements = nonExportedElements
        };
        if (!AsyncDialog.Error)
        {
          if (!AsyncDialog.Done)
          {
            CloudExportCommand.RepaintForm();
            jsonExporter.Export(view3D, new Func<string, byte[][], IRestResponse>(this.SaveModel));
          }
        }
      }
    }
    catch (Exception ex)
    {
      this._asyncDialog.AddError("Cannot export sheet " + view3D.Id.ToString());
      this._asyncDialog.AddStep();
    }
    return restResponse;
  }

  private IRestResponse SaveModel(string assemblyName, byte[][] bytes = null)
  {
    IRestResponse restResponse = (IRestResponse) null;
    UploadAssembly uploadAssembly1 = (UploadAssembly) null;
    try
    {
      AssemblyInfo assemblyInfo = this._assemblyInfo;
      if (this._assemblyInfos != null && this._assemblyInfos.ContainsKey(assemblyName))
        assemblyInfo = this._assemblyInfos[assemblyName];
      string empty = string.Empty;
      if (bytes == null)
      {
        string str = Project.CharacterFromProjectType(ProjectType.Upload);
        UploadAssembly uploadAssembly2 = new UploadAssembly();
        uploadAssembly2.ServiceUrl = this._assemblyInfo.ServerUrl;
        uploadAssembly2.AuthenticateUrl = this._assemblyInfo.AuthenticationUrl;
        uploadAssembly2.Username = this._assemblyInfo.Username;
        uploadAssembly2.Password = this._assemblyInfo.Password;
        uploadAssembly2.ManufactureName = assemblyInfo.ProducerName;
        uploadAssembly2.PlantName = assemblyInfo.PlantName;
        uploadAssembly2.ProjectName = assemblyInfo.ProjectName + str;
        uploadAssembly2.ProjectNumber = assemblyInfo.ProjectNumber;
        uploadAssembly2.ProjectGUID = assemblyInfo.ProjectGUID;
        uploadAssembly2.Name = assemblyName;
        uploadAssembly2.Views = this._views[assemblyName];
        uploadAssembly2.AddToExisting = false;
        uploadAssembly1 = uploadAssembly2;
      }
      else if (bytes.Length == 5)
      {
        string str = Project.CharacterFromProjectType(ProjectType.Export);
        UploadAssembly uploadAssembly3 = new UploadAssembly();
        uploadAssembly3.ServiceUrl = this._assemblyInfo.ServerUrl;
        uploadAssembly3.AuthenticateUrl = this._assemblyInfo.AuthenticationUrl;
        uploadAssembly3.Username = this._assemblyInfo.Username;
        uploadAssembly3.Password = this._assemblyInfo.Password;
        uploadAssembly3.ManufactureName = assemblyInfo.ProducerName;
        uploadAssembly3.PlantName = assemblyInfo.PlantName;
        uploadAssembly3.ProjectName = assemblyInfo.ProjectName + str;
        uploadAssembly3.ProjectNumber = assemblyInfo.ProjectNumber;
        uploadAssembly3.ProjectGUID = assemblyInfo.ProjectGUID;
        uploadAssembly3.Name = assemblyName;
        uploadAssembly3.ProjectData = bytes[0];
        uploadAssembly3.MetaData = bytes[1];
        uploadAssembly3.GeometryData = bytes[2];
        uploadAssembly3.PropertyData = bytes[3];
        uploadAssembly3.MaterialData = bytes[4];
        uploadAssembly3.Views = this._views[assemblyName];
        uploadAssembly1 = uploadAssembly3;
      }
      if (uploadAssembly1 != null)
      {
        Task<IRestResponse> task = UploadAssembly.UploadAsync(uploadAssembly1, CloudExportCommand.Token);
        task.Wait();
        if (task == null)
        {
          this._asyncDialog.AddError("Error to upload to Edge Cloud, either to check your internet connection or something may be wrong with the Edge Cloud.");
          this._asyncDialog.Add(assemblyName, true);
        }
        else
        {
          restResponse = task.Result;
          object obj1 = JsonConvert.DeserializeObject<object>(restResponse.Content);
          // ISSUE: reference to a compiler-generated field
          if (CloudExportCommand.\u003C\u003Eo__37.\u003C\u003Ep__2 == null)
          {
            // ISSUE: reference to a compiler-generated field
            CloudExportCommand.\u003C\u003Eo__37.\u003C\u003Ep__2 = CallSite<Func<CallSite, object, long>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (long), typeof (CloudExportCommand)));
          }
          // ISSUE: reference to a compiler-generated field
          Func<CallSite, object, long> target1 = CloudExportCommand.\u003C\u003Eo__37.\u003C\u003Ep__2.Target;
          // ISSUE: reference to a compiler-generated field
          CallSite<Func<CallSite, object, long>> p2 = CloudExportCommand.\u003C\u003Eo__37.\u003C\u003Ep__2;
          // ISSUE: reference to a compiler-generated field
          if (CloudExportCommand.\u003C\u003Eo__37.\u003C\u003Ep__1 == null)
          {
            // ISSUE: reference to a compiler-generated field
            CloudExportCommand.\u003C\u003Eo__37.\u003C\u003Ep__1 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "Value", typeof (CloudExportCommand), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
            {
              CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
            }));
          }
          // ISSUE: reference to a compiler-generated field
          Func<CallSite, object, object> target2 = CloudExportCommand.\u003C\u003Eo__37.\u003C\u003Ep__1.Target;
          // ISSUE: reference to a compiler-generated field
          CallSite<Func<CallSite, object, object>> p1 = CloudExportCommand.\u003C\u003Eo__37.\u003C\u003Ep__1;
          // ISSUE: reference to a compiler-generated field
          if (CloudExportCommand.\u003C\u003Eo__37.\u003C\u003Ep__0 == null)
          {
            // ISSUE: reference to a compiler-generated field
            CloudExportCommand.\u003C\u003Eo__37.\u003C\u003Ep__0 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "AssemblyId", typeof (CloudExportCommand), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
            {
              CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
            }));
          }
          // ISSUE: reference to a compiler-generated field
          // ISSUE: reference to a compiler-generated field
          object obj2 = CloudExportCommand.\u003C\u003Eo__37.\u003C\u003Ep__0.Target((CallSite) CloudExportCommand.\u003C\u003Eo__37.\u003C\u003Ep__0, obj1);
          object obj3 = target2((CallSite) p1, obj2);
          long num = target1((CallSite) p2, obj3);
          // ISSUE: reference to a compiler-generated field
          if (CloudExportCommand.\u003C\u003Eo__37.\u003C\u003Ep__5 == null)
          {
            // ISSUE: reference to a compiler-generated field
            CloudExportCommand.\u003C\u003Eo__37.\u003C\u003Ep__5 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof (string), typeof (CloudExportCommand)));
          }
          // ISSUE: reference to a compiler-generated field
          Func<CallSite, object, string> target3 = CloudExportCommand.\u003C\u003Eo__37.\u003C\u003Ep__5.Target;
          // ISSUE: reference to a compiler-generated field
          CallSite<Func<CallSite, object, string>> p5 = CloudExportCommand.\u003C\u003Eo__37.\u003C\u003Ep__5;
          // ISSUE: reference to a compiler-generated field
          if (CloudExportCommand.\u003C\u003Eo__37.\u003C\u003Ep__4 == null)
          {
            // ISSUE: reference to a compiler-generated field
            CloudExportCommand.\u003C\u003Eo__37.\u003C\u003Ep__4 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "Value", typeof (CloudExportCommand), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
            {
              CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
            }));
          }
          // ISSUE: reference to a compiler-generated field
          Func<CallSite, object, object> target4 = CloudExportCommand.\u003C\u003Eo__37.\u003C\u003Ep__4.Target;
          // ISSUE: reference to a compiler-generated field
          CallSite<Func<CallSite, object, object>> p4 = CloudExportCommand.\u003C\u003Eo__37.\u003C\u003Ep__4;
          // ISSUE: reference to a compiler-generated field
          if (CloudExportCommand.\u003C\u003Eo__37.\u003C\u003Ep__3 == null)
          {
            // ISSUE: reference to a compiler-generated field
            CloudExportCommand.\u003C\u003Eo__37.\u003C\u003Ep__3 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "Message", typeof (CloudExportCommand), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
            {
              CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
            }));
          }
          // ISSUE: reference to a compiler-generated field
          // ISSUE: reference to a compiler-generated field
          object obj4 = CloudExportCommand.\u003C\u003Eo__37.\u003C\u003Ep__3.Target((CallSite) CloudExportCommand.\u003C\u003Eo__37.\u003C\u003Ep__3, obj1);
          object obj5 = target4((CallSite) p4, obj4);
          string str = target3((CallSite) p5, obj5);
          if (!string.IsNullOrEmpty(str))
            this._asyncDialog.AddError($"{assemblyName}: {str}");
          this._asyncDialog.Add(assemblyName, num == 0L);
        }
      }
    }
    catch (Exception ex)
    {
      throw;
    }
    return restResponse;
  }

  private static string GetProjectInfoValue(Document doc, string parameterName)
  {
    string empty = string.Empty;
    Autodesk.Revit.DB.Parameter parameter = doc.ProjectInformation.LookupParameter(parameterName);
    if (parameter != null)
    {
      string str = parameter.AsString();
      if (!string.IsNullOrEmpty(str))
        empty = str.Split('\r')[0];
    }
    return empty;
  }

  private static void RepaintForm()
  {
    System.Windows.Forms.Application.DoEvents();
    Thread.Sleep(200);
  }

  public static void GetServerUrlsFromCloudId(int cloudId)
  {
    if (cloudId < 0 || cloudId >= CloudExportCommand._serviceUrls.Length)
      return;
    switch (cloudId)
    {
      case 0:
        CloudExportCommand.AuthenticationUrl = "http://localhost:65227/";
        break;
      case 1:
        CloudExportCommand.AuthenticationUrl = "https://edgeauthenticationdev.azurewebsites.net";
        break;
      case 2:
        CloudExportCommand.AuthenticationUrl = "https://edgeauthenticationdemo.azurewebsites.net";
        break;
      case 3:
      case 4:
        CloudExportCommand.AuthenticationUrl = "https://edgeauthentication.azurewebsites.net";
        break;
      case 7:
        CloudExportCommand.AuthenticationUrl = "https://edgeauthentication.azurewebsites.net";
        break;
      default:
        CloudExportCommand.AuthenticationUrl = "http://authentication.edge.a2hosted.com";
        break;
    }
    CloudExportCommand.ServiceUrl = CloudExportCommand._serviceUrls[cloudId];
    CloudExportCommand.WebUrl = CloudExportCommand._webUrls[cloudId];
  }

  public void ExportSheetsToPDF()
  {
    string str1 = "Export Sheets to PDF";
    Document document = this._uiDocument.Document;
    Autodesk.Revit.DB.View activeView = document.ActiveView;
    if (activeView is ViewSheet viewSheet)
    {
      List<Autodesk.Revit.DB.View> viewList = new List<Autodesk.Revit.DB.View>()
      {
        activeView
      };
      using (Transaction transaction = new Transaction(document, "Export Sheets"))
      {
        int num = (int) transaction.Start();
        List<ElementId> views = new List<ElementId>();
        DWGExportOptions options = new DWGExportOptions();
        options.MergedViews = true;
        options.SharedCoords = true;
        foreach (Element element in viewList)
        {
          try
          {
            if (!(element as ViewSheet).IsPlaceholder)
              views.Add(element.Id);
          }
          catch
          {
          }
        }
        string str2 = Path.GetDirectoryName(document.PathName) + "\\PDF";
        string name = "";
        Directory.CreateDirectory(str2);
        document.Export(str2, name, (ICollection<ElementId>) views, options);
        if (System.IO.File.Exists($"{str2}\\{Path.GetFileNameWithoutExtension(document.PathName).Replace(" ", "")}-Sheet - {viewSheet.SheetNumber} - {viewSheet.Name}".Replace(".", "-") + ".dwg"))
        {
          List<string> stringList = (List<string>) null;
          if (stringList.Count > 0)
            TaskDialog.Show(str1, "Active sheet exported to:\n" + stringList[0]);
          else
            TaskDialog.Show(str1, "No PDF file is created.");
        }
        else
          TaskDialog.Show(str1, "No DWG file is created.");
      }
    }
    else
      TaskDialog.Show(str1, "Please open sheet to export to PDF.");
  }
}
