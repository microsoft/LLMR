using System;
using System.Net.Http.Headers;
using System.Text;
using System.Net.Http;
using System.Web;


namespace CSHttpClientSampleManager
{
    static class Program
    {
        // {"url":"https://portal.vision.cognitive.azure.com/dist/assets/ImageCaptioningSample1-bbe41ac5.png"}
        static void Main()
        {
            MakeRequest();
            Console.WriteLine("Hit ENTER to exit...");
            Console.ReadLine();
        }

        static async void MakeRequest()

        {

            var client = new HttpClient();

            var queryString = HttpUtility.ParseQueryString("https://portal.vision.cognitive.azure.com/dist/assets/ImageCaptioningSample1-bbe41ac5.png");




            // Request headers

            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "");//put your azure vision studio api key here




            // Request parameters

            queryString["visualFeatures"] = "Categories";

            queryString["details"] = "{string}";

            queryString["language"] = "en";

            queryString["model-version"] = "latest";

            var uri = "https://*.cognitiveservices.azure.com/vision/v3.2/analyze?" + queryString;




            HttpResponseMessage response;




            // Request body

            byte[] byteData = Encoding.UTF8.GetBytes("{body}");




            using (var content = new ByteArrayContent(byteData))

            {

                content.Headers.ContentType = new MediaTypeHeaderValue("< your content type, i.e. application/json >");

                response = await client.PostAsync(uri, content);

            }




        }

    }

}
