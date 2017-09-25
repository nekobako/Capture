using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Capture.Extensions;

namespace Capture {
	public class Screenshot : IDisposable {
		private static class Native {
			public enum DwmWindowAttributeType {
				ExtendedFrameBounds = 0x09
			}


			[StructLayout(LayoutKind.Sequential)]
			public class Point {
				public static implicit operator Point(System.Drawing.Point point) {
					return new Point(point.X, point.Y);
				}

				public static implicit operator System.Drawing.Point(Point point) {
					return new System.Drawing.Point(point.X, point.Y);
				}


				public int X;

				public int Y;


				public Point() {
				}

				public Point(int x, int y) {
					this.X = x;
					this.Y = y;
				}
			}

			[StructLayout(LayoutKind.Sequential)]
			public class Size {
				public static implicit operator Size(System.Drawing.Size size) {
					return new Size(size.Width, size.Height);
				}

				public static implicit operator System.Drawing.Size(Size size) {
					return new System.Drawing.Size(size.Width, size.Height);
				}


				public int Width;

				public int Height;


				public Size() {
				}

				public Size(int width, int height) {
					this.Width = width;
					this.Height = height;
				}
			}

			[StructLayout(LayoutKind.Sequential)]
			public class Rect {
				public static implicit operator Rect(Rectangle rect) {
					return new Rect(rect.Left, rect.Top, rect.Right, rect.Bottom);
				}

				public static implicit operator Rectangle(Rect rect) {
					return new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
				}


				public int Left;

				public int Top;

				public int Right;

				public int Bottom;


				public int X {
					get => this.Left;
					set {
						this.Right -= (this.Left - value);
						this.Left = value;
					}
				}

				public int Y {
					get => this.Top;
					set {
						this.Bottom -= (this.Top - value);
						this.Top = value;
					}
				}

				public Point Location {
					get => new Point(this.X, this.Y);
					set {
						this.X = value.X;
						this.Y = value.Y;
					}
				}

				public int Width {
					get => this.Right - this.Left;
					set => this.Right = value + this.Left;
				}

				public int Height {
					get => this.Bottom - this.Top;
					set => this.Bottom = value + this.Top;
				}

				public Size Size {
					get => new Size(this.Width, this.Height);
					set {
						this.Width = value.Width;
						this.Height = value.Height;
					}
				}


				public Rect() {
				}

				public Rect(int left, int top, int right, int bottom) {
					this.Left = left;
					this.Top = top;
					this.Right = right;
					this.Bottom = bottom;
				}
			}


			[DllImport("user32.dll")]
			public static extern IntPtr GetForegroundWindow();

			[DllImport("user32.dll", CharSet = CharSet.Auto)]
			public static extern int GetWindowTextLength(IntPtr hWnd);

			[DllImport("user32.dll", CharSet = CharSet.Auto)]
			public static extern bool GetWindowText(IntPtr hWnd, StringBuilder text, int maxCount);

			[DllImport("user32.dll")]
			public static extern bool GetClientRect(IntPtr hWnd, [Out] Rect rect);

			[DllImport("user32.dll")]
			public static extern bool ClientToScreen(IntPtr hWnd, [Out] Point point);

			[DllImport("dwmapi.dll")]
			public static extern int DwmGetWindowAttribute(IntPtr hWnd, DwmWindowAttributeType attributeType, [Out] Rect attribute, int attributeSize);
		}

