 using System;
using System.Collections.Generic;
using System.Text;

namespace Iskai.Net.Mail
{
    class Pop3Exception : ApplicationException 
    {

        private string m_ProtocolData = null;

        public Pop3Exception(string message, Exception innerException)  : base(message, innerException) { }
        public Pop3Exception(string message) : base(message, null) { }
        public Pop3Exception(string message, string protocolData) : this(message, null, protocolData) { }

        public Pop3Exception(string message, Exception innerException, string protocolData) : base(message, innerException) {
            this.m_ProtocolData = protocolData;
        }

        public string ProtocolData {
            get { return this.m_ProtocolData; }
        }

    } // class

} // namespace
