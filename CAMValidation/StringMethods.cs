using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AllanStevens.CAMValidation
{
    class StringMethods 
    {
        static public string RemovedWhiteSpace(string s)
        {
            s = s.Trim()
                 .Replace("' )","')")
                 .Replace("not( /","not(/")
                 .Replace("not( //","not(//");

            if(s.Contains("  "))
            {
                while(s.Contains("  "))
                {
                    s = s.Replace("  "," ");
                }
            }
            return s.Trim();
        }

        static public string ArrayToString(string[] stringArray)
        {
            string returnValue = "";
            foreach (string s in stringArray)
            {
                returnValue = returnValue + "," + s;
            }
            if (returnValue.Length != 0) returnValue = returnValue.Substring(1);
            return returnValue;
        }
    }
}
