namespace XeniumBt {

    internal class Config {

        public string LogFile { get; set; }
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public int Timeout { get; set; }
        public string PhoneFilter { get; set; }
        public bool LoadContacts { get; set; }
        public bool LoadSms { get; set; }
        public string RawContactsFile { get; set; }
        public string RawSmsFile { get; set; }
    }
}