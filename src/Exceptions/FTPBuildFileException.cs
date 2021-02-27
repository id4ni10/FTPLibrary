using System;

namespace FTPLibrary.Exceptions
{
    public class FTPBuildFileException : FtpExeption
    {
        public FTPBuildFileException(Exception origin)
            : base(origin, $"Não foi possível construir o arquivo!{Environment.NewLine}{origin.Message}") => Console.WriteLine(this);
    }
}
