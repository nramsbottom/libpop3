
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Test_Application.Services
{
    public class SaveEmailService : IEmailService
    {
        #region IEmailService Members

        void IEmailService.Process(Iskai.Net.Mail.Pop3Message msg)
        {

            string tempDirectory = @"C:\tmp\email\in\";
            string tempFilename = Guid.NewGuid().ToString();
            string tempFilePath = Path.Combine(tempDirectory, tempFilename + ".txt");
            
            FileStream fs = File.Open(tempFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            StreamWriter sw = new StreamWriter(fs);

            sw.Write(msg.Source);
            
            sw.Flush();
            sw.Close();
        }

        #endregion
    }
}
