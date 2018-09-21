namespace ArgusPaintNet.Shared
{
	partial class InputBox
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
			this.pMain = new System.Windows.Forms.FlowLayoutPanel();
			this.lText = new System.Windows.Forms.Label();
			this.tbInput = new System.Windows.Forms.TextBox();
			this.pButttons = new System.Windows.Forms.FlowLayoutPanel();
			this.bCancel = new System.Windows.Forms.Button();
			this.bOK = new System.Windows.Forms.Button();
			this.pMain.SuspendLayout();
			this.pButttons.SuspendLayout();
			this.SuspendLayout();
			// 
			// pMain
			// 
			this.pMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pMain.AutoSize = true;
			this.pMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.pMain.Controls.Add(this.lText);
			this.pMain.Controls.Add(this.tbInput);
			this.pMain.Controls.Add(this.pButttons);
			this.pMain.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.pMain.Location = new System.Drawing.Point(13, 13);
			this.pMain.Name = "pMain";
			this.pMain.Size = new System.Drawing.Size(168, 74);
			this.pMain.TabIndex = 0;
			this.pMain.WrapContents = false;
			// 
			// lText
			// 
			this.lText.AutoSize = true;
			this.lText.Location = new System.Drawing.Point(3, 0);
			this.lText.Name = "lText";
			this.lText.Size = new System.Drawing.Size(35, 13);
			this.lText.TabIndex = 0;
			this.lText.Text = "label1";
			// 
			// tbInput
			// 
			this.tbInput.Dock = System.Windows.Forms.DockStyle.Top;
			this.tbInput.Location = new System.Drawing.Point(3, 16);
			this.tbInput.Name = "tbInput";
			this.tbInput.Size = new System.Drawing.Size(162, 20);
			this.tbInput.TabIndex = 1;
			// 
			// pButttons
			// 
			this.pButttons.AutoSize = true;
			this.pButttons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.pButttons.Controls.Add(this.bCancel);
			this.pButttons.Controls.Add(this.bOK);
			this.pButttons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
			this.pButttons.Location = new System.Drawing.Point(3, 42);
			this.pButttons.Name = "pButttons";
			this.pButttons.Size = new System.Drawing.Size(162, 29);
			this.pButttons.TabIndex = 2;
			// 
			// bCancel
			// 
			this.bCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.bCancel.Location = new System.Drawing.Point(84, 3);
			this.bCancel.Name = "bCancel";
			this.bCancel.Size = new System.Drawing.Size(75, 23);
			this.bCancel.TabIndex = 0;
			this.bCancel.Text = "Cancel";
			this.bCancel.UseVisualStyleBackColor = true;
			this.bCancel.Click += new System.EventHandler(this.bCancel_Click);
			// 
			// bOK
			// 
			this.bOK.Location = new System.Drawing.Point(3, 3);
			this.bOK.Name = "bOK";
			this.bOK.Size = new System.Drawing.Size(75, 23);
			this.bOK.TabIndex = 1;
			this.bOK.Text = "OK";
			this.bOK.UseVisualStyleBackColor = true;
			this.bOK.Click += new System.EventHandler(this.bOK_Click);
			// 
			// InputBox
			// 
			this.AcceptButton = this.bOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CancelButton = this.bCancel;
			this.ClientSize = new System.Drawing.Size(193, 101);
			this.Controls.Add(this.pMain);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "InputBox";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "InputBox";
			this.pMain.ResumeLayout(false);
			this.pMain.PerformLayout();
			this.pButttons.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.FlowLayoutPanel pMain;
		private System.Windows.Forms.Label lText;
		private System.Windows.Forms.TextBox tbInput;
		private System.Windows.Forms.FlowLayoutPanel pButttons;
		private System.Windows.Forms.Button bCancel;
		private System.Windows.Forms.Button bOK;
	}
}