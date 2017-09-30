using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Capture.Extensions;

namespace Capture.UI.Controls {
	public partial class HotkeyPicker : UserControl {
		public static readonly DependencyProperty ModifierKeysProperty = DependencyProperty.Register(nameof(HotkeyPicker.ModifierKeys), typeof(ModifierKeys), typeof(HotkeyPicker), new FrameworkPropertyMetadata(default(ModifierKeys), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public static readonly DependencyProperty KeyProperty = DependencyProperty.Register(nameof(HotkeyPicker.Key), typeof(Key), typeof(HotkeyPicker), new FrameworkPropertyMetadata(default(Key), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


		public ModifierKeys ModifierKeys {
			get => (ModifierKeys)this.GetValue(HotkeyPicker.ModifierKeysProperty);
			set => this.SetValue(HotkeyPicker.ModifierKeysProperty, value);
		}

		public Key Key {
			get => (Key)this.GetValue(HotkeyPicker.KeyProperty);
			set => this.SetValue(HotkeyPicker.KeyProperty, value);
		}


		public HotkeyPicker() : base() {
			this.InitializeComponent();

			this.modifierKeysFlagsSelector.ItemsSource = Enum.GetNames(typeof(ModifierKeys))
				.Select(x => new { Name = x, Value = x.ToEnum<ModifierKeys>() })
				.Where(x => x.Value != 0)
				.ToDictionary(x => x.Value, x => x.Name);

			this.keyComboBox.ItemsSource = Enum.GetNames(typeof(Key))
				.Select(x => new { Name = x, Value = x.ToEnum<Key>() })
				.Where(x => x.Value != 0)
				.GroupBy(x => x.Value, x => x.Name)
				.ToDictionary(x => x.Key, x => string.Join(" / ", x));
		}
	}
}
