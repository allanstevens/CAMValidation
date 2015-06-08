using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using AllanStevens.CAMValidation;

namespace TestHarness
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Validator validateXml = new Validator())
            {
                validateXml.CAMFilename = "test1.cam";
                validateXml.ValidateXMLFilename = "test1.xml";
                validateXml.RaiseExceptionOnError = false;

                Console.WriteLine("Response from Validate() method: {0}", validateXml.Validate());
                Console.WriteLine("Summary: {0} information messages", validateXml.Messages.InformationCount);
                Console.WriteLine("         {0} warning messages", validateXml.Messages.WarningCount);
                Console.WriteLine("         {0} error messages", validateXml.Messages.ErrorCount);
                Console.WriteLine("         {0} passed validation messages", validateXml.Messages.PassedValidationCount);
                Console.WriteLine("         {0} failed validation messages", validateXml.Messages.FailedValidationCount);
                
                if (validateXml.Messages.ErrorCount + validateXml.Messages.FailedValidationCount != 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("[ENTER] to see error and failed validation messages");
                    Console.ReadLine();
                    foreach (Message message in validateXml.Messages)
                    {
                        if (message.Type == MessageTypes.FailedValidation || message.Type == MessageTypes.Error)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Type: {0}", message.Type);
                            Console.WriteLine("Name: {0}", message.Name);
                            //Console.WriteLine("Friendly Message: {0}", message.FriendlyMessage);
                            Console.WriteLine("Details: {0}", message.Details);
                            Console.WriteLine();
                            Console.ResetColor();
                        }
                    }
                }

                Console.WriteLine("[ENTER] to see all messages");
                Console.ReadLine();

                foreach (Message message in validateXml.Messages)
                {
                    if (message.Type == MessageTypes.Warning) Console.ForegroundColor = ConsoleColor.Yellow;
                    if (message.Type == MessageTypes.Error || message.Type == MessageTypes.FailedValidation) Console.ForegroundColor = ConsoleColor.Red;
                    if (message.Type == MessageTypes.PassedValidation) Console.ForegroundColor = ConsoleColor.Green;
                    Output();
                    Output("Type: {0}", message.Type);
                    Output("Name: {0}", message.Name);
                    //Output("Friendly Message: {0}", message.FriendlyMessage);
                    Output("Details: {0}", message.Details);
                    Console.ResetColor();
                }

            }

            Output();
            Output("End of test");
            Console.ReadLine();
        }

        static void Output()
        {
            Console.WriteLine();
            Debug.WriteLine("");
        }
        static void Output(string s)
        {
            Console.WriteLine(s);
            Debug.WriteLine(s);
        }

        static void Output(string s, object arg0)
        {
            Console.WriteLine(s, arg0);

            if (arg0 == null)
                Debug.WriteLine(s.Replace("{0}", string.Empty));
            else
                Debug.WriteLine(s.Replace("{0}", arg0.ToString()));
                
        }
    }
}
