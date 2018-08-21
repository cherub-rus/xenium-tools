using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

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
            StringBuilder vcards = new StringBuilder();
            IList<string> cardsData;
            string rawContactsFileName = config.RawContactsFile;
            if(rawContactsFileName == null || !File.Exists(rawContactsFileName)) {
                cardsData = LoadCardsFromPhone();
                if(rawContactsFileName != null) {
                    FileTools.Serialize(rawContactsFileName, cardsData);
                }
            } else {
                IList<string> tempData = (List<string>)FileTools.Deserialize(rawContactsFileName, typeof(List<string>));
                cardsData = new List<string>();
                foreach(string str in tempData) {
                    cardsData.Add(str.Replace("\n", "\r\n").Replace("\r\r\n", "\r\n"));
                }
            }

            foreach(string data in cardsData) {
                string card = CleanUpVCard(CommandParser.ParseEFSR(data.Replace("\r\n", "")));
                vcards.AppendLine(card);
            }

            string fileName = ".\\contacts_" + DateTime.Today.ToString("yyyyMMdd") + ".vcf";
            string vcfFile = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            File.WriteAllText(vcfFile, vcards.ToString(), Encoding.UTF8);
        }

        private IList<string> LoadCardsFromPhone() {
            try {
                IList<string> cardsData = new List<string>();

                modemHelper.Connect();
                modemHelper.DoCommand("AT+CSCS=\"UCS2\"");
                modemHelper.DoCommand("AT+CPBS=\"ME\"");
                int count = CommandParser.ParseCPBS(modemHelper.DoCommandWithResult("AT+CPBS?").Replace("\r\n", ""));

                for (int i = 1; i <= count; i++) {
                    cardsData.Add(GetVCard(i));
                }
                return cardsData;
            } finally {
                if (modemHelper != null) {
                    modemHelper.Disconnect();
                }
            }
        }

        private string GetVCard(int index) {
            modemHelper.DoCommand("AT+EVCARD=1," + index);
            modemHelper.DoCommand("AT+ESUO=3");

            const string vcardFileName = "C:\\Received\\~vcard_r.vcf";

            string fileNameEncoded = Ucs2Tools.UnicodeStringToHexString(vcardFileName);

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
    }
}