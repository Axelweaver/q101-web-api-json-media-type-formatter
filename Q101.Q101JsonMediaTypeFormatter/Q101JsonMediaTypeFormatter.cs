using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Q101.Q101JsonMediaTypeFormatter
{
    /// <summary>
    /// Json Media type formatter using Newtonsoft.Json library
    /// </summary>
    public class Q101JsonMediaTypeFormatter : JsonMediaTypeFormatter
    {
        /// <summary>
        /// 
        /// </summary>
        public Q101JsonMediaTypeFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
        }

        /// <inheritdoc />
        public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
        {
            base.SetDefaultContentHeaders(type, headers, mediaType);
            headers.ContentType = new MediaTypeHeaderValue("application/json");
        }
        /// <summary>
        /// Десериализация
        /// </summary>
        /// <param name="type"></param>
        /// <param name="readStream"></param>
        /// <param name="content"></param>
        /// <param name="formatterLogger"></param>
        /// <returns></returns>
        public override Task<object> ReadFromStreamAsync(Type type,
                                                         Stream readStream,
                                                         HttpContent content,
                                                         IFormatterLogger formatterLogger)
        {
            readStream.Position = 0;

            var task = Task.Factory.StartNew<object>(() =>
            {
                using (var streamReader = new StreamReader(readStream))
                {
                    var body = streamReader.ReadToEnd();

                    var bodyObject = JsonConvert.DeserializeObject(body, type);

                    return bodyObject;
                }
            });

            return task;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <param name="stream"></param>
        /// <param name="content"></param>
        /// <param name="transportContext"></param>
        /// <returns></returns>
        public override Task WriteToStreamAsync(Type type,
                                                object value,
                                                Stream stream,
                                                HttpContent content,
                                                TransportContext transportContext)
        {
            var task = Task.Factory.StartNew(() =>
            {
                using (var streamWriter = new StreamWriter(stream))
                {
                    var contractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    };

                    var newtonSoftJsonSerializerSettings = new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore, // Ignore the Self Reference looping
                        PreserveReferencesHandling = PreserveReferencesHandling.None, // Do not Preserve the Reference Handling
                        ContractResolver = contractResolver, // Make All properties Camel Case
                        Formatting = Formatting.Indented
                    };

                    var stringBody = JsonConvert.SerializeObject(value,
                        newtonSoftJsonSerializerSettings);

                    streamWriter.Write(stringBody);

                    streamWriter.Flush();
                }
            });

            return task;
        }

    }
}
