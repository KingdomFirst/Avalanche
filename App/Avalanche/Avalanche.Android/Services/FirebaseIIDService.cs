using System;
using Android.App;
using Firebase.Iid;
using Firebase.Messaging;
using Android.Util;

namespace SoutheastChristian.Droid.Services
{
    [Service]
    [IntentFilter( new[] { "com.google.firebase.INSTANCE_ID_EVENT" } )]
    public class FirebaseIIDService : FirebaseInstanceIdService
    {
        const string TAG = "FirebaseIIDService";
        public override void OnTokenRefresh()
        {
            var refreshedToken = FirebaseInstanceId.Instance.Token;
            Log.Debug( TAG, "Refreshed token: " + refreshedToken );
            SendRegistrationToServer( refreshedToken );
        }
        void SendRegistrationToServer( string token )
        {
            // Add custom implementation, as needed.
        }
    }
}