using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace XeniumBt {

    internal class ModemHelper {

        private SerialPort serial;
        private readonly Config config;

        public ModemHelper(Config config) {
            this.config = config;
        }

        public void DoCommand(string command) {
            ExecAndReceive(command);
        }

        public string DoCommandWithResult(string command) {
            return ExecAndReceive(command);
        }

        private string ExecAndReceive(string command) {
            WriteLog("Calling : " + command);

            serial.Write(command + "\r\n");
            // Todo receive data wo sleep
            Thread.Sleep(config.Timeout);
            
            string reply = serial.ReadExisting();
            WriteLog(reply);
            string s = Convert.ToString(reply).Replace(command + "\r\n", "").Replace("OK\r\n", "");
            if (s.Contains("ERROR")) {
                throw new Exception("ERROR received");
            }

            return s;
        }

        public void Connect() {
            serial = new SerialPort {
                PortName = config.PortName,
                BaudRate = config.BaudRate,
            };

            serial.DataReceived += OnReceived;

            serial.Open();
        }

        private void OnReceived(object sender, SerialDataReceivedEventArgs e) {
            // Todo receive data wo sleep
            string a = e.ToString();
        }

        public void Disconnect() {

            if (serial != null) {
                serial.Close();
                serial = null;
            }
        }

        private void WriteLog(string message) {
            FileTools.WriteLog(config.LogFile, message);
        }
    }
}