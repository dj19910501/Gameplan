using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RevenuePlanner.Helpers
{
    // Added By: Maninder Singh Wadhva on 12/10/2014 to address ticket #900 Exception handling in client side scripting
    // Summary:
    //     Represents javascript errors.
    public class JavaScriptException : Exception
    {
        private string _message;

        // Summary:
        //     Gets a message that describes the current javascript exception.
        //
        // Returns:
        //     The error message that explains the reason for the javascript exception, or an empty
        //     string("").
        public override string Message
        {
            get
            {
                if (_message.Contains(": at document path "))
                {
                    return _message.Substring(0, _message.IndexOf(": at document path "));
                }
                return _message;
            }
        }

        //
        // Summary:
        //     Initializes a new instance of the JavaScriptException class with a specified
        //     error message.
        //
        // Parameters:
        //   message:
        //     The message that describes the error.
        public JavaScriptException(string message)
            : base(message)
        {
            this._message = message;
        }

        //
        // Summary:
        //     Creates and returns a string representation of the current javascript exception.
        //
        // Returns:
        //     A string representation of the current javascript exception.
        public override string ToString()
        {
            return _message;
        }
    }
}