// Decompiled with JetBrains decompiler
// Type: EDGE.Cloud.JsonExportContext
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.Cloud.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using Utils.ElementUtils;
using Utils.MiscUtils;
using Utils.SettingsUtils;

#nullable disable
namespace EDGE.Cloud;

internal class JsonExportContext : IExportContext
{
  private double _scale_bim = 1.0;
  private double _scale_vertex = 1.0;
  private bool _switch_coordinates = true;
  private Document _document;
  private AssemblyInstance _assemblyInstance;
  private string _fileName;
  private Func<string, byte[][], IRestResponse> _saveModel;
  private JsonContainer _container;
  private Dictionary<string, JsonMaterial> _materials;
  private Dictionary<string, JsonObject> _objects;
  private Dictionary<string, JsonGeometry> _geometries;
  private JsonObject _currentElement;
  private Dictionary<string, JsonObject> _currentObject;
  private Dictionary<string, JsonGeometry> _currentGeometry;
  private Dictionary<string, JsonExportContext.VertexLookupInt> _vertices;
  private bool _noErrorOnElementEnd;
  private ElementId _elementId;
  private Stack<ElementId> _elementStack = new Stack<ElementId>();
  private Stack<Transform> _transformationStack = new Stack<Transform>();
  private string _currentMaterialUid;
  private Dictionary<string, string> _chartDict;
  private Dictionary<string, Dictionary<string, string>> _nodeModel;
  private Dictionary<string, Dictionary<string, string>> _nodeRebars;
  private Dictionary<string, Dictionary<string, string>> _nodeMeshes;
  private Dictionary<string, Dictionary<string, string>> _nodeEmbeds;
  private Dictionary<string, Dictionary<string, string>> _nodeLiftings;
  private Dictionary<string, Dictionary<string, string>> _nodeStrands;
  private Dictionary<string, Dictionary<string, string>> _nodeConcretes;
  private Dictionary<string, Dictionary<string, string>> _nodeFraming;
  private Dictionary<string, Dictionary<string, string>> _nodeWallFinishes;
  private Dictionary<string, Dictionary<string, string>> _nodeFormliners;
  private Dictionary<string, Dictionary<string, string>> _nodeOthers;
  private string newMaterial;

  private JsonObject CurrentObjectPerMaterial => this._currentObject[this._currentMaterialUid];

  private JsonGeometry CurrentGeometryPerMaterial
  {
    get => this._currentGeometry[this._currentMaterialUid];
  }

  private JsonExportContext.VertexLookupInt CurrentVerticesPerMaterial
  {
    get => this._vertices[this._currentMaterialUid];
  }

  private Transform CurrentTransform => this._transformationStack.Peek();

  public List<ElementId> NotToExportElements { get; set; }

  ~JsonExportContext()
  {
    if (this._objects == null)
      return;
    this._objects.Clear();
    this._materials.Clear();
    this._geometries.Clear();
    this._currentObject.Clear();
    this._currentGeometry.Clear();
    this._elementStack.Clear();
    this._transformationStack.Clear();
  }

  public JsonExportContext(Document document, string filename)
  {
    this._document = document;
    this._fileName = filename;
    this.NotToExportElements = new List<ElementId>();
  }

  public JsonExportContext(
    AssemblyInstance assemblyInstance,
    Func<string, byte[][], IRestResponse> saveModel)
  {
    this._assemblyInstance = assemblyInstance;
    this._document = assemblyInstance.Document;
    this._saveModel = saveModel;
    this.NotToExportElements = new List<ElementId>();
  }

