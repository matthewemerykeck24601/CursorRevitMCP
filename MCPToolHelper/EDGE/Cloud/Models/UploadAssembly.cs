// Decompiled with JetBrains decompiler
// Type: EDGE.Cloud.Models.UploadAssembly
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

#nullable disable
namespace EDGE.Cloud.Models;

public class UploadAssembly : Assembly
{
  public string AuthenticateUrl { get; set; }

  public string ServiceUrl { get; set; }

  public string ServiceQuery { get; set; }

  public string Username { get; set; }

  public string Password { get; set; }

  public string ManufactureName { get; set; }

  public string PlantName { get; set; }

  public string ProjectName { get; set; }

  public string ProjectNumber { get; set; }

  public string ProjectGUID { get; set; }

  public byte[] ProjectData { get; set; }

  public List<View> Views { get; set; }

  public byte[] MetaData { get; set; }

  public byte[] GeometryData { get; set; }

  public byte[] PropertyData { get; set; }

  public byte[] MaterialData { get; set; }

  public bool AddToExisting { get; set; }

  public UploadAssembly()
  {
    this.ServiceUrl = "http://edgeservicemvc.azurewebsites.net/";
    this.ServiceQuery = "api/UploadAssembly";
    this.Views = new List<View>();
  }

  public static Task<IRestResponse> UploadAsync(UploadAssembly uploadAssembly, string token)
  {
    try
    {
      Task<IRestResponse> task = (Task<IRestResponse>) null;
      string serviceUrl = uploadAssembly.ServiceUrl;
      string resource = "api/UploadAssemblyData";
      RestClient restClient = new RestClient(serviceUrl);
      RestRequest request = new RestRequest(resource, Method.POST)
      {
        RequestFormat = DataFormat.Json
      };
      if (!string.IsNullOrEmpty(token))
      {
        request.AddHeader("Authorization", token);
        request.AddJsonBody((object) uploadAssembly);
        uploadAssembly.CreationDate = DateTime.Now;
        task = restClient.ExecuteTaskAsync((IRestRequest) request);
        task.Wait();
      }
      return task;
    }
    catch (Exception ex)
    {
      throw;
    }
  }

  public static IRestResponse GetTokenResponse(
    string authenticateUrl,
    string username,
    string password)
  {
    RestClient restClient = new RestClient(string.IsNullOrEmpty(authenticateUrl) ? "https://edgeauthentication.azurewebsites.net/" : authenticateUrl);
    RestRequest request = new RestRequest("/api/login", Method.POST);
    request.AddHeader("content-type", "application/json");
    string str = $"{{\n\t\"Username\": \"{username}\",\n\t\"Password\": \"{password}\"\n}}";
    request.AddParameter("application/json", (object) str, ParameterType.RequestBody);
    return restClient.Execute((IRestRequest) request);
  }

  public static string GetToken(string authenticateUrl = "", string username = "", string password = "")
  {
    string token = string.Empty;
    string content = UploadAssembly.GetTokenResponse(authenticateUrl, username, password).Content;
    if (!string.IsNullOrEmpty(content))
      token = UploadAssembly.ToObject<AuthenticationModel>(content).Token;
    return token;
  }

  public static List<Manufacture> GetProducers(string serviceUrl, string token)
  {
    List<Manufacture> producers = (List<Manufacture>) null;
    try
    {
      RestClient restClient = new RestClient(serviceUrl);
      RestRequest restRequest = new RestRequest("api/manufactures", Method.GET);
      restRequest.AddHeader("Authorization", token);
      RestRequest request = restRequest;
      IRestResponse restResponse = restClient.Execute((IRestRequest) request);
      if (restResponse.StatusCode == HttpStatusCode.OK)
        producers = JsonConvert.DeserializeObject<Model>(restResponse.Content).Manufactures.ToList<Manufacture>();
    }
    catch (Exception ex)
    {
    }
    return producers;
  }

  public static List<Plant> GetPlants(string serviceUrl, string token, int producerId)
  {
    List<Plant> plants = (List<Plant>) null;
    try
    {
      RestClient restClient = new RestClient(serviceUrl);
      RestRequest restRequest = new RestRequest($"/api/{producerId}", Method.GET);
      restRequest.AddHeader("Authorization", token);
      RestRequest request = restRequest;
      IRestResponse restResponse = restClient.Execute((IRestRequest) request);
      if (restResponse.StatusCode == HttpStatusCode.OK)
        plants = JsonConvert.DeserializeObject<Model>(restResponse.Content).Plants.ToList<Plant>();
    }
    catch (Exception ex)
    {
    }
    return plants;
  }

  public static List<Project> GetProjects(
    string serviceUrl,
    string token,
    int producerId,
    int plantId)
  {
    List<Project> projects = (List<Project>) null;
    try
    {
      RestClient restClient = new RestClient(serviceUrl);
      RestRequest restRequest = new RestRequest($"/api/{producerId}/{plantId}", Method.GET);
      restRequest.AddHeader("Authorization", token);
      RestRequest request = restRequest;
      IRestResponse restResponse = restClient.Execute((IRestRequest) request);
      if (restResponse.StatusCode == HttpStatusCode.OK)
        projects = JsonConvert.DeserializeObject<Model>(restResponse.Content).Projects.ToList<Project>();
    }
    catch (Exception ex)
    {
    }
    return projects;
  }

  public static List<Assembly> GetAssemblies(
    string serviceUrl,
    string token,
    int producerId,
    int plantId,
    int projectId)
  {
    List<Assembly> assemblies = (List<Assembly>) null;
    try
    {
      RestClient restClient = new RestClient(serviceUrl);
      RestRequest restRequest = new RestRequest($"/api/{producerId}/{plantId}/{projectId}", Method.GET);
      restRequest.AddHeader("Authorization", token);
      RestRequest request = restRequest;
      IRestResponse restResponse = restClient.Execute((IRestRequest) request);
      if (restResponse.StatusCode == HttpStatusCode.OK)
        assemblies = JsonConvert.DeserializeObject<Model>(restResponse.Content).Assemblies.ToList<Assembly>();
    }
    catch (Exception ex)
    {
    }
    return assemblies;
  }

  public static bool UploadAssemblyPermission(
    string serviceUrl,
    string token,
    string producerName,
    string plantName,
    string projectName,
    string projectNumber)
  {
    bool flag = false;
    try
    {
      RestClient restClient = new RestClient(serviceUrl);
      RestRequest restRequest = new RestRequest("api/uploadAssemblyPermission", Method.GET);
      restRequest.AddHeader("Authorization", token);
      restRequest.AddHeader("Manufacture", producerName);
      restRequest.AddHeader("Plant", plantName);
      restRequest.AddHeader("Project", projectName);
      restRequest.AddHeader("ProjectNumber", projectNumber);
      RestRequest request = restRequest;
      IRestResponse restResponse = restClient.Execute((IRestRequest) request);
      if (restResponse.StatusCode == HttpStatusCode.OK)
        flag = JsonConvert.DeserializeObject<bool>(restResponse.Content);
    }
    catch (Exception ex)
    {
    }
    return flag;
  }

  private static string ToJson(object obj)
  {
    string empty = string.Empty;
    return JsonConvert.SerializeObject(obj);
  }

  public static T ToObject<T>(string json) => JsonConvert.DeserializeObject<T>(json);
}
