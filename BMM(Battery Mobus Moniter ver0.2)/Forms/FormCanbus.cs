using BMM_Battery_Mobus_Moniter_ver0._2_.Controls;
using BMSHostMonitor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace BMM_Battery_Mobus_Moniter_ver0._2_.Forms
{
    public partial class FormCanbus : Form
    {
        canbusMainStructure canbus = new canbusMainStructure();
        int nowCs2Id = 0;

        // 儲存所有動態加入的 BatteryPanel (key: batteryId as string)
        private Dictionary<string, BatteryPanel> _batteryPanels = new Dictionary<string, BatteryPanel>();

        // 紀錄所有打開的 Popup (key: "comPort|batteryId")
        private Dictionary<string, FormPopup> _popups = new Dictionary<string, FormPopup>();

        public FormCanbus(canbusMainStructure _canbus, int _nowCs2Id)
        {
            InitializeComponent();
            canbus = _canbus;
            this.nowCs2Id = _nowCs2Id;
        }

        // 將讀取的資料更新到指定 BatteryPanel
        private void UpdateBatteryPanel(string batteryId)
        {
            if (!_batteryPanels.ContainsKey(batteryId)) return;
            if (canbus.TotalMasterDataChayi == null) return;

            var panel = _batteryPanels[batteryId];
            if (!int.TryParse(batteryId, out int id)) return;
            if (id < 0 || id >= canbus.TotalMasterDataChayi.Count) return;

            var data = canbus.TotalMasterDataChayi[id];

            var lines = new List<string>();

            // 基本資訊示範
            double voltage = Convert.ToDouble(canbus.RackDataChayiBCU.RackVoltage24Bits) / 1000;
            double current = Convert.ToDouble(canbus.RackDataChayiBCU.RackCurrent24Bits) / 1000;
            double soc = Convert.ToDouble(canbus.RackDataChayiBCU.SoC) / 10;
            double soh = Convert.ToDouble(canbus.RackDataChayiBCU.SOH) / 10;

            lines.Add($"電壓: {voltage:#0.000} V");
            lines.Add($"電流: {current:#0.000} A");
            lines.Add($"SOC: {soc:#0.0} %");
            lines.Add($"SOH: {soh:#0.0} %");
            lines.Add(""); // 空行

            // CellVoltage1~22
            for (int i = 1; i <= 22; i++)
            {
                var prop = data.GetType().GetProperty($"CellVoltage{i}");
                if (prop != null)
                {
                    var val = prop.GetValue(data);
                    lines.Add($"Cell {i}: {val}");
                }
            }

            lines.Add(""); // 空行

            // Temperature1~12 (除以100)
            for (int i = 1; i <= 12; i++)
            {
                var prop = data.GetType().GetProperty($"Temperature{i}");
                if (prop != null)
                {
                    var val = prop.GetValue(data);
                    if (val != null && double.TryParse(val.ToString(), out double t))
                    {
                        lines.Add($"溫度 {i}: {t / 100:#0.00} °C");
                    }
                }
            }

            lines.Add(""); // 空行

            // 狀態標記字串（可以依照需求擴充）
            var statusObj = canbus.RackDataChayiBCU;

            var statusFlags = new List<string>();

            var statusProperties = new Dictionary<string, bool>
            {
                { "CBStart", statusObj.CBStart },
                { "MBStart", statusObj.MBStart },
                { "MIM", statusObj.MIM },
                { "CIM", statusObj.CIM },
                { "SVE", statusObj.SVE },
                { "SCE", statusObj.SCE },
                { "SPE", statusObj.SPE },
                { "BMU_Comm_Miss", statusObj.BMU_Comm_Miss },
                { "EMS_Comm_Miss", statusObj.EMS_Comm_Miss },
                { "IRU_Comm_Miss", statusObj.IRU_Comm_Miss },
                { "MBU1_Comm_Miss", statusObj.MBU1_Comm_Miss },
                { "MBU2_Comm_Miss", statusObj.MBU2_Comm_Miss },
                { "MBU3_Comm_Miss", statusObj.MBU3_Comm_Miss },
                { "MBU4_Comm_Miss", statusObj.MBU4_Comm_Miss },
                { "UIR_1", statusObj.UIR_1 },
                { "UIR_2", statusObj.UIR_2 },
                { "COV", statusObj.COV },
                { "CUV", statusObj.CUV },
                { "MOV", statusObj.MOV },
                { "MUV", statusObj.MUV },
                { "ROV", statusObj.ROV },
                { "RUV", statusObj.RUV },
                { "P2C", statusObj.P2C },
                { "Prot2nd", statusObj.Prot2nd },
                { "EmergencyStopFlag", statusObj.EmergencyStopFlag },
                { "FalureMCCB", statusObj.FalureMCCB },
                { "COCP_1", statusObj.COCP_1 },
                { "COCP_2", statusObj.COCP_2 },
                { "DOCP_1", statusObj.DOCP_1 },
                { "DOCP_2", statusObj.DOCP_2 },
                { "DOCP_3", statusObj.DOCP_3 },
                { "DOCP_4", statusObj.DOCP_4 },
                { "COT", statusObj.COT },
                { "DOT", statusObj.DOT },
                { "CUT", statusObj.CUT },
                { "DUT", statusObj.DUT },
                { "AUT", statusObj.AUT },
                { "AOT", statusObj.AOT },
                { "UVPF", statusObj.UVPF },
                { "OVPF", statusObj.OVPF },
                { "OTPF", statusObj.OTPF },
                { "FUSEPF", statusObj.FUSEPF },
                { "CIMPF", statusObj.CIMPF },
                { "THOPF", statusObj.THOPF },
                { "THSPF", statusObj.THSPF },
                { "AWPF", statusObj.AWPF },
            };

            foreach (var kvp in statusProperties)
            {
                if (kvp.Value) statusFlags.Add(kvp.Key);
            }

            lines.Add("Status:");
            if (statusFlags.Count > 0)
                lines.AddRange(statusFlags);
            else
                lines.Add("None");

            panel.SetDataLines(lines.ToArray());

            // 如果 Popup 是開啟狀態，也同步更新 Popup 內容
            string key = panel.BatteryId;
            if (_popups.TryGetValue(key, out FormPopup popup))
            {
                popup.SetDataLines(lines.ToArray());
            }
        }

        // 新增多個 BatteryPanel (會忽略重複的)
        public void AddBatteryPanels(string comPort, List<string> batteryIds)
        {
            if (batteryIds == null || batteryIds.Count == 0)
                return;

            foreach (var batteryId in batteryIds)
            {
                if (_batteryPanels.ContainsKey(batteryId))
                    continue; // 已存在跳過

                var panel = new BatteryPanel()
                {
                    ComPort = comPort,
                    BatteryId = batteryId,
                    BatteryName = $"Battery {batteryId}",
                    Width = 250,
                    Height = 200,
                    Margin = new Padding(5),
                    Dock = DockStyle.None
                };

                panel.PanelClicked += Panel_PanelClicked;

                tableLayoutPanelPanels.Controls.Add(panel);
                _batteryPanels[batteryId] = panel;
            }

            tableLayoutPanelPanels.PerformLayout();
        }

        // Panel 點擊事件，顯示 Popup
        private void Panel_PanelClicked(object sender, EventArgs e)
        {
            if (sender is BatteryPanel panel)
            {
                string batteryId = panel.BatteryId;
                string key = batteryId;

                if (!int.TryParse(batteryId, out int id))
                {
                    MessageBox.Show("BatteryId 格式錯誤");
                    return;
                }

                if (canbus.TotalMasterDataChayi == null || id < 0 || id >= canbus.TotalMasterDataChayi.Count)
                {
                    MessageBox.Show("找不到對應的 Canbus 電池資料");
                    return;
                }

                var data = canbus.TotalMasterDataChayi[id];

                var lines = new List<string>();

                double voltage = Convert.ToDouble(canbus.RackDataChayiBCU.RackVoltage24Bits) / 1000;
                double current = Convert.ToDouble(canbus.RackDataChayiBCU.RackCurrent24Bits) / 1000;
                double soc = Convert.ToDouble(canbus.RackDataChayiBCU.SoC) / 10;
                double soh = Convert.ToDouble(canbus.RackDataChayiBCU.SOH) / 10;

                lines.Add($"電壓: {voltage:#0.000} V");
                lines.Add($"電流: {current:#0.000} A");
                lines.Add($"SOC: {soc:#0.0} %");
                lines.Add($"SOH: {soh:#0.0} %");
                lines.Add("");

                for (int i = 1; i <= 22; i++)
                {
                    var prop = data.GetType().GetProperty($"CellVoltage{i}");
                    if (prop != null)
                    {
                        var val = prop.GetValue(data);
                        lines.Add($"Cell {i}: {val}");
                    }
                }
                lines.Add("");
                for (int i = 1; i <= 12; i++)
                {
                    var prop = data.GetType().GetProperty($"Temperature{i}");
                    if (prop != null)
                    {
                        var val = prop.GetValue(data);
                        if (val != null && double.TryParse(val.ToString(), out double t))
                        {
                            lines.Add($"溫度 {i}: {t / 100:#0.00} °C");
                        }
                    }
                }
                lines.Add("");

                // 你也可以把狀態字串加進來，這裡省略

                // 檢查是否已開啟 Popup，避免重複開啟
                if (_popups.TryGetValue(key, out FormPopup existingPopup))
                {
                    existingPopup.BringToFront();
                    existingPopup.SetDataLines(lines.ToArray());
                }
                else
                {
                    var popup = new FormPopup();
                    popup.BatteryName = panel.BatteryName;
                    popup.SetDataLines(lines.ToArray());
                    popup.FormClosed += (s, args) =>
                    {
                        _popups.Remove(key);
                    };
                    popup.Show();
                    _popups[key] = popup;
                }
            }
        }

        // 輪詢 Canbus 資料的 BackgroundWorker
        private void FormCanbus_Load(object sender, EventArgs e)
        {
            BgwCs2PollingCmd.RunWorkerAsync();
        }

        private void BgwCs2PollingCmd_DoWork(object sender, DoWorkEventArgs e)
        {
            Stopwatch t1 = Stopwatch.StartNew();

            uint rackid = 1;
            canbus.csReadRackInfo(rackid);

            foreach (var id in canbus.idList)
            {
                canbus.csReadBmsAll(rackid, id);
                BgwCs2PollingCmd.ReportProgress(0, t1.ElapsedMilliseconds.ToString());
            }

            t1.Stop();
            BgwCs2PollingCmd.ReportProgress(1, t1.ElapsedMilliseconds.ToString());

            Thread.Sleep(10);
        }

        private void BgwCs2PollingCmd_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            foreach (var batteryId in _batteryPanels.Keys.ToList())
            {
                UpdateBatteryPanel(batteryId);
            }
        }

        private void BgwCs2PollingCmd_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BgwCs2PollingCmd.RunWorkerAsync();
        }
    }
}
