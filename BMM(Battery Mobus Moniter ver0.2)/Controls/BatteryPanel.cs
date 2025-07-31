using BMM_Battery_Mobus_Moniter_ver0._2_.Forms;
using System;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Windows.Forms;

namespace BMM_Battery_Mobus_Moniter_ver0._2_.Controls
{
    public partial class BatteryPanel : UserControl
    {
        public string ComPort { get; set; }
        public string BatteryId { get; set; }


        public string Key => $"{ComPort}|{BatteryId}";

        private string[] _allLines = Array.Empty<string>();
        private int _linesPerPage = 28; // 每頁顯示多少行，可調整
        private int _currentPage = 0;
        private int _totalPages = 0;
        public int CellCount { get; private set; }

        public string DisplayText => string.Join(Environment.NewLine, _allLines);

        private Timer _flashTimer;
        private bool _flashOn = false;
        private Color _originalBackColor;

        public event EventHandler PanelClicked;
        private void BT_popup_Click(object sender, EventArgs e)
        {
            PanelClicked?.Invoke(this, EventArgs.Empty);
        }

        private string _batteryName;
        public string BatteryName
        {
            get => _batteryName;
            set
            {
                _batteryName = value;
                if (InvokeRequired)
                {
                    Invoke(new Action(() => labelBatteryName.Text = _batteryName));
                }
                else
                {
                    labelBatteryName.Text = _batteryName;
                }
            }
        }
        //For CellCount
        public int UpdateCellcount(int MaxCellCount)
        {
            if (int.TryParse(TB_CellCount.Text, out int parsed))
            {
                if (parsed<MaxCellCount)
                    CellCount = parsed;
                else
                    CellCount = MaxCellCount;
            }
            else
            {
                CellCount = MaxCellCount;
            }
            return CellCount;
        }


        public BatteryPanel()
        {
            InitializeComponent();

            _flashTimer = new Timer();
            _flashTimer.Interval = 500; // 閃爍間隔 500ms
            _flashTimer.Tick += FlashTimer_Tick;
            BackColor = Color.White;
            _originalBackColor = Color.White;
        }

        private void BatteryPanel_Load(object sender, EventArgs e)
        {
            UpdatePageButtons();
        }

        // 閃爍計時器事件
        private void FlashTimer_Tick(object sender, EventArgs e)
        {
            if (_flashOn)
            {
                this.BackColor = _originalBackColor;
            }
            else
            {
                this.BackColor = Color.Red;
            }
            _flashOn = !_flashOn;
        }

        public void StartFlashing()
        {
            if (!_flashTimer.Enabled)
            {
                _flashTimer.Start();
                _flashOn = false; // 確保從原色開始閃
            }
        }

        public void StopFlashing()
        {
            if (_flashTimer.Enabled)
            {
                _flashTimer.Stop();
                this.BackColor = _originalBackColor; // 還原原始背景色
            }
        }

        // 設定顯示的全部資料行
        public void SetDataLines(string[] lines)
        {
            if (lines == null)
            {
                lines = Array.Empty<string>();
            }


            // 資料沒變就不更新
            if (_allLines.SequenceEqual(lines))
                return;

            _allLines = lines;

            int newTotalPages = (_allLines.Length + _linesPerPage - 1) / _linesPerPage;

            if (_totalPages != newTotalPages)
            {
                _totalPages = newTotalPages;
                if (_currentPage >= _totalPages)
                {
                    _currentPage = Math.Max(0, _totalPages - 1);
                }
            }

            UpdateDisplayedText();
            UpdatePageButtons();
        }


        private void UpdateDisplayedText()
        {
            int startLine = _currentPage * _linesPerPage;
            int count = Math.Min(_linesPerPage, _allLines.Length - startLine);

            if (count <= 0)
            {
                if (textBoxData.Text != "")
                    textBoxData.Text = "";
                return;
            }

            var pageLines = new string[count];
            Array.Copy(_allLines, startLine, pageLines, 0, count);

            string newText = string.Join(Environment.NewLine, pageLines);

            // 如果內容不變，就不更新，避免刷新導致滾動條跳回頂端
            if (textBoxData.Text != newText)
            {
                textBoxData.Text = newText;
            }
        }


        private void UpdatePageButtons()
        {
            buttonPrevPage.Enabled = _currentPage > 0;
            buttonNextPage.Enabled = _currentPage < _totalPages - 1;
        }

        private void buttonPrevPage_Click(object sender, EventArgs e)
        {
            if (_currentPage > 0)
            {
                _currentPage--;
                UpdateDisplayedText();
                UpdatePageButtons();
            }
        }

        private void buttonNextPage_Click(object sender, EventArgs e)
        {
            if (_currentPage < _totalPages - 1)
            {
                _currentPage++;
                UpdateDisplayedText();
                UpdatePageButtons();
            }
        }

        private void textBoxData_TextChanged(object sender, EventArgs e)
        {
            // 可根據需求加內容變更處理
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }


    }
}
