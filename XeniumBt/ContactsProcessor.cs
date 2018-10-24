using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using XeniumBt.Objects;

namespace XeniumBt {

    internal class ContactsProcessor {

        private const string FULL_NAME_FORMAT = "N:{0};;;;";
        private const string NAME_FORMAT = "N:{0};{1};;;";

        private readonly ModemHelper modemHelper;
        private readonly Config config;

        public ContactsProcessor(Config config, ModemHelper modemHelper) {
            this.config = config;
            this.modemHelper = modemHelper;
        }

        public void GetContacts() {
            IList<CellData> cardsData;
            string rawContactsFileName = config.RawContactsFile;
            if(rawContactsFileName == null || !File.Exists(rawContactsFileName)) {
                cardsData = LoadCardsFromPhone();
                if(rawContactsFileName != null) {
                    FileTools.Serialize(rawContactsFileName, cardsData);
                }
            } else {
                cardsData = (List<CellData>)FileTools.Deserialize(rawContactsFileName, typeof(List<CellData>));
                foreach(CellData data in cardsData) {
                    data.text = data.text.Replace("\n", "\r\n").Replace("\r\r\n", "\r\n");
                }
            }

            SortedList<string,string> sl = new SortedList<string, string>();
            foreach(CellData data in cardsData) {
                string card = CleanUpVCard(CommandParser.ParseEFSR(data.text.Replace("\r\n", "")));
                string name = card.Split('\n').Single(s => s.StartsWith("N:")).Replace(';',' ');
                sl.Add(name, card);
            }

            string fileName = ".\\contacts_" + DateTime.Today.ToString("yyyyMMdd") + ".vcf";
            string vcfFile = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            File.WriteAllLines(vcfFile, sl.Values, Encoding.UTF8);
        }

        private IList<CellData> LoadCardsFromPhone() {
            try {
                modemHelper.Connect();

                modemHelper.DoCommand("AT+CSCS=\"UCS2\"");
                modemHelper.DoCommand("AT+CPBS=\"ME\"");
                string CPBSResult = modemHelper.DoCommandWithResult("AT+CPBS?").Replace("\r\n", "");

                Tuple<int, int> stat;
                try {
                    stat = CommandParser.ParseCPBS(CPBSResult);
                } catch(Exception) {
                    WriteLog("CPBSResult :" + CPBSResult);
                    throw;
                }

                int count = 0;
                IList<CellData> cardsData = new List<CellData>();
                for(int i = 1; i <= stat.Item2; i++) {
                    if(count < stat.Item1) {
                        string result = GetVCard(i);
                        if(result.Replace("\r\n", "").Equals(CommandParser.ERROR_CODE)) {
                            continue;
                        }
                        cardsData.Add(new CellData(i, result));
                        count++;
                    } else {
                        break;
                    }
                }

                return cardsData;
            } finally {
                if (modemHelper != null) {
                    modemHelper.Disconnect();
                }
            }
        }

        private string GetVCard(int index) {
            string fileNameEncoded = CommandParser.ParseEVCARD(modemHelper.DoCommandWithResult("AT+EVCARD=1," + index).Replace("\r\n", ""));
            if (fileNameEncoded.Length == 0) {
                return CommandParser.ERROR_CODE;
            }

            modemHelper.DoCommand("AT+ESUO=3");

            string data = modemHelper.DoCommandWithResult($"AT+EFSR=\"{fileNameEncoded}\"");

            modemHelper.DoCommand($"AT+EFSD=\"{fileNameEncoded}\"");
            modemHelper.DoCommand("AT+ESUO=4");
            return data;
        }

        private static string CleanUpVCard(string message) {
            string card = message
                    .Replace("=\r\n", "")
                    .Replace("TEL;CELL:\r\n", "")
                    .Replace("TEL;HOME:\r\n", "")
                    .Replace("TEL;WORK:\r\n", "")
                    .Replace("TEL;FAX:\r\n", "")
                ;

            const string namePattern = @"N\;CHARSET=UTF-8\;ENCODING=QUOTED-PRINTABLE\:\;(?<name>.+)\;\;\;";

            return new Regex(namePattern).Replace(card, NameFixer);
        }

        private static string NameFixer(Match m) {
            string encodedName = m.Groups["name"].Value;
            string decodedName = new Regex("(\\=([0-9A-F]{2}))+", RegexOptions.IgnoreCase).Replace(encodedName, QuotedPrintableEvaluator);
            return PrepareName(decodedName);
        }

        private static string QuotedPrintableEvaluator(Match m) {
            return DecodeQuotedPrintables(m.Groups[0].Value, Encoding.UTF8);
        }

        private static string DecodeQuotedPrintables(string input, Encoding encoding) {
            Regex regex = new Regex(@"\=(?<symbol>[0-9A-Z]{2})", RegexOptions.Multiline);
            MatchCollection matches = regex.Matches(input);
            byte[] bytes = new byte[matches.Count];

            for (int i = 0; i < matches.Count; i++) {
                bytes[i] = Convert.ToByte(matches[i].Groups["symbol"].Value, 16);
            }
            return encoding.GetString(bytes);
        }

        private static string PrepareName(string name) {
            if (IsSolidName(name)) {
                return string.Format(FULL_NAME_FORMAT, name);
            }

            int lastNameLenth = name.IndexOf(" ", StringComparison.Ordinal);
            string lastName = name.Substring(0, lastNameLenth);
            string firstName = name.Substring(lastNameLenth + 1, name.Length - lastNameLenth - 1);

            return string.Format(NAME_FORMAT, lastName, firstName);
        }

        private static bool IsSolidName(string name) {
            if (!name.Contains(" ")) {
                return true;
            }

            Regex regex = new Regex(@"^\d\s.*$", RegexOptions.IgnoreCase);
            Match m = regex.Match(name);
            if (m.Success) {
                return true;
            }

            return false;
        }

        private void WriteLog(string message) {
            FileTools.WriteLog(config.LogFile, message);
        }
    }
}