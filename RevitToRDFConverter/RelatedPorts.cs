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
    class RelatedPorts
    {
        public static StringBuilder FamilyInstanceConnectors(FamilyInstance component, string componentID, StringBuilder sb)
        {
            //Port type
            ConnectorSet connectorSet = component.MEPModel.ConnectorManager.Connectors;

            string connectorID = "";
            string connectorDirectionID = "";
            string connectorDirection = "";
            double connectorOuterDiameter = 0;
            string connectedConnectorID = "";
            string connectedConnectorDirection = "";
            string connectorOuterDiameterID = "";

            foreach (Connector connector in connectorSet)
            {
                //Type
                connectorID = componentID + "-" + connector.Id;
                sb.Append($"inst:{componentID} fso:hasPort inst:{connectorID} ." + "\n"
                    + $"inst:{connectorID} a fso:Port ." + "\n");
                if (Domain.DomainHvac == connector.Domain || Domain.DomainPiping == connector.Domain)
                {
                    //FlowDirection
                    connectorDirectionID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                    connectorDirection = connector.Direction.ToString();

                    sb.Append($"inst:{connectorID} fpo:flowDirection inst:{connectorDirectionID} ." + "\n"
                                            + $"inst:{connectorDirectionID} a fpo:FlowDirection ." + "\n"
                                            + $"inst:{connectorDirectionID} fpo:value '{connectorDirection}'^^xsd:string ." + "\n");

                    if (connector.Shape.ToString() == "Round")
                    {
                        connectorOuterDiameterID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                        connectorOuterDiameter = UnitUtils.ConvertFromInternalUnits(connector.Radius * 2, UnitTypeId.Meters);
                        sb.Append($"inst:{connectorID} fpo:outerDiameter inst:{connectorOuterDiameterID} ." + "\n" +
                            $"inst:{connectorOuterDiameterID} fpo:value '{connectorOuterDiameter}'^^xsd:double ." + "\n" +
                            $"inst:{connectorOuterDiameterID} fpo:unit 'meter'^^xsd:string  ." + "\n");

                    }
                    //Port relationship to other ports
                    ConnectorSet joinedconnectors = connector.AllRefs;
                    if (connectorDirection == "Out")
                    {
                        foreach (Connector connectedConnector in joinedconnectors)
                        {
                            connectedConnectorID = connectedConnector.Owner.UniqueId.ToString() + "-" + connectedConnector.Id.ToString();

                            if (Domain.DomainHvac == connectedConnector.Domain && connector.DuctSystemType == DuctSystemType.SupplyAir
                            || (Domain.DomainPiping == connectedConnector.Domain && connector.PipeSystemType == PipeSystemType.SupplyHydronic))
                            {
                                connectedConnectorDirection = connectedConnector.Direction.ToString();

                                sb.Append($"inst:{connectorID} fso:suppliesFluidTo inst:{connectedConnectorID} ." + "\n"
                                    + $"inst:{connectedConnectorID} a fso:Port ." + "\n");

                            }
                            if (Domain.DomainHvac == connectedConnector.Domain && connector.DuctSystemType == DuctSystemType.ReturnAir
                            || Domain.DomainPiping == connectedConnector.Domain && connector.PipeSystemType == PipeSystemType.ReturnHydronic)
                            {
                                connectedConnectorDirection = connectedConnector.Direction.ToString();

                                sb.Append($"inst:{connectorID} fso:returnsFluidTo inst:{connectedConnectorID} ." + "\n"
                                    + $"inst:{connectedConnectorID} a fso:Port ." + "\n");

                            }



                        }
                    }
                }
            }

            return sb;
        }


        public static StringBuilder PipeConnectors(Pipe component, string componentID, StringBuilder sb)
        {
            //Port type
            ConnectorSet connectorSet = component.ConnectorManager.Connectors;

            string connectorID = "";
            string connectorDirectionID = "";
            string connectorDirection = "";
            double connectorOuterDiameter = 0;
            string connectedConnectorID = "";
            string connectedConnectorDirection = "";
            string connectorOuterDiameterID = "";

            foreach (Connector connector in connectorSet)
            {
                //Type
                connectorID = componentID + "-" + connector.Id;
                sb.Append($"inst:{componentID} fso:hasPort inst:{connectorID} ." + "\n"
                    + $"inst:{connectorID} a fso:Port ." + "\n");
                if (Domain.DomainHvac == connector.Domain || Domain.DomainPiping == connector.Domain)
                {
                    //FlowDirection
                    connectorDirectionID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                    connectorDirection = connector.Direction.ToString();

                    sb.Append($"inst:{connectorID} fpo:flowDirection inst:{connectorDirectionID} ." + "\n"
                                            + $"inst:{connectorDirectionID} a fpo:FlowDirection ." + "\n"
                                            + $"inst:{connectorDirectionID} fpo:value '{connectorDirection}'^^xsd:string ." + "\n");

                    if (connector.Shape.ToString() == "Round")
                    {
                        connectorOuterDiameterID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                        connectorOuterDiameter = UnitUtils.ConvertFromInternalUnits(connector.Radius * 2, UnitTypeId.Meters);
                        sb.Append($"inst:{connectorID} fpo:outerDiameter inst:{connectorOuterDiameterID} ." + "\n" +
                            $"inst:{connectorOuterDiameterID} fpo:value '{connectorOuterDiameter}'^^xsd:double ." + "\n" +
                            $"inst:{connectorOuterDiameterID} fpo:unit 'meter'^^xsd:string  ." + "\n");

                    }
                    //Port relationship to other ports
                    ConnectorSet joinedconnectors = connector.AllRefs;
                    if (connectorDirection == "Out")
                    {
                        foreach (Connector connectedConnector in joinedconnectors)
                        {
                            connectedConnectorID = connectedConnector.Owner.UniqueId.ToString() + "-" + connectedConnector.Id.ToString();

                            if (Domain.DomainHvac == connectedConnector.Domain && connector.DuctSystemType == DuctSystemType.SupplyAir
                            || (Domain.DomainPiping == connectedConnector.Domain && connector.PipeSystemType == PipeSystemType.SupplyHydronic))
                            {
                                connectedConnectorDirection = connectedConnector.Direction.ToString();

                                sb.Append($"inst:{connectorID} fso:suppliesFluidTo inst:{connectedConnectorID} ." + "\n"
                                    + $"inst:{connectedConnectorID} a fso:Port ." + "\n");

                            }
                            if (Domain.DomainHvac == connectedConnector.Domain && connector.DuctSystemType == DuctSystemType.ReturnAir
                            || Domain.DomainPiping == connectedConnector.Domain && connector.PipeSystemType == PipeSystemType.ReturnHydronic)
                            {
                                connectedConnectorDirection = connectedConnector.Direction.ToString();

                                sb.Append($"inst:{connectorID} fso:returnsFluidTo inst:{connectedConnectorID} ." + "\n"
                                    + $"inst:{connectedConnectorID} a fso:Port ." + "\n");

                            }



                        }
                    }
                }
            }

            return sb;
        }





        public static StringBuilder DuctConnectors(Duct component, string componentID, StringBuilder sb)
        {
            //Port type
            ConnectorSet connectorSet = component.ConnectorManager.Connectors;

            string connectorID = "";
            string connectorDirectionID = "";
            string connectorDirection = "";
            double connectorOuterDiameter = 0;
            string connectedConnectorID = "";
            string connectedConnectorDirection = "";
            string connectorOuterDiameterID = "";

            foreach (Connector connector in connectorSet)
            {
                //Type
                connectorID = componentID + "-" + connector.Id;
                sb.Append($"inst:{componentID} fso:hasPort inst:{connectorID} ." + "\n"
                    + $"inst:{connectorID} a fso:Port ." + "\n");
                if (Domain.DomainHvac == connector.Domain || Domain.DomainPiping == connector.Domain)
                {
                    //FlowDirection
                    connectorDirectionID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                    connectorDirection = connector.Direction.ToString();

                    sb.Append($"inst:{connectorID} fpo:flowDirection inst:{connectorDirectionID} ." + "\n"
                                            + $"inst:{connectorDirectionID} a fpo:FlowDirection ." + "\n"
                                            + $"inst:{connectorDirectionID} fpo:value '{connectorDirection}'^^xsd:string ." + "\n");

                    //Diameter
                    if (connector.Shape.ToString() == "Round")
                    {
                        connectorOuterDiameterID = System.Guid.NewGuid().ToString().Replace(' ', '-');
                        connectorOuterDiameter = UnitUtils.ConvertFromInternalUnits(connector.Radius * 2, UnitTypeId.Meters);
                        sb.Append($"inst:{connectorID} fpo:outerDiameter inst:{connectorOuterDiameterID} ." + "\n" +
                            $"inst:{connectorOuterDiameterID} fpo:value '{connectorOuterDiameter}'^^xsd:double ." + "\n" +
                            $"inst:{connectorOuterDiameterID} fpo:unit 'meter'^^xsd:string  ." + "\n");

                    }
                    //Port relationship to other ports
                    ConnectorSet joinedconnectors = connector.AllRefs;
                    if (connectorDirection == "Out")
                    {
                        foreach (Connector connectedConnector in joinedconnectors)
                        {
                            connectedConnectorID = connectedConnector.Owner.UniqueId.ToString() + "-" + connectedConnector.Id.ToString();

                            if (Domain.DomainHvac == connectedConnector.Domain && connector.DuctSystemType == DuctSystemType.SupplyAir
                            || (Domain.DomainPiping == connectedConnector.Domain && connector.PipeSystemType == PipeSystemType.SupplyHydronic))
                            {
                                connectedConnectorDirection = connectedConnector.Direction.ToString();

                                sb.Append($"inst:{connectorID} fso:suppliesFluidTo inst:{connectedConnectorID} ." + "\n"
                                    + $"inst:{connectedConnectorID} a fso:Port ." + "\n");

                            }
                            if (Domain.DomainHvac == connectedConnector.Domain && connector.DuctSystemType == DuctSystemType.ReturnAir
                            || Domain.DomainPiping == connectedConnector.Domain && connector.PipeSystemType == PipeSystemType.ReturnHydronic)
                            {
                                connectedConnectorDirection = connectedConnector.Direction.ToString();

                                sb.Append($"inst:{connectorID} fso:returnsFluidTo inst:{connectedConnectorID} ." + "\n"
                                    + $"inst:{connectedConnectorID} a fso:Port ." + "\n");

                            }



                        }
                    }
                }
            }

            return sb;
        }



    }
}
