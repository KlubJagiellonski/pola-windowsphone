using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Pola.Model
{
    /// <summary>
    /// Provides methods to comunicate with a serer using JSON data.
    /// </summary>
    public class JsonWebClient
    {
        private static async Task<T> DoRequest<T>(string uri, string method, object requestBody = null)
        {
            Debug.WriteLine(uri);
            WebRequest request = WebRequest.Create(uri);
            request.Method = method;
            if (requestBody != null)
                using (StreamWriter writer = new StreamWriter(await request.GetRequestStreamAsync()))
                    writer.Write(JsonConvert.SerializeObject(requestBody));
            using (WebResponse response = await request.GetResponseAsync())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                string responseString = reader.ReadToEnd();
                T responseObject = JsonConvert.DeserializeObject<T>(responseString);
#if DEBUG
                Debug.WriteLine(JsonConvert.SerializeObject(responseObject, Formatting.Indented));
#endif
                return responseObject;
            }
        }

        public static async Task<T> Get<T>(string uri)
        {
            return await DoRequest<T>(uri, "GET");
        }

        public static async Task<T> Post<T>(string uri, object requestBody = null)
        {
            return await DoRequest<T>(uri, "POST", requestBody);
        }
    }
}
