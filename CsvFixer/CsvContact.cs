using System;
using System.Collections.Generic;

namespace Org.XeniumTools.CsvFixer {

    public class CsvContact : IComparable<CsvContact> {

        private const string TYPE_NUMBER = "2";
        private const string TYPE_NUMBER2 = "4";
        private const string TYPE_HOME = "5";
        private const string TYPE_OFFICE = "3";

        private readonly string name;

        private readonly PhoneNumber number = new PhoneNumber(TYPE_NUMBER);
        private readonly PhoneNumber number2 = new PhoneNumber(TYPE_NUMBER2);
        private readonly PhoneNumber home = new PhoneNumber(TYPE_HOME);
        private readonly PhoneNumber office = new PhoneNumber(TYPE_OFFICE);

        private readonly List<string> errors = new List<string>();

        public CsvContact(string name) {
            this.name = name;
        }

        public string Name {
            get { return name; }
        }

        public PhoneNumber Number {
            get { return number; }
        }

        public PhoneNumber Number2 {
            get { return number2; }
        }

        public PhoneNumber Home {
            get { return home; }
        }

        public PhoneNumber Office {
            get { return office; }
        }

        public List<string> Errors {
            get { return errors; }
        }

        public int CompareTo(CsvContact other) {
            return String.Compare(name, other.name, StringComparison.CurrentCulture);
        }

        public PhoneNumber GetPhoneNumber(string type) {
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
