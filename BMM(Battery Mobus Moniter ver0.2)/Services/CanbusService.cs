using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BMM_Battery_Mobus_Moniter_ver0._2_.Services
{
    // 1. ZLGCAN系列接口卡信息的数据类型。
    public struct VCI_BOARD_INFO
    {
        public UInt16 hw_Version;
        public UInt16 fw_Version;
        public UInt16 dr_Version;
        public UInt16 in_Version;
        public UInt16 irq_Num;
        public byte can_Num;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] str_Serial_Num;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        public byte[] str_hw_Type;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] Reserved;
    }

    // 2. 定义CAN信息帧的数据类型。
    unsafe public struct VCI_CAN_OBJ // 使用不安全代码
    {
        public uint ID;
        public uint TimeStamp;        // 时间标识
        public byte TimeFlag;         // 是否使用时间标识
        public byte SendType;         // 发送标志。保留，未用
        public byte RemoteFlag;       // 是否是远程帧
        public byte ExternFlag;       // 是否是扩展帧
        public byte DataLen;          // 数据长度
        public fixed byte Data[8];    // 数据
        public fixed byte Reserved[3];// 保留位
    }

    // 3. 定义初始化CAN的数据类型
    public struct VCI_INIT_CONFIG
    {
        public UInt32 AccCode;
        public UInt32 AccMask;
        public UInt32 Reserved;
        public byte Filter;   // 0或1接收所有帧。2标准帧滤波，3是扩展帧滤波。
        public byte Timing0;  // 波特率参数，具体配置，请查看二次开发库函数说明书。
        public byte Timing1;
        public byte Mode;     // 模式，0表示正常模式，1表示只听模式,2自测模式
    }

    // 4. USB-CAN总线适配器板卡信息的数据类型1，该类型为VCI_FindUsbDevice函数的返回参数。
    public struct VCI_BOARD_INFO1
    {
        public UInt16 hw_Version;
        public UInt16 fw_Version;
        public UInt16 dr_Version;
        public UInt16 in_Version;
        public UInt16 irq_Num;
        public byte can_Num;
        public byte Reserved;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] str_Serial_Num;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] str_hw_Type;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] str_Usb_Serial;
    }

    /// <summary>
    /// Canbus 通訊核心類別，負責設備操作、數據發送和接收。
    /// </summary>
    internal class CanbusService
    {
        // DllImport 函數聲明
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_OpenDevice(UInt32 DeviceType, UInt32 DeviceInd, UInt32 Reserved);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_CloseDevice(UInt32 DeviceType, UInt32 DeviceInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_InitCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_INIT_CONFIG pInitConfig);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ReadBoardInfo(UInt32 DeviceType, UInt32 DeviceInd, ref VCI_BOARD_INFO pInfo);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_GetReceiveNum(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ClearBuffer(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_StartCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);
        [DllImport("controlcan.dll")]
        public static extern UInt32 VCI_ResetCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_Transmit(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_CAN_OBJ pSend, UInt32 Len);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_Receive(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_CAN_OBJ pReceive, UInt32 Len, Int32 WaitTime);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ConnectDevice(UInt32 DevType, UInt32 DevIndex);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_UsbDeviceReset(UInt32 DevType, UInt32 DevIndex, UInt32 Reserved);
        [DllImport("controlcan.dll")]
        public static extern UInt32 VCI_FindUsbDevice(ref VCI_BOARD_INFO1 pInfo);

        private UInt32 m_devtype = 4; // USBCAN2
        public UInt32 DeviceIndex { get; set; } = 0; // 對應usb 裝置0-3
        public UInt32 CanIndex { get; set; } = 0; // 一個adapter有兩路，指定要開啟 adapter 上的第一路 (can1) 或第二路 (can2)
        public VCI_CAN_OBJ[] ReceiveObjects = new VCI_CAN_OBJ[1000];
        public VCI_BOARD_INFO1 UsbAdapterInfo = new VCI_BOARD_INFO1();

        public bool IsConnected { get; private set; } = false;
        private uint _commandDelayUs = 200 * 1000; // 預設 200ms 延遲 (us)

        /// <summary>
        /// 設定單一命令發送後的延遲時間 (微秒)。
        /// </summary>
        public uint CommandDelayUs
        {
            set { _commandDelayUs = value; }
            get { return _commandDelayUs; }
        }

        /// <summary>
        /// 尋找並返回所有連接的 Canbus 設備序列號。
        /// </summary>
        /// <returns>連接設備的序列號列表。</returns>
        public List<string> FindDeviceSerials()
        {
            uint rcvVal = VCI_FindUsbDevice(ref UsbAdapterInfo);
            List<string> rstList = new List<string>();
            if (rcvVal > 0)
            {
                string[] serialArr = ExtractAsciiSerials(UsbAdapterInfo.str_Usb_Serial);
                foreach (var str in serialArr)
                {
                    if (!string.IsNullOrWhiteSpace(str) && str != "\0\0\0\0") // 排除空字串或全空字元
                    {
                        rstList.Add(str);
                    }
                }
            }
            return rstList;
        }

        /// <summary>
        /// 從 byte 陣列中提取 ASCII 序列號。
        /// </summary>
        /// <param name="data">包含序列號的 byte 陣列。</param>
        /// <returns>序列號字串陣列。</returns>
        private string[] ExtractAsciiSerials(byte[] data)
        {
            if (data == null || data.Length != 16)
                throw new ArgumentException("Byte array must be exactly 16 bytes long.");

            string[] serials = new string[4];
            for (int i = 0; i < 4; i++)
            {
                serials[i] = Encoding.ASCII.GetString(data, i * 4, 4);
            }
            return serials;
        }

        /// <summary>
        /// 開啟並初始化 Canbus 設備。
        /// </summary>
        /// <returns>如果成功開啟並初始化則為 true，否則為 false。</returns>
        public bool OpenDevice()
        {
            try
            {
                if (VCI_OpenDevice(m_devtype, DeviceIndex, 0) == 0)
                {
                    return false;
                }

                VCI_INIT_CONFIG config = new VCI_INIT_CONFIG
                {
                    AccCode = 0x00000000,
                    AccMask = 0xFFFFFFFF,
                    Timing0 = 0x00, // 根據波特率設定，例如 1M bps: 0x00, 0x1C
                    Timing1 = 0x1C,
                    Filter = 1,   // 1 接收全部類型
                    Mode = 0      // 0 正常模式
                };

                if (VCI_InitCAN(m_devtype, DeviceIndex, CanIndex, ref config) == 0)
                {
                    VCI_CloseDevice(m_devtype, DeviceIndex);
                    return false;
                }

                if (VCI_ResetCAN(m_devtype, DeviceIndex, CanIndex) == 0)
                {
                    VCI_CloseDevice(m_devtype, DeviceIndex);
                    return false;
                }

                if (VCI_StartCAN(m_devtype, DeviceIndex, CanIndex) == 0)
                {
                    VCI_CloseDevice(m_devtype, DeviceIndex);
                    return false;
                }

                IsConnected = true;
                Thread.Sleep(100); // 給設備一點時間啟動
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CanbusCommunicator OpenDevice Error: {ex.Message}");
                CloseDevice(); // 確保在錯誤時關閉設備
                return false;
            }
        }

        /// <summary>
        /// 關閉 Canbus 設備。
        /// </summary>
        /// <returns>如果成功關閉則為 true，否則為 false。</returns>
        public bool CloseDevice()
        {
            try
            {
                if (IsConnected)
                {
                    VCI_CloseDevice(m_devtype, DeviceIndex);
                }
                IsConnected = false;
                Thread.Sleep(100); // 給設備一點時間關閉
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CanbusCommunicator CloseDevice Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 發送單一 CAN 命令，不等待接收。
        /// </summary>
        /// <param name="canId">CAN ID (十六進制字串，例如 "0x700")。</param>
        /// <param name="data">要發送的數據 (byte 陣列，最多 8 個位元組)。</param>
        /// <returns>如果發送成功則為 true，否則為 false。</returns>
        unsafe public bool SendCommand(string canId, byte[] data = null)
        {
            if (!IsConnected)
            {
                Console.WriteLine("Canbus not connected. Cannot send command.");
                return false;
            }

            try
            {
                VCI_CAN_OBJ sendobj = new VCI_CAN_OBJ
                {
                    RemoteFlag = 0, // 0: 數據幀, 1: 遠程幀
                    ExternFlag = 1, // 0: 標準幀, 1: 擴展幀
                    ID = Convert.ToUInt32(canId, 16),
                    DataLen = (byte)(data?.Length ?? 0)
                };

                if (data != null && data.Length > 0)
                {
                    for (int i = 0; i < data.Length && i < 8; i++)
                    {
                        sendobj.Data[i] = data[i];
                    }
                }

                if (VCI_Transmit(m_devtype, DeviceIndex, CanIndex, ref sendobj, 1) == 0)
                {
                    Console.WriteLine($"Failed to send command: {canId}");
                    return false;
                }

                // 延遲以避免過快發送導致設備來不及處理
                Thread.Sleep((int)(_commandDelayUs / 1000));
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CanbusCommunicator SendCommand Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 接收 CAN 數據。
        /// </summary>
        /// <param name="waitTimeMs">等待接收數據的時間 (毫秒)。</param>
        /// <returns>一個字典，鍵為 CAN ID (字串)，值為接收到的數據列表 (short)。</returns>
        unsafe public ConcurrentDictionary<string, List<short>> ReceiveData(int waitTimeMs = 100)
        {
            if (!IsConnected)
            {
                Console.WriteLine("Canbus not connected. Cannot receive data.");
                return null;
            }

            ConcurrentDictionary<string, List<short>> resultDict = new ConcurrentDictionary<string, List<short>>();
            UInt32 res = 0;
            Stopwatch stopwatch = Stopwatch.StartNew();

            // 持續接收直到沒有數據或超時
            while (stopwatch.ElapsedMilliseconds < waitTimeMs)
            {
                res = VCI_Receive(m_devtype, DeviceIndex, CanIndex, ref ReceiveObjects[0], (UInt32)ReceiveObjects.Length, 0); // WaitTime 設為 0，非阻塞式讀取

                if (res == 0xFFFFFFFF) // 設備未初始化或錯誤
                {
                    Console.WriteLine("Canbus ReceiveData Error: Device not initialized or error.");
                    stopwatch.Stop();
                    return null;
                }

                if (res > 0)
                {
                    for (UInt32 i = 0; i < res; i++)
                    {
                        VCI_CAN_OBJ currentObj = ReceiveObjects[i];
                        string frameRcvStrId = Convert.ToString(currentObj.ID, 16).ToUpper(); // 轉換為大寫十六進制字串

                        // 避免重複添加相同的 CAN ID
                        if (resultDict.ContainsKey(frameRcvStrId))
                        {
                            continue;
                        }

                        List<short> dataList = new List<short>();
                        // 將 CAN ID 作為第一個元素 (可選，根據實際需求決定是否需要)
                        // dataList.Add((short)currentObj.ID); 

                        if (currentObj.RemoteFlag == 0) // 數據幀
                        {
                            for (int j = 0; j < currentObj.DataLen; j++)
                            {
                                dataList.Add(currentObj.Data[j]);
                            }
                        }
                        resultDict.TryAdd(frameRcvStrId, dataList);
                    }
                }
                else
                {
                    // 如果沒有收到數據，稍微延遲一下再嘗試，避免 CPU 佔用過高
                    Thread.Sleep(1);
                }
            }
            stopwatch.Stop();
            return resultDict;
        }

        /// <summary>
        /// 清除接收緩衝區。
        /// </summary>
        public void ClearReceiveBuffer()
        {
            if (IsConnected)
            {
                VCI_ClearBuffer(m_devtype, DeviceIndex, CanIndex);
            }
        }
    }
}