  public bool Start()
  {
    this._materials = new Dictionary<string, JsonMaterial>();
    this._geometries = new Dictionary<string, JsonGeometry>();
    this._objects = new Dictionary<string, JsonObject>();
    this._chartDict = new Dictionary<string, string>();
    this._nodeModel = new Dictionary<string, Dictionary<string, string>>();
    this._nodeRebars = new Dictionary<string, Dictionary<string, string>>();
    this._nodeMeshes = new Dictionary<string, Dictionary<string, string>>();
    this._nodeEmbeds = new Dictionary<string, Dictionary<string, string>>();
    this._nodeLiftings = new Dictionary<string, Dictionary<string, string>>();
    this._nodeStrands = new Dictionary<string, Dictionary<string, string>>();
    this._nodeConcretes = new Dictionary<string, Dictionary<string, string>>();
    this._nodeFraming = new Dictionary<string, Dictionary<string, string>>();
    this._nodeWallFinishes = new Dictionary<string, Dictionary<string, string>>();
    this._nodeFormliners = new Dictionary<string, Dictionary<string, string>>();
    this._nodeOthers = new Dictionary<string, Dictionary<string, string>>();
    this._transformationStack.Push(Transform.Identity);
    return true;
  }

  private void UpdateKeyValue(
    string mainKey,
    string key,
    string value,
    Dictionary<string, Dictionary<string, string>> dictionary)
  {
    if (!dictionary.ContainsKey(mainKey))
      return;
    Dictionary<string, string> dictionary1 = dictionary[mainKey];
    if (dictionary1.ContainsKey(key))
      dictionary1[key] = value;
    else
      dictionary1.Add(key, value);
  }

  private void UpdateLocationInForm(
    string locationInForm,
    List<ElementId> elementIds,
    Dictionary<string, Dictionary<string, string>> dictionary)
  {
    string key = "LocationInForm";
    foreach (ElementId elementId in elementIds)
      this.UpdateKeyValue(JsonExporter.GetGuid(this._document.GetElement(elementId).UniqueId), key, locationInForm, dictionary);
  }

  private void UpdateLocationInForm(string locationInForm, List<ElementId> elementIds)
  {
    this.UpdateLocationInForm(locationInForm, elementIds, this._nodeConcretes);
    this.UpdateLocationInForm(locationInForm, elementIds, this._nodeFraming);
    this.UpdateLocationInForm(locationInForm, elementIds, this._nodeEmbeds);
    this.UpdateLocationInForm(locationInForm, elementIds, this._nodeLiftings);
    this.UpdateLocationInForm(locationInForm, elementIds, this._nodeMeshes);
    this.UpdateLocationInForm(locationInForm, elementIds, this._nodeRebars);
    this.UpdateLocationInForm(locationInForm, elementIds, this._nodeStrands);
    this.UpdateLocationInForm(locationInForm, elementIds, this._nodeWallFinishes);
    this.UpdateLocationInForm(locationInForm, elementIds, this._nodeFormliners);
  }

  private void MoveChildToParent(
    string childId,
    string parentId,
    Dictionary<string, Dictionary<string, string>> nodeFrom,
    Dictionary<string, Dictionary<string, string>> nodeTo)
  {
    if (!nodeTo.ContainsKey(parentId) || !nodeFrom.ContainsKey(childId))
      return;
    Dictionary<string, string> dictionary = nodeFrom[childId];
    nodeTo.Add(childId, dictionary);
    nodeFrom.Remove(childId);
  }

  private void MoveChildToParent(string childId, string parentId)
  {
    this.MoveChildToParent(childId, parentId, this._nodeRebars, this._nodeEmbeds);
    this.MoveChildToParent(childId, parentId, this._nodeRebars, this._nodeLiftings);
    this.MoveChildToParent(childId, parentId, this._nodeEmbeds, this._nodeRebars);
    this.MoveChildToParent(childId, parentId, this._nodeEmbeds, this._nodeLiftings);
    this.MoveChildToParent(childId, parentId, this._nodeLiftings, this._nodeRebars);
    this.MoveChildToParent(childId, parentId, this._nodeLiftings, this._nodeEmbeds);
  }

