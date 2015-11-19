using Newtonsoft.Json;
using Pola.Data;
using Pola.Model.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.System.Profile;

namespace Pola.Model
{
    /// <summary>
    /// Provides methods to communicate with Pola backend.
    /// </summary>
    public class PolaClient
    {
        public const string BaseUrl = @"https://www.pola-app.pl";
        public static readonly string DeviceId = Settings.DeviceId;

        public static async Task<Product> FindProduct(long code)
        {
            return await FindProduct(code.ToString());
        }

        /// <summary>
        /// Returns info about product and company using barcode. The barcode should be in EAN-13 format.
        /// </summary>
        /// <param name="barcode">EAN-13 barcode of a product.</param>
        /// <returns>Product details.</returns>
        public static async Task<Product> FindProduct(string barcode)
        {
            try
            {
                string requestUri = string.Format("{0}/a/get_by_code/{1}?device_id={2}", BaseUrl, barcode, DeviceId);
                Debug.WriteLine(requestUri);
                WebRequest productRequest = WebRequest.Create(requestUri);
                using (WebResponse productResponse = await productRequest.GetResponseAsync())
                using (StreamReader productReader = new StreamReader(productResponse.GetResponseStream()))
                {

                    string productString = productReader.ReadToEnd();
                    Product product = JsonConvert.DeserializeObject<Product>(productString);

#if DEBUG
                    Debug.WriteLine(JsonConvert.SerializeObject(product, Formatting.Indented));
#endif
                    return product;

                }
            }
            catch
            {
                return null;
            }
        }
    }
}
