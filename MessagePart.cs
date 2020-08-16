
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net.Mime;

namespace Iskai.Net.Mail
{
    public class MessagePart
    {

        private String m_Source = null;
        private Dictionary<string, string> m_Headers = new Dictionary<string, string>();
        private String m_Body = null;

        private MessagePart() { }

        public ContentType ContentType
        {
            get
            {
                string headerValue = GetHeaderValue("CONTENT-TYPE");

                if (string.IsNullOrEmpty(headerValue))
                    headerValue = "text/plain";
                return new ContentType(headerValue);
            }
        }

        public string Body
        {
            get { return this.m_Body; }
        }


        private string GetHeaderValue(string headerName)
        {
            if (m_Headers.ContainsKey(headerName))
                return m_Headers[headerName];
            return string.Empty;
        } //

        public static List<MessagePart> GetParts(string source, string boundary)
        {
            List<MessagePart> parts = new List<MessagePart>();
            StringReader sr = new StringReader(source);
            string line;
            StringBuilder partsrc = null;

            while (true)
            {
                
                line = sr.ReadLine();

                if (line == null)
                    // end of headers
                    break;
                else
                {
                    if (string.Compare(line, "--" + boundary + "--", true) == 0)
                    {
                        // last boundary header
                        continue;
                    }
                    else if (string.Compare(line, "--" + boundary, true) == 0)
                    {
                        if (partsrc == null)
                        {
                            partsrc = new StringBuilder();
                        }
                        else
                        {
                            parts.Add(MessagePart.Parse(partsrc.ToString()));
                            partsrc = new StringBuilder();
                        }
                    }
                    else
                        partsrc.AppendLine(line);
                }

            }

            // add anything left as a message part
            parts.Add(MessagePart.Parse(partsrc.ToString()));

            return parts;

        } //


        public static MessagePart Parse(string source)
        {
            StringReader    sr = new StringReader(source);
            string          prevName = null;
            MessagePart     part = new MessagePart();

            part.m_Source = source;

            // parse headers
            while (true)
            {

                string line = sr.ReadLine();
                string name = null;
                string value = null;

                if (line == null)
                    break;

                if (line == "")
                    break;
                else
                {
                    if (line.StartsWith("\t"))
                        part.m_Headers[prevName] += line.Substring(1);
                    else if (line.IndexOf(":") > -1)
                    {
                        name = line.Substring(0, line.IndexOf(":")).ToUpper();
                        value = line.Substring(line.IndexOf(":") + 2).Trim(); // one for colon, one for space after colon
                        prevName = name;

                        if (part.m_Headers.ContainsKey(name))
                            part.m_Headers[name] += "; " + value;
                        else
                            part.m_Headers[name] = value;

                    }
                }
            }

            part.m_Body = sr.ReadToEnd();

            return part; 

        } // Parse

        public static MessagePart ParseNoHeaders(string source)
        {
            MessagePart part = new MessagePart();
            part.m_Source = source;
            part.m_Body = source;
            part.m_Headers.Add("CONTENT-TYPE", "text/plain");
            return part;
        }
        
    } // class
} // namespace
