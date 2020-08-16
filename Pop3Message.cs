
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net.Mime;

namespace Iskai.Net.Mail
{
    public class Pop3Message 
    {
        /*
        private List<String> m_Recipients   = new List<string>;
        private List<String> m_CC           = new List<string>;
        private List<String> m_BCC          = new List<string>;
        */
        private string m_Source = null;

        private Dictionary<string,string> m_Headers   = new Dictionary<string, string>();
        private List<MessagePart> m_Parts = new List<MessagePart>();

        private string m_TextBody = null;

        // TODO: make internal
        public Pop3Message()
        {

        } //

        // TODO: make internal
        public static Pop3Message Parse(string messageData)
        {

            StringReader    sr = new StringReader(messageData);
            StringBuilder   sb = new StringBuilder();
            Pop3Message     msg = new Pop3Message();

            msg.m_Source = messageData;

            string body;
            string prevName = null;

            // parse headers
            while (true)
            {

                string line = sr.ReadLine();
                string name = null;
                string value = null;

                if (line == "")
                    break;
                else
                {
                    if (line.StartsWith("\t"))
                        msg.m_Headers[prevName] += line.Substring(1);
                    else if (line.IndexOf(":") > -1) 
                    {
                        name = line.Substring(0, line.IndexOf(":")).ToUpper();
                        value = line.Substring(line.IndexOf(":") + 2).Trim(); // one for colon, one for space after colon
                        prevName = name;

                        if (msg.m_Headers.ContainsKey(name))
                            msg.m_Headers[name] += "; " + value;
                        else
                            msg.m_Headers[name] = value;

                    }
                    sb.AppendLine(line);
                }
            }

            // parse out all the message parts based on the content boundary
            // that is part of the content-type header.
            ContentType contentType = msg.ContentType;

            if (!string.IsNullOrEmpty(contentType.Boundary))
            {
                // boundary exists. start parsing message parts
                msg.m_Parts = MessagePart.GetParts(sr.ReadToEnd(), contentType.Boundary);    
            }
            else
            {
                // content type isn't defined. assume plain text.
                //msg.m_Parts = null; // TODO: just add a 'Plain Text' part
               // msg.m_TextBody = sr.ReadToEnd(); // get rid of this
                msg.m_Parts = new List<MessagePart>();
                msg.m_Parts.Add(MessagePart.ParseNoHeaders(sr.ReadToEnd()));
            }
            

            /*
            System.Net.Mime.ContentType contentType = new System.Net.Mime.ContentType(msg.GetHeaderValue("CONTENT-TYPE"));

            switch (contentType.MediaType.ToLower())
            {
                case "multipart/related": 
                    {
                        msg.m_TextBody = "multipart related :-)";
                    }
                    break;

                case "multipart/mixed":
                    {
                        // TODO: lots of work :(
                        msg.m_TextBody = "multipart mixed :-)";
                    }
                    break;

                // everything else (including text only)
                default:
                    {
                        // rest of the message is the body
                        msg.m_TextBody = sr.ReadToEnd();
                    }
                break;
            }
            */
            
            return msg;

        } //


        private string GetHeaderValue(string headerName)
        {
            if (m_Headers.ContainsKey(headerName))
                return m_Headers[headerName];
            return string.Empty;
        } //

        private System.Net.Mail.MailAddress ParseEmailAddress(string data)
        {
            return new System.Net.Mail.MailAddress(data);
        } //

#region Properties

        public List<MessagePart> Parts
        {
            get { return m_Parts; }
        }

        public string Source
        {
            get { return m_Source; }
        }

        public System.Net.Mail.MailAddress From
        {
            // change to 'return-path' ?
            get
            {
                try
                {
                    string header = GetHeaderValue("FROM");
                    if (string.IsNullOrEmpty(header))
                        return null;
                    return ParseEmailAddress(header);
                }
                catch (FormatException ex)
                {
                    return null;
                }
            }
        }

        public System.Net.Mail.MailAddress To
        {
            get 
            {
                try
                {
                    string header = GetHeaderValue("TO");
                    if (string.IsNullOrEmpty(header))
                        return null;
                    return ParseEmailAddress(header);
                }
                catch (FormatException ex)
                {
                    return null;
                }
            }
        }

        public string TextBody
        {
            get 
            { 
                // find the first text/plain part
                for (int nPart = 0; nPart < this.Parts.Count; nPart++)
                {
                    if (string.Compare(this.Parts[nPart].ContentType.MediaType, "text/plain", true) == 0)
                        return this.Parts[nPart].Body;
                }
                return null;
            }
        }


        public string HtmlBody
        {
            get
            {
                // find the first text/html part
                for (int nPart = 0; nPart < this.Parts.Count; nPart++)
                {
                    if (string.Compare(this.Parts[nPart].ContentType.MediaType, "text/html", true) == 0)
                    {
                        StringReader sr = new StringReader(this.Parts[nPart].Body);
                        bool appendNext = false;
                        StringBuilder body = new StringBuilder();

                        while (true)
                        {
                            string line = sr.ReadLine();

                            if (line == null)
                                break;

                            if (appendNext)
                            {
                                body.AppendLine(line);
                                appendNext = false;
                            }
                            else
                            {
                                if (line.EndsWith("="))
                                {
                                    appendNext = true;
                                    body.Append(line.Substring(0, line.Length - 1));
                                }
                                else
                                    body.AppendLine(line);
                            }
                        }

                        return body.ToString();

                    }
                }
                return null;
            }
        }

            public Dictionary<string, string> Headers
        {
            get { return m_Headers; }
        }

        public string Subject
        {
            get { return GetHeaderValue("SUBJECT"); }
        }

        public System.Net.Mime.ContentType ContentType
        {
            get 
            {
                string headerValue = GetHeaderValue("CONTENT-TYPE");

                if (string.IsNullOrEmpty(headerValue))
                    headerValue = "text/plain";
                return new System.Net.Mime.ContentType(headerValue); }
        }

#endregion


        public void SaveToDisk(string filename)
        {
            System.IO.File.WriteAllText(filename, this.m_Source);
        }

    } // class

} // namespace
