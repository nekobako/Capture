using System;
using System.Collections.Generic;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Capture.UI.Controls {
	public class Toast {
		private readonly ToastNotifier notifier;

		private ToastNotification notification;


		public string Title {
			get => this.Content.GetElementsByTagName("text")[0].InnerText;
			set => this.Content.GetElementsByTagName("text")[0].InnerText = value;
		}

		public string Message {
			get => this.Content.GetElementsByTagName("text")[1].InnerText;
			set => this.Content.GetElementsByTagName("text")[1].InnerText = value;
		}

		public string ImagePath {
			get => this.Content.GetElementsByTagName("image")[0].Attributes.GetNamedItem("src").NodeValue as string;
			set => this.Content.GetElementsByTagName("image")[0].Attributes.GetNamedItem("src").NodeValue = value;
		}

		public XmlDocument Content { get; set; }

		public Action<string, IDictionary<string, string>> Activated { get; set; }


		public Toast(string title, string message) : this(ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText02)) {
			this.Title = title;
			this.Message = message;
		}

		public Toast(string content) : this(new XmlDocument()) {
			this.Content.LoadXml(content);
		}

		public Toast(XmlDocument content) {
			this.Content = content;
			this.notifier = ToastNotificationManager.CreateToastNotifier(@"{1AC14E77-02E7-4E5D-B744-2EB1AE5198B7}\WindowsPowerShell\v1.0\powershell.exe");
		}

		public void Show() {
			this.notification = new ToastNotification(this.Content);
			this.notification.Activated += (sender, args) => this.Activated?.Invoke((args as ToastActivatedEventArgs)?.Arguments, sender.Data?.Values);
			this.notifier.Show(this.notification);
		}

		public void Hide() {
			if(this.notification == null) {
				return;
			}

			this.notifier.Hide(this.notification);
			this.notification = null;
		}
	}
}
