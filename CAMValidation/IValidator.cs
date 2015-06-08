using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AllanStevens.CAMValidation
{
    interface IValidator
    {
        /// <summary>
        /// Filename of the CAM file that will be used for validation
        /// </summary>
        string CAMFilename { set; }
        
        /// <summary>
        /// Filename of the XML that will be validated, this is optional if a string has been passed
        /// </summary>
        string ValidateXMLFilename { set; }
        
        /// <summary>
        /// String of the XML that will be validated, this is optional if a xml filename has been passed
        /// </summary>
        string ValidateXMLAsString { set; }
                
        /// <summary>
        /// If true exceptions will be raised in initialisation phase, false will handle the errors in the message collection.
        /// </summary>
        bool RaiseExceptionOnError { get; set; }
        
        /// <summary>
        /// Stores the messages that will be captured during validation,  this also contains counts on each message count.
        /// </summary>       
        MessageCollection Messages { get; }

        /// <summary>
        /// Validates the xml against the cam file.  The message collection will be populated with all the error/pass/fail information
        /// </summary>
        /// <returns>Return true if validation passed</returns>
        bool Validate();
    }
}
