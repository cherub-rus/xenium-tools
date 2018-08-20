using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;

namespace XeniumBt {

    internal class Program {

        private static readonly string logFile = 
            Path.Combine(Directory.GetCurrentDirectory(), ".\\XeniumBt.log");

        private readonly Config config;
        private readonly ContactsProcessor contactsProcessor;
        private readonly SmsProcessor smsProcessor;

        private static void Main() {

            File.Delete(logFile);
            try {
                Program program = new Program();
                program.LoadConfiguration();
                program.Do();
                Console.WriteLine("done");
            }
            catch (Exception e) {
                WriteLog(e);
                Console.WriteLine("error");
            }
            Console.ReadKey();
        }

        private Program() {
            config = new Config();
            ModemHelper modemHelper = new ModemHelper(config, logFile);
            contactsProcessor = new ContactsProcessor(modemHelper);
            smsProcessor = new SmsProcessor(config.PhoneFilter, modemHelper, logFile);
        }

        private void LoadConfiguration() {
            NameValueCollection settings = ConfigurationManager.AppSettings;

            config.PortName = settings["port_name"];
            config.BaudRate = Int32.Parse(settings["baud_rate"]);
            config.Timeout = Int32.Parse(settings["timeout"]);
            config.PhoneFilter = settings["phone_filter"];
        }

        private void Do() {
            // Todo contacts
            if (false) {
                contactsProcessor.GetContacts();
            }
            smsProcessor.GetMessages();
        }

        private static void WriteLog(Exception exp) {
            Console.WriteLine(exp);
        }
    }
}