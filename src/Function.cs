using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using lenus_predict_dotnet.Class;

using System.Net;
using Amazon.Lambda.Core;
using System.Net.Http;
using System.Net.Http.Headers;
using Amazon.Lambda.APIGatewayEvents;
using System.IO;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace lenus_predict_dotnet
{
    public class Function
    {
        public APIGatewayProxyResponse FunctionHandler(Parameters input, ILambdaContext context)
        {
            Task<string> task = InvokeRequestResponseService(input);
            task.Wait();
            var result = task.Result;
            return CreateResponse(result);
        }

        public static async Task<string> InvokeRequestResponseService(Parameters details)
        {
            using (var client = new HttpClient())
            {
                var scoreRequest = new
                {

                    Inputs = new Dictionary<string, StringTable>() {
                        {
                            "input1",
                            new StringTable()
                            {
                                ColumnNames = new string[] {"gender", "age", "currentSmoker", "cigsPerDay", "BPMeds", "prevalentHyp", "diabetes", "totChol", "sysBP", "diaBP", "BMI", "heartRate", "glucose", "TenYearCHD"},
                                Values = new string[,] {  { details.gender, details.age, details.currentSmoker, details.cigsPerDay, details.BPMeds, details.prevalentHyp, details.diabetes, details.totChol, details.sysBP, details.diaBP, details.BMI, details.heartRate, details.glucose, "0" },  }
                            }
                        },
                    },
                    GlobalParameters = new Dictionary<string, string>()
                    {
                    }
                };
                const string apiKey = "qT/zfKa8n340N2GNDINp2brLnwmQGcJMLs7pnpLZLyw4W5gy+bMoKHV5tLDewFqrNVTj1c+GiJqQMBCwExxY8w=="; // Replace this with the API key for the web service
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                client.BaseAddress = new Uri("https://ussouthcentral.services.azureml.net/workspaces/3ea4b54c3ccb46d4a795aa0bb2f06a99/services/bbc286f434e940e4886fa96bb1ab2423/execute?api-version=2.0&details=true");
                HttpResponseMessage response = await client.PostAsJsonAsync("", scoreRequest).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    return result;

                }
                else
                {
                    Console.WriteLine(string.Format("The request failed with status code: {0}", response.StatusCode));

                    // Print the headers - they include the requert ID and the timestamp, which are useful for debugging the failure
                    Console.WriteLine(response.Headers.ToString());
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseContent);
                    return responseContent;
                }
            }
        }

        APIGatewayProxyResponse CreateResponse(string result)
        {
            //There's an issue with AWS and serialization. 
            //Refering to this github for the workaround:https://github.com/aws/aws-lambda-dotnet/issues/692
            int statusCode = (result != null) ?
                (int)HttpStatusCode.OK :
                (int)HttpStatusCode.InternalServerError;

            string body = (result != null) ?
                result : string.Empty;

            var response = new APIGatewayProxyResponse
            {
                StatusCode = statusCode,
                Body = $"{result}",
                Headers = new Dictionary<string, string>
        {
            { "Content-Type", "application/json" },
            { "Access-Control-Allow-Origin", "*" }
        }
            };
            Console.WriteLine(response.Body);
            return response;
        }
    }


}
