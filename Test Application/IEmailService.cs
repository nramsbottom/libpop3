using System;
using System.Collections.Generic;
using System.Text;

namespace Test_Application
{
    public interface IEmailService
    {
        void Process(Iskai.Net.Mail.Pop3Message msg);
    }
}
