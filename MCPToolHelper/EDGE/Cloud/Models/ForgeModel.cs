// Decompiled with JetBrains decompiler
// Type: EDGE.Cloud.Models.ForgeModel
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using EDGE.Cloud.Models.DataContracts;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#nullable disable
namespace EDGE.Cloud.Models;

public class ForgeModel
{
  private RestClient _restClient;
  private RestClient _forgeClient;
  private bool _useForge;
  private string _clientKey;
  private string _secretKey;

  public event OnTokenRefreshedHandler OnTokenRefreshed;

  public ForgeModel(string serviceUrl, string serverUrl)
  {
    this._useForge = false;
    this.TokenResponse = new TokenResponse();
    this._forgeClient = new RestClient(serviceUrl);
    this._restClient = new RestClient(serverUrl);
  }

  public ForgeModel(string serviceUrl, string clientKey, string secretKey, bool autoRefresh = true)
  {
    this._useForge = true;
    this._clientKey = clientKey;
    this._secretKey = secretKey;
    this.AutoRefresh = autoRefresh;
    this.TokenResponse = new TokenResponse();
    this._forgeClient = new RestClient(serviceUrl);
  }

  public bool AutoRefresh { get; set; }

  public TokenResponse TokenResponse { get; private set; }

  private Task<TokenResponse> GetAccessTokenAsync(string scope = "")
  {
    if (this._useForge)
    {
      RestRequest request = new RestRequest("authentication/v1/authenticate", Method.POST);
      request.AddParameter("client_id", (object) this._clientKey);
      request.AddParameter("client_secret", (object) this._secretKey);
      request.AddParameter("grant_type", (object) "client_credentials");
      request.AddParameter(nameof (scope), (object) scope);
      return this._forgeClient.ExecuteAsync<TokenResponse>(request);
    }
    RestRequest request1 = new RestRequest("api/getAutodeskForgeToken", Method.POST);
    request1.AddHeader(nameof (scope), scope);
    return this._restClient.ExecuteAsync<TokenResponse>(request1);
  }

  private TokenResponse GetAccessToken(string scope = "")
  {
    if (this._useForge)
    {
      RestRequest request = new RestRequest("authentication/v1/authenticate", Method.POST);
      request.AddParameter("client_id", (object) this._clientKey);
      request.AddParameter("client_secret", (object) this._secretKey);
      request.AddParameter("grant_type", (object) "client_credentials");
      request.AddParameter(nameof (scope), (object) scope);
      return this._forgeClient.Execute<TokenResponse>((IRestRequest) request).Data;
    }
    RestRequest request1 = new RestRequest("api/getAutodeskForgeToken", Method.POST);
    request1.AddHeader(nameof (scope), scope);
    return this._restClient.Execute<TokenResponse>((IRestRequest) request1).Data;
  }

  public async Task<TokenResponse> AuthenticateAsync()
  {
    TokenResponse tokenResponse = await this.GetAccessTokenAsync();
    if (!tokenResponse.IsOk())
      return tokenResponse;
    new Thread((ThreadStart) (() =>
    {
      Thread.CurrentThread.IsBackground = true;
      Thread.Sleep(TimeSpan.FromSeconds((double) tokenResponse.ExpirationTime));
      if (!this.AutoRefresh)
        return;
      this.AuthenticateAsync().Wait();
      if (this.OnTokenRefreshed == null)
        return;
      this.OnTokenRefreshed(this.TokenResponse);
    })).Start();
    this.TokenResponse = tokenResponse;
    return tokenResponse;
  }

  public async Task<BucketDetailsResponse> CreateBucketAsync(BucketCreationData bucketData)
  {
    this.TokenResponse = await this.GetAccessTokenAsync("bucket:create");
    RestRequest request = new RestRequest("oss/v2/buckets", Method.POST);
    if (this.TokenResponse != null)
      request.AddHeader("Authorization", "Bearer " + this.TokenResponse.AccessToken);
    request.AddHeader("Content-Type", "application/json");
    request.AddParameter("application/json", (object) bucketData.ToJsonString(), RestSharp.ParameterType.RequestBody);
    return await this._forgeClient.ExecuteAsync<BucketDetailsResponse>(request);
  }

  public async Task<BucketDetailsResponse> GetBucketDetailsAsync(string bucketKey)
  {
    this.TokenResponse = await this.GetAccessTokenAsync("bucket:read");
    RestRequest request = new RestRequest($"oss/v2/buckets/{bucketKey.ToLower()}/details", Method.GET);
    if (this.TokenResponse != null)
      request.AddHeader("Authorization", "Bearer " + this.TokenResponse.AccessToken);
    request.AddHeader("Content-Type", "application/json");
    return await this._forgeClient.ExecuteAsync<BucketDetailsResponse>(request);
  }

  public BucketDetailsResponse GetBucketDetails(string bucketKey)
  {
    this.TokenResponse = this.GetAccessToken("bucket:read");
    RestRequest request = new RestRequest($"oss/v2/buckets/{bucketKey.ToLower()}/details", Method.GET);
    if (this.TokenResponse != null)
      request.AddHeader("Authorization", "Bearer " + this.TokenResponse.AccessToken);
    request.AddHeader("Content-Type", "application/json");
    return JsonConvert.DeserializeObject<BucketDetailsResponse>(this._forgeClient.Execute<BucketDetailsResponse>((IRestRequest) request).Content);
  }

