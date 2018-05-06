using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace LandCoverMapping
{
    static class Program
    {
        // Subscription Key for Land Cover Mapping
        #region Hiding subscription key :)
        // Request an AI for Earth subscription key by emailing aiforearthapi@microsoft.com with 
        // details of your use case, geographic area of interest, anticipated number of calls per
        // unit time, and if you are willing to provide us feedback to improve the API
        const string subscriptionKey = "<your key here>";
        #endregion
        const string uriBase = "https://aiforearth.azure-api.net/v0.1/landcover/details";


        static void Main()
        {
            Console.WriteLine("\nAI for Earth Land Cover Mapping APIs - Sample application ");
            // Specify the ESRI NAIP Image
            string imageFilePath = "../../images/kent_island-2015-MD.tif";

            // Call the AI for Earth API
            AIforEarthLandCoverMappingAnalyze(imageFilePath).Wait();

            Console.WriteLine("\nPress Enter to exit...\n");
            Console.ReadLine();
        }


        /// <summary>
        /// Analyze the ESRI NAIP image file using the AI for Earth Land Cover Mapping REST API.
        /// </summary>
        /// <param name="imageFilePath">image file.</param>
        static async Task AIforEarthLandCoverMappingAnalyze(string imageFilePath)
        {
            try
            {
                HttpClient client = new HttpClient();

                // Request headers.
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

                // Request parameters. A third optional parameter is "details".
                string requestParameters = "type=jpeg";

                // Assemble the URI for the REST API Call.
                string uri = uriBase + "?" + requestParameters;

                Console.WriteLine("Sending request to {0}",uri);
                HttpResponseMessage response;

                // Request body. Posts a locally stored JPEG image.
                byte[] byteData = GetImageAsByteArray(imageFilePath);

                using (ByteArrayContent content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("image/tiff");

                    // Call the AI for Earth API
                    response = await client.PostAsync(uri, content);
                    string str = await response.Content.ReadAsStringAsync();

                    // Show the response status code
                    Console.WriteLine("RESPONSE: {0}", response.StatusCode );

                    // Convert the results to a JSON object
                    JObject results = JObject.Parse(str);
                    JToken landCoverMapping = results.Descendants()
                        .Where(t => t.Type == JTokenType.Property && ((JProperty)t).Name == "label_breakdown")
                        .Select(p => ((JProperty)p).Value)
                        .FirstOrDefault();

                    // Show the Land Cover Mapping Breakdown
                    Console.WriteLine(landCoverMapping);

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.ToString());                
            }

        }


        /// <summary>
        /// Returns the contents of the specified file as a byte array.
        /// </summary>
        /// <param name="imageFilePath">Image file to read.</param>
        /// <returns>Byte array of the image data.</returns>
        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }

    }
   
}