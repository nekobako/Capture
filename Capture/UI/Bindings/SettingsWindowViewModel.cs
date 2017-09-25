using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Capture.Extensions;

namespace Capture.UI.Bindings {
	public class SettingsWindowViewModel : DependencyObject {
		public IReadOnlyDictionary<ModifierKeys, string> ModifierKeys { get; }

		public IReadOnlyDictionary<Key, string> Keys { get; }

		public IReadOnlyDictionary<Screenshot.FileFormat, string> FileFormats { get; }

		public IReadOnlyCollection<object> CaseSensitivePathTemplateTags { get; }

		public IReadOnlyCollection<object> CaseInsensitivePathTemplateTags { get; }


		public SettingsWindowViewModel() : base() {
			this.ModifierKeys = Enum.GetNames(typeof(ModifierKeys))
				.Select(x => new { Name = x, Value = x.ToEnum<ModifierKeys>() })
				.Where(x => x.Value != 0)
				.ToDictionary(x => x.Value, x => x.Name);

			this.Keys = Enum.GetNames(typeof(Key))
				.Select(x => new { Name = x, Value = x.ToEnum<Key>() })
				.Where(x => x.Value != 0)
				.GroupBy(x => x.Value, x => x.Name)
				.ToDictionary(x => x.Key, x => string.Join(" / ", x));

			this.FileFormats = Enum.GetNames(typeof(Screenshot.FileFormat))
				.ToDictionary(x => x.ToEnum<Screenshot.FileFormat>(), x => x.ToLower());

			this.CaseSensitivePathTemplateTags = new[] {
				new {
					UpperCaseTag = Screenshot.PathTemplateTag.UpperCaseGuid.GetAttribute<Screenshot.PathTemplateTagAttribute>().Tag,
					LowerCaseTag = Screenshot.PathTemplateTag.LowerCaseGuid.GetAttribute<Screenshot.PathTemplateTagAttribute>().Tag,
					Description = Properties.Resources.SettingsWindow_Description_Guid
				},
				new {
					UpperCaseTag = Screenshot.PathTemplateTag.UpperCaseMD5.GetAttribute<Screenshot.PathTemplateTagAttribute>().Tag,
					LowerCaseTag = Screenshot.PathTemplateTag.LowerCaseMD5.GetAttribute<Screenshot.PathTemplateTagAttribute>().Tag,
					Description = Properties.Resources.SettingsWindow_Description_MD5
				},
				new {
					UpperCaseTag = Screenshot.PathTemplateTag.UpperCaseSHA1.GetAttribute<Screenshot.PathTemplateTagAttribute>().Tag,
					LowerCaseTag = Screenshot.PathTemplateTag.LowerCaseSHA1.GetAttribute<Screenshot.PathTemplateTagAttribute>().Tag,
					Description = Properties.Resources.SettingsWindow_Description_SHA1
				},
				new {
					UpperCaseTag = Screenshot.PathTemplateTag.UpperCaseSHA256.GetAttribute<Screenshot.PathTemplateTagAttribute>().Tag,
					LowerCaseTag = Screenshot.PathTemplateTag.LowerCaseSHA256.GetAttribute<Screenshot.PathTemplateTagAttribute>().Tag,
					Description = Properties.Resources.SettingsWindow_Description_SHA256
				}
			};

			this.CaseInsensitivePathTemplateTags = new[] {
				new {
					Tag = Screenshot.PathTemplateTag.WindowTitle.GetAttribute<Screenshot.PathTemplateTagAttribute>().Tag,
					Description = Properties.Resources.SettingsWindow_Description_WindowTitle
				}
			};
		}
	}
}
