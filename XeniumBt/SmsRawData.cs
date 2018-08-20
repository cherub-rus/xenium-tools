using System;
using System.Collections.Generic;
using System.Globalization;

namespace XeniumBt {

    public class SmsRawData {

        public List<string> cells = new List<string>();
        public string type;
        public string status;
        public string phonenumber;
        public DateTime date;
        public int partlyCount;
        public int partlyNum;
        public int partlyId;
        public byte fo;
        public string userdataheader;
        public string[] text = new string[10];

        public bool IsPartly => (partlyId  + partlyNum + partlyCount) > 0;

        public string MessageKey {
            get { return phonenumber + date.ToString("yyyyMMddHHmmss") + partlyId.ToString("0000"); }
        }

        public string PartGroupKey {
            get { return partlyId.ToString("0000") + date.ToString("yyyyMMddHHmm"); }
        }

        public override string ToString() {
            return DebugInfo();
        }

        public string ToMobilePhoneToolsSms() {
            return "Отправитель : " + phonenumber + "\r\n" +
                   "Время : " + date.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n" +
                   "Содержание : " + String.Join("", text)
                ;
        }

        public string ToMtkPhoneSuiteSms() {
            return "\"" + phonenumber + "\"\t\"" +
                   date.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) + "\"\t\"" +
                   String.Join("", text) + "\""
                ;
        }

        private string DebugInfo() {
            return
                "Cell : #" + String.Join(", #", cells) + ", "
                + "Type : " + type + ", "
                + "Status : " + status + ", "
                + "Partly : " + partlyCount + ":" + partlyNum + ":" + partlyId + ", "
                + "Phone Number : " + phonenumber + ", "
                + "Date : " + date.ToString("dd-MM-yyyy HH:mm:ss") + ", "
                + "First Octet : " + fo + ", "
                + "User Data Header : " + userdataheader + "\r\n"
                + "Text : " + String.Join("", text)
                ;
        }

    }

}