  public void Finish()
  {
    try
    {
      if (!this._noErrorOnElementEnd)
        this.ElementEnd(this._elementId);
      double searchDistance = 1.0;
      List<ElementId> elementIds1 = new List<ElementId>();
      List<ElementId> elementIds2 = new List<ElementId>();
      List<ElementId> elementIds3 = new List<ElementId>();
      Document document = this._assemblyInstance.Document;
      LocationInFormAnalyzer locationInFormAnalyzer = new LocationInFormAnalyzer(this._assemblyInstance, searchDistance);
      AutoTicketLIF locationInFormValues = LocationInFormAnalyzer.extractLocationInFormValues(document);
      foreach (ElementId memberId in (IEnumerable<ElementId>) this._assemblyInstance.GetMemberIds())
      {
        Element element = document.GetElement(memberId);
        string parameterAsString;
        if ((parameterAsString = Parameters.GetParameterAsString(element, "LOCATION_IN_FORM")) != "")
        {
          if (parameterAsString == locationInFormValues.TIF || parameterAsString == locationInFormValues.defaultTIF)
            elementIds2.Add(memberId);
          else if (parameterAsString == locationInFormValues.SIF || parameterAsString == locationInFormValues.defaultSIF)
            elementIds1.Add(memberId);
          else if (parameterAsString == locationInFormValues.BIF || parameterAsString == locationInFormValues.defaultBIF)
            elementIds3.Add(memberId);
        }
        else if (locationInFormAnalyzer.ElementsInTopFaces.Contains(memberId))
          elementIds2.Add(memberId);
        else if (locationInFormAnalyzer.ElementsInSideFaces.Contains(memberId))
          elementIds1.Add(memberId);
        else if (locationInFormAnalyzer.ElementsInDownFaces.Contains(memberId))
          elementIds3.Add(memberId);
      }
      this.UpdateLocationInForm("Top", elementIds2);
      this.UpdateLocationInForm("Side", elementIds1);
      this.UpdateLocationInForm("Down", elementIds3);
      JsonChart chart = JsonExporter.GetChart(this._document);
      Dictionary<string, string> paramData = JsonExporter.GetParamData(JsonExporter.GetStructuralFraming(this._assemblyInstance));
      IDictionary<string, string> parentOfElements = JsonExporter.GetParentOfElements(this._assemblyInstance);
      JsonContainer jsonContainer1 = new JsonContainer();
      jsonContainer1.metadata = new JsonMetadata()
      {
        type = "Object",
        version = 1.0,
        generator = "Revit exporter",
        file = this._document.PathName,
        chart = chart,
        notes = "",
        structuralFraming = (IDictionary<string, string>) paramData,
        parentOfChildren = parentOfElements,
        nodeConcretes = this._nodeConcretes,
        nodeFraming = this._nodeFraming,
        nodeEmbeds = this._nodeEmbeds,
        nodeLiftings = this._nodeLiftings,
        nodeMeshes = this._nodeMeshes,
        nodeRebars = this._nodeRebars,
        nodeStrands = this._nodeStrands,
        nodeWallFinishes = this._nodeWallFinishes,
        nodeFormliners = this._nodeFormliners,
        nodeOthers = this._nodeOthers
      };
      JsonContainer jsonContainer2 = jsonContainer1;
      JsonObject jsonObject1 = new JsonObject();
      jsonObject1.uuid = this._document.ActiveView.UniqueId;
      jsonObject1.name = "BIM " + this._document.Title;
      jsonObject1.type = "Scene";
      JsonObject jsonObject2 = jsonObject1;
      double[] numArray1 = new double[16 /*0x10*/];
      numArray1[0] = this._scale_bim;
      numArray1[5] = this._scale_bim;
      numArray1[10] = this._scale_bim;
      numArray1[15] = this._scale_bim;
      jsonObject2.matrix = numArray1;
      JsonObject jsonObject3 = jsonObject1;
      jsonContainer2.obj = jsonObject3;
      this._container = jsonContainer1;
      this._container.obj.geometries = this._geometries.Values.ToList<JsonGeometry>();
      this._container.geometries = this._container.obj.geometries;
      this._container.obj.materials = this._materials.Values.ToList<JsonMaterial>();
      this._container.materials = this._container.obj.materials;
      this._container.obj.children = this._objects.Values.ToList<JsonObject>();
      JsonMetadata metadata = this._container.metadata;
      List<JsonGeometry> geometries = this._container.geometries;
      List<JsonObject> children = this._container.obj.children;
      List<JsonMaterial> materials = this._container.materials;
      if (this._saveModel == null)
      {
        using (FileStream fileStream = File.OpenWrite(this._fileName))
          new DataContractJsonSerializer(typeof (JsonContainer)).WriteObject((Stream) fileStream, (object) this._container);
      }
      else
      {
        ProjectData projectData = new ProjectData()
        {
          Chart = chart.ToString()
        };
        foreach (JsonGeometry jsonGeometry in geometries)
        {
          string key = jsonGeometry.uuid;
          if (!string.IsNullOrEmpty(key) && key.Length > 45)
            key = key.Substring(0, 36);
          string parentId = parentOfElements.ContainsKey(key) ? parentOfElements[key] : "";
          if (!string.IsNullOrEmpty(parentId))
          {
            jsonGeometry.parentId = parentId;
            geometries.Where<JsonGeometry>((Func<JsonGeometry, bool>) (g => g.uuid.Substring(0, 36) == parentId)).FirstOrDefault<JsonGeometry>();
          }
        }
        foreach (KeyValuePair<string, string> keyValuePair in (IEnumerable<KeyValuePair<string, string>>) parentOfElements)
          this.MoveChildToParent(keyValuePair.Key, keyValuePair.Value);
        string key1 = "LocationInForm";
        foreach (KeyValuePair<string, Dictionary<string, string>> keyValuePair in this._nodeModel)
        {
          if (keyValuePair.Value.ContainsKey(key1))
          {
            string key2 = keyValuePair.Key;
            string str = keyValuePair.Value[key1];
            if (parentOfElements.ContainsKey(key2))
            {
              string key3 = parentOfElements[key2];
              if (this._nodeModel.ContainsKey(key3))
              {
                Dictionary<string, string> dictionary = this._nodeModel[key3];
                if (dictionary.ContainsKey(key1))
                  keyValuePair.Value[key1] = dictionary[key1];
              }
            }
          }
        }
        byte[] numArray2 = Compressor.Compress((object) projectData);
        byte[] numArray3 = Compressor.Compress((object) metadata);
        byte[] numArray4 = Compressor.Compress((object) geometries);
        byte[] numArray5 = Compressor.Compress((object) children);
        byte[] numArray6 = Compressor.Compress((object) materials);
        Func<string, byte[][], IRestResponse> saveModel = this._saveModel;
        if (saveModel == null)
          return;
        IRestResponse restResponse = saveModel(this._assemblyInstance.Name, new byte[5][]
        {
          numArray2,
          numArray3,
          numArray4,
          numArray5,
          numArray6
        });
      }
    }
    catch (Exception ex)
    {
      throw;
    }
  }

