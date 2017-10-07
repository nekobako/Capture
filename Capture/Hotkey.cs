using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace Capture {
	public class Hotkey : IDisposable {
		private static class Native {
			public enum ModifierKeys {
				NoRepeat = 0x4000
			}

			public enum WindowMessage {
				HotKey = 0x0312
			}


			[DllImport("user32.dll")]
			public static extern bool RegisterHotKey(IntPtr hWnd, int id, int modifierKeys, int virtualKey);

			[DllImport("user32.dll")]
			public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
		}


		private const int idMin = 0x0001;

		private const int idMax = 0xBFFF;

		private static readonly HashSet<int> ids = new HashSet<int>();


		private int id;


		public ModifierKeys ModifierKeys {
			get => this.modifierKeys;
			set {
				this.modifierKeys = value;

				this.register();
			}
		}
		private ModifierKeys modifierKeys;

		public Key Key {
			get => this.key;
			set {
				this.key = value;

				this.register();
			}
		}
		private Key key;

		public Action Pressed { get; set; }


		public Hotkey(ModifierKeys modifierKeys, Key key, Action pressed) {
			this.ModifierKeys = modifierKeys;
			this.Key = key;
			this.Pressed = pressed;

			ComponentDispatcher.ThreadPreprocessMessage += this.threadPreprocessMessage;
		}

		public void Dispose() {
			ComponentDispatcher.ThreadPreprocessMessage -= this.threadPreprocessMessage;

			this.unregister();
		}

		private void register() {
			this.unregister();

			if(this.Key != Key.None) {
				int modifierKeys = (int)this.ModifierKeys | (int)Native.ModifierKeys.NoRepeat;
				int virtualKey = KeyInterop.VirtualKeyFromKey(this.Key);
				this.id = Enumerable.Range(Hotkey.idMin, Hotkey.idMax - Hotkey.idMin + 1)
					.FirstOrDefault(x => !Hotkey.ids.Contains(x) && Native.RegisterHotKey(IntPtr.Zero, x, modifierKeys, virtualKey));
				Hotkey.ids.Add(this.id);
			}
		}

		private void unregister() {
			if(this.id == 0) {
				return;
			}

			Native.UnregisterHotKey(IntPtr.Zero, this.id);
			Hotkey.ids.Remove(this.id);
			this.id = 0;
		}

		private void threadPreprocessMessage(ref MSG msg, ref bool handled) {
			if(handled || msg.message != (int)Native.WindowMessage.HotKey || (int)msg.wParam != this.id) {
				return;
			}

			this.Pressed?.Invoke();
			handled = true;
		}
	}
}
