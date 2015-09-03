using System;

namespace FTPLibrary.Exceptions
{
    public class FtpExeption : Exception
    {
        private Exception origin;

        public Exception Origin { get { return origin; } }

        public FtpExeption(Exception origin, String message)
            : base(message)
        {
            this.origin = origin;
        }

    }
}
