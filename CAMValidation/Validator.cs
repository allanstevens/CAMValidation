using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;

namespace AllanStevens.CAMValidation
{
    public class Validator : IValidator, IDisposable
    {
        // Private Variables

        private string camFilename;
        private string validateXmlFilename;
        private string validateXmlAsString;
        private bool raiseExceptionOnError;

        private MessageCollection validationMessages;
        private Constraints constraint;

        // Protected Variables
        
        protected XmlDocument xmlCam;
        protected XmlNamespaceManager nsmgr;
        
        //protected XmlDocument xmlToValidate;
        //protected XmlDocument xmlMandatoryFieldMap; 

        // Constructor(s)

        public Validator()
        {
            validationMessages = new MessageCollection();
            raiseExceptionOnError = false;
            xmlCam = new XmlDocument();
            
            //xmlToValidate = new XmlDocument();
           // xmlMandatoryFieldMap = new XmlDocument();
        }

        // Class Properties

        public string CAMFilename { set { camFilename = value; } }
        public string ValidateXMLFilename { set { validateXmlFilename = value; } }
        public string ValidateXMLAsString { set { validateXmlAsString = value; } }
        public bool RaiseExceptionOnError { set { raiseExceptionOnError = value; } get { return raiseExceptionOnError; } }
        public MessageCollection Messages { get { return validationMessages; } }
 

        // Public Methods

        private void Initialize()
        {
            try
            {
                // Do some validation
                if (!File.Exists(camFilename))
                {
                    throw new Exception("CAM file missing.");
                }

                if (!File.Exists(validateXmlFilename) && (validateXmlAsString.Equals(null)))
                {
                    throw new Exception("XML file to validate is missing, or string xml to validate is empty");
                }

                //load cam xml
                xmlCam.Load(camFilename);
                validationMessages.Add(new Message("CAM file loaded successfully", "Loaded file '" + camFilename + "'", MessageTypes.Information));
                                
                //load namespaces from cam
                nsmgr = new XmlNamespaceManager(xmlCam.NameTable);
                foreach (XmlAttribute attr in xmlCam.DocumentElement.Attributes)
                {
                    if (attr.Name.StartsWith("xmlns:") && 
                        (attr.Name != "xmlns:xml" && attr.Name != "xmlns:xsd" ))
                    {
                        nsmgr.AddNamespace(attr.LocalName, attr.Value);
                    }
                }
                validationMessages.Add(new Message("CAM namespaces loaded successfully", "", MessageTypes.Information));

                //load xml structure, this is to create a maditory FieldMap
                XmlDocument xmlMandatoryFieldMap = new XmlDocument();
                xmlMandatoryFieldMap.LoadXml(xmlCam.SelectSingleNode("/as:CAM/as:AssemblyStructure/as:Structure", nsmgr).InnerXml);
                foreach(XmlNode xmlNode in xmlMandatoryFieldMap.SelectNodes("//*"))
                {
                    if (xmlNode.InnerXml.StartsWith("%") && xmlNode.InnerXml.EndsWith("%")) xmlNode.InnerXml = "%mandatory%";
                }
                validationMessages.Add(new Message("XML field map structure loaded successfully", "", MessageTypes.Information));
                                
                //load xmltoValidate xml
                XmlDocument xmlToValidate = new XmlDocument();
                if (validateXmlAsString != null) { xmlToValidate.LoadXml(validateXmlAsString); }
                else { xmlToValidate.Load(validateXmlFilename); }
                validationMessages.Add(new Message("XML to validate loaded successfully", "Loaded file '" + validateXmlFilename + "'", MessageTypes.Information));

                //setup constraint object, this holds all the constant action logic
                constraint = new Constraints(xmlToValidate, xmlMandatoryFieldMap);
                validationMessages.Add(new Message("Constraint engine setup successfully", "", MessageTypes.Information));

            }
            catch (Exception ex)
            {
                validationMessages.Add(new Message("Exception during initialize", ex.Message, MessageTypes.Error));
                if (RaiseExceptionOnError) throw (ex);
            }
        }

