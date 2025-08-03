using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BMSHostMonitor
{
  //1.ZLGCAN系列接口卡信息的数据类型。
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

  /////////////////////////////////////////////////////
  //2.定义CAN信息帧的数据类型。
  unsafe public struct VCI_CAN_OBJ //使用不安全代码
  {
    public uint ID;
    public uint TimeStamp;        //时间标识
    public byte TimeFlag;         //是否使用时间标识
    public byte SendType;         //发送标志。保留，未用
    public byte RemoteFlag;       //是否是远程帧
    public byte ExternFlag;       //是否是扩展帧
    public byte DataLen;          //数据长度
    public fixed byte Data[8];    //数据
    public fixed byte Reserved[3];//保留位

  }

  //3.定义初始化CAN的数据类型
  public struct VCI_INIT_CONFIG
  {
    public UInt32 AccCode;
    public UInt32 AccMask;
    public UInt32 Reserved;
    public byte Filter;   //0或1接收所有帧。2标准帧滤波，3是扩展帧滤波。
    public byte Timing0;  //波特率参数，具体配置，请查看二次开发库函数说明书。
    public byte Timing1;
    public byte Mode;     //模式，0表示正常模式，1表示只听模式,2自测模式
  }

  /*------------其他数据结构描述---------------------------------*/
  //4.USB-CAN总线适配器板卡信息的数据类型1，该类型为VCI_FindUsbDevice函数的返回参数。
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

  /*------------数据结构描述完成---------------------------------*/

  public struct CHGDESIPANDPORT
  {
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public byte[] szpwd;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    public byte[] szdesip;
    public Int32 desport;

    public void Init()
    {
      szpwd = new byte[10];
      szdesip = new byte[20];
    }
  }

  public class canbusMainStructure
  {
    // 0建構式，先預設要建立8個黑色電池盒
    public canbusMainStructure()
    {
      _TotalPackNum = 10;
      _TotalMasterData = new ConcurrentDictionary<int, BatteryCanbus2P5SPara>();
      for (int index = 1; index <= _TotalPackNum; index++)
      {
        _TotalMasterData.TryAdd(index, new BatteryCanbus2P5SPara());
        TotalMasterDataChayi.TryAdd(index, new BatteryCanbusNew2P22SPBmsPara());

      }
    }

    private int _TotalPackNum;
    private ConcurrentDictionary<int, BatteryCanbus2P5SPara> _TotalMasterData;

    private ConcurrentDictionary<string, BatteryCanbus2P28SPara> _TotalMasterData28;


    public List<int> idList = new List<int>();

    // 1參數的建構式，看參數幾個就建立幾個
    public canbusMainStructure(int packNum)
    {
      _TotalPackNum = packNum;
      _TotalMasterData = new ConcurrentDictionary<int, BatteryCanbus2P5SPara>();
      for (int index = 1; index <= _TotalPackNum; index++)
      {
        _TotalMasterData.TryAdd(index, new BatteryCanbus2P5SPara());
        TotalMasterDataChayi.TryAdd(index, new BatteryCanbusNew2P22SPBmsPara());
      }
    }

    // 動態建立總共幾個PACK
    public int TotalPackNum
    {
      get { return _TotalPackNum; }
      set { _TotalPackNum = value; }
    }

    // 動態建立總共幾個 Pack Information
    public ConcurrentDictionary<int, BatteryCanbus2P5SPara> TotalMasterData
    {
      get { return _TotalMasterData; }
      set { _TotalMasterData = value; }
    }

    public ConcurrentDictionary<string, BatteryCanbus2P28SPara> TotalMasterData28
    {
      get { return _TotalMasterData28; }
      set { _TotalMasterData28 = value; }
    }


    // 新增中油嘉義的pack information
    public ConcurrentDictionary<int, BatteryCanbusNew2P22SPBmsPara> TotalMasterDataChayi = new ConcurrentDictionary<int, BatteryCanbusNew2P22SPBmsPara>();

    // 中油嘉義的Rack information
    public BatteryCanbusNew2P22SPBcuPara RackDataChayiBCU = new BatteryCanbusNew2P22SPBcuPara();

    /// <summary>
    /// 串併後的總電壓
    /// </summary>
    public Int32 TotalVoltage { get; set; } = 0;

    /// <summary>
    /// 標稱總電壓
    /// </summary>
    public double NorminalTotalVoltage { get; set; } = 0;

    /// <summary>
    /// 標稱總電流
    /// </summary>
    public double NorminalTotalCurrent { get; set; } = 0;

    /// <summary>
    /// 標稱總容量
    /// </summary>
    public double NorminalTotalCapacity { get; set; } = 0;

    /// <summary>
    /// 標稱最大功率
    /// </summary>
    public double NorminalTotalPower { get; set; } = 0;

    /// <summary>
    /// 串併後的總電流
    /// </summary>
    public double TotalCurrent { get; set; } = 0;

    /// <summary>
    /// 串併後的總容量
    /// </summary>
    public Int32 TotalCapacity { get; set; } = 0;

    /// <summary>
    /// 串併後的總功率
    /// </summary>
    public double TotalPower { get; set; } = 0;

    /// <summary>
    /// 存放canbus Master模式的全部變數 
    /// </summary>
    //Dictionary<int, BatteryPara> _TotalMasterData = new Dictionary<int, BatteryPara>();

    /*------------兼容ZLG的函数描述---------------------------------*/
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


    /*------------------------------------其他函数描述---------------------------------*/
    [DllImport("controlcan.dll")]
    static extern UInt32 VCI_ConnectDevice(UInt32 DevType, UInt32 DevIndex);

    [DllImport("controlcan.dll")]
    static extern UInt32 VCI_UsbDeviceReset(UInt32 DevType, UInt32 DevIndex, UInt32 Reserved);

    [DllImport("controlcan.dll")]
    public static extern UInt32 VCI_FindUsbDevice(ref VCI_BOARD_INFO1 pInfo);
    
    /*------------------------------------函数描述结束---------------------------------*/


    private UInt32 m_devtype = 4; // USBCAN2
    public UInt32 m_devind { get; set; } = 0; // 對應usb 裝置0-3，想確認序號對應的是幾要執行find usb
    public UInt32 m_canind { get; set; } = 0; //一個adapter有兩路，指定要開啟 adapter 上的第一路 (can1) 或第二路 (can2)
    public VCI_CAN_OBJ[] m_recobj = new VCI_CAN_OBJ[1000];
    public VCI_BOARD_INFO1 usbAdapterInfo = new VCI_BOARD_INFO1();
    public UInt32[] m_arrdevtype = new UInt32[20];

    /// <summary>
    /// us delay class
    /// </summary>
    public class TTimeObj
    {
      private long rStart;
      private long rEnd;
      private long rFreq;
      private long rPeriod;

      [DllImport("kernel32.dll ")]
      static extern bool QueryPerformanceCounter(ref long lpPerformanceCount);

      [DllImport("kernel32")]
      static extern bool QueryPerformanceFrequency(ref long PerformanceFrequency);

      //public STimePara TP = new STimePara();

      /// <summary>
      /// 輸入想要延遲多少us
      /// </summary>
      /// <param name="us"></param>
      /// <returns></returns>
      public bool TimeDelayus(long us)
      {
        //取得現在 頻率/秒
        QueryPerformanceFrequency(ref rFreq);
        //取得高精度計數器現在值
        QueryPerformanceCounter(ref rStart);
        rPeriod = rStart;
        //計算延遲值
        rEnd = (us * rFreq) / 1000000;
        // 進入迴圈前取得延遲結束計數值
        rEnd += rStart;
        //延遲開始
        do
        {
          // 取得目前計數值
          QueryPerformanceCounter(ref rStart);
        } while (rStart <= rEnd);
        return true;
      }

      /// <summary>
      /// 跟stopwatch用法相同，參考CPU時脈頻率運算，準度可達us
      /// </summary>
      public class TimeNow
      {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(
            out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(
            out long lpFrequency);

        private long startTime, stopTime;
        private long freq;

        /// <summary>
        /// 物件建立就載入應用
        /// </summary>
        public TimeNow()
        {
          startTime = 0;
          stopTime = 0;

          if (QueryPerformanceFrequency(out freq) == false)
          {
            throw new Exception("Timer not supported.");
          }
        }
        /// <summary>
        /// 開始時間測量
        /// </summary>
        public void Start()
        {
          Thread.Sleep(0);
          QueryPerformanceCounter(out startTime);
        }

        /// <summary>
        /// 結束測量
        /// </summary>
        public void Stop()
        {
          QueryPerformanceCounter(out stopTime);
        }

        /// <summary>
        /// stop-start的時間(s)
        /// </summary>
        public double Duration
        {
          get
          {
            return (double)(stopTime - startTime) / (double)freq;
          }
        }
        /// <summary>
        /// stop-start的時間(ms)
        /// </summary>
        public double msDuration
        {
          get
          {
            double val = (double)(stopTime - startTime) / (double)freq;
            return Math.Round(val * 1000, 0);
          }
        }
        /// <summary>
        /// stop-start的時間(us)
        /// </summary>
        public double usDuration
        {
          get
          {
            double val = (double)(stopTime - startTime) / (double)freq;
            return Math.Round(val * 1000000, 0);
          }
        }

      }

    }
    public TTimeObj usdelay = new TTimeObj();

    public List<string> findDeviceSerial()
    {
      // 先檢查有幾個CANBUS USB裝置
      uint rcvVal = VCI_FindUsbDevice(ref usbAdapterInfo);
      List<string> rstList = new List<string>();
      if (rcvVal > 0)
      {
        // 67 65 78 45 67 53 68 51
        string[] serialArr = ExtractAsciiSerials(usbAdapterInfo.str_Usb_Serial);
        //rstList = serialArr.ToList();
        foreach(var str in serialArr)
        {
          //if(str!= "\0\0\0\0")
           rstList.Add(str); 
        }
      }
      uint rcvVal2 = VCI_FindUsbDevice(ref usbAdapterInfo);
      return rstList;

    }

    // 開啟要對哪一個CANBUS DEVICE 通訊
    public bool startDevice()
    {
      try
      {
        if (VCI_OpenDevice(m_devtype, m_devind, 0) == 0)
        {
          return false;
        }
        // 初始化裝置設定值
        VCI_INIT_CONFIG config = new VCI_INIT_CONFIG();
        config.AccCode = System.Convert.ToUInt32("0x00000000", 16);
        config.AccMask = System.Convert.ToUInt32("0xFFFFFFFF", 16);
        config.Timing0 = System.Convert.ToByte("0x00", 16);
        config.Timing1 = System.Convert.ToByte("0x1C", 16);
        config.Filter = (Byte)(1); // 1 接收全部類型 2 只接收標準frame 3 只接收Extended
        config.Mode = (Byte)(0); // 0 正常  1 只接收  2 自測
        VCI_InitCAN(m_devtype, m_devind, m_canind, ref config);
        VCI_ResetCAN(m_devtype, m_devind, m_canind);

        // canbus 啟動通訊
        VCI_StartCAN(m_devtype, m_devind, m_canind);
        swConn = true;
        Thread.Sleep(100);
        return true;
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    public bool closeDevice()
    {
      try
      {
        VCI_CloseDevice(m_devtype, m_devind);
        swConn = false;
        Thread.Sleep(100);
        return true;
      }
      catch (Exception ex)
      {
        return false;
      }
    }


    public string[] ExtractAsciiSerials(byte[] data)
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

    public bool CheckDevice()
    {
      try
      {
        //List<string> serialList = findDeviceSerial();

        uint rcvVal = VCI_FindUsbDevice(ref usbAdapterInfo);
        if (rcvVal > 0)
        {
          // 67 65 78 45 67 53 68 51
          //string[] serialArr = ExtractAsciiSerials(usbAdapterInfo.str_Usb_Serial);
          //string[] hwTypeArr = ExtractAsciiSerials(usbAdapterInfo.str_hw_Type);


          return true;
        }
        else
        {
          return false;
        }
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    //是否連線中
    public bool swConn = false;



    // 連續發送失敗系統異常
    public int errSendCount = 0;

    public string canbusMode { get; set; } = "ID_Slave"; // 預設 SLAVE 模式

    /// <summary>
    /// 單一命令傳送收值(不受pack參數改變，獨立使用)
    /// </summary>
    /// <param name="cmdRequest"></param>
    /// <returns></returns>
    unsafe public Dictionary<int, List<short>> singleCmd(string cmdRequest)
    {
      try
      {
        // 回傳碼Str
        //string feedbackFst = "";
        // 寫入測試
        VCI_CAN_OBJ sendobj = new VCI_CAN_OBJ();
        sendobj.RemoteFlag = (byte)0; // 0 data  1 remote
        sendobj.ExternFlag = (byte)1; // 0 標準frame  1 extended
                                      //string cmdRequest = "0x0700";
        sendobj.ID = System.Convert.ToUInt32(cmdRequest, 16); // request Command
        int length = 0;
        sendobj.DataLen = System.Convert.ToByte(length); // 純讀取模式
        if (VCI_Transmit(m_devtype, m_devind, m_canind, ref sendobj, 1) == 0)
        {
          errSendCount++;
        }
        usdelay.TimeDelayus(80000);
        //Thread.Sleep(50);

        UInt32 res = 0;

        int waitCount = 100; // 1秒
                             // 每100ms讀取一次,waitCount=0則跳出
                             // 當串接的時候有可能會串接兩個以上,最多8個PACK包
                             //UInt32 getStrNum = VCI_GetReceiveNum(m_devtype, m_devind, m_canind);

        Stopwatch t1 = new Stopwatch();
        t1.Start();
        while (res == 0 && waitCount != 0) // 只要收到res>0 或是waitcount==0 都會跳出
        {
          res = VCI_Receive(m_devtype, m_devind, m_canind, ref m_recobj[0], 1000, 100);
          usdelay.TimeDelayus(1000);
          //Thread.Sleep(1);
          waitCount--;
        }
        t1.Stop();
        string debugStr = t1.ElapsedMilliseconds.ToString();

        if (res == 0xFFFFFFFF) res = 0;//当设备未初始化时，返回0xFFFFFFFF，不进行列表显示。
        String str = "";

        List<short> resultList = new List<short>();
        Dictionary<int, List<short>> ResultDict = new Dictionary<int, List<short>>();
        string frameRcvStrId = "";

        int orgSendIdNum = Convert.ToInt32(cmdRequest.Replace("0x", ""));
        int rcvIdNum = orgSendIdNum;
        for (UInt32 i = 0; i < res; i++)
        {
          //VCI_CAN_OBJ obj = (VCI_CAN_OBJ)Marshal.PtrToStructure((IntPtr)((UInt32)pt + i * Marshal.SizeOf(typeof(VCI_CAN_OBJ))), typeof(VCI_CAN_OBJ));
          resultList = new List<short>();
          str = "接收到数据: ";
          str += "  帧ID:0x" + Convert.ToString(m_recobj[i].ID, 16);
          frameRcvStrId = Convert.ToString(m_recobj[i].ID, 16);
          rcvIdNum = Convert.ToInt32(frameRcvStrId);
          int SlaveID = rcvIdNum - orgSendIdNum;
          str += "  帧格式:";
          if (m_recobj[i].RemoteFlag == 0)
            str += "数据帧 ";
          else
            str += "远程帧 ";
          if (m_recobj[i].ExternFlag == 0)
            str += "标准帧 ";
          else
            str += "扩展帧 ";
          Int32 tmpVal = Convert.ToInt32(m_recobj[i].ID);

          resultList.Add((short)tmpVal);
          if (m_recobj[i].RemoteFlag == 0)
          {
            str += "数据: ";
            byte len = (byte)(m_recobj[i].DataLen % 9);
            byte j = 0;
            fixed (VCI_CAN_OBJ* m_recobj1 = &m_recobj[i])
            {
              if (j++ < len)
                str += " " + System.Convert.ToString(m_recobj1->Data[0], 16);
              if (j++ < len)
                str += " " + System.Convert.ToString(m_recobj1->Data[1], 16);
              if (j++ < len)
                str += " " + System.Convert.ToString(m_recobj1->Data[2], 16);
              if (j++ < len)
                str += " " + System.Convert.ToString(m_recobj1->Data[3], 16);
              if (j++ < len)
                str += " " + System.Convert.ToString(m_recobj1->Data[4], 16);
              if (j++ < len)
                str += " " + System.Convert.ToString(m_recobj1->Data[5], 16);
              if (j++ < len)
                str += " " + System.Convert.ToString(m_recobj1->Data[6], 16);
              if (j++ < len)
                str += " " + System.Convert.ToString(m_recobj1->Data[7], 16);

              resultList.Add(m_recobj1->Data[0]);
              resultList.Add(m_recobj1->Data[1]);
              resultList.Add(m_recobj1->Data[2]);
              resultList.Add(m_recobj1->Data[3]);
              resultList.Add(m_recobj1->Data[4]);
              resultList.Add(m_recobj1->Data[5]);
              resultList.Add(m_recobj1->Data[6]);
              resultList.Add(m_recobj1->Data[7]);
            }
          }
          ResultDict.Add(SlaveID, resultList);
        }
        return ResultDict;
      }
      catch (Exception ex)
      {
        return null;
      }


    }

    /// <summary>
    /// 重新撰寫發送命令，不等待接收
    /// </summary>
    /// <param name="cmdRequest"></param>
    /// <returns></returns>
    public bool singleCmdNowait(string cmdRequest)
    {
      try
      {
        // 寫入測試
        VCI_CAN_OBJ sendobj = new VCI_CAN_OBJ();
        sendobj.RemoteFlag = (byte)0; // 0 data  1 remote
        sendobj.ExternFlag = (byte)1; // 0 標準frame  1 extended
                                      //string cmdRequest = "0x0700";
        sendobj.ID = System.Convert.ToUInt32(cmdRequest, 16); // request Command
        int length = 0;
        sendobj.DataLen = System.Convert.ToByte(length); // 純讀取模式
        if (VCI_Transmit(m_devtype, m_devind, m_canind, ref sendobj, 1) == 0)
        {
          errSendCount++;
        }
        usdelay.TimeDelayus(_cmdDelay * 1000);
        //Thread.Sleep(800);
        return true;
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    unsafe public ConcurrentDictionary<int, List<short>> RcvData()
    {
      try
      {
        UInt32 res = 0;
        int waitCount = 100; // 每100ms讀取一次,waitCount=0則跳出
                             // 當串接的時候有可能會串接兩個以上,最多8個PACK包
                             //UInt32 getStrNum = VCI_GetReceiveNum(m_devtype, m_devind, m_canind);

        Stopwatch t1 = new Stopwatch();
        t1.Start();
        while (res == 0 && waitCount != 0) // 只要收到res>0 或是waitcount==0 都會跳出
        {
          res = VCI_Receive(m_devtype, m_devind, m_canind, ref m_recobj[0], 1000, 100);
          usdelay.TimeDelayus(10000);
          //Thread.Sleep(1);
          waitCount--;
        }
        t1.Stop();
        string debugStr = t1.ElapsedMilliseconds.ToString();

        if (res == 0xFFFFFFFF) res = 0;//当设备未初始化时，返回0xFFFFFFFF，不进行列表显示。
        String str = "";

        List<short> resultList = new List<short>();
        ConcurrentDictionary<int, List<short>> ResultDict = new ConcurrentDictionary<int, List<short>>();
        string frameRcvStrId = "";
        int rcvIdNum = 0;
        int SlaveID = 0;


        // 重新建立資料結構
        // ResultDict
        // KEY =>  frameRcvStrId (這個數字包含了ID站名以及命令代號)，之後再去解析
        // VALUE => 8bit 的List

        // 由於丟的速度太快收的慢，可能導致重複數據擷取
        // 這樣會讓 ResultDict 新增相同KEY，造成崩潰所以遇到重複就跳出不建立
        for (UInt32 i = 0; i < res; i++)
        {
          //VCI_CAN_OBJ obj = (VCI_CAN_OBJ)Marshal.PtrToStructure((IntPtr)((UInt32)pt + i * Marshal.SizeOf(typeof(VCI_CAN_OBJ))), typeof(VCI_CAN_OBJ));
          resultList = new List<short>();
          str = "接收到数据: ";
          str += "  帧ID:0x" + Convert.ToString(m_recobj[i].ID, 16);
          frameRcvStrId = Convert.ToString(m_recobj[i].ID, 16);
          int frmNum = Convert.ToInt32(frameRcvStrId);
          if (ResultDict.ContainsKey(frmNum)) // 不要重複建立，跳出換下一個
            continue;

          rcvIdNum = Convert.ToInt32(frameRcvStrId);

          // slaveid 是 frameRcvStrId 的個位數
          SlaveID = rcvIdNum % 10;

          str += "  帧格式:";
          if (m_recobj[i].RemoteFlag == 0)
            str += "数据帧 ";
          else
            str += "远程帧 ";
          if (m_recobj[i].ExternFlag == 0)
            str += "标准帧 ";
          else
            str += "扩展帧 ";
          //Int32 tmpVal = Convert.ToInt32(m_recobj[i].ID);

          resultList.Add((short)SlaveID); // index 0 代表ID號，讓以後去解析
          if (m_recobj[i].RemoteFlag == 0)
          {
            str += "数据: ";
            byte len = (byte)(m_recobj[i].DataLen % 9);
            byte j = 0;
            fixed (VCI_CAN_OBJ* m_recobj1 = &m_recobj[i])
            {
              if (j++ < len)
                str += " " + System.Convert.ToString(m_recobj1->Data[0], 16);
              if (j++ < len)
                str += " " + System.Convert.ToString(m_recobj1->Data[1], 16);
              if (j++ < len)
                str += " " + System.Convert.ToString(m_recobj1->Data[2], 16);
              if (j++ < len)
                str += " " + System.Convert.ToString(m_recobj1->Data[3], 16);
              if (j++ < len)
                str += " " + System.Convert.ToString(m_recobj1->Data[4], 16);
              if (j++ < len)
                str += " " + System.Convert.ToString(m_recobj1->Data[5], 16);
              if (j++ < len)
                str += " " + System.Convert.ToString(m_recobj1->Data[6], 16);
              if (j++ < len)
                str += " " + System.Convert.ToString(m_recobj1->Data[7], 16);

              resultList.Add(m_recobj1->Data[0]);
              resultList.Add(m_recobj1->Data[1]);
              resultList.Add(m_recobj1->Data[2]);
              resultList.Add(m_recobj1->Data[3]);
              resultList.Add(m_recobj1->Data[4]);
              resultList.Add(m_recobj1->Data[5]);
              resultList.Add(m_recobj1->Data[6]);
              resultList.Add(m_recobj1->Data[7]);
            }
          }
          int idNum = Convert.ToInt16(frameRcvStrId);
          ResultDict.TryAdd(idNum, resultList);
        }
        return ResultDict;
      }
      catch (Exception ex)
      {
        return null;
      }
    }

    unsafe public ConcurrentDictionary<string, List<short>> RcvData2()
    {
      try
      {
        UInt32 res = 0;
        int waitCount = 100; // 每100ms讀取一次,waitCount=0則跳出
                             // 當串接的時候有可能會串接兩個以上,最多8個PACK包
                             //UInt32 getStrNum = VCI_GetReceiveNum(m_devtype, m_devind, m_canind);

        Stopwatch t1 = new Stopwatch();
        t1.Start();
        while (res == 0 && waitCount != 0) // 只要收到res>0 或是waitcount==0 都會跳出
        {
          res = VCI_Receive(m_devtype, m_devind, m_canind, ref m_recobj[0], 1000, 100);
          usdelay.TimeDelayus(10000);
          //Thread.Sleep(1);
          waitCount--;
        }
        t1.Stop();
        string debugStr = t1.ElapsedMilliseconds.ToString();

        if (res == 0xFFFFFFFF) res = 0;//当设备未初始化时，返回0xFFFFFFFF，不进行列表显示。
        String str = "";

        List<short> resultList = new List<short>();
        ConcurrentDictionary<string, List<short>> ResultDict = new ConcurrentDictionary<string, List<short>>();
        string frameRcvStrId = "";
        int rcvIdNum = 0;
        int SlaveID = 0;


        // 重新建立資料結構
        // ResultDict
        // KEY =>  frameRcvStrId (這個數字包含了ID站名以及命令代號)，之後再去解析
        // VALUE => 8bit 的List

        // 由於丟的速度太快收的慢，可能導致重複數據擷取
        // 這樣會讓 ResultDict 新增相同KEY，造成崩潰所以遇到重複就跳出不建立
        for (UInt32 i = 0; i < res; i++)
        {
          //VCI_CAN_OBJ obj = (VCI_CAN_OBJ)Marshal.PtrToStructure((IntPtr)((UInt32)pt + i * Marshal.SizeOf(typeof(VCI_CAN_OBJ))), typeof(VCI_CAN_OBJ));
          resultList = new List<short>();
          str = "接收到数据: ";
          str += "  帧ID:0x" + Convert.ToString(m_recobj[i].ID, 16);
          frameRcvStrId = Convert.ToString(m_recobj[i].ID, 16);
          //int frmNum = Convert.ToInt32(frameRcvStrId);
          if (ResultDict.ContainsKey(frameRcvStrId)) // 不要重複建立，跳出換下一個
            continue;

          //rcvIdNum = Convert.ToInt32(frameRcvStrId);

          // slaveid 是 frameRcvStrId 的個位數
          //SlaveID = rcvIdNum % 10;
          SlaveID = Convert.ToInt16(frameRcvStrId.Substring(2,1));

          str += "  帧格式:";
          if (m_recobj[i].RemoteFlag == 0)
            str += "数据帧 ";
          else
            str += "远程帧 ";
          if (m_recobj[i].ExternFlag == 0)
            str += "标准帧 ";
          else
            str += "扩展帧 ";
          //Int32 tmpVal = Convert.ToInt32(m_recobj[i].ID);

          resultList.Add((short)SlaveID); // index 0 代表ID號，讓以後去解析
          if (m_recobj[i].RemoteFlag == 0)
          {
            str += "数据: ";
            byte len = (byte)(m_recobj[i].DataLen % 9);
            byte j = 0;
            fixed (VCI_CAN_OBJ* m_recobj1 = &m_recobj[i])
            {
              if (j++ < len)
                str += " " + System.Convert.ToString(m_recobj1->Data[0], 16);
              if (j++ < len)
                str += " " + System.Convert.ToString(m_recobj1->Data[1], 16);
              if (j++ < len)
                str += " " + System.Convert.ToString(m_recobj1->Data[2], 16);
              if (j++ < len)
                str += " " + System.Convert.ToString(m_recobj1->Data[3], 16);
              if (j++ < len)
                str += " " + System.Convert.ToString(m_recobj1->Data[4], 16);
              if (j++ < len)
                str += " " + System.Convert.ToString(m_recobj1->Data[5], 16);
              if (j++ < len)
                str += " " + System.Convert.ToString(m_recobj1->Data[6], 16);
              if (j++ < len)
                str += " " + System.Convert.ToString(m_recobj1->Data[7], 16);

              resultList.Add(m_recobj1->Data[0]);
              resultList.Add(m_recobj1->Data[1]);
              resultList.Add(m_recobj1->Data[2]);
              resultList.Add(m_recobj1->Data[3]);
              resultList.Add(m_recobj1->Data[4]);
              resultList.Add(m_recobj1->Data[5]);
              resultList.Add(m_recobj1->Data[6]);
              resultList.Add(m_recobj1->Data[7]);
            }
          }
          //int idNum = Convert.ToInt16(frameRcvStrId);
          ResultDict.TryAdd(frameRcvStrId, resultList);
        }
        return ResultDict;
      }
      catch (Exception ex)
      {
        return null;
      }
    }


    /// <summary>
    /// 單一命令 嘉義案 經發局
    /// </summary>
    /// <param name="id"></param>
    /// 

    // 重寫正確規格
    // 20241203
    // BCU + BMU 組合時 ，每個箱子上都有自己的 DIP - SWITCH，代表不同ID
    //                        rackcoreID
    // 1. 想要詢問 RACK資訊時，請使用(BCU-ID) << 12 + 0x602
    // -- 1.1 命令為 A0 10 25 01 00 00 00 00
    // 2. 想要詢問 PACK資訊時，請使用(BCU-ID) << 12 + 0x610 + PACKID
    // -- 2.1 電壓命令為 A0 40 25 01 00 00 00 00
    // -- 2.2 溫度命令為 A0 60 25 01 00 00 00 00
    // 所以直接重寫兩個FUNC

    // 計算ID
    //public uint calRackID(uint rackDipsw)
    //{
    //  uint val = (rackDipsw << 12) +Convert.ToUInt32("0610", 16);
    //  return val;
    //}

    // RACK
    unsafe public bool csReadRackInfo(uint rackDipsw)
    {
      try
      {
        // 寫入測試
        VCI_CAN_OBJ sendobj1 = new VCI_CAN_OBJ();
        sendobj1.RemoteFlag = (byte)0; // 0 data  1 remote
        sendobj1.ExternFlag = (byte)1; // 0 標準frame  1 extended
        //sendobj1.ID = System.Convert.ToUInt32("0610", 16); // request Command
        //sendobj1.ID += Convert.ToUInt16(id);
        sendobj1.ID = (rackDipsw << 12) + Convert.ToUInt32("602", 16); // request Command
        sendobj1.Data[0] = 0xA0;
        sendobj1.Data[1] = 0x10;
        sendobj1.Data[2] = 0x25;
        sendobj1.Data[3] = 0x01;
        sendobj1.Data[4] = 0x00;
        sendobj1.Data[5] = 0x00;
        sendobj1.Data[6] = 0x00;
        sendobj1.Data[7] = 0x00;
        sendobj1.DataLen = 8;

        if (VCI_Transmit(m_devtype, m_devind, m_canind, ref sendobj1, 1) == 0)
          errSendCount++;
        //usdelay.TimeDelayus(_cmdDelay * 1000 * 2);
        //2025 07 17 rack 需要更多時間回資訊 兩倍時間

        Stopwatch t1 = new Stopwatch();
        t1.Start();
        uint res = 0;
        int waitCount = 100;
        
        List<short> resultList = new List<short>();
        Dictionary<string, List<short>> ResultDict = new Dictionary<string, List<short>>();

        while (ResultDict.Count != 7 && t1.ElapsedMilliseconds < 1000)
        {
          res = VCI_Receive(m_devtype, m_devind, m_canind, ref m_recobj[0], 1000, 100);
          usdelay.TimeDelayus(10000);

          // 如果res > 0 把數值存起來
          for (UInt32 i = 0; i < res; i++)
          {
            resultList = new List<short>();
            Int32 tmpVal = Convert.ToInt32(m_recobj[i].ID);

            resultList.Add((short)tmpVal);
            if (m_recobj[i].RemoteFlag == 0)
            {
              byte len = (byte)(m_recobj[i].DataLen % 9);
              fixed (VCI_CAN_OBJ* m_recobj1 = &m_recobj[i])
              {
                resultList.Add(m_recobj1->Data[0]);
                resultList.Add(m_recobj1->Data[1]);
                resultList.Add(m_recobj1->Data[2]);
                resultList.Add(m_recobj1->Data[3]);
                resultList.Add(m_recobj1->Data[4]);
                resultList.Add(m_recobj1->Data[5]);
                resultList.Add(m_recobj1->Data[6]);
                resultList.Add(m_recobj1->Data[7]);
              }
            }
            string bt1Str = Convert.ToString(resultList[1], 16);
            if (ResultDict.ContainsKey(bt1Str) != true)
              ResultDict.Add(bt1Str, resultList);
          }

        }
        t1.Stop();
        string debugStr = t1.ElapsedMilliseconds.ToString();

        //if (res == 0xFFFFFFFF) res = 0;//当设备未初始化时，返回0xFFFFFFFF，不进行列表显示。
        //String str = "";

        
        //string frameRcvStrId = "";

        //for (UInt32 i = 0; i < res; i++)
        //{
        //  resultList = new List<short>();

        //  Int32 tmpVal = Convert.ToInt32(m_recobj[i].ID);

        //  resultList.Add((short)tmpVal);
        //  if (m_recobj[i].RemoteFlag == 0)
        //  {
        //    byte len = (byte)(m_recobj[i].DataLen % 9);
        //    byte j = 0;
        //    fixed (VCI_CAN_OBJ* m_recobj1 = &m_recobj[i])
        //    {
        //      resultList.Add(m_recobj1->Data[0]);
        //      resultList.Add(m_recobj1->Data[1]);
        //      resultList.Add(m_recobj1->Data[2]);
        //      resultList.Add(m_recobj1->Data[3]);
        //      resultList.Add(m_recobj1->Data[4]);
        //      resultList.Add(m_recobj1->Data[5]);
        //      resultList.Add(m_recobj1->Data[6]);
        //      resultList.Add(m_recobj1->Data[7]);
        //    }
        //  }
        //  string bt1Str = Convert.ToString(resultList[1], 16);
        //  ResultDict.Add(bt1Str, resultList);
        //}
        // 全部做完一次assign
        RackDataChayiBCU.Flag1 = ResultDict["b3"][2].ToString();
        RackDataChayiBCU.Flag2 = ResultDict["b3"][3].ToString();
        RackDataChayiBCU.Flag3 = ResultDict["b3"][4].ToString();
        RackDataChayiBCU.Flag4 = ResultDict["b3"][5].ToString();
        RackDataChayiBCU.Flag5 = ResultDict["b3"][6].ToString();
        RackDataChayiBCU.Flag6 = ResultDict["b3"][7].ToString();
        RackDataChayiBCU.Flag7 = ResultDict["b3"][8].ToString();

        RackDataChayiBCU.Flag8 = ResultDict["b4"][2].ToString();
        RackDataChayiBCU.Flag9 = ResultDict["b4"][3].ToString();
        RackDataChayiBCU.Flag10 = ResultDict["b4"][4].ToString();
        RackDataChayiBCU.Flag11 = ResultDict["b4"][5].ToString();
        RackDataChayiBCU.Flag12 = ResultDict["b4"][6].ToString();
        RackDataChayiBCU.Flag13 = ResultDict["b4"][7].ToString();
        RackDataChayiBCU.Flag14 = ResultDict["b4"][8].ToString();

        // 解析正負號
        Int32 tmp24bitVoltageVal = (ResultDict["b1"][4] << 16) + (ResultDict["b1"][3] << 8) + ResultDict["b1"][2];
        if (tmp24bitVoltageVal <= 0x7FFFFF)
          RackDataChayiBCU.RackVoltage24Bits = tmp24bitVoltageVal.ToString();
        else
          RackDataChayiBCU.RackVoltage24Bits = (tmp24bitVoltageVal - 0x1000000).ToString();

        Int32 tmp24bitCurrentVal = (ResultDict["b2"][4] << 16) + (ResultDict["b2"][3] << 8) + ResultDict["b2"][2];
        if (tmp24bitCurrentVal <= 0x7FFFFF)
          RackDataChayiBCU.RackCurrent24Bits = tmp24bitCurrentVal.ToString();
        else
          RackDataChayiBCU.RackCurrent24Bits = (tmp24bitCurrentVal - 0x1000000).ToString();

        //if (ResultDict["b1"][4] <= 127)
        //  RackDataChayiBCU.RackVoltage24Bits = ((ResultDict["b1"][4] << 16) + (ResultDict["b1"][3] << 8) + ResultDict["b1"][2]).ToString();
        //else
        //{
        //  RackDataChayiBCU.RackVoltage24Bits = ((ResultDict["b1"][4] << 24) + (ResultDict["b1"][4] << 16) + (ResultDict["b1"][3] << 8) + ResultDict["b1"][2]).ToString();
        //}

        //if(ResultDict["b2"][4] <= 127)
        //  RackDataChayiBCU.RackCurrent24Bits = ((ResultDict["b2"][4] << 16) + (ResultDict["b2"][3] << 8) + ResultDict["b2"][2]).ToString();
        //else
        //{
        //  RackDataChayiBCU.RackCurrent24Bits = ((ResultDict["b2"][4] << 24) + (ResultDict["b2"][4] << 16) + (ResultDict["b2"][3] << 8) + ResultDict["b2"][2]).ToString();
        //}

        RackDataChayiBCU.SoC = ((ResultDict["b0"][3] << 8) + ResultDict["b0"][2]).ToString();
        RackDataChayiBCU.SOH = ((ResultDict["b0"][5] << 8) + ResultDict["b0"][4]).ToString();

        // 解析 取string 轉 byte list
        List<bool> flag3List = BytesToList(Convert.ToInt32(RackDataChayiBCU.Flag3));
        List<bool> flag6List = BytesToList(Convert.ToInt32(RackDataChayiBCU.Flag6));
        RackDataChayiBCU.COV = flag3List[2];
        RackDataChayiBCU.CUV = flag3List[3];

        RackDataChayiBCU.COT = flag6List[0];
        RackDataChayiBCU.DOT = flag6List[1];

        // 紀錄全部FLAG
        string allflagStr = "";
        allflagStr += RackDataChayiBCU.Flag1.ToString().PadLeft(2, '0');
        allflagStr += RackDataChayiBCU.Flag2.ToString().PadLeft(2, '0');
        allflagStr += RackDataChayiBCU.Flag3.ToString().PadLeft(2, '0');
        allflagStr += RackDataChayiBCU.Flag4.ToString().PadLeft(2, '0');
        allflagStr += RackDataChayiBCU.Flag5.ToString().PadLeft(2, '0');
        allflagStr += RackDataChayiBCU.Flag6.ToString().PadLeft(2, '0');
        allflagStr += RackDataChayiBCU.Flag7.ToString().PadLeft(2, '0');
        allflagStr += RackDataChayiBCU.Flag8.ToString().PadLeft(2, '0');
        allflagStr += RackDataChayiBCU.Flag9.ToString().PadLeft(2, '0');
        allflagStr += RackDataChayiBCU.Flag10.ToString().PadLeft(2, '0');
        allflagStr += RackDataChayiBCU.Flag11.ToString().PadLeft(2, '0');
        allflagStr += RackDataChayiBCU.Flag12.ToString().PadLeft(2, '0');
        allflagStr += RackDataChayiBCU.Flag13.ToString().PadLeft(2, '0');
        allflagStr += RackDataChayiBCU.Flag14.ToString().PadLeft(2, '0');
        
        RackDataChayiBCU.FlagALL = allflagStr;

        //usdelay.TimeDelayus(_cmdDelay * 1000 );

        return true;
      }
      catch (Exception ex)
      {
        ex.ToString();
        return false;
      }
    }

    List<short> resultList = new List<short>();
    Dictionary<string, List<short>> ResultDict = new Dictionary<string, List<short>>();


    // PACK
    unsafe public bool csReadBmsAll(uint rackDipsw, int id)
    {
      // id bms only
      try
      {
        //VCI_ClearBuffer(m_devtype, m_devind, m_canind);
        //usdelay.TimeDelayus(10000);
        // 寫入測試
        VCI_CAN_OBJ sendobj1 = new VCI_CAN_OBJ();
        sendobj1.RemoteFlag = (byte)0; // 0 data  1 remote
        sendobj1.ExternFlag = (byte)1; // 0 標準frame  1 extended
                                       // string cmdRequest = "0x0700";
        sendobj1.ID = rackDipsw << 12;
        sendobj1.ID += System.Convert.ToUInt32("0610", 16); // request Command
        sendobj1.ID += Convert.ToUInt16(id);

        // read cell
        sendobj1.Data[0] = 0xA0;
        sendobj1.Data[1] = 0x40;
        sendobj1.Data[2] = 0x25;
        sendobj1.Data[3] = 0x01;
        sendobj1.Data[4] = 0x00;
        sendobj1.Data[5] = 0x00;
        sendobj1.Data[6] = 0x00;
        sendobj1.Data[7] = 0x00;
        sendobj1.DataLen = 8;

        if (VCI_Transmit(m_devtype, m_devind, m_canind, ref sendobj1, 1) == 0)
          errSendCount++;
        //Thread.Sleep(600);
        usdelay.TimeDelayus(1000);
        //usdelay.TimeDelayus(_cmdDelay * 1000 *4); // 回8個，實測命令大約會等 400 - 500ms

        Stopwatch t1 = new Stopwatch();
        t1.Start();
        uint res = 0;
        //res = VCI_Receive(m_devtype, m_devind, m_canind, ref m_recobj[0], 1000, 100);
        //usdelay.TimeDelayus(10000);

        int waitCount = 10;
        // 2025 07 18 改變處理方式
        // 發送完畢不用等
        // 持續每100 ms 收命令 直到收到 res =0 跳出
        // 
        resultList = new List<short>();
        ResultDict = new Dictionary<string, List<short>>();

        //res = VCI_Receive(m_devtype, m_devind, m_canind, ref m_recobj[0], 1000, 100);
        //usdelay.TimeDelayus(10000);
        // 這邊我就是要收到8個,沒收完不走，除非超過一秒
        while (ResultDict.Count!= 8 && t1.ElapsedMilliseconds < 1000) 
        {
          res = VCI_Receive(m_devtype, m_devind, m_canind, ref m_recobj[0], 1000, 100);
          usdelay.TimeDelayus(10000);

          // 如果res > 0 把數值存起來
          for (UInt32 i = 0; i < res; i++)
          {
            resultList = new List<short>();
            Int32 tmpVal = Convert.ToInt32(m_recobj[i].ID);

            resultList.Add((short)tmpVal);
            if (m_recobj[i].RemoteFlag == 0)
            {
              byte len = (byte)(m_recobj[i].DataLen % 9);
              fixed (VCI_CAN_OBJ* m_recobj1 = &m_recobj[i])
              {
                resultList.Add(m_recobj1->Data[0]);
                resultList.Add(m_recobj1->Data[1]);
                resultList.Add(m_recobj1->Data[2]);
                resultList.Add(m_recobj1->Data[3]);
                resultList.Add(m_recobj1->Data[4]);
                resultList.Add(m_recobj1->Data[5]);
                resultList.Add(m_recobj1->Data[6]);
                resultList.Add(m_recobj1->Data[7]);
              }
            }
            string bt1Str = Convert.ToString(resultList[1], 16);
            if(ResultDict.ContainsKey(bt1Str)!=true)
              ResultDict.Add(bt1Str, resultList);
          }

          //Thread.Sleep(1);
          //waitCount--;
        }
        t1.Stop();
        string debugStr = t1.ElapsedMilliseconds.ToString();

        //if (res == 0xFFFFFFFF) res = 0;//当设备未初始化时，返回0xFFFFFFFF，不进行列表显示。
        //String str = "";

        //resultList = new List<short>();
        //ResultDict = new Dictionary<string, List<short>>();
        //string frameRcvStrId = "";
        //// 計算frameRcvStrId
        //for (UInt32 i = 0; i < res; i++)
        //{
        //  resultList = new List<short>();

        //  Int32 tmpVal = Convert.ToInt32(m_recobj[i].ID);

        //  resultList.Add((short)tmpVal);
        //  if (m_recobj[i].RemoteFlag == 0)
        //  {
        //    byte len = (byte)(m_recobj[i].DataLen % 9);
        //    byte j = 0;
        //    fixed (VCI_CAN_OBJ* m_recobj1 = &m_recobj[i])
        //    {
        //      resultList.Add(m_recobj1->Data[0]);
        //      resultList.Add(m_recobj1->Data[1]);
        //      resultList.Add(m_recobj1->Data[2]);
        //      resultList.Add(m_recobj1->Data[3]);
        //      resultList.Add(m_recobj1->Data[4]);
        //      resultList.Add(m_recobj1->Data[5]);
        //      resultList.Add(m_recobj1->Data[6]);
        //      resultList.Add(m_recobj1->Data[7]);
        //    }
        //  }
        //  string bt1Str = Convert.ToString(resultList[1], 16);
        //  if (ResultDict.ContainsKey(bt1Str) != true)
        //    ResultDict.Add(bt1Str, resultList);
        //}
        // 全部做完一次 assign 

        TotalMasterDataChayi[id].CellVoltage1 = ((ResultDict["b0"][3] << 8) + ResultDict["b0"][2]).ToString();
        TotalMasterDataChayi[id].CellVoltage2 = ((ResultDict["b0"][5] << 8) + ResultDict["b0"][4]).ToString();
        TotalMasterDataChayi[id].CellVoltage3 = ((ResultDict["b0"][7] << 8) + ResultDict["b0"][6]).ToString();
        TotalMasterDataChayi[id].CellVoltage4 = ((ResultDict["b1"][2] << 8) + ResultDict["b0"][8]).ToString();
        TotalMasterDataChayi[id].CellVoltage5 = ((ResultDict["b1"][4] << 8) + ResultDict["b1"][3]).ToString();
        TotalMasterDataChayi[id].CellVoltage6 = ((ResultDict["b1"][6] << 8) + ResultDict["b1"][5]).ToString();
        TotalMasterDataChayi[id].CellVoltage7 = ((ResultDict["b1"][8] << 8) + ResultDict["b1"][7]).ToString();
        TotalMasterDataChayi[id].CellVoltage8 = ((ResultDict["b2"][3] << 8) + ResultDict["b2"][2]).ToString();
        TotalMasterDataChayi[id].CellVoltage9 = ((ResultDict["b2"][5] << 8) + ResultDict["b2"][4]).ToString();
        TotalMasterDataChayi[id].CellVoltage10 = ((ResultDict["b2"][7] << 8) + ResultDict["b2"][6]).ToString();
        TotalMasterDataChayi[id].CellVoltage11 = ((ResultDict["b3"][2] << 8) + ResultDict["b2"][8]).ToString();
        TotalMasterDataChayi[id].CellVoltage12 = ((ResultDict["b3"][4] << 8) + ResultDict["b3"][3]).ToString();
        TotalMasterDataChayi[id].CellVoltage13 = ((ResultDict["b3"][6] << 8) + ResultDict["b3"][5]).ToString();
        TotalMasterDataChayi[id].CellVoltage14 = ((ResultDict["b3"][8] << 8) + ResultDict["b3"][7]).ToString();
        TotalMasterDataChayi[id].CellVoltage15 = ((ResultDict["b4"][3] << 8) + ResultDict["b4"][2]).ToString();
        TotalMasterDataChayi[id].CellVoltage16 = ((ResultDict["b4"][5] << 8) + ResultDict["b4"][4]).ToString();
        TotalMasterDataChayi[id].CellVoltage17 = ((ResultDict["b4"][7] << 8) + ResultDict["b4"][6]).ToString();
        TotalMasterDataChayi[id].CellVoltage18 = ((ResultDict["b5"][2] << 8) + ResultDict["b4"][8]).ToString();
        TotalMasterDataChayi[id].CellVoltage19 = ((ResultDict["b5"][4] << 8) + ResultDict["b5"][3]).ToString();
        TotalMasterDataChayi[id].CellVoltage20 = ((ResultDict["b5"][6] << 8) + ResultDict["b5"][5]).ToString();
        TotalMasterDataChayi[id].CellVoltage21 = ((ResultDict["b5"][8] << 8) + ResultDict["b5"][7]).ToString();
        TotalMasterDataChayi[id].CellVoltage22 = ((ResultDict["b6"][3] << 8) + ResultDict["b6"][2]).ToString();

        // 取 deltav

        List<int> cellList = new List<int>();
        cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage1));
        cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage2));
        cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage3));
        cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage4));
        cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage5));
        cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage6));
        cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage7));
        cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage8));
        cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage9));
        cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage10));
        cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage11));
        cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage12));
        cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage13));
        cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage14));
        cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage15));
        cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage16));
        cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage17));
        cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage18));
        cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage19));
        cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage20));
        cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage21));
        cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage22));
        TotalMasterDataChayi[id].PackVoltage = cellList.Sum().ToString();

        // 2025/06/10 REX 反映有21串 ,所以如果有0要捨棄 
        List<int> tmpCellNonZeroList = new List<int>();
        for(int i=0;i<10;i++)
        {
          if (cellList[i] != 0)
            tmpCellNonZeroList.Add(cellList[i]);
        }

        //TotalMasterDataChayi[id].deltaV = (cellList.Max() - cellList.Min()).ToString();
        TotalMasterDataChayi[id].deltaV = (tmpCellNonZeroList.Max() - tmpCellNonZeroList.Min()).ToString();

        usdelay.TimeDelayus(1000);

        // temperature
        sendobj1.Data[0] = 0xA0;
        sendobj1.Data[1] = 0x60;
        sendobj1.Data[2] = 0x25;
        sendobj1.Data[3] = 0x01;
        sendobj1.Data[4] = 0x00;
        sendobj1.Data[5] = 0x00;
        sendobj1.Data[6] = 0x00;
        sendobj1.Data[7] = 0x00;
        sendobj1.DataLen = 8;

        if (VCI_Transmit(m_devtype, m_devind, m_canind, ref sendobj1, 1) == 0)
        {
          errSendCount++;
        }
        //usdelay.TimeDelayus(_cmdDelay * 1000 *4);

        Stopwatch t2 = new Stopwatch();
        t2.Start();
        res = 0;
        waitCount = 10;
        // 這邊我就是要收到5個,沒收完不走，除非超過一秒
        resultList = new List<short>();
        ResultDict = new Dictionary<string, List<short>>();

        while (ResultDict.Count != 5 && t2.ElapsedMilliseconds < 1000)
        {
          res = VCI_Receive(m_devtype, m_devind, m_canind, ref m_recobj[0], 1000, 100);
          usdelay.TimeDelayus(10000);

          // 如果res > 0 把數值存起來
          for (UInt32 i = 0; i < res; i++)
          {
            resultList = new List<short>();
            Int32 tmpVal = Convert.ToInt32(m_recobj[i].ID);

            resultList.Add((short)tmpVal);
            if (m_recobj[i].RemoteFlag == 0)
            {
              byte len = (byte)(m_recobj[i].DataLen % 9);
              fixed (VCI_CAN_OBJ* m_recobj1 = &m_recobj[i])
              {
                resultList.Add(m_recobj1->Data[0]);
                resultList.Add(m_recobj1->Data[1]);
                resultList.Add(m_recobj1->Data[2]);
                resultList.Add(m_recobj1->Data[3]);
                resultList.Add(m_recobj1->Data[4]);
                resultList.Add(m_recobj1->Data[5]);
                resultList.Add(m_recobj1->Data[6]);
                resultList.Add(m_recobj1->Data[7]);
              }
            }
            string bt1Str = Convert.ToString(resultList[1], 16);
            if (ResultDict.ContainsKey(bt1Str) != true)
              ResultDict.Add(bt1Str, resultList);
          }

        }
        t2.Stop();
        debugStr = t2.ElapsedMilliseconds.ToString();

        //if (res == 0xFFFFFFFF) res = 0;//当设备未初始化时，返回0xFFFFFFFF，不进行列表显示。
        //str = "";

        //resultList = new List<short>();
        //ResultDict = new Dictionary<string, List<short>>();
        //frameRcvStrId = "";
        //if (res != 5)
        //  return false;
        //for (UInt32 i = 0; i < res; i++)
        //{
        //  resultList = new List<short>();

        //  Int32 tmpVal = Convert.ToInt32(m_recobj[i].ID);

        //  resultList.Add((short)tmpVal);
        //  if (m_recobj[i].RemoteFlag == 0)
        //  {
        //    byte len = (byte)(m_recobj[i].DataLen % 9);
        //    byte j = 0;
        //    fixed (VCI_CAN_OBJ* m_recobj1 = &m_recobj[i])
        //    {
        //      resultList.Add(m_recobj1->Data[0]);
        //      resultList.Add(m_recobj1->Data[1]);
        //      resultList.Add(m_recobj1->Data[2]);
        //      resultList.Add(m_recobj1->Data[3]);
        //      resultList.Add(m_recobj1->Data[4]);
        //      resultList.Add(m_recobj1->Data[5]);
        //      resultList.Add(m_recobj1->Data[6]);
        //      resultList.Add(m_recobj1->Data[7]);
        //    }
        //  }
        //  string bt1Str = Convert.ToString(resultList[1], 16);
        //  // 重複就捨棄
        //  if(ResultDict.ContainsKey(bt1Str)!=true)
        //    ResultDict.Add(bt1Str, resultList);
        //}
        // 全部做完一次assign
        TotalMasterDataChayi[id].Temperature1 = ((ResultDict["b0"][3] << 8) + ResultDict["b0"][2]).ToString();
        TotalMasterDataChayi[id].Temperature2 = ((ResultDict["b0"][5] << 8) + ResultDict["b0"][4]).ToString();
        TotalMasterDataChayi[id].Temperature3 = ((ResultDict["b0"][7] << 8) + ResultDict["b0"][6]).ToString();
        TotalMasterDataChayi[id].Temperature4 = ((ResultDict["b1"][2] << 8) + ResultDict["b0"][8]).ToString();
        TotalMasterDataChayi[id].Temperature5 = ((ResultDict["b1"][4] << 8) + ResultDict["b1"][3]).ToString();
        TotalMasterDataChayi[id].Temperature6 = ((ResultDict["b1"][6] << 8) + ResultDict["b1"][5]).ToString();
        TotalMasterDataChayi[id].Temperature7 = ((ResultDict["b1"][8] << 8) + ResultDict["b1"][7]).ToString();
        TotalMasterDataChayi[id].Temperature8 = ((ResultDict["b2"][3] << 8) + ResultDict["b2"][2]).ToString();
        TotalMasterDataChayi[id].Temperature9 = ((ResultDict["b2"][5] << 8) + ResultDict["b2"][4]).ToString();
        TotalMasterDataChayi[id].Temperature10 = ((ResultDict["b2"][7] << 8) + ResultDict["b2"][6]).ToString();
        TotalMasterDataChayi[id].Temperature11 = ((ResultDict["b3"][2] << 8) + ResultDict["b2"][8]).ToString();
        TotalMasterDataChayi[id].Temperature12 = ((ResultDict["b3"][4] << 8) + ResultDict["b3"][3]).ToString();
        //usdelay.TimeDelayus(_cmdDelay * 1000);

        return true;
      }
      catch (Exception ex)
      {
        //ex.ToString();
        Console.WriteLine("例外訊息：" + ex.Message);
        Console.WriteLine("堆疊追蹤：" + ex.StackTrace);

        return false;
      }
    }

    // PACK
    //unsafe public bool csReadBmsCell(int id)
    //{
    //  // id bms only
    //  try
    //  {
    //    // 寫入測試
    //    VCI_CAN_OBJ sendobj1 = new VCI_CAN_OBJ();
    //    sendobj1.RemoteFlag = (byte)0; // 0 data  1 remote
    //    sendobj1.ExternFlag = (byte)1; // 0 標準frame  1 extended
    //    // string cmdRequest = "0x0700";

    //    sendobj1.ID = System.Convert.ToUInt32("0610", 16); // request Command
    //    sendobj1.ID += Convert.ToUInt16(id);

    //    // read cell
    //    sendobj1.Data[0] = 0xA0;
    //    sendobj1.Data[1] = 0x40;
    //    sendobj1.Data[2] = 0x25;
    //    sendobj1.Data[3] = 0x01;
    //    sendobj1.Data[4] = 0x00;
    //    sendobj1.Data[5] = 0x00;
    //    sendobj1.Data[6] = 0x00;
    //    sendobj1.Data[7] = 0x00;
    //    sendobj1.DataLen = 8;

    //    if (VCI_Transmit(m_devtype, m_devind, m_canind, ref sendobj1, 1) == 0)
    //    {
    //      errSendCount++;
    //    }
    //    usdelay.TimeDelayus(_cmdDelay * 1000);

    //    Stopwatch t1 = new Stopwatch();
    //    t1.Start();
    //    uint res = 0;
    //    int waitCount = 10;
    //    while (res == 0 && waitCount != 0) // 只要收到res>0 或是waitcount==0 都會跳出
    //    {
    //      res = VCI_Receive(m_devtype, m_devind, m_canind, ref m_recobj[0], 1000, 100);
    //      usdelay.TimeDelayus(1000);
    //      //Thread.Sleep(1);
    //      waitCount--;
    //    }
    //    t1.Stop();
    //    string debugStr = t1.ElapsedMilliseconds.ToString();

    //    if (res == 0xFFFFFFFF) res = 0;//当设备未初始化时，返回0xFFFFFFFF，不进行列表显示。
    //    String str = "";

    //    List<short> resultList = new List<short>();
    //    Dictionary<string, List<short>> ResultDict = new Dictionary<string, List<short>>();
    //    string frameRcvStrId = "";

    //    for (UInt32 i = 0; i < res; i++)
    //    {
    //      resultList = new List<short>();

    //      Int32 tmpVal = Convert.ToInt32(m_recobj[i].ID);

    //      resultList.Add((short)tmpVal);
    //      if (m_recobj[i].RemoteFlag == 0)
    //      {
    //        byte len = (byte)(m_recobj[i].DataLen % 9);
    //        byte j = 0;
    //        fixed (VCI_CAN_OBJ* m_recobj1 = &m_recobj[i])
    //        {
    //          resultList.Add(m_recobj1->Data[0]);
    //          resultList.Add(m_recobj1->Data[1]);
    //          resultList.Add(m_recobj1->Data[2]);
    //          resultList.Add(m_recobj1->Data[3]);
    //          resultList.Add(m_recobj1->Data[4]);
    //          resultList.Add(m_recobj1->Data[5]);
    //          resultList.Add(m_recobj1->Data[6]);
    //          resultList.Add(m_recobj1->Data[7]);
    //        }



    //      }
    //      string bt1Str = Convert.ToString(resultList[1], 16);
    //      ResultDict.Add(bt1Str, resultList);
    //    }
    //    // 全部做完一次assign
    //    TotalMasterDataChayi[id].CellVoltage1 = ((ResultDict["b0"][3] << 8) + ResultDict["b0"][2]).ToString();
    //    TotalMasterDataChayi[id].CellVoltage2 = ((ResultDict["b0"][5] << 8) + ResultDict["b0"][4]).ToString();
    //    TotalMasterDataChayi[id].CellVoltage3 = ((ResultDict["b0"][7] << 8) + ResultDict["b0"][6]).ToString();
    //    TotalMasterDataChayi[id].CellVoltage4 = ((ResultDict["b1"][2] << 8) + ResultDict["b0"][8]).ToString();
    //    TotalMasterDataChayi[id].CellVoltage5 = ((ResultDict["b1"][4] << 8) + ResultDict["b1"][3]).ToString();
    //    TotalMasterDataChayi[id].CellVoltage6 = ((ResultDict["b1"][6] << 8) + ResultDict["b1"][5]).ToString();
    //    TotalMasterDataChayi[id].CellVoltage7 = ((ResultDict["b1"][8] << 8) + ResultDict["b1"][7]).ToString();
    //    TotalMasterDataChayi[id].CellVoltage8 = ((ResultDict["b2"][3] << 8) + ResultDict["b2"][2]).ToString();
    //    TotalMasterDataChayi[id].CellVoltage9 = ((ResultDict["b2"][5] << 8) + ResultDict["b2"][4]).ToString();
    //    TotalMasterDataChayi[id].CellVoltage10 = ((ResultDict["b2"][7] << 8) + ResultDict["b2"][6]).ToString();
    //    TotalMasterDataChayi[id].CellVoltage11 = ((ResultDict["b3"][2] << 8) + ResultDict["b2"][8]).ToString();
    //    TotalMasterDataChayi[id].CellVoltage12 = ((ResultDict["b3"][4] << 8) + ResultDict["b3"][3]).ToString();
    //    TotalMasterDataChayi[id].CellVoltage13 = ((ResultDict["b3"][6] << 8) + ResultDict["b3"][5]).ToString();
    //    TotalMasterDataChayi[id].CellVoltage14 = ((ResultDict["b3"][8] << 8) + ResultDict["b3"][7]).ToString();
    //    TotalMasterDataChayi[id].CellVoltage15 = ((ResultDict["b4"][3] << 8) + ResultDict["b4"][2]).ToString();
    //    TotalMasterDataChayi[id].CellVoltage16 = ((ResultDict["b4"][5] << 8) + ResultDict["b4"][4]).ToString();
    //    TotalMasterDataChayi[id].CellVoltage17 = ((ResultDict["b4"][7] << 8) + ResultDict["b4"][6]).ToString();
    //    TotalMasterDataChayi[id].CellVoltage18 = ((ResultDict["b5"][2] << 8) + ResultDict["b4"][8]).ToString();
    //    TotalMasterDataChayi[id].CellVoltage19 = ((ResultDict["b5"][4] << 8) + ResultDict["b5"][3]).ToString();
    //    TotalMasterDataChayi[id].CellVoltage20 = ((ResultDict["b5"][6] << 8) + ResultDict["b5"][5]).ToString();
    //    TotalMasterDataChayi[id].CellVoltage21 = ((ResultDict["b5"][8] << 8) + ResultDict["b5"][7]).ToString();
    //    TotalMasterDataChayi[id].CellVoltage22 = ((ResultDict["b6"][3] << 8) + ResultDict["b6"][2]).ToString();

    //    // 取deltav
    //    List<int> cellList = new List<int>();
    //    cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage1));
    //    cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage2));
    //    cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage3));
    //    cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage4));
    //    cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage5));
    //    cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage6));
    //    cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage7));
    //    cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage8));
    //    cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage9));
    //    cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage10));
    //    cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage11));
    //    cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage12));
    //    cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage13));
    //    cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage14));
    //    cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage15));
    //    cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage16));
    //    cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage17));
    //    cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage18));
    //    cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage19));
    //    cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage20));
    //    cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage21));
    //    cellList.Add(Convert.ToInt16(TotalMasterDataChayi[id].CellVoltage22));
    //    TotalMasterDataChayi[id].deltaV = (cellList.Max() - cellList.Min()).ToString();
    //    //return ResultDict;


    //    return true;
    //  }
    //  catch (Exception ex)
    //  {
    //    ex.ToString();
    //    return false;
    //  }
    //}

    //// PACK
    //unsafe public bool csReadBmsTemperature(int id)
    //{
    //  // id bms only
    //  try
    //  {
    //    // 寫入測試
    //    VCI_CAN_OBJ sendobj1 = new VCI_CAN_OBJ();
    //    sendobj1.RemoteFlag = (byte)0; // 0 data  1 remote
    //    sendobj1.ExternFlag = (byte)1; // 0 標準frame  1 extended
    //                                   // string cmdRequest = "0x0700";

    //    sendobj1.ID = System.Convert.ToUInt32("0610", 16); // request Command
    //    sendobj1.ID += Convert.ToUInt16(id);
    //    // read cell
    //    sendobj1.Data[0] = 0xA0;
    //    sendobj1.Data[1] = 0x60;
    //    sendobj1.Data[2] = 0x25;
    //    sendobj1.Data[3] = 0x01;
    //    sendobj1.Data[4] = 0x00;
    //    sendobj1.Data[5] = 0x00;
    //    sendobj1.Data[6] = 0x00;
    //    sendobj1.Data[7] = 0x00;
    //    sendobj1.DataLen = 8;

    //    if (VCI_Transmit(m_devtype, m_devind, m_canind, ref sendobj1, 1) == 0)
    //    {
    //      errSendCount++;
    //    }
    //    usdelay.TimeDelayus(_cmdDelay * 1000);

    //    Stopwatch t1 = new Stopwatch();
    //    t1.Start();
    //    uint res = 0;
    //    int waitCount = 10;
    //    while (res == 0 && waitCount != 0) // 只要收到res>0 或是waitcount==0 都會跳出
    //    {
    //      res = VCI_Receive(m_devtype, m_devind, m_canind, ref m_recobj[0], 1000, 100);
    //      usdelay.TimeDelayus(1000);
    //      //Thread.Sleep(1);
    //      waitCount--;
    //    }
    //    t1.Stop();
    //    string debugStr = t1.ElapsedMilliseconds.ToString();

    //    if (res == 0xFFFFFFFF) res = 0;//当设备未初始化时，返回0xFFFFFFFF，不进行列表显示。
    //    String str = "";

    //    List<short> resultList = new List<short>();
    //    Dictionary<string, List<short>> ResultDict = new Dictionary<string, List<short>>();
    //    string frameRcvStrId = "";

    //    for (UInt32 i = 0; i < res; i++)
    //    {
    //      resultList = new List<short>();

    //      Int32 tmpVal = Convert.ToInt32(m_recobj[i].ID);

    //      resultList.Add((short)tmpVal);
    //      if (m_recobj[i].RemoteFlag == 0)
    //      {
    //        byte len = (byte)(m_recobj[i].DataLen % 9);
    //        byte j = 0;
    //        fixed (VCI_CAN_OBJ* m_recobj1 = &m_recobj[i])
    //        {
    //          resultList.Add(m_recobj1->Data[0]);
    //          resultList.Add(m_recobj1->Data[1]);
    //          resultList.Add(m_recobj1->Data[2]);
    //          resultList.Add(m_recobj1->Data[3]);
    //          resultList.Add(m_recobj1->Data[4]);
    //          resultList.Add(m_recobj1->Data[5]);
    //          resultList.Add(m_recobj1->Data[6]);
    //          resultList.Add(m_recobj1->Data[7]);
    //        }
    //      }
    //      string bt1Str = Convert.ToString(resultList[1], 16);
    //      ResultDict.Add(bt1Str, resultList);
    //    }
    //    // 全部做完一次assign
    //    TotalMasterDataChayi[id].Temperature1 = ((ResultDict["b0"][3] << 8) + ResultDict["b0"][2]).ToString();
    //    TotalMasterDataChayi[id].Temperature2 = ((ResultDict["b0"][5] << 8) + ResultDict["b0"][4]).ToString();
    //    TotalMasterDataChayi[id].Temperature3 = ((ResultDict["b0"][7] << 8) + ResultDict["b0"][6]).ToString();
    //    TotalMasterDataChayi[id].Temperature4 = ((ResultDict["b1"][2] << 8) + ResultDict["b0"][8]).ToString();
    //    TotalMasterDataChayi[id].Temperature5 = ((ResultDict["b1"][4] << 8) + ResultDict["b1"][3]).ToString();
    //    TotalMasterDataChayi[id].Temperature6 = ((ResultDict["b1"][6] << 8) + ResultDict["b1"][5]).ToString();
    //    TotalMasterDataChayi[id].Temperature7 = ((ResultDict["b1"][8] << 8) + ResultDict["b1"][7]).ToString();
    //    TotalMasterDataChayi[id].Temperature8 = ((ResultDict["b2"][3] << 8) + ResultDict["b2"][2]).ToString();
    //    TotalMasterDataChayi[id].Temperature9 = ((ResultDict["b2"][5] << 8) + ResultDict["b2"][4]).ToString();
    //    TotalMasterDataChayi[id].Temperature10 = ((ResultDict["b2"][7] << 8) + ResultDict["b2"][6]).ToString();
    //    TotalMasterDataChayi[id].Temperature11 = ((ResultDict["b3"][2] << 8) + ResultDict["b2"][8]).ToString();
    //    TotalMasterDataChayi[id].Temperature12 = ((ResultDict["b3"][4] << 8) + ResultDict["b3"][3]).ToString();

    //    //return ResultDict;
    //    return true;
    //  }
    //  catch (Exception ex)
    //  {
    //    ex.ToString();
    //    return false;
    //  }
    //}

    //// RACK
    //unsafe public bool csReadBmsProtection(int id)
    //{
    //  // id bms only
    //  try
    //  {
    //    // 寫入測試
    //    VCI_CAN_OBJ sendobj1 = new VCI_CAN_OBJ();
    //    sendobj1.RemoteFlag = (byte)0; // 0 data  1 remote
    //    sendobj1.ExternFlag = (byte)1; // 0 標準frame  1 extended
    //    sendobj1.ID = System.Convert.ToUInt32("0610", 16); // request Command
    //    sendobj1.ID += Convert.ToUInt16(id);
    //    sendobj1.Data[0] = 0xA0;
    //    sendobj1.Data[1] = 0x10;
    //    sendobj1.Data[2] = 0x25;
    //    sendobj1.Data[3] = 0x01;
    //    sendobj1.Data[4] = 0x00;
    //    sendobj1.Data[5] = 0x00;
    //    sendobj1.Data[6] = 0x00;
    //    sendobj1.Data[7] = 0x00;
    //    sendobj1.DataLen = 8;

    //    if (VCI_Transmit(m_devtype, m_devind, m_canind, ref sendobj1, 1) == 0)
    //    {
    //      errSendCount++;
    //    }
    //    usdelay.TimeDelayus(_cmdDelay * 1000);

    //    Stopwatch t1 = new Stopwatch();
    //    t1.Start();
    //    uint res = 0;
    //    int waitCount = 10;
    //    while (res == 0 && waitCount != 0) // 只要收到res>0 或是waitcount==0 都會跳出
    //    {
    //      res = VCI_Receive(m_devtype, m_devind, m_canind, ref m_recobj[0], 1000, 100);
    //      usdelay.TimeDelayus(1000);
    //      //Thread.Sleep(1);
    //      waitCount--;
    //    }
    //    t1.Stop();
    //    string debugStr = t1.ElapsedMilliseconds.ToString();

    //    if (res == 0xFFFFFFFF) res = 0;//当设备未初始化时，返回0xFFFFFFFF，不进行列表显示。
    //    String str = "";

    //    List<short> resultList = new List<short>();
    //    Dictionary<string, List<short>> ResultDict = new Dictionary<string, List<short>>();
    //    string frameRcvStrId = "";

    //    for (UInt32 i = 0; i < res; i++)
    //    {
    //      resultList = new List<short>();

    //      Int32 tmpVal = Convert.ToInt32(m_recobj[i].ID);

    //      resultList.Add((short)tmpVal);
    //      if (m_recobj[i].RemoteFlag == 0)
    //      {
    //        byte len = (byte)(m_recobj[i].DataLen % 9);
    //        byte j = 0;
    //        fixed (VCI_CAN_OBJ* m_recobj1 = &m_recobj[i])
    //        {
    //          resultList.Add(m_recobj1->Data[0]);
    //          resultList.Add(m_recobj1->Data[1]);
    //          resultList.Add(m_recobj1->Data[2]);
    //          resultList.Add(m_recobj1->Data[3]);
    //          resultList.Add(m_recobj1->Data[4]);
    //          resultList.Add(m_recobj1->Data[5]);
    //          resultList.Add(m_recobj1->Data[6]);
    //          resultList.Add(m_recobj1->Data[7]);
    //        }
    //      }
    //      string bt1Str = Convert.ToString(resultList[1], 16);
    //      ResultDict.Add(bt1Str, resultList);
    //    }
    //    // 全部做完一次assign
    //    TotalMasterDataChayi[id].Flag1 = ResultDict["b3"][2].ToString();
    //    TotalMasterDataChayi[id].Flag2 = ResultDict["b3"][3].ToString();
    //    TotalMasterDataChayi[id].Flag3 = ResultDict["b3"][4].ToString();
    //    TotalMasterDataChayi[id].Flag4 = ResultDict["b3"][5].ToString();
    //    TotalMasterDataChayi[id].Flag5 = ResultDict["b3"][6].ToString();
    //    TotalMasterDataChayi[id].Flag6 = ResultDict["b3"][7].ToString();
    //    TotalMasterDataChayi[id].Flag7 = ResultDict["b3"][8].ToString();

    //    TotalMasterDataChayi[id].Flag8 = ResultDict["b4"][2].ToString();
    //    TotalMasterDataChayi[id].Flag9 = ResultDict["b4"][3].ToString();
    //    TotalMasterDataChayi[id].Flag10 = ResultDict["b4"][4].ToString();
    //    TotalMasterDataChayi[id].Flag11 = ResultDict["b4"][5].ToString();
    //    TotalMasterDataChayi[id].Flag12 = ResultDict["b4"][6].ToString();
    //    TotalMasterDataChayi[id].Flag13 = ResultDict["b4"][7].ToString();
    //    TotalMasterDataChayi[id].Flag14 = ResultDict["b4"][8].ToString();

    //    TotalMasterDataChayi[id].PackVoltage = ((ResultDict["b1"][4] << 16) + (ResultDict["b1"][3] << 8) + ResultDict["b1"][2]).ToString();
    //    TotalMasterDataChayi[id].PackCurrent = ((ResultDict["b2"][4] << 16) + (ResultDict["b2"][3] << 8) + ResultDict["b2"][2]).ToString();

    //    TotalMasterDataChayi[id].SoC = ((ResultDict["b0"][3] << 8) + ResultDict["b0"][2]).ToString();
    //    TotalMasterDataChayi[id].SOH = ((ResultDict["b0"][5] << 8) + ResultDict["b0"][4]).ToString();

    //    // 解析 取string 轉 byte list
    //    List<bool> flag3List = BytesToList(Convert.ToInt32(TotalMasterDataChayi[id].Flag3));
    //    List<bool> flag6List = BytesToList(Convert.ToInt32(TotalMasterDataChayi[id].Flag6));
    //    TotalMasterDataChayi[id].COV = flag3List[2];
    //    TotalMasterDataChayi[id].CUV = flag3List[3];

    //    TotalMasterDataChayi[id].COT = flag6List[0];
    //    TotalMasterDataChayi[id].DOT = flag6List[1];

    //    // 紀錄全部FLAG
    //    string allflagStr = "";
    //    allflagStr += TotalMasterDataChayi[id].Flag1.ToString().PadLeft(2, '0');
    //    allflagStr += TotalMasterDataChayi[id].Flag2.ToString().PadLeft(2, '0');
    //    allflagStr += TotalMasterDataChayi[id].Flag3.ToString().PadLeft(2, '0');
    //    allflagStr += TotalMasterDataChayi[id].Flag4.ToString().PadLeft(2, '0');
    //    allflagStr += TotalMasterDataChayi[id].Flag5.ToString().PadLeft(2, '0');
    //    allflagStr += TotalMasterDataChayi[id].Flag6.ToString().PadLeft(2, '0');
    //    allflagStr += TotalMasterDataChayi[id].Flag7.ToString().PadLeft(2, '0');
    //    allflagStr += TotalMasterDataChayi[id].Flag8.ToString().PadLeft(2, '0');
    //    allflagStr += TotalMasterDataChayi[id].Flag9.ToString().PadLeft(2, '0');
    //    allflagStr += TotalMasterDataChayi[id].Flag10.ToString().PadLeft(2, '0');
    //    allflagStr += TotalMasterDataChayi[id].Flag11.ToString().PadLeft(2, '0');
    //    allflagStr += TotalMasterDataChayi[id].Flag12.ToString().PadLeft(2, '0');
    //    allflagStr += TotalMasterDataChayi[id].Flag13.ToString().PadLeft(2, '0');
    //    allflagStr += TotalMasterDataChayi[id].Flag14.ToString().PadLeft(2, '0');
    //    TotalMasterDataChayi[id].FlagALL = allflagStr;

    //    return true;
    //  }
    //  catch (Exception ex)
    //  {
    //    ex.ToString();
    //    return false;
    //  }
    //}





    /// <summary>
    /// 專門給5s2P的命令起頭
    /// </summary>
    private List<int> cmdIntList5s2p = new List<int>()
    {
      700,710,720,730,
      740,750,800,810,820
    };

    private List<int> cmdIntList22s2p = new List<int>()
    {
      700,710,720,730,
      740,800,810,820,
      830,840,850
    };

    /// <summary>
    /// 回傳輸入的命令應該是屬於哪個ID的電池盒,如果回傳是0代表不須理會(5s2p專用)
    /// </summary>
    /// <param name="cmdName"></param>
    /// <returns></returns>
    private int GetPackNum5s2p(string cmdName)
    {
      // 如果出現7a0 會出現異常
      try
      {
        int cmdNum = Convert.ToInt32(cmdName);

        foreach (int initNum in cmdIntList5s2p)
        {
          for (int packNum = 1; packNum < TotalPackNum; packNum++)
          {
            if (cmdNum == initNum + packNum)
              return packNum;
          }
        }
      }
      catch (Exception ex)
      {
        return 0;
      }

      // 如果都沒有,就回0
      return 0;
    }

    /// <summary>
    /// 回傳字串命令所代表的原始 Request命令,如果回傳是0代表不須理會
    /// </summary>
    /// <param name="cmdName"></param>
    /// <returns></returns>
    private int GetInitCmd5s2p(string cmdName)
    {
      // 出現7a0 直接回0
      try
      {
        int cmdNum = Convert.ToInt32(cmdName);
        foreach (int initNum in cmdIntList5s2p)
        {
          for (int packNum = 1; packNum < TotalPackNum; packNum++)
          {
            if (cmdNum == initNum + packNum)
              return initNum;
          }
        }
      }
      catch (Exception ex)
      {
        return 0;
      }

      // 如果都沒有,就回0
      return 0;
    }

    /// <summary>
    /// 給外部檢查使用
    /// </summary>
    public List<int> detectedIdList = new List<int>();

    public List<string> detectedIdList28 = new List<string>();

    /// <summary>
    /// 根據5s2p命令名稱去寫入到相對應的 PACK
    /// </summary>
    /// <param name="cmdName"></param>
    /// <param name="rstList"></param>
    /// <returns></returns>
    private bool UpdatePack5s2p(string cmdName, List<short> rstList)
    {
      // 空的或不是正確命令,裡面沒東西,直接離開
      int PackNum = GetPackNum5s2p(cmdName); // 所屬ID
      int InitCmd = GetInitCmd5s2p(cmdName); // 所屬原始命令

      if (PackNum == 0 || InitCmd == 0)
      {
        return false;
      }

      TotalMasterData[PackNum].Detected = true;
      // 將偵測到的PACK 放進去就是目前已可正常讀取的PACK
      if (detectedIdList.Contains(PackNum) != true)
        detectedIdList.Add(PackNum);
      switch (InitCmd)
      {
        case 700:
          TotalMasterData[PackNum].BatteryVoltage = ((short)(rstList[1] * 256 + rstList[2])).ToString();
          TotalMasterData[PackNum].BatteryCurrent = ((short)(rstList[3] * 256 + rstList[4])).ToString();
          TotalMasterData[PackNum].FCC = ((short)(rstList[5] * 256 + rstList[6])).ToString();
          TotalMasterData[PackNum].RC = ((short)(rstList[7] * 256 + rstList[8])).ToString();
          break;
        case 710:
          // data1
          TotalMasterData[PackNum].SaftyStatus1 = rstList[1].ToString();
          List<bool> tmpSafetyStatusData1List = BytesToList(rstList[1]);
          TotalMasterData[PackNum].OCDL = tmpSafetyStatusData1List[5];
          TotalMasterData[PackNum].COVL = tmpSafetyStatusData1List[4];
          TotalMasterData[PackNum].UTD = tmpSafetyStatusData1List[3];
          TotalMasterData[PackNum].UTC = tmpSafetyStatusData1List[2];
          TotalMasterData[PackNum].PCHGC = tmpSafetyStatusData1List[1];
          TotalMasterData[PackNum].CHGV = tmpSafetyStatusData1List[0];

          // data2
          TotalMasterData[PackNum].SaftyStatus2 = rstList[2].ToString();
          List<bool> tmpSafetyStatusData2List = BytesToList(rstList[2]);
          TotalMasterData[PackNum].CHGC = tmpSafetyStatusData2List[7];
          TotalMasterData[PackNum].OC = tmpSafetyStatusData2List[6];
          TotalMasterData[PackNum].CTO = tmpSafetyStatusData2List[4];
          TotalMasterData[PackNum].PTO = tmpSafetyStatusData2List[2];
          TotalMasterData[PackNum].OTF = tmpSafetyStatusData2List[0];

          // data3
          TotalMasterData[PackNum].SaftyStatus3 = rstList[3].ToString();
          List<bool> tmpSafetyStatusData3List = BytesToList(rstList[3]);
          TotalMasterData[PackNum].CUVC = tmpSafetyStatusData3List[6];
          TotalMasterData[PackNum].OTD = tmpSafetyStatusData3List[5];
          TotalMasterData[PackNum].OTC = tmpSafetyStatusData3List[4];
          TotalMasterData[PackNum].ASCDL = tmpSafetyStatusData3List[3];
          TotalMasterData[PackNum].ASCD = tmpSafetyStatusData3List[2];
          TotalMasterData[PackNum].ASCCL = tmpSafetyStatusData3List[1];
          TotalMasterData[PackNum].ASCC = tmpSafetyStatusData3List[0];

          // data4
          TotalMasterData[PackNum].SaftyStatus4 = rstList[4].ToString();
          List<bool> tmpSafetyStatusData4List = BytesToList(rstList[4]);
          TotalMasterData[PackNum].AOLDL = tmpSafetyStatusData4List[7];
          TotalMasterData[PackNum].AOLD = tmpSafetyStatusData4List[6];
          TotalMasterData[PackNum].OCD2 = tmpSafetyStatusData4List[5];
          TotalMasterData[PackNum].OCD1 = tmpSafetyStatusData4List[4];
          TotalMasterData[PackNum].OCC1 = tmpSafetyStatusData4List[2];
          TotalMasterData[PackNum].COV = tmpSafetyStatusData4List[1];
          TotalMasterData[PackNum].CUV = tmpSafetyStatusData4List[0];

          // battery status
          // data5
          TotalMasterData[PackNum].BatteryStatus5 = rstList[5].ToString();
          List<bool> tmpBatteryStatusData5List = BytesToList(rstList[5]);
          TotalMasterData[PackNum].OCA = tmpBatteryStatusData5List[7];
          TotalMasterData[PackNum].TCA = tmpBatteryStatusData5List[6];
          TotalMasterData[PackNum].OTA = tmpBatteryStatusData5List[4];
          TotalMasterData[PackNum].TDA = tmpBatteryStatusData5List[3];
          TotalMasterData[PackNum].RCA = tmpBatteryStatusData5List[1];
          TotalMasterData[PackNum].RTA = tmpBatteryStatusData5List[0];

          // data6
          TotalMasterData[PackNum].BatteryStatus6 = rstList[6].ToString();
          List<bool> tmpBatteryStatusData6List = BytesToList(rstList[6]);
          TotalMasterData[PackNum].INIT = tmpBatteryStatusData6List[7];
          TotalMasterData[PackNum].DSG = tmpBatteryStatusData6List[6];
          TotalMasterData[PackNum].FC = tmpBatteryStatusData6List[5];
          TotalMasterData[PackNum].FD = tmpBatteryStatusData6List[4];
          TotalMasterData[PackNum].EC3 = tmpBatteryStatusData6List[3];
          TotalMasterData[PackNum].EC2 = tmpBatteryStatusData6List[2];
          TotalMasterData[PackNum].EC1 = tmpBatteryStatusData6List[1];
          TotalMasterData[PackNum].EC0 = tmpBatteryStatusData6List[0];

          //// data7
          TotalMasterData[PackNum].RSoC = rstList[7].ToString();

          //// data8
          TotalMasterData[PackNum].SoH = rstList[8].ToString();
          break;
        case 720:
          TotalMasterData[PackNum].SystemCounter = (rstList[1] * 256 + rstList[2]).ToString();
          TotalMasterData[PackNum].FirmwareVersion = rstList[3].ToString() + " " + rstList[4].ToString();
          break;
        case 730:
          TotalMasterData[PackNum].Ts1Temp = ((short)(rstList[1] * 256 + rstList[2])).ToString();
          TotalMasterData[PackNum].Ts2Temp = ((short)(rstList[3] * 256 + rstList[4])).ToString();
          TotalMasterData[PackNum].Ts3Temp = ((short)(rstList[5] * 256 + rstList[6])).ToString();
          TotalMasterData[PackNum].Fet = ((short)(rstList[7] * 256 + rstList[8])).ToString();
          break;
        case 740:
          TotalMasterData[PackNum].AnalogTemp1 = ((short)(rstList[1] * 256 + rstList[2])).ToString();
          TotalMasterData[PackNum].AnalogTemp2 = ((short)(rstList[3] * 256 + rstList[4])).ToString();
          TotalMasterData[PackNum].AnalogTemp3 = ((short)(rstList[5] * 256 + rstList[6])).ToString();
          TotalMasterData[PackNum].AnalogTemp4 = ((short)(rstList[7] * 256 + rstList[8])).ToString();
          break;
        case 750:
          TotalMasterData[PackNum].DipSWStatus = rstList[1].ToString();
          TotalMasterData[PackNum].ID = (rstList[1] & 0xF).ToString();
          TotalMasterData[PackNum].Mode = ((rstList[1] >> 4) & 0xF).ToString();
          break;
        case 800:
          TotalMasterData[PackNum].CellVoltage1 = ((short)(rstList[1] * 256 + rstList[2])).ToString();
          TotalMasterData[PackNum].CellVoltage2 = ((short)(rstList[3] * 256 + rstList[4])).ToString();
          TotalMasterData[PackNum].CellVoltage3 = ((short)(rstList[5] * 256 + rstList[6])).ToString();
          TotalMasterData[PackNum].CellVoltage4 = ((short)(rstList[7] * 256 + rstList[8])).ToString();

          break;
        case 810:
          TotalMasterData[PackNum].CellVoltage5 = ((short)(rstList[1] * 256 + rstList[2])).ToString();
          break;
        case 820:
          TotalMasterData[PackNum].swid = (rstList[1]).ToString();
          break;
        default:
          break;
      }

      return true;
    }

    /// <summary>
    /// 根據22s2p命令名稱去寫入到相對應的 PACK
    /// </summary>
    /// <param name="cmdName"></param>
    /// <param name="rstList"></param>
    /// <returns></returns>
    private bool UpdatePack22s2p(string cmdName, List<short> rstList)
    {
      // 空的或不是正確命令,裡面沒東西,直接離開
      int PackNum = GetPackNum5s2p(cmdName); // 所屬ID
      int InitCmd = GetInitCmd5s2p(cmdName); // 所屬原始命令

      if (PackNum == 0 || InitCmd == 0)
      {
        return false;
      }

      TotalMasterData[PackNum].Detected = true;
      switch (InitCmd)
      {
        case 700:
          TotalMasterData[PackNum].BatteryVoltage = ((short)(rstList[1] * 256 + rstList[2])).ToString();
          TotalMasterData[PackNum].BatteryCurrent = ((short)(rstList[3] * 256 + rstList[4])).ToString();
          TotalMasterData[PackNum].FCC = ((short)(rstList[5] * 256 + rstList[6])).ToString();
          TotalMasterData[PackNum].RC = ((short)(rstList[7] * 256 + rstList[8])).ToString();
          break;
        case 710:
          // data1
          TotalMasterData[PackNum].SaftyStatus1 = rstList[1].ToString();
          List<bool> tmpSafetyStatusData1List = BytesToList(rstList[1]);
          TotalMasterData[PackNum].OCDL = tmpSafetyStatusData1List[5];
          TotalMasterData[PackNum].COVL = tmpSafetyStatusData1List[4];
          TotalMasterData[PackNum].UTD = tmpSafetyStatusData1List[3];
          TotalMasterData[PackNum].UTC = tmpSafetyStatusData1List[2];
          TotalMasterData[PackNum].PCHGC = tmpSafetyStatusData1List[1];
          TotalMasterData[PackNum].CHGV = tmpSafetyStatusData1List[0];

          // data2
          TotalMasterData[PackNum].SaftyStatus2 = rstList[2].ToString();
          List<bool> tmpSafetyStatusData2List = BytesToList(rstList[2]);
          TotalMasterData[PackNum].CHGC = tmpSafetyStatusData2List[7];
          TotalMasterData[PackNum].OC = tmpSafetyStatusData2List[6];
          TotalMasterData[PackNum].CTO = tmpSafetyStatusData2List[4];
          TotalMasterData[PackNum].PTO = tmpSafetyStatusData2List[2];
          TotalMasterData[PackNum].OTF = tmpSafetyStatusData2List[0];

          // data3
          TotalMasterData[PackNum].SaftyStatus3 = rstList[3].ToString();
          List<bool> tmpSafetyStatusData3List = BytesToList(rstList[3]);
          TotalMasterData[PackNum].CUVC = tmpSafetyStatusData3List[6];
          TotalMasterData[PackNum].OTD = tmpSafetyStatusData3List[5];
          TotalMasterData[PackNum].OTC = tmpSafetyStatusData3List[4];
          TotalMasterData[PackNum].ASCDL = tmpSafetyStatusData3List[3];
          TotalMasterData[PackNum].ASCD = tmpSafetyStatusData3List[2];
          TotalMasterData[PackNum].ASCCL = tmpSafetyStatusData3List[1];
          TotalMasterData[PackNum].ASCC = tmpSafetyStatusData3List[0];

          // data4
          TotalMasterData[PackNum].SaftyStatus4 = rstList[4].ToString();
          List<bool> tmpSafetyStatusData4List = BytesToList(rstList[4]);
          TotalMasterData[PackNum].AOLDL = tmpSafetyStatusData4List[7];
          TotalMasterData[PackNum].AOLD = tmpSafetyStatusData4List[6];
          TotalMasterData[PackNum].OCD2 = tmpSafetyStatusData4List[5];
          TotalMasterData[PackNum].OCD1 = tmpSafetyStatusData4List[4];
          TotalMasterData[PackNum].OCC1 = tmpSafetyStatusData4List[2];
          TotalMasterData[PackNum].COV = tmpSafetyStatusData4List[1];
          TotalMasterData[PackNum].CUV = tmpSafetyStatusData4List[0];

          // battery status
          // data5
          TotalMasterData[PackNum].BatteryStatus5 = rstList[5].ToString();
          List<bool> tmpBatteryStatusData5List = BytesToList(rstList[5]);
          TotalMasterData[PackNum].OCA = tmpBatteryStatusData5List[7];
          TotalMasterData[PackNum].TCA = tmpBatteryStatusData5List[6];
          TotalMasterData[PackNum].OTA = tmpBatteryStatusData5List[4];
          TotalMasterData[PackNum].TDA = tmpBatteryStatusData5List[3];
          TotalMasterData[PackNum].RCA = tmpBatteryStatusData5List[1];
          TotalMasterData[PackNum].RTA = tmpBatteryStatusData5List[0];

          // data6
          TotalMasterData[PackNum].BatteryStatus6 = rstList[6].ToString();
          List<bool> tmpBatteryStatusData6List = BytesToList(rstList[6]);
          TotalMasterData[PackNum].INIT = tmpBatteryStatusData6List[7];
          TotalMasterData[PackNum].DSG = tmpBatteryStatusData6List[6];
          TotalMasterData[PackNum].FC = tmpBatteryStatusData6List[5];
          TotalMasterData[PackNum].FD = tmpBatteryStatusData6List[4];
          TotalMasterData[PackNum].EC3 = tmpBatteryStatusData6List[3];
          TotalMasterData[PackNum].EC2 = tmpBatteryStatusData6List[2];
          TotalMasterData[PackNum].EC1 = tmpBatteryStatusData6List[1];
          TotalMasterData[PackNum].EC0 = tmpBatteryStatusData6List[0];

          //// data7
          TotalMasterData[PackNum].RSoC = rstList[7].ToString();

          //// data8
          TotalMasterData[PackNum].SoH = rstList[8].ToString();
          break;
        case 720:
          TotalMasterData[PackNum].SystemCounter = (rstList[1] * 256 + rstList[2]).ToString();
          TotalMasterData[PackNum].FirmwareVersion = rstList[3].ToString() + " " + rstList[4].ToString();
          break;
        case 730:
          TotalMasterData[PackNum].Ts1Temp = ((short)(rstList[1] * 256 + rstList[2])).ToString();
          TotalMasterData[PackNum].Ts2Temp = ((short)(rstList[3] * 256 + rstList[4])).ToString();
          TotalMasterData[PackNum].Ts3Temp = ((short)(rstList[5] * 256 + rstList[6])).ToString();
          TotalMasterData[PackNum].Fet = ((short)(rstList[7] * 256 + rstList[8])).ToString();
          break;
        case 740:
          TotalMasterData[PackNum].AnalogTemp1 = ((short)(rstList[1] * 256 + rstList[2])).ToString();
          TotalMasterData[PackNum].AnalogTemp2 = ((short)(rstList[3] * 256 + rstList[4])).ToString();
          TotalMasterData[PackNum].AnalogTemp3 = ((short)(rstList[5] * 256 + rstList[6])).ToString();
          TotalMasterData[PackNum].AnalogTemp4 = ((short)(rstList[7] * 256 + rstList[8])).ToString();
          break;
        case 750:
          TotalMasterData[PackNum].DipSWStatus = rstList[1].ToString();
          TotalMasterData[PackNum].ID = (rstList[1] & 0xF).ToString();
          TotalMasterData[PackNum].Mode = ((rstList[1] >> 4) & 0xF).ToString();
          break;
        case 800:
          TotalMasterData[PackNum].CellVoltage1 = ((short)(rstList[1] * 256 + rstList[2])).ToString();
          TotalMasterData[PackNum].CellVoltage2 = ((short)(rstList[3] * 256 + rstList[4])).ToString();
          TotalMasterData[PackNum].CellVoltage3 = ((short)(rstList[5] * 256 + rstList[6])).ToString();
          TotalMasterData[PackNum].CellVoltage4 = ((short)(rstList[7] * 256 + rstList[8])).ToString();

          break;
        case 810:
          TotalMasterData[PackNum].CellVoltage5 = ((short)(rstList[1] * 256 + rstList[2])).ToString();
          break;
        default:
          break;
      }

      return true;
    }


    unsafe public UInt32 RcvMaster5s2pINT()
    {
      // 回傳碼Str
      //string feedbackFst = "";

      UInt32 res = 0;
      int count = 10;
      while (res == 0 && count > 0)
      {
        res = VCI_Receive(m_devtype, m_devind, m_canind, ref m_recobj[0], 1000, 100);
        usdelay.TimeDelayus(500);
        count--;
      }
      //Thread.Sleep(10);

      if (res == 0xFFFFFFFF || res == 0)
        return 0; //当设备未初始化时，返回0xFFFFFFFF，不进行列表显示。
      String str = "";

      List<short> resultList = new List<short>();
      Dictionary<string, List<short>> ResultDict = new Dictionary<string, List<short>>();
      string frameRcvStrId = "";

      for (UInt32 i = 0; i < res; i++)
      {
        resultList = new List<short>();
        str = "接收到数据: ";
        str += "  帧ID:0x" + Convert.ToString(m_recobj[i].ID, 16);
        frameRcvStrId = Convert.ToString(m_recobj[i].ID, 16);

        str += "  帧格式:";
        if (m_recobj[i].RemoteFlag == 0)
          str += "数据帧 ";
        else
          str += "远程帧 ";
        if (m_recobj[i].ExternFlag == 0)
          str += "标准帧 ";
        else
          str += "扩展帧 ";
        Int32 tmpVal = Convert.ToInt32(m_recobj[i].ID);

        resultList.Add((short)tmpVal);
        if (m_recobj[i].RemoteFlag == 0)
        {
          str += "数据: ";
          byte len = (byte)(m_recobj[i].DataLen % 9);
          byte j = 0;
          fixed (VCI_CAN_OBJ* m_recobj1 = &m_recobj[i])
          {
            if (j++ < len)
              str += " " + System.Convert.ToString(m_recobj1->Data[0], 16);
            if (j++ < len)
              str += " " + System.Convert.ToString(m_recobj1->Data[1], 16);
            if (j++ < len)
              str += " " + System.Convert.ToString(m_recobj1->Data[2], 16);
            if (j++ < len)
              str += " " + System.Convert.ToString(m_recobj1->Data[3], 16);
            if (j++ < len)
              str += " " + System.Convert.ToString(m_recobj1->Data[4], 16);
            if (j++ < len)
              str += " " + System.Convert.ToString(m_recobj1->Data[5], 16);
            if (j++ < len)
              str += " " + System.Convert.ToString(m_recobj1->Data[6], 16);
            if (j++ < len)
              str += " " + System.Convert.ToString(m_recobj1->Data[7], 16);

            resultList.Add(m_recobj1->Data[0]);
            resultList.Add(m_recobj1->Data[1]);
            resultList.Add(m_recobj1->Data[2]);
            resultList.Add(m_recobj1->Data[3]);
            resultList.Add(m_recobj1->Data[4]);
            resultList.Add(m_recobj1->Data[5]);
            resultList.Add(m_recobj1->Data[6]);
            resultList.Add(m_recobj1->Data[7]);
          }
          // 更新變數
          UpdatePack5s2p(frameRcvStrId, resultList);
        }

      }

      return res;
    }


    /// <summary>
    /// 確認MODE模式每個都一樣(預設為true)
    /// </summary>
    public bool verifyModeAllConsistance = true;

    /// <summary>
    /// 確認id與MODE模式相符(預設為true)
    /// </summary>
    public bool verifyidNumCorrect = true;

    /// <summary>
    /// single command delay(ms) default 10ms
    /// </summary>
    //private uint _cmdDelay = 80; canbus2 比較慢
    private uint _cmdDelay = 200;
    /// <summary>
    /// set single command delay(ms)
    /// </summary>
    public uint cmdDelay
    {
      set { _cmdDelay = value; }
    }

    /// <summary>
    /// 回傳收到多少筆結果
    /// </summary>
    /// <returns></returns>
    public int GetAllPara5s2pNew()
    {
      //ConcurrentDictionary<int, BatteryCanbus2P5SPara> resultParaDict = new ConcurrentDictionary<int, BatteryCanbus2P5SPara>();

      BatteryCanbus2P5SPara tmpPara = new BatteryCanbus2P5SPara();
      singleCmdNowait("0x0700");
      singleCmdNowait("0x0710");
      singleCmdNowait("0x0720");
      singleCmdNowait("0x0730");
      singleCmdNowait("0x0740");
      singleCmdNowait("0x0750");
      singleCmdNowait("0x0800");
      singleCmdNowait("0x0810");
      singleCmdNowait("0x0820");
      //usdelay.TimeDelayus(5000);
      ConcurrentDictionary<int, List<short>> tmpRstDict = RcvData();

      // 重新建立資料結構
      // tmpRstDict
      // KEY =>  frameRcvStrId (這個數字包含了ID站名以及命令代號)，之後再去解析
      // VALUE => 8bit 的List，其中第0個元素是id

      try
      {
        // 找出所有的SlaveID
        List<int> SlaveIdList = new List<int>();
        foreach (var k in tmpRstDict.Keys)
        {
          int slaveid = tmpRstDict[k][0];
          if (SlaveIdList.Contains(slaveid) != true)
            SlaveIdList.Add(slaveid);// 找出所有的SlaveID
        }

        // 檢查結果如果不是正確回應的封包這筆乾脆不要轉換
        int normalRstCount = SlaveIdList.Count * 8;
        if (tmpRstDict.Count != normalRstCount)
        {
          return 0;
        }

        // 重建tmpResultDict0700 -- tmpResultDict0820
        ConcurrentDictionary<int, List<short>> tmpResultDict0700 = new ConcurrentDictionary<int, List<short>>();
        ConcurrentDictionary<int, List<short>> tmpResultDict0710 = new ConcurrentDictionary<int, List<short>>();
        ConcurrentDictionary<int, List<short>> tmpResultDict0720 = new ConcurrentDictionary<int, List<short>>();
        ConcurrentDictionary<int, List<short>> tmpResultDict0730 = new ConcurrentDictionary<int, List<short>>();
        ConcurrentDictionary<int, List<short>> tmpResultDict0740 = new ConcurrentDictionary<int, List<short>>();
        ConcurrentDictionary<int, List<short>> tmpResultDict0750 = new ConcurrentDictionary<int, List<short>>();
        ConcurrentDictionary<int, List<short>> tmpResultDict0800 = new ConcurrentDictionary<int, List<short>>();
        ConcurrentDictionary<int, List<short>> tmpResultDict0810 = new ConcurrentDictionary<int, List<short>>();
        ConcurrentDictionary<int, List<short>> tmpResultDict0820 = new ConcurrentDictionary<int, List<short>>();

        // 
        foreach (var k in tmpRstDict.Keys)
        {
          // k 的數值為 701 702 703 -- 709
          //           711 712 713 -- 719
          //       ... 811 812 813 -- 819
          int slaveid = tmpRstDict[k][0];

          int cmdNum1 = (k / 100) % 10;   // 取百位數 7 or 8

          int cmdNum2 = (k % 100) / 10; // 取10位數 0 1 2 3 4 5

          if (cmdNum1 == 7 && cmdNum2 == 0)
          {
            tmpResultDict0700.TryAdd(slaveid, tmpRstDict[k]);
            continue;
          }

          if (cmdNum1 == 7 && cmdNum2 == 1)
          {
            tmpResultDict0710.TryAdd(slaveid, tmpRstDict[k]);
            continue;
          }

          if (cmdNum1 == 7 && cmdNum2 == 2)
          {
            tmpResultDict0720.TryAdd(slaveid, tmpRstDict[k]);
            continue;
          }

          if (cmdNum1 == 7 && cmdNum2 == 3)
          {
            tmpResultDict0730.TryAdd(slaveid, tmpRstDict[k]);
            continue;
          }

          if (cmdNum1 == 7 && cmdNum2 == 4)
          {
            tmpResultDict0740.TryAdd(slaveid, tmpRstDict[k]);
            continue;
          }

          if (cmdNum1 == 7 && cmdNum2 == 5)
          {
            tmpResultDict0750.TryAdd(slaveid, tmpRstDict[k]);
            continue;
          }

          if (cmdNum1 == 8 && cmdNum2 == 0)
          {
            tmpResultDict0800.TryAdd(slaveid, tmpRstDict[k]);
            continue;
          }

          if (cmdNum1 == 8 && cmdNum2 == 1)
          {
            tmpResultDict0810.TryAdd(slaveid, tmpRstDict[k]);
            continue;
          }

          if (cmdNum1 == 8 && cmdNum2 == 2)
          {
            tmpResultDict0820.TryAdd(slaveid, tmpRstDict[k]);
            continue;
          }
        }


        detectedIdList = new List<int>();
        // 收到哪邊做到哪邊
        // 每段檢查 700 710 720 730 740 750 800 810 820
        foreach (var id in SlaveIdList)
        {
          //tmpPara = new BatteryCanbus2P5SPara();
          tmpPara = TotalMasterData[id];
          tmpPara.Detected = true;
          detectedIdList.Add(id);
          if (tmpResultDict0700.ContainsKey(id))
          {
            // 0700
            tmpPara.BatteryVoltage = ((short)(tmpResultDict0700[id][1] * 256 + tmpResultDict0700[id][2])).ToString();
            tmpPara.BatteryCurrent = ((short)(tmpResultDict0700[id][3] * 256 + tmpResultDict0700[id][4])).ToString();

            tmpPara.FCC = ((short)(tmpResultDict0700[id][5] * 256 + tmpResultDict0700[id][6])).ToString();
            tmpPara.RC = ((short)(tmpResultDict0700[id][7] * 256 + tmpResultDict0700[id][8])).ToString();

          }

          if (tmpResultDict0710.ContainsKey(id))
          {
            // 0710
            // safety status
            // data1
            // List[0]是LSB  List[7] 是MSB
            tmpPara.SaftyStatus1 = tmpResultDict0710[id][1].ToString();
            List<bool> tmpSafetyStatusData1List = BytesToList(tmpResultDict0710[id][1]);
            tmpPara.OCDL = tmpSafetyStatusData1List[5];
            tmpPara.COVL = tmpSafetyStatusData1List[4];
            tmpPara.UTD = tmpSafetyStatusData1List[3];
            tmpPara.UTC = tmpSafetyStatusData1List[2];
            tmpPara.PCHGC = tmpSafetyStatusData1List[1];
            tmpPara.CHGV = tmpSafetyStatusData1List[0];

            // data2
            tmpPara.SaftyStatus2 = tmpResultDict0710[id][2].ToString();
            List<bool> tmpSafetyStatusData2List = BytesToList(tmpResultDict0710[id][2]);
            tmpPara.CHGC = tmpSafetyStatusData2List[7];
            tmpPara.OC = tmpSafetyStatusData2List[6];
            tmpPara.CTO = tmpSafetyStatusData2List[4];
            tmpPara.PTO = tmpSafetyStatusData2List[2];
            tmpPara.OTF = tmpSafetyStatusData2List[0];

            // data3
            tmpPara.SaftyStatus3 = tmpResultDict0710[id][3].ToString();
            List<bool> tmpSafetyStatusData3List = BytesToList(tmpResultDict0710[id][3]);
            tmpPara.CUVC = tmpSafetyStatusData3List[6];
            tmpPara.OTD = tmpSafetyStatusData3List[5];
            tmpPara.OTC = tmpSafetyStatusData3List[4];
            tmpPara.ASCDL = tmpSafetyStatusData3List[3];
            tmpPara.ASCD = tmpSafetyStatusData3List[2];
            tmpPara.ASCCL = tmpSafetyStatusData3List[1];
            tmpPara.ASCC = tmpSafetyStatusData3List[0];

            // data4
            tmpPara.SaftyStatus4 = tmpResultDict0710[id][4].ToString();
            List<bool> tmpSafetyStatusData4List = BytesToList(tmpResultDict0710[id][4]);
            tmpPara.AOLDL = tmpSafetyStatusData4List[7];
            tmpPara.AOLD = tmpSafetyStatusData4List[6];
            tmpPara.OCD2 = tmpSafetyStatusData4List[5];
            tmpPara.OCD1 = tmpSafetyStatusData4List[4];
            tmpPara.OCC1 = tmpSafetyStatusData4List[2];
            tmpPara.COV = tmpSafetyStatusData4List[1];
            tmpPara.CUV = tmpSafetyStatusData4List[0];

            // battery status
            // data5

            tmpPara.BatteryStatus5 = tmpResultDict0710[id][5].ToString();
            List<bool> tmpBatteryStatusData5List = BytesToList(tmpResultDict0710[id][5]);
            tmpPara.OCA = tmpBatteryStatusData5List[7];
            tmpPara.TCA = tmpBatteryStatusData5List[6];
            tmpPara.OTA = tmpBatteryStatusData5List[4];
            tmpPara.TDA = tmpBatteryStatusData5List[3];
            tmpPara.RCA = tmpBatteryStatusData5List[1];
            tmpPara.RTA = tmpBatteryStatusData5List[0];

            // data6
            tmpPara.BatteryStatus6 = tmpResultDict0710[id][6].ToString();
            List<bool> tmpBatteryStatusData6List = BytesToList(tmpResultDict0710[id][6]);
            tmpPara.INIT = tmpBatteryStatusData6List[7];
            tmpPara.DSG = tmpBatteryStatusData6List[6];
            tmpPara.FC = tmpBatteryStatusData6List[5];
            tmpPara.FD = tmpBatteryStatusData6List[4];
            tmpPara.EC3 = tmpBatteryStatusData6List[3];
            tmpPara.EC2 = tmpBatteryStatusData6List[2];
            tmpPara.EC1 = tmpBatteryStatusData6List[1];
            tmpPara.EC0 = tmpBatteryStatusData6List[0];

            //// data7
            tmpPara.RSoC = tmpResultDict0710[id][7].ToString();

            //// data8
            tmpPara.SoH = tmpResultDict0710[id][8].ToString();
          }

          if (tmpResultDict0720.ContainsKey(id))
          {
            //// 0720
            tmpPara.SystemCounter = (tmpResultDict0720[id][1] * 256 + tmpResultDict0720[id][2]).ToString();
            tmpPara.FirmwareVersion = tmpResultDict0720[id][3].ToString() + " " + tmpResultDict0720[id][4].ToString();

          }
          if (tmpResultDict0730.ContainsKey(id))
          {
            //// 0730
            tmpPara.Ts1Temp = ((short)(tmpResultDict0730[id][1] * 256 + tmpResultDict0730[id][2])).ToString();
            tmpPara.Ts2Temp = ((short)(tmpResultDict0730[id][3] * 256 + tmpResultDict0730[id][4])).ToString();
            tmpPara.Ts3Temp = ((short)(tmpResultDict0730[id][5] * 256 + tmpResultDict0730[id][6])).ToString();
            tmpPara.Fet = ((short)(tmpResultDict0730[id][7] * 256 + tmpResultDict0730[id][8])).ToString();

          }
          if (tmpResultDict0740.ContainsKey(id))
          {
            //// 0740
            tmpPara.AnalogTemp1 = ((short)(tmpResultDict0740[id][1] * 256 + tmpResultDict0740[id][2])).ToString();
            tmpPara.AnalogTemp2 = ((short)(tmpResultDict0740[id][3] * 256 + tmpResultDict0740[id][4])).ToString();
            tmpPara.AnalogTemp3 = ((short)(tmpResultDict0740[id][5] * 256 + tmpResultDict0740[id][6])).ToString();
            tmpPara.AnalogTemp4 = ((short)(tmpResultDict0740[id][7] * 256 + tmpResultDict0740[id][8])).ToString();
          }

          if (tmpResultDict0750.ContainsKey(id))
          {
            // 0750
            // data1
            //List<int> tmpDipSwData1List = BytesToList(tmpResultDict0750[id][1]);

            tmpPara.DipSWStatus = tmpResultDict0750[id][1].ToString();
            tmpPara.ID = (tmpResultDict0750[id][1] & 0xF).ToString();
            tmpPara.Mode = ((tmpResultDict0750[id][1] >> 4) & 0xF).ToString();
          }


          if (tmpResultDict0800.ContainsKey(id))
          {
            //// 0800
            tmpPara.CellVoltage1 = ((short)(tmpResultDict0800[id][1] * 256 + tmpResultDict0800[id][2])).ToString();
            tmpPara.CellVoltage2 = ((short)(tmpResultDict0800[id][3] * 256 + tmpResultDict0800[id][4])).ToString();
            tmpPara.CellVoltage3 = ((short)(tmpResultDict0800[id][5] * 256 + tmpResultDict0800[id][6])).ToString();
            tmpPara.CellVoltage4 = ((short)(tmpResultDict0800[id][7] * 256 + tmpResultDict0800[id][8])).ToString();

          }

          if (tmpResultDict0810.ContainsKey(id))
          {
            //// 0810
            tmpPara.CellVoltage5 = ((short)(tmpResultDict0810[id][1] * 256 + tmpResultDict0810[id][2])).ToString();
          }

          if (tmpResultDict0820.ContainsKey(id))
          {
            //// 0820
            tmpPara.swid = ((short)tmpResultDict0820[id][1]).ToString();
          }
          //resultParaDict.TryAdd(id, tmpPara);
          TotalMasterData[id] = tmpPara;
        }
      }
      catch (Exception ex)
      {
        return 0;
      }

      return tmpRstDict.Count;
    }

    public int GetAllPara28s2pNew()
    {
      //ConcurrentDictionary<int, BatteryCanbus2P5SPara> resultParaDict = new ConcurrentDictionary<int, BatteryCanbus2P5SPara>();

      // 2025/05/09 測試發現問一個命令約800 ms 左右會回來，如果很頻繁問的話他命令會卡住回不來
      // 經Kevin 確認無誤，一個命令通常需1s左右，待韌體改善
      // 這邊為了能正常讀取命令，每段命令延遲 1s 後再發送
      BatteryCanbus2P28SPara tmpPara = new BatteryCanbus2P28SPara();
      singleCmdNowait("0x0700");
      Thread.Sleep(1000);
      singleCmdNowait("0x0710");
      Thread.Sleep(1000);
      singleCmdNowait("0x0720");
      Thread.Sleep(1000);
      singleCmdNowait("0x0730");
      Thread.Sleep(1000);
      singleCmdNowait("0x0740");
      Thread.Sleep(1000);
      singleCmdNowait("0x0800");
      Thread.Sleep(1000);
      singleCmdNowait("0x0810");
      Thread.Sleep(1000);
      singleCmdNowait("0x0820");
      Thread.Sleep(1000);
      singleCmdNowait("0x0830");
      Thread.Sleep(1000);
      singleCmdNowait("0x0840");
      Thread.Sleep(1000);
      singleCmdNowait("0x0850");
      Thread.Sleep(1000);
      singleCmdNowait("0x0860");
      Thread.Sleep(1000);
      singleCmdNowait("0x0870");
      Thread.Sleep(1000);
      singleCmdNowait("0x0880");
      Thread.Sleep(1000);
      singleCmdNowait("0x0890");
      Thread.Sleep(1000);
      singleCmdNowait("0x08A0");
      Thread.Sleep(1000);
      singleCmdNowait("0x08B0");
      Thread.Sleep(1000);
      //usdelay.TimeDelayus(5000);
      ConcurrentDictionary<string, List<short>> tmpRstDict = RcvData2();

      // 重新建立資料結構
      // 命令有超過10以上，所以更改取值方式 2025/05/09
      // tmpRstDict
      // KEY =>  frameRcvStrId (這個數字包含了ID站名以及命令代號)，之後再去解析
      // VALUE => 8bit 的List，其中第0個元素是id

      try
      {
        // 找出所有的SlaveID
        List<int> SlaveIdList = new List<int>();
        foreach (var k in tmpRstDict.Keys)
        {
          int slaveid = tmpRstDict[k][0];
          if (SlaveIdList.Contains(slaveid) != true)
            SlaveIdList.Add(slaveid);// 找出所有的SlaveID
        }

        // 檢查結果如果不是正確回應的封包這筆乾脆不要轉換
        int normalRstCount = SlaveIdList.Count * 8;
        if (tmpRstDict.Count != normalRstCount)
        {
          return 0;
        }

        // 重建tmpResultDict0700 -- tmpResultDict08B0
        ConcurrentDictionary<string, List<short>> tmpResultDict0700 = new ConcurrentDictionary<string, List<short>>();
        ConcurrentDictionary<string, List<short>> tmpResultDict0710 = new ConcurrentDictionary<string, List<short>>();
        ConcurrentDictionary<string, List<short>> tmpResultDict0720 = new ConcurrentDictionary<string, List<short>>();
        ConcurrentDictionary<string, List<short>> tmpResultDict0730 = new ConcurrentDictionary<string, List<short>>();
        ConcurrentDictionary<string, List<short>> tmpResultDict0740 = new ConcurrentDictionary<string, List<short>>();
        ConcurrentDictionary<string, List<short>> tmpResultDict0800 = new ConcurrentDictionary<string, List<short>>();
        ConcurrentDictionary<string, List<short>> tmpResultDict0810 = new ConcurrentDictionary<string, List<short>>();
        ConcurrentDictionary<string, List<short>> tmpResultDict0820 = new ConcurrentDictionary<string, List<short>>();
        ConcurrentDictionary<string, List<short>> tmpResultDict0830 = new ConcurrentDictionary<string, List<short>>();
        ConcurrentDictionary<string, List<short>> tmpResultDict0840 = new ConcurrentDictionary<string, List<short>>();
        ConcurrentDictionary<string, List<short>> tmpResultDict0850 = new ConcurrentDictionary<string, List<short>>();
        ConcurrentDictionary<string, List<short>> tmpResultDict0860 = new ConcurrentDictionary<string, List<short>>();
        ConcurrentDictionary<string, List<short>> tmpResultDict0870 = new ConcurrentDictionary<string, List<short>>();
        ConcurrentDictionary<string, List<short>> tmpResultDict0880 = new ConcurrentDictionary<string, List<short>>();
        ConcurrentDictionary<string, List<short>> tmpResultDict0890 = new ConcurrentDictionary<string, List<short>>();
        ConcurrentDictionary<string, List<short>> tmpResultDict08A0 = new ConcurrentDictionary<string, List<short>>();
        ConcurrentDictionary<string, List<short>> tmpResultDict08B0 = new ConcurrentDictionary<string, List<short>>();

        // 
        foreach (var k in tmpRstDict.Keys)
        {
          // k 的數值為 701 702 703 -- 709
          //           711 712 713 -- 719
          //       ... 811 812 813 -- 819
          string slaveid = tmpRstDict[k][0].ToString();

          //int cmdNum1 = (k / 100) % 10;   // 取百位數 7 or 8
          string cmdNum1 = k.Substring(0 ,1);
          //int cmdNum2 = (k % 100) / 10; // 取10位數 0 1 2 3 4 5
          string cmdNum2 = k.Substring(1, 1);
          if (cmdNum1 == "7" && cmdNum2 == "0")
          {
            tmpResultDict0700.TryAdd(slaveid, tmpRstDict[k]);
            continue;
          }

          if (cmdNum1 == "7" && cmdNum2 == "1")
          {
            tmpResultDict0710.TryAdd(slaveid, tmpRstDict[k]);
            continue;
          }

          if (cmdNum1 == "7" && cmdNum2 == "2")
          {
            tmpResultDict0720.TryAdd(slaveid, tmpRstDict[k]);
            continue;
          }

          if (cmdNum1 == "7" && cmdNum2 == "3")
          {
            tmpResultDict0730.TryAdd(slaveid, tmpRstDict[k]);
            continue;
          }

          if (cmdNum1 == "7" && cmdNum2 == "4")
          {
            tmpResultDict0740.TryAdd(slaveid, tmpRstDict[k]);
            continue;
          }

          if (cmdNum1 == "8" && cmdNum2 == "0")
          {
            tmpResultDict0800.TryAdd(slaveid, tmpRstDict[k]);
            continue;
          }

          if (cmdNum1 == "8" && cmdNum2 == "1")
          {
            tmpResultDict0810.TryAdd(slaveid, tmpRstDict[k]);
            continue;
          }

          if (cmdNum1 == "8" && cmdNum2 == "2")
          {
            tmpResultDict0820.TryAdd(slaveid, tmpRstDict[k]);
            continue;
          }

          if (cmdNum1 == "8" && cmdNum2 == "3")
          {
            tmpResultDict0830.TryAdd(slaveid, tmpRstDict[k]);
            continue;
          }

          if (cmdNum1 == "8" && cmdNum2 == "4")
          {
            tmpResultDict0840.TryAdd(slaveid, tmpRstDict[k]);
            continue;
          }

          if (cmdNum1 == "8" && cmdNum2 == "5")
          {
            tmpResultDict0850.TryAdd(slaveid, tmpRstDict[k]);
            continue;
          }

          if (cmdNum1 == "8" && cmdNum2 == "6")
          {
            tmpResultDict0860.TryAdd(slaveid, tmpRstDict[k]);
            continue;
          }

          if (cmdNum1 == "8" && cmdNum2 == "7")
          {
            tmpResultDict0870.TryAdd(slaveid, tmpRstDict[k]);
            continue;
          }

          if (cmdNum1 == "8" && cmdNum2 == "8")
          {
            tmpResultDict0880.TryAdd(slaveid, tmpRstDict[k]);
            continue;
          }

          if (cmdNum1 == "8" && cmdNum2 == "9")
          {
            tmpResultDict0890.TryAdd(slaveid, tmpRstDict[k]);
            continue;
          }

          if (cmdNum1 == "8" && cmdNum2 == "A")
          {
            tmpResultDict08A0.TryAdd(slaveid, tmpRstDict[k]);
            continue;
          }

          if (cmdNum1 == "8" && cmdNum2 == "B")
          {
            tmpResultDict08B0.TryAdd(slaveid, tmpRstDict[k]);
            continue;
          }
        }


        detectedIdList28 = new List<string>();
        // 收到哪邊做到哪邊
        // 每段檢查 700 710 720 730 740 750 800 810 820
        foreach (var idNum in SlaveIdList)
        {
          string id = idNum.ToString();
          //tmpPara = new BatteryCanbus2P5SPara();
          tmpPara = TotalMasterData28[id];
          tmpPara.Detected = true;
          detectedIdList28.Add(id);
          if (tmpResultDict0700.ContainsKey(id))
          {
            // 0700
            tmpPara.BatteryLoVoltage = ((short)(tmpResultDict0700[id][1] * 256 + tmpResultDict0700[id][2])).ToString();
            tmpPara.BatteryHiVoltage = ((short)(tmpResultDict0700[id][3] * 256 + tmpResultDict0700[id][4])).ToString();

            tmpPara.BatteryCurrent = ((short)(tmpResultDict0700[id][5] * 256 + tmpResultDict0700[id][6])).ToString();
            tmpPara.BatterySOC = ((short)(tmpResultDict0700[id][7] * 256 + tmpResultDict0700[id][8])).ToString();

          }

          if (tmpResultDict0710.ContainsKey(id))
          {
            tmpPara.LoStatus = ((short)(tmpResultDict0710[id][1] * 256 + tmpResultDict0710[id][2])).ToString();
            tmpPara.HiStatus = ((short)(tmpResultDict0710[id][3] * 256 + tmpResultDict0710[id][4])).ToString();

            tmpPara.RC = ((short)(tmpResultDict0710[id][5] * 256 + tmpResultDict0710[id][6])).ToString();
            tmpPara.FCC = ((short)(tmpResultDict0710[id][7] * 256 + tmpResultDict0710[id][8])).ToString();

          }

          if (tmpResultDict0720.ContainsKey(id))
          {
            //// 0720
            tmpPara.SoH = (tmpResultDict0720[id][1] * 256 + tmpResultDict0720[id][2]).ToString();
            tmpPara.FirmwareVersion = tmpResultDict0720[id][3].ToString() + " " + tmpResultDict0720[id][4].ToString();

          }
          if (tmpResultDict0730.ContainsKey(id))
          {
            //// 0730
            tmpPara.LoTS1Temp = ((short)(tmpResultDict0730[id][1] * 256 + tmpResultDict0730[id][2])).ToString();
            tmpPara.LoTS2Temp = ((short)(tmpResultDict0730[id][3] * 256 + tmpResultDict0730[id][4])).ToString();
            tmpPara.LoTS3Temp = ((short)(tmpResultDict0730[id][5] * 256 + tmpResultDict0730[id][6])).ToString();

          }
          if (tmpResultDict0740.ContainsKey(id))
          {
            //// 0740
            tmpPara.HiTS1Temp = ((short)(tmpResultDict0740[id][1] * 256 + tmpResultDict0740[id][2])).ToString();
            tmpPara.HiTS2Temp = ((short)(tmpResultDict0740[id][3] * 256 + tmpResultDict0740[id][4])).ToString();
            tmpPara.HiTS3Temp = ((short)(tmpResultDict0740[id][5] * 256 + tmpResultDict0740[id][6])).ToString();
          }


          if (tmpResultDict0800.ContainsKey(id))
          {
            //// 0800
            tmpPara.CellVoltage1 = ((short)(tmpResultDict0800[id][1] * 256 + tmpResultDict0800[id][2])).ToString();
            tmpPara.CellVoltage2 = ((short)(tmpResultDict0800[id][3] * 256 + tmpResultDict0800[id][4])).ToString();
            tmpPara.CellVoltage3 = ((short)(tmpResultDict0800[id][5] * 256 + tmpResultDict0800[id][6])).ToString();
            tmpPara.CellVoltage4 = ((short)(tmpResultDict0800[id][7] * 256 + tmpResultDict0800[id][8])).ToString();

          }

          if (tmpResultDict0810.ContainsKey(id))
          {
            //// 0810
            tmpPara.CellVoltage5 = ((short)(tmpResultDict0810[id][1] * 256 + tmpResultDict0810[id][2])).ToString();
            tmpPara.CellVoltage6 = ((short)(tmpResultDict0810[id][3] * 256 + tmpResultDict0810[id][4])).ToString();
            tmpPara.CellVoltage7 = ((short)(tmpResultDict0810[id][5] * 256 + tmpResultDict0810[id][6])).ToString();
            tmpPara.CellVoltage8 = ((short)(tmpResultDict0810[id][7] * 256 + tmpResultDict0810[id][8])).ToString();
          }

          if (tmpResultDict0820.ContainsKey(id))
          {
            //// 0820
            tmpPara.CellVoltage9 = ((short)(tmpResultDict0820[id][1] * 256 + tmpResultDict0820[id][2])).ToString();
            tmpPara.CellVoltage10 = ((short)(tmpResultDict0820[id][3] * 256 + tmpResultDict0820[id][4])).ToString();
            tmpPara.CellVoltage11 = ((short)(tmpResultDict0820[id][5] * 256 + tmpResultDict0820[id][6])).ToString();
            tmpPara.CellVoltage12 = ((short)(tmpResultDict0820[id][7] * 256 + tmpResultDict0820[id][8])).ToString();
          }

          if (tmpResultDict0830.ContainsKey(id))
          {
            //// 0830
            tmpPara.CellVoltage13 = ((short)(tmpResultDict0830[id][1] * 256 + tmpResultDict0830[id][2])).ToString();
            tmpPara.CellVoltage14 = ((short)(tmpResultDict0830[id][3] * 256 + tmpResultDict0830[id][4])).ToString();
            tmpPara.CellVoltage15 = ((short)(tmpResultDict0830[id][5] * 256 + tmpResultDict0830[id][6])).ToString();
            tmpPara.CellVoltage16 = ((short)(tmpResultDict0830[id][7] * 256 + tmpResultDict0830[id][8])).ToString();
          }

          if (tmpResultDict0840.ContainsKey(id))
          {
            //// 0840
            tmpPara.CellVoltage17 = ((short)(tmpResultDict0840[id][1] * 256 + tmpResultDict0840[id][2])).ToString();
            tmpPara.CellVoltage18 = ((short)(tmpResultDict0840[id][3] * 256 + tmpResultDict0840[id][4])).ToString();
            tmpPara.CellVoltage19 = ((short)(tmpResultDict0840[id][5] * 256 + tmpResultDict0840[id][6])).ToString();
            tmpPara.CellVoltage20 = ((short)(tmpResultDict0840[id][7] * 256 + tmpResultDict0840[id][8])).ToString();
          }

          if (tmpResultDict0850.ContainsKey(id))
          {
            //// 0850
            tmpPara.CellVoltage21 = ((short)(tmpResultDict0850[id][1] * 256 + tmpResultDict0850[id][2])).ToString();
            tmpPara.CellVoltage22 = ((short)(tmpResultDict0850[id][3] * 256 + tmpResultDict0850[id][4])).ToString();
            tmpPara.CellVoltage23 = ((short)(tmpResultDict0850[id][5] * 256 + tmpResultDict0850[id][6])).ToString();
            tmpPara.CellVoltage24 = ((short)(tmpResultDict0850[id][7] * 256 + tmpResultDict0850[id][8])).ToString();
          }

          if (tmpResultDict0860.ContainsKey(id))
          {
            //// 0860
            tmpPara.CellVoltage25 = ((short)(tmpResultDict0860[id][1] * 256 + tmpResultDict0860[id][2])).ToString();
            tmpPara.CellVoltage26 = ((short)(tmpResultDict0860[id][3] * 256 + tmpResultDict0860[id][4])).ToString();
            tmpPara.CellVoltage27 = ((short)(tmpResultDict0860[id][5] * 256 + tmpResultDict0860[id][6])).ToString();
            tmpPara.CellVoltage28 = ((short)(tmpResultDict0860[id][7] * 256 + tmpResultDict0860[id][8])).ToString();
          }

          if (tmpResultDict0870.ContainsKey(id))
          {
            //// 0870
            tmpPara.CellVoltage29 = ((short)(tmpResultDict0870[id][1] * 256 + tmpResultDict0870[id][2])).ToString();
            tmpPara.CellVoltage30 = ((short)(tmpResultDict0870[id][3] * 256 + tmpResultDict0870[id][4])).ToString();
          }

          if (tmpResultDict0880.ContainsKey(id))
          {
            //// 0880
            tmpPara.LoVADTemp1 = ((short)(tmpResultDict0880[id][1] * 256 + tmpResultDict0880[id][2])).ToString();
            tmpPara.LoVADTemp2 = ((short)(tmpResultDict0880[id][3] * 256 + tmpResultDict0880[id][4])).ToString();
            tmpPara.LoVADTemp3 = ((short)(tmpResultDict0880[id][5] * 256 + tmpResultDict0880[id][6])).ToString();
            tmpPara.LoVADTemp4 = ((short)(tmpResultDict0880[id][7] * 256 + tmpResultDict0880[id][8])).ToString();
          }

          if (tmpResultDict0890.ContainsKey(id))
          {
            // 0890
            tmpPara.LoVADTemp5 = ((short)(tmpResultDict0890[id][1] * 256 + tmpResultDict0890[id][2])).ToString();
            tmpPara.LoVADTemp6 = ((short)(tmpResultDict0890[id][3] * 256 + tmpResultDict0890[id][4])).ToString();
          }


          if (tmpResultDict08A0.ContainsKey(id))
          {
            //// 08A0
            tmpPara.HiVADTemp1 = ((short)(tmpResultDict08A0[id][1] * 256 + tmpResultDict08A0[id][2])).ToString();
            tmpPara.HiVADTemp2 = ((short)(tmpResultDict08A0[id][3] * 256 + tmpResultDict08A0[id][4])).ToString();
            tmpPara.HiVADTemp3 = ((short)(tmpResultDict08A0[id][5] * 256 + tmpResultDict08A0[id][6])).ToString();
            tmpPara.HiVADTemp4 = ((short)(tmpResultDict08A0[id][7] * 256 + tmpResultDict08A0[id][8])).ToString();
          }

          if (tmpResultDict08B0.ContainsKey(id))
          {
            // 08B0
            tmpPara.HiVADTemp5 = ((short)(tmpResultDict08B0[id][1] * 256 + tmpResultDict08B0[id][2])).ToString();
            tmpPara.HiVADTemp6 = ((short)(tmpResultDict08B0[id][3] * 256 + tmpResultDict08B0[id][4])).ToString();
          }

          //resultParaDict.TryAdd(id, tmpPara);
          TotalMasterData28[id] = tmpPara;
        }
      }
      catch (Exception ex)
      {
        return 0;
      }

      return tmpRstDict.Count;
    }


    /// <summary>
    /// 第一次讀取為了抓到有存在的PACK，要放慢時間
    /// </summary>
    /// <returns></returns>
    public ConcurrentDictionary<int, BatteryCanbus2P5SPara> GetFstAllPara5s2p()
    {
      ConcurrentDictionary<int, BatteryCanbus2P5SPara> resultParaDict = new ConcurrentDictionary<int, BatteryCanbus2P5SPara>();

      BatteryCanbus2P5SPara tmpPara = new BatteryCanbus2P5SPara();
      Stopwatch t1 = new Stopwatch();
      t1.Start();
      Dictionary<int, List<short>> tmpResultDict0700 = singleCmd("0x0700");

      string timeSingleStr = t1.ElapsedMilliseconds.ToString();
      Dictionary<int, List<short>> tmpResultDict0710 = singleCmd("0x0710");
      Dictionary<int, List<short>> tmpResultDict0720 = singleCmd("0x0720");
      Dictionary<int, List<short>> tmpResultDict0730 = singleCmd("0x0730");
      Dictionary<int, List<short>> tmpResultDict0740 = singleCmd("0x0740");
      Dictionary<int, List<short>> tmpResultDict0750 = singleCmd("0x0750");
      Dictionary<int, List<short>> tmpResultDict0800 = singleCmd("0x0800");
      Dictionary<int, List<short>> tmpResultDict0810 = singleCmd("0x0810");
      string timeSingleStr2 = t1.ElapsedMilliseconds.ToString();
      t1.Stop();
      // 找出所有的SlaveID
      List<int> SlaveIdList = new List<int>();

      try
      {
        foreach (var k in tmpResultDict0700.Keys)
        {
          SlaveIdList.Add(k);
        }

        foreach (var id in SlaveIdList)
        {

          tmpPara = new BatteryCanbus2P5SPara();

          tmpPara.Detected = true;
          // 0700
          tmpPara.BatteryVoltage = ((short)(tmpResultDict0700[id][1] * 256 + tmpResultDict0700[id][2])).ToString();
          tmpPara.BatteryCurrent = ((short)(tmpResultDict0700[id][3] * 256 + tmpResultDict0700[id][4])).ToString();

          tmpPara.FCC = ((short)(tmpResultDict0700[id][5] * 256 + tmpResultDict0700[id][6])).ToString();
          tmpPara.RC = ((short)(tmpResultDict0700[id][7] * 256 + tmpResultDict0700[id][8])).ToString();

          // 0710
          // safety status
          // data1
          // List[0]是LSB  List[7] 是MSB
          tmpPara.SaftyStatus1 = tmpResultDict0710[id][1].ToString();
          List<bool> tmpSafetyStatusData1List = BytesToList(tmpResultDict0710[id][1]);
          tmpPara.OCDL = tmpSafetyStatusData1List[5];
          tmpPara.COVL = tmpSafetyStatusData1List[4];
          tmpPara.UTD = tmpSafetyStatusData1List[3];
          tmpPara.UTC = tmpSafetyStatusData1List[2];
          tmpPara.PCHGC = tmpSafetyStatusData1List[1];
          tmpPara.CHGV = tmpSafetyStatusData1List[0];

          // data2
          tmpPara.SaftyStatus2 = tmpResultDict0710[id][2].ToString();
          List<bool> tmpSafetyStatusData2List = BytesToList(tmpResultDict0710[id][2]);
          tmpPara.CHGC = tmpSafetyStatusData2List[7];
          tmpPara.OC = tmpSafetyStatusData2List[6];
          tmpPara.CTO = tmpSafetyStatusData2List[4];
          tmpPara.PTO = tmpSafetyStatusData2List[2];
          tmpPara.OTF = tmpSafetyStatusData2List[0];

          // data3
          tmpPara.SaftyStatus3 = tmpResultDict0710[id][3].ToString();
          List<bool> tmpSafetyStatusData3List = BytesToList(tmpResultDict0710[id][3]);
          tmpPara.CUVC = tmpSafetyStatusData3List[6];
          tmpPara.OTD = tmpSafetyStatusData3List[5];
          tmpPara.OTC = tmpSafetyStatusData3List[4];
          tmpPara.ASCDL = tmpSafetyStatusData3List[3];
          tmpPara.ASCD = tmpSafetyStatusData3List[2];
          tmpPara.ASCCL = tmpSafetyStatusData3List[1];
          tmpPara.ASCC = tmpSafetyStatusData3List[0];

          // data4
          tmpPara.SaftyStatus4 = tmpResultDict0710[id][4].ToString();
          List<bool> tmpSafetyStatusData4List = BytesToList(tmpResultDict0710[id][4]);
          tmpPara.AOLDL = tmpSafetyStatusData4List[7];
          tmpPara.AOLD = tmpSafetyStatusData4List[6];
          tmpPara.OCD2 = tmpSafetyStatusData4List[5];
          tmpPara.OCD1 = tmpSafetyStatusData4List[4];
          tmpPara.OCC1 = tmpSafetyStatusData4List[2];
          tmpPara.COV = tmpSafetyStatusData4List[1];
          tmpPara.CUV = tmpSafetyStatusData4List[0];

          // battery status
          // data5

          tmpPara.BatteryStatus5 = tmpResultDict0710[id][5].ToString();
          List<bool> tmpBatteryStatusData5List = BytesToList(tmpResultDict0710[id][5]);
          tmpPara.OCA = tmpBatteryStatusData5List[7];
          tmpPara.TCA = tmpBatteryStatusData5List[6];
          tmpPara.OTA = tmpBatteryStatusData5List[4];
          tmpPara.TDA = tmpBatteryStatusData5List[3];
          tmpPara.RCA = tmpBatteryStatusData5List[1];
          tmpPara.RTA = tmpBatteryStatusData5List[0];

          // data6
          tmpPara.BatteryStatus6 = tmpResultDict0710[id][6].ToString();
          List<bool> tmpBatteryStatusData6List = BytesToList(tmpResultDict0710[id][6]);
          tmpPara.INIT = tmpBatteryStatusData6List[7];
          tmpPara.DSG = tmpBatteryStatusData6List[6];
          tmpPara.FC = tmpBatteryStatusData6List[5];
          tmpPara.FD = tmpBatteryStatusData6List[4];
          tmpPara.EC3 = tmpBatteryStatusData6List[3];
          tmpPara.EC2 = tmpBatteryStatusData6List[2];
          tmpPara.EC1 = tmpBatteryStatusData6List[1];
          tmpPara.EC0 = tmpBatteryStatusData6List[0];

          //// data7
          tmpPara.RSoC = tmpResultDict0710[id][7].ToString();

          //// data8
          tmpPara.SoH = tmpResultDict0710[id][8].ToString();

          //// 0720
          tmpPara.SystemCounter = (tmpResultDict0720[id][1] * 256 + tmpResultDict0720[id][2]).ToString();
          tmpPara.FirmwareVersion = tmpResultDict0720[id][3].ToString() + " " + tmpResultDict0720[id][4].ToString();

          //// 0730
          tmpPara.Ts1Temp = ((short)(tmpResultDict0730[id][1] * 256 + tmpResultDict0730[id][2])).ToString();
          tmpPara.Ts2Temp = ((short)(tmpResultDict0730[id][3] * 256 + tmpResultDict0730[id][4])).ToString();
          tmpPara.Ts3Temp = ((short)(tmpResultDict0730[id][5] * 256 + tmpResultDict0730[id][6])).ToString();
          tmpPara.Fet = ((short)(tmpResultDict0730[id][7] * 256 + tmpResultDict0730[id][8])).ToString();

          //// 0740
          tmpPara.AnalogTemp1 = ((short)(tmpResultDict0740[id][1] * 256 + tmpResultDict0740[id][2])).ToString();
          tmpPara.AnalogTemp2 = ((short)(tmpResultDict0740[id][3] * 256 + tmpResultDict0740[id][4])).ToString();
          tmpPara.AnalogTemp3 = ((short)(tmpResultDict0740[id][5] * 256 + tmpResultDict0740[id][6])).ToString();
          tmpPara.AnalogTemp4 = ((short)(tmpResultDict0740[id][7] * 256 + tmpResultDict0740[id][8])).ToString();

          // 0750
          // data1
          //List<int> tmpDipSwData1List = BytesToList(tmpResultDict0750[id][1]);

          tmpPara.DipSWStatus = tmpResultDict0750[id][1].ToString();
          tmpPara.ID = (tmpResultDict0750[id][1] & 0xF).ToString();
          tmpPara.Mode = ((tmpResultDict0750[id][1] >> 4) & 0xF).ToString();

          //// 0800
          tmpPara.CellVoltage1 = ((short)(tmpResultDict0800[id][1] * 256 + tmpResultDict0800[id][2])).ToString();
          tmpPara.CellVoltage2 = ((short)(tmpResultDict0800[id][3] * 256 + tmpResultDict0800[id][4])).ToString();
          tmpPara.CellVoltage3 = ((short)(tmpResultDict0800[id][5] * 256 + tmpResultDict0800[id][6])).ToString();
          tmpPara.CellVoltage4 = ((short)(tmpResultDict0800[id][7] * 256 + tmpResultDict0800[id][8])).ToString();

          //// 0810
          tmpPara.CellVoltage5 = ((short)(tmpResultDict0810[id][1] * 256 + tmpResultDict0810[id][2])).ToString();

          resultParaDict.TryAdd(id, tmpPara);
          TotalMasterData[id] = tmpPara;
        }
      }
      catch (Exception ex)
      {
        return null;
      }

      return resultParaDict;
    }

    private int TransToInt32(string str)
    {
      return Convert.ToInt32(str);
    }

    public List<bool> BytesToList(Int32 byte_value)
    {
      // List[0]是LSB  List[7] 是MSB

      List<Int32> TMP_List1 = new List<Int32>();
      List<bool> BooltmpList = new List<bool>();
      Int32 int_value = (Int32)byte_value;
      for (Int32 i = 0; i < 8; i++)
      {
        Int32 TMP_int1 = (int_value) % 2;
        TMP_List1.Add(TMP_int1);
        BooltmpList.Add(Convert.ToBoolean(TMP_int1));
        int_value = int_value / 2;
      }

      //return TMP_List1;
      return BooltmpList;
    }


    /// <summary>
    /// 目前設置的轉換對應表
    /// </summary>
    public Dictionary<string, int> cnvDictPsId = new Dictionary<string, int>()
        {
            { "A",1},{ "B",2},{ "C",3},{ "D",4},
            { "E",5},{ "F",6},{ "G",7},{ "H",8}
        };

    /// <summary>
    /// 檢查id與輸入的對應轉換字串是否存在相符，不符的話代表設定錯誤
    /// </summary>
    /// <param name="chkIdList"></param>
    /// <returns></returns>
    public void idCheck(List<string> chkIdList)
    {
      List<string> tmpList = new List<string>();

      verifyModeAllConsistance = true;
      verifyidNumCorrect = true;
      foreach (string BatStr in chkIdList)
      {
        tmpList.Add(TotalMasterData[cnvDictPsId[BatStr]].Mode);
      }
      for (int i = 0; i < tmpList.Count; i++)
      {
        for (int j = 1; j < tmpList.Count; j++)
        {
          if (tmpList[i] != tmpList[j])
          {
            verifyModeAllConsistance = false;
            break;
          }
        }
      }

      foreach (string BatStr in chkIdList)
      {
        if (TotalMasterData[cnvDictPsId[BatStr]].Detected != true)
        {
          verifyidNumCorrect = false;
          break;
        }
      }
    }

    /// <summary>
    /// 計算統計資訊
    /// </summary>
    /// <param name="psStr"></param>
    /// <returns></returns>
    public List<int> CalSum(int index)
    {
      // 計算電量的時候,不管是串並聯都取最小容量
      // 串連的時候取最小
      // 串並聯則取各個串連的最小再相加各個並聯的
      List<int> ResponseIdList = new List<int>();
      List<int> noResponseIdList = new List<int>();

      foreach (string k in cnvDictPsId.Keys)
      {
        if (TotalMasterData[cnvDictPsId[k]].Detected)
          ResponseIdList.Add(cnvDictPsId[k]);
        else
          noResponseIdList.Add(cnvDictPsId[k]);
      }

      switch (index)
      {
        case 0: // 1S1P  A

          idCheck(new List<string> { "A" });

          // 不管A是否存在，以下都成立，所以不用改
          TotalVoltage = Convert.ToInt32(TotalMasterData[cnvDictPsId["A"]].BatteryVoltage);
          TotalCurrent = Convert.ToDouble(TotalMasterData[cnvDictPsId["A"]].BatteryCurrent) * 10;
          TotalCapacity = Convert.ToInt32(TotalMasterData[cnvDictPsId["A"]].RC);
          TotalPower = (TotalVoltage / 1000) * (TotalCurrent / 1000);
          break;
        case 1:
          {
            idCheck(new List<string> { "A", "E" });

            // 1S2P : A E
            // Voltage = A = E  // 要判斷有沒有存在
            // Current = A + E 
            // Capacity = A + E
            // Power =  TotalVoltage * TotalCurrent

            int volA = Convert.ToInt32(TotalMasterData[cnvDictPsId["A"]].BatteryVoltage);
            int volE = Convert.ToInt32(TotalMasterData[cnvDictPsId["E"]].BatteryVoltage);

            double ampA = Convert.ToInt32(TotalMasterData[cnvDictPsId["A"]].BatteryCurrent) * 10;
            double ampE = Convert.ToInt32(TotalMasterData[cnvDictPsId["E"]].BatteryCurrent) * 10;

            int capA = Convert.ToInt32(TotalMasterData[cnvDictPsId["A"]].RC);
            int capE = Convert.ToInt32(TotalMasterData[cnvDictPsId["E"]].RC);

            // 判斷 A E 是否存在
            int vol = 0;
            if (TotalMasterData[cnvDictPsId["A"]].Detected)
              vol = volA;
            else if (TotalMasterData[cnvDictPsId["E"]].Detected)
              vol = volE;

            TotalVoltage = vol;
            TotalCurrent = ampA + ampE;
            TotalCapacity = capA + capE;
            TotalPower = (TotalVoltage / 1000) * (TotalCurrent / 1000);
            break;
          }
        case 2: // 2S1P A B
          {
            idCheck(new List<string> { "A", "B" });

            // Voltage = A + B  
            // Current =  A  =  B // 判斷 AB是否存在
            // Capacity = A  =  B // 判斷 AB是否存在
            // Power =  TotalVoltage * TotalCurrent
            int volA = Convert.ToInt32(TotalMasterData[cnvDictPsId["A"]].BatteryVoltage);
            int volB = Convert.ToInt32(TotalMasterData[cnvDictPsId["B"]].BatteryVoltage);

            double ampA = Convert.ToInt32(TotalMasterData[cnvDictPsId["A"]].BatteryCurrent) * 10;
            double ampB = Convert.ToInt32(TotalMasterData[cnvDictPsId["B"]].BatteryCurrent) * 10;

            int capA = Convert.ToInt32(TotalMasterData[cnvDictPsId["A"]].RC);
            int capB = Convert.ToInt32(TotalMasterData[cnvDictPsId["B"]].RC);

            // 判斷 A B 是否存在的問題
            int cap = 0;
            double amp = 0;

            if (TotalMasterData[cnvDictPsId["A"]].Detected)
            {
              amp = ampA;
              cap = capA;
            }
            else if (TotalMasterData[cnvDictPsId["B"]].Detected)
            {
              amp = ampB;
              cap = capB;
            }

            TotalVoltage = volA + volB;
            TotalCurrent = amp;
            TotalCapacity = cap;
            TotalPower = (TotalVoltage / 1000) * (TotalCurrent / 1000);
            break;
          }

        case 3: // 2S2P A B E F
          {
            idCheck(new List<string> { "A", "B", "E", "F" });

            // 電壓 / 電流 一定要個別計算 ( 因為還要計算空接問題 )
            // 2023/12/20 串接測試時發現SUM沒有電壓，原因是只有使用到EF
            // 現在要針對每個未知狀況去做計算
            // Voltage =  (A+B) = (E+F) // 判斷是不是>0就可以了
            // Current =  (A=B) + (E=F) // 要判斷 A B E F 是否存在
            // Capacity = (A=B) + (E=F) // 要判斷 A B E F 是否存在
            // Power =  TotalVoltage * TotalCurrent

            int volA = Convert.ToInt32(TotalMasterData[cnvDictPsId["A"]].BatteryVoltage);
            int volB = Convert.ToInt32(TotalMasterData[cnvDictPsId["B"]].BatteryVoltage);
            int volE = Convert.ToInt32(TotalMasterData[cnvDictPsId["E"]].BatteryVoltage);
            int volF = Convert.ToInt32(TotalMasterData[cnvDictPsId["F"]].BatteryVoltage);

            double ampA = Convert.ToInt32(TotalMasterData[cnvDictPsId["A"]].BatteryCurrent) * 10;
            double ampB = Convert.ToInt32(TotalMasterData[cnvDictPsId["B"]].BatteryCurrent) * 10;
            double ampE = Convert.ToInt32(TotalMasterData[cnvDictPsId["E"]].BatteryCurrent) * 10;
            double ampF = Convert.ToInt32(TotalMasterData[cnvDictPsId["F"]].BatteryCurrent) * 10;

            int capA = Convert.ToInt32(TotalMasterData[cnvDictPsId["A"]].RC);
            int capB = Convert.ToInt32(TotalMasterData[cnvDictPsId["B"]].RC);
            int capE = Convert.ToInt32(TotalMasterData[cnvDictPsId["E"]].RC);
            int capF = Convert.ToInt32(TotalMasterData[cnvDictPsId["F"]].RC);

            int v1 = 0;
            int v2 = 0;
            v1 = volA + volB;
            v2 = volE + volF;

            int vol = 0;
            if (v1 > 0)
              vol = v1;
            else if (v2 > 0)
              vol = v2;

            int cap1 = 0;
            double amp1 = 0;
            if (TotalMasterData[cnvDictPsId["A"]].Detected)
            {
              amp1 = ampA;
              cap1 = capA;
            }
            else if (TotalMasterData[cnvDictPsId["B"]].Detected)
            {
              amp1 = ampB;
              cap1 = capB;
            }

            double amp2 = 0;
            int cap2 = 0;
            if (TotalMasterData[cnvDictPsId["E"]].Detected)
            {
              amp2 = ampE;
              cap2 = capE;
            }
            else if (TotalMasterData[cnvDictPsId["F"]].Detected)
            {
              amp2 = ampF;
              cap2 = capF;
            }

            TotalVoltage = vol;
            TotalCurrent = amp1 + amp2;
            TotalCapacity = cap1 + cap2;
            TotalPower = (TotalVoltage / 1000) * (TotalCurrent / 1000);
            break;
          }
        case 4: // 3S1P A B C
          {
            idCheck(new List<string> { "A", "B", "C" });

            // Voltage =  (A+B+C)
            // Current =  (A=B=C) // 判斷 ABC是否存在
            // Capacity = (A=B=C) // 判斷 ABC是否存在

            // Power =  TotalVoltage * TotalCurrent
            int volA = Convert.ToInt32(TotalMasterData[cnvDictPsId["A"]].BatteryVoltage);
            int volB = Convert.ToInt32(TotalMasterData[cnvDictPsId["B"]].BatteryVoltage);
            int volC = Convert.ToInt32(TotalMasterData[cnvDictPsId["C"]].BatteryVoltage);

            double ampA = Convert.ToInt32(TotalMasterData[cnvDictPsId["A"]].BatteryCurrent) * 10;
            double ampB = Convert.ToInt32(TotalMasterData[cnvDictPsId["B"]].BatteryCurrent) * 10;
            double ampC = Convert.ToInt32(TotalMasterData[cnvDictPsId["C"]].BatteryCurrent) * 10;

            int capA = Convert.ToInt32(TotalMasterData[cnvDictPsId["A"]].RC);
            int capB = Convert.ToInt32(TotalMasterData[cnvDictPsId["B"]].RC);
            int capC = Convert.ToInt32(TotalMasterData[cnvDictPsId["C"]].RC);

            double amp = 0;
            int cap = 0;
            if (TotalMasterData[cnvDictPsId["A"]].Detected)
            {
              amp = ampA;
              cap = capA;
            }
            else if (TotalMasterData[cnvDictPsId["B"]].Detected)
            {
              amp = ampB;
              cap = capB;
            }
            else if (TotalMasterData[cnvDictPsId["C"]].Detected)
            {
              amp = ampC;
              cap = capC;
            }

            TotalVoltage = volA + volB + volC;
            TotalCurrent = amp;
            TotalCapacity = cap;
            TotalPower = (TotalVoltage / 1000) * (TotalCurrent / 1000);
            break;
          }
        case 5: // 3S2P A B C E F G
          {
            idCheck(new List<string> { "A", "B", "C", "E", "F", "G" });

            // Voltage =  (A+B+C) = (E+F+G) // 判斷是不是>0就可以了
            // Current =  (A=B=C) + (E=F=G) // 要判斷 A B C E F G 是否存在
            // Capacity = (A=B=C) + (E=F=G) // 要判斷 A B C E F G 是否存在
            // Power =  TotalVoltage * TotalCurrent
            int volA = Convert.ToInt32(TotalMasterData[cnvDictPsId["A"]].BatteryVoltage);
            int volB = Convert.ToInt32(TotalMasterData[cnvDictPsId["B"]].BatteryVoltage);
            int volC = Convert.ToInt32(TotalMasterData[cnvDictPsId["C"]].BatteryVoltage);
            int volE = Convert.ToInt32(TotalMasterData[cnvDictPsId["E"]].BatteryVoltage);
            int volF = Convert.ToInt32(TotalMasterData[cnvDictPsId["F"]].BatteryVoltage);
            int volG = Convert.ToInt32(TotalMasterData[cnvDictPsId["G"]].BatteryVoltage);

            double ampA = Convert.ToInt32(TotalMasterData[cnvDictPsId["A"]].BatteryCurrent) * 10;
            double ampB = Convert.ToInt32(TotalMasterData[cnvDictPsId["B"]].BatteryCurrent) * 10;
            double ampC = Convert.ToInt32(TotalMasterData[cnvDictPsId["C"]].BatteryCurrent) * 10;
            double ampE = Convert.ToInt32(TotalMasterData[cnvDictPsId["E"]].BatteryCurrent) * 10;
            double ampF = Convert.ToInt32(TotalMasterData[cnvDictPsId["F"]].BatteryCurrent) * 10;
            double ampG = Convert.ToInt32(TotalMasterData[cnvDictPsId["G"]].BatteryCurrent) * 10;

            int capA = Convert.ToInt32(TotalMasterData[cnvDictPsId["A"]].RC);
            int capB = Convert.ToInt32(TotalMasterData[cnvDictPsId["B"]].RC);
            int capC = Convert.ToInt32(TotalMasterData[cnvDictPsId["C"]].RC);
            int capE = Convert.ToInt32(TotalMasterData[cnvDictPsId["E"]].RC);
            int capF = Convert.ToInt32(TotalMasterData[cnvDictPsId["F"]].RC);
            int capG = Convert.ToInt32(TotalMasterData[cnvDictPsId["G"]].RC);

            int v1 = 0;
            int v2 = 0;
            int vol = 0;
            v1 = volA + volB + volC;
            v2 = volE + volF + volG;
            if (v1 > 0)
              vol = v1;
            else if (v2 > 0)
              vol = v2;

            int cap1 = 0;
            double amp1 = 0;
            if (TotalMasterData[cnvDictPsId["A"]].Detected)
            {
              amp1 = ampA;
              cap1 = capA;
            }
            else if (TotalMasterData[cnvDictPsId["B"]].Detected)
            {
              amp1 = ampB;
              cap1 = capB;
            }
            else if (TotalMasterData[cnvDictPsId["C"]].Detected)
            {
              amp1 = ampC;
              cap1 = capC;
            }

            double amp2 = 0;
            int cap2 = 0;
            if (TotalMasterData[cnvDictPsId["E"]].Detected)
            {
              amp2 = ampE;
              cap2 = capE;
            }
            else if (TotalMasterData[cnvDictPsId["F"]].Detected)
            {
              amp2 = ampF;
              cap2 = capF;
            }
            else if (TotalMasterData[cnvDictPsId["G"]].Detected)
            {
              amp2 = ampG;
              cap2 = capG;
            }

            TotalVoltage = vol;
            TotalCurrent = amp1 + amp2;
            TotalCapacity = cap1 + cap2;
            TotalPower = (TotalVoltage / 1000) * (TotalCurrent / 1000);
            break;
          }
        case 6: // 4S1P A B C D
          {
            idCheck(new List<string> { "A", "B", "C", "D" });

            // Voltage =  (A+B+C+D)
            // Current =  (A=B=C=D) // 判斷 ABCD 是否存在
            // Capacity = (A=B=C=D) // 判斷 ABCD 是否存在
            // Power =  TotalVoltage * TotalCurrent

            int volA = Convert.ToInt32(TotalMasterData[cnvDictPsId["A"]].BatteryVoltage);
            int volB = Convert.ToInt32(TotalMasterData[cnvDictPsId["B"]].BatteryVoltage);
            int volC = Convert.ToInt32(TotalMasterData[cnvDictPsId["C"]].BatteryVoltage);
            int volD = Convert.ToInt32(TotalMasterData[cnvDictPsId["D"]].BatteryVoltage);

            double ampA = Convert.ToInt32(TotalMasterData[cnvDictPsId["A"]].BatteryCurrent) * 10;
            double ampB = Convert.ToInt32(TotalMasterData[cnvDictPsId["B"]].BatteryCurrent) * 10;
            double ampC = Convert.ToInt32(TotalMasterData[cnvDictPsId["C"]].BatteryCurrent) * 10;
            double ampD = Convert.ToInt32(TotalMasterData[cnvDictPsId["D"]].BatteryCurrent) * 10;

            int capA = Convert.ToInt32(TotalMasterData[cnvDictPsId["A"]].RC);
            int capB = Convert.ToInt32(TotalMasterData[cnvDictPsId["B"]].RC);
            int capC = Convert.ToInt32(TotalMasterData[cnvDictPsId["C"]].RC);
            int capD = Convert.ToInt32(TotalMasterData[cnvDictPsId["D"]].RC);

            double amp = 0;
            int cap = 0;
            if (TotalMasterData[cnvDictPsId["A"]].Detected)
            {
              amp = ampA;
              cap = capA;
            }
            else if (TotalMasterData[cnvDictPsId["B"]].Detected)
            {
              amp = ampB;
              cap = capB;
            }
            else if (TotalMasterData[cnvDictPsId["C"]].Detected)
            {
              amp = ampC;
              cap = capC;
            }
            else if (TotalMasterData[cnvDictPsId["D"]].Detected)
            {
              amp = ampD;
              cap = capD;
            }

            TotalVoltage = volA + volB + volC + volD;
            TotalCurrent = amp;
            TotalCapacity = cap;
            TotalPower = (TotalVoltage / 1000) * (TotalCurrent / 1000);
            break;
          }

        case 7: // 4S2P A B C D E F G H
          {
            idCheck(new List<string> { "A", "B", "C", "D", "E", "F", "G", "H" });

            // Voltage =  (A+B+C+D) = (E+F+G+H) // 判斷是不是>0就可以了

            // Current =  (A=B=C=D) + (E=F=G=H) // 要判斷 A B C D E F G H 是否存在
            // Capacity = (A=B=C=D) + (E=F=G=H) // 要判斷 A B C D E F G H 是否存在

            // Power =  TotalVoltage * TotalCurrent

            int volA = Convert.ToInt32(TotalMasterData[cnvDictPsId["A"]].BatteryVoltage);
            int volB = Convert.ToInt32(TotalMasterData[cnvDictPsId["B"]].BatteryVoltage);
            int volC = Convert.ToInt32(TotalMasterData[cnvDictPsId["C"]].BatteryVoltage);
            int volD = Convert.ToInt32(TotalMasterData[cnvDictPsId["D"]].BatteryVoltage);
            int volE = Convert.ToInt32(TotalMasterData[cnvDictPsId["E"]].BatteryVoltage);
            int volF = Convert.ToInt32(TotalMasterData[cnvDictPsId["F"]].BatteryVoltage);
            int volG = Convert.ToInt32(TotalMasterData[cnvDictPsId["G"]].BatteryVoltage);
            int volH = Convert.ToInt32(TotalMasterData[cnvDictPsId["H"]].BatteryVoltage);


            double ampA = Convert.ToInt32(TotalMasterData[cnvDictPsId["A"]].BatteryCurrent) * 10;
            double ampB = Convert.ToInt32(TotalMasterData[cnvDictPsId["B"]].BatteryCurrent) * 10;
            double ampC = Convert.ToInt32(TotalMasterData[cnvDictPsId["C"]].BatteryCurrent) * 10;
            double ampD = Convert.ToInt32(TotalMasterData[cnvDictPsId["D"]].BatteryCurrent) * 10;
            double ampE = Convert.ToInt32(TotalMasterData[cnvDictPsId["E"]].BatteryCurrent) * 10;
            double ampF = Convert.ToInt32(TotalMasterData[cnvDictPsId["F"]].BatteryCurrent) * 10;
            double ampG = Convert.ToInt32(TotalMasterData[cnvDictPsId["G"]].BatteryCurrent) * 10;
            double ampH = Convert.ToInt32(TotalMasterData[cnvDictPsId["H"]].BatteryCurrent) * 10;

            int capA = Convert.ToInt32(TotalMasterData[cnvDictPsId["A"]].RC);
            int capB = Convert.ToInt32(TotalMasterData[cnvDictPsId["B"]].RC);
            int capC = Convert.ToInt32(TotalMasterData[cnvDictPsId["C"]].RC);
            int capD = Convert.ToInt32(TotalMasterData[cnvDictPsId["D"]].RC);
            int capE = Convert.ToInt32(TotalMasterData[cnvDictPsId["E"]].RC);
            int capF = Convert.ToInt32(TotalMasterData[cnvDictPsId["F"]].RC);
            int capG = Convert.ToInt32(TotalMasterData[cnvDictPsId["G"]].RC);
            int capH = Convert.ToInt32(TotalMasterData[cnvDictPsId["H"]].RC);

            int v1 = 0;
            int v2 = 0;

            int vol = 0;

            v1 = volA + volB + volC + volD;
            v2 = volE + volF + volG + volH;

            if (v1 > 0)
              vol = v1;
            else if (v2 > 0)
              vol = v2;

            int cap1 = 0;
            double amp1 = 0;
            if (TotalMasterData[cnvDictPsId["A"]].Detected)
            {
              amp1 = ampA;
              cap1 = capA;
            }
            else if (TotalMasterData[cnvDictPsId["B"]].Detected)
            {
              amp1 = ampB;
              cap1 = capB;
            }
            else if (TotalMasterData[cnvDictPsId["C"]].Detected)
            {
              amp1 = ampC;
              cap1 = capC;
            }
            else if (TotalMasterData[cnvDictPsId["D"]].Detected)
            {
              amp1 = ampD;
              cap1 = capD;
            }

            double amp2 = 0;
            int cap2 = 0;
            if (TotalMasterData[cnvDictPsId["E"]].Detected)
            {
              amp2 = ampE;
              cap2 = capE;
            }
            else if (TotalMasterData[cnvDictPsId["F"]].Detected)
            {
              amp2 = ampF;
              cap2 = capF;
            }
            else if (TotalMasterData[cnvDictPsId["G"]].Detected)
            {
              amp2 = ampG;
              cap2 = capG;
            }
            else if (TotalMasterData[cnvDictPsId["H"]].Detected)
            {
              amp2 = ampH;
              cap2 = capH;
            }

            TotalVoltage = volA + volB + volC + volD;
            TotalCurrent = amp1 + amp2;
            TotalCapacity = cap1 + cap2;
            TotalPower = (TotalVoltage / 1000) * (TotalCurrent / 1000);
            break;
          }
        default:
          return noResponseIdList;
      }
      return noResponseIdList;
    }

    public bool CalSumNorminal(int index)
    {
      double NorminalVoltage = 13.5; // 13.5V
      double NorminalCurrent = 150; // 150A
      double NorminalCapacity = 50; // 50Ah

      switch (index)
      {
        case 0: // 1S1P  A
          NorminalTotalVoltage = NorminalVoltage;
          NorminalTotalCurrent = NorminalCurrent;
          NorminalTotalCapacity = NorminalCapacity;
          NorminalTotalPower = NorminalTotalVoltage * NorminalTotalCurrent / 1000;
          break;
        case 1:
          {
            // 1S2P : A E
            // Voltage = A = E  取 A
            // Current = A + E
            // Capacity = A + E
            // Power =  TotalVoltage * TotalCurrent
            NorminalTotalVoltage = NorminalVoltage;
            NorminalTotalCurrent = NorminalCurrent * 2;
            NorminalTotalCapacity = NorminalCapacity * 2;
            NorminalTotalPower = NorminalTotalVoltage * NorminalTotalCurrent / 1000;
            break;
          }
        case 2: // 2S1P A B
                // Voltage = A + B  
                // Current =  A  =  B
                // Capacity = A  =  B
                // Power =  TotalVoltage * TotalCurrent
          {
            NorminalTotalVoltage = NorminalVoltage * 2;
            NorminalTotalCurrent = NorminalCurrent;
            NorminalTotalCapacity = NorminalCapacity;
            NorminalTotalPower = NorminalTotalVoltage * NorminalTotalCurrent / 1000;
            break;
          }

        case 3: // 2S2P A B E F
          {
            // Voltage =  (A+B) = (E+F)
            // Current =  (A=B) + (E=F)
            // Capacity = (A=B) + (E=F)
            // Power =  TotalVoltage * TotalCurrent
            NorminalTotalVoltage = NorminalVoltage * 2;
            NorminalTotalCurrent = NorminalCurrent * 2;
            NorminalTotalCapacity = NorminalCapacity * 2;
            NorminalTotalPower = NorminalTotalVoltage * NorminalTotalCurrent / 1000;
            break;
          }
        case 4: // 3S1P A B C
          {
            // Voltage =  (A+B+C)
            // Current =  (A=B=C)
            // Capacity = (A=B=C)
            // Power =  TotalVoltage * TotalCurrent
            NorminalTotalVoltage = NorminalVoltage * 3;
            NorminalTotalCurrent = NorminalCurrent;
            NorminalTotalCapacity = NorminalCapacity;
            NorminalTotalPower = NorminalTotalVoltage * NorminalTotalCurrent / 1000;
            break;
          }
        case 5: // 3S2P A B C E F G
          {
            // Voltage =  (A+B+C) = (E+F+G)
            // Current =  (A=B=C) + (E=F=G)
            // Capacity = (A=B=C) + (E=F=G)
            // Power =  TotalVoltage * TotalCurrent
            NorminalTotalVoltage = NorminalVoltage * 3;
            NorminalTotalCurrent = NorminalCurrent * 2;
            NorminalTotalCapacity = NorminalCapacity * 2;
            NorminalTotalPower = NorminalTotalVoltage * NorminalTotalCurrent / 1000;
            break;
          }
        case 6: // 4S1P A B C D
          {
            // Voltage =  (A+B+C+D)
            // Current =  (A=B=C=D)
            // Capacity = (A=B=C=D)
            // Power =  TotalVoltage * TotalCurrent

            NorminalTotalVoltage = NorminalVoltage * 4;
            NorminalTotalCurrent = NorminalCurrent * 1;
            NorminalTotalCapacity = NorminalCapacity * 1;
            NorminalTotalPower = NorminalTotalVoltage * NorminalTotalCurrent / 1000;
            break;
          }

        case 7: // 4S2P A B C D E F G
          {
            // Voltage =  (A+B+C+D) = (E+F+G+H)
            // Current =  (A=B=C=D) + (E=F=G=H)
            // Capacity = (A=B=C=D) + (E=F=G=H)
            // Power =  TotalVoltage * TotalCurrent

            NorminalTotalVoltage = NorminalVoltage * 4;
            NorminalTotalCurrent = NorminalCurrent * 2;
            NorminalTotalCapacity = NorminalCapacity * 2;
            NorminalTotalPower = NorminalTotalVoltage * NorminalTotalCurrent / 1000;
            break;
          }
        default:
          return false;
      }
      return true;
    }

  }


  public class BatteryCanbus2P5SPara
  {
    public string swid { get; set; } = "x"; //2024 /09/20 新增需求要顯示swid
    public bool Detected { get; set; } = false;
    public string BatteryVoltage { get; set; } = "0"; // 目前電壓
    public string BatteryCurrent { get; set; } = "0"; // 目前電流
    public string FCC { get; set; } = "0"; // 充滿電的容量 mAh
    public string RC { get; set; } = "0"; // 目前容量 mAh
    public string SaftyStatus1 { get; set; } = "0";
    public string SaftyStatus2 { get; set; } = "0";
    public string SaftyStatus3 { get; set; } = "0";
    public string SaftyStatus4 { get; set; } = "0";
    public string BatteryStatus5 { get; set; } = "0";
    public string BatteryStatus6 { get; set; } = "0";
    public string RSoC { get; set; } = "0";
    public string SoH { get; set; } = "0";
    public string SystemCounter { get; set; } = "0";
    public string FirmwareVersion { get; set; } = "0";
    public string Ts1Temp { get; set; } = "0";
    public string Ts2Temp { get; set; } = "0";
    public string Ts3Temp { get; set; } = "0";
    public string Fet { get; set; } = "0";
    public string AnalogTemp1 { get; set; } = "0";
    public string AnalogTemp2 { get; set; } = "0";
    public string AnalogTemp3 { get; set; } = "0";
    public string AnalogTemp4 { get; set; } = "0";
    public string CellVoltage1 { get; set; } = "0";
    public string CellVoltage2 { get; set; } = "0";
    public string CellVoltage3 { get; set; } = "0";
    public string CellVoltage4 { get; set; } = "0";
    public string CellVoltage5 { get; set; } = "0";
    public string CellVoltage6 { get; set; } = "0";
    public string CellVoltage7 { get; set; } = "0";
    public string CellVoltage8 { get; set; } = "0";
    public string CellVoltage9 { get; set; } = "0";
    public string CellVoltage10 { get; set; } = "0";
    public string CellVoltage11 { get; set; } = "0";
    public string CellVoltage12 { get; set; } = "0";
    public string CellVoltage13 { get; set; } = "0";
    public string CellVoltage14 { get; set; } = "0";
    public string CellVoltage15 { get; set; } = "0";
    public string CellVoltage16 { get; set; } = "0";
    public string CellVoltage17 { get; set; } = "0";
    public string CellVoltage18 { get; set; } = "0";
    public string CellVoltage19 { get; set; } = "0";
    public string CellVoltage20 { get; set; } = "0";
    public string CellVoltage21 { get; set; } = "0";
    public string CellVoltage22 { get; set; } = "0";
    public string CellVoltage23 { get; set; } = "0";
    public string CellVoltage24 { get; set; } = "0";
    public string CellVoltage25 { get; set; } = "0";
    public string CellVoltage26 { get; set; } = "0";
    public string CellVoltage27 { get; set; } = "0";
    public string CellVoltage28 { get; set; } = "0";
    public string CellVoltage29 { get; set; } = "0";
    public string CellVoltage30 { get; set; } = "0";
    public string DipSWStatus { get; set; } = "0";
    public string ProtectiveStatus { get; set; } = "0";
    public string CommunicationStatus { get; set; } = "0";



    // SAFETY STATUS
    // DATA1
    public bool OCDL { get; set; } = false;
    public bool COVL { get; set; } = false;
    public bool UTD { get; set; } = false;
    public bool UTC { get; set; } = false;
    public bool PCHGC { get; set; } = false;
    public bool CHGV { get; set; } = false;

    //DATA2
    public bool CHGC { get; set; } = false;
    public bool OC { get; set; } = false;
    public bool CTO { get; set; } = false;
    public bool PTO { get; set; } = false;
    public bool OTF { get; set; } = false;

    //DATA3
    public bool CUVC { get; set; } = false;
    public bool OTD { get; set; } = false;
    public bool OTC { get; set; } = false;
    public bool ASCDL { get; set; } = false;
    public bool ASCD { get; set; } = false;
    public bool ASCCL { get; set; } = false;
    public bool ASCC { get; set; } = false;

    //DATA4
    public bool AOLDL { get; set; } = false;
    public bool AOLD { get; set; } = false;
    public bool OCD2 { get; set; } = false;
    public bool OCD1 { get; set; } = false;
    public bool OCC1 { get; set; } = false;
    public bool COV { get; set; } = false;
    public bool CUV { get; set; } = false;

    //BATTERY STATUS
    //DATA5
    public bool OCA { get; set; } = false;
    public bool TCA { get; set; } = false;
    public bool OTA { get; set; } = false;
    public bool TDA { get; set; } = false;
    public bool RCA { get; set; } = false;
    public bool RTA { get; set; } = false;

    //DATA6
    public bool INIT { get; set; } = false;
    public bool DSG { get; set; } = false;
    public bool FC { get; set; } = false;
    public bool FD { get; set; } = false;
    public bool EC3 { get; set; } = false;
    public bool EC2 { get; set; } = false;
    public bool EC1 { get; set; } = false;
    public bool EC0 { get; set; } = false;

    //dIPSW STATUS
    //DATA1
    public string Mode { get; set; } = "0";
    public string ID { get; set; } = "0";

    //PROTECTIVE STATUS
    //DATA1
    public bool WAKEUP { get; set; } = false;
    public bool ALERT { get; set; } = false;
    public bool UVW { get; set; } = false;
    public bool SCP { get; set; } = false;
    public bool XCHG { get; set; } = false;
    public bool XDSG { get; set; } = false;
    public bool CHG_FET { get; set; } = false;
    public bool DSG_FET { get; set; } = false;

    public bool Comm_C_W { get; set; } = false;
    public bool Comm_S_W { get; set; } = false;
    public bool Comm_W { get; set; } = false;
  }

  public class BatteryCanbus2P28SPara
  {
    public string swid { get; set; } = "x"; //2024 /09/20 新增需求要顯示swid
    public bool Detected { get; set; } = false;
    public string BatteryLoVoltage { get; set; } = "0"; // 目前電壓
    public string BatteryHiVoltage { get; set; } = "0"; // 目前電壓
    public string BatteryCurrent { get; set; } = "0"; // 目前電流
    public string BatterySOC { get; set; } = "0";
    public string LoStatus { get; set; } = "0";
    public string HiStatus { get; set; } = "0";
    public string RC { get; set; } = "0"; // 目前容量 mAh
    public string FCC { get; set; } = "0"; // 充滿電的容量 mAh
    public string SoH { get; set; } = "0";

    public string FirmwareVersion { get; set; } = "0";

    public string LoTS1Temp { get; set; } = "0";
    public string LoTS2Temp { get; set; } = "0";
    public string LoTS3Temp { get; set; } = "0";

    public string HiTS1Temp { get; set; } = "0";
    public string HiTS2Temp { get; set; } = "0";
    public string HiTS3Temp { get; set; } = "0";

    public string CellVoltage1 { get; set; } = "0";
    public string CellVoltage2 { get; set; } = "0";
    public string CellVoltage3 { get; set; } = "0";
    public string CellVoltage4 { get; set; } = "0";
    public string CellVoltage5 { get; set; } = "0";
    public string CellVoltage6 { get; set; } = "0";
    public string CellVoltage7 { get; set; } = "0";
    public string CellVoltage8 { get; set; } = "0";
    public string CellVoltage9 { get; set; } = "0";
    public string CellVoltage10 { get; set; } = "0";
    public string CellVoltage11 { get; set; } = "0";
    public string CellVoltage12 { get; set; } = "0";
    public string CellVoltage13 { get; set; } = "0";
    public string CellVoltage14 { get; set; } = "0";
    public string CellVoltage15 { get; set; } = "0";
    public string CellVoltage16 { get; set; } = "0";
    public string CellVoltage17 { get; set; } = "0";
    public string CellVoltage18 { get; set; } = "0";
    public string CellVoltage19 { get; set; } = "0";
    public string CellVoltage20 { get; set; } = "0";
    public string CellVoltage21 { get; set; } = "0";
    public string CellVoltage22 { get; set; } = "0";
    public string CellVoltage23 { get; set; } = "0";
    public string CellVoltage24 { get; set; } = "0";
    public string CellVoltage25 { get; set; } = "0";
    public string CellVoltage26 { get; set; } = "0";
    public string CellVoltage27 { get; set; } = "0";
    public string CellVoltage28 { get; set; } = "0";
    public string CellVoltage29 { get; set; } = "0";
    public string CellVoltage30 { get; set; } = "0";

    public string LoVADTemp1 { get; set; } = "0";
    public string LoVADTemp2 { get; set; } = "0";
    public string LoVADTemp3 { get; set; } = "0";
    public string LoVADTemp4 { get; set; } = "0";
    public string LoVADTemp5 { get; set; } = "0";
    public string LoVADTemp6 { get; set; } = "0";

    public string HiVADTemp1 { get; set; } = "0";
    public string HiVADTemp2 { get; set; } = "0";
    public string HiVADTemp3 { get; set; } = "0";
    public string HiVADTemp4 { get; set; } = "0";
    public string HiVADTemp5 { get; set; } = "0";
    public string HiVADTemp6 { get; set; } = "0";


    // LoStatus
    public bool LoCUV { get; set; } = false;
    public bool LoCOV { get; set; } = false;
    public bool LoOCC { get; set; } = false;
    public bool LoOCD { get; set; } = false;
    public bool LoAOLD { get; set; } = false;
    public bool LoAOLDL { get; set; } = false;

    public bool LoASCD { get; set; } = false;
    public bool LoASCDL { get; set; } = false;
    public bool LoOTC { get; set; } = false;
    public bool LoOTD { get; set; } = false;
    public bool LoUTC { get; set; } = false;

    public bool LoUTD { get; set; } = false;
    public bool LoCHG { get; set; } = false;
    public bool LoDSG { get; set; } = false;
    public bool LoOC { get; set; } = false;

    // HiStatus
    public bool HiCUV { get; set; } = false;
    public bool HiCOV { get; set; } = false;
    public bool HiOTC { get; set; } = false;
    public bool HiOTD { get; set; } = false;
    public bool HiUTC { get; set; } = false;
    public bool HiUTD { get; set; } = false;
    public bool HiOC { get; set; } = false;
  }

  public class BatteryCanbus2P22SPara
  {
    private List<short> _dataList = new List<short>();
    public List<short> dataList
    {
      get { return _dataList; }
      set { _dataList = value; }
    }
    public bool Detected { get; set; } = false;  // 偵測是否有此 ID PACK
    public string BatteryLoVoltage { get; set; } = "0";  // 0x700 Data12
    public string BatteryHiVoltage { get; set; } = "0";  // 0x700 Data34
    public string BatteryCurrent { get; set; } = "0";    // 0x700 Data56
    public string BatterySOC { get; set; } = "0";        // 0x700 Data78

    public string LoStatus { get; set; } = "0";          // 0x710 Data12
    public string HiStatus { get; set; } = "0";          // 0x710 Data34
    public string RemainCp { get; set; } = "0";          // 0x710 Data56
    public string FullChargeCp { get; set; } = "0";      // 0x710 Data78

    public string SoH { get; set; } = "0";               // 0x720 Data12
    public string FirmwareVersion { get; set; } = "0";   // 0x720 Data34

    public string LoTS1Temp { get; set; } = "0";         // 0x730 Data12
    public string LoTS2Temp { get; set; } = "0";         // 0x730 Data34
    public string LoTS3Temp { get; set; } = "0";         // 0x730 Data56

    public string HiTS1Temp { get; set; } = "0";         // 0x740 Data12
    public string HiTS2Temp { get; set; } = "0";         // 0x740 Data34
    public string HiTS3Temp { get; set; } = "0";         // 0x740 Data56

    public string CellVoltage1 { get; set; } = "0";      // 0x800 Data12
    public string CellVoltage2 { get; set; } = "0";      // 0x800 Data34
    public string CellVoltage3 { get; set; } = "0";      // 0x800 Data56
    public string CellVoltage4 { get; set; } = "0";      // 0x800 Data78

    public string CellVoltage5 { get; set; } = "0";      // 0x810 Data12
    public string CellVoltage6 { get; set; } = "0";      // 0x810 Data34
    public string CellVoltage7 { get; set; } = "0";      // 0x810 Data56
    public string CellVoltage8 { get; set; } = "0";      // 0x810 Data78

    public string CellVoltage9 { get; set; } = "0";      // 0x820 Data12
    public string CellVoltage10 { get; set; } = "0";     // 0x820 Data34
    public string CellVoltage11 { get; set; } = "0";     // 0x820 Data56 
    public string CellVoltage12 { get; set; } = "0";     // 0x820 Data78

    public string CellVoltage13 { get; set; } = "0";     // 0x830 Data12
    public string CellVoltage14 { get; set; } = "0";     // 0x830 Data34
    public string CellVoltage15 { get; set; } = "0";     // 0x830 Data56
    public string CellVoltage16 { get; set; } = "0";     // 0x830 Data78

    public string CellVoltage17 { get; set; } = "0";     // 0x840 Data12
    public string CellVoltage18 { get; set; } = "0";     // 0x840 Data34
    public string CellVoltage19 { get; set; } = "0";     // 0x840 Data56
    public string CellVoltage20 { get; set; } = "0";     // 0x840 Data78

    public string CellVoltage21 { get; set; } = "0";     // 0x850 Data12
    public string CellVoltage22 { get; set; } = "0";     // 0x850 Data34

    // 以下為LoStatus
    public bool LoOC { get; set; } = false;    // 15
    public bool LoDSG { get; set; } = false;   // 13
    public bool LoCHG { get; set; } = false;   // 12
    public bool LoUTD { get; set; } = false;   //11
    public bool LoUTC { get; set; } = false;   //10
    public bool LoOTD { get; set; } = false;   //9
    public bool LoOTC { get; set; } = false;   //8
    public bool LoASCDL { get; set; } = false; //7
    public bool LoASCD { get; set; } = false;  //6
    public bool LoAOLDL { get; set; } = false;//5
    public bool LoAOLD { get; set; } = false;//4
    public bool LoOCD { get; set; } = false;//3
    public bool LoOCC { get; set; } = false;//2
    public bool LoCOV { get; set; } = false;//1
    public bool LoCUV { get; set; } = false;//0

    // 以下為HiStatus
    public bool HiOC { get; set; } = false; // 15
    public bool HiUTD { get; set; } = false; // 11
    public bool HiUTC { get; set; } = false; // 10
    public bool HiOTD { get; set; } = false; // 9
    public bool HiOTC { get; set; } = false; // 8
    public bool HiCOV { get; set; } = false; // 1
    public bool HiCUV { get; set; } = false; // 0

    public string CellVoltage23 { get; set; } = "0";
    public string CellVoltage24 { get; set; } = "0";
    public string CellVoltage25 { get; set; } = "0";

    public string CellVoltage26 { get; set; } = "0";
    public string CellVoltage27 { get; set; } = "0";
    public string CellVoltage28 { get; set; } = "0";
    public string CellVoltage29 { get; set; } = "0";
    public string CellVoltage30 { get; set; } = "0";

    public BatteryCanbus2P22SPara()
    {

    }

    //public BatteryCanbus2P22SPara(List<short> dataListMain)
    //{
    //  _dataList = dataListMain.ToList();
    //  Convert22s2pProperties();
    //}

    public List<bool> BytesToList(Int32 byte_value)
    {
      List<Int32> TMP_List1 = new List<Int32>();
      List<bool> BooltmpList = new List<bool>();
      Int32 int_value = (Int32)byte_value;
      for (Int32 i = 0; i < 16; i++)
      {
        Int32 TMP_int1 = (int_value) % 2;
        TMP_List1.Add(TMP_int1);
        BooltmpList.Add(Convert.ToBoolean(TMP_int1));
        int_value = int_value / 2;
      }

      //return TMP_List1;
      return BooltmpList;
    }
  }

  /// <summary>
  /// 嘉義中油60KW貨櫃型態(BMS)
  /// </summary>
  public class BatteryCanbusNew2P22SPBmsPara
  {
    private List<short> _dataList = new List<short>();
    public List<short> dataList
    {
      get { return _dataList; }
      set { _dataList = value; }
    }

    public string PackVoltage { get; set; } = "0";
    //public string PackCurrent { get; set; } = "0";
    //public string SoC { get; set; } ="0";
    //public string SOH { get; set; } ="0";

    public string CellVoltage1 { get; set; } = "0";
    public string CellVoltage2 { get; set; } = "0";
    public string CellVoltage3 { get; set; } = "0";
    public string CellVoltage4 { get; set; } = "0";
    public string CellVoltage5 { get; set; } = "0";

    public string CellVoltage6 { get; set; } = "0";
    public string CellVoltage7 { get; set; } = "0";
    public string CellVoltage8 { get; set; } = "0";
    public string CellVoltage9 { get; set; } = "0";
    public string CellVoltage10 { get; set; } = "0";

    public string CellVoltage11 { get; set; } = "0";
    public string CellVoltage12 { get; set; } = "0";
    public string CellVoltage13 { get; set; } = "0";
    public string CellVoltage14 { get; set; } = "0";
    public string CellVoltage15 { get; set; } = "0";

    public string CellVoltage16 { get; set; } = "0";
    public string CellVoltage17 { get; set; } = "0";
    public string CellVoltage18 { get; set; } = "0";
    public string CellVoltage19 { get; set; } = "0";
    public string CellVoltage20 { get; set; } = "0";

    public string CellVoltage21 { get; set; } = "0";
    public string CellVoltage22 { get; set; } = "0";
    public string deltaV { get; set; } = "0";

    public string Temperature1 { get; set; } = "0";
    public string Temperature2 { get; set; } = "0";
    public string Temperature3 { get; set; } = "0";
    public string Temperature4 { get; set; } = "0";
    public string Temperature5 { get; set; } = "0";

    public string Temperature6 { get; set; } = "0";
    public string Temperature7 { get; set; } = "0";
    public string Temperature8 { get; set; } = "0";
    public string Temperature9 { get; set; } = "0";
    public string Temperature10 { get; set; } = "0";

    public string Temperature11 { get; set; } = "0";
    public string Temperature12 { get; set; } = "0";

    //public string MBU_SYS_TEMP1 { get; set; } = "0";
    //public string MBU_SYS_TEMP2 { get; set; } = "0";

    //public string MBU_RELAY_TEMP1 { get; set; } = "0";
    //public string MBU_RELAY_TEMP2L { get; set; } = "0";
    //public string MBU_RELAY_TEMP2H { get; set; } = "0";
    //public string MBU_V_CHG { get; set; } = "0";
    //public string MBU_I_CHG { get; set; } = "0";

    public string FlagALL { get; set; } = "0"; // 16進制的全部FLAG集合體
    //public string Flag1 { get; set; } = "0";
    //public string Flag2 { get; set; } = "0";
    //public string Flag3 { get; set; } = "0";
    //public string Flag4 { get; set; } = "0";
    //public string Flag5 { get; set; } = "0";
    //public string Flag6 { get; set; } = "0";
    //public string Flag7 { get; set; } = "0";

    //public string Flag8 { get; set; } = "0";
    //public string Flag9 { get; set; } = "0";
    //public string Flag10 { get; set; } = "0";
    //public string Flag11 { get; set; } = "0";
    //public string Flag12 { get; set; } = "0";
    //public string Flag13 { get; set; } = "0";
    //public string Flag14 { get; set; } = "0";

    //// Flag3
    //public bool UIR1 { get; set; } = false;
    //public bool UIR2 { get; set; } = false;
    //public bool COV { get; set; } = false;
    //public bool CUV { get; set; } = false;
    //public bool MOV { get; set; } = false;
    //public bool MUV { get; set; } = false;
    //public bool ROV { get; set; } = false;
    //public bool RUV { get; set; } = false;

    //// Flag6
    //public bool COT { get; set; } = false;
    //public bool DOT { get; set; } = false;
    //public bool CUT { get; set; } = false;
    //public bool DUT { get; set; } = false;
    //public bool AOT { get; set; } = false;

  }



  /// <summary>
  /// 嘉義中油60KW貨櫃型態(BCU)
  /// </summary>
  public class BatteryCanbusNew2P22SPBcuPara
  {

    private List<short> _dataList = new List<short>();
    public List<short> dataList
    {
      get { return _dataList; }
      set { _dataList = value; }
    }
    public string HWVersion { get; set; } = "0";
    public string SWVersion { get; set; } = "0";

    public string ProtocolVersion { get; set; } = "0";
    public string ModuleSeries { get; set; } = "0";

    public string SoC { get; set; } = "0";
    public string SOH { get; set; } = "0";

    public string Status1ofsystem { get; set; } = "0"; // byto to list
    public string IRUState { get; set; } = "0"; // byto to list

    public string RackVoltage24Bits { get; set; } = "0";
    public string RackCurrent24Bits { get; set; } = "0";

    public string Flag1 { get; set; } = "0";
    public string Flag2 { get; set; } = "0";
    public string Flag3 { get; set; } = "0";
    public string Flag4 { get; set; } = "0";
    public string Flag5 { get; set; } = "0";
    public string Flag6 { get; set; } = "0";
    public string Flag7 { get; set; } = "0";

    public string Flag8 { get; set; } = "0";
    public string Flag9 { get; set; } = "0";
    public string Flag10 { get; set; } = "0";
    public string Flag11 { get; set; } = "0";
    public string Flag12 { get; set; } = "0";
    public string Flag13 { get; set; } = "0";
    public string Flag14 { get; set; } = "0";
    public string FlagALL { get; set; } = "0";

    public List<bool> BytesToList(Int32 byte_value)
    {
      List<Int32> TMP_List1 = new List<Int32>();
      List<bool> BooltmpList = new List<bool>();
      Int32 int_value = (Int32)byte_value;
      for (Int32 i = 0; i < 16; i++)
      {
        Int32 TMP_int1 = (int_value) % 2;
        TMP_List1.Add(TMP_int1);
        BooltmpList.Add(Convert.ToBoolean(TMP_int1));
        int_value = int_value / 2;
      }

      //return TMP_List1;
      return BooltmpList;
    }

    // Status1ofsystem bit15-0
    public bool EmergencyStopStatus1ofsystem { get; set; } = false; //15
    public bool CommunicationConnectClose { get; set; } = false; //14 通訊連接閉鎖

    // 0:shutdown  1:Idle 2:Normal 3:Alarm 4:Fault
    public string RunningStatus { get; set; } = ""; // 10-8
    // 0x3: turn on  0x0:turn off
    public string ActuatorInstruction { get; set; } = ""; //7-6

    public bool MCCB { get; set; } = false; //5

    public bool NegativeActuator { get; set; } = false; //4

    public bool PositiveActuator { get; set; } = false; //3
    public bool PrePositiveActuator { get; set; } = false; //2
    public bool EoC { get; set; } = false; //1 End of charge
    public bool EoD { get; set; } = false; //0 End of DisCharge

    // IRU State
    public bool IRUExist { get; set; } = false; //7
    public bool IRDetected10s { get; set; } = false; //6

    // 0:Normal 1:500ohm/V 2:100ohm/V 3:NotWorking
    public string IRFeature { get; set; } = ""; //2-0

    // Flag1 Status2ofSystemLowByte
    public bool CBStart { get; set; } = false; //0 Cell Balance Running
    public bool MBStart { get; set; } = false; //1 Module Balance Running
    public bool MIM { get; set; } = false; //2 Module Imbalance
    public bool CIM { get; set; } = false; //3 Cell Imbalance
    public bool SVE { get; set; } = false; //4 Sensing Voltage Error
    public bool SCE { get; set; } = false; //5 Sensing Current Error
    public bool SPE { get; set; } = false; //6 Sensing Temperatur Error

    // Flag2 Status2ofSystemHighByte
    public bool BMU_Comm_Miss { get; set; } = false; //0 Rack to BMU Communication Fault
    public bool EMS_Comm_Miss { get; set; } = false; //1 Rack to EMS Communication Fault
    public bool IRU_Comm_Miss { get; set; } = false; //2 Rack to IRU Communication Fault

    public bool MBU1_Comm_Miss { get; set; } = false; //4 Module balance Unit 1
    public bool MBU2_Comm_Miss { get; set; } = false; //5 Module balance Unit 2
    public bool MBU3_Comm_Miss { get; set; } = false; //6 Module balance Unit 3
    public bool MBU4_Comm_Miss { get; set; } = false; //7 Module balance Unit 4

    // Flag3 ProtectionStatus1ofSystemLowByte
    public bool UIR_1 { get; set; } = false; //0 Inder Insulation resistance
    public bool UIR_2 { get; set; } = false; //1 Inder Insulation resistance
    public bool COV { get; set; } = false; //2 Cell Over Voltage
    public bool CUV { get; set; } = false; //3 Cell Under Voltage
    public bool MOV { get; set; } = false; //4 Module Over Voltage
    public bool MUV { get; set; } = false; //5 Module Under Voltage
    public bool ROV { get; set; } = false; //6 Rack Over Voltage
    public bool RUV { get; set; } = false; //7 Rack Under Voltage

    // Flag4 ProtectionStatus1ofSystemHighByte
    public bool P2C { get; set; } = false; //0 Prot_2nd_Cable
    public bool Prot2nd { get; set; } = false; //1 Prot 2nd
    public bool EmergencyStopFlag { get; set; } = false; //2 
    public bool FalureMCCB { get; set; } = false; //5 module case circuit breaker failure 

    // Flag5 ProtectionStatus2ofSystemLowByte
    public bool COCP_1 { get; set; } = false; //1 Charge Over Current Protection level1
    public bool COCP_2 { get; set; } = false; //2 Charge Over Current Protection level2

    public bool DOCP_1 { get; set; } = false; //4 DisCharge Over Current Protection level1
    public bool DOCP_2 { get; set; } = false; //5 DisCharge Over Current Protection level2
    public bool DOCP_3 { get; set; } = false; //6 DisCharge Over Current Protection level3
    public bool DOCP_4 { get; set; } = false; //7 DisCharge Over Current Protection level4

    // Flag6 ProtectionStatus2ofSystemHighByte
    public bool COT { get; set; } = false; //0 charge over temperature
    public bool DOT { get; set; } = false; //1 Discharge over temperature
    public bool CUT { get; set; } = false; //2 charge under temperature
    public bool DUT { get; set; } = false; //3 discharge under temperature

    public bool AUT { get; set; } = false; //6 Reserved ??
    public bool AOT { get; set; } = false; //7 Actuator (Relay/Mos) over temperature

    // Flag7 Reserved

    // Flag8 Reserved

    // Flag9 ProtectionStatus4ofSystemPermanentFailureProtectionLowByte
    public bool UVPF { get; set; } = false; //0 under voltage permanent failure
    public bool OVPF { get; set; } = false; //1 over voltage permanent failure
    public bool OTPF { get; set; } = false; //2 over temperature permanent failure
    public bool FUSEPF { get; set; } = false; //3 FUSE permanent failure
    public bool CIMPF { get; set; } = false; //4 cell imbalance permanent failure
    public bool THOPF { get; set; } = false; //5 thermal sensor open circuit permanent failure
    public bool THSPF { get; set; } = false; //6 
    public bool AWPF { get; set; } = false; //7 Autuator weld permanent failure

    // Flag10 ProtectionStatus4ofSystemPermanentFailureProtectionHighByte Reserved

    // Flag11 Reserved

    // Flag12 Reserved

    // Flag13 Reserved

    // Flag14 Reserved

  }

}
