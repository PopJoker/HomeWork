using BMM_Battery_Mobus_Moniter_ver0._2_.Forms;
using BMSHostMonitor;
using MM_ModbusMonitor;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Windows.Forms;

namespace BMM_Battery_Mobus_Moniter_ver0._2_
{
    public partial class FormMain : Form
    {
        private FormMonitor _monitorForm;
        private Dictionary<string, ModbusService> _modbusServices = new Dictionary<string, ModbusService>();
        canbusMainStructure canbus = new canbusMainStructure();

        public FormMain()
        {
            InitializeComponent();
            LoadAvailableComPorts();
        }

        private void BT_Connect_Click(object sender, EventArgs e)
        {
            if (!CbCs2Polling.Checked)
            {
                string selectedCOM = CMB_ComChoice.SelectedItem?.ToString();
                if (string.IsNullOrEmpty(selectedCOM) || selectedCOM == "無可用 COM")
                {
                    MessageBox.Show("請選擇 COM port！");
                    return;
                }

                List<string> selectedIDs = new List<string>();
                if (CB_ID0.Checked) selectedIDs.Add("HV");
                if (CB_ID1.Checked) selectedIDs.Add("ID1");
                if (CB_ID2.Checked) selectedIDs.Add("ID2");
                if (CB_ID3.Checked) selectedIDs.Add("ID3");
                if (CB_ID4.Checked) selectedIDs.Add("ID4");
                if (CB_ID5.Checked) selectedIDs.Add("ID5");
                if (CB_ID6.Checked) selectedIDs.Add("ID6");
                if (CB_ID7.Checked) selectedIDs.Add("ID7");

                if (selectedIDs.Count == 0)
                {
                    MessageBox.Show("請至少勾選一個 ID！");
                    return;
                }

                // 建立 ID 名稱映射表
                Dictionary<string, string> idNameMap = new Dictionary<string, string>();

                foreach (var id in selectedIDs)
                {
                    string customName = id; // 預設是 ID 名稱本身

                    switch (id)
                    {
                        case "HV":
                            if (!string.IsNullOrWhiteSpace(TB_HVname.Text))
                                customName = TB_HVname.Text.Trim();
                            break;
                        case "ID1":
                            if (!string.IsNullOrWhiteSpace(TB_ID1name.Text))
                                customName = TB_ID1name.Text.Trim();
                            break;
                        case "ID2":
                            if (!string.IsNullOrWhiteSpace(TB_ID2name.Text))
                                customName = TB_ID2name.Text.Trim();
                            break;
                        case "ID3":
                            if (!string.IsNullOrWhiteSpace(TB_ID3name.Text))
                                customName = TB_ID3name.Text.Trim();
                            break;
                        case "ID4":
                            if (!string.IsNullOrWhiteSpace(TB_ID4name.Text))
                                customName = TB_ID4name.Text.Trim();
                            break;
                        case "ID5":
                            if (!string.IsNullOrWhiteSpace(TB_ID5name.Text))
                                customName = TB_ID5name.Text.Trim();
                            break;
                        case "ID6":
                            if (!string.IsNullOrWhiteSpace(TB_ID6name.Text))
                                customName = TB_ID6name.Text.Trim();
                            break;
                        case "ID7":
                            if (!string.IsNullOrWhiteSpace(TB_ID7name.Text))
                                customName = TB_ID7name.Text.Trim();
                            break;
                    }

                    idNameMap[id] = customName;
                }

                if (!_modbusServices.TryGetValue(selectedCOM, out ModbusService modbusService))
                {
                    modbusService = new ModbusService();

                    bool connected = modbusService.Connect(
                        selectedCOM,
                        115200,
                        Parity.None,
                        8,
                        StopBits.One);

                    if (!connected)
                    {
                        MessageBox.Show("連接失敗，請檢查設備與連線設定！");
                        return;
                    }

                    _modbusServices[selectedCOM] = modbusService;
                    MessageBox.Show($"已連接 {selectedCOM}。");
                }

                if (_monitorForm == null || _monitorForm.IsDisposed)
                {
                    _monitorForm = new FormMonitor();
                    _monitorForm.Show();
                }

                // 傳入自訂名稱字典，讓 FormMonitor 取用顯示及記錄檔名
                _monitorForm.AddConnection(selectedCOM, selectedIDs, modbusService, idNameMap);
                _monitorForm.BringToFront();
            }
            else {
                 FormCanbus canbusForm = new FormCanbus(canbus, 1);
                    canbusForm.Show();
            }
        }


        private void LoadAvailableComPorts()
        {
            CMB_ComChoice.Items.Clear();

            string[] ports = SerialPort.GetPortNames();
            Array.Sort(ports);

            if (ports.Length > 0)
            {
                CMB_ComChoice.Items.AddRange(ports);
                CMB_ComChoice.SelectedIndex = 0;
            }
            else
            {
                CMB_ComChoice.Items.Add("無可用 COM");
                CMB_ComChoice.SelectedIndex = 0;
            }
        }

        private void FormMain_Load(object sender, EventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void CbCs2Polling_CheckedChanged(object sender, EventArgs e)
        {
            if (CbCs2Polling.Checked)
            {
                // 先檢查USB裝置是否有接上
                if (canbus.CheckDevice() != true)
                {
                    //CbPoll.Checked = false;
                    MessageBox.Show("USB 檢查異常,離開輪詢讀取!!!");
                    return;
                }

                if (canbus.startDevice() != true)
                {
                    MessageBox.Show("開啟 DEVICE 有誤無法開啟!");
                    //CbPoll.Checked = false;
                    return;
                }

                // 確認要收集的id
                canbus.idList = new List<int>();

                if (CB_ID1.Checked)
                    canbus.idList.Add(1);
                if (CB_ID2.Checked)
                    canbus.idList.Add(2);
                if (CB_ID3.Checked)
                    canbus.idList.Add(3);
                if (CB_ID4.Checked)
                    canbus.idList.Add(4);
                if (CB_ID5.Checked)
                    canbus.idList.Add(5);
                if (CB_ID6.Checked)
                    canbus.idList.Add(6);
                if (CB_ID7.Checked)
                    canbus.idList.Add(7);
                /*if (CB_ID8.Checked)
                    canbus.idList.Add(8);
                if (Cb2Id9.Checked)
                    canbus.idList.Add(9);
                if (Cb2Id10.Checked)
                    canbus.idList.Add(10);*/

                CMB_ComChoice.DataSource = canbus.idList;

            }

            // 由於是手動發命令，所以要識別需要發的命令
            //canbus.csReadBmsCell(4);
            //canbus.csReadBmsTemperature(4);
            //canbus.csReadBmsProtection(4);

        }
    }
}
