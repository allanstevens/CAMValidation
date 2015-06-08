using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace AllanStevens.CAMValidation
{
    public enum MessageTypes
    {
        Information,
        Warning,
        Error,
        FailedValidation,
        PassedValidation
    }

    public class MessageCollection : CollectionBase
    {
        // Private Variables

        private int _informationCount = 0;
        private int _warningCount = 0;
        private int _errorCount = 0;
        private int _validationPassedCount = 0;
        private int _validationFailedCount = 0;

        // Class Properties

        public int InformationCount { get { return _informationCount; } }
        public int WarningCount { get { return _warningCount; } }
        public int ErrorCount
        {
            get { return _errorCount; }
            //set { _validationSuccess = value; }
        }
        public int PassedValidationCount { get { return _validationPassedCount; } }
        public int FailedValidationCount { get { return _validationFailedCount; } }

        // Public Methods

        public Message this[int index]
        {
            get
            {
                // if array is empty return null not crash!
                if (index < List.Count)
                {
                    return (Message)List[index];
                }
                else
                {
                    return null;
                }
            }
            set { List[index] = value; }
        }
        public Message this[string fieldName]
        {
            get
            {
                foreach (Message item in List)
                {
                    if (item.Name == fieldName) return (Message)item;
                }
                return null;
            }
        }
        public int Add(Message value)
        {
            if (value.Type == MessageTypes.Information) _informationCount++;
            if (value.Type == MessageTypes.Warning) _warningCount++;
            if (value.Type == MessageTypes.Error) _errorCount++;
            if (value.Type == MessageTypes.PassedValidation) _validationPassedCount++;
            if (value.Type == MessageTypes.FailedValidation) _validationFailedCount++;

            return (List.Add(value));
        }
    }

    public class Message
    {
        // Private Variables

        private string _name;
        private string _detail;
        //private string _friendlyMessage;
        private MessageTypes _type;

        // Constructors

        public Message(string name, string details, string friendlyMessage, MessageTypes type)
        {
            _name = name;
            _detail = details;
         //   _friendlyMessage = friendlyMessage;
            _type = type;
        }
        public Message(string name, string details, MessageTypes type)
        {
            _name = name;
            _detail = details;
            _type = type;
        }
        public Message(string name, MessageTypes type)
        {
            _name = name;
            _type = type;
        }
        
        // Class Properties

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        public string Details
        {
            get { return _detail; }
            set { _detail = value; }
        }
       // public string FriendlyMessage
       // {
       //     get { return _friendlyMessage; }
       //     set { _friendlyMessage = value; }
       // }
        public MessageTypes Type
        {
            get { return _type; }
            set { _type = value; }
        }

    }
}
