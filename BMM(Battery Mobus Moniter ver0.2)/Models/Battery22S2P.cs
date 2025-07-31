using BMM_Battery_Mobus_Moniter_.Models;
using BMM_Battery_Mobus_Moniter_ver0._2_.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BMM_Battery_Mobus_Moniter_.Models
{
    public class Battery22S2P : IBatteryData
    {
        public byte SlaveID { get; set; }
        //電壓差 電芯最高值 數字(CellVoltageNUM) 最低值
        private ushort _batteryLoVoltageRaw;
        private ushort _batteryHiVoltageRaw;


        public double Voltage
        {
            get
            {
                uint combined = ((uint)_batteryHiVoltageRaw << 16) | _batteryLoVoltageRaw;
                return combined; // 假設mV轉V
            }
        }   
        public double Current => BatteryCurrent;
        public double SOC => BatterySOC;

        // 基本參數
        public double BatteryLoVoltage { get; private set; } = 0;
        public double BatteryHiVoltage { get; private set; } = 0;
        public double BatteryCurrent { get; private set; } = 0;
        public double BatterySOC { get; private set; } = 0;

        // 韌體版本
        public string FirmwareVersion { get; private set; } = "";

        // 狀態寄存器
        public int LoStatus { get; private set; }
        public int HiStatus { get; private set; }

        // 溫度傳感器（6個）
        public double[] Temperatures { get; private set; } = new double[6];

        // 電芯電壓 (22個)
        public static int CellCount { get; private set; } = 22;
        public double[] CellVoltages { get; private set; } = new double[22];

        // 容量相關
        public double RemainCp { get; private set; } = 0;
        public double FullChargeCp { get; private set; } = 0;
        public double SoH { get; private set; } = 0;

        // 狀態旗標
        public bool LoOC { get; private set; }
        public bool LoDSG { get; private set; }
        public bool LoCHG { get; private set; }
        public bool LoUTD { get; private set; }
        public bool LoUTC { get; private set; }
        public bool LoOTD { get; private set; }
        public bool LoOTC { get; private set; }
        public bool LoASCDL { get; private set; }
        public bool LoASCD { get; private set; }
        public bool LoAOLDL { get; private set; }
        public bool LoAOLD { get; private set; }
        public bool LoOCD { get; private set; }
        public bool LoOCC { get; private set; }
        public bool LoCOV { get; private set; }
        public bool LoCUV { get; private set; }

        public bool HiOC { get; private set; }
        public bool HiUTD { get; private set; }
        public bool HiUTC { get; private set; }
        public bool HiOTD { get; private set; }
        public bool HiOTC { get; private set; }
        public bool HiCOV { get; private set; }
        public bool HiCUV { get; private set; }

        public Battery22S2P(int cellCount)
        {
            CellCount = cellCount;
        }

        public void ParseFromRegisters(ushort[] registers)
        {
            try
            {
                // 基本參數，寄存器 0 ~ 3
                _batteryLoVoltageRaw = registers.Length > 0 ? registers[0] : (ushort)0;
                _batteryHiVoltageRaw = registers.Length > 1 ? registers[1] : (ushort)0;
                BatteryCurrent = registers.Length > 2 ? (short)registers[2] : 0;
                BatterySOC = registers.Length > 3 ? (short)registers[3] : 0;

                // 狀態寄存器 4,5
                LoStatus = registers.Length > 4 ? registers[4] : 0;
                HiStatus = registers.Length > 5 ? registers[5] : 0;

                // 溫度 6-11
                for (int i = 0; i < 6; i++)
                {
                    if (registers.Length > 6 + i)
                        Temperatures[i] = (short)registers[6 + i] * 0.1;
                    else
                        Temperatures[i] = 0;
                }

                // 韌體版本 12
                FirmwareVersion = registers.Length > 12 ? ((short)registers[12] * 0.1).ToString("F1") : "";

                // 電芯電壓 13 ~ 34 (22個)
                for (int i = 0; i < 22; i++)
                {
                    int index = 13 + i;
                    if (registers.Length > index)
                        CellVoltages[i] = (short)registers[index];
                    else
                        CellVoltages[i] = 0;
                }

                // 容量相關 35~37
                RemainCp = registers.Length > 35 ? registers[35] * 10 : 0;
                FullChargeCp = registers.Length > 36 ? registers[36] * 10 : 0;
                SoH = registers.Length > 37 ? registers[37] : 0;

                // 狀態旗標解析
                ParseStatusFlags();
            }
            catch (Exception ex)
            {
                // 如果解析錯誤，重置或記錄
                FirmwareVersion = "Error";
                Console.WriteLine($"Battery22S2P 解析錯誤: {ex.Message}");
            }
        }

        private void ParseStatusFlags()
        {
            // 低位元狀態寄存器
            int lo = LoStatus;
            LoOC = ((lo >> 15) & 1) == 1;
            LoDSG = ((lo >> 13) & 1) == 1;
            LoCHG = ((lo >> 12) & 1) == 1;
            LoUTD = ((lo >> 11) & 1) == 1;
            LoUTC = ((lo >> 10) & 1) == 1;
            LoOTD = ((lo >> 9) & 1) == 1;
            LoOTC = ((lo >> 8) & 1) == 1;
            LoASCDL = ((lo >> 7) & 1) == 1;
            LoASCD = ((lo >> 6) & 1) == 1;
            LoAOLDL = ((lo >> 5) & 1) == 1;
            LoAOLD = ((lo >> 4) & 1) == 1;
            LoOCD = ((lo >> 3) & 1) == 1;
            LoOCC = ((lo >> 2) & 1) == 1;
            LoCOV = ((lo >> 1) & 1) == 1;
            LoCUV = (lo & 1) == 1;

            // 高位元狀態寄存器
            int hi = HiStatus;
            HiOC = ((hi >> 15) & 1) == 1;
            HiUTD = ((hi >> 11) & 1) == 1;
            HiUTC = ((hi >> 10) & 1) == 1;
            HiOTD = ((hi >> 9) & 1) == 1;
            HiOTC = ((hi >> 8) & 1) == 1;
            HiCOV = ((hi >> 1) & 1) == 1;
            HiCUV = (hi & 1) == 1;
        }

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            int safeCount = Math.Min(CellCount, CellVoltages.Length);
            double maxVal = CellVoltages.Take(safeCount).Max();

            int maxIndex = CellVoltages
                .Take(safeCount)
                .Select((val, idx) => new { val, idx })
                .First(x => x.val == maxVal)
                .idx + 1; // 從1開始算


            double minVal = CellVoltages.Take(safeCount).Min();
            int minIndex = CellVoltages
                .Take(safeCount)
                .Select((val, idx) => new { val, idx })
                .First(x => x.val == minVal)
                .idx + 1;


            sb.AppendLine($"Voltage: {Voltage} mV");
            sb.AppendLine($"Current: {Current*10} mA");

            sb.AppendLine($"MaxCell: {maxVal} mV({maxIndex})");
            sb.AppendLine($"MinCell: {minVal} mV ({minIndex})");
            sb.AppendLine($"DeltaCell: {maxVal - minVal} mV");

            double maxTemp = Temperatures.Max();
            int maxTempIndex = Temperatures
                .Select((val, idx) => new { val, idx })
                .First(x => x.val == maxTemp)
                .idx + 1;

            sb.AppendLine($"MaxTemp: {maxTemp} °C({maxTempIndex})");

            sb.AppendLine($"SOC: {SOC} %");


            sb.AppendLine($"Firmware Version: {FirmwareVersion}");
            sb.AppendLine($"RC: {RemainCp} mAh");
            sb.AppendLine($"FCC: {FullChargeCp} mAh");
            sb.AppendLine($"SoH: {SoH} %");

            // Lo-Status
            sb.Append("Lo-Status: ");
            List<string> loFlags = new List<string>();
            if (LoOC) loFlags.Add("OC");
            if (LoDSG) loFlags.Add("DSG");
            if (LoCHG) loFlags.Add("CHG");
            if (LoUTD) loFlags.Add("UTD");
            if (LoUTC) loFlags.Add("UTC");
            if (LoOTD) loFlags.Add("OTD");
            if (LoOTC) loFlags.Add("OTC");
            if (LoASCDL) loFlags.Add("ASCDL");
            if (LoASCD) loFlags.Add("ASCD");
            if (LoAOLDL) loFlags.Add("AOLDL");
            if (LoAOLD) loFlags.Add("AOLD");
            if (LoOCD) loFlags.Add("OCD");
            if (LoOCC) loFlags.Add("OCC");
            if (LoCOV) loFlags.Add("COV");
            if (LoCUV) loFlags.Add("CUV");
            sb.AppendLine(loFlags.Count > 0 ? string.Join(", ", loFlags) : "None");

            // Hi-Status
            sb.Append("Hi-Status: ");
            List<string> hiFlags = new List<string>();
            if (HiOC) hiFlags.Add("OC");
            if (HiUTD) hiFlags.Add("UTD");
            if (HiUTC) hiFlags.Add("UTC");
            if (HiOTD) hiFlags.Add("OTD");
            if (HiOTC) hiFlags.Add("OTC");
            if (HiCOV) hiFlags.Add("COV");
            if (HiCUV) hiFlags.Add("CUV");
            sb.AppendLine(hiFlags.Count > 0 ? string.Join(", ", hiFlags) : "None");


            // 多行換行，用 \r\n 連續換行，避免 RichTextBox 換行問題
            sb.Append(string.Concat(System.Linq.Enumerable.Repeat("\r\n", 15)));

            sb.AppendLine("--- Cell Voltages (mV) ---");
            for (int i = 0; i < CellCount; i++)
            {
                sb.AppendLine($"CellVoltage{i + 1}: {CellVoltages[i]}");
            }
            sb.Append(string.Concat(System.Linq.Enumerable.Repeat("\r\n", 22 - CellCount + 5)));

            sb.AppendLine("--- Temperatures (°C) ---");
            for (int i = 0; i < Temperatures.Length; i++)
            {
                sb.AppendLine($"Temperatures{i + 1}: {Temperatures[i]:F1}");
            }


            return sb.ToString();
        }



    }
}