  public bool IsCanceled() => false;

  public RenderNodeAction OnElementBegin(ElementId elementId)
  {
    try
    {
      this._elementId = elementId;
      if (!this.NotToExportElements.Contains(elementId))
      {
        Element element = this._document.GetElement(elementId);
        string guid = JsonExporter.GetGuid(element.UniqueId);
        if (this._objects.ContainsKey(guid) || element.Category == null)
          return RenderNodeAction.Skip;
        this._elementStack.Push(elementId);
        ICollection<ElementId> materialIds = element.GetMaterialIds(false);
        element.GetMaterialIds(true);
        int count = materialIds.Count;
        this._currentElement = new JsonObject()
        {
          name = Util.ElementDescription(element),
          material = this._currentMaterialUid,
          matrix = new double[16 /*0x10*/]
          {
            1.0,
            0.0,
            0.0,
            0.0,
            0.0,
            1.0,
            0.0,
            0.0,
            0.0,
            0.0,
            1.0,
            0.0,
            0.0,
            0.0,
            0.0,
            1.0
          },
          type = "RevitElement",
          uuid = guid
        };
        this._currentObject = new Dictionary<string, JsonObject>();
        this._currentGeometry = new Dictionary<string, JsonGeometry>();
        this._vertices = new Dictionary<string, JsonExportContext.VertexLookupInt>();
        if (element.Category != null)
        {
          if (element.Category.Material != null)
            this.SetCurrentMaterial(element.Category.Material.UniqueId);
        }
      }
    }
    catch (Exception ex)
    {
      throw;
    }
    return RenderNodeAction.Proceed;
  }