        public bool Validate()
        {
            // Initialize if not done already
            if (xmlCam.InnerXml == string.Empty)
            {
                Initialize();
            }

            try
            {
                //add namespaces to xmlToValidate
                foreach (XmlAttribute attr in xmlCam.DocumentElement.Attributes)
                {
                    if (attr.Name.StartsWith("xmlns:") &&
                        (attr.Name != "xmlns:xml" && attr.Name != "xmlns:xsd"))
                    {
                        constraint.AddNamespace(attr.LocalName, attr.Value);
                    }
                }

                // Step though all make optional constraints
                foreach (XmlNode nodeConstraint in xmlCam.SelectNodes("/as:CAM/as:BusinessUseContext/as:Rules/as:default/as:context/as:constraint[starts-with(@action,'makeOptional')]", nsmgr))
                {
                    ProcessConstraintNode(nodeConstraint, constraint);
                }


                // Step though all make nillable constraints
                foreach (XmlNode nodeConstraint in xmlCam.SelectNodes("/as:CAM/as:BusinessUseContext/as:Rules/as:default/as:context/as:constraint[starts-with(@action,'makeNillable')]", nsmgr))
                {
                    ProcessConstraintNode(nodeConstraint, constraint);
                }
                
                // Step though all make Mandatory constraints
                foreach (XmlNode nodeConstraint in xmlCam.SelectNodes("/as:CAM/as:BusinessUseContext/as:Rules/as:default/as:context/as:constraint[starts-with(@action,'makeMandatory')]", nsmgr))
                {
                    ProcessConstraintNode(nodeConstraint, constraint);
                }

                // Check on Mandatory values/ if any
                foreach (Message message in constraint.ValidateMandatoryFields())
                {
                    // log each message back
                    validationMessages.Add(message);
                }


                // Step though other constraints
                foreach (XmlNode nodeConstraint in xmlCam.SelectNodes("/as:CAM/as:BusinessUseContext/as:Rules/as:default/as:context/as:constraint[starts-with(@action,'makeOptional') = false and starts-with(@action,'makeMandatory') = false and starts-with(@action,'makeNillable') = false]", nsmgr))
                {
                    ProcessConstraintNode(nodeConstraint, constraint);
                }

                //// Step though other constraints
                //foreach (XmlNode nodeConstraint in xmlCam.SelectNodes("/as:CAM/as:BusinessUseContext/as:Rules/as:default/as:context/as:constraint", nsmgr))
                //{
                //    ProcessConstraintNode(nodeConstraint, constraint);
                //}
            }
            catch (Exception ex)
            {
                validationMessages.Add(new Message("Exception during validation", ex.Message, MessageTypes.Error));
                if (RaiseExceptionOnError) throw (ex);
            }

            return validationMessages.FailedValidationCount == 0 && validationMessages.PassedValidationCount > 0 && validationMessages.ErrorCount == 0;
        }

        private void ProcessConstraintNode(XmlNode node, Constraints constraint)
        {
            // Decalire values
            string actionCommand;
            string[] actionArgs;
            string condition;

            // Set action and args
            actionCommand = node.Attributes["action"].Value;
            actionArgs = actionCommand.Substring(
                actionCommand.IndexOf(char.Parse("(")) + 1,
                actionCommand.LastIndexOf(char.Parse(")")) - (actionCommand.IndexOf(char.Parse("(")) + 1)
                ).Split(char.Parse(","));
            actionCommand = actionCommand.Substring(0, actionCommand.IndexOf(char.Parse("(")));

            // Set condition
            if (node.Attributes["condition"] != null)
            {
                condition = node.Attributes["condition"].Value;
            }
            else
            {
                condition = "";
            }

            // Run action/constraint and report back
            validationMessages.Add(constraint.Execute(actionCommand, actionArgs, condition));
        }
               
        public void Dispose()
        {
            if (constraint != null) constraint.Dispose();
            //xmlToValidate = null;
        }
    }
}
