using System.Windows.Forms;

namespace BMM_Battery_Mobus_Moniter_ver0._2_.Forms
{
    partial class FormMonitor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel tableLayoutPanelPanels;
        private System.Windows.Forms.Panel panelLogSettings;
        private System.Windows.Forms.Label labelLogPath;
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMonitor));
            this.tableLayoutPanelPanels = new System.Windows.Forms.TableLayoutPanel();
            this.panelLogSettings = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.labelLogPath = new System.Windows.Forms.Label();
            this.BT_Browse = new System.Windows.Forms.Button();
            this.textBoxLogPath = new System.Windows.Forms.TextBox();
            this.LB_Time = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.TB_Cellmax = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.TB_DeltaCell = new System.Windows.Forms.TextBox();
            this.panel5 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.TB_TempMax = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.TB_CellMin = new System.Windows.Forms.TextBox();
            this.BT_apply = new System.Windows.Forms.Button();
            this.panelCell = new System.Windows.Forms.Panel();
            this.panelLogSettings.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panelCell.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanelPanels
            // 
            this.tableLayoutPanelPanels.AutoScroll = true;
            this.tableLayoutPanelPanels.ColumnCount = 5;
            this.tableLayoutPanelPanels.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelPanels.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelPanels.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelPanels.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelPanels.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelPanels.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanelPanels.Location = new System.Drawing.Point(0, 91);
            this.tableLayoutPanelPanels.Name = "tableLayoutPanelPanels";
            this.tableLayoutPanelPanels.RowCount = 2;
            this.tableLayoutPanelPanels.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelPanels.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelPanels.Size = new System.Drawing.Size(1384, 687);
            this.tableLayoutPanelPanels.TabIndex = 0;
            this.tableLayoutPanelPanels.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanelPanels_Paint);
            // 
            // panelLogSettings
            // 
            this.panelLogSettings.Controls.Add(this.panel3);
            this.panelLogSettings.Controls.Add(this.panel1);
            this.panelLogSettings.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelLogSettings.Location = new System.Drawing.Point(0, 0);
            this.panelLogSettings.Name = "panelLogSettings";
            this.panelLogSettings.Size = new System.Drawing.Size(477, 91);
            this.panelLogSettings.TabIndex = 1;
            // 
            // panel3
            // 
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(0, 45);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(477, 47);
            this.panel3.TabIndex = 7;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.labelLogPath);
            this.panel1.Controls.Add(this.BT_Browse);
            this.panel1.Controls.Add(this.textBoxLogPath);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(477, 45);
            this.panel1.TabIndex = 6;
            // 
            // labelLogPath
            // 
            this.labelLogPath.AutoSize = true;
            this.labelLogPath.Font = new System.Drawing.Font("文泉驛微米黑", 12F);
            this.labelLogPath.Location = new System.Drawing.Point(0, 11);
            this.labelLogPath.Name = "labelLogPath";
            this.labelLogPath.Size = new System.Drawing.Size(107, 24);
            this.labelLogPath.TabIndex = 0;
            this.labelLogPath.Text = "Log 路徑：";
            // 
            // BT_Browse
            // 
            this.BT_Browse.Font = new System.Drawing.Font("文泉驛微米黑", 12F);
            this.BT_Browse.Location = new System.Drawing.Point(358, 4);
            this.BT_Browse.Name = "BT_Browse";
            this.BT_Browse.Size = new System.Drawing.Size(104, 35);
            this.BT_Browse.TabIndex = 5;
            this.BT_Browse.Text = "Browse";
            this.BT_Browse.UseVisualStyleBackColor = true;
            this.BT_Browse.Click += new System.EventHandler(this.BT_Browse_Click);
            // 
            // textBoxLogPath
            // 
            this.textBoxLogPath.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.textBoxLogPath.Location = new System.Drawing.Point(109, 6);
            this.textBoxLogPath.Name = "textBoxLogPath";
            this.textBoxLogPath.Size = new System.Drawing.Size(245, 31);
            this.textBoxLogPath.TabIndex = 2;
            this.textBoxLogPath.TextChanged += new System.EventHandler(this.textBoxLogPath_TextChanged);
            // 
            // LB_Time
            // 
            this.LB_Time.AutoSize = true;
            this.LB_Time.Dock = System.Windows.Forms.DockStyle.Right;
            this.LB_Time.Font = new System.Drawing.Font("文泉驛微米黑", 18F);
            this.LB_Time.Location = new System.Drawing.Point(71, 0);
            this.LB_Time.Name = "LB_Time";
            this.LB_Time.Size = new System.Drawing.Size(165, 70);
            this.LB_Time.TabIndex = 7;
            this.LB_Time.Text = "XXXX/XX/XX\r\nXX:xx:xx";
            this.LB_Time.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel2
            // 
            this.panel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel2.Controls.Add(this.LB_Time);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(1148, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(236, 91);
            this.panel2.TabIndex = 9;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.label2);
            this.panel4.Controls.Add(this.TB_Cellmax);
            this.panel4.Controls.Add(this.label1);
            this.panel4.Controls.Add(this.TB_DeltaCell);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(0, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(560, 45);
            this.panel4.TabIndex = 9;
            this.panel4.Paint += new System.Windows.Forms.PaintEventHandler(this.panel4_Paint);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("文泉驛微米黑", 12F);
            this.label2.Location = new System.Drawing.Point(6, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(123, 24);
            this.label2.TabIndex = 0;
            this.label2.Text = "Cell最大值：";
            // 
            // TB_Cellmax
            // 
            this.TB_Cellmax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.TB_Cellmax.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.TB_Cellmax.Location = new System.Drawing.Point(132, 7);
            this.TB_Cellmax.Name = "TB_Cellmax";
            this.TB_Cellmax.Size = new System.Drawing.Size(87, 31);
            this.TB_Cellmax.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("文泉驛微米黑", 12F);
            this.label1.Location = new System.Drawing.Point(222, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(131, 24);
            this.label1.TabIndex = 4;
            this.label1.Text = "DeltaCell值：";
            // 
            // TB_DeltaCell
            // 
            this.TB_DeltaCell.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.TB_DeltaCell.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.TB_DeltaCell.Location = new System.Drawing.Point(351, 7);
            this.TB_DeltaCell.Name = "TB_DeltaCell";
            this.TB_DeltaCell.Size = new System.Drawing.Size(87, 31);
            this.TB_DeltaCell.TabIndex = 3;
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.label4);
            this.panel5.Controls.Add(this.TB_TempMax);
            this.panel5.Controls.Add(this.label3);
            this.panel5.Controls.Add(this.TB_CellMin);
            this.panel5.Controls.Add(this.BT_apply);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel5.Location = new System.Drawing.Point(0, 45);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(560, 45);
            this.panel5.TabIndex = 10;
            this.panel5.Paint += new System.Windows.Forms.PaintEventHandler(this.panel5_Paint);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("文泉驛微米黑", 12F);
            this.label4.Location = new System.Drawing.Point(222, 10);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(126, 24);
            this.label4.TabIndex = 6;
            this.label4.Text = "Temp最大值:";
            // 
            // TB_TempMax
            // 
            this.TB_TempMax.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.TB_TempMax.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.TB_TempMax.Location = new System.Drawing.Point(351, 7);
            this.TB_TempMax.Name = "TB_TempMax";
            this.TB_TempMax.Size = new System.Drawing.Size(87, 31);
            this.TB_TempMax.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("文泉驛微米黑", 12F);
            this.label3.Location = new System.Drawing.Point(6, 10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(123, 24);
            this.label3.TabIndex = 7;
            this.label3.Text = "Cell最小值：";
            // 
            // TB_CellMin
            // 
            this.TB_CellMin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.TB_CellMin.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.TB_CellMin.Location = new System.Drawing.Point(132, 7);
            this.TB_CellMin.Name = "TB_CellMin";
            this.TB_CellMin.Size = new System.Drawing.Size(87, 31);
            this.TB_CellMin.TabIndex = 8;
            // 
            // BT_apply
            // 
            this.BT_apply.Font = new System.Drawing.Font("新細明體", 12F);
            this.BT_apply.Location = new System.Drawing.Point(441, 5);
            this.BT_apply.Name = "BT_apply";
            this.BT_apply.Size = new System.Drawing.Size(104, 35);
            this.BT_apply.TabIndex = 6;
            this.BT_apply.Text = "Apply";
            this.BT_apply.UseVisualStyleBackColor = true;
            this.BT_apply.Click += new System.EventHandler(this.BT_apply_Click);
            // 
            // panelCell
            // 
            this.panelCell.Controls.Add(this.panel5);
            this.panelCell.Controls.Add(this.panel4);
            this.panelCell.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelCell.Location = new System.Drawing.Point(477, 0);
            this.panelCell.Name = "panelCell";
            this.panelCell.Size = new System.Drawing.Size(560, 91);
            this.panelCell.TabIndex = 6;
            // 
            // FormMonitor
            // 
            this.ClientSize = new System.Drawing.Size(1384, 778);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panelCell);
            this.Controls.Add(this.panelLogSettings);
            this.Controls.Add(this.tableLayoutPanelPanels);
            this.Font = new System.Drawing.Font("文泉驛微米黑", 18F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormMonitor";
            this.Text = "FormMonitor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMonitor_FormClosing);
            this.Load += new System.EventHandler(this.FormMonitor_Load);
            this.panelLogSettings.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.panelCell.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private Button BT_Browse;
        private TextBox textBoxLogPath;
        private Label LB_Time;
        private Panel panel2;
        private Panel panel3;
        private Panel panel1;
        private Panel panel4;
        private Label label2;
        private TextBox TB_Cellmax;
        private Label label1;
        private TextBox TB_DeltaCell;
        private Panel panel5;
        private Label label3;
        private TextBox TB_CellMin;
        private Button BT_apply;
        private Panel panelCell;
        private Label label4;
        private TextBox TB_TempMax;
    }
}