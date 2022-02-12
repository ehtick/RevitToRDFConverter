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
using System.Net.Http;

namespace RevitToRDFConverter
{



    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Command : IExternalCommand
    {
        Result IExternalCommand.Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;


            //IList<Element> wallCollector = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Walls).WhereElementIsElementType().ToElements();
            //List<Wall> wallList = new List<Wall>();
            StringBuilder sb = new StringBuilder();
            sb.Append(
                "@prefix owl: <http://www.w3.org/2002/07/owl#> ." + "\n" +
                "@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> ." + "\n" +
                "@prefix xml: <http://www.w3.org/XML/1998/namespace> ." + "\n" +
                "@prefix xsd: <http://www.w3.org/2001/XMLSchema#> ." + "\n" +
                "@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> ." + "\n" +
                "@prefix bot: <https://w3id.org/bot#> ." + "\n" +
                "@prefix fso: <https://w3id.org/fso#> ." + "\n" +
                "@prefix inst: <https://example.com/inst#> ." + "\n" +
                "@prefix fpo: <https://w3id.org/fpo#> ." + "\n");

            //foreach (Element wall in wallCollector)
            //{
            //    Wall w = wall as Wall;
            //    sb.Append(wall.Name + "\n");

            //}

            FilteredElementCollector roomCollector = new FilteredElementCollector(doc);
            ICollection<Element> levels = roomCollector.OfClass(typeof(SpatialElement)).ToElements();
            List<SpatialElement> roomList = new List<SpatialElement>();
            foreach (SpatialElement room in roomCollector)
            {
                SpatialElement w = room as SpatialElement;
                string changedRoomName = room.Name.Replace(' ', '-');
                sb.Append($"inst:{changedRoomName} a bot:Space ." + "\n");
            }
            string reader = sb.ToString();

            //string reader = @"@prefix ex: <https://example.com/ex#> .
            //" + "\n" +
            //@"@prefix xsd: <http://www.w3.org/2001/XMLSchema#> .
            //" + "\n" +
            //@"
            //" + "\n" +
            //@"
            //" + "\n" +
            //@"ex:Alice
            //" + "\n" +
            //@"	a ex:Person ;
            //" + "\n" +
            //@"	ex:ssn ""987-65-432A"" .
            //" + "\n" +
            //@"  
            //" + "\n" +
            //@"ex:Bob
            //" + "\n" +
            //@"	a ex:Person ;
            //" + "\n" +
            //@"	ex:ssn ""123-45-6789"" ;
            //" + "\n" +
            //@"	ex:ssn ""124-35-6789"" .
            //" + "\n" +
            //@"  
            //" + "\n" +
            //@"ex:Calvin
            //" + "\n" +
            //@"	a ex:Person ;
            //" + "\n" +
            //@"	ex:birthDate ""1971-07-07""^^xsd:date ;
            //" + "\n" +
            //@"	ex:worksFor ex:UntypedCompany .";

            var test = HttpClientHelper.POSTDataAsync(reader);
                      
            TaskDialog.Show("Revit", sb.ToString());
            return Result.Succeeded;
        }
    }


    
}
