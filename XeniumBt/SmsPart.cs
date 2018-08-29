
namespace XeniumBt {

    public class SmsPart : SmsRawData {

        public int Id { private get; set; }
        public int TotalParts { get; set; }
        public int Number { get; set; }
        public string UserDataHeader { get; set; }

        public string PartInfo => 
            $"Partly : {TotalParts}:{Number}:{Id}, User Data Header : {UserDataHeader}";

        public override string MessageKey => 
            $"{PhoneNumber}@{Date:yyyyMMddHHmmss}{Id:0000}{TotalParts:0000}{Cells}";

        public string PartsGroupKey => 
            $"{PhoneNumber}@{Id:0000}{TotalParts:0000}";

        public string PartInMessageKey => 
            $"{Number:0000}{Date:yyyyMMddHHmmss}{Cells}";

        protected override string DebugInfo() {
            return base.DebugInfo() + $" {PartInfo}\r\nText : {Text}";
        }
    }
}