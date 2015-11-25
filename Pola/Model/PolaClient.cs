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
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.System.Profile;
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace Pola.Model
{
    /// <summary>
    /// Provides methods to communicate with Pola backend.
    /// </summary>
    public class PolaClient : JsonWebClient
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
                return await Get<Product>(requestUri);
            }
            catch
            {
                return null;
            }
        }

        public static async Task<ReportResponse> CreateReport(Report report)
        {
            string requestUri = string.Format("{0}/a/v2/create_report?device_id={1}", BaseUrl, DeviceId);
            return await Post<ReportResponse>(requestUri, report);
        }

        public static async Task<ReportResponse> UpdateReport(Report report, int reportId)
        {
            string requestUri = string.Format("{0}/a/v2/update_report?device_id={1}&report_id={2}", BaseUrl, DeviceId, reportId);
            return await Post<ReportResponse>(requestUri, report);
        }

        public static async Task<AttachResponse> AttachFile(int reportId)
        {
            string requestUri = string.Format("{0}/a/v2/attach_file?device_id={1}&report_id={2}", BaseUrl, DeviceId, reportId);
            return await Post<AttachResponse>(requestUri, Attach.Default);
        }

        public static async Task UploadImage(string uri, WriteableBitmap bmp)
        {
            InMemoryRandomAccessStream memoryStream = new InMemoryRandomAccessStream();
            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, memoryStream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)bmp.PixelWidth, (uint)bmp.PixelHeight, 96, 96, bmp.PixelBuffer.ToArray());
            await encoder.FlushAsync();

            Stream stream = memoryStream.AsStreamForRead();
            byte[] pngBuffer = new byte[stream.Length];
            stream.Read(pngBuffer, 0, pngBuffer.Length);

            await UploadImage(uri, pngBuffer);
        }

        public static async Task UploadImage(string uri, byte[] pngBuffer)
        {
            Debug.WriteLine(uri);
            WebRequest request = WebRequest.Create(uri);
            request.Method = "PUT";
            request.Headers["x-amz-acl"] = "public-read";
            request.ContentType = "image/png";
            using (Stream requestStream = await request.GetRequestStreamAsync())
                requestStream.Write(pngBuffer, 0, pngBuffer.Length);
            using (WebResponse response = await request.GetResponseAsync())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                string responseString = reader.ReadToEnd();
                Debug.WriteLine(responseString);
            }
        }
    }
}
