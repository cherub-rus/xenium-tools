using System.Collections.Generic;
using System.Text;

using Org.XeniumTools.Common;

namespace Org.XeniumTools.CsvFixer {
    
    public static class Builder {

        public static StringBuilder BuildFileContent(IList<CsvContact> contacts) {

            StringBuilder fileContent = new StringBuilder();
            fileContent.AppendLine(Resources.CsvHeader);
            foreach (CsvContact contact in contacts) {
                fileContent.Append(Resources.CsvRowPrefix).Append(',');
                fileContent.Append('"').Append(contact.Name).Append("\",");
                AppendNumber(fileContent, contact.Number);
                AppendNumber(fileContent, contact.Number2);
                AppendNumber(fileContent, contact.Home);
                AppendNumber(fileContent, contact.Office);
                fileContent.AppendLine();
            }
            return fileContent;
        }

        private static void AppendNumber(StringBuilder fileContent, CsvPhoneNumber number) {
            if (number.Number != null) {
                fileContent.Append(string.Format("\"{0}\",\"{1}\",", number.Type, number.Number));
            }
        }

    }
}
