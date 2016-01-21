using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using Org.XeniumTools.Common;

namespace Org.XeniumTools.vCardGenerator {
    
    public static class Builder {

        private const string HEADER = "BEGIN:VCARD\r\nVERSION:2.1";
        private const string FOOTER = "END:VCARD";

        private const string FULL_NAME_FORMAT = "N:{0};;;;";
        private const string NAME_FORMAT = "N:{0};{1};;;";

        private const string NUMBER_FORMAT = "TEL;CELL:{0}";
        private const string NUMBER2_FORMAT = "TEL;WORK;FAX:{0}";
        private const string HOME_FORMAT = "TEL;HOME:{0}";
        private const string OFFICE_FORMAT = "TEL;WORK:{0}";
        
        public static StringBuilder BuildFileContent(IList<CsvContact> contacts) {

            StringBuilder fileContent = new StringBuilder();
            foreach (CsvContact contact in contacts) {
                fileContent.AppendLine(HEADER);
                fileContent.AppendLine(PrepareName(contact));
                AppendNumber(fileContent, contact.Number, NUMBER_FORMAT);
                AppendNumber(fileContent, contact.Number2, NUMBER2_FORMAT);
                AppendNumber(fileContent, contact.Home, HOME_FORMAT);
                AppendNumber(fileContent, contact.Office, OFFICE_FORMAT);
                fileContent.AppendLine(FOOTER);
            }
            return fileContent;
        }

        private static string PrepareName(CsvContact contact) {
            string name = contact.Name;
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
            Regex regex = new Regex(@"^\d.*$", RegexOptions.IgnoreCase);
            Match m = regex.Match(name);
            if (m.Success) {
                return true;
            }
            if (name.StartsWith("0")) {
                return true;
                
            }
            return false;
        }

        private static void AppendNumber(StringBuilder fileContent, CsvPhoneNumber number, string format) {
            if (!string.IsNullOrEmpty(number.Number)) {
                fileContent.AppendLine(string.Format(format, number.Number));
            }
        }

    }
}
