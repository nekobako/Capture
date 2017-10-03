using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using MetroRadiance.UI;
using Capture.Extensions;
using Capture.Helpers;
using Capture.UI.Controls;
using Capture.UI.Windows;
using R = Capture.Properties.Resources;
using S = Capture.Properties.Settings;

namespace Capture {
	public partial class App : Application {
		public enum ToastAction {
			Open, Delete
		}


		private readonly Toast successToast = new Toast($@"
			<toast launch='{ToastAction.Open}'>
				<visual>
					<binding template='ToastGeneric'>
						<text>{R.Toast_Title_Success}</text>
						<image src='' />
					</binding>
				</visual>
				<actions>
					<action arguments='{ToastAction.Open}' content='{R.Toast_Action_Open}' />
					<action arguments='{ToastAction.Delete}' content='{R.Toast_Action_Delete}' />
				</actions>
			</toast>");

		private readonly Toast failureToast = new Toast(R.Toast_Title_Failed, string.Empty);

		private TaskTrayIcon taskTrayIcon;

		private IReadOnlyList<Hotkey> hotkeys;


		public App() : base() {
		}

		protected override void OnStartup(StartupEventArgs args) {
			base.OnStartup(args);

			ThemeService.Current.Register(this, Theme.Windows, Accent.Windows);

			this.taskTrayIcon = new TaskTrayIcon() {
				Items = new[] {
					new TaskTrayIconItem(R.TaskTrayIconItem_Settings, WindowHelper.ShowOrActivate<SettingsWindow>),
					new TaskTrayIconItem(R.TaskTrayIconItem_Quit, this.Shutdown)
				},
				Icon = IconHelper.LoadIcon("Assets/White.ico")
			};

			this.registerHotkeys();

			S.Default.PropertyChanged += this.settingsPropertyChanged;
		}

		protected override void OnExit(ExitEventArgs args) {
			base.OnExit(args);

			S.Default.PropertyChanged -= this.settingsPropertyChanged;

			this.unregisterHotkeys();

			this.taskTrayIcon.Dispose();
		}

		private void hotkeyPressed(Func<Screenshot> capture) {
			this.successToast.Hide();
			this.failureToast.Hide();

			try {
				using(var screenshot = capture()) {
					this.successToast.ImagePath = screenshot.Save(S.Default.DirectoryName, S.Default.FileName, S.Default.FileFormat);
				}

				this.successToast.Activated = (arguments, data) => this.toastActivated(arguments.ToEnum<ToastAction>());
				this.successToast.Show();
			}
			catch(Exception ex) {
				this.failureToast.Message = ex.Message;
				this.failureToast.Show();
			}
		}

		private void toastActivated(ToastAction action) {
			if(!File.Exists(this.successToast.ImagePath)) {
				return;
			}

			switch(action) {
				case ToastAction.Open:
					Process.Start(this.successToast.ImagePath);
					break;
				case ToastAction.Delete:
					File.Delete(this.successToast.ImagePath);
					break;
			}
		}

		private void settingsPropertyChanged(object sender, PropertyChangedEventArgs args) {
			this.registerHotkeys();
		}

		private void registerHotkeys() {
			this.unregisterHotkeys();

			this.hotkeys = new List<Hotkey>() {
				new Hotkey(S.Default.Hotkey_CaptureActiveWindow_ModifierKeys, S.Default.Hotkey_CaptureActiveWindow_Key, () => this.hotkeyPressed(() => Screenshot.CaptureActiveWindow(false))),
				new Hotkey(S.Default.Hotkey_CaptureActiveWindowWithFrame_ModifierKeys, S.Default.Hotkey_CaptureActiveWindowWithFrame_Key, () => this.hotkeyPressed(() => Screenshot.CaptureActiveWindow(true)))
			};
		}

		private void unregisterHotkeys() {
			if(this.hotkeys == null) {
				return;
			}

			foreach(var hotkey in this.hotkeys) {
				hotkey.Dispose();
			}

			this.hotkeys = null;
		}
	}
}
