using System.Collections.Generic;

namespace Org.XeniumTools.Common {

    public static class CsvParser {

        public static List<CsvContact> Parse(string[] lines) {
            List<CsvContact> contacts = new List<CsvContact>();

            for (int j = 1; j < lines.Length; j++) {
                string[] temp = lines[j].Split(',');
                CsvContact contact = new CsvContact(temp[2].Trim('"'));

                for (int i = 4; i <= 10 && i < temp.Length; i = i + 2) {
                    SetNumber(contact, temp[i].Trim('"'), temp[i - 1].Trim('"'));
                }
                contacts.Add(contact);
            }
            return contacts;
        }

        private static void SetNumber(CsvContact contact, string number, string type) {
            CsvPhoneNumber field = contact.GetPhoneNumber(type);
            if (field == null) {
                contact.Errors.Add(
                    string.Format(
                        "Unknown number type \"{0}\". Number: {1}",
                        type, number));
                return;
            }

            if (field.Number == null) {
                field.Number = number;
            } else {
                contact.Errors.Add(
                    string.Format(
                        "Number with type {0} already exist. Existed number : {1}. New number: {2}",
                        type, field.Number, number));
            }
        }
    }
}