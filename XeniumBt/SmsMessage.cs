using System.Globalization;

namespace XeniumBt {

    public class SmsMessage : SmsRawData {

        public string debug;

        public SmsMessage() { }

        public SmsMessage(SmsPart part, bool copyAll = false) {
            type = part.type;
            status = part.status;
            phoneNumber = part.phoneNumber;
            fo = part.fo;
            date = part.date;
            if (copyAll) {
                cells = part.cells;
                text = part.text;
                debug = " " + part.PartInfo;
            }
        }

        public override string MessageKey => 
            $"{phoneNumber}@{date:yyyyMMddHHmmss}{cells}";

        protected string ToMobilePhoneToolsSms() {
            return "Отправитель : " + phoneNumber + "\r\n" +
                   "Время : " + date.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n" +
                   "Содержание : " + text;
        }

        public string ToMtkPhoneSuiteSms() {
            string dateString = date.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
            return $"\"{phoneNumber}\"\t\"{dateString}\"\t\"{text}\"";
        }

        public override string DebugInfo() {
            return base.DebugInfo() + $"{debug}\r\nText : {text}";
        }
    }
}