using System;

namespace Capture.Extensions {
	public static class EnumExtensions {
		public static T GetAttribute<T>(this Enum value) where T : Attribute {
			var attributes = value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(T), false);
			return (attributes?.Length ?? 0) <= 0 ? null : (T)attributes[0];
		}
	}
}