  public void OnElementEnd(ElementId id)
  {
    if (this.NotToExportElements.Contains(id))
      return;
    this._noErrorOnElementEnd = true;
    this.ElementEnd(id);
  }

  private void ElementEnd(ElementId id)
  {
    try
    {
      Element element = this._document.GetElement(id);
      string guid = JsonExporter.GetGuid(element.UniqueId);
      if (this._objects.ContainsKey(guid) || element.Category == null)
        return;
      List<string> list = this._vertices.Keys.ToList<string>();
      this._currentElement.children = new List<JsonObject>(list.Count);
      foreach (string key in list)
      {
        JsonObject jsonObject = this._currentObject[key];
        JsonGeometry jsonGeometry = this._currentGeometry[key];
        foreach (KeyValuePair<JsonExportContext.PointInt, int> keyValuePair in (Dictionary<JsonExportContext.PointInt, int>) this._vertices[key])
        {
          jsonGeometry.data.vertices.Add(this._scale_vertex * (double) keyValuePair.Key.X);
          jsonGeometry.data.vertices.Add(this._scale_vertex * (double) keyValuePair.Key.Y);
          jsonGeometry.data.vertices.Add(this._scale_vertex * (double) keyValuePair.Key.Z);
        }
        jsonObject.geometry = jsonGeometry.uuid;
        if (!this._geometries.ContainsKey(jsonGeometry.uuid))
          this._geometries.Add(jsonGeometry.uuid, jsonGeometry);
        this._currentElement.children.Add(jsonObject);
      }
      Dictionary<string, string> elementProperties = Util.GetElementProperties(element, true);
      this._currentElement.userData = elementProperties;
      if (!this._currentElement.userData.ContainsKey("ElementId"))
        this._currentElement.userData.Add("ElementId", element.Id.ToString());
      if (!this._currentElement.userData.ContainsKey("UniqueId"))
        this._currentElement.userData.Add("UniqueId", element.UniqueId);
      if (!this._objects.ContainsKey(this._currentElement.uuid))
        this._objects.Add(this._currentElement.uuid, this._currentElement);
      this._elementStack.Pop();
      string[] strArray = new string[4]
      {
        "TICKET_REINFORCED_DATE_CURRENT",
        "TICKET_DETAILED_DATE_CURRENT",
        "TICKET_CREATED_DATE_INITIAL",
        "TICKET_RELEASED_DATE_CURRENT"
      };
      foreach (string str in strArray)
      {
        string parameterAsString = JsonExporter.GetParameterAsString(element, str);
        if (!string.IsNullOrEmpty(parameterAsString))
          this._chartDict.Add(str, parameterAsString);
      }
      Dictionary<string, string> dictionary = elementProperties;
      this._nodeModel[guid] = dictionary;
      string str1 = dictionary.ContainsKey("Type Structural Material") ? dictionary["Type Structural Material"] : "";
      if (string.IsNullOrEmpty(str1))
      {
        str1 = dictionary.ContainsKey("Material") ? dictionary["Material"] : "";
        if (string.IsNullOrEmpty(str1))
        {
          str1 = dictionary.ContainsKey("Structural Material") ? dictionary["Structural Material"] : "";
          if (string.IsNullOrEmpty(str1))
            str1 = dictionary.ContainsKey("TYPE_Structural Material") ? dictionary["TYPE_Structural Material"] : "";
        }
      }
      string lower1 = str1.ToLower();
      string name = element.Category.Name;
      string str2 = dictionary.ContainsKey("MANUFACTURE_COMPONENT") ? dictionary["MANUFACTURE_COMPONENT"] : "";
      if (string.IsNullOrEmpty(str2))
      {
        str2 = dictionary.ContainsKey("TYPE_MANUFACTURE_COMPONENT") ? dictionary["TYPE_MANUFACTURE_COMPONENT"] : "";
        if (string.IsNullOrEmpty(str2))
          str2 = dictionary.ContainsKey("Type MANUFACTURE_COMPONENT") ? dictionary["Type MANUFACTURE_COMPONENT"] : "";
      }
      string lower2 = str2.ToLower();
      if (lower2.IndexOf("lift") > -1)
        this._nodeLiftings[guid] = dictionary;
      else if (lower2.IndexOf("rebar") > -1)
        this._nodeRebars[guid] = dictionary;
      else if (lower2.IndexOf("mesh") > -1 || lower2.IndexOf("wwf") > -1 || lower2.IndexOf("cgrid") > -1 || lower2.IndexOf("sheargrid") > -1)
        this._nodeMeshes[guid] = dictionary;
      else if (lower2.IndexOf("strand") > -1)
        this._nodeStrands[guid] = dictionary;
      else if (lower2.IndexOf("formliner") > -1)
        this._nodeFormliners[guid] = dictionary;
      else if (lower2.IndexOf("embed") > -1 || lower2.IndexOf("raw consumable") > -1 || lower2.IndexOf("insulation") > -1 || lower2.IndexOf("erection") > -1 || lower2.IndexOf("wood nailer") > -1)
        this._nodeEmbeds[guid] = dictionary;
      else if (lower1.IndexOf("precast concrete") > -1)
      {
        if (name == Category.GetCategory(this._document, BuiltInCategory.OST_StructuralFraming).Name)
          this._nodeFraming[guid] = dictionary;
        this._nodeConcretes[guid] = dictionary;
      }
      else if (name == Category.GetCategory(this._document, BuiltInCategory.OST_Walls).Name)
        this._nodeWallFinishes[guid] = dictionary;
      else
        this._nodeOthers[guid] = dictionary;
    }
    catch (Exception ex)
    {
      throw;
    }
  }

