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




            TaskDialog.Show("Revit", sb.ToString());
            return Result.Succeeded;
        }
    }


    
}
