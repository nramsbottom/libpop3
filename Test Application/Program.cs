using System;
using System.Collections.Generic;
using System.Text;

namespace Test_Application
{
    class Program
    {
        static void Main(string[] args)
        {

            Iskai.Net.Mail.Pop3Client client = new Iskai.Net.Mail.Pop3Client("xxxxxxx", 
                                                                             110, 
                                                                             "xxxxxxx", 
                                                                             "xxxxxxx");
            Iskai.Net.Mail.Pop3Message msg;

            client.Connect();

            int messageCount = client.GetMessageCount();

            if (messageCount > 0)
            {
          
                for (int n = 0; n < messageCount; n++)
                {
                    int id = n + 1;

                    msg = Iskai.Net.Mail.Pop3Message.Parse(client.Retrieve(id));

                    IEmailService svc;
                    
                    //svc = new Services.AutoReplyService();
                    svc = new Services.SaveEmailService();

                    svc.Process(msg);

                    //Console.WriteLine("Message {0}", id);

                    //for (int nPart = 0; nPart < msg.Parts.Count; nPart++)
                    //{
                    //    Console.WriteLine("\tType: {0}", msg.Parts[nPart].ContentType.MediaType);                        
//
                    //}

                    //Console.WriteLine("======");

                    string html = msg.HtmlBody;
                    string body = null;

                    if (html != null)
                        body = html;
                    else
                        body = msg.TextBody;

                    System.Net.Mail.MailAddress to;
                    System.Net.Mail.MailAddress from;

                    to = msg.To;
                    from = msg.From;
                    


                    Console.WriteLine("New Email");
                    Console.WriteLine("=========");

                    if (from != null)
                        Console.WriteLine("From:     {0,-25} <{1}>", from.DisplayName, from.Address);
                    else
                        Console.WriteLine("From: {0,-25}<{1}>", "unknown", "unknown");
                    
                    if (to != null)
                        Console.WriteLine("To:       {0,-25} <{1}>", to.DisplayName, to.Address);
                    else
                        Console.WriteLine("To:       {0,-25} <{1}>", "unknown", "unknown");

                    Console.WriteLine("Subject:  {0,-25}", msg.Subject);

                    Console.WriteLine("Parts: {0,-25}", msg.Parts.Count);

                    Console.WriteLine("Text Body:");
                    Console.WriteLine(msg.TextBody);

                    Console.WriteLine();

                    //Console.WriteLine(body);

                    //client.Delete(id);

                    //Console.WriteLine("Content Boundary: " + msg.ContentType.Boundary);
                    //Console.WriteLine("Number of parts: " + msg.Parts.Count);

                    //Console.WriteLine(msg.TextBody);                    
                }
                
            }
            else
                Console.WriteLine("There are no messages in your inbox.");

            client.Disconnect();

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();

        }
    } // class
} // namespace