  public RenderNodeAction OnInstanceBegin(InstanceNode node)
  {
    this._transformationStack.Push(this.CurrentTransform.Multiply(node.GetTransform()));
    return RenderNodeAction.Proceed;
  }

  public void OnInstanceEnd(InstanceNode node) => this._transformationStack.Pop();

  public RenderNodeAction OnLinkBegin(LinkNode node)
  {
    this._transformationStack.Push(this.CurrentTransform.Multiply(node.GetTransform()));
    return RenderNodeAction.Proceed;
  }

  public void OnLinkEnd(LinkNode node) => this._transformationStack.Pop();

  public void OnMaterial(MaterialNode node)
  {
    try
    {
      if (this.NotToExportElements.Contains(this._elementId))
        return;
      if (ElementId.InvalidElementId != node.MaterialId)
      {
        Element element = this._document.GetElement(node.MaterialId);
        if (element != null)
        {
          this.SetCurrentMaterial(element.UniqueId);
          this.newMaterial = element.UniqueId;
        }
        else
          this.SetCurrentMaterial(this.newMaterial);
      }
      else
      {
        int num = Util.ColorToInt(node.Color);
        string str = $"MaterialNode_{num}_{Util.RealString(node.Transparency * 100.0)}";
        if (!this._materials.ContainsKey(str))
        {
          JsonMaterial jsonMaterial = new JsonMaterial()
          {
            uuid = str,
            type = "MeshPhongMaterial",
            color = num,
            ambient = num,
            emissive = 0,
            specular = num,
            shininess = node.Glossiness,
            opacity = 1.0 - node.Transparency,
            transparent = 0.0 < node.Transparency,
            wireframe = false
          };
          if (!this._materials.ContainsKey(str))
            this._materials.Add(str, jsonMaterial);
        }
        this.SetCurrentMaterial(str);
      }
    }
    catch (Exception ex)
    {
      throw;
    }
  }

