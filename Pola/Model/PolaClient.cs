using Pola.Data;
using Pola.Model.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

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
        /// Returns info about product and company using barcode. The barcode should be in EAN-13 or EAN-8 format.
        /// </summary>
        /// <param name="barcode">Barcode of a product.</param>
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

        /// <summary>
        /// Creates a new report and returns URLs where photos can be uploaded.
        /// </summary>
        /// <param name="report">Report as JSON object.</param>
        /// <returns></returns>
        public static async Task<ReportResponse> CreateReport(Report report)
        {
            string requestUri = string.Format("{0}/a/v2/create_report?device_id={1}", BaseUrl, DeviceId);
            return await Post<ReportResponse>(requestUri, report);
        }

        /// <summary>
        /// Update the descirption of an existing report. This method doesn't allow to upload photos.
        /// </summary>
        /// <param name="report"></param>
        /// <param name="reportId"></param>
        /// <returns></returns>
        public static async Task<ReportResponse> UpdateReport(Report report, int reportId)
        {
            string requestUri = string.Format("{0}/a/v2/update_report?device_id={1}&report_id={2}", BaseUrl, DeviceId, reportId);
            return await Post<ReportResponse>(requestUri, report);
        }

        /// <summary>
        /// Gets the URL where photo can be uploaded as an attachment to an existing report.
        /// </summary>
        /// <param name="reportId"></param>
        /// <returns></returns>
        public static async Task<AttachResponse> AttachFile(int reportId)
        {
            string requestUri = string.Format("{0}/a/v2/attach_file?device_id={1}&report_id={2}", BaseUrl, DeviceId, reportId);
            return await Post<AttachResponse>(requestUri, Attach.Default);
        }

        /// <summary>
        /// Uploads a photo as a WriteableBitmap. This methods converts the given bitmap to a PNG file before sending it to the server.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="bmp"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Uploads a photo as row data of a PNG file.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="pngBuffer"></param>
        /// <returns></returns>
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