		[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
		public class PathTemplateTagAttribute : Attribute {
			public string Tag { get; private set; }


			public PathTemplateTagAttribute(string tag) {
				this.Tag = tag;
			}
		}


		public enum FileFormat {
			Bmp, Png, Jpg, Gif, Tiff
		}

		public enum PathTemplateTag {
			[PathTemplateTag("{GUID}")] UpperCaseGuid,
			[PathTemplateTag("{guid}")] LowerCaseGuid,
			[PathTemplateTag("{MD5}")] UpperCaseMD5,
			[PathTemplateTag("{md5}")] LowerCaseMD5,
			[PathTemplateTag("{SHA1}")] UpperCaseSHA1,
			[PathTemplateTag("{sha1}")] LowerCaseSHA1,
			[PathTemplateTag("{SHA256}")] UpperCaseSHA256,
			[PathTemplateTag("{sha256}")] LowerCaseSHA256,
			[PathTemplateTag("{WindowTitle}")] WindowTitle,
			[PathTemplateTag("{(.+?)}")] DateAndTimeFormatStrings
		}


		public static Screenshot CaptureRegion(IntPtr hWnd, Rectangle rect) {
			var title = new StringBuilder(Native.GetWindowTextLength(hWnd) + 1);
			if(!Native.GetWindowText(hWnd, title, title.Capacity)) {
				throw new Win32Exception(Properties.Resources.Exception_FailedToGetWindowTitle);
			}

			var bitmap = new Bitmap(rect.Width, rect.Height);
			using(var graphics = Graphics.FromImage(bitmap)) {
				graphics.CopyFromScreen(rect.Location, Point.Empty, rect.Size);
			}

			return new Screenshot(title.ToString(), DateTime.Now, bitmap);
		}

		public static Screenshot CaptureWindow(IntPtr hWnd, bool includeFrame) {
			var windowRect = new Native.Rect();
			if(Native.DwmGetWindowAttribute(hWnd, Native.DwmWindowAttributeType.ExtendedFrameBounds, windowRect, Marshal.SizeOf<Native.Rect>()) != 0) {
				throw new Win32Exception(Properties.Resources.Exception_FailedToMeasureWindow);
			}

			var clientRect = new Native.Rect();
			var clientLocation = new Native.Point();
			if(!Native.GetClientRect(hWnd, clientRect) || !Native.ClientToScreen(hWnd, clientLocation) || clientRect == Rectangle.Empty) {
				throw new Win32Exception(Properties.Resources.Exception_FailedToMeasureWindow);
			}
			clientRect.Location = clientLocation;

			// adjustment for UWP Apps
			if(clientRect.Top == windowRect.Top) {
				clientRect.Top += windowRect.Bottom - clientRect.Bottom;
			}

			return Screenshot.CaptureRegion(hWnd, includeFrame ? windowRect : clientRect);
		}

		public static Screenshot CaptureActiveWindow(bool includeFrame) {
			var hWnd = Native.GetForegroundWindow();
			if(hWnd == IntPtr.Zero) {
				throw new Win32Exception(Properties.Resources.Exception_ActiveWindowWasNotFound);
			}

			return Screenshot.CaptureWindow(hWnd, includeFrame);
		}


		public string Title { get; }

		public DateTime DateTime { get; }

		public Image Image { get; }


		private Screenshot(string title, DateTime dateTime, Image image) {
			this.Title = title;
			this.DateTime = dateTime;
			this.Image = image;
		}

		public string Save(string directoryName, string fileName, FileFormat fileFormat) {
			string tempFilePath = Path.GetTempFileName();
			this.Image.Save(tempFilePath, this.getImageFormat(fileFormat));

			string filePath = Path.Combine(directoryName, $"{fileName}.{fileFormat.ToString().ToLower()}");
			filePath = this.evaluateTemplate(Environment.ExpandEnvironmentVariables(filePath), tempFilePath);

			Directory.CreateDirectory(Path.GetDirectoryName(filePath));

			try {
				File.Move(tempFilePath, filePath);
			}
			catch(Exception ex) {
				File.Delete(tempFilePath);
				throw ex;
			}

			return filePath;
		}

		public void Dispose() {
			this.Image.Dispose();
		}


		private ImageFormat getImageFormat(FileFormat fileFormat) {
			switch(fileFormat) {
				case FileFormat.Bmp:
					return ImageFormat.Bmp;
				case FileFormat.Png:
					return ImageFormat.Png;
				case FileFormat.Jpg:
					return ImageFormat.Jpeg;
				case FileFormat.Gif:
					return ImageFormat.Gif;
				case FileFormat.Tiff:
					return ImageFormat.Tiff;
				default:
					return null;
			}
		}

		private string evaluateTemplate(string template, string path) {
			var algorithms = new HashAlgorithm[] { MD5.Create(), SHA1.Create(), SHA256.Create() };
			var hashes = this.getFileHashes(path, algorithms);
			foreach(var algorithm in algorithms) {
				algorithm.Dispose();
			}

			template = Regex.Replace(template, PathTemplateTag.UpperCaseGuid.GetAttribute<PathTemplateTagAttribute>().Tag, match => Guid.NewGuid().ToString("N").ToUpper());
			template = Regex.Replace(template, PathTemplateTag.LowerCaseGuid.GetAttribute<PathTemplateTagAttribute>().Tag, match => Guid.NewGuid().ToString("N").ToLower());

			template = Regex.Replace(template, PathTemplateTag.UpperCaseMD5.GetAttribute<PathTemplateTagAttribute>().Tag, match => hashes[0].ToUpper());
			template = Regex.Replace(template, PathTemplateTag.LowerCaseMD5.GetAttribute<PathTemplateTagAttribute>().Tag, match => hashes[0].ToLower());

			template = Regex.Replace(template, PathTemplateTag.UpperCaseSHA1.GetAttribute<PathTemplateTagAttribute>().Tag, match => hashes[1].ToUpper());
			template = Regex.Replace(template, PathTemplateTag.LowerCaseSHA1.GetAttribute<PathTemplateTagAttribute>().Tag, match => hashes[1].ToLower());

			template = Regex.Replace(template, PathTemplateTag.UpperCaseSHA256.GetAttribute<PathTemplateTagAttribute>().Tag, match => hashes[2].ToUpper());
			template = Regex.Replace(template, PathTemplateTag.LowerCaseSHA256.GetAttribute<PathTemplateTagAttribute>().Tag, match => hashes[2].ToLower());

			template = Regex.Replace(template, PathTemplateTag.WindowTitle.GetAttribute<PathTemplateTagAttribute>().Tag, match => this.Title.Trim());

			template = Regex.Replace(template, PathTemplateTag.DateAndTimeFormatStrings.GetAttribute<PathTemplateTagAttribute>().Tag, match => this.DateTime.ToString(match.Groups[1].Value));

			return template;
		}

		private string[] getFileHashes(string path, HashAlgorithm[] algorithms) {
			using(var stream = new FileStream(path, FileMode.Open, FileAccess.Read)) {
				return algorithms
					.Select(x => BitConverter.ToString(x.ComputeHash(stream)).Replace("-", string.Empty))
					.ToArray();
			}
		}
	}
}
