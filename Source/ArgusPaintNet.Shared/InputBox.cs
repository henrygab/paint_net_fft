using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArgusPaintNet.Shared
{
	public partial class InputBox : Form
	{
		private InputBox()
		{
            this.InitializeComponent();
		}

		private void bOK_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
		}

		private void bCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}

		public static string Show(string text, string caption)
		{
            var inpBox = new InputBox
            {
                Text = caption
            };
            inpBox.lText.Text = text;
			if (inpBox.ShowDialog() == DialogResult.OK)
				return inpBox.tbInput.Text;
			return null;
		}
	}
}
