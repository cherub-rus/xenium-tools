using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace XeniumBt {

    public static class CommandParser {

        public const string ERROR_CODE = "ERROR";

        public static Tuple<int, int> ParseCPBS(string data) {

            const string pattern =
                @"^\+CPBS: ""(?<store>.+)"", (?<count>\d+), (?<total>\d+)$";

            IDictionary<string, string> result = ParseByRegex(data, pattern);

            string count = result["count"];
            string total = result["total"];

            return new Tuple<int, int>(Int32.Parse(count), Int32.Parse(total));
        }

        public static string ParseEVCARD(string data) {
            const string pattern =
                    @"^\+EVCARD: ""(?<name>.*)""$";

            IDictionary<string, string> result = ParseByRegex(data, pattern);
            return result["name"];
        }

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

        public static Tuple<int, int> ParseCPMS(string data) {

            const string pattern =
                    @"^\+CPMS: (?<count>\d+), (?<total>\d+), \d+, \d+, \d+, \d+$";

            IDictionary<string, string> result = ParseByRegex(data, pattern);

            string count = result["count"];
            string total = result["total"];

            return new Tuple<int, int>(Int32.Parse(count), Int32.Parse(total));
        }

        public static SmsRawData ParseCMGR(string str, int cellNumber) {
            const string pattern =
                    @"^\+CMGR: ""(?<info>.+)"",""(?<number>.+)"",,""(?<date>.+)"",\d+,(?<fo>\d+),\d+,\d+,""\d+"",\d+,\d+$";

            string[] lines = str.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            IDictionary<string, string> result = ParseByRegex(lines[0], pattern);

            SmsRawData data = new SmsRawData();
            data.type = result["info"];
            data.status = result["info"];
            data.phonenumber = result["number"];
            string dateWithTz = result["date"].Replace("+12", "+3:00").Replace("+28","+7:00");
            data.date = DateTime.Parse(dateWithTz);
            data.fo = byte.Parse(result["fo"]);

            string userdata = lines[1];
            if ((data.fo & 64) != 0) {
                int headerLength = (Ucs2Tools.HexStringToHexBytes(userdata.Substring(0, 2))[0] + 1)*2;
                data.userdataheader = userdata.Substring(0, headerLength);
                byte[] header = Ucs2Tools.HexStringToHexBytes(data.userdataheader);
                data.partlyCount = header[4];
                data.partlyNum = header[5];
                data.partlyId = header[3];
                userdata = userdata.Remove(0, headerLength);
            }
            data.cells.Add(cellNumber.ToString()); 
            data.text[data.partlyNum] = Ucs2Tools.HexStringToUnicodeString(userdata);
            return data;
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
