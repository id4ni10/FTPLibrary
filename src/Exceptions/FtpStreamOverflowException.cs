using System;

namespace FTPLibrary.Exceptions
{
    public class FtpStreamOverflowException : FtpExeption
    {
        public FtpStreamOverflowException(Exception origin)
            : base(origin, "O comprimento máximo do Stream excedeu o limite máximo!") => Console.WriteLine(this);
    }
}
