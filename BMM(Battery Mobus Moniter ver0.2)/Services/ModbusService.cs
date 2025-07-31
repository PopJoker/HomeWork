using System;
using System.IO.Ports;
using Modbus.Device;

namespace MM_ModbusMonitor
{
    public class ModbusService
    {
        private SerialPort _serialPort;
        public IModbusSerialMaster _master;

        public bool IsConnected { get; private set; }

        public bool Connect(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            try
            {
                _serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
                _serialPort.Open();
                _master = ModbusSerialMaster.CreateRtu(_serialPort);
                _master.Transport.ReadTimeout = 1000;  // 最多等 1 秒
                _master.Transport.WriteTimeout = 1000;
                IsConnected = true;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ModbusService Connect Error: " + ex.Message);
                Disconnect();
                return false;
            }
        }

        public void Disconnect()
        {
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                    _serialPort.Close();

                if (_serialPort != null)
                    _serialPort.Dispose();

                IsConnected = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ModbusService Disconnect Error: " + ex.Message);
            }
        }

        public ushort[] ReadHoldingRegisters(byte slaveId, ushort startAddress, ushort numRegisters)
        {
            if (!IsConnected || _master == null)
                throw new InvalidOperationException("Modbus not connected");

            try
            {
                return _master.ReadHoldingRegisters(slaveId, startAddress, numRegisters);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ModbusService Read Error: " + ex.Message);
                throw;
            }
        }
    }
}
