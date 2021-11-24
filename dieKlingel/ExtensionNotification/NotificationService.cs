using Foundation;
using System;
using UIKit;
using UserNotifications;

namespace ExtensionNotification
{
    [Register("NotificationService")]
    public class NotificationService : UNNotificationServiceExtension
    {
        Action<UNNotificationContent> ContentHandler { get; set; }
        UNMutableNotificationContent BestAttemptContent { get; set; }
        delegate void CompletionHandler(UNNotificationAttachment attach);

        protected NotificationService(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void DidReceiveNotificationRequest(UNNotificationRequest request, Action<UNNotificationContent> contentHandler)
        {
            ContentHandler = contentHandler;
            BestAttemptContent = (UNMutableNotificationContent)request.Content.MutableCopy();

            // Modify the notification content here...
            //BestAttemptContent.Title = string.Format("{0}[modified]", BestAttemptContent.Title);
            BestAttemptContent.Title = string.Format("{0}.", BestAttemptContent.Title);

            NSDictionary dict = BestAttemptContent.UserInfo;
            NSString imgUrl = dict["image-path"] as NSString;

            if (imgUrl.Length == null)
            {
                ContentHandler(BestAttemptContent);
            }
            LoadAttachmentForUrlString(imgUrl, "png", (attach) =>
            {
                if (attach != null)
                {
                    BestAttemptContent.Attachments = new UNNotificationAttachment[] { attach };
                }

                ContentHandler(BestAttemptContent);
            });
            //ContentHandler(BestAttemptContent);
        }

        void LoadAttachmentForUrlString(string urlString, string type, CompletionHandler completionHandler)
        {
            NSUrl attachmentURL = new NSUrl(urlString);

            NSUrlSession session = NSUrlSession.FromConfiguration(NSUrlSessionConfiguration.DefaultSessionConfiguration);
            session.CreateDownloadTask(attachmentURL, (url, response, error) => {

                if (error != null)
                {
                    // Fail
                }
                else
                {
                    NSFileManager fileManager = NSFileManager.DefaultManager;
                    NSUrl localUrl = new NSUrl(url.Path + "." + type, false);
                    NSError moveError = null;
                    fileManager.Move(url, localUrl, out moveError);

                    NSError attachmentError = null;
                    UNNotificationAttachment attachment = UNNotificationAttachment.FromIdentifier("", localUrl, options: null, error: out attachmentError);
                    if (attachmentError != null)
                    {
                        // Fail
                    }
                    else
                    {
                        completionHandler(attachment);
                    }
                }
            }).Resume();
        }

        public override void TimeWillExpire()
        {
            // Called just before the extension will be terminated by the system.
            // Use this as an opportunity to deliver your "best attempt" at modified content, otherwise the original push payload will be used.

            ContentHandler(BestAttemptContent);
        }
    }
}
