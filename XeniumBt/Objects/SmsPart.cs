namespace XeniumBt.Objects {

    public class SmsPart : SmsRawData {

        public PartInfo Info { get; set; }
        public string UserDataHeader { get; set; }
        public int HeaderLength { get; set; }

        public string PartInfo =>
            $"Partly : {Info.TotalParts}:{Info.Number}:{Info.Id}, User Data Header : {UserDataHeader}";

        public override string MessageKey =>
            $"{PhoneNumber}@{Date:yyyyMMddHHmmss}{Info.Id:0000}{Info.TotalParts:0000}{Cells}";

        public string PartsGroupKey =>
            $"{PhoneNumber}@{Info.Id:0000}{Info.TotalParts:0000}";

        public string PartInMessageKey =>
            $"{Info.Number:0000}{Date:yyyyMMddHHmmss}{Cells}";

        protected override string DebugInfo() {
            return base.DebugInfo() + $" {PartInfo}\r\nText : {Text}";
        }

    }

}