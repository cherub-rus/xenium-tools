
namespace XeniumBt {

    public class SmsPart : SmsRawData {

        public int id;
        public int totalParts;
        public int partlyNum;
        public string userdataheader;

        public string PartInfo {
            get {
                return $"Partly : {totalParts}:{partlyNum}:{id}, User Data Header : {userdataheader}";
            }
        }

        public override string MessageKey {
            get {
                return phoneNumber + "@" +
                       date.ToString("yyyyMMddHHmmss") +
                       id.ToString("0000") +
                       totalParts.ToString("0000") +
                       cells;
            }
        }

         public string PartsGroupKey {
            get {
                return phoneNumber + "@" +
                       id.ToString("0000") +
                       totalParts.ToString("0000")
                       //TODO remove
                       + date.ToString("yyyyMMddHHmm")
                    ;
            }
        }


        public override string DebugInfo() {
            return base.DebugInfo() + " " + PartInfo + "\r\nText : " + text;
        }
    }
}