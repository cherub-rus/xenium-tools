using System;
using System.Globalization;
using System.Text;

namespace XeniumBt {
     
    public static class Ucs2Tools {

        public static String UnicodeStringToHexString(String strMessage) {
            byte[] bytes = Encoding.BigEndianUnicode.GetBytes(strMessage);
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        public static String HexStringToUnicodeString(String strHex) {
            byte[] bytes = HexStringToHexBytes(strHex);
            return Encoding.BigEndianUnicode.GetString(bytes, 0, bytes.Length);
        }

        public static String HexStringToASCIIString(String strHex)
        {
            byte[] bytes = HexStringToHexBytes(strHex);
            return Encoding.ASCII.GetString(bytes, 0, bytes.Length);
        }

        public static byte[] HexStringToHexBytes(String hexString) {

            if (hexString == null) {
                throw new ArgumentNullException(nameof(hexString));
            }

            if ((hexString.Length & 1) != 0) {
                throw new ArgumentOutOfRangeException(nameof(hexString), hexString, "Hexadecimal String must contain an even number of characters.");
            }

            byte[] result = new byte[hexString.Length / 2];

            for (int i = 0; i < hexString.Length; i += 2) {
                result[i / 2] = byte.Parse(hexString.Substring(i, 2), NumberStyles.HexNumber);
            }

            return result;
        }


    }
}