  public async Task<ObjectDetailsResponse> GetObjectDetailsAsync(string bucketKey, string objectKey)
  {
    RestRequest request = new RestRequest($"oss/v2/buckets/{bucketKey}/objects/{objectKey}/details", Method.GET);
    if (this.TokenResponse != null)
      request.AddHeader("Authorization", "Bearer " + this.TokenResponse.AccessToken);
    request.AddHeader("Content-Type", "application/json");
    ObjectDetailsResponse objectDetailsAsync = await this._forgeClient.ExecuteAsync<ObjectDetailsResponse>(request);
    if (objectDetailsAsync.IsOk())
    {
      foreach (ObjectDetails objectDetails in objectDetailsAsync.Objects)
        objectDetailsAsync.Objects[0].SetFileId(ForgeModel.GetFileId(bucketKey, objectDetails.ObjectKey));
    }
    return objectDetailsAsync;
  }

  public async Task<ObjectDetailsResponse> UploadFileAsync(string bucketKey, FileUploadInfo fi)
  {
    this.TokenResponse = await this.GetAccessTokenAsync("data:write");
    string key = fi.Key;
    ObjectDetailsResponse objectDetailsResponse;
    using (BinaryReader binaryReader = new BinaryReader(fi.InputStream))
    {
      byte[] numArray = binaryReader.ReadBytes((int) fi.Length);
      RestRequest request = new RestRequest($"oss/v2/buckets/{bucketKey.ToLower()}/objects/{key}", Method.PUT);
      if (this.TokenResponse != null)
        request.AddHeader("Authorization", "Bearer " + this.TokenResponse.AccessToken);
      request.AddParameter("Content-Type", (object) "application/stream");
      request.AddParameter("Content-Length", (object) fi.Length);
      request.AddParameter("requestBody", (object) numArray, RestSharp.ParameterType.RequestBody);
      request.Timeout = 3600000;
      objectDetailsResponse = await this._forgeClient.ExecuteAsync<ObjectDetailsResponse>(request);
    }
    return objectDetailsResponse;
  }

  public ObjectDetailsResponse UploadFile(string bucketKey, FileUploadInfo fi)
  {
    this.TokenResponse = this.GetAccessToken("data:write");
    string key = fi.Key;
    using (BinaryReader binaryReader = new BinaryReader(fi.InputStream))
    {
      byte[] numArray = binaryReader.ReadBytes((int) fi.Length);
      RestRequest request = new RestRequest($"oss/v2/buckets/{bucketKey.ToLower()}/objects/{key}", Method.PUT);
      if (this.TokenResponse != null)
        request.AddHeader("Authorization", "Bearer " + this.TokenResponse.AccessToken);
      request.AddParameter("Content-Type", (object) "application/stream");
      request.AddParameter("Content-Length", (object) fi.Length);
      request.AddParameter("requestBody", (object) numArray, RestSharp.ParameterType.RequestBody);
      request.Timeout = 3600000;
      return JsonConvert.DeserializeObject<ObjectDetailsResponse>(this._forgeClient.Execute<ObjectDetailsResponse>((IRestRequest) request).Content);
    }
  }

  public Task<RegisterResponse> RegisterAsync(string fileId)
  {
    RestRequest request = new RestRequest("viewingservice/v1/register", Method.POST);
    if (this.TokenResponse != null)
      request.AddHeader("Authorization", "Bearer " + this.TokenResponse.AccessToken);
    request.AddHeader("Content-Type", "application/json");
    string str = $"{{\"urn\":\"{fileId.ToBase64()}\"}}";
    request.AddParameter("application/json", (object) str, RestSharp.ParameterType.RequestBody);
    return this._forgeClient.ExecuteAsync<RegisterResponse>(request);
  }

  public async Task<ThumbnailResponse> GetThumbnailAsync(
    string fileId,
    int width = 150,
    int height = 150,
    string guid = "")
  {
    ThumbnailResponse response = new ThumbnailResponse();
    try
    {
      RestRequest request = new RestRequest("viewingservice/v1/thumbnails/" + fileId.ToBase64(), Method.GET);
      if (this.TokenResponse != null)
        request.AddHeader("Authorization", "Bearer " + this.TokenResponse.AccessToken);
      request.AddParameter(nameof (width), (object) width);
      request.AddParameter(nameof (height), (object) height);
      if (guid != "")
        request.AddParameter(nameof (guid), (object) guid);
      IRestResponse restResponse = await this._forgeClient.ExecuteAsync(request);
      if (restResponse.StatusCode != HttpStatusCode.OK)
      {
        if (restResponse.Content != string.Empty)
        {
          response.Error = JsonConvert.DeserializeObject<ViewDataError>(restResponse.Content);
          response.Error.StatusCode = restResponse.StatusCode;
        }
        else
          response.Error = new ViewDataError(restResponse.StatusCode);
      }
      else
      {
        using (MemoryStream memoryStream = new MemoryStream(restResponse.RawBytes))
          response.Image = Image.FromStream((Stream) memoryStream);
      }
    }
    catch (Exception ex)
    {
      response.Error = new ViewDataError(ex);
    }
    ThumbnailResponse thumbnailAsync = response;
    response = (ThumbnailResponse) null;
    return thumbnailAsync;
  }

