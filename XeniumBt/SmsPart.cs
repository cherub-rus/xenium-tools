
namespace XeniumBt {

    public class SmsPart : SmsRawData {

        public int id;
        public int totalParts;
        public int number;
        public string userdataheader;

        public string PartInfo => 
            $"Partly : {totalParts}:{number}:{id}, User Data Header : {userdataheader}";

        public override string MessageKey => 
            $"{phoneNumber}@{date:yyyyMMddHHmmss}{id:0000}{totalParts:0000}{cells}";

        public string PartsGroupKey => 
            //TODO remove
            $"{phoneNumber}@{id:0000}{totalParts:0000}";

        public string PartInMessageKey => 
            $"{number:0000}{date:yyyyMMddHHmmss}{cells}";

        public override string DebugInfo() {
            return base.DebugInfo() + $" {PartInfo}\r\nText : {text}";
        }
    }
}