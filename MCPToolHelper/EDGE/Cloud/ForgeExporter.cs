// Decompiled with JetBrains decompiler
// Type: EDGE.Cloud.ForgeExporter
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.Cloud.Models;
using EDGE.Cloud.Models.DataContracts;
using EDGE.Properties;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.AssemblyUtils;

#nullable disable
namespace EDGE.Cloud;

internal class ForgeExporter
{
  private static ForgeModel _forgeModel;
  private string _forgeBucket;

  public AsyncDialog AsyncDialog { get; set; }

  public ForgeExporter(string serverUrl)
  {
    this._forgeBucket = this.GetBucketName(serverUrl);
    string autodeskDeveloperApi = Resources.AutodeskDeveloperAPI;
    if (string.IsNullOrEmpty(serverUrl))
    {
      string autodeskConsumerKey = Resources.AutodeskConsumerKey;
      string autodeskConsumerSecret = Resources.AutodeskConsumerSecret;
      ForgeExporter._forgeModel = new ForgeModel(autodeskDeveloperApi, autodeskConsumerKey, autodeskConsumerSecret);
    }
    else
      ForgeExporter._forgeModel = new ForgeModel(autodeskDeveloperApi, serverUrl);
  }

  public List<ElementId> Export2D(
    View3D view3D,
    string option,
    List<ElementId> nonExportedElements,
    Func<View3D, List<ElementId>, List<EDGE.Cloud.Models.View>, IRestResponse> saveSheets)
  {
    List<ElementId> elementIdList1 = (List<ElementId>) null;
    string tempPath = Path.GetTempPath();
    Document document = view3D.Document;
    AssemblyInstance assemblyInstance = document.GetElement(view3D.AssociatedAssemblyInstanceId) as AssemblyInstance;
    if (assemblyInstance != null)
    {
      string assemblyTypeName = assemblyInstance.AssemblyTypeName;
      List<ElementId> collection1 = new List<ElementId>();
      List<ElementId> collection2 = new List<ElementId>();
      List<ElementId> collection3 = new List<ElementId>();
      if (option == "01" || option == "11")
      {
        using (FilteredElementCollector elementCollector = new FilteredElementCollector(document))
          collection1 = elementCollector.OfCategory(BuiltInCategory.OST_Sheets).OfClass(typeof (ViewSheet)).ToElements().Cast<ViewSheet>().Where<ViewSheet>((Func<ViewSheet, bool>) (s => s.IsAssemblyView && s.AssociatedAssemblyInstanceId == assemblyInstance.Id)).Select<ViewSheet, ElementId>((Func<ViewSheet, ElementId>) (s => s.Id)).ToList<ElementId>();
        List<ElementId> elementIdList2 = new List<ElementId>();
        foreach (ElementId id in collection1)
        {
          Autodesk.Revit.DB.Parameter parameter = document.GetElement(id).LookupParameter("IS_CLOUD_SHEET");
          if (parameter != null && parameter.AsInteger() > 0)
            elementIdList2.Add(id);
        }
        if (elementIdList2.Count > 0)
          collection1 = elementIdList2;
      }
      if (option == "10" || option == "11")
      {
        using (FilteredElementCollector elementCollector = new FilteredElementCollector(document))
          collection2 = elementCollector.OfCategory(BuiltInCategory.OST_Views).OfClass(typeof (Autodesk.Revit.DB.View)).ToElements().Cast<Autodesk.Revit.DB.View>().Where<Autodesk.Revit.DB.View>((Func<Autodesk.Revit.DB.View, bool>) (s => s.IsAssemblyView && s.AssociatedAssemblyInstanceId == assemblyInstance.Id && s.ViewType != ViewType.ThreeD)).Select<Autodesk.Revit.DB.View, ElementId>((Func<Autodesk.Revit.DB.View, ElementId>) (s => s.Id)).ToList<ElementId>();
        List<ElementId> elementIdList3 = new List<ElementId>();
        foreach (ElementId id in collection2)
        {
          Autodesk.Revit.DB.Parameter parameter = document.GetElement(id).LookupParameter("IS_CLOUD_VIEW");
          if (parameter != null && parameter.AsInteger() > 0)
            elementIdList3.Add(id);
        }
        if (elementIdList3.Count > 0)
          collection2 = elementIdList3;
        using (FilteredElementCollector elementCollector = new FilteredElementCollector(document))
        {
          Autodesk.Revit.DB.Parameter designNo = assemblyInstance.GetStructuralFramingElement().LookupParameter("DESIGN_NUMBER");
          if (designNo != null)
          {
            List<ElementId> list = elementCollector.OfCategory(BuiltInCategory.OST_Views).OfClass(typeof (Autodesk.Revit.DB.View)).ToElements().Cast<Autodesk.Revit.DB.View>().Where<Autodesk.Revit.DB.View>((Func<Autodesk.Revit.DB.View, bool>) (s => s.ViewType == ViewType.Legend && s.Name == "STRAND PATTERN " + designNo.AsString())).Select<Autodesk.Revit.DB.View, ElementId>((Func<Autodesk.Revit.DB.View, ElementId>) (s => s.Id)).ToList<ElementId>();
            collection3.AddRange((IEnumerable<ElementId>) list);
          }
        }
        using (FilteredElementCollector elementCollector = new FilteredElementCollector(document))
        {
          Autodesk.Revit.DB.Parameter productionFinish = assemblyInstance.GetStructuralFramingElement().LookupParameter("PRODUCTION_FINISH");
          if (productionFinish != null)
          {
            List<ElementId> list = elementCollector.OfCategory(BuiltInCategory.OST_Views).OfClass(typeof (Autodesk.Revit.DB.View)).ToElements().Cast<Autodesk.Revit.DB.View>().Where<Autodesk.Revit.DB.View>((Func<Autodesk.Revit.DB.View, bool>) (s => s.ViewType == ViewType.Legend && s.Name == "PRODUCTION FINISH " + productionFinish.AsString())).Select<Autodesk.Revit.DB.View, ElementId>((Func<Autodesk.Revit.DB.View, ElementId>) (s => s.Id)).ToList<ElementId>();
            collection3.AddRange((IEnumerable<ElementId>) list);
          }
        }
      }
      if (collection1 == null)
        collection1 = new List<ElementId>(0);
      if (collection2 == null)
        collection2 = new List<ElementId>(0);
      collection2.AddRange((IEnumerable<ElementId>) collection3);
      elementIdList1 = new List<ElementId>(collection2.Count + collection1.Count);
      elementIdList1.AddRange((IEnumerable<ElementId>) collection1);
      elementIdList1.AddRange((IEnumerable<ElementId>) collection2);
      List<EDGE.Cloud.Models.View> views1 = new List<EDGE.Cloud.Models.View>();
      if (elementIdList1.Count > 0)
      {
        using (Transaction transaction = new Transaction(document, "Export2DViewsForAssembly"))
        {
          int num1 = (int) transaction.Start();
          foreach (ElementId id in elementIdList1)
          {
            if (!AsyncDialog.Done)
            {
              Autodesk.Revit.DB.View element = document.GetElement(id) as Autodesk.Revit.DB.View;
              string parameterAsString1 = JsonExporter.GetParameterAsString((Element) element, "Sheet Number");
              string str1 = element.ViewType == ViewType.DrawingSheet ? JsonExporter.GetParameterAsString((Element) element, "Sheet Name") : element.Name;
              Dictionary<string, string> dictionary = new Dictionary<string, string>();
              string[] strArray = new string[5]
              {
                "TKT_STRUCT_CUYDS",
                "TKT_ARCH_VOL_1",
                "TKT_ARCH_VOL_2",
                "TKT_ARCH_VOL_3",
                "TKT_ARCH_VOL_4"
              };
              foreach (string str2 in strArray)
              {
                string parameterAsString2 = JsonExporter.GetParameterAsString((Element) element, str2);
                if (!string.IsNullOrEmpty(parameterAsString2))
                  dictionary.Add(str2, parameterAsString2);
              }
              string s;
              if (element.ViewType == ViewType.DrawingSheet)
                s = $"{Guid.NewGuid().ToString()}_Sheet {parameterAsString1} - {str1}";
              else
                s = $"{Guid.NewGuid().ToString()}_{str1}";
              string base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(s));
              string name;
              if (false)
              {
                name = base64String + ".dwg";
                ICollection<ElementId> views2 = (ICollection<ElementId>) new List<ElementId>()
                {
                  element.Id
                };
                DWGExportOptions dwgExportOptions = new DWGExportOptions();
                dwgExportOptions.ExportingAreas = true;
                dwgExportOptions.ExportOfSolids = SolidGeometry.Polymesh;
                dwgExportOptions.MergedViews = true;
                dwgExportOptions.FileVersion = ACADVersion.R2010;
                dwgExportOptions.TargetUnit = ExportUnit.Default;
                dwgExportOptions.TextTreatment = TextTreatment.Approximate;
                DWGExportOptions options = dwgExportOptions;
                document.Export(tempPath, name, views2, options);
              }
              else
              {
                name = base64String + ".dwf";
                ViewSet views3 = new ViewSet();
                views3.Insert(element);
                DWFExportOptions options = new DWFExportOptions()
                {
                  ExportObjectData = true,
                  ExportingAreas = true,
                  ExportTexture = true
                };
                document.Export(tempPath, name, views3, options);
              }
              EDGE.Cloud.Models.View view = new EDGE.Cloud.Models.View()
              {
                FileName = name,
                PropertyData = dictionary.Count == 0 ? (byte[]) null : Compressor.Compress((object) dictionary)
              };
              views1.Add(view);
            }
          }
          int num2 = (int) transaction.Commit();
        }
      }
      this.UploadViews(tempPath, views1, view3D, nonExportedElements, saveSheets);
    }
    return elementIdList1;
  }

  private static string GetAssemblyName(View3D view3D)
  {
    string assemblyName = string.Empty;
    if (view3D != null)
      assemblyName = !(view3D.Document.GetElement(view3D.AssociatedAssemblyInstanceId) is AssemblyInstance element) ? view3D.Name : element.Name;
    return assemblyName;
  }

  private string ToBase64(string text)
  {
    string base64 = text;
    if (!string.IsNullOrEmpty(text))
      base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
    return base64;
  }

  public async Task UploadViews(
    string savedLocation,
    List<EDGE.Cloud.Models.View> views,
    View3D view3D,
    List<ElementId> nonExportedElements,
    Func<View3D, List<ElementId>, List<EDGE.Cloud.Models.View>, IRestResponse> saveSheets)
  {
    BucketDetailsResponse edgeBucketAsync = await this.GetEdgeBucketAsync();
    string bucketKey = edgeBucketAsync.BucketKey;
    Dictionary<string, string> twoDUrns;
    if (string.IsNullOrEmpty(edgeBucketAsync.BucketKey))
    {
      if (string.IsNullOrEmpty((await this.CreateBucket()).BucketKey))
      {
        this.AsyncDialog.AddError($"{ForgeExporter.GetAssemblyName(view3D)}: Unable to successfully export this assembly. Please check your internet connection and try again.");
        this.AsyncDialog.AddStep();
        AsyncDialog.Error = true;
        bucketKey = (string) null;
        twoDUrns = (Dictionary<string, string>) null;
        return;
      }
    }
    twoDUrns = new Dictionary<string, string>();
    if (!savedLocation.EndsWith("\\"))
      savedLocation += "\\";
    foreach (EDGE.Cloud.Models.View view1 in views)
    {
      EDGE.Cloud.Models.View view = view1;
      string fileName = view.FileName;
      BucketCreationData bucketCreationData = new BucketCreationData(this._forgeBucket, BucketPolicyEnum.kTransient);
      Exception ex1 = (Exception) null;
      FileUploadInfo fromFile = FileUploadInfo.CreateFromFile(fileName, savedLocation + fileName, out ex1);
      if (ex1 != null)
      {
        if (ex1 is FileNotFoundException || ex1 is DirectoryNotFoundException || ex1 is ArgumentNullException)
          this.AsyncDialog.AddError($"{savedLocation + fileName}: Path was not found or was invalid.");
        if (ex1 is UnauthorizedAccessException)
          this.AsyncDialog.AddError(string.Format("{0}: User lacks permissions to export to selected path {0}", (object) (savedLocation + fileName)));
        this.AsyncDialog.AddStep();
        AsyncDialog.Error = true;
        bucketKey = (string) null;
        twoDUrns = (Dictionary<string, string>) null;
        return;
      }
      if (view3D == null)
        fromFile.Key = $"{Guid.NewGuid().ToString()}_{fromFile.Key}";
      string withoutExtension1 = Path.GetFileNameWithoutExtension(fromFile.Key);
      string extension = Path.GetExtension(fromFile.Key);
      fromFile.Key = this.ToBase64(withoutExtension1) + extension;
      ObjectDetailsResponse objectDetailsResponse = await ForgeExporter._forgeModel.UploadFileAsync(this._forgeBucket, fromFile);
      if (!objectDetailsResponse.IsOk())
      {
        this.AsyncDialog.AddError($"Unable to upload file: {objectDetailsResponse.Error}");
        bucketKey = (string) null;
        twoDUrns = (Dictionary<string, string>) null;
        return;
      }
      string objectKey = objectDetailsResponse.ObjectKey;
      string urn = ForgeModel.GetUrn(bucketKey, objectKey);
      string withoutExtension2 = Path.GetFileNameWithoutExtension(fileName);
      string str1 = withoutExtension2;
      try
      {
        bool flag = false;
        string str2 = Encoding.UTF8.GetString(Convert.FromBase64String(withoutExtension2));
        foreach (char ch in str2.ToCharArray())
        {
          if (ch >= 'z')
            flag = true;
        }
        if (!flag)
          str1 = str2;
        if (str1.IndexOf("_") == 36)
          str1 = str1.Substring(str1.IndexOf("_") + 1);
      }
      catch (Exception ex2)
      {
      }
      view.Name = str1;
      view.Urn = urn;
      view.Guid = objectKey;
      view.Is3d = false;
      if (!twoDUrns.ContainsKey(urn))
        twoDUrns.Add(urn, objectKey);
      fileName = (string) null;
      view = (EDGE.Cloud.Models.View) null;
    }
    foreach (string objectKey in twoDUrns.Values.ToList<string>())
    {
      if (!(await ForgeExporter._forgeModel.RegisterDerivatives(bucketKey, objectKey)).IsOk())
        this.AsyncDialog.AddError($"Registration failed: {view3D.Name}");
    }
    Func<View3D, List<ElementId>, List<EDGE.Cloud.Models.View>, IRestResponse> func = saveSheets;
    if (func == null)
    {
      bucketKey = (string) null;
      twoDUrns = (Dictionary<string, string>) null;
    }
    else
    {
      IRestResponse restResponse = func(view3D, nonExportedElements, views);
      bucketKey = (string) null;
      twoDUrns = (Dictionary<string, string>) null;
    }
  }

  private async Task Authenticate()
  {
    TokenResponse tokenResponse = await ForgeExporter._forgeModel.AuthenticateAsync();
    if (tokenResponse.IsOk())
      return;
    this.AsyncDialog.AddError($"Authentication failed: {tokenResponse.Error.Reason}");
  }

  private string GetBucketName(string serverUrl)
  {
    string bucketName = new RestClient(serverUrl).Execute((IRestRequest) new RestRequest("api/getConfig/forgeBucket", Method.GET)).Content;
    if (bucketName.Length > 2)
      bucketName = bucketName.Substring(1, bucketName.Length - 2);
    return bucketName;
  }

  private async Task<BucketDetailsResponse> CreateBucket()
  {
    BucketCreationData bucketData = new BucketCreationData(this._forgeBucket, BucketPolicyEnum.kTransient);
    BucketDetailsResponse bucketAsync = await ForgeExporter._forgeModel.CreateBucketAsync(bucketData);
    if (!bucketAsync.IsOk())
      this.AsyncDialog.AddError("Failed to create a new Forge bucket");
    return bucketAsync;
  }

  private async Task<BucketDetailsResponse> GetEdgeBucketAsync()
  {
    BucketDetailsResponse bucketDetailsAsync = await ForgeExporter._forgeModel.GetBucketDetailsAsync(this._forgeBucket);
    if (bucketDetailsAsync == null || !bucketDetailsAsync.IsOk())
      this.AsyncDialog.AddError("Failed to get the Forge bucket name " + this._forgeBucket);
    return bucketDetailsAsync;
  }

  private BucketDetailsResponse GetEdgeBucket()
  {
    BucketDetailsResponse bucketDetails = ForgeExporter._forgeModel.GetBucketDetails(this._forgeBucket);
    if (bucketDetails == null || !bucketDetails.IsOk())
      this.AsyncDialog.AddError("Failed to get the Forge bucket name " + this._forgeBucket);
    return bucketDetails;
  }
}
