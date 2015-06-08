using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Globalization;

namespace AllanStevens.CAMValidation
{
    class Constraints : IDisposable
    {
        // Private Variables
        
        XmlDocument _xmlToValidate;
        XmlNamespaceManager _xmlToValidateNs;
        XmlDocument _xmlMandatoryFieldMap;
        XmlNamespaceManager _xmlMandatoryFieldMapNs;
        
        // Constructors

        public Constraints(XmlDocument xmlToValidate, XmlDocument xmlMandatoryFieldMap)
        {
            _xmlToValidate = xmlToValidate;
            _xmlToValidateNs = new XmlNamespaceManager(_xmlToValidate.NameTable);
            _xmlMandatoryFieldMap = xmlMandatoryFieldMap;
            _xmlMandatoryFieldMapNs = new XmlNamespaceManager(_xmlMandatoryFieldMap.NameTable);
        }

        // Public Methods

        public void AddNamespace(string prefix, string uri)
        {
            _xmlToValidateNs.AddNamespace(prefix, uri);
            _xmlMandatoryFieldMapNs.AddNamespace(prefix, uri);
        }

        public MessageCollection ValidateMandatoryFields()
        {
            MessageCollection returnMessageCollection = new MessageCollection();

            foreach (XmlNode xmlNode in _xmlMandatoryFieldMap.SelectNodes("//*"))
            {
                // Build XPath Query
                string xPathQuery = "";
                XmlNode xmlParentNode = xmlNode;
                while (xmlParentNode.NodeType == XmlNodeType.Element)
                {
                    xPathQuery = "/" + xmlParentNode.Name + xPathQuery;
                    xmlParentNode = xmlParentNode.ParentNode;
                }

                if (xmlNode.InnerXml.Contains("%mandatory%") 
                    && xmlNode.InnerXml.Contains("%nillable%"))
                {
                    if (_xmlToValidate.SelectSingleNode(xPathQuery, _xmlToValidateNs) == null)
                    {
                        returnMessageCollection.Add(new Message(
                            "Failed element validation.",
                            "Element " + xPathQuery,
                            "Failed validation, element is mandatory and nillable. Element location " + xPathQuery + ".",
                            MessageTypes.FailedValidation));
                    }
                    else
                    {
                        returnMessageCollection.Add(new Message(
                            "Passed element validation.",
                            "Mandatory nillable element " + xPathQuery,
                            MessageTypes.PassedValidation));
                    }

                }

                else if (xmlNode.InnerXml.Contains("%mandatory%"))
                {
                    if (_xmlToValidate.SelectSingleNode(xPathQuery, _xmlToValidateNs) == null
                        || _xmlToValidate.SelectSingleNode(xPathQuery, _xmlToValidateNs).InnerXml.Length == 0)
                    {
                        returnMessageCollection.Add(new Message(
                            "Failed element validation.",
                            "Element " + xPathQuery,
                            "Failed validation, element is mandatory. Element location " + xPathQuery + ".",
                            MessageTypes.FailedValidation));
                    }
                    else
                    {
                        returnMessageCollection.Add(new Message(
                            "Passed element validation.",
                            "Manditory element " + xPathQuery,
                            MessageTypes.PassedValidation));
                    }

                }

                else if (xmlNode.InnerXml.Contains("%optional%") && !xmlNode.InnerXml.Contains("%nillable%"))
                {
                    // check field is not valid
                    if (_xmlToValidate.SelectSingleNode(xPathQuery, _xmlToValidateNs) != null
                        && _xmlToValidate.SelectSingleNode(xPathQuery, _xmlToValidateNs).InnerXml.Length == 0)
                    {
                        returnMessageCollection.Add(new Message(
                            "Failed element validation.",
                            "Element " + xPathQuery,
                            "Failed validation, element is optional. Element location " + xPathQuery + ".",
                            MessageTypes.FailedValidation));
                    }
                    else
                    {
                        returnMessageCollection.Add(new Message(
                            "Passed element validation.",
                            "Element " + xPathQuery,
                            MessageTypes.PassedValidation));
                    }
                }

                else
                {
                    returnMessageCollection.Add(new Message(
                        "Passed element validation.",
                        "Element " + xPathQuery,
                        MessageTypes.PassedValidation));
                }
            }

            return returnMessageCollection;
        }
        
