using System;
using System.Windows.Forms;
using System.Xml;
namespace LiveSplit.Splasher {
	public partial class SplasherSettings : UserControl {
		public bool AutoReset { get; set; }
		public bool Checkpoints { get; set; }
		private bool isLoading;

		public SplasherSettings() {
			isLoading = true;
			InitializeComponent();

			//Defaults
			AutoReset = true;
			Checkpoints = true;

			isLoading = false;
		}

		private void Settings_Load(object sender, EventArgs e) {
			isLoading = true;
			LoadSettings();
			isLoading = false;
		}
		public void LoadSettings() {
			chkAutoReset.Checked = AutoReset;
			chkCheckpoints.Checked = Checkpoints;
		}
		private void chkBox_CheckedChanged(object sender, EventArgs e) {
			UpdateSplits();
		}
		public void UpdateSplits() {
			if (isLoading) return;

			AutoReset = chkAutoReset.Checked;
			Checkpoints = chkCheckpoints.Checked;
		}
		public XmlNode UpdateSettings(XmlDocument document) {
			XmlElement xmlSettings = document.CreateElement("Settings");

			SetSetting(document, xmlSettings, chkAutoReset, "AutoReset");
			SetSetting(document, xmlSettings, chkCheckpoints, "Checkpoints");

			return xmlSettings;
		}
		private void SetSetting(XmlDocument document, XmlElement settings, CheckBox chk, string name) {
			XmlElement xmlOption = document.CreateElement(name);
			xmlOption.InnerText = chk.Checked.ToString();
			settings.AppendChild(xmlOption);
		}
		public void SetSettings(XmlNode settings) {
			AutoReset = GetSetting(settings, "//AutoReset", true);
			Checkpoints = GetSetting(settings, "//Checkpoints", true);
		}
		private bool GetSetting(XmlNode settings, string name, bool defaultVal = false) {
			XmlNode option = settings.SelectSingleNode(name);
			if (option != null && option.InnerText != "") {
				return bool.Parse(option.InnerText);
			}
			return defaultVal;
		}
	}
}