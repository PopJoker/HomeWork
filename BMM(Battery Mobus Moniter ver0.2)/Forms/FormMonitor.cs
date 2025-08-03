using BMM_Battery_Mobus_Moniter_.Models;
using BMM_Battery_Mobus_Moniter_ver0._2_.Controls;
using BMM_Battery_Mobus_Moniter_ver0._2_.Utils;
using MM_ModbusMonitor;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;



namespace BMM_Battery_Mobus_Moniter_ver0._2_.Forms
{
    public partial class FormMonitor : Form
    {
        private string _logPath = @"C:\Logs";
        string soundPath = Path.Combine(Application.StartupPath, "Alert", "error_sound-221445.wav");
        private SoundPlayer player;
        private bool isPlaying = false;

        private Dictionary<string, ModbusService> _modbusServices = new Dictionary<string, ModbusService>();
        private Dictionary<string, BatteryPanel> _batteryPanels = new Dictionary<string, BatteryPanel>();
        private Dictionary<string, Logger> _loggers = new Dictionary<string, Logger>();
        private Dictionary<string, string> _loggerNames = new Dictionary<string, string>();
        private Dictionary<string, FormPopup> _popups = new Dictionary<string, FormPopup>();

        private System.Windows.Forms.Timer _uiTimer;

        // 紀錄每個 key 上一次寫入的秒數，用來避免同秒重複寫
        private Dictionary<string, int> _lastLogSeconds = new Dictionary<string, int>();
        private readonly object _logTimeLock = new object();

        private bool _isLogging = false;

        private double maxThreshold = 4200;
        private double minThreshold = -1;
        private double deltaThreshold = 50;
        private double tempThreshold = 50;

        private Dictionary<BatteryPanel, bool> flashingPanels = new Dictionary<BatteryPanel, bool>();
        private System.Windows.Forms.Timer flashTimer;
        private bool isFlashOn = false;

        private int cellcount = 0;

        public FormMonitor()
        {
            InitializeComponent();

            soundPath = Path.Combine(Application.StartupPath, "Alert", "error_sound-221445.wav");
            player = new SoundPlayer(soundPath); // 初始化一次
            textBoxLogPath.Text = _logPath;

            TB_Cellmax.Text = maxThreshold.ToString();
            TB_DeltaCell.Text = deltaThreshold.ToString();
            TB_CellMin.Text = minThreshold.ToString();
            TB_TempMax.Text = tempThreshold.ToString();

            SetupTableLayoutPanel();
            StartTimer();

            flashTimer = new System.Windows.Forms.Timer();
            flashTimer.Interval = 500;
            flashTimer.Tick += FlashTimer_Tick;
            flashTimer.Start();


        }

        private void FlashTimer_Tick(object sender, EventArgs e)
        {
            foreach (var kvp in flashingPanels.ToList())
            {
                var panel = kvp.Key;
                panel.BackColor = isFlashOn ? Color.Red : Color.White;
            }
            isFlashOn = !isFlashOn;

            LB_Time.Text = DateTime.Now.ToString("yyyy/MM/dd\nHH:mm:ss");
        }

        private void StartTimer()
        {
            if (_uiTimer != null) return;

            _uiTimer = new System.Windows.Forms.Timer();
            _uiTimer.Interval = 200; // 200ms 執行一次
            _uiTimer.Tick += UiTimer_Tick;
            _uiTimer.Start();
        }

