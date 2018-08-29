using System;

namespace XeniumBt {

    public abstract class SmsRawData {

        public string @Type { get; set; }
        public string Status { get; set; }
        public string PhoneNumber { get; set; }
        public byte Fo { get; set; }
        public string Cells { get; set; }
        public DateTime Date { get; set; }
        public string Text { get; set; }

        public abstract string MessageKey { get; }

        protected virtual string DebugInfo() {
            return
                "Cell : " + Cells + ", " 
                + "Type : " + Type + ", "
                + "Status : " + Status + ", "
                + "Phone Number : " + PhoneNumber + ", "
                + "First Octet : " + Fo + ", "
                + "Date : " + Date.ToString("dd-MM-yyyy HH:mm:ss");
        }

        public override string ToString() {
            return DebugInfo();
        }
    }
}