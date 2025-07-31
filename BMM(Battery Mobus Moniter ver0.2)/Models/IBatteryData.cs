namespace BMM_Battery_Mobus_Moniter_.Models
{
    internal interface IBatteryData
    {
        double Voltage { get; }
        double Current { get; }
        double SOC { get; }
        double[] Temperatures { get; }
        double[] CellVoltages { get; }
        string FirmwareVersion { get; }

        /// <summary>
        /// 用 Modbus 讀取到的 ushort[] 寄存器資料解析成電池資料物件
        /// </summary>
        /// <param name="registers">Modbus 讀取到的寄存器陣列</param>
        void ParseFromRegisters(ushort[] registers);
    }
}
