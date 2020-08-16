
// http://www.electrictoolbox.com/article/networking/pop3-commands/

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Iskai.Net.Mail
{
    public class Pop3Client
    {

        private Socket  m_DataSocket = null;
        private string  m_Host = null;
        private int     m_Port = 110;

        private string  m_Username = null;
        private string  m_Password = null;

        public Pop3Client(string host, int port, string username, string password) {
            m_Host = host;
            m_Port = port;
            m_Username = username;
            m_Password = password;
        }

        public void Connect() {
            
            // NGR: name resolution needs beefing up
            IPEndPoint ipep = new IPEndPoint(Dns.GetHostEntry(m_Host).AddressList[0], m_Port);
            string response = null;

            this.m_DataSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.m_DataSocket.Connect(ipep);

            if (!this.m_DataSocket.Connected){
                Disconnect();
                throw new Pop3Exception("Unable to connect to server");
            }

            // read banner
            response = ReadSingleLineResponse();

            if (IsErrorResponse(response)){
                Disconnect();
                throw new Pop3Exception("Server error after connected", response);
            }

            SendCommand("USER", m_Username);
            response = ReadSingleLineResponse();

            if (IsErrorResponse(response)){
                Disconnect();
                throw new Pop3Exception("Server rejected username", response);
            }

            SendCommand("PASS", m_Password);
            response = ReadSingleLineResponse();

            if (IsErrorResponse(response))
            {
                Disconnect();
                throw new Pop3Exception("Server rejected password", response);
            }

        } //

        public void Disconnect()
        {
            if (this.m_DataSocket != null) {

                if (this.m_DataSocket.Connected)
                {
                    // not too bothered about the response here
                    SendCommand("QUIT");
                    ReadSingleLineResponse();
                }

                this.m_DataSocket.Shutdown(SocketShutdown.Both);
                this.m_DataSocket.Close();
                this.m_DataSocket = null;
            }
        } //

        #region Properties

        public bool IsConnected
        {
            get { return this.m_DataSocket.Connected; }
        } //

        #endregion 

        #region Helpers

        // NGR: this is broken
        public bool RetrieveToFile(int id, out string filename) {

            string tempDirectory = Environment.GetFolderPath(Environment.SpecialFolder.InternetCache);
            string tempFilename = Guid.NewGuid().ToString();
            string tempFilePath = Path.Combine(tempDirectory, tempFilename);

            FileStream fs = File.Open(tempFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);

            System.Diagnostics.Debug.WriteLine(" -> Downloading message " + id.ToString() + " to " + tempFilePath);

            SendCommand("RETR", id.ToString());
            ReadMultiLineResponseToStream(fs);

            fs.Flush();
            fs.Close();

            filename = tempFilePath;
            return true;

        } //

        public int GetMessageCount() {

            SendCommand("STAT");

            string response = ReadSingleLineResponse();

            if (IsErrorResponse(response))
                throw new Pop3Exception("Unable to get message count", response);

            Match match = Regex.Match(response, @"^\+OK\s(\d+)\s(\d+)$", RegexOptions.IgnoreCase);

            if (match != null && match.Groups.Count > 1)
                return int.Parse(match.Groups[1].Value);
            else
                return 0;

        } //

        private bool IsErrorResponse(string response) {
            if (response.StartsWith("-ERR"))
                return true;
            return false;
        } //

        public void Delete(int id) {
            SendCommand("DELE", id.ToString());
        } //

        public void Reset() {
            SendCommand("RSET");
        } //

        public string Retrieve(int id) {
            
            SendCommand("RETR", id.ToString());
            
            return RetrieveMessage(id);
        } //

        #endregion

        private void SendCommand(string command, params string[] args) {
            
            foreach(string arg in args) {
                command += " ";
                command += arg;
            }
            
            this.SendCommand(command);

        } //

        private void SendCommand(string command, string args) {
            this.SendCommand(command + " " + args);
        } //

        private void SendCommand(string command) {

            NetworkStream   ns = new NetworkStream(this.m_DataSocket);
            StreamWriter    sw = new StreamWriter(ns);

            System.Diagnostics.Debug.WriteLine(" -> " + command);

            sw.WriteLine(command);
            sw.Flush();

        } //

        private string ReadSingleLineResponse() {

            NetworkStream   ns = new NetworkStream(this.m_DataSocket);
            StreamReader    sr = new StreamReader(ns);
            string          response = sr.ReadLine();
            
            System.Diagnostics.Debug.WriteLine(" <- " + response);

            return response;

        } //

        private string ReadMultiLineResponse() {
            
            NetworkStream   ns = new NetworkStream(this.m_DataSocket);
            StreamReader    sr = new StreamReader(ns);
            StringBuilder   sb = new StringBuilder();

            while(true) {
                
                string line = sr.ReadLine();

                sb.AppendLine(line);

                if (line == ".")
                    break;

            }

            System.Diagnostics.Debug.WriteLine(" <- " + sb.ToString());

            return sb.ToString();
            
        } //

        private void ReadMultiLineResponseToStream(Stream stream) {

            NetworkStream   ns = new NetworkStream(this.m_DataSocket);
            StreamReader    sr = new StreamReader(ns);
            StreamWriter    sw = new StreamWriter(stream);
            string          response = ReadSingleLineResponse();

            if (IsErrorResponse(response))
                throw new Pop3Exception("Unable to receive response", response);

            while (true) {
                
                string line = sr.ReadLine();

                if (line == ".")
                    break;
                else
                    sw.WriteLine(line);
            }

            sw.Flush();

        } //

        private string RetrieveMessage(int id)
        {

            NetworkStream   ns = new NetworkStream(this.m_DataSocket);
            StreamReader    sr = new StreamReader(ns);
            StringBuilder   sb = new StringBuilder();
            StringWriter    sw = new StringWriter(sb);
            string          response = ReadSingleLineResponse();

            if (IsErrorResponse(response))
                throw new Pop3Exception("Unable to receive response", response);

            while (true)
            {

                string line = sr.ReadLine();

                if (line == ".")
                    break;
                else
                    sw.WriteLine(line);
            }

            sw.Flush();

            return sb.ToString();

        } //

    } // class
} // namespace
