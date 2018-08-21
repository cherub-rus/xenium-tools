using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;

namespace XeniumBt {

    internal class Program {

        private readonly Config config;
        private readonly ContactsProcessor contactsProcessor;
        private readonly SmsProcessor smsProcessor;

        private static void Main() {

            try {
                Program program = new Program();
                program.LoadConfiguration();
                program.ClearLog();
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
            ModemHelper modemHelper = new ModemHelper(config);
            contactsProcessor = new ContactsProcessor(modemHelper);
            smsProcessor = new SmsProcessor(config, modemHelper);
        }

        private void ClearLog() {
            File.Delete(config.LogFile);
        }

        private void LoadConfiguration() {
            NameValueCollection settings = ConfigurationManager.AppSettings;

            config.LogFile = Path.Combine(Directory.GetCurrentDirectory(), "XeniumBt.log");
            config.PortName = settings["port_name"];
            config.BaudRate = Int32.Parse(settings["baud_rate"]);
            config.Timeout = Int32.Parse(settings["timeout"]);
            config.PhoneFilter = settings["phone_filter"];
            config.LoadContacts = Boolean.Parse(settings["load_contacts"]);
            config.LoadSms = Boolean.Parse(settings["load_sms"]);
            config.RawContactsFile = settings["raw_contacts_file"];
            config.RawSmsFile = settings["raw_sms_file"];
        }

        private void Do() {
            if (config.LoadContacts) {
                contactsProcessor.GetContacts();
            }
            if (config.LoadSms) {
                smsProcessor.GetMessages();
            }
        }

        private static void WriteLog(Exception exp) {
            Console.WriteLine(exp);
        }
    }
}