        public Message Execute(string actionCommand, string[] actionArgs, string condition )
        {
           
            switch (actionCommand)
            {
                case "makeOptional":
                    return MakeOptionalConstraint(actionArgs, condition);

                case "makeMandatory":
                    return MakeMandatoryConstraint(actionArgs, condition);
                    
                case "makeNillable":
                    return MakeNillableConstraint(actionArgs, condition);
                    
                case "setLength":
                    return SetLengthConstraint(actionArgs, condition);

                case "setNumberRange":
                    return SetNumberRange(actionArgs, condition);

                case "restrictValues":
                    return RestrictValuesConstraint(actionArgs, condition);

                case "setNumberMask":
                    return SetNumberMaskConstraint();

                case "setDateMask":
                    return SetDateMaskConstraint(actionArgs, condition);

                default:
                    return new Message("Unrecognised constraint", "Constraint " + actionCommand + " is not recognised", MessageTypes.Warning);
            }
        }

        public void Dispose()
        {
            _xmlToValidate = null;
        }
        
        // Private Methods   

        private Message MakeOptionalConstraint(string[] args, string condition)
        {
            try
            {
                bool passedConditionLogic = ConditionLogic(condition);

                // Update xml Mandatory field map to Mandatory/optional value
                if (passedConditionLogic)
                {
                    //Set value
                    _xmlMandatoryFieldMap.SelectSingleNode(args[0], _xmlMandatoryFieldMapNs).InnerXml = _xmlMandatoryFieldMap.SelectSingleNode(args[0], _xmlMandatoryFieldMapNs).InnerXml
                        .Replace("%mandatory%", "%optional%");
//                        += "%optional%";
                        

                    return new Message(
                        "Constraint has updated xml field map.",
                        "makeOptional(" + StringMethods.ArrayToString(args) + ") condition(" + condition + ")",
                        MessageTypes.Information);
                }
                else
                {
                    return new Message(
                            "Constraint did not update xml field map, condition not met.",
                            "makeOptional(" + StringMethods.ArrayToString(args) + ") condition=" + condition + ")",
                            MessageTypes.Information);
                }
            }
            catch (Exception ex)
            {
                return new Message(
                                "Constraint failed to function.",
                                "makeOptional(" + StringMethods.ArrayToString(args) + ") condition(" + condition + ")\nError Message: " + ex.Message,
                                MessageTypes.Error);
            }
        }

        private Message MakeNillableConstraint(string[] args, string condition)
        {
            try
            {
                bool passedConditionLogic = ConditionLogic(condition);

                // Update xml Mandatory field map to Mandatory/optional value
                if (passedConditionLogic)
                {
                    //Set value
                    _xmlMandatoryFieldMap.SelectSingleNode(args[0], _xmlMandatoryFieldMapNs).InnerXml = _xmlMandatoryFieldMap.SelectSingleNode(args[0], _xmlMandatoryFieldMapNs).InnerXml
                        += "%nillable%";

                    return new Message(
                        "Constraint has updated xml field map.",
                        "makeNillable(" + StringMethods.ArrayToString(args) + ") condition(" + condition + ")",
                        MessageTypes.Information);
                }
                else
                {
                    return new Message(
                            "Constraint did not update xml field map, condition not met.",
                            "makeNillable(" + StringMethods.ArrayToString(args) + ") condition=" + condition + ")",
                            MessageTypes.Information);
                }
            }
            catch (Exception ex)
            {
                return new Message(
                                "Constraint failed to function.",
                                "makeNillable(" + StringMethods.ArrayToString(args) + ") condition(" + condition + ")\nError Message: " + ex.Message,
                                MessageTypes.Error);
            }
        }

