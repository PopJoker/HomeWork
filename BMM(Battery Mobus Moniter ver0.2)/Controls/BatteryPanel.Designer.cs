using System.Windows.Forms;

namespace BMM_Battery_Mobus_Moniter_ver0._2_.Controls
{
    partial class BatteryPanel
    {
        /// <summary> 
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清除任何使用中的資源。
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region 元件設計工具產生的程式碼

        private void InitializeComponent()
        {
            this.labelBatteryName = new System.Windows.Forms.Label();
            this.buttonPrevPage = new System.Windows.Forms.Button();
            this.buttonNextPage = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.TB_CellCount = new System.Windows.Forms.TextBox();
            this.textBoxData = new System.Windows.Forms.RichTextBox();
            this.BT_popup = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelBatteryName
            // 
            this.labelBatteryName.AutoSize = true;
            this.labelBatteryName.Font = new System.Drawing.Font("文泉驛微米黑", 14F, System.Drawing.FontStyle.Bold);
            this.labelBatteryName.Location = new System.Drawing.Point(4, 10);
            this.labelBatteryName.Name = "labelBatteryName";
            this.labelBatteryName.Size = new System.Drawing.Size(119, 28);
            this.labelBatteryName.TabIndex = 0;
            this.labelBatteryName.Text = "Battery X";
            // 
            // buttonPrevPage
            // 
            this.buttonPrevPage.Location = new System.Drawing.Point(8, 39);
            this.buttonPrevPage.Name = "buttonPrevPage";
            this.buttonPrevPage.Size = new System.Drawing.Size(41, 25);
            this.buttonPrevPage.TabIndex = 2;
            this.buttonPrevPage.Text = "<";
            this.buttonPrevPage.UseVisualStyleBackColor = true;
            this.buttonPrevPage.Click += new System.EventHandler(this.buttonPrevPage_Click);
            // 
            // buttonNextPage
            // 
            this.buttonNextPage.Location = new System.Drawing.Point(55, 39);
            this.buttonNextPage.Name = "buttonNextPage";
            this.buttonNextPage.Size = new System.Drawing.Size(43, 25);
            this.buttonNextPage.TabIndex = 3;
            this.buttonNextPage.Text = ">";
            this.buttonNextPage.UseVisualStyleBackColor = true;
            this.buttonNextPage.Click += new System.EventHandler(this.buttonNextPage_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.BT_popup);
            this.panel1.Controls.Add(this.labelBatteryName);
            this.panel1.Controls.Add(this.buttonPrevPage);
            this.panel1.Controls.Add(this.buttonNextPage);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(384, 70);
            this.panel1.TabIndex = 4;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.label2);
            this.panel3.Controls.Add(this.TB_CellCount);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel3.Location = new System.Drawing.Point(0, 355);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(384, 47);
            this.panel3.TabIndex = 5;
            this.panel3.Paint += new System.Windows.Forms.PaintEventHandler(this.panel3_Paint);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("文泉驛微米黑", 16F);
            this.label2.Location = new System.Drawing.Point(8, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(131, 31);
            this.label2.TabIndex = 1;
            this.label2.Text = "CellCount";
            // 
            // TB_CellCount
            // 
            this.TB_CellCount.Font = new System.Drawing.Font("新細明體", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.TB_CellCount.Location = new System.Drawing.Point(145, 4);
            this.TB_CellCount.Name = "TB_CellCount";
            this.TB_CellCount.Size = new System.Drawing.Size(100, 40);
            this.TB_CellCount.TabIndex = 0;
            // 
            // textBoxData
            // 
            this.textBoxData.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.textBoxData.Font = new System.Drawing.Font("文泉驛微米黑", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxData.Location = new System.Drawing.Point(0, 93);
            this.textBoxData.Name = "textBoxData";
            this.textBoxData.ReadOnly = true;
            this.textBoxData.Size = new System.Drawing.Size(384, 262);
            this.textBoxData.TabIndex = 1;
            this.textBoxData.Text = "";
            this.textBoxData.WordWrap = false;
            this.textBoxData.TextChanged += new System.EventHandler(this.textBoxData_TextChanged);
            // 
            // BT_popup
            // 
            this.BT_popup.Location = new System.Drawing.Point(104, 39);
            this.BT_popup.Name = "BT_popup";
            this.BT_popup.Size = new System.Drawing.Size(43, 25);
            this.BT_popup.TabIndex = 4;
            this.BT_popup.Text = "/";
            this.BT_popup.UseVisualStyleBackColor = true;
            this.BT_popup.Click += new System.EventHandler(this.BT_popup_Click);
            // 
            // BatteryPanel
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.textBoxData);
            this.Controls.Add(this.panel3);
            this.Font = new System.Drawing.Font("文泉驛微米黑", 9F);
            this.Name = "BatteryPanel";
            this.Size = new System.Drawing.Size(384, 402);
            this.Load += new System.EventHandler(this.BatteryPanel_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);

        }


        #endregion

        private System.Windows.Forms.Label labelBatteryName;
        private System.Windows.Forms.Button buttonPrevPage;
        private System.Windows.Forms.Button buttonNextPage;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox TB_CellCount;
        private System.Windows.Forms.RichTextBox textBoxData;
        private Button BT_popup;
    }
}
