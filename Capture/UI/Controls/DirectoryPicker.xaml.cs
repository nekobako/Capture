using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Capture.UI.Controls {
	public partial class DirectoryPicker : UserControl {
		public static readonly DependencyProperty DirectoryProperty = DependencyProperty.Register(nameof(DirectoryPicker.Directory), typeof(string), typeof(DirectoryPicker), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, null, DirectoryPicker.coerceDirectoryProperty));


		private static string coerceDirectoryProperty(DependencyObject sender, object value) {
			return Environment.ExpandEnvironmentVariables(value as string);
		}


		public string Directory {
			get => this.GetValue(DirectoryPicker.DirectoryProperty) as string;
			set => this.SetValue(DirectoryPicker.DirectoryProperty, value);
		}


		public DirectoryPicker() : base() {
			this.InitializeComponent();
		}

		private void referenceButtonClicked(object sender, RoutedEventArgs args) {
			var dialog = new CommonOpenFileDialog() {
				IsFolderPicker = true,
				InitialDirectory = this.Directory,
				AllowNonFileSystemItems = false,
				EnsureReadOnly = false
			};
			if(dialog.ShowDialog() == CommonFileDialogResult.Ok) {
				this.Directory = dialog.FileName;
			}
		}
	}
}