        private Message MakeMandatoryConstraint(string[] args, string condition)
        {
            try
            {
                bool passedConditionLogic = ConditionLogic(condition);
 
                // Update xml Mandatory field map to Mandatory/optional value
                if (passedConditionLogic)
                {
                    _xmlMandatoryFieldMap.SelectSingleNode(args[0], _xmlMandatoryFieldMapNs).InnerXml =
                        _xmlMandatoryFieldMap.SelectSingleNode(args[0], _xmlMandatoryFieldMapNs).InnerXml
                        .Replace("%optional%","%mandatory%");
                    
                    return new Message(
                        "Constraint has updated xml field map.",
                        "makeMandatory(" + StringMethods.ArrayToString(args) + ")  condition(" + condition + ")",
                        MessageTypes.Information);
                }
                else
                {
                    return new Message(
                            "Constraint did not update xml field map, condition not met.",
                            "makeMandatory(" + StringMethods.ArrayToString(args) + ")  condition(" + condition + ")",
                            MessageTypes.Information);
                }
            }
            catch (Exception ex)
            {
                return new Message(
                                "Constraint failed to function",
                                "makeMandatory(" + StringMethods.ArrayToString(args) + ")  condition(" + condition + ")\nError Message: " + ex.Message,
                                MessageTypes.Error);
            }
        }

        private Message SetLengthConstraint(string[] args, string condition)
        {
            try
            {
                bool passedConditionLogic = ConditionLogic(condition);

                if (!passedConditionLogic)
                {
                    return new Message(
                           "Constraint did not run, condition not met.",
                           "setLength(" + StringMethods.ArrayToString(args) + ") condition(" + condition + ")",
                           MessageTypes.Information);
                }
                else
                {
                    // If value is missing/zerolength skip validation for optional/nillable elements
                    if (ConstraintSkipLogic(args[0]))
                    {
                        return new Message(
                           "Constraint did not run, nillable or optional element.",
                           "setLength(" + StringMethods.ArrayToString(args) + ") condition(" + condition + ")",
                           MessageTypes.Information);
                    }

                    int minValue, maxValue, valueLength;

                    //if argument contains - then split and set min - max
                    if (args[1].Contains("-"))
                    {
                        minValue = int.Parse(args[1].Split(char.Parse("-"))[0]);
                        maxValue = int.Parse(args[1].Split(char.Parse("-"))[1]);
                    }
                    // else set min value and max the same
                    else
                    {
                        minValue = int.Parse(args[1]);
                        maxValue = int.Parse(args[1]);
                    }


                    // Set lengh of value
                    if (_xmlToValidate.SelectSingleNode(args[0], _xmlToValidateNs) == null)
                    {
                        valueLength = -1;
                    }
                    else
                    {
                        valueLength = _xmlToValidate.SelectSingleNode(args[0], _xmlToValidateNs).InnerText.Length;
                    }

                    // If passes validation
                    if (valueLength >= minValue && valueLength <= maxValue)
                    {
                        return new Message(
                            "Passed validation constraint.",
                            "setLength(" + StringMethods.ArrayToString(args) + ") condition(" + condition + ")",
                            MessageTypes.PassedValidation);
                    }
                    // If fails validation
                    else
                    {
                        return new Message(
                             "Failed validation constraint.",
                             "Failed validation on set length " + args[1] + " constraint.  Element location " + args[0] + ".",
                             "setLength(" + StringMethods.ArrayToString(args) + ") condition(" + condition + ")",
                             MessageTypes.FailedValidation);
                    }
                }
            }
            catch (Exception ex)
            {
                return new Message(
                                "Constraint failed to function.",
                                "setLength(" + StringMethods.ArrayToString(args) + ") condition(" + condition + ")\nError Message: " + ex.Message,
                                MessageTypes.Error);
            }
        }

