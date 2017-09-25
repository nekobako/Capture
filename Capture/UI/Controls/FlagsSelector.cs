using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Capture.UI.Controls {
	public partial class FlagsSelector : ListBox {
		public static readonly DependencyProperty FlagPathProperty = DependencyProperty.Register(nameof(FlagsSelector.FlagPath), typeof(string), typeof(FlagsSelector), new FrameworkPropertyMetadata(string.Empty, FlagsSelector.flagPathPropertyChanged));

		public static readonly DependencyProperty SelectedFlagsProperty = DependencyProperty.Register(nameof(FlagsSelector.SelectedFlags), typeof(object), typeof(FlagsSelector), new FrameworkPropertyMetadata(default(object), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, FlagsSelector.selectedFlagsPropertyChanged));

		private static readonly DependencyProperty itemFlagProperty = DependencyProperty.Register(nameof(FlagsSelector.itemFlag), typeof(object), typeof(FlagsSelector));


		private static void flagPathPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) {
			(sender as FlagsSelector).updateSelection();
		}

		private static void selectedFlagsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) {
			(sender as FlagsSelector).updateSelection();
		}


		public string FlagPath {
			get => this.GetValue(FlagsSelector.FlagPathProperty) as string;
			set => this.SetValue(FlagsSelector.FlagPathProperty, value);
		}

		public object SelectedFlags {
			get => this.GetValue(FlagsSelector.SelectedFlagsProperty);
			set => this.SetValue(FlagsSelector.SelectedFlagsProperty, value);
		}

		private object itemFlag => this.GetValue(FlagsSelector.itemFlagProperty);


		protected override void OnItemsChanged(NotifyCollectionChangedEventArgs args) {
			base.OnItemsChanged(args);

			this.updateSelection();
		}

		protected override void OnSelectionChanged(SelectionChangedEventArgs args) {
			base.OnSelectionChanged(args);

			if(this.SelectedFlags == null) {
				return;
			}

			object flags = Enum.ToObject(this.SelectedFlags.GetType(), this.SelectedItems
				.Cast<object>()
				.Sum(x => (int)this.getItemFlag(x)));
			if(!this.SelectedFlags.Equals(flags)) {
				this.SelectedFlags = flags;
			}
		}

		private object getItemFlag(object item) {
			if(string.IsNullOrEmpty(this.FlagPath)) {
				return item;
			}

			this.SetBinding(FlagsSelector.itemFlagProperty, new Binding() {
				Source = item,
				Path = new PropertyPath(this.FlagPath)
			});
			return this.itemFlag;
		}

		private void updateSelection() {
			if(this.SelectedFlags == null) {
				this.UnselectAll();
			}
			else {
				this.SetSelectedItems(this.Items
					.Cast<object>()
					.Where(x => ((int)this.SelectedFlags & (int)this.getItemFlag(x)) != 0));
			}
		}
	}
}
