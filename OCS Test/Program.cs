using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace OCS_TEST
{
    /*
     A producer of OMF messages intended for OCS is called a Publisher.
    Messages are sent to a queue called a Topic.
    A Subscription recieves messages from a Topic and writes them either to the OCS data store or makes them available to an external consumer.
         */
    class Program
    {
        public static HttpClient _client;
        public const string SECURITY_TOKEN = "ENTER_SECURITY_TOKEN";

        public static void Main()
        {
            //MainAsync().GetAwaiter().GetResult();
            omfAsync().GetAwaiter().GetResult();
            Console.ReadLine();
        }

        private static async Task omfAsync() {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            IConfiguration configuration = builder.Build();

            // ==== Client constants ====
            string tenantId = configuration["TenantId"];
            string namespaceId = configuration["NamespaceId"];
            string address = configuration["Address"];
            string resource = configuration["Resource"];
            string clientId = configuration["ClientId"];
            string clientKey = configuration["ClientKey"];
            string aadInstanceFormat = configuration["AADInstanceFormat"];

            QiSecurityHandler securityHandler =
                new QiSecurityHandler(resource, tenantId, aadInstanceFormat, clientId, clientKey);
            HttpClient httpClient = new HttpClient(securityHandler)
            {
                BaseAddress = new Uri(address)
            };
            _client = httpClient;
            //create type
            string JsonSchema =
            @"{""id"": ""SimpleType"",""type"": ""object"",
                ""classification"": ""dynamic"",
                ""properties"": {
                    ""Time"": { ""type"": ""string"", ""format"": ""date-time"", ""isindex"": true },
                    ""Value"": { ""type"": ""number"", ""format"": ""float64"" }
                }
            }";

            JsonSchema = string.Format("[{0}]", string.Join(",", new string[] { JsonSchema}));
            var bytes = Encoding.UTF8.GetBytes(JsonSchema);
            Message type = new Message();
            type.ProducerToken = SECURITY_TOKEN;
            type.MessageType = MessageType.Type;
            type.Action = MessageAction.Create;
            type.MessageFormat = MessageFormat.JSON;
            type.Body = bytes;
            type.Version = "1.0";
            HttpResponseMessage response = await httpClient.PostAsync($"api/omf", HttpContentFromMessage(type));
            response.EnsureSuccessStatusCode();

            //create container
            JsonSchema =
            @"{""Id"": ""TestStream1"",""TypeId"": ""SimpleType""}";
            JsonSchema = string.Format("[{0}]", string.Join(",", new string[] { JsonSchema }));
            bytes = Encoding.UTF8.GetBytes(JsonSchema);
            Message contain = new Message();
            contain.ProducerToken = SECURITY_TOKEN;
            contain.MessageType = MessageType.Container;
            contain.Action = MessageAction.Create;
            contain.MessageFormat = MessageFormat.JSON;
            contain.Body = bytes;
            contain.Version = "1.0";
            response.EnsureSuccessStatusCode();
            //create data


            // send and read time series indefinitely
            while (true)
            {
                // Create our set of values to send to our streams
                List<SimpleType> values = new List<SimpleType>();
                for (int i = 0; i < 10; i++)
                {
                    values.Add(new SimpleType() { Time = DateTime.UtcNow, Value = i });
                    Thread.Sleep(10);  // Offset the time-stamps by 10 ms
                }
                Console.Write("Sending Data\n");
                StreamValues vals1 = new StreamValues() { ContainerId = "TestStream1", Values = values };

                // Now send them
                SendValuesAsync(new StreamValues[] { vals1}).Wait();
                JsonSchema = JsonConvert.SerializeObject(values);

                // Now read from the Namespace
                Console.WriteLine("Getting data");
                response = await httpClient.GetAsync(
                    $"api/Tenants/{tenantId}/Namespaces/{namespaceId}/Streams/TestStream1/Data/GetLastValue");
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(response.ToString());
                }

                Console.WriteLine(response.Content.ReadAsStringAsync().Result);
                Thread.Sleep(1000);
            }
            


        }

        public static Task SendValuesAsync(IEnumerable<StreamValues> values)
        {
            string json = JsonConvert.SerializeObject(values);
            //Console.Write(json + "\n");
            var bytes = Encoding.UTF8.GetBytes(json);
            return SendMessageAsync(bytes);
        }

        private static async Task SendMessageAsync(byte[] body)
        {
            Message msg = new Message();
            msg.ProducerToken = SECURITY_TOKEN;
            msg.MessageType = MessageType.Data;
            msg.Action = MessageAction.Create;
            msg.MessageFormat = MessageFormat.JSON;
            msg.Body = body;
            msg.Version = "1.0";



            HttpContent content = HttpContentFromMessage(msg);;
            HttpResponseMessage response = await _client.PostAsync($"api/omf", content);
            Console.Write(response.Content.ReadAsStringAsync().Result);
            response.EnsureSuccessStatusCode();
        }

        private static HttpContent HttpContentFromMessage(Message msg)
        {
            ByteArrayContent content = new ByteArrayContent(msg.Body);
            foreach (var header in msg.Headers)
            {
                content.Headers.Add(header.Key, header.Value);
            }
            return content;
        }

     
    }
}
 