  public void OnPolymesh(PolymeshTopology polymesh)
  {
    try
    {
      if (this.NotToExportElements.Contains(this._elementId))
        return;
      IList<XYZ> points = polymesh.GetPoints();
      if (points == null)
        return;
      Transform t = this.CurrentTransform;
      IList<XYZ> list = (IList<XYZ>) points.Select<XYZ, XYZ>((Func<XYZ, XYZ>) (p => t.OfPoint(p))).ToList<XYZ>();
      if (polymesh.GetFacets() == null)
        return;
      foreach (PolymeshFacet facet in (IEnumerable<PolymeshFacet>) polymesh.GetFacets())
      {
        int num1 = this.CurrentVerticesPerMaterial.AddVertex(new JsonExportContext.PointInt(list[facet.V1], this._switch_coordinates));
        int num2 = this.CurrentVerticesPerMaterial.AddVertex(new JsonExportContext.PointInt(list[facet.V2], this._switch_coordinates));
        int num3 = this.CurrentVerticesPerMaterial.AddVertex(new JsonExportContext.PointInt(list[facet.V3], this._switch_coordinates));
        this.CurrentGeometryPerMaterial.data.faces.Add(0);
        this.CurrentGeometryPerMaterial.data.faces.Add(num1);
        this.CurrentGeometryPerMaterial.data.faces.Add(num2);
        this.CurrentGeometryPerMaterial.data.faces.Add(num3);
      }
    }
    catch (Exception ex)
    {
      throw;
    }
  }

  public RenderNodeAction OnViewBegin(ViewNode node) => RenderNodeAction.Proceed;

  public void OnViewEnd(ElementId elementId)
  {
  }

  public RenderNodeAction OnFaceBegin(FaceNode node) => RenderNodeAction.Proceed;

  public void OnFaceEnd(FaceNode node)
  {
  }

  public void OnRPC(RPCNode node)
  {
  }

  public void OnLight(LightNode node)
  {
  }

  private void SetCurrentMaterial(string uidMaterial)
  {
    if (!this._materials.ContainsKey(uidMaterial))
    {
      Material element = this._document.GetElement(uidMaterial) as Material;
      int num = Util.ColorToInt(element.Color);
      JsonMaterial jsonMaterial = new JsonMaterial()
      {
        uuid = uidMaterial,
        name = element.Name,
        type = "MeshPhongMaterial",
        color = num,
        ambient = num,
        emissive = 0,
        specular = num,
        shininess = 1,
        opacity = 0.01 * (double) (100 - element.Transparency),
        transparent = 0 < element.Transparency,
        wireframe = false
      };
      this._materials.Add(uidMaterial, jsonMaterial);
    }
    this._currentMaterialUid = uidMaterial;
    string str = $"{this._currentElement.uuid}-{uidMaterial}";
    if (!this._currentObject.ContainsKey(uidMaterial))
      this._currentObject.Add(uidMaterial, new JsonObject()
      {
        name = this._currentElement.name,
        geometry = str,
        material = this._currentMaterialUid,
        matrix = new double[16 /*0x10*/]
        {
          1.0,
          0.0,
          0.0,
          0.0,
          0.0,
          1.0,
          0.0,
          0.0,
          0.0,
          0.0,
          1.0,
          0.0,
          0.0,
          0.0,
          0.0,
          1.0
        },
        type = "Mesh",
        uuid = str
      });
    if (!this._currentGeometry.ContainsKey(uidMaterial))
      this._currentGeometry.Add(uidMaterial, new JsonGeometry()
      {
        uuid = str,
        type = "Geometry",
        data = new JsonGeometryData()
        {
          faces = new List<int>(),
          vertices = new List<double>(),
          normals = new List<double>(),
          uvs = new List<double>(),
          visible = true,
          castShadow = true,
          receiveShadow = false,
          doubleSided = true,
          scale = 1.0
        }
      });
    if (this._vertices.ContainsKey(uidMaterial))
      return;
    this._vertices.Add(uidMaterial, new JsonExportContext.VertexLookupInt());
  }

