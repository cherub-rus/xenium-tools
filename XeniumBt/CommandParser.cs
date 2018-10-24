using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using XeniumBt.Objects;

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
                    @"^\+CMGR: ""(?<type>.+)\s(?<status>.+)"",""(?<number>.+)"",,(""(?<date>.*)""){0,1}\d{0,3},\d+,(?<fo>\d+),\d+,\d+,""\d*"",\d+,\s{0,1}\d+$";

            string[] lines = str.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            IDictionary<string, string> result = ParseByRegex(lines[0], pattern);
            bool isPart = (byte.Parse(result["fo"]) & 64) != 0;

            SmsRawData data = isPart ? (SmsRawData)new SmsPart() : new SmsMessage();

            data.Type = result["type"];
            data.Status = result["status"];
            data.PhoneNumber = result["number"];
            string dateString = result["date"];
            data.Date = dateString.Length == 0 ? DateTime.MinValue : DateTime.Parse(FixTimeZoneOffset(dateString));
            data.Fo = byte.Parse(result["fo"]);
            data.Cells = $"#{cellNumber:000}";

            string userData = lines[1];
            if (data is SmsMessage) {
                data.Text = Ucs2Tools.HexStringToUnicodeString(userData);
            } else if (data is SmsPart part) {
                ProcessUserData(part, userData);
            }
            return data;
        }

        private static void ProcessUserData(SmsPart part, string userData) {
            part.HeaderLength = (Ucs2Tools.HexStringToHexBytes(userData.Substring(0, 2))[0] + 1) * 2;
            part.UserDataHeader = userData.Substring(0, part.HeaderLength);
            part.Text = Ucs2Tools.HexStringToUnicodeString(userData.Substring(part.HeaderLength));

            part.Info = GetPartInfo(ProcessDataHeader(part.UserDataHeader));
        }

        private static IDictionary<int, HeaderEntry> ProcessDataHeader(string userDataHeader) {
            IDictionary<int, HeaderEntry> entries = new Dictionary<int, HeaderEntry>();

            byte[] header = Ucs2Tools.HexStringToHexBytes(userDataHeader);

            for(int i = 1; i < header.Length; i++) {
                HeaderEntry entry = new HeaderEntry {
                    Type = header[i],
                    Size = header[i + 1]
                };
                entry.Data = header.Skip(i + 2).Take(entry.Size).ToArray();
                i = i + 2 + entry.Size;
                entries.Add(entry.Type, entry);
            }
            return entries;
        }

        private static PartInfo GetPartInfo(IDictionary<int, HeaderEntry> entries) {
            // ReSharper disable once InconsistentNaming
            const int CONCATENATED_8_BIT = 0;

            if (entries.ContainsKey(CONCATENATED_8_BIT)) {
                byte[] data = entries[CONCATENATED_8_BIT].Data;
                return new PartInfo {
                    Id = data[0],
                    TotalParts = data[1],
                    Number = data[2]
                };
            }

            // ReSharper disable once InconsistentNaming
            const int CONCATENATED_16_BIT = 8;

            if (entries.ContainsKey(CONCATENATED_16_BIT)) {
                byte[] data = entries[CONCATENATED_16_BIT].Data;
                return new PartInfo {
                    Id = (data[0]<<8) + data[1],
                    TotalParts = data[2],
                    Number = data[3]
                };
            }

            return null;
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
