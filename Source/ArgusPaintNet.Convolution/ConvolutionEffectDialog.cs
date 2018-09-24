using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PaintDotNet;
using PaintDotNet.Effects;
using System.IO;
using ArgusPaintNet.Shared;

namespace ArgusPaintNet.Convolution
{
#if DESIGNING
	public partial class ConvolutionEffectDialog : Form
	{
		public ConvolutionEffectDialog()
		{
			InitializeComponent();
		}

		private void tbKernel_TextChanged(object sender, EventArgs e)
		{

		}

		private void nudFactor_ValueChanged(object sender, EventArgs e)
		{

		}

		private void bNormalize_Click(object sender, EventArgs e)
		{

		}

		private void cbNormalize_CheckedChanged(object sender, EventArgs e)
		{

		}

		private void bApply_Click(object sender, EventArgs e)
		{

		}

		private void bCancel_Click(object sender, EventArgs e)
		{

		}

		private void lbPresets_SelectedIndexChanged(object sender, EventArgs e)
		{

		}

		private void lbPresets_Format(object sender, ListControlConvertEventArgs e)
		{

		}

		private void tbKernel_Leave(object sender, EventArgs e)
		{

		}

		private void tsmiAddKernel_Click(object sender, EventArgs e)
		{

		}

		private void tsmiResetPresets_Click(object sender, EventArgs e)
		{

		}

		private void tsmiLoadPresets_Click(object sender, EventArgs e)
		{

		}

		private void tsmiSavePresets_Click(object sender, EventArgs e)
		{

		}

		private void lbPresets_KeyDown(object sender, KeyEventArgs e)
		{

		}
	}
#else
    internal partial class ConvolutionEffectDialog : EffectConfigDialog<ConvolutionEffect,ConvolutionConfigEffectToken>
	{
        private bool _isValidKernel = true;
        private Dictionary<string, ConvolutionConfigEffectToken> _dictPresets;

		public ConvolutionEffectDialog()
		{
            this.InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			this.LoadPresets(Properties.Settings.Default.PresetFile);

			base.OnLoad(e);
		}

        private void LoadPresets(string filename)
		{
			Preset[] presets = Preset.LoadFromFile(filename);
			if (presets.Length < 1)
				presets = Preset.DefaultPresets;
			this._dictPresets = presets.ToDictionary();
			this.lbPresets.Items.Clear();
			foreach (string name in this._dictPresets.Keys)
				this.lbPresets.Items.Add(name);
		}

		protected override ConvolutionConfigEffectToken CreateInitialToken()
		{
			return new ConvolutionConfigEffectToken();
		}

		protected override void InitDialogFromToken(ConvolutionConfigEffectToken effectTokenCopy)
		{
			this.tbKernel.Text = effectTokenCopy.Kernel.ToString();
			this.cbNormalize.Checked = effectTokenCopy.Normalize;
            this.nudFactor.Value = (decimal)effectTokenCopy.Factor;
		}

		protected override void LoadIntoTokenFromDialog(ConvolutionConfigEffectToken writeValuesHere)
		{
			Matrix kernel;
			this._isValidKernel = Matrix.TryParse(this.tbKernel.Text, out kernel);
			if (this._isValidKernel)
			{
				writeValuesHere.Kernel = kernel;
			}

			writeValuesHere.Normalize = this.cbNormalize.Checked;
			writeValuesHere.Factor = (float)this.nudFactor.Value;
		}

		private void lbPresets_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.lbPresets.SelectedIndex < 0)
				return;

			string name = this.lbPresets.SelectedItem.ToString();
			ConvolutionConfigEffectToken token;
			if (this._dictPresets.TryGetValue(name, out token))
				this.InitDialogFromToken(token);
		}

		private void lbPresets_Format(object sender, ListControlConvertEventArgs e)
		{
			string str = Properties.Resources.ResourceManager.GetString("ConvPresetName" + e.ListItem.ToString());
			if (string.IsNullOrEmpty(str))
				str = e.ListItem.ToString();
			e.Value = str;
		}

		private void tbKernel_TextChanged(object sender, EventArgs e)
		{
			this.FinishTokenUpdate();
		}

		private void nudFactor_ValueChanged(object sender, EventArgs e)
		{
			this.FinishTokenUpdate();
		}

		private void cbNormalize_CheckedChanged(object sender, EventArgs e)
		{
			this.FinishTokenUpdate();
		}

		private void bApply_Click(object sender, EventArgs e)
		{
			if (!this._isValidKernel)
			{
				MessageBox.Show("Invalid Expression for Kernel", "Error");
				return;
			}

			this.DialogResult = DialogResult.OK;
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			Properties.Settings settings = Properties.Settings.Default;
			string filename = settings.PresetFile;
			if (string.IsNullOrEmpty(filename))
			{
				filename = Path.GetTempFileName();
				settings.PresetFile = filename;
				settings.Save();
			}
			Preset.SaveToFile(filename, this._dictPresets.ToPresets());
			base.OnClosing(e);
		}

		private void bCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}

		private void tbKernel_Leave(object sender, EventArgs e)
		{
			this.tbKernel.Text = this.EffectToken.Kernel.ToString();
		}

		private void tsmiAddKernel_Click(object sender, EventArgs e)
		{
			if (!this._isValidKernel)
			{
				MessageBox.Show("Invalid Kernel.", "Error");
				return;
			}
			string name = InputBox.Show("Preset Name:", "Prest Name");
			if (string.IsNullOrEmpty(name))
				return;
			if (this._dictPresets.ContainsKey(name))
			{
				MessageBox.Show(string.Format("Name \"{0}\" is already in use.", name), "Error");
				return;
			}

			this._dictPresets.Add(name, new ConvolutionConfigEffectToken(this.EffectToken));
			this.lbPresets.Items.Add(name);
		}

		private void tsmiResetPresets_Click(object sender, EventArgs e)
		{
			this.LoadPresets(null);
		}

		private void tsmiLoadPresets_Click(object sender, EventArgs e)
		{
            var ofd = new OpenFileDialog
            {
                Filter = "Convolution Presets (*.xml)|*.xml"
            };
            if (ofd.ShowDialog() != DialogResult.OK)
				return;
			this.LoadPresets(ofd.FileName);
		}

		private void tsmiSavePresets_Click(object sender, EventArgs e)
		{
            var sfd = new SaveFileDialog
            {
                Filter = "Convolution Presets (*.xml)|*.xml"
            };
            if (sfd.ShowDialog() != DialogResult.OK)
				return;
			Preset.SaveToFile(sfd.FileName, this._dictPresets.ToPresets());
		}

		private void lbPresets_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode != Keys.Delete || this.lbPresets.SelectedIndex < 0)
				return;

			string name = this.lbPresets.SelectedItem.ToString();
			this.lbPresets.Items.RemoveAt(this.lbPresets.SelectedIndex);
			this._dictPresets.Remove(name);
		}
	}
#endif
}
