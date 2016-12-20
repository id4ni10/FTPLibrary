using System;

namespace FTPLibrary.Exceptions
{
    public class FtpStreamOverflowException : FtpExeption
    {
        public FtpStreamOverflowException(Exception ex)
            : base(ex, string.Format("O comprimento máximo do Stream deverá ser de {0}!", int.MaxValue))
        {
        }
    }
}