        private async void UiTimer_Tick(object sender, EventArgs e)
        {
            if (_isLogging) return; // 防止重入
            _isLogging = true;

            int nowSeconds = DateTime.Now.Second;

            try
            {
                foreach (var kvp in _batteryPanels)
                {
                    string key = kvp.Key;
                    var panel = kvp.Value;

                    bool canLog = false;
                    lock (_logTimeLock)
                    {
                        if (!_lastLogSeconds.TryGetValue(key, out int lastSecond))
                        {
                            canLog = true;
                            _lastLogSeconds[key] = nowSeconds;
                        }
                        else if (nowSeconds != lastSecond)
                        {
                            canLog = true;
                            _lastLogSeconds[key] = nowSeconds;
                        }
                    }

                    var parts = key.Split('|');
                    if (parts.Length != 2) continue;

                    string comPort = parts[0];
                    string batteryId = parts[1];

                    if (!_modbusServices.TryGetValue(comPort, out ModbusService modbus)) continue;

                    int slaveId;
                    if (batteryId == "HV") slaveId = 0;
                    else if (batteryId.StartsWith("ID") && int.TryParse(batteryId.Substring(2), out int id)) slaveId = id;
                    else continue;

                    // 非同步讀取避免UI卡頓
                    ushort[] registers = await Task.Run(() => modbus.ReadHoldingRegisters((byte)slaveId, 0, 38));

                    cellcount = panel.UpdateCellcount(22);
                    Battery22S2P battery = new Battery22S2P(cellcount);
                    battery.ParseFromRegisters(registers);

                    Console.WriteLine($"cellcount = {cellcount}");
                    double maxCell = battery.CellVoltages.Take(cellcount).Max();
                    double minCell = battery.CellVoltages.Take(cellcount).Min();
                    double deltaCell = maxCell - minCell;
                    double maxTemp = battery.Temperatures.Max();
                    if (maxCell >= maxThreshold || deltaCell >= deltaThreshold || minCell <= minThreshold || maxTemp >= tempThreshold)
                    {
                        flashingPanels[panel] = true;

                        if (!isPlaying)
                        {
                            if (File.Exists(player.SoundLocation))
                            {
                                player.PlayLooping();
                                isPlaying = true;
                            }
                            else
                            {
                                MessageBox.Show("找不到音效檔案：" + player.SoundLocation);
                            }
                        }
                    }
                    else
                    {
                        if (flashingPanels.ContainsKey(panel))
                        {
                            flashingPanels.Remove(panel);
                            panel.BackColor = Color.White;

                            if (isPlaying)
                            {
                                player.Stop();
                                isPlaying = false;
                            }
                        }
                    }


                    UpdateBatteryPanelUI(panel, battery);

                    if (_popups.TryGetValue(key, out var popup))
                    {
                        popup.UpdateBatteryData(battery);
                    }


                    if (canLog && _loggers.TryGetValue(key, out Logger logger))
                    {
                        // 非同步寫檔
                        await Task.Run(() => logger.LogBatteryData(battery));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("UiTimer_Tick Exception: " + ex.Message);
            }
            finally
            {
                _isLogging = false;
            }
        }

        private void UpdateBatteryPanelUI(BatteryPanel panel, Battery22S2P battery)
        {
            if (panel.InvokeRequired)
            {
                panel.Invoke(new Action(() => UpdateBatteryPanelUI(panel, battery)));
                return;
            }
            panel.SetDataLines(battery.ToString().Split(new[] { "\r\n", "\n" }, StringSplitOptions.None));
        }

        private void SetupTableLayoutPanel()
        {
            tableLayoutPanelPanels.Controls.Clear();
            tableLayoutPanelPanels.ColumnStyles.Clear();
            tableLayoutPanelPanels.RowStyles.Clear();

            tableLayoutPanelPanels.ColumnCount = 5;
            tableLayoutPanelPanels.RowCount = 2;

            for (int i = 0; i < 5; i++)
            {
                tableLayoutPanelPanels.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            }
            for (int j = 0; j < 2; j++)
            {
                tableLayoutPanelPanels.RowStyles.Add(new RowStyle(SizeType.Absolute, 400F));
            }
        }

        private void AddPanelToTableLayout(BatteryPanel panel)
        {
            int index = tableLayoutPanelPanels.Controls.Count;
            int row = index / tableLayoutPanelPanels.ColumnCount;
            int col = index % tableLayoutPanelPanels.ColumnCount;

            tableLayoutPanelPanels.Controls.Add(panel, col, row);
        }

        public void AddConnection(string comPort, List<string> batteryIds, ModbusService modbusService, Dictionary<string, string> idNameMap)
        {
            if (string.IsNullOrWhiteSpace(comPort) || batteryIds == null || modbusService == null)
                return;

            if (!_modbusServices.ContainsKey(comPort))
                _modbusServices[comPort] = modbusService;

            foreach (var batteryId in batteryIds)
            {
                // 取得自訂名稱，如果沒有就用原本 batteryId
                string customName = batteryId;
                if (idNameMap != null && idNameMap.ContainsKey(batteryId) && !string.IsNullOrWhiteSpace(idNameMap[batteryId]))
                    customName = idNameMap[batteryId];

                string name = $"{customName} ({comPort})";
                string key = $"{comPort}|{batteryId}";

                // 更新或建立 BatteryPanel
                if (_batteryPanels.ContainsKey(key))
                {
                    _batteryPanels[key].BatteryName = name;
                }
                else
                {
                    var panel = new BatteryPanel()
                    {
                        BatteryName = name,
                        Width = 340,
                        Height = 400,
                        Margin = new Padding(5),
                        ComPort = comPort,
                        BatteryId = batteryId
                    };

                     panel.PanelClicked += Panel_PanelClicked;

                    AddPanelToTableLayout(panel);
                    _batteryPanels[key] = panel;
                }

                // 檢查 Logger 是否存在且名稱是否不同
                if (_loggers.ContainsKey(key))
                {
                    if (_loggerNames.ContainsKey(key) && _loggerNames[key] != customName)
                    {
                        // 名稱變了，關閉舊的 Logger 並建立新 Logger
                        _loggers.Remove(key);
                        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
                        string fileName = $"{customName}_{comPort}_{timestamp}.csv";
                        _loggers[key] = new Logger(_logPath, comPort, batteryId, cellcount,6, fileName);
                        _loggerNames[key] = customName;
                    }
                }
                else
                {
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
                    string fileName = $"{customName}_{comPort}_{timestamp}.csv";
                    //以後做其他的要記得增加
                    cellcount = 22;
                    _loggers[key] = new Logger(_logPath, comPort, batteryId, cellcount, 6, fileName);
                    _loggerNames[key] = customName;
                }
            }
        }




        private void FormMonitor_FormClosing(object sender, FormClosingEventArgs e)
        {
            _uiTimer?.Stop();
            _uiTimer?.Dispose();
            _uiTimer = null;
        }
        //panel
        private async void Panel_PanelClicked(object sender, EventArgs e)
        {
            if (sender is BatteryPanel panel)
            {
                var key = $"{panel.ComPort}|{panel.BatteryId}";

                var popup = new FormPopup();

                PanelDataSplitter.SplitToSections(
                    panel.DisplayText,
                    out var main, out var cell, out var temp, out var lo, out var hi);

                if (_modbusServices.TryGetValue(panel.ComPort, out var modbus))
                {
                    int slaveId = 0;
                    if (panel.BatteryId == "HV") slaveId = 0;
                    else if (panel.BatteryId.StartsWith("ID") && int.TryParse(panel.BatteryId.Substring(2), out int id))
                        slaveId = id;

                    ushort[] registers = await Task.Run(() => modbus.ReadHoldingRegisters((byte)slaveId, 0, 38));
                    Battery22S2P battery = new Battery22S2P(22);
                    battery.ParseFromRegisters(registers);

                    popup.LoadData(panel.ComPort, panel.BatteryId, battery, main, cell, temp, lo, hi);
                    popup.Show();

                    // 記錄 popup
                    _popups[key] = popup;
                }
                else
                {
                    MessageBox.Show("找不到 ModbusService: " + panel.ComPort);
                }
            }
        }







        private void BT_Browse_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "請選擇紀錄檔資料夾路徑";
                folderDialog.SelectedPath = textBoxLogPath.Text;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    textBoxLogPath.Text = folderDialog.SelectedPath;
                    _logPath = textBoxLogPath.Text;

                    foreach (var logger in _loggers.Values)
                    {
                        logger.ChangeLogFilePath(_logPath);
                    }
                }
            }
        }

        private void BT_apply_Click(object sender, EventArgs e)
        {
            if (double.TryParse(TB_Cellmax.Text, out double max))
                maxThreshold = max;

            if (double.TryParse(TB_DeltaCell.Text, out double delta))
                deltaThreshold = delta;

            if (double.TryParse(TB_CellMin.Text, out double min))
                minThreshold = min;

            if (double.TryParse(TB_TempMax.Text, out double temp))
                tempThreshold = temp;
        }

        private void textBoxLogPath_TextChanged(object sender, EventArgs e)
        {
        }

        private void tableLayoutPanelPanels_Paint(object sender, PaintEventArgs e)
        {
        }

        private void FormMonitor_Load(object sender, EventArgs e)
        {
        }

        private void panel5_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
