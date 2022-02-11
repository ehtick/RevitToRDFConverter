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


            IList<Element> wallCollector = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Walls).WhereElementIsElementType().ToElements();
            List<Wall> wallList = new List<Wall>();
            StringBuilder sb = new StringBuilder();

            foreach (Element wall in wallCollector)
            {
                Wall w = wall as Wall;
                sb.Append(wall.Name + "\n");

            }

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

            var tedt = HttpClientHelper.POSTDataAsync(reader);
                      
            TaskDialog.Show("Revit", sb.ToString());
            return Result.Succeeded;
        }
    }


    
}
