using System;
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

        public override string MessageKey {
            get { return phoneNumber + "@" +
                         date.ToString("yyyyMMddHHmmss") + 
                         cells;
            }
        }

        protected string ToMobilePhoneToolsSms() {
            return "Отправитель : " + phoneNumber + "\r\n" +
                   "Время : " + date.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n" +
                   "Содержание : " + String.Join("", text)
                ;
        }

        public string ToMtkPhoneSuiteSms() {
            return "\"" + phoneNumber + "\"\t\"" +
                   date.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) + "\"\t\"" +
                   String.Join("", text) + "\""
                ;
        }

        public override string DebugInfo() {
            return base.DebugInfo() + 
                   debug +
                   "\r\nText : " + text;
        }
    }
}