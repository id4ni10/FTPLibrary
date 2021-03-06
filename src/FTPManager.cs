﻿using System;
using System.IO;
using System.Net;
using FTPLibrary.Exceptions;

namespace FTPLibrary
{
    public class Ftp : IDisposable
    {
        public string Host { get; set; }

        public Uri Uri { get; private set; }

        private FtpWebRequest Request { get; set; }

        private FtpWebResponse Response { get; set; }

        private NetworkCredential UserCredentials { get; set; }

        private Stream FtpStream { get; set; }

        private (bool passive, bool binary, bool alive)? Settings { get; set; }

        /// <summary>
        /// Construtor do objeto Ftp.
        /// </summary>
        /// <param name="host">Nome do host</param>
        /// <param name="user">Nome do usuario</param>
        /// <param name="pass">Senha do usuario</param>
        public Ftp(string host, string user, string pass, (bool passive, bool binary, bool alive)? settings = null)
        {
            Host = host;
            UserCredentials = new NetworkCredential(user, pass);
            Settings = settings;
        }

        public Ftp() { }

        ~Ftp() { this.Dispose(); }

        /// <summary>
        /// Cria um diretorio relativo ao host.
        /// </summary>
        /// <param name="path">Diretorio relativo ao Host. ex: /files, /arquivos/</param>
        /// <returns>Retorna true para sucesso.</returns>
        public bool CreateDirectory(string path = null)
        {
            try
            {
                SetupRequest(path, WebRequestMethods.Ftp.MakeDirectory);

                Response = (FtpWebResponse)Request.GetResponse();

                FtpStream = Response.GetResponseStream();

                Response.Close();

                FtpStream.Close();
            }
            catch (WebException ex)
            {
                Response = (FtpWebResponse)ex.Response;

                Response.Close();

                if (Response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    return true;

                throw new FtpExeption(ex, "Erro ao criar diretório no caminho: " + Uri.AbsoluteUri);
            }
            catch (Exception e)
            {
                throw new FtpExeption(e, "Erro ao realizar a operação desejada.");
            }

            return true;
        }

        /// <summary>
        /// Baixa um arquivo do host destino.
        /// </summary>
        /// <param name="filename">Arquivo relativo ao Host. ex: file.txt, arquivos/other.pdf</param>
        /// <param name="offset">Marca de início (byte), para a leitura do arquivo no servidor.</param>
        /// <exception cref="FtpExeption"></exception>
        /// <returns>Retorna o arquivo desejado.</returns>
        public byte[] DownloadFile(string filename, long offset = 0)
        {
            try
            {
                SetupRequest(filename, WebRequestMethods.Ftp.DownloadFile, offset);

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

                throw new FtpExeption(ex, "Erro ao baixar arquivo no caminho: " + Uri.AbsoluteUri);
            }
            catch (Exception e)
            {
                throw new FtpExeption(e, "Erro ao realizar a operação desejada.");
            }
        }

        /// <summary>
        /// Envia um arquivo
        /// </summary>
        /// <param name="path">Diretorio relativo ao Host. ex: /testes/files/file.txt, /arquivos/other.pdf</param>
        /// <param name="file">Arquivo a ser enviado.</param>
        /// <exception cref="FtpExeption"></exception>
        /// <returns>Retorna true para sucesso.</returns>
        public bool UploadFile(string path, byte[] file, long offset = 0)
        {
            try
            {
                SetupRequest(path, WebRequestMethods.Ftp.UploadFile, offset);

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

                throw new FtpExeption(ex, "Erro ao enviar o arquivo no caminho: " + Uri.AbsoluteUri);
            }
            catch (Exception e)
            {
                throw new FtpExeption(e, "Erro ao realizar a operação desejada.");
            }
        }

        /// <summary>
        /// Envia um arquivo
        /// </summary>
        /// <param name="path">Diretorio relativo ao Host. ex: /testes/files/file.txt, /arquivos/other.pdf</param>
        /// <param name="file">Arquivo a ser enviado.</param>
        /// <exception cref="FtpExeption"></exception>
        /// <returns>Retorna true para sucesso.</returns>
        public bool UploadFile(string path, Stream stream, long offset = 0)
        {
            try
            {
                SetupRequest(path, WebRequestMethods.Ftp.UploadFile, offset);

                Request.ContentLength = stream.Length;

                FtpStream = Request.GetRequestStream();

                var ms = new MemoryStream();

                stream.CopyTo(ms);

                FtpStream.Write(ms.ToArray(), 0, Convert.ToInt32(ms.Length));

                FtpStream.Close();

                return true;
            }
            catch (WebException ex)
            {
                Response = (FtpWebResponse)ex.Response;

                Response.Close();

                throw new FtpExeption(ex, "Erro ao enviar o arquivo no caminho: " + Uri.AbsoluteUri);
            }
            catch (OverflowException exp)
            {
                throw new FtpStreamOverflowException(exp);
            }
            catch (Exception e)
            {
                throw new FtpExeption(e, "Erro ao realizar a operação desejada.");
            }
        }

        /// <summary>
        /// Apaga o arquivo desejado no servidor.
        /// </summary>
        /// <param name="path">Diretorio relativo ao Host. ex: /testes/files/file.txt, /arquivos/other.pdf</param>
        /// <exception cref="FtpExeption"></exception>
        /// <returns>Retorna true para sucesso.</returns>
        public bool DeleteFile(string path)
        {
            try
            {
                SetupRequest(path, WebRequestMethods.Ftp.DeleteFile);

                Response = (FtpWebResponse)Request.GetResponse();

                Response.Close();

                if (Response.StatusCode.Equals(
                    FtpStatusCode.ActionAbortedLocalProcessingError |
                    FtpStatusCode.ActionAbortedUnknownPageType |
                    FtpStatusCode.ActionNotTakenFilenameNotAllowed |
                    FtpStatusCode.ActionNotTakenFileUnavailable |
                    FtpStatusCode.ActionNotTakenFilenameNotAllowed |
                    FtpStatusCode.ActionNotTakenFileUnavailableOrBusy |
                    FtpStatusCode.ActionNotTakenInsufficientSpace))
                    return false;

                return true;
            }
            catch (WebException ex)
            {
                Response = (FtpWebResponse)ex.Response;

                Response.Close();

                throw new FtpExeption(ex, "Erro ao apagar o arquivo no caminho: " + Uri.AbsoluteUri);
            }
            catch (Exception e)
            {
                throw new FtpExeption(e, "Erro ao realizar a operação desejada.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <exception cref="WebException"></exception>
        /// <returns></returns>
        public long GetFileSize(string filename)
        {
            try
            {
                SetupRequest(filename, WebRequestMethods.Ftp.GetFileSize);

                Response = (FtpWebResponse)Request.GetResponse();

                var size = Response.ContentLength;

                Response.Close();

                return size;
            }
            catch (WebException ex)
            {
                Response = (FtpWebResponse)ex.Response;

                Response.Close();

                if (Response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    return 0;

                throw new FtpExeption(ex, "Não foi possível encontrar o arquivo no caminho: " + Uri.AbsoluteUri);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <param name="offset"></param>
        /// <exception cref="Exception"></exception>
        private void SetupRequest(string path, string method, long offset = 0)
        {
            try
            {
                Uri uri;

                var success = Uri.TryCreate(Host + (string.IsNullOrWhiteSpace(path) ? "" : path), UriKind.RelativeOrAbsolute, out uri);

                if (!success)
                    throw new UriFormatException(uri.AbsoluteUri);

                Uri = uri;

                Request = (FtpWebRequest)WebRequest.Create(uri);

                if (offset > 0)
                    Request.ContentOffset = offset;

                Request.Method = method;

                Request.Credentials = UserCredentials;

                if (Settings.HasValue)
                {
                    Request.UsePassive = Settings.Value.passive;
                    Request.UseBinary = Settings.Value.binary;
                    Request.KeepAlive = Settings.Value.alive;
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Constroi um arquivo a partir de um stream.
        /// </summary>
        /// <param name="stream">Stream contendo o arquivo.</param>
        /// <exception cref="FtpExeption"></exception>
        /// <returns>Arquivo em um MemoryStream</returns>
        private MemoryStream BuildFile(Stream stream)
        {
            try
            {
                var file = new MemoryStream();

                var buffer = new byte[2048];

                var readCount = stream.Read(buffer, 0, buffer.Length);

                while (readCount > 0)
                {
                    file.Write(buffer, 0, readCount);
                    readCount = stream.Read(buffer, 0, buffer.Length);
                }

                return file;
            }
            catch (Exception ex)
            {
                throw new FTPBuildFileException(ex);
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
