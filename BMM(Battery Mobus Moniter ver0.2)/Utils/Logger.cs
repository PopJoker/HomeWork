using BMM_Battery_Mobus_Moniter_.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BMM_Battery_Mobus_Moniter_ver0._2_.Utils
{
    public class Logger
    {
        private readonly object _lock = new object();
        private string _folderPath;
        private string _comPort;
        private string _batteryId;
        private string _customFileName;
        private string _logFilePath;

        private int _cellCount;
        private int _tempCount;

        public Logger(string folderPath, string comPort, string batteryId, int cellCount, int tempCount, string customFileName = null)
        {
            _folderPath = string.IsNullOrWhiteSpace(folderPath) ? AppDomain.CurrentDomain.BaseDirectory : folderPath;
            _comPort = comPort;
            _batteryId = batteryId;
            _cellCount = cellCount;
            _tempCount = tempCount;
            _customFileName = customFileName;

            Directory.CreateDirectory(_folderPath);

            string fileName = !string.IsNullOrWhiteSpace(_customFileName)
                ? _customFileName
                : $"{_batteryId}_{_comPort}_{DateTime.Now:yyyyMMdd}.csv";

            _logFilePath = Path.Combine(_folderPath, fileName);

            if (!File.Exists(_logFilePath))
            {
                WriteLine(GetCsvHeader());
            }
        }

        private string GetCsvHeader()
        {
            var headers = new List<string>()
    {
        "Timestamp","Voltage(mV)","Current(mA)","SOC(%)","MaxCell(mV)","MinCell(mV)","DeltaCell(mV)",
        "LoStatus","HiStatus","FirmwareVersion","RemainCp","FCC","SoH"
    };

            for (int i = 0; i < _cellCount; i++)
            {
                headers.Add($"CellVoltage{i + 1}(mV)");
            }

            for (int i = 0; i < _tempCount; i++)
            {
                headers.Add($"Temperature{i + 1}(°C)");
            }

            return string.Join(",", headers);
        }


        public void WriteLine(string line)
        {
            lock (_lock)
            {
                File.AppendAllText(_logFilePath, line + Environment.NewLine, Encoding.UTF8);
            }
        }

        public void LogBatteryData(Battery22S2P battery)
        {
            string cellVoltagesStr = string.Join(";", battery.CellVoltages.Select(v => v.ToString("F1")));
            string temperaturesStr = string.Join(";", battery.Temperatures.Select(t => t.ToString("F1")));

            double maxCell = battery.CellVoltages.Max();
            double minCell = battery.CellVoltages.Min();
            double deltaCell = maxCell - minCell;

            string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}," +
               $"{battery.Voltage:F1}," +
               $"{battery.Current:F1}," +
               $"{battery.SOC:F1}," +
               $"{maxCell:F1}," +
               $"{minCell:F1}," +
               $"{deltaCell:F1}," +
               $"{battery.LoStatus}," +
               $"{battery.HiStatus}," +
               $"{battery.FirmwareVersion}," +
               $"{battery.RemainCp:F1}," +
               $"{battery.FullChargeCp:F1}," +
               $"{battery.SoH:F1}";

            // 把所有 cell voltages 分開欄位加到 line 後面
            foreach (var v in battery.CellVoltages)
            {
                line += $",{v:F1}";
            }

            // 把所有 temperatures 分開欄位加到 line 後面
            foreach (var t in battery.Temperatures)
            {
                line += $",{t:F1}";
            }

            WriteLine(line);

        }

        public void ChangeLogFilePath(string newFolderPath, string newFileName = null)
        {
            _folderPath = string.IsNullOrWhiteSpace(newFolderPath) ? AppDomain.CurrentDomain.BaseDirectory : newFolderPath;
            Directory.CreateDirectory(_folderPath);

            string fileName = !string.IsNullOrWhiteSpace(newFileName)
                ? newFileName
                : Path.GetFileName(_logFilePath);

            _logFilePath = Path.Combine(_folderPath, fileName);

            if (!File.Exists(_logFilePath))
            {
                WriteLine(GetCsvHeader());
            }
        }


    }
}
