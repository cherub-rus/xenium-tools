using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace XeniumBt {

    internal class SmsProcessor {

        private readonly ModemHelper modemHelper;
        private readonly Config config;

        public SmsProcessor(Config config, ModemHelper modemHelper) {
            this.config = config;
            this.modemHelper = modemHelper;
        }

        public void GetMessages() {
            IList<CellData> rawSms;

            string rawSmsFileName = config.RawSmsFile;
            if (rawSmsFileName == null || !File.Exists(rawSmsFileName)) {
                rawSms = LoadMessagesFromPhone();
                if (rawSmsFileName != null) {
                    FileTools.Serialize(rawSmsFileName, rawSms);
                }
            } else {
                rawSms = (List<CellData>)FileTools.Deserialize(rawSmsFileName, typeof(List<CellData>));
                foreach (CellData sms in rawSms) {
                    sms.text = sms.text.Replace("\n", "\r\n").Replace("\r\r\n", "\r\n");
                }
            }

            IList<SmsRawData> smsList = new List<SmsRawData>();
            foreach (CellData msgData in rawSms) {
                string msg = msgData.text;

                if (msg.StartsWith("\r\n+CMGR: \"REC READ\"") ||
                    msg.StartsWith("\r\n+CMGR: \"REC UNREAD\"")) {
                    SmsRawData data = CommandParser.ParseCMGR(msg, msgData.cell);
                    smsList.Add(data);
                }
            }

            StringBuilder smsDebug = new StringBuilder();
            StringBuilder smsOut = new StringBuilder();
            smsOut.AppendLine("#sorted");
            smsOut.AppendLine("Phone Number\tTime\tMessage");

            IList<SmsMessage> combined = CombineSms(smsList);
            foreach (SmsMessage data in combined) {
                smsDebug.AppendLine(data.ToString()).AppendLine();
                if (config.PhoneFilter == null || config.PhoneFilter == data.phoneNumber) {
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

        private IList<CellData> LoadMessagesFromPhone() {
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
                } catch (Exception) {
                    WriteLog("CPMSResult :" + CPMSResult);
                    throw;
                }

                int count = 0;
                List<CellData> rawSms = new List<CellData>();
                for (int i = 1; i <= stat.Item2; i++) {
                    if (count < stat.Item1) {
                        string result = modemHelper.DoCommandWithResult("AT+CMGR=" + i);
                        if (result.Replace("\r\n", "").Equals(CommandParser.ERROR_CODE)) {
                            continue;
                        }
                        rawSms.Add(new CellData(i, result));
                        count++;
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

        private IList<SmsMessage> CombineSms(IList<SmsRawData> raw) {
            SortedList<string, SmsMessage> result = new SortedList<string, SmsMessage>();
            SortedList<string, List<SmsPart>> sl = new SortedList<string, List<SmsPart>>();

            foreach (SmsRawData data in raw) {
                if (data is SmsPart multi) {
                    string key = multi.PartsGroupKey;
                    if (!sl.ContainsKey(key)) {
                        sl.Add(key, new List<SmsPart>());
                    }

                    sl[key].Add(multi);
                } else {
                    result.Add(data.MessageKey, (SmsMessage)data);
                }
            }

            foreach (string key in sl.Keys) {
                List<SmsPart> list = sl[key];
                SmsPart first = list.First();
                int totalParts = first.totalParts;

                if (list.Count < 2) {
                    WriteLog("Invalid parts. key = " + key);
                    result.Add(first.MessageKey, new SmsMessage(first, true));
                    continue;
                }

                IList<SmsPart> ordered = list.OrderBy(i => i.PartInMessageKey).ToList();
                IList<SmsMessage> combined = new List<SmsMessage>();

                for (int i = 1; i <= totalParts; i++) {
                    foreach (SmsPart part in ordered.Where(p => p.number == i)) {
                        List<SmsMessage> notFilled = combined.Where(c => !c.HasPart(i)).ToList();
                        SmsMessage message = FindNearestMessage(notFilled, part);
                        if (message != null) {
                            message.AddPart(part);
                        } else {
                            combined.Add(new SmsMessage(part, true));
                        }
                    }
                }

                foreach (SmsMessage part in combined) {
                    result.Add(part.MessageKey, part);
                }
            }

            return result.Values;
        }

        private SmsMessage FindNearestMessage(IList<SmsMessage> messages, SmsPart part) {
            foreach (SmsMessage message in messages) {
                if (message.date == part.date) {
                    return message;
                }
            }

            TimeSpan dateSpread = new TimeSpan(0, 0, 30);
            foreach (SmsMessage message in messages) {
                if (part.date.Subtract(message.date).Duration() <= dateSpread) {
                    return message;
                }
            }

            return null;
        }

        private void WriteLog(string message) {
            FileTools.WriteLog(config.LogFile, message);
        }
    }
}