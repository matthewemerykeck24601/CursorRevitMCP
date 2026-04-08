// Decompiled with JetBrains decompiler
// Type: EDGE.Cloud.Models.RestClientExtensions
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using EDGE.Cloud.Models.DataContracts;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

#nullable disable
namespace EDGE.Cloud.Models;

public static class RestClientExtensions
{
  public static async Task<IRestResponse> ExecuteAsync(this RestClient client, RestRequest request)
  {
    return await Task<IRestResponse>.Factory.StartNew((Func<IRestResponse>) (() => client.Execute((IRestRequest) request)));
  }

  public static Task<T> ExecuteAsync<T>(this RestClient client, RestRequest request) where T : new()
  {
    TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
    client.ExecuteAsync<T>((IRestRequest) request, (Action<IRestResponse<T>>) (httpResponse =>
    {
      try
      {
        if (httpResponse.StatusCode != HttpStatusCode.OK)
        {
          object obj1 = (object) new T();
          // ISSUE: reference to a compiler-generated field
          if (RestClientExtensions.\u003C\u003Eo__1<T>.\u003C\u003Ep__2 == null)
          {
            // ISSUE: reference to a compiler-generated field
            RestClientExtensions.\u003C\u003Eo__1<T>.\u003C\u003Ep__2 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof (RestClientExtensions), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
            {
              CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
            }));
          }
          // ISSUE: reference to a compiler-generated field
          Func<CallSite, object, bool> target1 = RestClientExtensions.\u003C\u003Eo__1<T>.\u003C\u003Ep__2.Target;
          // ISSUE: reference to a compiler-generated field
          CallSite<Func<CallSite, object, bool>> p2 = RestClientExtensions.\u003C\u003Eo__1<T>.\u003C\u003Ep__2;
          // ISSUE: reference to a compiler-generated field
          if (RestClientExtensions.\u003C\u003Eo__1<T>.\u003C\u003Ep__1 == null)
          {
            // ISSUE: reference to a compiler-generated field
            RestClientExtensions.\u003C\u003Eo__1<T>.\u003C\u003Ep__1 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.NotEqual, typeof (RestClientExtensions), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
            {
              CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
              CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.Constant, (string) null)
            }));
          }
          // ISSUE: reference to a compiler-generated field
          Func<CallSite, object, object, object> target2 = RestClientExtensions.\u003C\u003Eo__1<T>.\u003C\u003Ep__1.Target;
          // ISSUE: reference to a compiler-generated field
          CallSite<Func<CallSite, object, object, object>> p1 = RestClientExtensions.\u003C\u003Eo__1<T>.\u003C\u003Ep__1;
          // ISSUE: reference to a compiler-generated field
          if (RestClientExtensions.\u003C\u003Eo__1<T>.\u003C\u003Ep__0 == null)
          {
            // ISSUE: reference to a compiler-generated field
            RestClientExtensions.\u003C\u003Eo__1<T>.\u003C\u003Ep__0 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "Error", typeof (RestClientExtensions), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[1]
            {
              CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
            }));
          }
          // ISSUE: reference to a compiler-generated field
          // ISSUE: reference to a compiler-generated field
          object obj2 = RestClientExtensions.\u003C\u003Eo__1<T>.\u003C\u003Ep__0.Target((CallSite) RestClientExtensions.\u003C\u003Eo__1<T>.\u003C\u003Ep__0, obj1);
          object obj3 = target2((CallSite) p1, obj2, (object) null);
          if (target1((CallSite) p2, obj3))
          {
            // ISSUE: reference to a compiler-generated field
            if (RestClientExtensions.\u003C\u003Eo__1<T>.\u003C\u003Ep__3 == null)
            {
              // ISSUE: reference to a compiler-generated field
              RestClientExtensions.\u003C\u003Eo__1<T>.\u003C\u003Ep__3 = CallSite<Func<CallSite, object, string, object>>.Create(Binder.SetMember(CSharpBinderFlags.None, "Error", typeof (RestClientExtensions), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
              {
                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, (string) null)
              }));
            }
            // ISSUE: reference to a compiler-generated field
            // ISSUE: reference to a compiler-generated field
            object obj4 = RestClientExtensions.\u003C\u003Eo__1<T>.\u003C\u003Ep__3.Target((CallSite) RestClientExtensions.\u003C\u003Eo__1<T>.\u003C\u003Ep__3, obj1, httpResponse.Content);
          }
          // ISSUE: reference to a compiler-generated field
          if (RestClientExtensions.\u003C\u003Eo__1<T>.\u003C\u003Ep__4 == null)
          {
            // ISSUE: reference to a compiler-generated field
            RestClientExtensions.\u003C\u003Eo__1<T>.\u003C\u003Ep__4 = CallSite<Action<CallSite, TaskCompletionSource<T>, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.ResultDiscarded, "SetResult", (IEnumerable<Type>) null, typeof (RestClientExtensions), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
            {
              CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, (string) null),
              CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
            }));
          }
          // ISSUE: reference to a compiler-generated field
          // ISSUE: reference to a compiler-generated field
          RestClientExtensions.\u003C\u003Eo__1<T>.\u003C\u003Ep__4.Target((CallSite) RestClientExtensions.\u003C\u003Eo__1<T>.\u003C\u003Ep__4, tcs, obj1);
        }
        else
        {
          List<ErrorEventArgs> jsonErrors = new List<ErrorEventArgs>();
          T result = JsonConvert.DeserializeObject<T>(httpResponse.Content, new JsonSerializerSettings()
          {
            Error = (EventHandler<ErrorEventArgs>) ((sender, args) =>
            {
              args.ErrorContext.Handled = true;
              jsonErrors.Add(args);
            })
          });
          if (jsonErrors.Count != 0)
          {
            object obj5 = (object) (result ?? new T());
            // ISSUE: reference to a compiler-generated field
            if (RestClientExtensions.\u003C\u003Eo__1<T>.\u003C\u003Ep__5 == null)
            {
              // ISSUE: reference to a compiler-generated field
              RestClientExtensions.\u003C\u003Eo__1<T>.\u003C\u003Ep__5 = CallSite<Func<CallSite, object, ViewDataError, object>>.Create(Binder.SetMember(CSharpBinderFlags.None, "Error", typeof (RestClientExtensions), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
              {
                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, (string) null)
              }));
            }
            // ISSUE: reference to a compiler-generated field
            // ISSUE: reference to a compiler-generated field
            object obj6 = RestClientExtensions.\u003C\u003Eo__1<T>.\u003C\u003Ep__5.Target((CallSite) RestClientExtensions.\u003C\u003Eo__1<T>.\u003C\u003Ep__5, obj5, new ViewDataError(jsonErrors));
            // ISSUE: reference to a compiler-generated field
            if (RestClientExtensions.\u003C\u003Eo__1<T>.\u003C\u003Ep__6 == null)
            {
              // ISSUE: reference to a compiler-generated field
              RestClientExtensions.\u003C\u003Eo__1<T>.\u003C\u003Ep__6 = CallSite<Action<CallSite, TaskCompletionSource<T>, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.ResultDiscarded, "SetResult", (IEnumerable<Type>) null, typeof (RestClientExtensions), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
              {
                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, (string) null),
                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
              }));
            }
            // ISSUE: reference to a compiler-generated field
            // ISSUE: reference to a compiler-generated field
            RestClientExtensions.\u003C\u003Eo__1<T>.\u003C\u003Ep__6.Target((CallSite) RestClientExtensions.\u003C\u003Eo__1<T>.\u003C\u003Ep__6, tcs, obj5);
          }
          else
            tcs.SetResult(result);
        }
      }
      catch (Exception ex)
      {
        object obj7 = (object) new T();
        // ISSUE: reference to a compiler-generated field
        if (RestClientExtensions.\u003C\u003Eo__1<T>.\u003C\u003Ep__7 == null)
        {
          // ISSUE: reference to a compiler-generated field
          RestClientExtensions.\u003C\u003Eo__1<T>.\u003C\u003Ep__7 = CallSite<Func<CallSite, object, ViewDataError, object>>.Create(Binder.SetMember(CSharpBinderFlags.None, "Error", typeof (RestClientExtensions), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
          {
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null),
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, (string) null)
          }));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        object obj8 = RestClientExtensions.\u003C\u003Eo__1<T>.\u003C\u003Ep__7.Target((CallSite) RestClientExtensions.\u003C\u003Eo__1<T>.\u003C\u003Ep__7, obj7, new ViewDataError(ex));
        // ISSUE: reference to a compiler-generated field
        if (RestClientExtensions.\u003C\u003Eo__1<T>.\u003C\u003Ep__8 == null)
        {
          // ISSUE: reference to a compiler-generated field
          RestClientExtensions.\u003C\u003Eo__1<T>.\u003C\u003Ep__8 = CallSite<Action<CallSite, TaskCompletionSource<T>, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.ResultDiscarded, "SetResult", (IEnumerable<Type>) null, typeof (RestClientExtensions), (IEnumerable<CSharpArgumentInfo>) new CSharpArgumentInfo[2]
          {
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, (string) null),
            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, (string) null)
          }));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        RestClientExtensions.\u003C\u003Eo__1<T>.\u003C\u003Ep__8.Target((CallSite) RestClientExtensions.\u003C\u003Eo__1<T>.\u003C\u003Ep__8, tcs, obj7);
      }
    }));
    return tcs.Task;
  }
}
