using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace IngressServiceAPI.API
{
    /// <summary>
    /// Client used to send OMF message to the ingress service.
    /// </summary>
    public class IngressClient : IDisposable
    {
        public const string CurrentOMFVersion = "1.0";
        private readonly HttpClient _client;
        private string _producerToken;

        public bool UseCompression { get; set; }

        /// <summary>
        /// Create an IngressClient by passing it the required connection information.
        /// </summary>
        /// <param name="serviceUrl">The HTTP endpoint for the ingress service.</param>
        /// <param name="producerToken">Security token used to authenticate with the service.</param>     
        public IngressClient(string serviceUrl, string producerToken)
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri(serviceUrl);
            _producerToken = producerToken;
        }

        /// <summary>
        /// Sends a collection of JSONSchema strings to ingress service.  The JSONSchema describes
        /// the types used for the data that will be sent.
        /// </summary>
        /// <param name="types">A collection of JSONSchema string.</param>
        public void CreateTypes(IEnumerable<string> types)
        {
            string json = string.Format("[{0}]", string.Join(",", types));
            var bytes = Encoding.UTF8.GetBytes(json);
            SendMessageAsync(bytes, MessageType.Type, MessageAction.Create).Wait();
        }

        /// <summary>
        /// Sends a collection of ContainerId, TypeId pairs to the endpoint.
        /// </summary>
        /// <param name="streams"></param>
        public void CreateContainers(IEnumerable<ContainerInfo> streams)
        {
            string json = JsonConvert.SerializeObject(streams);Console.Write(json);
            var bytes = Encoding.UTF8.GetBytes(json);
            SendMessageAsync(bytes, MessageType.Container, MessageAction.Create).Wait();
        }

        /// <summary>
        /// Sends the actual values to the ingress service.  This is async to allow for higher
        /// throughput to the event hub. For DynamicStreamValues.
        /// </summary>
        /// <param name="values">A collection of values and their associated streams.</param>
        /// <returns></returns>
        public Task SendValuesAsync(IEnumerable<DynamicStreamValues> values)
        {
            string json = JsonConvert.SerializeObject(values);
            var bytes = Encoding.UTF8.GetBytes(json);
            return SendMessageAsync(bytes, MessageType.Data, MessageAction.Create);
        }

        /// <summary>
        /// Sends the actual values to the ingress service.  This is async to allow for higher
        /// throughput to the event hub. For StaticStreamValues.
        /// </summary>
        /// <param name="values">A collection of values and their associated streams.</param>
        /// <returns></returns>
        public Task SendValuesAsync2(IEnumerable<StaticStreamValues> values)
        {
            string json = JsonConvert.SerializeObject(values);
            //Console.Write(json + "\n");
            var bytes = Encoding.UTF8.GetBytes(json);
            return SendMessageAsync(bytes, MessageType.Data, MessageAction.Create);
        }

        private async Task SendMessageAsync(byte[] body, MessageType msgType, MessageAction action)
        {
            Message msg = new Message();
            msg.ProducerToken = _producerToken;
            msg.MessageType = msgType;
            msg.Action = action;
            msg.MessageFormat = MessageFormat.JSON;
            msg.Body = body;
            msg.Version = CurrentOMFVersion;

            if (UseCompression)
                msg.Compress(MessageCompression.GZip);

            HttpContent content = HttpContentFromMessage(msg);
            HttpResponseMessage response = await _client.PostAsync("" /* use the base URI */, content);
            Console.Write(response.Content.ReadAsStringAsync().Result);
            response.EnsureSuccessStatusCode();
        }

        private HttpContent HttpContentFromMessage(Message msg)
        {
            ByteArrayContent content = new ByteArrayContent(msg.Body);
            foreach(var header in msg.Headers)
            {
                content.Headers.Add(header.Key, header.Value);
            }
            return content;
        }

        #region IDisposable
        private bool _disposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _client.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
