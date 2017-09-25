using System;
using System.Drawing;
using System.Windows;

namespace Capture.Helpers {
	public static class IconHelper {
		public static Icon LoadIcon(string path) {
			using(var stream = Application.GetResourceStream(new Uri($"pack://application:,,,/{path}")).Stream) {
				return new Icon(stream);
			}
		}
	}
}
