using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RevitToRDFConverter
{


    public class HttpClientHelper
    {
        private static HttpClient client = new HttpClient();

        public static async Task<string> POSTDataAsync(string data)
        {

            var data1 = data.ToString();
            var data2 = new StringContent(JsonConvert.SerializeObject(data1), Encoding.UTF8, "text/turtle");

            var url = "http://localhost:3500/Bot";
            HttpResponseMessage response = await client.PostAsync(url, data2);
            string result = response.Content.ReadAsStringAsync().Result;
            return result;
        }
    }


}

