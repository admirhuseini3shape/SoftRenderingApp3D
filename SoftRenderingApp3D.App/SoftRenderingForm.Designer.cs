using SoftRenderingApp3D.Controls;

namespace SoftRenderingApp3D.App {
    partial class SoftRenderingForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
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
            this.components = new System.ComponentModel.Container();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.chkLinearFiltering = new System.Windows.Forms.CheckBox();
            this.chkShowTexture = new System.Windows.Forms.CheckBox();
            this.lstDemos = new System.Windows.Forms.ListBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.arcBallCamControl1 = new SoftRenderingApp3D.Controls.ArcBallCamControl();
            this.panel3D1 = new SoftRenderingApp3D.App.Panel3D();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btnBench = new System.Windows.Forms.Button();
            this.lblSw = new System.Windows.Forms.Label();
            this.btnChangeTexture = new System.Windows.Forms.Button();
            this.groupBox2.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.chkLinearFiltering);
            this.groupBox2.Controls.Add(this.chkShowTexture);
            this.groupBox2.Location = new System.Drawing.Point(918, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(135, 67);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Display";
            // 
            // chkLinearFiltering
            // 
            this.chkLinearFiltering.AutoSize = true;
            this.chkLinearFiltering.Location = new System.Drawing.Point(11, 42);
            this.chkLinearFiltering.Name = "chkLinearFiltering";
            this.chkLinearFiltering.Size = new System.Drawing.Size(91, 17);
            this.chkLinearFiltering.TabIndex = 10;
            this.chkLinearFiltering.Text = "Linear filtering";
            this.chkLinearFiltering.UseVisualStyleBackColor = true;
            // 
            // chkShowTexture
            // 
            this.chkShowTexture.AutoSize = true;
            this.chkShowTexture.Location = new System.Drawing.Point(11, 20);
            this.chkShowTexture.Name = "chkShowTexture";
            this.chkShowTexture.Size = new System.Drawing.Size(62, 17);
            this.chkShowTexture.TabIndex = 9;
            this.chkShowTexture.Text = "Texture";
            this.chkShowTexture.UseVisualStyleBackColor = true;
            // 
            // lstDemos
            // 
            this.lstDemos.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstDemos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstDemos.IntegralHeight = false;
            this.lstDemos.Location = new System.Drawing.Point(8, 21);
            this.lstDemos.Name = "lstDemos";
            this.lstDemos.Size = new System.Drawing.Size(144, 565);
            this.lstDemos.TabIndex = 8;
            this.toolTip1.SetToolTip(this.lstDemos, "Use double click to select");
            // 
            // groupBox5
            // 
            this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox5.Controls.Add(this.lstDemos);
            this.groupBox5.Location = new System.Drawing.Point(12, 12);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Padding = new System.Windows.Forms.Padding(8, 8, 8, 8);
            this.groupBox5.Size = new System.Drawing.Size(160, 594);
            this.groupBox5.TabIndex = 9;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Worlds (select=2x click)";
            // 
            // toolTip1
            // 
            this.toolTip1.IsBalloon = true;
            // 
            // groupBox6
            // 
            this.groupBox6.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox6.Controls.Add(this.arcBallCamControl1);
            this.groupBox6.Controls.Add(this.panel3D1);
            this.groupBox6.Location = new System.Drawing.Point(0, 0);
            this.groupBox6.Margin = new System.Windows.Forms.Padding(0);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Padding = new System.Windows.Forms.Padding(8, 8, 8, 8);
            this.groupBox6.Size = new System.Drawing.Size(734, 594);
            this.groupBox6.TabIndex = 10;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Camera view";
            // 
            // arcBallCamControl1
            // 
            this.arcBallCamControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.arcBallCamControl1.BackColor = System.Drawing.Color.Gainsboro;
            this.arcBallCamControl1.Camera = null;
            this.arcBallCamControl1.Location = new System.Drawing.Point(588, 16);
            this.arcBallCamControl1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.arcBallCamControl1.Name = "arcBallCamControl1";
            this.arcBallCamControl1.Size = new System.Drawing.Size(139, 117);
            this.arcBallCamControl1.TabIndex = 1;
            // 
            // panel3D1
            // 
            this.panel3D1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.panel3D1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.panel3D1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.panel3D1.Location = new System.Drawing.Point(11, 16);
            this.panel3D1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panel3D1.Name = "panel3D1";
            this.panel3D1.Size = new System.Drawing.Size(716, 566);
            this.panel3D1.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox6, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(178, 12);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(734, 594);
            this.tableLayoutPanel1.TabIndex = 12;
            // 
            // btnBench
            // 
            this.btnBench.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBench.Location = new System.Drawing.Point(918, 510);
            this.btnBench.Name = "btnBench";
            this.btnBench.Size = new System.Drawing.Size(135, 24);
            this.btnBench.TabIndex = 14;
            this.btnBench.Text = "Bench";
            this.btnBench.UseVisualStyleBackColor = true;
            // 
            // lblSw
            // 
            this.lblSw.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSw.AutoSize = true;
            this.lblSw.Location = new System.Drawing.Point(1014, 537);
            this.lblSw.Name = "lblSw";
            this.lblSw.Size = new System.Drawing.Size(0, 13);
            this.lblSw.TabIndex = 15;
            this.lblSw.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnChangeTexture
            // 
            this.btnChangeTexture.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnChangeTexture.Location = new System.Drawing.Point(918, 540);
            this.btnChangeTexture.Name = "btnChangeTexture";
            this.btnChangeTexture.Size = new System.Drawing.Size(135, 25);
            this.btnChangeTexture.TabIndex = 16;
            this.btnChangeTexture.Text = "Change Texture";
            this.btnChangeTexture.UseVisualStyleBackColor = true;
            this.btnChangeTexture.Click += new System.EventHandler(this.btnChangeTexture_Click);
            // 
            // SoftRenderingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1065, 618);
            this.Controls.Add(this.btnChangeTexture);
            this.Controls.Add(this.lblSw);
            this.Controls.Add(this.btnBench);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox2);
            this.MinimumSize = new System.Drawing.Size(433, 328);
            this.Name = "SoftRenderingForm";
            this.Text = "Form2";
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private Panel3D panel3D1;
        private ArcBallCamControl arcBallCamControl1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ListBox lstDemos;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.GroupBox groupBox6;
        private Panel3D panel3D2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button btnBench;
        private System.Windows.Forms.Label lblSw;
        private System.Windows.Forms.CheckBox chkShowTexture;
        private System.Windows.Forms.CheckBox chkLinearFiltering;
        private System.Windows.Forms.Button btnChangeTexture;
    }
}