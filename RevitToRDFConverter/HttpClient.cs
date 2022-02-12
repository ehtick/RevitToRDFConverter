using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitToRDFConverter
{


    public class HttpClientHelper
    {
        private static HttpClient client = new HttpClient();

        public static async Task<string> POSTDataAsync(string data)
        {

            var data1 = data.ToString();
            var data2 = new StringContent(data1, Encoding.UTF8, "text/turtle");

            var url = "http://localhost:3030/ny-db/data";
            HttpResponseMessage response = await client.PostAsync(url, data2);
            string result = response.Content.ReadAsStringAsync().Result;
            return result;
        }
    }


}

