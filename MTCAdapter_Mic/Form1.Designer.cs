namespace MTCAdapter_Mic
{
    partial class MicAdapter
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            this.components = new System.ComponentModel.Container();
            this.comboWasapiDevices = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnStartRecording = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.buttonStopRecording = new System.Windows.Forms.Button();
            this.comboBoxSampleRate = new System.Windows.Forms.ComboBox();
            this.comboBoxChannels = new System.Windows.Forms.ComboBox();
            this.btnMTConnect = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.chk2ndAvail = new System.Windows.Forms.CheckBox();
            this.comboWasapiDevices2 = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.listBoxRecordings = new System.Windows.Forms.ListBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtStatus = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxMaxDivisor = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboWasapiDevices
            // 
            this.comboWasapiDevices.FormattingEnabled = true;
            this.comboWasapiDevices.Location = new System.Drawing.Point(19, 38);
            this.comboWasapiDevices.Name = "comboWasapiDevices";
            this.comboWasapiDevices.Size = new System.Drawing.Size(189, 21);
            this.comboWasapiDevices.TabIndex = 0;
            this.comboWasapiDevices.SelectedIndexChanged += new System.EventHandler(this.comboWasapiDevices_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Input source";
            // 
            // btnStartRecording
            // 
            this.btnStartRecording.Location = new System.Drawing.Point(32, 264);
            this.btnStartRecording.Name = "btnStartRecording";
            this.btnStartRecording.Size = new System.Drawing.Size(105, 29);
            this.btnStartRecording.TabIndex = 4;
            this.btnStartRecording.Text = "Start Recording";
            this.btnStartRecording.UseVisualStyleBackColor = true;
            this.btnStartRecording.Click += new System.EventHandler(this.button1_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(19, 299);
            this.progressBar1.Maximum = 60;
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(266, 13);
            this.progressBar1.Step = 1;
            this.progressBar1.TabIndex = 5;
            // 
            // buttonStopRecording
            // 
            this.buttonStopRecording.Location = new System.Drawing.Point(160, 264);
            this.buttonStopRecording.Name = "buttonStopRecording";
            this.buttonStopRecording.Size = new System.Drawing.Size(94, 29);
            this.buttonStopRecording.TabIndex = 7;
            this.buttonStopRecording.Text = "Stop";
            this.buttonStopRecording.UseVisualStyleBackColor = true;
            this.buttonStopRecording.Click += new System.EventHandler(this.buttonStopRecording_Click);
            // 
            // comboBoxSampleRate
            // 
            this.comboBoxSampleRate.Enabled = false;
            this.comboBoxSampleRate.FormattingEnabled = true;
            this.comboBoxSampleRate.Location = new System.Drawing.Point(16, 150);
            this.comboBoxSampleRate.Name = "comboBoxSampleRate";
            this.comboBoxSampleRate.Size = new System.Drawing.Size(121, 21);
            this.comboBoxSampleRate.TabIndex = 8;
            // 
            // comboBoxChannels
            // 
            this.comboBoxChannels.Enabled = false;
            this.comboBoxChannels.FormattingEnabled = true;
            this.comboBoxChannels.Location = new System.Drawing.Point(143, 150);
            this.comboBoxChannels.Name = "comboBoxChannels";
            this.comboBoxChannels.Size = new System.Drawing.Size(121, 21);
            this.comboBoxChannels.TabIndex = 9;
            // 
            // btnMTConnect
            // 
            this.btnMTConnect.Location = new System.Drawing.Point(16, 23);
            this.btnMTConnect.Name = "btnMTConnect";
            this.btnMTConnect.Size = new System.Drawing.Size(102, 29);
            this.btnMTConnect.TabIndex = 11;
            this.btnMTConnect.Text = "Connect MTC";
            this.btnMTConnect.UseVisualStyleBackColor = true;
            this.btnMTConnect.Click += new System.EventHandler(this.btnMTConnect_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textBoxMaxDivisor);
            this.groupBox1.Controls.Add(this.buttonStopRecording);
            this.groupBox1.Controls.Add(this.numericUpDown1);
            this.groupBox1.Controls.Add(this.chk2ndAvail);
            this.groupBox1.Controls.Add(this.comboWasapiDevices2);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.comboBoxSampleRate);
            this.groupBox1.Controls.Add(this.comboWasapiDevices);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.comboBoxChannels);
            this.groupBox1.Controls.Add(this.btnStartRecording);
            this.groupBox1.Controls.Add(this.listBoxRecordings);
            this.groupBox1.Controls.Add(this.progressBar1);
            this.groupBox1.Location = new System.Drawing.Point(28, 25);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(373, 393);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Manual recording";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(54, 204);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            120,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(59, 20);
            this.numericUpDown1.TabIndex = 15;
            this.numericUpDown1.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // chk2ndAvail
            // 
            this.chk2ndAvail.AutoSize = true;
            this.chk2ndAvail.Checked = true;
            this.chk2ndAvail.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk2ndAvail.Location = new System.Drawing.Point(213, 92);
            this.chk2ndAvail.Name = "chk2ndAvail";
            this.chk2ndAvail.Size = new System.Drawing.Size(69, 17);
            this.chk2ndAvail.TabIndex = 14;
            this.chk2ndAvail.Text = "Available";
            this.chk2ndAvail.UseVisualStyleBackColor = true;
            // 
            // comboWasapiDevices2
            // 
            this.comboWasapiDevices2.FormattingEnabled = true;
            this.comboWasapiDevices2.Location = new System.Drawing.Point(19, 90);
            this.comboWasapiDevices2.Name = "comboWasapiDevices2";
            this.comboWasapiDevices2.Size = new System.Drawing.Size(189, 21);
            this.comboWasapiDevices2.TabIndex = 13;
            this.comboWasapiDevices2.SelectedIndexChanged += new System.EventHandler(this.comboWasapiDevices2_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 74);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(82, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Input source #2";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(29, 188);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(100, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Recode length(sec)";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(157, 134);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(76, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Stereo / Mono";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(40, 134);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Wave Hz";
            // 
            // listBoxRecordings
            // 
            this.listBoxRecordings.FormattingEnabled = true;
            this.listBoxRecordings.Location = new System.Drawing.Point(19, 318);
            this.listBoxRecordings.Name = "listBoxRecordings";
            this.listBoxRecordings.Size = new System.Drawing.Size(266, 69);
            this.listBoxRecordings.TabIndex = 6;
            this.listBoxRecordings.SelectedIndexChanged += new System.EventHandler(this.listBoxRecordings_SelectedIndexChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnMTConnect);
            this.groupBox2.Location = new System.Drawing.Point(407, 32);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(210, 386);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "MTConnect adapter";
            // 
            // txtStatus
            // 
            this.txtStatus.AcceptsReturn = true;
            this.txtStatus.Location = new System.Drawing.Point(28, 424);
            this.txtStatus.Multiline = true;
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.ReadOnly = true;
            this.txtStatus.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtStatus.Size = new System.Drawing.Size(589, 97);
            this.txtStatus.TabIndex = 14;
            // 
            // timer1
            // 
            this.timer1.Interval = 30000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // timer2
            // 
            this.timer2.Interval = 30000;
            this.timer2.Tick += new System.EventHandler(this.timer2_Tick);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(166, 188);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 13);
            this.label6.TabIndex = 11;
            this.label6.Text = "Max. Divisor";
            this.label6.Click += new System.EventHandler(this.label5_Click);
            // 
            // textBoxMaxDivisor
            // 
            this.textBoxMaxDivisor.Location = new System.Drawing.Point(169, 203);
            this.textBoxMaxDivisor.Name = "textBoxMaxDivisor";
            this.textBoxMaxDivisor.Size = new System.Drawing.Size(71, 20);
            this.textBoxMaxDivisor.TabIndex = 16;
            this.textBoxMaxDivisor.Text = "5.0";
            this.textBoxMaxDivisor.TextChanged += new System.EventHandler(this.textBoxMaxDivisor_TextChanged);
            // 
            // MicAdapter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(634, 533);
            this.Controls.Add(this.txtStatus);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "MicAdapter";
            this.Text = "MTConnect adapter - microphone";
            this.Load += new System.EventHandler(this.MicAdapter_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboWasapiDevices;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnStartRecording;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button buttonStopRecording;
        private System.Windows.Forms.ComboBox comboBoxSampleRate;
        private System.Windows.Forms.ComboBox comboBoxChannels;
        private System.Windows.Forms.Button btnMTConnect;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListBox listBoxRecordings;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox txtStatus;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.CheckBox chk2ndAvail;
        private System.Windows.Forms.ComboBox comboWasapiDevices2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBoxMaxDivisor;
    }
}

