using System;

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