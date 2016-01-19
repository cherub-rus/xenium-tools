namespace Org.XeniumTools.Common {

    public class PhoneNumber {

        private readonly string type;

        public PhoneNumber(string type) {
            this.type = type;
        }

        public string Type {
            get { return type; }
        }

        public string Number { get; set; }

        public void InitWhenNull() {
            if (Number == null) {
                Number = string.Empty;
            }
        }

        public override string ToString() {
            return Number;
        }
    }
}