  public override string ToString()
  {
    string str = string.Empty;
    if (this._container != null)
      str = JsonConvert.SerializeObject((object) this._container, Formatting.None, new JsonSerializerSettings()
      {
        NullValueHandling = NullValueHandling.Ignore
      });
    return str;
  }

  internal class PointInt : IComparable<JsonExportContext.PointInt>
  {
    private const double _eps = 1E-09;
    private const double _feet_to_mm = 304.79999999999995;

    public long X { get; set; }

    public long Y { get; set; }

    public long Z { get; set; }

    private static long ConvertFeetToMillimetres(double d)
    {
      return 0.0 < d ? (1E-09 <= d ? (long) (1524.0 / 5.0 * d + 0.5) : 0L) : (1E-09 <= -d ? (long) (1524.0 / 5.0 * d - 0.5) : 0L);
    }

    public PointInt(XYZ p, bool switch_coordinates)
    {
      this.X = JsonExportContext.PointInt.ConvertFeetToMillimetres(p.X);
      this.Y = JsonExportContext.PointInt.ConvertFeetToMillimetres(p.Y);
      this.Z = JsonExportContext.PointInt.ConvertFeetToMillimetres(p.Z);
      if (!switch_coordinates)
        return;
      this.X = -this.X;
      long y = this.Y;
      this.Y = this.Z;
      this.Z = y;
    }

    public int CompareTo(JsonExportContext.PointInt a)
    {
      long num = this.X - a.X;
      if (num == 0L)
      {
        num = this.Y - a.Y;
        if (num == 0L)
          num = this.Z - a.Z;
      }
      if (num == 0L)
        return 0;
      return 0L >= num ? -1 : 1;
    }
  }

  internal class VertexLookupInt : Dictionary<JsonExportContext.PointInt, int>
  {
    public VertexLookupInt()
      : base((IEqualityComparer<JsonExportContext.PointInt>) new JsonExportContext.VertexLookupInt.PointIntEqualityComparer())
    {
    }

    public int AddVertex(JsonExportContext.PointInt p)
    {
      return !this.ContainsKey(p) ? (this[p] = this.Count) : this[p];
    }

    private class PointIntEqualityComparer : IEqualityComparer<JsonExportContext.PointInt>
    {
      public bool Equals(JsonExportContext.PointInt p, JsonExportContext.PointInt q)
      {
        return p.CompareTo(q) == 0;
      }

      public int GetHashCode(JsonExportContext.PointInt p)
      {
        return $"{p.X.ToString()},{p.Y.ToString()},{p.Z.ToString()}".GetHashCode();
      }
    }
  }

  internal class VertexLookupXyz : Dictionary<XYZ, int>
  {
    public VertexLookupXyz()
      : base((IEqualityComparer<XYZ>) new JsonExportContext.VertexLookupXyz.XyzEqualityComparer())
    {
    }

    public int AddVertex(XYZ p) => !this.ContainsKey(p) ? (this[p] = this.Count) : this[p];

    private class XyzEqualityComparer : IEqualityComparer<XYZ>
    {
      private const double _sixteenthInchInFeet = 0.005208333333333333;

      public bool Equals(XYZ p, XYZ q) => p.IsAlmostEqualTo(q, 1.0 / 192.0);

      public int GetHashCode(XYZ p) => Util.PointString(p).GetHashCode();
    }
  }
}
