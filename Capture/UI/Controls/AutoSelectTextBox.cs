using System.Windows.Input;
using MetroRadiance.UI.Controls;

namespace Capture.UI.Controls {
	public class AutoSelectTextBox : PromptTextBox {
		public AutoSelectTextBox() : base() {
			this.PreviewMouseDown += this.previewMouseDown;
		}

		private void previewMouseDown(object sender, MouseButtonEventArgs args) {
			if(this.IsFocused) {
				return;
			}

			this.SelectAll();
			this.Focus();
			args.Handled = true;
		}
	}
}