  public Task<ViewableResponse> GetViewableAsync(
    string fileId,
    ViewableOptionEnum option = ViewableOptionEnum.kDefault,
    string guid = "")
  {
    string str = "";
    switch (option)
    {
      case ViewableOptionEnum.kStatus:
        str = "/status";
        break;
      case ViewableOptionEnum.kAll:
        str = "/all";
        break;
    }
    RestRequest request = new RestRequest($"viewingservice/v1/{fileId.ToBase64()}{str}", Method.GET);
    if (this.TokenResponse != null)
      request.AddHeader("Authorization", "Bearer " + this.TokenResponse.AccessToken);
    if (guid != "")
      request.AddParameter(nameof (guid), (object) guid);
    return this._forgeClient.ExecuteAsync<ViewableResponse>(request);
  }

  public Task<FormatResponse> GetSupportedFormats()
  {
    RestRequest request = new RestRequest("viewingservice/v1/supported", Method.GET);
    if (this.TokenResponse != null)
      request.AddHeader("Authorization", "Bearer " + this.TokenResponse.AccessToken);
    request.AddHeader("Content-Type", "application/json");
    return this._forgeClient.ExecuteAsync<FormatResponse>(request);
  }

  public static string GetFileId(string bucketKey, string objectKey)
  {
    return $"urn:adsk.objects:os.object:{bucketKey.ToLower()}/{objectKey}";
  }

  public static string GetUrn(string bucketKey, string objectKey)
  {
    return ForgeModel.EncodeToBase64(ForgeModel.GetFileId(bucketKey, objectKey));
  }

  public async Task<ViewDataResponseBase> UploadAndRegisterAsync(
    BucketCreationData bucketData,
    FileUploadInfo fi,
    bool waitForTranslation = false,
    bool createBucketIfNotExist = true)
  {
    BucketDetailsResponse bucketDetailsResponse = await this.GetBucketDetailsAsync(bucketData.Name);
    if (!bucketDetailsResponse.IsOk())
    {
      if (!(bucketDetailsResponse.Error.StatusCode == HttpStatusCode.NotFound & createBucketIfNotExist))
        return (ViewDataResponseBase) bucketDetailsResponse;
      BucketDetailsResponse bucketAsync = await this.CreateBucketAsync(bucketData);
      if (!bucketDetailsResponse.IsOk())
        return (ViewDataResponseBase) bucketAsync;
    }
    ObjectDetailsResponse objectDetailsResponse = await this.UploadFileAsync(bucketData.Name, fi);
    if (!objectDetailsResponse.IsOk() || objectDetailsResponse.Objects.Count < 1)
      return (ViewDataResponseBase) objectDetailsResponse;
    string fileId = objectDetailsResponse.Objects[0].FileId;
    RegisterResponse registerResponse = await this.RegisterAsync(fileId);
    if (!registerResponse.IsOk() || !waitForTranslation)
      return (ViewDataResponseBase) registerResponse;
    ViewableResponse viewableAsync;
    do
    {
      await Task.Delay(2000);
      viewableAsync = await this.GetViewableAsync(fileId, ViewableOptionEnum.kStatus);
    }
    while (viewableAsync.IsOk() && !(viewableAsync.Progress.ToLower() == "complete"));
    return (ViewDataResponseBase) viewableAsync;
  }

  public async Task<RegisterResponse> RegisterDerivatives(string bucketKey, string objectKey)
  {
    this.TokenResponse = await this.GetAccessTokenAsync("data:read data:write");
    string urn = ForgeModel.GetUrn(bucketKey, objectKey);
    RestRequest request = new RestRequest("modelderivative/v2/designdata/job", Method.POST);
    if (this.TokenResponse != null)
      request.AddHeader("Authorization", "Bearer " + this.TokenResponse.AccessToken);
    request.AddHeader("Content-Type", "application/json");
    request.AddHeader("x-ads-force", "true");
    string str = JsonConvert.SerializeObject((object) new
    {
      input = new{ urn = urn },
      output = new
      {
        destination = new{ region = "us" },
        formats = new \u003C\u003Ef__AnonymousType5<string, string[]>[1]
        {
          new{ type = "svf", views = new string[1]{ "2d" } }
        }
      }
    });
    request.AddParameter("application/json", (object) str, RestSharp.ParameterType.RequestBody);
    return await this._forgeClient.ExecuteAsync<RegisterResponse>(request);
  }

  private static string EncodeToBase64(string toEncode)
  {
    return Convert.ToBase64String(Encoding.ASCII.GetBytes(toEncode));
  }

  private static string DecodeFromBase64(string encodedData)
  {
    return Encoding.ASCII.GetString(Convert.FromBase64String(encodedData));
  }
}
