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
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
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
                "@prefix fpo: <https://w3id.org/fpo#> ." + "\n"+
                "@prefix ex: <https://example.com/ex#> ." + "\n");

            //foreach (Element wall in wallCollector)
            //{
            //    Wall w = wall as Wall;
            //    sb.Append(wall.Name + "\n");

            //}

            ////Get projectName and assign it as buildingName for now. 
            //ProjectInfo projectInfo = doc.ProjectInformation;
            //string buildingName = projectInfo.BuildingName;
            //string buildingGuid = System.Guid.NewGuid().ToString().Replace(' ', '-');
            //sb.Append($"inst:{buildingGuid} a bot:Building ." + "\n" + $"inst:{buildingGuid} rdfs:label '{buildingName}'^^xsd:string  ." + "\n");

            ////Get all level and the building it is related to 
            //FilteredElementCollector levelCollector = new FilteredElementCollector(doc);
            //ICollection<Element> levels = levelCollector.OfClass(typeof(Level)).ToElements();
            //List<Level> levelList = new List<Level>();
            //foreach (Level level in levelCollector)
            //{
            //    Level w = level as Level;
            //    string levelName = level.Name.Replace(' ', '-');
            //    string guidNumber = level.UniqueId.ToString();
            //    sb.Append($"inst:{guidNumber} a bot:Level ." + "\n" + $"inst:{guidNumber} rdfs:label '{levelName}'^^xsd:string ." + "\n" + $"inst:{buildingGuid} bot:hasStorey inst:{guidNumber} ." + "\n");
            //}

            //Get all rooms and assign the level to it
            //FilteredElementCollector rooms = new FilteredElementCollector(doc);
            //RoomFilter filter = new RoomFilter();
            //rooms.WherePasses(filter);
            //foreach (Room room in rooms)
            //{
            //    Room w = room as Room;
            //    string roomName = room.Name.Replace(' ', '-');
            //    string isSpaceOf = room.Level.UniqueId;
            //    string roomGuid = room.UniqueId.ToString();
            //    sb.Append($"inst:{roomGuid} a bot:Space ." + "\n" + $"inst:{roomGuid} rdfs:label '{roomName}'^^xsd:string ." + "\n" + $"inst:{isSpaceOf} bot:hasSpace inst:{roomGuid} ."+ "\n");
            //}

            ////Get all pipes 
            //FilteredElementCollector pipeCollector = new FilteredElementCollector(doc);
            //ICollection<Element> pipes = pipeCollector.OfClass(typeof(Pipe)).ToElements();
            //List<Pipe> pipeList = new List<Pipe>();
            //foreach (Pipe pipe in pipeCollector)
            //{
            //    Pipe w = pipe as Pipe;
            //    string pipeGuid = pipe.Id.ToString();
            //    double pipeLength = UnitUtils.ConvertFromInternalUnits(pipe.LookupParameter("Length").AsDouble(), UnitTypeId.Meters);
            //    string parameterGuid = System.Guid.NewGuid().ToString().Replace(' ', '-');
            //    sb.Append($"inst:{pipeGuid} a fso:Pipe ." + "\n"
            //        + $"inst:{pipeGuid} fpo:hasParameter inst:{parameterGuid} ." + "\n" +
            //        $"inst:{parameterGuid} a fpo:Length ." + "\n" +
            //         $"inst:{parameterGuid} fpo:value '{pipeLength}' ^^xsd:double ." + "\n");
            //    ConnectorSet connectorSet = pipe.ConnectorManager.Connectors;
            //    foreach (Connector port in connectorSet)
            //    {
            //        string connectorGuid = System.Guid.NewGuid().ToString().Replace(' ', '-');
            //        sb.Append($"inst:{pipeGuid} fso:hasPort inst:{connectorGuid} .");

            //        if (port.Direction.ToString() == "Out")

            //        {
            //            ConnectorSet joinedConnectors = port.AllRefs;
            //            foreach (Connector cousine in joinedConnectors)
            //            {
            //                //if (Domain.DomainHvac == cousine.Domain
            //                //|| Domain.DomainPiping == cousine.Domain)
            //                //{
            //                //    string cousineName = cousine.ToString();
            //                //    string connectorDirection = cousine.Direction.ToString();

            //                //    sb.Append($"inst:{connectorDirection} fso:hasPort inst:{cousine} .");
            //                //}

            //                if (Domain.DomainHvac != cousine.Domain
            //                 || Domain.DomainPiping != cousine.Domain)
            //                {
            //                    string cousineName = cousine.ToString();
            //                    //string connectorDirection = cousine.Direction.ToString();

            //                    sb.Append($"fso:hasPort inst:{cousineName} .");
            //                }

            //            }

            //        }

            //    }
            //}

            ////Get all components
            //FilteredElementCollector fittingCollector = new FilteredElementCollector(doc);
            //ICollection<Element> fittings = fittingCollector.OfClass(typeof(FamilyInstance)).ToElements();
            //List<FamilyInstance> fittingList = new List<FamilyInstance>();
            //foreach (FamilyInstance fitting in fittingCollector)
            //{
            //if (fitting.Category.Name == "Duct Fittings" || fitting.Category.Name == "Pipe Fittings")
            //{
            //    string fittingType = ((MechanicalFitting)fitting.MEPModel).PartType.ToString(); //in order to go from MEPModel to MechanicalFitting i am using casting ii.MEPModel
            //    //FamilyInstance dFitting = ii as FamilyInstance;
            //    //MechanicalFitting mechanicalFitting = dFitting.MEPModel as MechanicalFitting;
            //    //PartType partType = mechanicalFitting.PartType;
            //    //string lala = partType.ToString();
            //    sb.Append($"inst:{fittingType} fso:hasPort inst:{fittingType} ." + "\n");
            //}
            //if ( fitting.LookupParameter("FSC_type").AsValueString() == "HeatExchanger" )


            ////New method of defining components
            //if (fitting.Category.Name == "Duct Accessories" )
            //{
            //    sb.Append($"inst:Habibi Hundige fso:hasPort inst:Habibi Hundige ." + "\n");
            //}

            //if (fitting.Symbol.LookupParameter("FSC_type") != null )
            //{
            //    string name = fitting.Symbol.LookupParameter("FSC_type").AsString();

            //    //Finding system information for each component
            //    string systemClassification = fitting.LookupParameter("System Classification").AsValueString();
            //    if (fitting.LookupParameter("System Type") != null) { string systemType = fitting.LookupParameter("System Type").AsValueString();
            //        sb.Append($"inst:{systemType} fso:hasComponent inst:{name} ." + "\n");
            //    }
            //    else { string systemType = "Ventilation";
            //        sb.Append($"inst:{systemType} fso:hasComponent inst:{name} ." + "\n");
            //    };
            //    string systemName = fitting.LookupParameter("System Name").AsString();

            //    sb.Append($"inst:{systemClassification} fso:hasComponent inst:{name} ." + "\n");
            //    sb.Append($"inst:{systemName} fso:hasComponent inst:{name} ." + "\n");


            //}

            //}

            //*****************
            ////Relationship between ventilation systems and their components WORKING
            //FilteredElementCollector ventilationSystemCollector = new FilteredElementCollector(doc);
            //ICollection<Element> ventilationSystems = ventilationSystemCollector.OfClass(typeof(MechanicalSystem)).ToElements();
            //List<MechanicalSystem> ventilationSystemList = new List<MechanicalSystem>();
            //foreach (MechanicalSystem system in ventilationSystemCollector)
            //{
            //    //Get systems
            //    DuctSystemType systemType = system.SystemType;
            //    string systemID = system.UniqueId;
            //    string systemName = system.LookupParameter("Type").AsValueString();

            //    switch (systemType)
            //    {
            //        case DuctSystemType.SupplyAir:
            //            sb.Append($"inst:{systemID} a fso:SupplySystem ." + "\n"
            //                + $"inst:{systemID} rdfs:label '{systemName}'^^xsd:string ." + "\n");
            //            break;
            //        case DuctSystemType.ReturnAir:
            //            sb.Append($"inst:{systemID} a fso:ReturnSystem ." + "\n"
            //                 + $"inst:{systemID} rdfs:label '{systemName}'^^xsd:string ." + "\n");
            //            break;
            //        case
            //       DuctSystemType.ExhaustAir:
            //            sb.Append($"inst:{systemID} a fso:ReturnSystem ." + "\n"
            //                 + $"inst:{systemID} rdfs:label '{systemName}'^^xsd:string ." + "\n");
            //            break;
            //        default:
            //            break;
            //    }

            //    ElementSet systemComponents = system.DuctNetwork;

            //    //Relate components to systems
            //    foreach (Element component in systemComponents)
            //    {
            //        string componentID = component.UniqueId;
            //        sb.Append($"inst:{systemID} fso:hasComponent inst:{componentID} ." + "\n");
            //    }
            //}
            //*****************

            //*****************
            ////Relationship between heating and cooling systems and their components WORKING
            //FilteredElementCollector hydraulicSystemCollector = new FilteredElementCollector(doc);
            //ICollection<Element> hydraulicSystems = hydraulicSystemCollector.OfClass(typeof(PipingSystem)).ToElements();
            //List<PipingSystem> hydraulicSystemList = new List<PipingSystem>();
            //foreach (PipingSystem system in hydraulicSystemCollector)
            //{
            //    //Get systems
            //    PipeSystemType systemType = system.SystemType;
            //    string systemID = system.UniqueId;
            //    string systemName = system.LookupParameter("Type").AsValueString();

            //    switch (systemType)
            //    {
            //        case PipeSystemType.SupplyHydronic:
            //            sb.Append($"inst:{systemID} a fso:SupplySystem ." + "\n"
            //                + $"inst:{systemID} rdfs:label '{systemName}'^^xsd:string ." + "\n");
            //            break;
            //        case PipeSystemType.ReturnHydronic:
            //            sb.Append($"inst:{systemID} a fso:ReturnSystem ." + "\n"
            //                 + $"inst:{systemID} rdfs:label '{systemName}'^^xsd:string ." + "\n");
            //            break;
            //        default:
            //            break;
            //    }

            //    ElementSet systemComponents = system.PipingNetwork;

            //    //Relate components to systems
            //    foreach (Element component in systemComponents)
            //    {
            //        string componentID = component.UniqueId;
            //        sb.Append($"inst:{systemID} fso:hasComponent inst:{componentID} ." + "\n");
            //    }
            //}

            //*****************



            //Get all components
            FilteredElementCollector componentCollector = new FilteredElementCollector(doc);
            ICollection<Element> components = componentCollector.OfClass(typeof(FamilyInstance)).ToElements();
            List<FamilyInstance> componentList = new List<FamilyInstance>();
            foreach (FamilyInstance component in componentCollector)
            {

                if (component.Symbol.LookupParameter("FSC_type") != null)
                {

                    ////HeatExchanger
                    //if (component.Symbol.LookupParameter("FSC_type").AsString() == "HeatExchanger")
                    //{
                    //    //Type
                    //    string componentType = component.Symbol.LookupParameter("FSC_type").AsString();
                    //    string componentID = component.UniqueId.ToString();

                    //    sb.Append($"inst:{componentID} a fpo:{componentType} ." + "\n");

                    //    //DesignHeatPower
                    //    string designHeatPowerID = component.LookupParameter("FSC_nomPower").GUID.ToString();
                    //    double designHeatPowerValue = UnitUtils.ConvertFromInternalUnits(component.LookupParameter("FSC_nomPower").AsDouble(), UnitTypeId.Watts);
                    //    sb.Append($"inst:{componentID} fpo:designHeatingPower inst:{designHeatPowerID} ." + "\n"
                    //     + $"inst:{designHeatPowerID} a fpo:DesignHeatingPower ." + "\n"
                    //     + $"inst:{designHeatPowerID} fpo:value  '{designHeatPowerValue}'^^xsd:double ." + "\n"
                    //     + $"inst:{designHeatPowerID} fpo:unit  'Watt'^^xsd:string ." + "\n");

                    //    RelatedPorts.getAllRelatedConnectors(component, componentID, sb);

                    //}

                    //Fan
                    //if (component.Symbol.LookupParameter("FSC_type").AsString() == "Fan")
                    //{
                    //    //Type
                    //    string componentType = component.Symbol.LookupParameter("FSC_type").AsString();
                    //    string componentID = component.UniqueId.ToString();

                    //    sb.Append($"inst:{componentID} a fpo:{componentType} ." + "\n");

                    //    //PressureCurve
                    //    string pressureCurveID = component.LookupParameter("FSC_pressureCurve").GUID.ToString();
                    //    string pressureCurveValue = component.LookupParameter("FSC_pressureCurve").AsString();
                    //    sb.Append($"inst:{componentID} fpo:pressureCurve inst:{pressureCurveID} ." + "\n"
                    //     + $"inst:{pressureCurveID} a fpo:PressureCurve ." + "\n"
                    //     + $"inst:{pressureCurveID} fpo:curve  '{pressureCurveValue}'^^xsd:string ." + "\n"
                    //     + $"inst:{pressureCurveID} fpo:unit  'PA:m3/h'^^xsd:string ." + "\n");

                    //    //PowerCurve
                    //    string powerCurveID = component.LookupParameter("FSC_powerCurve").GUID.ToString();
                    //    string powerCurveValue = component.LookupParameter("FSC_powerCurve").AsString();
                    //    sb.Append($"inst:{componentID} fpo:powerCurve inst:{pressureCurveID} ." + "\n"
                    //     + $"inst:{powerCurveID} a fpo:PowerCurve ." + "\n"
                    //     + $"inst:{powerCurveID} fpo:curve  '{powerCurveValue}'^^xsd:string ." + "\n"
                    //     + $"inst:{powerCurveID} fpo:unit  'PA:m3/h'^^xsd:string ." + "\n");

                    //    RelatedPorts.getAllRelatedConnectors(component, componentID, sb);

                    //}

                    //Pump
                    //if (component.Symbol.LookupParameter("FSC_type").AsString() == "Pump")
                    //{
                    //    //Type
                    //    string componentType = component.Symbol.LookupParameter("FSC_type").AsString();
                    //    string componentID = component.UniqueId.ToString();

                    //    sb.Append($"inst:{componentID} a fpo:{componentType} ." + "\n");

                    //    //PressureCurve
                    //    string pressureCurveID = component.LookupParameter("FSC_pressureCurve").GUID.ToString();
                    //    string pressureCurveValue = component.LookupParameter("FSC_pressureCurve").AsString();
                    //    sb.Append($"inst:{componentID} fpo:pressureCurve inst:{pressureCurveID} ." + "\n"
                    //     + $"inst:{pressureCurveID} a fpo:PressureCurve ." + "\n"
                    //     + $"inst:{pressureCurveID} fpo:curve  '{pressureCurveValue}'^^xsd:string ." + "\n"
                    //     + $"inst:{pressureCurveID} fpo:unit  'PA:m3/h'^^xsd:string ." + "\n");

                    //    //PowerCurve
                    //    string powerCurveID = component.LookupParameter("FSC_powerCurve").GUID.ToString();
                    //    string powerCurveValue = component.LookupParameter("FSC_powerCurve").AsString();
                    //    sb.Append($"inst:{componentID} fpo:powerCurve inst:{pressureCurveID} ." + "\n"
                    //     + $"inst:{powerCurveID} a fpo:PowerCurve ." + "\n"
                    //     + $"inst:{powerCurveID} fpo:curve  '{powerCurveValue}'^^xsd:string ." + "\n"
                    //     + $"inst:{powerCurveID} fpo:unit  'PA:m3/h'^^xsd:string ." + "\n");

                    //    RelatedPorts.getAllRelatedConnectors(component, componentID, sb);

                    //}

                    //Valve
                    if (component.Symbol.LookupParameter("FSC_type").AsString() == "MotorizedValve"|| component.Symbol.LookupParameter("FSC_type").AsString() == "BalancingValve" || component.Symbol.LookupParameter("FSC_type").AsString() == "MotorizedDamper" || component.Symbol.LookupParameter("FSC_type").AsString() == "BalancingDamper")
                    {
                        //Type
                        string componentType = component.Symbol.LookupParameter("FSC_type").AsString();
                        string componentID = component.UniqueId.ToString();

                        sb.Append($"inst:{componentID} a fpo:Valve ." + "\n");

                        //Kv
                        string kvID = component.LookupParameter("FSC_kv").GUID.ToString();
                        double kvValue = component.LookupParameter("FSC_kv").AsDouble();
                        sb.Append($"inst:{componentID} fpo:kv inst:{kvID} ." + "\n"
                         + $"inst:{kvID} a fpo:Kv ." + "\n"
                         + $"inst:{kvID} fpo:value  '{kvValue}'^^xsd:double ." + "\n");

                        //Kvs
                        string kvsID = component.LookupParameter("FSC_kvs").GUID.ToString();
                        double kvsValue = component.LookupParameter("FSC_kvs").AsDouble();
                        sb.Append($"inst:{componentID} fpo:powerCurve inst:{kvsID} ." + "\n"
                         + $"inst:{kvsID} a fpo:PowerCurve ." + "\n"
                         + $"inst:{kvsID} fpo:curve  '{kvsValue}'^^xsd:double ." + "\n");

                        RelatedPorts.getAllRelatedConnectors(component, componentID, sb);

                    }


                }





            }





            //Converting to string before post request
            string reader = sb.ToString();
            var test = HttpClientHelper.POSTDataAsync(reader);
                      
            TaskDialog.Show("Revit", sb.ToString());
            return Result.Succeeded;
        }
    }


    
}
