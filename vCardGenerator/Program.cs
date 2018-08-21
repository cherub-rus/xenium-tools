using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

using Org.XeniumTools.Common;

namespace Org.XeniumTools.vCardGenerator {

    [SuppressMessage("ReSharper", "LocalizableElement")]
    public static class Program {

        static void Main(string[] args) {
            try {
                if (args.Length != 2) {
                    Console.WriteLine("Usage: vCardGenerator.exe sourceFile targetFile");
                    return;
                }
                String source = args[0];
                String target = args[1];

                if (!File.Exists(source)) {
                    Console.WriteLine("Source file does not exist");
                    return;
                }

                if (File.Exists(target)) {
                    Console.WriteLine("Target file already exist");
                    return;
                }

                string targetDir = Path.GetDirectoryName(Path.GetFullPath(target));
                if (targetDir!= null && !Directory.Exists(targetDir)) {
                    Console.WriteLine(targetDir);
                    Console.WriteLine("Target directory does not exist");
                    return;
                }

                doFix(source, target);
            } catch (Exception e) {
                Console.WriteLine(e);
            }

#if DEBUG
            Console.ReadLine();
#endif
        }

        private static void doFix (String source, String target) {
            string[] lines = File.ReadAllLines(source, Encoding.Default);

            List<CsvContact> contacts = new List<CsvContact>();
            foreach (CsvContact contact in CsvParser.Parse(lines)) {
                contacts.Add(FixContact(contact));
                PrintErrors(contact);
            }
            contacts.Sort();

            UTF8Encoding UTF8NoBOM = new UTF8Encoding(false, true);
            File.WriteAllText(target, Builder.BuildFileContent(contacts).ToString(), UTF8NoBOM);
        }

        private static void PrintErrors(CsvContact contact) {
            if (contact.Errors.Count <= 0) {
                return;
            }
            Console.WriteLine("Parse errors for contact \"" + contact.Name + "\":");
            foreach (string error in contact.Errors) {
                Console.WriteLine(error);
            }
        }

        private static CsvContact FixContact(CsvContact contact) {
            if (contact.Office.Number != null) {
                contact.Number.InitWhenNull();
                contact.Number2.InitWhenNull();
                contact.Home.InitWhenNull();
            } else if (contact.Home.Number != null) {
                contact.Number.InitWhenNull();
                contact.Number2.InitWhenNull();
            } else if (contact.Number2.Number != null) {
                contact.Number.InitWhenNull();
            }
            return contact;
        }
    }
}
