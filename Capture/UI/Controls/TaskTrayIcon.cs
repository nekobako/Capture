using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using MetroRadiance.Interop;

namespace Capture.UI.Controls {
	public class TaskTrayIcon : IDisposable {
		private readonly NotifyIcon notifyIcon;

		private readonly Icon defaultIcon;


		public string Text {
			get => this.notifyIcon.Text;
			set => this.notifyIcon.Text = value;
		}

		public Icon Icon {
			get => this.notifyIcon.Icon;
			set {
				var dpi = PerMonitorDpi.GetDpi(IntPtr.Zero);
				this.notifyIcon.Icon = new Icon(value, new Size((int)(16 * dpi.ScaleX), (int)(16 * dpi.ScaleY)));
			}
		}

		public TaskTrayIconItem[] Items {
			get => this.items;
			set {
				this.items = value;

				this.notifyIcon.ContextMenu?.Dispose();
				this.notifyIcon.ContextMenu = new ContextMenu(this.items
					.Select(x => (MenuItem)x)
					.ToArray());
			}
		}
		private TaskTrayIconItem[] items;

		public Action Clicked { get; set; }


		public TaskTrayIcon() {
			var assembly = Assembly.GetEntryAssembly();
			this.notifyIcon = new NotifyIcon() {
				Text = assembly.GetName().Name,
				Icon = Icon.ExtractAssociatedIcon(assembly.Location),
				Visible = true
			};
			this.defaultIcon = this.notifyIcon.Icon;

			this.notifyIcon.Click += (sender, args) => this.Clicked?.Invoke();
		}

		public void Dispose() {
			this.notifyIcon.Dispose();
			this.defaultIcon.Dispose();
		}
	}

	public class TaskTrayIconItem {
		public string Text { get; set; }

		public Action Clicked { get; set; }


		public static explicit operator MenuItem(TaskTrayIconItem item) {
			return new MenuItem(item.Text, (sender, args) => item.Clicked?.Invoke());
		}


		public TaskTrayIconItem(string text, Action clicked) {
			this.Text = text;
			this.Clicked = clicked;
		}
	}
}
