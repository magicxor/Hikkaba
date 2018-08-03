using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeKicker.BBCode
{
    [Serializable]
    public class BBCodeParsingException : Exception
    {
        public BBCodeParsingException()
        {
        }
        public BBCodeParsingException(string message)
            : base(message)
        {
        }
    }
}