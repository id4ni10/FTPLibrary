using System;
using System.IO;
using System.Net;
using FTPLibrary.Exceptions;

namespace DsAdmin
{
    public class Ftp
    {
        public string FtpFullPath { get; set; }

        private FtpWebRequest Request { get; set; }

        private FtpWebResponse Response { get; set; }

        private NetworkCredential UserCredentials { get; set; }

        private Stream FtpStream { get; set; }

        /// <summary>
        /// Construtor do objeto Ftp.
        /// </summary>
        /// <param name="host">Host destino \nex: ftphost.com.br, ftphomologacao.com.br...</param>
        /// <param name="user">Nome do usuário</param>
        /// <param name="pass">Senha do usuário</param>
        public Ftp(string user, string pass)
        {
            UserCredentials = new NetworkCredential(user, pass);
        }

        private Ftp() { }

        private void SetupRequest(String ftpfullpath, String method)
        {
            try
            {
                FtpFullPath = ftpfullpath;

                Request = (FtpWebRequest)WebRequest.Create(ftpfullpath);

                Request.Credentials = UserCredentials;
            }
            catch { }
        }

        /// <summary>
        /// Cria um diretório relativo ao host.
        /// </summary>
        /// <param name="path">Diretório relativo ao Host. \nex: /testes/files, /arquivos</param>
        /// <returns>Retorna 'true' para sucesso.</returns>
        public bool CreateDirectory(String ftpfullpath)
        {
            try
            {
                SetupRequest(ftpfullpath, WebRequestMethods.Ftp.MakeDirectory);

                Request.UsePassive = true;
                Request.UseBinary = true;
                Request.KeepAlive = false;

                Response = (FtpWebResponse)Request.GetResponse();

                FtpStream = Response.GetResponseStream();

            }
            catch (WebException ex)
            {
                Response = (FtpWebResponse)ex.Response;

                Response.Close();

                throw new FtpExeption(ex, "Erro ao criar diretório no caminho: " + FtpFullPath);
            }
            finally
            {
                FtpStream.Close();
                Response.Close();
            }

            return true;
        }

        public byte[] DownloadFile(String ftpfullpath)
        {
            try
            {
                SetupRequest(ftpfullpath, WebRequestMethods.Ftp.DownloadFile);

                Response = (FtpWebResponse)Request.GetResponse();

                FtpStream = Response.GetResponseStream();

                var file = BuildFile(FtpStream);

                file.Close();
                FtpStream.Close();
                Response.Close();

                return file.ToArray();
            }
            catch (WebException ex)
            {
                Response = (FtpWebResponse)ex.Response;

                Response.Close();

                throw new FtpExeption(ex, "Erro ao baixar arquivo no caminho: " + FtpFullPath);
            }
        }

        private MemoryStream BuildFile(Stream stream)
        {
            MemoryStream file = new MemoryStream();
            try
            {
                byte[] buffer = new byte[2048];
                int readCount = stream.Read(buffer, 0, buffer.Length);

                while (readCount > 0)
                {
                    file.Write(buffer, 0, readCount);
                    readCount = stream.Read(buffer, 0, buffer.Length);
                }
            }
            catch { throw; }

            return file;
        }


        public bool UploadFile(String ftpfullpath, Byte[] file)
        {
            try
            {
                SetupRequest(ftpfullpath, WebRequestMethods.Ftp.UploadFile);

                Request.ContentLength = file.Length;

                FtpStream = Request.GetRequestStream();

                FtpStream.Write(file, 0, file.Length);

                FtpStream.Close();

                return true;
            }
            catch (WebException ex)
            {
                Response = (FtpWebResponse)ex.Response;

                Response.Close();

                throw new FtpExeption(ex, "Erro ao enviar o arquivo no caminho: " + FtpFullPath);
            }
        }

        public bool DeleteFile(String ftpfullpath)
        {
            try
            {
                SetupRequest(ftpfullpath, WebRequestMethods.Ftp.DeleteFile);

                Request.UseBinary = true;
                Request.UsePassive = true;
                Request.KeepAlive = true;

                Response = (FtpWebResponse)Request.GetResponse();

                Response.Close();
                return true;
            }
            catch (WebException ex)
            {
                Response = (FtpWebResponse)ex.Response;

                Response.Close();

                throw new FtpExeption(ex, "Erro ao apagar o arquivo no caminho: " + FtpFullPath);
            }
        }

    }
}
