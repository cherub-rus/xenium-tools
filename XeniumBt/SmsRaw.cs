using System;

namespace XeniumBt {

    [Serializable]
    public class SmsRaw {

        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public int cell;
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public string text;

        // ReSharper disable once MemberCanBePrivate.Global
        public SmsRaw() { }

        public SmsRaw(int cell, string text) {
            this.cell = cell;
            this.text = text;
        }

    }
}