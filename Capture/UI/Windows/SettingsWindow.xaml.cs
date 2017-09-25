using System;
using System.Diagnostics;
using System.Windows.Navigation;
using MetroRadiance.UI.Controls;
using Capture.UI.Bindings;

namespace Capture.UI.Windows {
	public partial class SettingsWindow : MetroWindow {
		public SettingsWindow() : base() {
			this.InitializeComponent();
			this.DataContext = new SettingsWindowViewModel();
		}

		private void closed(object sender, EventArgs args) {
			Properties.Settings.Default.Save();
		}

		private void hyperlinkRequestNavigate(object sender, RequestNavigateEventArgs args) {
			Process.Start(args.Uri.AbsoluteUri);
			args.Handled = true;
		}
	}
}
