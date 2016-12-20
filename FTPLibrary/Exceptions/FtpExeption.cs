using System;

namespace FTPLibrary.Exceptions
{
    public class FtpExeption : Exception
    {
        public Exception Origin { get; private set; }

        public FtpExeption(Exception origin, String message)
            : base(message)
        {
            Origin = origin;
        }
    }
}
