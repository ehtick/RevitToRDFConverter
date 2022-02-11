using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.ApplicationServices;
using RestSharp;

namespace RevitToRDFConverter
{

    

    [Transaction(TransactionMode.Manual)]

    public class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;


            IList<Element> wallCollector = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Walls).WhereElementIsElementType().ToElements();
            List<Wall> wallList = new List<Wall>();
            StringBuilder sb = new StringBuilder();

            foreach (Element wall in wallCollector)
            {
                Wall w = wall as Wall;
                sb.Append(wall.Name + "\n");

            }

            //string url = "https://jsonplaceholder.typicode.com/posts";

            //var client = new RestClient("url");

            //var request = new RestRequest();

            //var body = new post { body = "This is the test body", title = "test post request", userID = 2 };

            //request.AddJsonBody(body);

            //var response = client.PostAsync(request);

            //Console.WriteLine(response.Status.ToString());

            var client = new RestClient("http://localhost:3030/test-db/data");

            var request = new RestRequest();

            request.AddHeader("Content-Type", "text/turtle");

            var body = new post { body = "@prefix ex: <https://example.com/ex#> ." };

            request.AddBody(body);

            //request.AddParameter("text/turtle", body, RestSharp.ParameterType.RequestBody);


            TaskDialog.Show("Revit", sb.ToString());
            return Result.Succeeded;
        }
    }


    
}
