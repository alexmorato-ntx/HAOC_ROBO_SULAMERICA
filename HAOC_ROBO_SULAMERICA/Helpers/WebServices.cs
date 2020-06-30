using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HAOC_ROBO_SULAMERICA.Helpers
{
    public class ResponseJson
    {
        public string statusRetorno { get; set; }

        public bool possuiDados { get; set; }

        public dynamic objetoRetorno { get; set; }
    }


    public class WebServices
    {


        public string urlWebAPI = System.Configuration.ConfigurationManager.AppSettings["urlAPI"];

        public ResponseJson retorno = new ResponseJson();


        public async Task<IRestResponse> PostFormDataAtendimentoRestAsync<T>(string ComplementoUrl, string dataInicial, string dataFinal)
        {
            try
            {
                var client = new RestClient(urlWebAPI + ComplementoUrl);

                var request = new RestRequest(Method.POST);
                var boundary = Guid.NewGuid();
                string formDataBoundary = String.Format("----------{0:N}", boundary);
                string contentType = "multipart/form-data; boundary=" + formDataBoundary;


                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddHeader("content-type", contentType);


            
                request.AddParameter("multipart/form-data; boundary=----WebKitFormBoundary" + boundary, "------WebKitFormBoundary" + boundary + "\r\nContent-Disposition: form-data; name=\"StartDate\"\r\n\r\n" + dataInicial + "\r\n------WebKitFormBoundary" + boundary + "\r\nContent-Disposition: form-data; name=\"EndDate\"\r\n\r\n" + dataFinal + "\r\n------WebKitFormBoundary" + boundary + "--", ParameterType.RequestBody);

                 

                var taskCompletionSource = new TaskCompletionSource<IRestResponse<T>>();
                EventWaitHandle executedCallBack = new AutoResetEvent(false);
                TaskCompletionSource<IRestResponse> tcs = new TaskCompletionSource<IRestResponse>();
                IRestResponse res = new RestResponse();
                 
                client.ExecuteAsync<RestResponse>(request, response =>
                { 
                    res = response;
                    tcs.TrySetResult(res);
                    executedCallBack.Set();
                });

                return await tcs.Task;

            }
            catch (Exception ex)
            {

                throw;
            }

        }






        //public async Task<IRestResponse> GetRestAsync<T>(string ComplementoUrl)
        //{
        //    var client = new RestClient(urlWebAPI + ComplementoUrl);

        //    var request = new RestRequest(Method.GET);

        //    request.AddHeader("cache-control", "no-cache");
        //    request.AddHeader("content-type", "application/json");
        //    request.AddHeader("accept", "application/json");
        //    request.AddHeader("authorization", "Basic QWNjZXNzSW5kaWNhZG9yZXM6RDRuMTNsNCQkJA==");

        //    var taskCompletionSource = new TaskCompletionSource<IRestResponse<T>>();
        //    EventWaitHandle executedCallBack = new AutoResetEvent(false);
        //    TaskCompletionSource<IRestResponse> tcs = new TaskCompletionSource<IRestResponse>();
        //    IRestResponse res = new RestResponse();

        //    client.ExecuteAsync<RestResponse>(request, response =>
        //    {
        //        res = response;
        //        tcs.TrySetResult(res);
        //        executedCallBack.Set();
        //    });

        //    return await tcs.Task;

        //}

        //public async Task<IRestResponse> GetParamtesRestAsync<T>(string ComplementoUrl, object objetoPut)
        //{
        //    var client = new RestClient(urlWebAPI + ComplementoUrl);

        //    var request = new RestRequest(Method.GET);

        //    request.AddHeader("cache-control", "no-cache");
        //    request.AddHeader("content-type", "application/json");
        //    request.AddHeader("accept", "application/json");
        //    request.AddHeader("authorization", "Basic QWNjZXNzSW5kaWNhZG9yZXM6RDRuMTNsNCQkJA==");
        //    request.RequestFormat = DataFormat.Json;

        //    request.AddObject(objetoPut);

        //    var taskCompletionSource = new TaskCompletionSource<IRestResponse<T>>();
        //    EventWaitHandle executedCallBack = new AutoResetEvent(false);
        //    TaskCompletionSource<IRestResponse> tcs = new TaskCompletionSource<IRestResponse>();
        //    IRestResponse res = new RestResponse();

        //    client.ExecuteAsync<RestResponse>(request, response =>
        //    {
        //        res = response;
        //        tcs.TrySetResult(res);
        //        executedCallBack.Set();
        //    });

        //    return await tcs.Task;


        //}

        public async Task<IRestResponse> PostObjectAtendimentoURIRestAsync<T>(string ComplementoUrl, object objetoPost)
        {
            var client = new RestClient(urlWebAPI + ComplementoUrl);

            var request = new RestRequest(Method.POST);

            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/json");
            request.AddHeader("accept", "application/json"); 
            request.RequestFormat = DataFormat.Json;

            request.AddBody((T)objetoPost);

            var taskCompletionSource = new TaskCompletionSource<IRestResponse<T>>();
            EventWaitHandle executedCallBack = new AutoResetEvent(false);
            TaskCompletionSource<IRestResponse> tcs = new TaskCompletionSource<IRestResponse>();
            IRestResponse res = new RestResponse();

            client.ExecuteAsync<RestResponse>(request, response =>
            {
                res = response;
                tcs.TrySetResult(res);
                executedCallBack.Set();
            });

            return await tcs.Task;


        }

        //public async Task<IRestResponse> PutParametersRestAsync<T>(string ComplementoUrl)
        //{
        //    var client = new RestClient(urlWebAPI + ComplementoUrl);

        //    var request = new RestRequest(Method.PUT);

        //    request.AddHeader("cache-control", "no-cache");
        //    request.AddHeader("content-type", "application/json");
        //    request.AddHeader("accept", "application/json");
        //    request.AddHeader("authorization", "Basic QWNjZXNzSW5kaWNhZG9yZXM6RDRuMTNsNCQkJA==");


        //    var taskCompletionSource = new TaskCompletionSource<IRestResponse<T>>();
        //    EventWaitHandle executedCallBack = new AutoResetEvent(false);
        //    TaskCompletionSource<IRestResponse> tcs = new TaskCompletionSource<IRestResponse>();
        //    IRestResponse res = new RestResponse();

        //    client.ExecuteAsync<RestResponse>(request, response =>
        //    {
        //        res = response;
        //        tcs.TrySetResult(res);
        //        executedCallBack.Set();
        //    });

        //    return await tcs.Task;


        //}

        //public async Task<IRestResponse> PostParametersRestAsync<T>(string ComplementoUrl)
        //{
        //    try
        //    {
        //        var client = new RestClient(urlWebAPI + ComplementoUrl);

        //        var request = new RestRequest(Method.POST);

        //        request.AddHeader("cache-control", "no-cache");
        //        request.AddHeader("content-type", "application/json");
        //        request.AddHeader("accept", "application/json"); 
        //        request.RequestFormat = DataFormat.Json;
        //        //  request.AddHeader("authorization", "Basic QWNjZXNzSW5kaWNhZG9yZXM6RDRuMTNsNCQkJA==");

        //        var taskCompletionSource = new TaskCompletionSource<IRestResponse<T>>();
        //        EventWaitHandle executedCallBack = new AutoResetEvent(false);
        //        TaskCompletionSource<IRestResponse> tcs = new TaskCompletionSource<IRestResponse>();
        //        IRestResponse res = new RestResponse();

        //        client.ExecuteAsync<RestResponse>(request, response =>
        //        {
        //            res = response;
        //            tcs.TrySetResult(res);
        //            executedCallBack.Set();
        //        });

        //        return await tcs.Task;

        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }

        //}



    }
}
