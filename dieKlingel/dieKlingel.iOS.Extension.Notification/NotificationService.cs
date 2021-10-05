using Foundation;
using System;
using UIKit;
using UserNotifications;

namespace dieKlingel.iOS.Extension.Notification
{
    [Register("NotificationService")]
    public class NotificationService : UNNotificationServiceExtension
    {
        Action<UNNotificationContent> ContentHandler { get; set; }
        UNMutableNotificationContent BestAttemptContent { get; set; }

        protected NotificationService(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void DidReceiveNotificationRequest(UNNotificationRequest request, Action<UNNotificationContent> contentHandler)
        {
            ContentHandler = contentHandler;
            BestAttemptContent = (UNMutableNotificationContent)request.Content.MutableCopy();
            System.Diagnostics.Debug.WriteLine("Modify Content");
            /*InvokeInBackground(() =>
            {
                NSThread.SleepFor(1);
                AudioToolbox.SystemSound.Vibrate.PlaySystemSound();
                NSThread.SleepFor(0.7);
                AudioToolbox.SystemSound.Vibrate.PlaySystemSound();
            });  */

            // Modify the notification content here...
            //self.bestAttemptContent.title = @"K";
            //self.bestAttemptContent.subtitle = @"";
            //self.bestAttemptContent.body = @"";
            BestAttemptContent.Title += ".";

            // Set the attachment
            
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
        delegate void CompletionHandler(UNNotificationAttachment attach);
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
