using RestSharp;
using System;
using System.Linq;
using System.Net;
using System.Threading;

namespace ZdFindDuplicateUsers.HelperFunctions
{
    public static class RestHelperFunctions
    {
        // Get integer value from response header
        public static int? GetIntFromHeaders(string headerName, IRestResponse response)
        {
            int? retVal = null;

            var foundHeader = response.Headers.FirstOrDefault(x => x.Name == headerName);
            if (!(foundHeader is null))
            {
                Int32.TryParse(foundHeader.Value.ToString(), out int val);
                retVal = val;
            }

            return retVal;
        }

        // Handle status code result 429 (Too Many Requests) by waiting the amount of time specified in Retry-After header, plus 250 millisecs
        public static void HandleTooManyRequests(IRestResponse response)
        {
            int? retryAfterSecs = GetIntFromHeaders("Retry-After", response);
            int waitMillisecs;
            if (retryAfterSecs.HasValue)
            {
                waitMillisecs = (retryAfterSecs.Value * 1000) + 250;
                Console.WriteLine($"Too Many Requests result received, need to wait {retryAfterSecs} seconds; sleeping {waitMillisecs} milliseconds...");
                Thread.Sleep(waitMillisecs);
            }
            else
            {
                waitMillisecs = 90 * 1000;
                Console.WriteLine($"Too Many Requests result received, unable to get amount of time to wait; sleeping {waitMillisecs} milliseconds...");
            }
        }

        // Send Rest request and return response (retry if rate limit response received)
        public static IRestResponse SendRestRequest(string baseUrl, string apiCredentials, string resource, HttpStatusCode successCode, string requestDescription, string requestJsonBody = null, Method httpMethod = Method.GET)
        {
            var client = new RestClient
            {
                BaseUrl = new Uri(baseUrl)
            };

            var restRequest = new RestRequest();
            restRequest.AddHeader("Authorization", $"Basic {apiCredentials}");
            restRequest.Method = httpMethod;
            restRequest.Resource = resource;
            if (!(requestJsonBody is null))
            {
                restRequest.AddJsonBody(requestJsonBody);
            }

            bool ok = false;
            IRestResponse response = null;
            while (!ok)
            {
                response = client.Execute(restRequest);
                if (response.StatusCode == successCode)
                {
                    ok = true;
                }
                else
                {
                    switch (response.StatusCode)
                    {
                        case System.Net.HttpStatusCode.Forbidden:
                            throw new Exception($"Forbidden result {(int)response.StatusCode} occurred when {requestDescription} with base URL: {baseUrl}. Please check that the provided email address has the necessary permissions.", response.ErrorException);
                        case System.Net.HttpStatusCode.Unauthorized:
                            throw new Exception($"Unauthorized result {(int)response.StatusCode} occurred when {requestDescription} with base URL: {baseUrl}. Please check that the provided email address and API token are correct.", response.ErrorException);
                        case System.Net.HttpStatusCode.TooManyRequests:
                            RestHelperFunctions.HandleTooManyRequests(response);
                            break;
                        case 0:
                            var innerExceptionTxt = response.ErrorException.InnerException is null ? "" : ", inner Exception: " + response.ErrorException.InnerException.Message;
                            Console.WriteLine($"Status code 0 when {requestDescription} with base URL: {baseUrl}, resource: {resource}, exception: {response.ErrorException.Message}{innerExceptionTxt}, retrying");
                            break;
                        default:
                            throw new Exception($"Error when {requestDescription} with base URL: {baseUrl}, resource: {resource}, result status code: {response.StatusCode}", response.ErrorException);
                    }
                }
            }            
            return response;
        }
    }
}