        private Message SetNumberRange(string[] args, string condition)
        {
            try
            {
                bool passedConditionLogic = ConditionLogic(condition);

                if (!passedConditionLogic)
                {
                    return new Message(
                           "Constraint did not run, condition not met.",
                           "setNumberRange(" + StringMethods.ArrayToString(args) + ") condition(" + condition + ")",
                           MessageTypes.Information);
                }
                else
                {
                    // If value is missing/zerolength skip validation for optional/nillable elements
                    if (ConstraintSkipLogic(args[0]))
                    {
                        return new Message(
                           "Constraint did not run, nillable or optional element.",
                           "setNumberRange(" + StringMethods.ArrayToString(args) + ") condition(" + condition + ")",
                           MessageTypes.Information);
                    }

                    decimal minValue, maxValue, valueNumber;

                    minValue = ParseNumberRange(args[1].Split(char.Parse("-"))[0]);
                    maxValue = ParseNumberRange(args[1].Split(char.Parse("-"))[1]);
                    
                    // Set lengh of value
                    if (_xmlToValidate.SelectSingleNode(args[0], _xmlToValidateNs) == null)
                    {
                        valueNumber = decimal.MinValue;
                    }
                    else
                    {
                        //try and parse
                        if (!decimal.TryParse(_xmlToValidate.SelectSingleNode(args[0], _xmlToValidateNs).InnerText, out valueNumber))
                        {
                            DateTime valueDateTime;
                            if (DateTime.TryParse(_xmlToValidate.SelectSingleNode(args[0], _xmlToValidateNs).InnerText, out valueDateTime))
                            {
                                valueNumber = valueDateTime.Ticks;
                            }
                            else
                            {
                                valueNumber = decimal.MinValue;
                            }
                        }
                    }

                    // If passes validation
                    if (valueNumber >= minValue && valueNumber <= maxValue)
                    {
                        return new Message(
                            "Passed validation constraint.",
                            "setNumberRange(" + StringMethods.ArrayToString(args) + ") condition(" + condition + ")",
                            MessageTypes.PassedValidation);
                    }
                    // If fails validation
                    else
                    {
                        return new Message(
                            "Failed validation constraint.",
                            "setNumberRange(" + StringMethods.ArrayToString(args) + ") condition(" + condition + ")",
                            "Failed validation on number range " + args[1] + " constraint.  Element location " + args[0] + ".",
                            MessageTypes.FailedValidation);
                    }
                }

            }
            catch (Exception ex)
            {
                return new Message(
                                "Constraint failed to function.",
                                "setNumberRange(" + StringMethods.ArrayToString(args) + ") condition(" + condition + ")\nError Message: " + ex.Message,
                                MessageTypes.Error);
            }
        }

        private Message RestrictValuesConstraint(string[] args, string condition)
        {
            try
            {
                bool passedConditionLogic = ConditionLogic(condition);

                if (!passedConditionLogic)
                {
                    return new Message(
                           "Constraint did not run, condition not met.",
                           "restrictValues(" + StringMethods.ArrayToString(args) + ") condition(" + condition + ")",
                           MessageTypes.Information);
                }
                else
                {
                    // If value is missing/zerolength skip validation for optional/nillable elements
                    if (ConstraintSkipLogic(args[0]))
                    {
                        return new Message(
                           "Constraint did not run, nillable or optional element.",
                           "restrictValues(" + StringMethods.ArrayToString(args) + ") condition(" + condition + ")",
                           MessageTypes.Information);
                    }

                    //check to see if value exists
                    string[] restrictedValues = args[1].Replace("'", "").Split(char.Parse("|"));
                    bool valueExists = false;
                    string valueToCheck = null;

                    if (_xmlToValidate.SelectSingleNode(args[0], _xmlToValidateNs) != null)
                    {
                        valueToCheck = _xmlToValidate.SelectSingleNode(args[0], _xmlToValidateNs).InnerText.Replace("'","").Trim();
                    }

                    foreach (string restrictedValue in restrictedValues)
                    {
                        if (restrictedValue.Trim() == valueToCheck) valueExists = true;
                    }

                    // If passes validation PASS response
                    if (valueExists)
                    {
                        return new Message(
                            "Passed validation constraint.",
                            "restrictValues(" + StringMethods.ArrayToString(args) + ") condition(" + condition + ")",
                            MessageTypes.PassedValidation);
                    }

                    // If fails FAIL response
                    else
                    {
                        return new Message(
                           "Failed validation constraint.",
                           "restrictValues(" + StringMethods.ArrayToString(args) + " condition(" + condition + ")",
                           "Failed validation on restricted values " + args[1] + " constraint.  Element location " + args[0] + ".",
                           MessageTypes.FailedValidation);
                    }
                }
            }
            catch (Exception ex)
            {
                return new Message(
                                "Constraint failed to function.",
                                "restrictValues(" + StringMethods.ArrayToString(args) + ") condition(" + condition + ")\nError Message: " + ex.Message,
                                MessageTypes.Error);
            }
        }

