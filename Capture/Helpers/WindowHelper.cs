using System.Linq;
using System.Windows;

namespace Capture.Helpers {
	public static class WindowHelper {
		public static void ShowOrActivate<T>() where T : Window, new() {
			var window = Application.Current.Windows.OfType<T>().FirstOrDefault();
			if(window == null) {
				window = new T();
				window.Show();
			}
			else {
				window.Activate();
			}
		}
	}
}
