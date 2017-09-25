using System;

namespace Capture.Extensions {
	public static class StringExtensions {
		public static T ToEnum<T>(this string value, bool ignoreCase = false) where T : struct {
			return (T)Enum.Parse(typeof(T), value, ignoreCase);
		}
	}
}
