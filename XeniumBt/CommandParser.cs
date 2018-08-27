using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace XeniumBt {

    public static class CommandParser {

        public const string ERROR_CODE = "ERROR";

        // ReSharper disable once InconsistentNaming
        public static Tuple<int, int> ParseCPBS(string data) {

            const string pattern =
                @"^\+CPBS: ""(?<store>.+)"", (?<count>\d+), (?<total>\d+)$";

            IDictionary<string, string> result = ParseByRegex(data, pattern);

            string count = result["count"];
            string total = result["total"];

            return new Tuple<int, int>(Int32.Parse(count), Int32.Parse(total));
        }

        // ReSharper disable once InconsistentNaming
        public static string ParseEVCARD(string data) {
            const string pattern =
                    @"^\+EVCARD: ""(?<name>.*)""$";

            IDictionary<string, string> result = ParseByRegex(data, pattern);
            return result["name"];
        }

        // ReSharper disable once InconsistentNaming
        public static string ParseEFSR(string data) {

            string[] parts = data.Split( new [] { "+EFSR" }, StringSplitOptions.RemoveEmptyEntries);
            string vcard = "";

            const string pattern =
                @"^\+EFSR\: (?<partnum>\d+), (?<digit2>\d+), (?<size>\d+), ""(?<vcard>.+)?""$";

            foreach (string part in parts) {
                IDictionary<string, string> result = ParseByRegex("+EFSR" + part, pattern);

                string partText = Ucs2Tools.HexStringToASCIIString(result["vcard"]);
                vcard += partText;
            }
            return vcard;
        }

        // ReSharper disable once InconsistentNaming
        public static Tuple<int, int> ParseCPMS(string data) {

            const string pattern =
                    @"^\+CPMS: (?<count>\d+), (?<total>\d+), \d+, \d+, \d+, \d+$";

            IDictionary<string, string> result = ParseByRegex(data, pattern);

            string count = result["count"];
            string total = result["total"];

            return new Tuple<int, int>(Int32.Parse(count), Int32.Parse(total));
        }

        // ReSharper disable once InconsistentNaming
        public static SmsRawData ParseCMGR(string str, int cellNumber) {
            const string pattern =
                    @"^\+CMGR: ""(?<info>.+)"",""(?<number>.+)"",,""(?<date>.+)"",\d+,(?<fo>\d+),\d+,\d+,""\d+"",\d+,\d+$";

            string[] lines = str.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            IDictionary<string, string> result = ParseByRegex(lines[0], pattern);
            bool isPart = (byte.Parse(result["fo"]) & 64) != 0;

            SmsRawData data = isPart ? (SmsRawData)new SmsPart() : new SmsMessage();

            data.type = result["info"];
            data.status = result["info"];
            data.phoneNumber = result["number"];
            data.date = DateTime.Parse(FixTimeZoneOffset(result["date"]));
            data.fo = byte.Parse(result["fo"]);
            data.cells = $"#{cellNumber:000}";

            string userData = lines[1];
            if (data is SmsMessage) {
                data.text = Ucs2Tools.HexStringToUnicodeString(userData);
            } else if (data is SmsPart part) {
                int headerLength = (Ucs2Tools.HexStringToHexBytes(userData.Substring(0, 2))[0] + 1)*2;
                part.userdataheader = userData.Substring(0, headerLength);
                // TODO header processing
                byte[] header = Ucs2Tools.HexStringToHexBytes(part.userdataheader);
                part.id = header[3];
                part.totalParts = header[4];
                part.number = header[5];
                part.text = Ucs2Tools.HexStringToUnicodeString(userData.Substring(headerLength));
            }
            return data;
        }

        private static string FixTimeZoneOffset(string dateString) {
            Tuple<string, string> dateFix = null;
            char[] splitters = {'+', '-'};
            foreach (char splitter in splitters) {
                int tzIndex = dateString.LastIndexOf(splitter);
                if (tzIndex >= dateString.Length - 3) {
                    string qTz = dateString.Substring(tzIndex);
                    int qOffset = Int32.Parse(qTz.Substring(1));
                    string offset = new TimeSpan(0, qOffset * 15, 0).ToString(@"hh\:mm");
                    dateFix = new Tuple<string, string>(qTz, splitter + offset);
                    break;
                }
            }

            return dateFix == null
                ? dateString
                : dateString.Replace(dateFix.Item1, dateFix.Item2);

        }

        private static IDictionary<string, string> ParseByRegex(String sdr, string pattern) {
            Regex regex = new Regex(pattern, RegexOptions.Multiline);
            Match m = regex.Match(sdr);
            if (!m.Success) {
                throw new Exception("No matches found. pattern = '" + pattern + "'");
            }
            IDictionary<string, string> result = new Dictionary<string, string>();
            foreach (string groupName in regex.GetGroupNames()) {
                result.Add(groupName, m.Groups[groupName].Value);
            }
            return result;
        }
    }
}
