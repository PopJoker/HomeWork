using BMM_Battery_Mobus_Moniter_.Models;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BMM_Battery_Mobus_Moniter_ver0._2_.Forms
{
    public partial class FormPopup : Form
    {
        private string _comPort;
        private string _batteryId;

        public FormPopup()
        {
            InitializeComponent();
        }
        public string BatteryName
        {
            get => labelTitle.Text;
            set => labelTitle.Text = value;
        }

        public void SetDataLines(string[] lines)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SetDataLines(lines)));
                return;
            }
            TB_mainPage.Lines = lines;
        }
        public void LoadData(
            string comPort,
            string batteryId,
            Battery22S2P batteryData,
            string[] main = null,
            string[] cell = null,
            string[] temp = null,
            string[] low = null,
            string[] high = null)
        {
            _comPort = comPort;
            _batteryId = batteryId;

            this.Text = $"Battery: {batteryId} | Port: {comPort}";
            labelTitle.Text = $"Battery ID: {batteryId} (COM: {comPort})";

            if (main != null) TB_mainPage.Lines = main;
            if (cell != null) TB_cellPage.Lines = cell;
            if (temp != null) TB_tempPage.Lines = temp;

            ShowStatusFlags(batteryData);
        }

        // 新增：用於 Timer 呼叫持續更新 UI
        public void UpdateBatteryData(Battery22S2P batteryData)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateBatteryData(batteryData)));
                return;
            }

            // 更新三個 TextBox 內容（如果有）
            TB_mainPage.Lines = batteryData.ToString().Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            // 或如果你想更新個別 Tab 分頁內容可自訂
            // 這裡簡單示範以 ToString 輸出顯示在 mainPage

            ShowStatusFlags(batteryData);
        }

        private void ShowStatusFlags(Battery22S2P battery)
        {
            GB_LowStatus.Controls.Clear();
            GB_HiStatus.Controls.Clear();

            int labelWidth = GB_LowStatus.ClientSize.Width - 20; // GroupBox 寬度扣邊距
            int yLow = 20;
            var lowProps = typeof(Battery22S2P).GetProperties()
                .Where(p => p.PropertyType == typeof(bool) && p.Name.StartsWith("Lo"))
                .ToList();

            foreach (var prop in lowProps)
            {
                bool value = (bool)prop.GetValue(battery);

                var lbl = new Label
                {
                    Text = prop.Name.Substring(2),
                    AutoSize = false,
                    Size = new Size(labelWidth, 25),
                    Location = new Point(10, yLow),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = value ? Color.Green : Color.LightGray,
                    ForeColor = value ? Color.White : Color.Black,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold)
                };
                GB_LowStatus.Controls.Add(lbl);
                yLow += lbl.Height + 5;
            }

            int yHi = 20;
            var hiProps = typeof(Battery22S2P).GetProperties()
                .Where(p => p.PropertyType == typeof(bool) && p.Name.StartsWith("Hi"))
                .ToList();

            foreach (var prop in hiProps)
            {
                bool value = (bool)prop.GetValue(battery);

                var lbl = new Label
                {
                    Text = prop.Name.Substring(2),
                    AutoSize = false,
                    Size = new Size(labelWidth, 25),
                    Location = new Point(10, yHi),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = value ? Color.Green : Color.LightGray,
                    ForeColor = value ? Color.White : Color.Black,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold)
                };
                GB_HiStatus.Controls.Add(lbl);
                yHi += lbl.Height + 5;
            }
        }

        private void FormPopup_Load(object sender, EventArgs e)
        {

        }
    }
}