        private Message SetDateMaskConstraint(string[] args, string condition)
        {
            //return new Message(
            //    "Constraint has not been implemented",
            //    "setDateMask()",
            //    MessageTypes.Warning);
            
            try
            {
                bool passedConditionLogic = ConditionLogic(condition);

                if (!passedConditionLogic)
                {
                    return new Message(
                           "Constraint did not run, condition not met.",
                           "setDateMask(" + StringMethods.ArrayToString(args) + ") condition(" + condition + ")",
                           MessageTypes.Information);
                }
                else
                {
                                        
                    // If value is missing/zerolength skip validation for optional/nillable elements
                    if (ConstraintSkipLogic(args[0]))
                    {
                        return new Message(
                           "Constraint did not run, nillable or optional element.",
                           "setDateMask(" + StringMethods.ArrayToString(args) + ") condition(" + condition + ")",
                           MessageTypes.Information);
                    }

                    //setDateMask(/tns:SubmitValuationReportRequest/tns:FormDate,YYYY-MM-DD)
                    
                    //create equalivant datetime mask for .net from CAM

                    string dateTimeMask = args[1]
                        .Replace("DDDD", "dddd")
                        .Replace("DDD", "ddd")
                        .Replace("DD", "dd")
                        .Replace("YYYY", "yyyy")
                        .Replace("YY", "yy")
                        .Replace("MI", "mm")
                        .Replace("SSZ", "ss");


                    //YYYY-MM-DD'T'HH:MI:SSZ

                    //get the value
                    string valueToCheck = "";
                    if (_xmlToValidate.SelectSingleNode(args[0], _xmlToValidateNs) != null)
                    {
                        valueToCheck = _xmlToValidate.SelectSingleNode(args[0], _xmlToValidateNs).InnerText.Replace("'", "").Trim();
                    }

                    DateTime dtOutValue;
                    //try and parse it, if successfull the pass validation
                    if (DateTime.TryParseExact(valueToCheck, dateTimeMask, new CultureInfo("en-GB"), DateTimeStyles.None, out dtOutValue))
                    {
                        return new Message(
                            "Passed validation constraint.",
                            "setDateMask(" + StringMethods.ArrayToString(args) + ") condition(" + condition + ")",
                            MessageTypes.PassedValidation);
                    }

                    // If fails
                    else
                    {
                        return new Message(
                           "Failed validation constraint.",
                           "setDateMask(" + StringMethods.ArrayToString(args) + " condition(" + condition + ")",
                           "Failed validation on date mask " + args[1] + " constraint.  Element location " + args[0] + ".",
                           MessageTypes.FailedValidation);
                    }
                }
            }
            catch (Exception ex)
            {
                return new Message(
                                "Constraint failed to function.",
                                "setDateMask(" + StringMethods.ArrayToString(args) + ") condition(" + condition + ")\nError Message: " + ex.Message,
                                MessageTypes.Error);
            }
        }

        // Unimplemented constraints

        private Message SetNumberMaskConstraint()
        {
            return new Message(
                "Constraint has not been implemented",
                "setNumberMask()",
                MessageTypes.Warning);
        }

        // Sub menthods for constaint methods

