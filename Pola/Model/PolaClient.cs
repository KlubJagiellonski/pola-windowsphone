using Newtonsoft.Json;
using Pola.Model.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Pola.Model
{
    public class PolaClient
    {
        public const string BaseUrl = @"https://pola-staging.herokuapp.com";
        public const string DeviceId = @"test";

        public static async Task<Product> FindProduct(long code)
        {
            return await FindProduct(code.ToString());
        }

        public static async Task<Product> FindProduct(string code)
        {
            WebRequest productRequest = WebRequest.Create(string.Format("{0}/a/get_by_code/{1}?device_id={2}", BaseUrl, code, DeviceId));
            using (WebResponse productResponse = await productRequest.GetResponseAsync())
            using (StreamReader productReader = new StreamReader(productResponse.GetResponseStream()))
            {
                try
                {
                    return JsonConvert.DeserializeObject<Product>(productReader.ReadToEnd());
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}
