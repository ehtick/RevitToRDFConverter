﻿using System;
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


           
            StringBuilder sb = new StringBuilder();
            sb.Append(
                "@prefix owl: <http://www.w3.org/2002/07/owl#> ." + "\n" +
                "@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> ." + "\n" +
                "@prefix xml: <http://www.w3.org/XML/1998/namespace> ." + "\n" +
                "@prefix xsd: <http://www.w3.org/2001/XMLSchema#> ." + "\n" +
                "@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> ." + "\n" +
                "@prefix bot: <https://w3id.org/bot#> ." + "\n" +
                "@prefix fso: <http://w3id.org/fso#> ." + "\n" +
                "@prefix inst: <https://example.com/inst#> ." + "\n" +
                "@prefix fpo: <http://w3id.org/fpo#> ." + "\n"+
                "@prefix ex: <https://example.com/ex#> ." + "\n");

            //            //*************
            //            //Get projectName and assign it as buildingName for now. WORKING
            ProjectInfo projectInfo = doc.ProjectInformation;
            string buildingName = projectInfo.BuildingName;
            string buildingGuid = System.Guid.NewGuid().ToString().Replace(' ', '-');
            sb.Append($"inst:{buildingGuid} a bot:Building ." + "\n" + $"inst:{buildingGuid} rdfs:label '{buildingName}'^^xsd:string  ." + "\n");

            //            //Get all level and the building it is related to. WOKRING 
            FilteredElementCollector levelCollector = new FilteredElementCollector(doc);
            ICollection<Element> levels = levelCollector.OfClass(typeof(Level)).ToElements();
            List<Level> levelList = new List<Level>();
            foreach (Level level in levelCollector)
            {
                Level w = level as Level;
                string levelName = level.Name.Replace(' ', '-');
                string guidNumber = level.UniqueId.ToString();
                sb.Append($"inst:{guidNumber} a bot:Storey ." + "\n"
                    + $"inst:{guidNumber} rdfs:label '{levelName}'^^xsd:string ." + "\n" + $"inst:{buildingGuid} bot:hasStorey inst:{guidNumber} ." + "\n");
            }

            //            //Get all level and the building it is related to. WOKRING 
            FilteredElementCollector roomCollector = new FilteredElementCollector(doc);
            ICollection<Element> rooms = roomCollector.OfClass(typeof(SpatialElement)).ToElements();
            List<SpatialElement> roomList = new List<SpatialElement>();
            foreach (SpatialElement space in roomCollector)
            {
                SpatialElement w = space as SpatialElement;
                if (space.Category.Name == "Spaces")
                {
                    string spaceName = space.Name.Replace(' ', '-');
                    string spaceGuid = space.UniqueId.ToString();
                    string isSpaceOf = space.Level.UniqueId;

                    string designCoolingLoadID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                    double designCoolingLoad = UnitUtils.ConvertFromInternalUnits(space.LookupParameter("Design Cooling Load").AsDouble(), UnitTypeId.Watts);

                    string designHeatingLoadID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                    double designHeatingLoad = UnitUtils.ConvertFromInternalUnits(space.LookupParameter("Design Heating Load").AsDouble(), UnitTypeId.Watts);

                    string designSupplyAirflowID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                    double designSupplyAirflow = UnitUtils.ConvertFromInternalUnits(space.LookupParameter("Actual Supply Airflow").AsDouble(), UnitTypeId.LitersPerSecond);

                    string designReturnAirflowID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                    double designReturnAirflow = UnitUtils.ConvertFromInternalUnits(space.LookupParameter("Actual Return Airflow").AsDouble(), UnitTypeId.LitersPerSecond);

                    sb.Append($"inst:{spaceGuid} a bot:Space ." + "\n" +
                        $"inst:{spaceGuid} rdfs:label '{spaceName}'^^xsd:string ." + "\n" +
                        $"inst:{isSpaceOf} bot:hasSpace inst:{spaceGuid} ." + "\n" +

                        $"#Cooling Demand in {spaceName}" + "\n" +
                        $"inst:{spaceGuid} ex:designCoolingDemand inst:{designCoolingLoadID} ." + "\n" +
                        $"inst:{designCoolingLoadID} a ex:DesignCoolingDemand ." + "\n" +
                        $"inst:{designCoolingLoadID} fpo:value '{designCoolingLoad}'^^xsd:double ." + "\n" +
                        $"inst:{designCoolingLoadID} fpo:unit 'Watts'^^xsd:string ." + "\n" +

                        $"#Heating Demand in {spaceName}" + "\n" +
                        $"inst:{spaceGuid} ex:designHeatingDemand inst:{designHeatingLoadID} ." + "\n" +
                        $"inst:{designHeatingLoadID} a ex:DesignHeatingDemand ." + "\n" +
                        $"inst:{designHeatingLoadID} fpo:value '{designHeatingLoad}'^^xsd:double ." + "\n" +
                        $"inst:{designHeatingLoadID} fpo:unit 'Watts'^^xsd:string ." + "\n" +

                        $"#Supply Air Flow Demand in {spaceName}" + "\n" +
                        $"inst:{spaceGuid} ex:designSupplyAirflowDemand inst:{designSupplyAirflowID} ." + "\n" +
                        $"inst:{designSupplyAirflowID} a ex:DesignSupplyAirflowDemand ." + "\n" +
                        $"inst:{designSupplyAirflowID} fpo:value '{designSupplyAirflow}'^^xsd:double ." + "\n" +
                        $"inst:{designSupplyAirflowID} fpo:unit 'Liters Per Second'^^xsd:string ." + "\n" +

                        $"#Return Air Flow Demand in {spaceName}" + "\n" +
                        $"inst:{spaceGuid} ex:designReturnAirflowDemand inst:{designReturnAirflowID} ." + "\n" +
                        $"inst:{designReturnAirflowID} a ex:DesignReturnAirflowDemand ." + "\n" +
                        $"inst:{designReturnAirflowID} ex:designReturnAirflowDemand '{designReturnAirflow}'^^xsd:double ." + "\n" +
                        $"inst:{designReturnAirflowID} fpo:unit 'Liters Per Second'^^xsd:string ." + "\n"
);
                };
            }

            //************************* NOT NEEDED

            ////Get all rooms and assign the level to it. ROOMS NOT NECESSARY USING SPACES INSTEAD
            //FilteredElementCollector rooms = new FilteredElementCollector(doc);
            //RoomFilter filter = new RoomFilter();
            //rooms.WherePasses(filter);
            //foreach (Room room in rooms)
            //{
            //    Room w = room as Room;
            //    string roomName = room.Name.Replace(' ', '-');
            //    string isSpaceOf = room.Level.UniqueId;
            //    string roomGuid = room.UniqueId.ToString();
            //    sb.Append($"inst:{roomGuid} a bot:Space ." + "\n" + $"inst:{roomGuid} rdfs:label '{roomName}'^^xsd:string ." + "\n" + $"inst:{isSpaceOf} bot:hasSpace inst:{roomGuid} ." + "\n");
            //}

            //*****************
            //Relationship between ventilation systems and their components. WORKING
            FilteredElementCollector ventilationSystemCollector = new FilteredElementCollector(doc);
            ICollection<Element> ventilationSystems = ventilationSystemCollector.OfClass(typeof(MechanicalSystem)).ToElements();
            List<MechanicalSystem> ventilationSystemList = new List<MechanicalSystem>();
            foreach (MechanicalSystem system in ventilationSystemCollector)
            {
                //Get systems
                DuctSystemType systemType = system.SystemType;

                string systemID = system.UniqueId;
                string systemName = system.Name;
                //    ElementId superSystemType = system.GetTypeId();
                //    string superSystemName = doc.GetElement(superSystemType).LookupParameter("Family Name").AsValueString();
                //    string superSystemID = doc.GetElement(superSystemType).UniqueId;
                string fluidID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                string flowTypeID = System.Guid.NewGuid().ToString().Replace(' ', '-');

                string fluidTemperatureID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                double fluidTemperature = UnitUtils.ConvertFromInternalUnits(system.LookupParameter("Fluid TemperatureX").AsDouble(), UnitTypeId.Celsius);

                switch (systemType)
                {
                    case DuctSystemType.SupplyAir:
                        sb.Append($"inst:{systemID} a fso:SupplySystem ." + "\n" +
                            $"inst:{systemID} rdfs:label '{systemName}'^^xsd:string ." + "\n" +
                            
                            $"inst:{systemID} fso:hasFlow inst:{fluidID} ." + "\n" +
                            $"inst:{fluidID} a fso:Flow ." + "\n" +
                            $"inst:{fluidID} fpo:flowType inst:{flowTypeID} ." + "\n" +
                            $"inst:{flowTypeID} a fpo:FlowType ." + "\n" +
                            $"inst:{flowTypeID} fpo:value 'Air'^^xsd:string ." + "\n" +

                            $"inst:{fluidID} fpo:temperature inst:{fluidTemperatureID} ." + "\n" +
                            $"inst:{fluidTemperatureID} a fpo:temperature ." + "\n" +
                            $"inst:{fluidTemperatureID} fpo:value '{fluidTemperature}'^^xsd:double ." + "\n" +
                            $"inst:{fluidTemperatureID} fpo:unit 'Celcius'^^xsd:string ." + "\n");
                        break;
                    case DuctSystemType.ReturnAir:
                        sb.Append($"inst:{systemID} a fso:ReturnSystem ." + "\n"
                             + $"inst:{systemID} rdfs:label '{systemName}'^^xsd:string ." + "\n" +
                            
                             $"inst:{systemID} fso:hasFlow inst:{fluidID} ." + "\n" +
                            $"inst:{fluidID} a fso:Flow ." + "\n" +
                            $"inst:{fluidID} fpo:flowType inst:{flowTypeID} ." + "\n" +
                            $"inst:{flowTypeID} a fpo:FlowType ." + "\n" +
                            $"inst:{flowTypeID} fpo:value 'Air'^^xsd:string ." + "\n" +

                            $"inst:{fluidID} fpo:temperature inst:{fluidTemperatureID} ." + "\n" +
                            $"inst:{fluidTemperatureID} a fpo:temperature ." + "\n" +
                            $"inst:{fluidTemperatureID} fpo:value '{fluidTemperature}'^^xsd:double ." + "\n" +
                            $"inst:{fluidTemperatureID} fpo:unit 'Celcius'^^xsd:string ." + "\n");
                        break;
                    case
                   DuctSystemType.ExhaustAir:
                        sb.Append($"inst:{systemID} a fso:ReturnSystem ." + "\n"
                             + $"inst:{systemID} rdfs:label '{systemName}'^^xsd:string ." + "\n" +
                              $"inst:{systemID} fso:hasFlow inst:{fluidID} ." + "\n" +

                            $"inst:{fluidID} a fso:Flow ." + "\n" +
                            $"inst:{fluidID} fpo:temperature inst:{fluidTemperatureID} ." + "\n" +
                            $"inst:{fluidTemperatureID} a fpo:temperature ." + "\n" +
                            $"inst:{fluidTemperatureID} fpo:value '{fluidTemperature}'^^xsd:double ." + "\n" +
                            $"inst:{fluidTemperatureID} fpo:unit 'Celcius'^^xsd:string ." + "\n");
                        break;
                    default:
                        break;
                }

                ElementSet systemComponents = system.DuctNetwork;

                //Relate components to systems
                foreach (Element component in systemComponents)
                {
                    string componentID = component.UniqueId;
                    sb.Append($"inst:{systemID} fso:hasComponent inst:{componentID} ." + "\n");
                }
            }
            //*****************

            //*****************
            //Relationship between heating and cooling systems and their components.WORKING
            FilteredElementCollector hydraulicSystemCollector = new FilteredElementCollector(doc);
            ICollection<Element> hydraulicSystems = hydraulicSystemCollector.OfClass(typeof(PipingSystem)).ToElements();
            List<PipingSystem> hydraulicSystemList = new List<PipingSystem>();
            foreach (PipingSystem system in hydraulicSystemCollector)
            {
                //Get systems
                PipeSystemType systemType = system.SystemType;
                string systemID = system.UniqueId;
                string systemName = system.Name;
                ElementId superSystemType = system.GetTypeId();
                //    string superSystemName = doc.GetElement(superSystemType).LookupParameter("Family Name").AsValueString();
                //    string superSystemID = doc.GetElement(superSystemType).UniqueId;

                //Fluid
                string fluidID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                string flowTypeID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                string fluidTemperatureID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                string fluidViscosityID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                string fluidDensityID = System.Guid.NewGuid().ToString().Replace(' ', '-');

                string flowType = doc.GetElement(superSystemType).LookupParameter("Fluid Type").AsValueString();
                double fluidTemperature = UnitUtils.ConvertFromInternalUnits(system.LookupParameter("Fluid TemperatureX").AsDouble(), UnitTypeId.Celsius);
                double fluidViscosity = UnitUtils.ConvertFromInternalUnits(doc.GetElement(superSystemType).LookupParameter("Fluid Dynamic Viscosity").AsDouble(), UnitTypeId.PascalSeconds);
                double fluidDensity = UnitUtils.ConvertFromInternalUnits(doc.GetElement(superSystemType).LookupParameter("Fluid Density").AsDouble(), UnitTypeId.KilogramsPerCubicMeter);

                switch (systemType)
                {
                    case PipeSystemType.SupplyHydronic:
                        sb.Append($"inst:{systemID} a fso:SupplySystem ." + "\n"
                            + $"inst:{systemID} rdfs:label '{systemName}'^^xsd:string ." + "\n" +

                            $"inst:{systemID} fso:hasFlow inst:{fluidID} ." + "\n" +

                            $"inst:{fluidID} a fso:Flow ." + "\n" +
                            $"inst:{fluidID} fpo:flowType inst:{flowTypeID} ." + "\n" +
                            $"inst:{flowTypeID} a fpo:FlowType ." + "\n" +
                             $"inst:{flowTypeID} fpo:value '{flowType}'^^xsd:string ." + "\n" +

                            $"inst:{fluidID} fpo:temperature inst:{fluidTemperatureID} ." + "\n" +
                            $"inst:{fluidTemperatureID} a fpo:temperature ." + "\n" +
                            $"inst:{fluidTemperatureID} fpo:value '{fluidTemperature}'^^xsd:double ." + "\n" +
                            $"inst:{fluidTemperatureID} fpo:unit 'Celcius'^^xsd:string ." + "\n" +

                            $"inst:{fluidID} fpo:viscosity inst:{fluidViscosityID} ." + "\n" +
                            $"inst:{fluidViscosityID} a fpo:Viscosity ." + "\n" +
                            $"inst:{fluidViscosityID} fpo:value '{fluidViscosity}'^^xsd:double ." + "\n" +
                            $"inst:{fluidViscosityID} fpo:unit 'Pascal per second'^^xsd:string ." + "\n" +

                            $"inst:{fluidID} fpo:density inst:{fluidDensityID} ." + "\n" +
                            $"inst:{fluidDensityID} a fpo:Density ." + "\n" +
                            $"inst:{fluidDensityID} fpo:value '{fluidDensity}'^^xsd:double ." + "\n" +
                            $"inst:{fluidDensityID} fpo:unit 'Kilograms per cubic meter'^^xsd:string ." + "\n"
                            );
                        break;
                    case PipeSystemType.ReturnHydronic:
                        sb.Append($"inst:{systemID} a fso:ReturnSystem ." + "\n" +
                            $"inst:{systemID} rdfs:label '{systemName}'^^xsd:string ." + "\n" +

                            $"inst:{systemID} fso:hasFlow inst:{fluidID} ." + "\n" +

                           $"inst:{fluidID} a fso:Flow ." + "\n" +
                            $"inst:{fluidID} fpo:flowType inst:{flowTypeID} ." + "\n" +
                            $"inst:{flowTypeID} a fpo:FlowType ." + "\n" +
                            $"inst:{flowTypeID} fpo:value '{flowType}'^^xsd:string ." + "\n" +

                            $"inst:{fluidID} fpo:temperature inst:{fluidTemperatureID} ." + "\n" +
                            $"inst:{fluidTemperatureID} a fpo:Temperature ." + "\n" +
                            $"inst:{fluidTemperatureID} fpo:value '{fluidTemperature}'^^xsd:double ." + "\n" +
                            $"inst:{fluidTemperatureID} fpo:unit 'Celcius'^^xsd:string ." + "\n" +

                            $"inst:{fluidID} fpo:viscosity inst:{fluidViscosityID} ." + "\n" +
                            $"inst:{fluidViscosityID} a fpo:Viscosity ." + "\n" +
                            $"inst:{fluidViscosityID} fpo:value '{fluidViscosity}'^^xsd:double ." + "\n" +
                            $"inst:{fluidViscosityID} fpo:unit 'Pascal per second'^^xsd:string ." + "\n" +

                            $"inst:{fluidID} fpo:density inst:{fluidDensityID} ." + "\n" +
                            $"inst:{fluidDensityID} a fpo:Density ." + "\n" +
                            $"inst:{fluidDensityID} fpo:value '{fluidDensity}'^^xsd:double ." + "\n" +
                            $"inst:{fluidDensityID} fpo:unit 'Kilograms per cubic meter'^^xsd:string ." + "\n"
                            );
                        break;
                    default:
                        break;
                }

                ElementSet systemComponents = system.PipingNetwork;

                //Relate components to systems
                foreach (Element component in systemComponents)
                {
                    string componentID = component.UniqueId;
                    sb.Append($"inst:{systemID} fso:hasComponent inst:{componentID} ." + "\n");
                }
            }

            //*****************

            //Get FSC_type components
            FilteredElementCollector componentCollector = new FilteredElementCollector(doc);
            ICollection<Element> components = componentCollector.OfClass(typeof(FamilyInstance)).ToElements();
            List<FamilyInstance> componentList = new List<FamilyInstance>();
            foreach (FamilyInstance component in componentCollector)
            {

                if (component.Symbol.LookupParameter("FSC_type") != null)
                {
                    //Type
                    string componentType = component.Symbol.LookupParameter("FSC_type").AsString();
                    string componentID = component.UniqueId.ToString();
                    string revitID = component.Id.ToString();
                    sb.Append($"inst:{componentID} ex:RevitID inst:{revitID} ." + "\n");

                    //Fan
                    if (component.Symbol.LookupParameter("FSC_type").AsString() == "Fan")
                    {
                        //Type 
                        sb.Append($"inst:{componentID} a fso:{componentType} ." + "\n");

                        if (component.LookupParameter("FSC_pressureCurve") != null)
                        {
                            //PressureCurve
                            string pressureCurveID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                            string pressureCurveValue = component.LookupParameter("FSC_pressureCurve").AsString();
                            sb.Append($"inst:{componentID} fpo:pressureCurve inst:{pressureCurveID} ." + "\n"
                             + $"inst:{pressureCurveID} a fpo:PressureCurve ." + "\n"
                             + $"inst:{pressureCurveID} fpo:curve  '{pressureCurveValue}'^^xsd:string ." + "\n"
                             + $"inst:{pressureCurveID} fpo:unit  'PA:m3/h'^^xsd:string ." + "\n");
                        }

                        if (component.LookupParameter("FSC_powerCurve") != null)
                        {
                            //PowerCurve
                            string powerCurveID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                            string powerCurveValue = component.LookupParameter("FSC_powerCurve").AsString();
                            sb.Append($"inst:{componentID} fpo:powerCurve inst:{powerCurveID} ." + "\n"
                             + $"inst:{powerCurveID} a fpo:PowerCurve ." + "\n"
                             + $"inst:{powerCurveID} fpo:curve  '{powerCurveValue}'^^xsd:string ." + "\n"
                             + $"inst:{powerCurveID} fpo:unit  'PA:m3/h'^^xsd:string ." + "\n");
                        }

                    }
                    //Pump
                    if (component.Symbol.LookupParameter("FSC_type").AsString() == "Pump")
                    {
                        //Type 
                        sb.Append($"inst:{componentID} a fso:{componentType} ." + "\n");

                        if (component.LookupParameter("FSC_pressureCurve") != null)
                        {
                            //PressureCurve
                            string pressureCurveID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                            string pressureCurveValue = component.LookupParameter("FSC_pressureCurve").AsString();
                            sb.Append($"inst:{componentID} fpo:pressureCurve inst:{pressureCurveID} ." + "\n"
                             + $"inst:{pressureCurveID} a fpo:PressureCurve ." + "\n"
                             + $"inst:{pressureCurveID} fpo:curve  '{pressureCurveValue}'^^xsd:string ." + "\n"
                             + $"inst:{pressureCurveID} fpo:unit  'PA:m3/h'^^xsd:string ." + "\n");
                        }

                        if (component.LookupParameter("FSC_powerCurve") != null)
                        {
                            //PowerCurve
                            string powerCurveID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                            string powerCurveValue = component.LookupParameter("FSC_powerCurve").AsString();
                            sb.Append($"inst:{componentID} fpo:powerCurve inst:{powerCurveID} ." + "\n"
                             + $"inst:{powerCurveID} a fpo:PowerCurve ." + "\n"
                             + $"inst:{powerCurveID} fpo:curve  '{powerCurveValue}'^^xsd:string ." + "\n"
                             + $"inst:{powerCurveID} fpo:unit  'PA:m3/h'^^xsd:string ." + "\n");
                        }
                    }

                    //Valve
                    if (component.Symbol.LookupParameter("FSC_type").AsString() == "MotorizedValve" || component.Symbol.LookupParameter("FSC_type").AsString() == "BalancingValve")
                    {
                        //Type 
                        sb.Append($"inst:{componentID} a fso:{componentType} ." + "\n"
                        + $"fso:{componentType} rdfs:subClassOf fso:Valve ." + "\n");

                        if (component.LookupParameter("FSC_kv") != null)
                        {
                            //Kv
                            string kvID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                            double kvValue = component.LookupParameter("FSC_kv").AsDouble();
                            sb.Append($"inst:{componentID} fpo:kv inst:{kvID} ." + "\n"
                             + $"inst:{kvID} a fpo:Kv ." + "\n"
                             + $"inst:{kvID} fpo:value  '{kvValue}'^^xsd:double ." + "\n");
                        }

                        if (component.LookupParameter("FSC_kvs") != null)
                        {
                            //Kvs
                            string kvsID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                            double kvsValue = component.LookupParameter("FSC_kvs").AsDouble();
                            sb.Append($"inst:{componentID} fpo:kvs inst:{kvsID} ." + "\n"
                                 + $"inst:{kvsID} a fpo:Kvs ." + "\n"
                                 + $"inst:{kvsID} fpo:value  '{kvsValue}'^^xsd:double ." + "\n");
                        }
                    }

                    //Shunt
                    if (component.Symbol.LookupParameter("FSC_type").AsString() == "Shunt")
                    {
                        //Type 
                        sb.Append($"inst:{componentID} a fso:{componentType} ." + "\n"
                        + $"fso:{componentType} rdfs:subClassOf fpo:Valve ." + "\n");

                        if (component.LookupParameter("FSC_hasCheckValve") != null)
                        {
                            //hasCheckValve
                            string hasCheckValveID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                            string hasCheckValveValue = component.LookupParameter("FSC_hasCheckValve").AsValueString();
                            sb.Append($"inst:{componentID} fpo:checkValve inst:{hasCheckValveID} ." + "\n"
                             + $"inst:{hasCheckValveID} a fpo:CheckValve ." + "\n"
                             + $"inst:{hasCheckValveID} fpo:value  '{hasCheckValveValue}'^^xsd:string ." + "\n");
                        }
                    }

                    //Damper
                    if (component.Symbol.LookupParameter("FSC_type").AsString() == "MotorizedDamper" || component.Symbol.LookupParameter("FSC_type").AsString() == "BalancingDamper")
                    {
                        //Type 
                        sb.Append($"inst:{componentID} a fso:{componentType} ." + "\n"
                        + $"fso:{componentType} rdfs:subClassOf fpo:Damper ." + "\n");

                        if (component.LookupParameter("FSC_kv") != null)
                        {
                            //Kv
                            string kvID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                            double kvValue = component.LookupParameter("FSC_kv").AsDouble();
                            sb.Append($"inst:{componentID} fpo:kv inst:{kvID} ." + "\n"
                             + $"inst:{kvID} a fpo:Kv ." + "\n"
                             + $"inst:{kvID} fpo:value  '{kvValue}'^^xsd:double ." + "\n");
                        }

                        if (component.LookupParameter("FSC_kvs") != null)
                        {
                            //Kvs
                            string kvsID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                            double kvsValue = component.LookupParameter("FSC_kvs").AsDouble();
                            sb.Append($"inst:{componentID} fpo:kvs inst:{kvsID} ." + "\n"
                             + $"inst:{kvsID} a fpo:Kvs ." + "\n"
                             + $"inst:{kvsID} fpo:value  '{kvsValue}'^^xsd:double ." + "\n");
                        }
                    }

                    //Pipe fittings
                    if (component.Category.Name == "Pipe Fittings")
                    {
                        string fittingType = ((MechanicalFitting)component.MEPModel).PartType.ToString();
                        sb.Append($"inst:{componentID} a fso:{fittingType} ." + "\n");

                        if (fittingType.ToString() == "Tee")
                        {
                            //MaterialType
                            string materialTypeID = System.Guid.NewGuid().ToString().Replace(' ', '-'); ;
                            string materialTypeValue = component.Name;
                            sb.Append($"inst:{componentID} fpo:materialType inst:{materialTypeID} ." + "\n"
                             + $"inst:{materialTypeID} a fpo:MaterialType ." + "\n"
                             + $"inst:{materialTypeID} fpo:value '{materialTypeValue}'^^xsd:string ." + "\n");

                        }

                        if (fittingType.ToString() == "Elbow")
                        {
                            if (component.LookupParameter("Angle") != null)
                            {
                                //Angle
                                string angleID = System.Guid.NewGuid().ToString().Replace(' ', '-'); ;
                                double angleValue = UnitUtils.ConvertFromInternalUnits(component.LookupParameter("Angle").AsDouble(), UnitTypeId.Degrees);
                                sb.Append($"inst:{componentID} fpo:angle inst:{angleID} ." + "\n"
                                 + $"inst:{angleID} a fpo:Angle ." + "\n"
                                 + $"inst:{angleID} fpo:value '{angleValue}'^^xsd:double ." + "\n"
                                 + $"inst:{angleID} fpo:unit 'Degree'^^xsd:string ." + "\n");
                            }

                            //MaterialType
                            string materialTypeID = System.Guid.NewGuid().ToString().Replace(' ', '-'); ;
                            string materialTypeValue = component.Name;
                            sb.Append($"inst:{componentID} fpo:materialType inst:{materialTypeID} ." + "\n"
                             + $"inst:{materialTypeID} a fpo:MaterialType ." + "\n"
                             + $"inst:{materialTypeID} fpo:value '{materialTypeValue}'^^xsd:string ." + "\n");
                        }

                        if (fittingType.ToString() == "Transition")
                        {
                            //MaterialType
                            string materialTypeID = System.Guid.NewGuid().ToString().Replace(' ', '-'); ;
                            string materialTypeValue = component.Name;
                            sb.Append($"inst:{componentID} fpo:materialType inst:{materialTypeID} ." + "\n"
                             + $"inst:{materialTypeID} a fpo:MaterialType ." + "\n"
                             + $"inst:{materialTypeID} fpo:value  '{materialTypeValue}'^^xsd:string ." + "\n");

                            if (component.LookupParameter("OffsetHeight") != null && component.LookupParameter("OffsetHeight").AsDouble() > 0)
                            {
                                //Length
                                string lengthID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                                double lengthValue = UnitUtils.ConvertFromInternalUnits(component.LookupParameter("OffsetHeight").AsDouble(), UnitTypeId.Meters);
                                sb.Append($"inst:{componentID} fpo:length inst:{lengthID} ." + "\n"
                                 + $"inst:{lengthID} a fpo:Length ." + "\n"
                                 + $"inst:{lengthID} fpo:value '{lengthValue}'^^xsd:double ." + "\n"
                                 + $"inst:{lengthID} fpo:unit 'Meter'^^xsd:string ." + "\n");
                            }
                            else {
                                string lengthID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                                double lengthValue = 0.02;
                                sb.Append($"inst:{componentID} fpo:length inst:{lengthID} ." + "\n"
                               + $"inst:{lengthID} a fpo:Length ." + "\n"
                               + $"inst:{lengthID} fpo:value '{lengthValue}'^^xsd:double ." + "\n"
                               + $"inst:{lengthID} fpo:unit 'Meter'^^xsd:string ." + "\n");
                            }


                        }
                    }

                    //Duct fittings
                    if (component.Category.Name == "Duct Fittings")
                    {

                        string fittingType = ((MechanicalFitting)component.MEPModel).PartType.ToString();
                        sb.Append($"inst:{componentID} a fso:{fittingType} ." + "\n");

                        if (fittingType.ToString() == "Tee")
                        {

                            //MaterialType
                            string materialTypeID = System.Guid.NewGuid().ToString().Replace(' ', '-'); ;
                            string materialTypeValue = component.Name;
                            sb.Append($"inst:{componentID} fpo:materialType inst:{materialTypeID} ." + "\n"
                             + $"inst:{materialTypeID} a fpo:MaterialType ." + "\n"
                             + $"inst:{materialTypeID} fpo:value  '{materialTypeValue}'^^xsd:string ." + "\n");
                        }

                        if (fittingType.ToString() == "Elbow")
                        {
                            if (component.LookupParameter("Angle") != null)
                            {
                                //Angle
                                string angleID = System.Guid.NewGuid().ToString().Replace(' ', '-'); ;
                                double angleValue = UnitUtils.ConvertFromInternalUnits(component.LookupParameter("Angle").AsDouble(), UnitTypeId.Degrees);
                                sb.Append($"inst:{componentID} fpo:angle inst:{angleID} ." + "\n"
                                 + $"inst:{angleID} a fpo:Angle ." + "\n"
                                 + $"inst:{angleID} fpo:value  '{angleValue}'^^xsd:double ." + "\n"
                                 + $"inst:{angleID} fpo:unit  'Degree'^^xsd:string ." + "\n");
                            }
                            //MaterialType
                            string materialTypeID = System.Guid.NewGuid().ToString().Replace(' ', '-'); ;
                            string materialTypeValue = component.Name;
                            sb.Append($"inst:{componentID} fpo:materialType inst:{materialTypeID} ." + "\n"
                             + $"inst:{materialTypeID} a fpo:MaterialType ." + "\n"
                             + $"inst:{materialTypeID} fpo:value  '{materialTypeValue}'^^xsd:string ." + "\n");

                        }

                        if (fittingType.ToString() == "Transition")
                        {
                            //MaterialType
                            string materialTypeID = System.Guid.NewGuid().ToString().Replace(' ', '-'); ;
                            string materialTypeValue = component.Name;
                            sb.Append($"inst:{componentID} fpo:materialType inst:{materialTypeID} ." + "\n"
                             + $"inst:{materialTypeID} a fpo:MaterialType ." + "\n"
                             + $"inst:{materialTypeID} fpo:value '{materialTypeValue}'^^xsd:string ." + "\n");

                            if (component.LookupParameter("OffsetHeight") != null && component.LookupParameter("OffsetHeight").AsDouble() > 0)
                            {
                                //Length
                                string lengthID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                                double lengthValue = UnitUtils.ConvertFromInternalUnits(component.LookupParameter("OffsetHeight").AsDouble(), UnitTypeId.Meters);
                                sb.Append($"inst:{componentID} fpo:length inst:{lengthID} ." + "\n"
                                 + $"inst:{lengthID} a fpo:Length ." + "\n"
                                 + $"inst:{lengthID} fpo:value '{lengthValue}'^^xsd:double ." + "\n"
                                 + $"inst:{lengthID} fpo:unit 'Meter'^^xsd:string ." + "\n");
                            }
                            else
                            {
                                string lengthID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                                double lengthValue = 0.02;
                                sb.Append($"inst:{componentID} fpo:length inst:{lengthID} ." + "\n"
                               + $"inst:{lengthID} a fpo:Length ." + "\n"
                               + $"inst:{lengthID} fpo:value '{lengthValue}'^^xsd:double ." + "\n"
                               + $"inst:{lengthID} fpo:unit 'Meter'^^xsd:string ." + "\n");
                            }
                        }

                    }


                    //Radiator
                    if (component.Symbol.LookupParameter("FSC_type").AsString() == "Radiator")
                    {
                        //Type
                        sb.Append($"inst:{componentID} a fso:SpaceHeater ." + "\n");

                        //DesignHeatPower
                        string designHeatPowerID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                        double designHeatPowerValue = UnitUtils.ConvertFromInternalUnits(component.LookupParameter("FSC_nomPower").AsDouble(), UnitTypeId.Watts);
                        sb.Append($"inst:{componentID} fpo:designHeatingPower inst:{designHeatPowerID} ." + "\n"
                         + $"inst:{designHeatPowerID} a fpo:DesignHeatingPower ." + "\n"
                         + $"inst:{designHeatPowerID} fpo:value  '{designHeatPowerValue}'^^xsd:double ." + "\n"
                         + $"inst:{designHeatPowerID} fpo:unit  'Watts'^^xsd:string ." + "\n");

                            if ( component.Space != null)
                            {
                                //string s = component.Space.Name;
                                string relatedRoomID = component.Space.UniqueId.ToString();
                                sb.Append($"inst:{componentID} fso:transfersHeatTo inst:{relatedRoomID} ." + "\n");
                            }
                        

                    }

                    //AirTerminal
                    if (component.Symbol.LookupParameter("FSC_type").AsString() == "AirTerminal")
                    {
                        //Type
                        sb.Append($"inst:{componentID} a fso:{componentType} ." + "\n");

                        if (component.LookupParameter("System Classification").AsString() == "Return Air")
                        {
                            //AirTerminalType
                            string airTerminalTypeID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                            string airTerminalTypeValue = "outlet";
                            sb.Append($"inst:{componentID} fpo:airTerminalType inst:{airTerminalTypeID} ." + "\n"
                             + $"inst:{airTerminalTypeID} a fpo:AirTerminalType ." + "\n"
                             + $"inst:{airTerminalTypeID} fpo:value '{airTerminalTypeValue}'^^xsd:string ." + "\n");
                            
                            //Relation to room and space
                            string relatedRoomID = component.Space.UniqueId.ToString();
                            sb.Append($"inst:{relatedRoomID} fso:suppliesFluidTo inst:{componentID} ." + "\n");

                            //Adding a fictive port the airterminal which is not included in Revit
                            string connectorID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                            sb.Append($"inst:{componentID} fso:hasPort inst:{connectorID} ." + "\n"
                                + $"inst:{connectorID} a fso:Port ." + "\n");

                            //Diameter to fictive port 

                            //FlowDirection to fictive port
                            string connectorDirectionID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                            string connectorDirection = "In";

                            sb.Append($"inst:{connectorID} fpo:flowDirection inst:{connectorDirectionID} ." + "\n"
                                                    + $"inst:{connectorDirectionID} a fpo:FlowDirection ." + "\n"
                                                    + $"inst:{connectorDirectionID} fpo:value '{connectorDirection}'^^xsd:string ." + "\n");
                        }
                    

                        if (component.LookupParameter("System Classification").AsString() == "Supply Air")
                        {
                            //AirTerminalType
                            string airTerminalTypeID = System.Guid.NewGuid().ToString().Replace(' ', '-'); ;
                            string airTerminalTypeValue = "inlet";
                            sb.Append($"inst:{componentID} fpo:airTerminalType inst:{airTerminalTypeID} ." + "\n"
                             + $"inst:{airTerminalTypeID} a fpo:AirTerminalType ." + "\n"
                             + $"inst:{airTerminalTypeID} fpo:value '{airTerminalTypeValue}'^^xsd:string ." + "\n");

                            string relatedRoomID = component.Space.UniqueId.ToString();
                            sb.Append($"inst:{componentID} fso:suppliesFluidTo inst:{relatedRoomID} ." + "\n");

                            //Adding a fictive port the airterminal which is not included in Revit
                            string connectorID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                            sb.Append($"inst:{componentID} fso:hasPort inst:{connectorID} ." + "\n"
                                + $"inst:{connectorID} a fso:Port ." + "\n");

                            //FlowDirection
                            string connectorDirectionID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                            string connectorDirection = "Out";

                            sb.Append($"inst:{connectorID} fpo:flowDirection inst:{connectorDirectionID} ." + "\n"
                                                    + $"inst:{connectorDirectionID} a fpo:FlowDirection ." + "\n"
                                                    + $"inst:{connectorDirectionID} fpo:value '{connectorDirection}'^^xsd:string ." + "\n");


                            //Fictive pressureDrop
                            string pressureDropID = System.Guid.NewGuid().ToString().Replace(' ', '-'); ;
                            double pressureDropValue = 5;
                            sb.Append($"inst:{connectorID} fpo:pressureDrop inst:{pressureDropID} ." + "\n"
                           + $"inst:{pressureDropID} a fpo:PressureDrop ." + "\n"
                           + $"inst:{pressureDropID} fpo:value '{pressureDropValue}'^^xsd:double ." + "\n"
                           + $"inst:{pressureDropID} fpo:unit 'Pascal'^^xsd:string ." + "\n");

                            //if (component.LookupParameter("Flow") != null)
                            //{
                            //    //Flow rate
                            //    string flowID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                            //    double flowValue = UnitUtils.ConvertFromInternalUnits(component.LookupParameter("Flow").AsDouble(), UnitTypeId.LitersPerSecond);
                            //    sb.Append($"inst:{connectorID} fpo:flowRate inst:{flowID} ." + "\n"
                            //     + $"inst:{flowID} a fpo:FlowRate ." + "\n"
                            //     + $"inst:{flowID} fpo:value '{flowValue}'^^xsd:double ." + "\n"
                            //     + $"inst:{flowID} fpo:unit 'Liters per second'^^xsd:string ." + "\n");
                            //}

                        

                            //{ SLETTES
                            //    ////Pressure Drop
                            //    //string pressureDropID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                            //    ////double pressureDropValue = UnitUtils.ConvertFromInternalUnits(component.LookupParameter("Pressure loss").AsDouble(), UnitTypeId.Pascals); VIRKER IKKE PÅ FREDERIKSBERG
                            //    //double pressureDropValue = 0; //Midlertidig på Frederiksberg skole

                            //    //sb.Append($"inst:{componentID} fpo:pressureDrop inst:{pressureDropID} ." + "\n"
                            //    // + $"inst:{pressureDropID} a fpo:PressureDrop ." + "\n"
                            //    // + $"inst:{pressureDropID} fpo:value '{pressureDropValue}'^^xsd:double ." + "\n"
                            //    // + $"inst:{pressureDropID} fpo:unit 'Pascal'^^xsd:string ." + "\n");

                            //}
                        }

                        ////FlowRate  SLETTES
                        //string flowrateID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                        //double flowrateValue = UnitUtils.ConvertFromInternalUnits(component.LookupParameter("Flow").AsDouble(), UnitTypeId.LitersPerSecond);
                        //sb.Append($"inst:{componentID} fpo:flowRate inst:{flowrateID} ." + "\n"
                        // + $"inst:{flowrateID} a fpo:FlowRate ." + "\n"
                        // + $"inst:{flowrateID} fpo:value  '{flowrateValue}'^^xsd:double ." + "\n"
                        // + $"inst:{flowrateID} fpo:unit  'm3/h'^^xsd:string ." + "\n");

                    }

                    if (component.Symbol.LookupParameter("FSC_type").AsString() != "HeatExchanger")
                    {
                        RelatedPorts.FamilyInstanceConnectors(component, revitID, componentID, sb);
                    }
                }
            }

            //************************

            //*****************

            ////Get FSC HeatExchanger_type components
            FilteredElementCollector heatExchangerCollector = new FilteredElementCollector(doc);
            ICollection<Element> heatExchangers = heatExchangerCollector.OfClass(typeof(FamilyInstance)).ToElements();
            List<FamilyInstance> heatExchangerList = new List<FamilyInstance>();
            foreach (FamilyInstance component in heatExchangerCollector)
            {

                if (component.Symbol.LookupParameter("FSC_type") != null)
                {
                

                    ////HeatExchanger
                    if (component.Symbol.LookupParameter("FSC_type").AsString() == "HeatExchanger")
                    {
                        //Type
                        string componentType = component.Symbol.LookupParameter("FSC_type").AsString();
                        string componentID = component.UniqueId.ToString();
                        string revitID = component.Id.ToString();
                        sb.Append($"inst:{componentID} ex:RevitID inst:{revitID} ." + "\n");
                        sb.Append($"inst:{componentID} a fso:{componentType} ." + "\n");

                        if (component.LookupParameter("FSC_nomPower") != null)
                        {
                            //DesignHeatPower
                            string designHeatPowerID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                            double designHeatPowerValue = UnitUtils.ConvertFromInternalUnits(component.LookupParameter("FSC_nomPower").AsDouble(), UnitTypeId.Watts);
                            sb.Append($"inst:{componentID} fpo:designHeatingPower inst:{designHeatPowerID} ." + "\n"
                             + $"inst:{designHeatPowerID} a fpo:DesignHeatingPower ." + "\n"
                             + $"inst:{designHeatPowerID} fpo:value  '{designHeatPowerValue}'^^xsd:double ." + "\n"
                             + $"inst:{designHeatPowerID} fpo:unit  'Watts'^^xsd:string ." + "\n");
                        }

                        RelatedPorts.HeatExchangerConnectors(component, componentID, sb);

                    }
                }
            }

            //************************

            //Get all pipes 
            FilteredElementCollector pipeCollector = new FilteredElementCollector(doc);
            ICollection<Element> pipes = pipeCollector.OfClass(typeof(Pipe)).ToElements();
            List<Pipe> pipeList = new List<Pipe>();
            foreach (Pipe component in pipeCollector)
            {
                Pipe w = component as Pipe;

                //Type
                string componentID = component.UniqueId.ToString();
                string revitID = component.Id.ToString();
                sb.Append(
                    $"inst:{componentID} a fso:Pipe ." + "\n" + 
                    $"inst:{componentID} ex:RevitID inst:{revitID} ." + "\n" );

                if (component.PipeType.Roughness != null)
                {
                    //Roughness
                    string roughnessID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                    double rougnessValue = component.PipeType.Roughness;
                    sb.Append($"inst:{componentID} fpo:roughness inst:{roughnessID} ." + "\n"
                     + $"inst:{roughnessID} a fpo:Roughness ." + "\n"
                     + $"inst:{roughnessID} fpo:value '{rougnessValue}'^^xsd:double ." + "\n" +
                     $"inst:{roughnessID} fpo:unit 'Meter'^^xsd:string ." + "\n");
                }
                if (component.LookupParameter("Length") != null)
                {
                    //Length
                    string lengthID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                    double lengthValue = UnitUtils.ConvertFromInternalUnits(component.LookupParameter("Length").AsDouble(), UnitTypeId.Meters);
                    sb.Append($"inst:{componentID} fpo:length inst:{lengthID} ." + "\n"
                     + $"inst:{lengthID} a fpo:Length ." + "\n"
                     + $"inst:{lengthID} fpo:value '{lengthValue}'^^xsd:double ." + "\n"
                     + $"inst:{lengthID} fpo:unit 'Meter'^^xsd:string ." + "\n");
                }
                //if (component.LookupParameter("Outside Diameter") != null)
                //{
                //    //Outside diameter
                //    string outsideDiameterID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                //    double outsideDiameterValue = UnitUtils.ConvertFromInternalUnits(component.LookupParameter("Outside Diameter").AsDouble(), UnitTypeId.Meters);
                //    sb.Append($"inst:{componentID} fpo:hydraulicDiameter inst:{outsideDiameterID} ." + "\n"
                //     + $"inst:{outsideDiameterID} a fpo:HydraulicDiameter ." + "\n"
                //     + $"inst:{outsideDiameterID} fpo:value '{outsideDiameterValue}'^^xsd:double ." + "\n"
                //     + $"inst:{outsideDiameterID} fpo:unit 'Meter'^^xsd:string ." + "\n");
                //}
                ////Inside diameter
                //string insideDiameterID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                //double insideDiameterValue = UnitUtils.ConvertFromInternalUnits(component.LookupParameter("Inside Diameter").AsDouble(), UnitTypeId.Meters);
                //sb.Append($"inst:{componentID} fpo:insideDiameter inst:{insideDiameterID} ." + "\n"
                // + $"inst:{insideDiameterID} a fpo:InsideDiameter ." + "\n"
                // + $"inst:{insideDiameterID} fpo:value '{insideDiameterValue}'^^xsd:double ." + "\n"
                // + $"inst:{insideDiameterID} fpo:unit 'meter'^^xsd:string ." + "\n");

                ////Wallthickness
                //string wallThicknessID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                //double wallThicknessValue = outsideDiameterValue - insideDiameterValue;
                //sb.Append($"inst:{componentID} fpo:wallThickness inst:{wallThicknessID} ." + "\n"
                // + $"inst:{wallThicknessID} a fpo:WallThickness ." + "\n"
                // + $"inst:{wallThicknessID} fpo:value '{wallThicknessValue}'^^xsd:double ." + "\n"
                // + $"inst:{wallThicknessID} fpo:unit 'meter'^^xsd:string ." + "\n");

                //MaterialType
                string materialTypeID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                string materialTypeValue = component.Name;
                sb.Append($"inst:{componentID} fpo:materialType inst:{materialTypeID} ." + "\n"
                 + $"inst:{materialTypeID} a fpo:MaterialType ." + "\n"
                 + $"inst:{materialTypeID} fpo:value '{materialTypeValue}'^^xsd:string ." + "\n");

 
                RelatedPorts.PipeConnectors(component, componentID, sb);
            }

            //************************

            //Get all ducts 
            FilteredElementCollector ductCollector = new FilteredElementCollector(doc);
            ICollection<Element> ducts = ductCollector.OfClass(typeof(Duct)).ToElements();
            List<Duct> ductList = new List<Duct>();
            foreach (Duct component in ductCollector)
            {
                Duct w = component as Duct;

                //Type
                string componentID = component.UniqueId.ToString();
                string revitID = component.Id.ToString();
               
                sb.Append(
                    $"inst:{componentID} a fso:Duct ." + "\n" +
                    $"inst:{componentID} ex:RevitID inst:{revitID} ." + "\n");


                if (component.DuctType.Roughness != null)
                {
                    //Roughness
                    string roughnessID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                    double rougnessValue = component.DuctType.Roughness;
                    sb.Append($"inst:{componentID} fpo:roughness inst:{roughnessID} ." + "\n"
                     + $"inst:{roughnessID} a fpo:Roughness ." + "\n"
                     + $"inst:{roughnessID} fpo:value '{rougnessValue}'^^xsd:double ." + "\n" +
                     $"inst:{roughnessID} fpo:unit 'Meter'^^xsd:string ." + "\n");
                }

                if (component.LookupParameter("Length") != null)
                {
                    //Length
                    string lengthID = System.Guid.NewGuid().ToString().Replace(' ', '-'); ;
                    double lengthValue = UnitUtils.ConvertFromInternalUnits(component.LookupParameter("Length").AsDouble(), UnitTypeId.Meters);
                    sb.Append($"inst:{componentID} fpo:length inst:{lengthID} ." + "\n"
                     + $"inst:{lengthID} a fpo:Length ." + "\n"
                     + $"inst:{lengthID} fpo:value '{lengthValue}'^^xsd:double ." + "\n"
                     + $"inst:{lengthID} fpo:unit 'Meter'^^xsd:string ." + "\n");
                }

                if (component.LookupParameter("Hydraulic Diameter") != null)
                {
                    //Outside diameter
                    string outsideDiameterID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                    double outsideDiameterValue = UnitUtils.ConvertFromInternalUnits(component.LookupParameter("Hydraulic Diameter").AsDouble(), UnitTypeId.Meters);
                    sb.Append($"inst:{componentID} fpo:hydraulicDiameter inst:{outsideDiameterID} ." + "\n"
                     + $"inst:{outsideDiameterID} a fpo:HydraulicDiameter ." + "\n"
                     + $"inst:{outsideDiameterID} fpo:value '{outsideDiameterValue}'^^xsd:double ." + "\n"
                     + $"inst:{outsideDiameterID} fpo:unit 'meter'^^xsd:string ." + "\n");
                }

              
                    //MaterialType
                    string materialTypeID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                    string materialTypeValue = component.Name;
                    sb.Append($"inst:{componentID} fpo:materialType inst:{materialTypeID} ." + "\n"
                     + $"inst:{materialTypeID} a fpo:MaterialType ." + "\n"
                     + $"inst:{materialTypeID} fpo:value '{materialTypeValue}'^^xsd:string ." + "\n");
           

                if (component.LookupParameter("Loss Coefficient") != null)
                {
                    //frictionFactor 
                    string frictionFactorID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                    double frictionFactorValue = component.LookupParameter("Loss Coefficient").AsDouble();
                    sb.Append($"inst:{componentID} fpo:frictionFactor inst:{frictionFactorID} ." + "\n"
                     + $"inst:{frictionFactorID} a fpo:FrictionFactor ." + "\n"
                     + $"inst:{frictionFactorID} fpo:value '{frictionFactorValue}'^^xsd:double ." + "\n");
                }

                if (component.LookupParameter("Friction") != null)
                {
                    //friction
                    string frictionID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                    double frictionIDValue = component.LookupParameter("Friction").AsDouble();
                    sb.Append($"inst:{componentID} fpo:friction inst:{frictionID} ." + "\n"
                     + $"inst:{frictionID} a fpo:Friction ." + "\n"
                     + $"inst:{frictionID} fpo:value '{frictionIDValue}'^^xsd:double ." + "\n"
                     + $"inst:{frictionID} fpo:unit 'Pascal per meter'^^xsd:string ." + "\n");
                }

                RelatedPorts.DuctConnectors(component, componentID, sb);
            }

            //************************




            //Converting to string before post request
            string reader = sb.ToString();
            var test = HttpClientHelper.POSTDataAsync(reader);

            //TaskDialog.Show("Revit", sb.ToString());
            return Result.Succeeded;
        }
    }


    
}
