using System;
using System.IO;
using System.Net;
using FTPLibrary.Exceptions;

namespace FTPLibrary
{
    public class Ftp
    {
        private string FtpFullPath { get; set; }

        private FtpWebRequest Request { get; set; }

        private FtpWebResponse Response { get; set; }

        private NetworkCredential UserCredentials { get; set; }

        private Stream FtpStream { get; set; }

        /// <summary>
        /// Construtor do objeto Ftp.
        /// </summary>
        /// <param name="user">Nome do usuario</param>
        /// <param name="pass">Senha do usuario</param>
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
            catch { throw; }
        }

        /// <summary>
        /// Cria um diretorio relativo ao host.
        /// </summary>
        /// <param name="ftpfullpath">Diretorio relativo ao Host. \nex: ftp://testes/files, ftp://arquivos/</param>
        /// <returns>Retorna true para sucesso.</returns>
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
            catch (Exception e)
            {
                throw new FtpExeption(e, "Erro ao realizar a operação desejada.");
            }
            finally
            {
                FtpStream.Close();
                Response.Close();
            }

            return true;
        }

        /// <summary>
        /// Baixa um arquivo do host destino.
        /// </summary>
        /// <param name="ftpfullpath">Diretorio relativo ao Host. \nex: ftp://testes/files/file.txt, ftp://arquivos/other.pdf</param>
        /// <returns>Retorna true para sucesso.</returns>
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
            catch (Exception e)
            {
                throw new FtpExeption(e, "Erro ao realizar a operação desejada.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream">Stream contendo o arquivo.</param>
        /// <returns>Arquivo em um MemoryStream</returns>
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

        /// <summary>
        /// Envia um arquivo 
        /// </summary>
        /// <param name="ftpfullpath">Diretorio relativo ao Host. \nex: ftp://testes/files/file.txt, ftp://arquivos/other.pdf</param>
        /// <param name="file">Arquivo a ser enviado.</param>
        /// <returns>Retorna true para sucesso.</returns>
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
            catch (Exception e)
            {
                throw new FtpExeption(e, "Erro ao realizar a operação desejada.");
            }
        }

        /// <summary>
        /// Apaga o arquivo desejado no servidor.
        /// </summary>
        /// <param name="ftpfullpath">Diretorio relativo ao Host. \nex: ftp://testes/files/file.txt, ftp://arquivos/other.pdf</param>
        /// <returns>Retorna true para sucesso.</returns>
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
            catch (Exception e)
            {
                throw new FtpExeption(e, "Erro ao realizar a operação desejada.");
            }
        }
    }
}
