using System;
using System.Collections.Generic;

namespace Org.XeniumTools.Common {

    public class CsvContact : IComparable<CsvContact> {

        private const string TYPE_NUMBER = "2";
        private const string TYPE_NUMBER2 = "4";
        private const string TYPE_HOME = "5";
        private const string TYPE_OFFICE = "3";

        private readonly string name;

        private readonly CsvPhoneNumber number = new CsvPhoneNumber(TYPE_NUMBER);
        private readonly CsvPhoneNumber number2 = new CsvPhoneNumber(TYPE_NUMBER2);
        private readonly CsvPhoneNumber home = new CsvPhoneNumber(TYPE_HOME);
        private readonly CsvPhoneNumber office = new CsvPhoneNumber(TYPE_OFFICE);

        private readonly List<string> errors = new List<string>();

        public CsvContact(string name) {
            this.name = name;
        }

        public string Name {
            get { return name; }
        }

        public CsvPhoneNumber Number {
            get { return number; }
        }

        public CsvPhoneNumber Number2 {
            get { return number2; }
        }

        public CsvPhoneNumber Home {
            get { return home; }
        }

        public CsvPhoneNumber Office {
            get { return office; }
        }

        public List<string> Errors {
            get { return errors; }
        }

        public int CompareTo(CsvContact other) {
            return String.Compare(name, other.name, StringComparison.CurrentCulture);
        }

        public CsvPhoneNumber GetPhoneNumber(string type) {
            switch (type) {
                case TYPE_NUMBER:
                    return number;
                case TYPE_NUMBER2:
                    return number2;
                case TYPE_HOME:
                    return home;
                case TYPE_OFFICE:
                    return office;
                default:
                    return null;
            }
        }
    }
}
