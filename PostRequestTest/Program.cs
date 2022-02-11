using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.IO;
using System.Net.Http;

namespace PostRequestTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //var url = "http://webcode.me";

            //var request = WebRequest.Create(url);
            //request.Method = "POST";

            //var webResponse = request.Post();
            //var webStream = webResponse.GetResponseStream();

            //var reader = new StreamReader(webStream);
            //var data = reader.ReadToEnd();


            //***************WORKING********************
            //var user ="John Doe";

            //var json = JsonSerializer.Serialize(user);
            //var data = new StringContent(json, Encoding.UTF8, "application/json");

            //var url = "https://httpbin.org/post";
            //var client = new HttpClient();

            //HttpResponseMessage response = await client.PostAsync(url, data);

            //string result = response.Content.ReadAsStringAsync().Result;
            //Console.WriteLine(result);
            //******************************************
            var user = "@prefix ex: < https://example.com/ex#> .";
            //var data = new StringContent(, Encoding.UTF8, "application/json");
            string reader = @"@prefix ex: <https://example.com/ex#> .
" + "\n" +
@"@prefix xsd: <http://www.w3.org/2001/XMLSchema#> .
" + "\n" +
@"
" + "\n" +
@"
" + "\n" +
@"ex:Alice
" + "\n" +
@"	a ex:Person ;
" + "\n" +
@"	ex:ssn ""987-65-432A"" .
" + "\n" +
@"  
" + "\n" +
@"ex:Bob
" + "\n" +
@"	a ex:Person ;
" + "\n" +
@"	ex:ssn ""123-45-6789"" ;
" + "\n" +
@"	ex:ssn ""124-35-6789"" .
" + "\n" +
@"  
" + "\n" +
@"ex:Calvin
" + "\n" +
@"	a ex:Person ;
" + "\n" +
@"	ex:birthDate ""1971-07-07""^^xsd:date ;
" + "\n" +
@"	ex:worksFor ex:UntypedCompany .";
            var data1 = reader.ToString();
            var data2 = new StringContent(data1, Encoding.UTF8, "text/turtle");
            
            var url = "http://localhost:3030/test-db/data";
            var client = new HttpClient();
            HttpResponseMessage response = await client.PostAsync(url, data2);
            string result = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine(result);




            //record User(string Name, string Occupation);

            //Console.WriteLine(Re);
            //string url = "https://jsonplaceholder.typicode.com./posts";
            ////var request = new RestRequest("test-db/data/", Method.Post);

            //var client = new RestClient(url);
            //var request = new RestRequest();
            //var body = new post { body = "This is the test body", title = "test post request", userID = 2 };
            //request.AddJsonBody(body);
            //var response = client.PostAsync(request);
            //Console.WriteLine(response.Id);

            //var client = new RestClient("http://localhost:3030");
            //var request = new RestRequest("test-db/data/", Method.Post);

            //request.RequestFormat = DataFormat.Json;
            //request.AddHeader("Content-Type", "text/turtle");
            //var body = new post { body = "This is the test body" ,};

            //request.AddParameter("text/turtle", body, ParameterType.RequestBody);

            ////client.Execute(request);

            //Console.WriteLine();

            //var client = new RestClient("http://localhost:3030");
            //var request = new RestRequest("test-db/data/", Method.Post);
            //request.AddHeader("Content-Type", "text/turtle");
            //var body = "@prefix ex: <https://example.com/ex#> .";
            //request.AddParameter("text/turtle", body, ParameterType.RequestBody);
            //var response = client.PostAsync<post>(request);
            //Console.WriteLine(response);

            //Console.WriteLine(response.Status.ToString());
            ////Console.WriteLine("Hi");
            Console.Read();
        }
    }

    public class User
    {
        public string v1;
        public string v2;

        public User(string v1, string v2)
        {
            this.v1 = v1;
            this.v2 = v2;
        }
    }
}