        private bool ConditionLogic(string condition)
        {
            // Split conditions into array to step though  
            string[] conditions = StringMethods.RemovedWhiteSpace(condition).Split(char.Parse(" "));

            // Return condition booleon
            bool conditionReturn = true;

            // Step thought the arguments (i=i+4 so just pickup xpath) (if condition exist)
            if (conditions[0] != "")
                for (int i = 0; i < conditions.Length; i = i + 4)
                {
                    // Check if 'exists' is involved, if so check for null values and step forward arg numbering
                    if (conditions[i].StartsWith("exists("))
                    {
                       if (_xmlToValidate.SelectSingleNode(conditions[i].Remove(0, 7).Replace(")",""), _xmlToValidateNs) == null) return false;
                       i = i - 2;
                    }
                    else if (conditions[i].StartsWith("not(exists("))
                    {
                       
                       if (_xmlToValidate.SelectSingleNode(conditions[i].Remove(0, 11).Replace(")", ""), _xmlToValidateNs) != null) return false;
                       i  = i - 2;                        
                    }
                    
                    // Check operator logic is valid 
                    else if (conditions[i + 1] != "=")
                    {
                        throw (new Exception("Operator not implemented in condition."));
                    }

                    // if current condition starts with NOT( then reverse operator
                    else if (conditions[i].StartsWith("not("))
                    {
                        if (_xmlToValidate.SelectSingleNode(conditions[i].Remove(0, 5), _xmlToValidateNs) != null &&
                           _xmlToValidate.SelectSingleNode(conditions[i].Remove(0, 5), _xmlToValidateNs).InnerText == conditions[i + 2].Replace("'", "").Replace(")", ""))
                        { return false; }
                    }
                    // if current condition != value then return false
                    else
                    {
                        if (_xmlToValidate.SelectSingleNode(conditions[i], _xmlToValidateNs) == null)
                        { return false; }

                        if (_xmlToValidate.SelectSingleNode(conditions[i], _xmlToValidateNs).InnerText != conditions[i + 2].Replace("'", ""))
                        { return false; }
                    }

                    // Check operator logic is valid 
                    if ((i + 3) > conditions.Length && conditions[i + 3].ToLower() != "and")
                    {
                        throw (new Exception("Logic not implemented in condition."));
                    }
                }

            return conditionReturn;
        }

        private bool ConstraintSkipLogic(string xpath)
        {
            if (
                _xmlToValidate.SelectSingleNode(xpath, _xmlToValidateNs) == null
                && _xmlMandatoryFieldMap.SelectSingleNode(xpath, _xmlMandatoryFieldMapNs).InnerText.Contains("%optional%"))
            {
                return true;
            }

            if (
                (_xmlToValidate.SelectSingleNode(xpath, _xmlToValidateNs) != null &&
                _xmlToValidate.SelectSingleNode(xpath, _xmlToValidateNs).InnerText.Length.Equals(0))
                && _xmlMandatoryFieldMap.SelectSingleNode(xpath, _xmlMandatoryFieldMapNs).InnerText.Contains("%nillable%"))
            {
                return true;
            }

            // value is present or element is manditory
            return false;
        }

        private decimal ParseNumberRange(string number)
        {
            decimal returnValue;

            switch (number.Trim().ToLower().Split(char.Parse("("))[0])
            {
                case "currentyear":
                    returnValue = DateTime.Now.Year;
                    break;

                case "currentyear.addyear":
                    int Years = 0;
                    int.TryParse(number.Trim().ToLower().Split(char.Parse("("))[1].Replace(")", ""), out Years);
                    returnValue = DateTime.Now.AddYears(Years).Year;
                    break;

                case "currentdate":
                    returnValue = DateTime.Now.Ticks;
                    break;

                case "currentdate.removemonth":
                    int months = 0;
                    int.TryParse(number.Trim().ToLower().Split(char.Parse("("))[1].Replace(")",""), out months);
                    returnValue = DateTime.Now.AddMonths(months - (months * 2)).Ticks;
                    break;

                default:
                    if (!decimal.TryParse(number, out returnValue))
                    {
                        returnValue = -1;
                    }
                    break;
            }

            return returnValue;
        }

    }
}
