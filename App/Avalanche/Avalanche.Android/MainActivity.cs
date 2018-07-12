// <copyright>
// Copyright Southeast Christian Church
// Mark Lee
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

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using FFImageLoading.Forms.Droid;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Android.Util;
using Android.Gms.Common;
using Firebase.Iid;

namespace Avalanche.Droid
{
    [Activity( Label = "Southeast Christian", Icon = "@drawable/icon", Theme = "@style/MainTheme", ScreenOrientation = ScreenOrientation.Sensor, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        const string TAG = "MainActivity";
        protected override void OnCreate( Bundle bundle )
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate( bundle );

            if ( Intent.Extras != null )
            {
                foreach ( var key in Intent.Extras.KeySet() )
                {
                    var value = Intent.Extras.GetString( key );
                    Log.Debug( TAG, "Key: {0} Value: {1}", key, value );
                }
            }

            Plugin.CurrentActivity.CrossCurrentActivity.Current.Activity = this;
            CachedImageRenderer.Init();
            var t = IsPlayServicesAvailable();
            global::Xamarin.Forms.Forms.Init( this, bundle );
            LoadApplication( new Avalanche.App() );
        }
        string debug;

        public bool IsPlayServicesAvailable()
        {
            int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable( this );
            if ( resultCode != ConnectionResult.Success )
            {
                if ( GoogleApiAvailability.Instance.IsUserResolvableError( resultCode ) )
                    debug = GoogleApiAvailability.Instance.GetErrorString( resultCode );
                else
                {
                    debug = "This device is not supported";
                    Finish();
                }
                return false;
            }
            else
            {

                debug = FirebaseInstanceId.Instance.Token;
                return true;
            }
        }
    }
}

