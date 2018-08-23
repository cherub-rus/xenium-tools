using System;

namespace XeniumBt {

    public abstract class SmsRawData {

        public string type;
        public string status;
        public string phoneNumber;
        public byte fo;
        public string cells;
        public DateTime date;
        public string text;

        public abstract string MessageKey { get; }

        public virtual string DebugInfo() {
            return
                "Cell : " + cells + ", " 
                + "Type : " + type + ", "
                + "Status : " + status + ", "
                + "Phone Number : " + phoneNumber + ", "
                + "First Octet : " + fo + ", "
                + "Date : " + date.ToString("dd-MM-yyyy HH:mm:ss");
        }

        public override string ToString() {
            return DebugInfo();
        }
    }
}