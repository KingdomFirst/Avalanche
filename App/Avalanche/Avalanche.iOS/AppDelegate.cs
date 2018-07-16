// <copyright>
// Copyright Southeast Christian Church
//
// Licensed under the  Southeast Christian Church License (the "License");
// you may not use this file except in compliance with the License.
// A copy of the License shoud be included with this file.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Linq;
using FFImageLoading.Forms.Touch;
using Firebase.InstanceID;
using Firebase.Core;
using Firebase.CloudMessaging;
using Foundation;
using UIKit;
using UserNotifications;
using Avalanche.Utilities;

namespace Avalanche.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register( "AppDelegate" )]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate, IUNUserNotificationCenterDelegate, IMessagingDelegate
    {

        public event EventHandler<UserInfoEventArgs> MessageReceived;

        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching( UIApplication app, NSDictionary options )
        {
            Firebase.Core.App.Configure();

            // Register your app for remote notifications.
            if ( UIDevice.CurrentDevice.CheckSystemVersion( 10, 0 ) )
            {
                // For iOS 10 display notification (sent via APNS)
                UNUserNotificationCenter.Current.Delegate = this;

                var authOptions = UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound;
                UNUserNotificationCenter.Current.RequestAuthorization( authOptions, ( granted, error ) =>
                {
                    Console.WriteLine( granted );
                } );
            }
            else
            {
                // iOS 9 or before
                var allNotificationTypes = UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound;
                var settings = UIUserNotificationSettings.GetSettingsForTypes( allNotificationTypes, null );
                UIApplication.SharedApplication.RegisterUserNotificationSettings( settings );
            }

            UIApplication.SharedApplication.RegisterForRemoteNotifications();

            Messaging.SharedInstance.Delegate = this;

            // To connect with FCM. FCM manages the connection, closing it
            // when your app goes into the background and reopening it 
            // whenever the app is foregrounded.
            Messaging.SharedInstance.ShouldEstablishDirectChannel = true;

            CachedImageRenderer.Init();
            global::Xamarin.Forms.Forms.Init();
            LoadApplication( new Avalanche.App() );

            return base.FinishedLaunching( app, options );
        }


        [Export( "messaging:didReceiveRegistrationToken:" )]
        public void DidReceiveRegistrationToken( Messaging messaging, string fcmToken )
        {
            Console.WriteLine( fcmToken );
            FCMHelper.RegisterFCMToken( fcmToken );
        }


        public override void DidReceiveRemoteNotification( UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler )
        {
            // Handle Notification messages in the background and foreground.
            // Handle Data messages for iOS 9 and below.

            // If you are receiving a notification message while your app is in the background,
            // this callback will not be fired till the user taps on the notification launching the application.
            // TODO: Handle data of notification

            // With swizzling disabled you must let Messaging know about the message, for Analytics
            //Messaging.SharedInstance.AppDidReceiveMessage (userInfo);

            HandleMessage( userInfo );

            // Print full message.
            LogInformation( nameof( DidReceiveRemoteNotification ), userInfo );

            completionHandler( UIBackgroundFetchResult.NewData );
        }

        [Export( "messaging:didReceiveMessage:" )]
        public void DidReceiveMessage( Messaging messaging, RemoteMessage remoteMessage )
        {
            // Handle Data messages for iOS 10 and above.
            HandleMessage( remoteMessage.AppData );

            LogInformation( nameof( DidReceiveMessage ), remoteMessage.AppData );
        }

        void HandleMessage( NSDictionary message )
        {
            if ( MessageReceived != null )
            {
                MessageType messageType;
                if ( message.ContainsKey( new NSString( "aps" ) ) )
                    messageType = MessageType.Notification;
                else
                    messageType = MessageType.Data;

                var e = new UserInfoEventArgs( message, messageType );
                MessageReceived( this, e );
            }
            else
            {
                string title = "";
                string body = "";
                var apsDictionary = message["aps"] as NSDictionary;
                if ( apsDictionary["alert"] is NSDictionary alertDictionary )
                {
                    title = alertDictionary["title"].ToString();
                    body = alertDictionary["body"].ToString();
                }
                else
                {
                    body = apsDictionary["alert"].ToString();
                }
                var window = UIApplication.SharedApplication.KeyWindow;
                var vc = window.RootViewController;
                while ( vc.PresentedViewController != null )
                {
                    vc = vc.PresentedViewController;
                }
                ShowMessage( title, body, vc );
            }
        }

        public static void ShowMessage( string title, string message, UIViewController fromViewController, Action actionForOk = null )
        {
            if ( UIDevice.CurrentDevice.CheckSystemVersion( 8, 0 ) )
            {
                var alert = UIAlertController.Create( title, message, UIAlertControllerStyle.Alert );
                alert.AddAction( UIAlertAction.Create( "Ok", UIAlertActionStyle.Default, ( obj ) => actionForOk?.Invoke() ) );
                fromViewController.PresentViewController( alert, true, null );
            }
            else
            {
                var alert = new UIAlertView( title, message, null, "Ok", null );
                alert.Clicked += ( sender, e ) => actionForOk?.Invoke();
                alert.Show();
            }
        }

        // Receive displayed notifications for iOS 10 devices.
        // Handle incoming notification messages while app is in the foreground.
        [Export( "userNotificationCenter:willPresentNotification:withCompletionHandler:" )]
        public void WillPresentNotification( UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler )
        {
            var userInfo = notification.Request.Content.UserInfo;

            // With swizzling disabled you must let Messaging know about the message, for Analytics
            //Messaging.SharedInstance.AppDidReceiveMessage (userInfo);

            // Print full message.
            Console.WriteLine( userInfo );

            // Change this to your preferred presentation option
            completionHandler( UNNotificationPresentationOptions.Alert );
        }

        // Handle notification messages after display notification is tapped by the user.
        [Export( "userNotificationCenter:didReceiveNotificationResponse:withCompletionHandler:" )]
        public void DidReceiveNotificationResponse( UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler )
        {
            var userInfo = response.Notification.Request.Content.UserInfo;

            // Print full message.
            Console.WriteLine( userInfo );

            completionHandler();
        }

        void LogInformation( string methodName, object information ) => Console.WriteLine( $"\nMethod name: {methodName}\nInformation: {information}" );
    }
}

