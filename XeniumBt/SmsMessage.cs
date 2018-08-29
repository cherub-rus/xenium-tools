using System;
using System.Collections.Generic;
using System.Globalization;

namespace XeniumBt {

    public class SmsMessage : SmsRawData {

        private string debug;
        private readonly IList<int> hasParts = new List<int>();

        public SmsMessage() { }

        public SmsMessage(SmsPart part, bool copyAll = false) {
            Type = part.Type;
            Status = part.Status;
            PhoneNumber = part.PhoneNumber;
            Fo = part.Fo;
            Date = part.Date;
            if (copyAll) {
                AddPart(part);
            }
        }

        public void AddPart(SmsPart part) {
            hasParts.Add(part.Number);
            Cells += part.Cells;
            Text += part.Text;
            debug += " " + part.PartInfo;
        }

        public bool HasPart(int num) {
            return hasParts.Contains(num);
        }

        public override string MessageKey => 
            $"{PhoneNumber}@{Date:yyyyMMddHHmmss}{Cells}";

        public string ToMtkPhoneSuiteSms() {
            string dateString = Date != DateTime.MinValue ? Date.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) : "";
            return $"\"{PhoneNumber}\"\t\"{dateString}\"\t\"{Text}\"";
        }

        protected override string DebugInfo() {
            return base.DebugInfo() + $"{debug}\r\nText : {Text}";
        }
    }
}