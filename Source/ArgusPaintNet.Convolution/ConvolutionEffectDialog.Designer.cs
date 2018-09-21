namespace ArgusPaintNet.Convolution
{
	partial class ConvolutionEffectDialog
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
			this.lbPresets = new System.Windows.Forms.ListBox();
			this.tbKernel = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.nudFactor = new System.Windows.Forms.NumericUpDown();
			this.bApply = new System.Windows.Forms.Button();
			this.bCancel = new System.Windows.Forms.Button();
			this.cbNormalize = new System.Windows.Forms.CheckBox();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.tsmiPresets = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiAddKernel = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiResetPresets = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiLoadPresets = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiSavePresets = new System.Windows.Forms.ToolStripMenuItem();
			((System.ComponentModel.ISupportInitialize)(this.nudFactor)).BeginInit();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// lbPresets
			// 
			this.lbPresets.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.lbPresets.FormattingEnabled = true;
			this.lbPresets.HorizontalScrollbar = true;
			this.lbPresets.IntegralHeight = false;
			this.lbPresets.Location = new System.Drawing.Point(16, 27);
			this.lbPresets.Name = "lbPresets";
			this.lbPresets.Size = new System.Drawing.Size(145, 211);
			this.lbPresets.TabIndex = 1;
			this.lbPresets.SelectedIndexChanged += new System.EventHandler(this.lbPresets_SelectedIndexChanged);
			this.lbPresets.Format += new System.Windows.Forms.ListControlConvertEventHandler(this.lbPresets_Format);
			this.lbPresets.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lbPresets_KeyDown);
			// 
			// tbKernel
			// 
			this.tbKernel.AcceptsReturn = true;
			this.tbKernel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tbKernel.Font = new System.Drawing.Font("Segoe UI Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tbKernel.Location = new System.Drawing.Point(167, 27);
			this.tbKernel.Multiline = true;
			this.tbKernel.Name = "tbKernel";
			this.tbKernel.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.tbKernel.Size = new System.Drawing.Size(172, 133);
			this.tbKernel.TabIndex = 2;
			this.tbKernel.Text = "0 0 0\r\n0 1 0\r\n0 0 0";
			this.tbKernel.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.tbKernel.WordWrap = false;
			this.tbKernel.TextChanged += new System.EventHandler(this.tbKernel_TextChanged);
			this.tbKernel.Leave += new System.EventHandler(this.tbKernel_Leave);
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(167, 191);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(40, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Factor:";
			// 
			// nudFactor
			// 
			this.nudFactor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.nudFactor.DecimalPlaces = 2;
			this.nudFactor.Location = new System.Drawing.Point(213, 189);
			this.nudFactor.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
			this.nudFactor.Minimum = new decimal(new int[] {
            10000,
            0,
            0,
            -2147483648});
			this.nudFactor.Name = "nudFactor";
			this.nudFactor.Size = new System.Drawing.Size(126, 20);
			this.nudFactor.TabIndex = 4;
			this.nudFactor.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.nudFactor.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.nudFactor.ValueChanged += new System.EventHandler(this.nudFactor_ValueChanged);
			// 
			// bApply
			// 
			this.bApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.bApply.AutoSize = true;
			this.bApply.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.bApply.Location = new System.Drawing.Point(240, 215);
			this.bApply.Name = "bApply";
			this.bApply.Size = new System.Drawing.Size(43, 23);
			this.bApply.TabIndex = 6;
			this.bApply.Text = "Apply";
			this.bApply.UseVisualStyleBackColor = true;
			this.bApply.Click += new System.EventHandler(this.bApply_Click);
			// 
			// bCancel
			// 
			this.bCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.bCancel.AutoSize = true;
			this.bCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.bCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.bCancel.Location = new System.Drawing.Point(289, 215);
			this.bCancel.Name = "bCancel";
			this.bCancel.Size = new System.Drawing.Size(50, 23);
			this.bCancel.TabIndex = 7;
			this.bCancel.Text = "Cancel";
			this.bCancel.UseVisualStyleBackColor = true;
			this.bCancel.Click += new System.EventHandler(this.bCancel_Click);
			// 
			// cbNormalize
			// 
			this.cbNormalize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.cbNormalize.AutoSize = true;
			this.cbNormalize.Checked = true;
			this.cbNormalize.CheckState = System.Windows.Forms.CheckState.Checked;
			this.cbNormalize.Location = new System.Drawing.Point(167, 166);
			this.cbNormalize.Name = "cbNormalize";
			this.cbNormalize.Size = new System.Drawing.Size(105, 17);
			this.cbNormalize.TabIndex = 8;
			this.cbNormalize.Text = "Normalize Kernel";
			this.cbNormalize.UseVisualStyleBackColor = true;
			this.cbNormalize.CheckedChanged += new System.EventHandler(this.cbNormalize_CheckedChanged);
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiPresets});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(351, 24);
			this.menuStrip1.TabIndex = 9;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// tsmiPresets
			// 
			this.tsmiPresets.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiAddKernel,
            this.tsmiResetPresets,
            this.tsmiLoadPresets,
            this.tsmiSavePresets});
			this.tsmiPresets.Name = "tsmiPresets";
			this.tsmiPresets.Size = new System.Drawing.Size(56, 20);
			this.tsmiPresets.Text = "Presets";
			// 
			// tsmiAddKernel
			// 
			this.tsmiAddKernel.Name = "tsmiAddKernel";
			this.tsmiAddKernel.Size = new System.Drawing.Size(186, 22);
			this.tsmiAddKernel.Text = "Add Kernel to Presets";
			this.tsmiAddKernel.Click += new System.EventHandler(this.tsmiAddKernel_Click);
			// 
			// tsmiResetPresets
			// 
			this.tsmiResetPresets.Name = "tsmiResetPresets";
			this.tsmiResetPresets.Size = new System.Drawing.Size(186, 22);
			this.tsmiResetPresets.Text = "Reset Presets";
			this.tsmiResetPresets.Click += new System.EventHandler(this.tsmiResetPresets_Click);
			// 
			// tsmiLoadPresets
			// 
			this.tsmiLoadPresets.Name = "tsmiLoadPresets";
			this.tsmiLoadPresets.Size = new System.Drawing.Size(186, 22);
			this.tsmiLoadPresets.Text = "Load Presets...";
			this.tsmiLoadPresets.Click += new System.EventHandler(this.tsmiLoadPresets_Click);
			// 
			// tsmiSavePresets
			// 
			this.tsmiSavePresets.Name = "tsmiSavePresets";
			this.tsmiSavePresets.Size = new System.Drawing.Size(186, 22);
			this.tsmiSavePresets.Text = "Save Presets...";
			this.tsmiSavePresets.Click += new System.EventHandler(this.tsmiSavePresets_Click);
			// 
			// ConvolutionEffectDialog
			// 
			this.AcceptButton = this.bApply;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.bCancel;
			this.ClientSize = new System.Drawing.Size(351, 250);
			this.Controls.Add(this.cbNormalize);
			this.Controls.Add(this.bCancel);
			this.Controls.Add(this.bApply);
			this.Controls.Add(this.nudFactor);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.tbKernel);
			this.Controls.Add(this.lbPresets);
			this.Controls.Add(this.menuStrip1);
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "ConvolutionEffectDialog";
			this.Text = "Convolution";
			((System.ComponentModel.ISupportInitialize)(this.nudFactor)).EndInit();
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.ListBox lbPresets;
		private System.Windows.Forms.TextBox tbKernel;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.NumericUpDown nudFactor;
		private System.Windows.Forms.Button bApply;
		private System.Windows.Forms.Button bCancel;
		private System.Windows.Forms.CheckBox cbNormalize;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem tsmiPresets;
		private System.Windows.Forms.ToolStripMenuItem tsmiAddKernel;
		private System.Windows.Forms.ToolStripMenuItem tsmiLoadPresets;
		private System.Windows.Forms.ToolStripMenuItem tsmiSavePresets;
		private System.Windows.Forms.ToolStripMenuItem tsmiResetPresets;
	}
}