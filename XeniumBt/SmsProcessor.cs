using System;
using System.Collections.Generic;
using System.IO;
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
            smsOut.AppendLine("Phone Number\tTime\tMessage");

            IList<SmsRawData> combined = CombineSms(smsList);
            foreach (SmsRawData data in combined) {
                smsDebug.AppendLine(data.ToString()).AppendLine();
                if (config.PhoneFilter == null || config.PhoneFilter == data.phonenumber) {
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
                                 $" adding  = {data}\r\n" +
                                 $" existed = {result[data.MessageKey]}");
                    }
                    continue;
                }

                if (!sl.ContainsKey(data.PartGroupKey)) {
                    sl.Add(data.PartGroupKey, new SortedList<int, SmsRawData>());
                }

                if (sl[data.PartGroupKey].ContainsKey(data.partlyNum)) {
                    // debug
                    WriteLog("partlyNum duplicate:\r\n" +
                             $" adding  = {data}\r\n" +
                             $" existed = {sl[data.PartGroupKey][data.partlyNum]}");
                    continue;
                }
                sl[data.PartGroupKey].Add(data.partlyNum, data);
            }

            foreach (string key in sl.Keys) {
                SortedList<int, SmsRawData> parts = sl[key];
                if (parts.Count < 2){
                    WriteLog("Invalid parts. data = " + parts.Values[0]);
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
            FileTools.WriteLog(config.LogFile, message);
        }
    }
}