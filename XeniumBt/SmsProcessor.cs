using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace XeniumBt {

    internal class SmsProcessor {

        private const string RAW_SMS_TEMP_FILE = "rawSms.txt";
        private readonly string phonefilter;
        private readonly ModemHelper modemHelper;
        private readonly string logFile;

        public SmsProcessor(string phonefilter, ModemHelper modemHelper, string logFile) {
            this.phonefilter = phonefilter;
            this.modemHelper = modemHelper;
            this.logFile = logFile;
        }

        public void GetMessages() {
            List<SmsRaw> rawSms;

            if (!File.Exists(RAW_SMS_TEMP_FILE)) {
                rawSms = LoadMessagesFromPhone();
                Serialize(RAW_SMS_TEMP_FILE, rawSms);
            } else {
                rawSms = (List<SmsRaw>)Deserialize(RAW_SMS_TEMP_FILE, typeof(List<SmsRaw>));
                foreach (SmsRaw sms in rawSms) {
                    sms.text = sms.text.Replace("\n", "\r\n").Replace("\r\r\n", "\r\n");
                }
            }

            IList<SmsRawData> smsList = new List<SmsRawData>();
            foreach (SmsRaw msgData in rawSms) {
                string msg = msgData.text;

                if (msg.StartsWith("\r\n+CMGR: \"REC READ\"") ||
                    msg.StartsWith("\r\n+CMGR: \"REC UNREAD\"")) {
                    SmsRawData data = CommandParser.ParseCMGR(msg, msgData.cell);
                    smsList.Add(data);
                }
            }

            StringBuilder smsDebug = new StringBuilder();
            StringBuilder smsOut = new StringBuilder();
            smsOut.AppendLine("Phone Number\tTime\tMessage");

            IList<SmsRawData> combined = CombineSms(smsList);
            foreach (SmsRawData data in combined) {
                smsDebug.AppendLine(data.ToString()).AppendLine();
                if (phonefilter == null || phonefilter == data.phonenumber) {
//                    smsOut.AppendLine(data.ToMobilePhoneToolsSms()).AppendLine();
                    smsOut.AppendLine(data.ToMtkPhoneSuiteSms());
                }
            }

            string smsDebugFile = Path.Combine(Directory.GetCurrentDirectory(), ".\\sms_debug.txt");
            File.WriteAllText(smsDebugFile, smsDebug.ToString(), Encoding.UTF8);

            string fileName = ".\\mysms_" + DateTime.Today.ToString("yyyyMMdd") + ".txt";
            string smsFile = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            File.WriteAllText(smsFile, smsOut.ToString(), Encoding.UTF8);
        }

        private List<SmsRaw> LoadMessagesFromPhone() {
            try {
                modemHelper.Connect();

                modemHelper.DoCommand("ATZ");
                modemHelper.DoCommand("AT+CSCS=\"UCS2\"");
                modemHelper.DoCommand("AT+CMGF=1");
                modemHelper.DoCommand("AT+CSDH=1");

                string CPMSResult = modemHelper.DoCommandWithResult("AT+CPMS=\"ME\",\"SM\",\"MT\"").Replace("\r\n", "");

                Tuple<int, int> stat;
                try {
                    stat = CommandParser.ParseCPMS(CPMSResult);
                } catch (Exception e) {
                    Console.WriteLine("CPMSResult :" + CPMSResult);
                    Console.WriteLine(e);
                    throw;
                }

                int count = 0;
                List<SmsRaw> rawSms = new List<SmsRaw>();
                for (int i = 1; i <= stat.Item2; i++) {
                    if (count < stat.Item1) {
                        try {
                            rawSms.Add(new SmsRaw(i, modemHelper.DoCommandWithResult("AT+CMGR=" + i)));
                            count++;
                        } catch (Exception e) {
                            //TODO  Console.WriteLine(e);
                            //log Error reading message from cell #x. Scipping.
                        }
                    } else {
                        break;
                    }
                }
                return rawSms;
            } finally {
                if (modemHelper != null) {
                    modemHelper.Disconnect();
                }
            }
        }

        private IList<SmsRawData> CombineSms(IList<SmsRawData> raw) {
            SortedList<string, SmsRawData> result = new SortedList<string, SmsRawData>();
            SortedList<string, SortedList<int, SmsRawData>> sl = new SortedList<string, SortedList<int, SmsRawData>>();

            foreach (SmsRawData data in raw) {
                if (!data.IsPartly) {
                    if (!result.ContainsKey(data.MessageKey)) {
                        result.Add(data.MessageKey, data);
                    } else {
                        // debug
                        WriteLog("MessageKey duplicate\r\n" +
                                 " adding = " + data + "\r\n" +
                                 " existed =" + result[data.MessageKey]);
                    }
                    continue;
                }

                if (!sl.ContainsKey(data.PartGroupKey)) {
                    sl.Add(data.PartGroupKey, new SortedList<int, SmsRawData>());
                }

                if (sl[data.PartGroupKey].ContainsKey(data.partlyNum)) {
                    // debug
                    WriteLog("partlyNum duplicate:\r\n" +
                             " adding  =" + data + "\r\n" + 
                             " existed =" + sl[data.PartGroupKey][data.partlyNum]);
                    continue;
                }
                sl[data.PartGroupKey].Add(data.partlyNum, data);
            }

            foreach (string key in sl.Keys) {
                SortedList<int, SmsRawData> parts = sl[key];
                if (parts.Count < 2){
                    WriteLog("Invalid parts. data=" + parts.Values[0]);
                    continue;
                }
                SmsRawData data = parts[1];
                for (int i = 2; i < parts.Count + 1; i++) {
                    data.text[i] = parts[i].text[i];
                    data.cells.AddRange(parts[i].cells);
                }
                result.Add(data.MessageKey, data);
            }

            return result.Values;
        }

        private void WriteLog(string message) {
            File.AppendAllLines(logFile, new[] { message }, Encoding.UTF8);
        }

        private void Serialize(string filename, object data) {
            FileStream fileStream =
                new FileStream(filename, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fileStream, Encoding.UTF8);
            XmlSerializer s = new XmlSerializer(data.GetType());
            s.Serialize(sw, data);
            sw.Close();
            fileStream.Close();
        }

        private object Deserialize(string filename, Type type) {
            FileStream fileStream =
                new FileStream(filename, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fileStream, Encoding.UTF8);
            try {
                XmlSerializer s = new XmlSerializer(type);
                return s.Deserialize(sr);
            } finally {
                sr.Close();
                fileStream.Close();
            }
        }

    }

}