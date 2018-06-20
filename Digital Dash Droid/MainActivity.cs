using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using System.Threading;

namespace Digital_Dash_Droid
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true, ScreenOrientation = ScreenOrientation.Landscape)]
    public class MainActivity : AppCompatActivity
    {
        public static Bluetooth bluetooth = new Bluetooth();
        private static Thread thread;
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            TextView statusText = FindViewById<TextView>(Resource.Id.Status);
            Intent intent = new Intent(this, typeof(SecondActivity));

            thread = new Thread(() =>
            {
                string[] output = new string[7] { "0", "0", "0", "0", "0", "0", "0" };
                while (true)
                {
                    output = bluetooth.GetData();
                    Thread.Sleep(10);

                    if (output[0] != null && output[0] != "")
                    {
                        if (output[4] == "0")
                        {
                            RunOnUiThread(() =>
                            {
                            //no dlc connected
                            statusText.Text = "No Data-Link connected / Turn key to ignition";
                                statusText.SetTextColor(Color.Red);
                            });
                            Thread.Sleep(100);
                        }

                        else
                        {
                            Thread.Sleep(500);
                            StartActivity(intent);
                        }
                    }
                }
            });

            thread.Start();
        }

        protected override void OnResume()
        {
            base.OnResume();

            if(thread.ThreadState == ThreadState.Suspended)
                thread.Resume();
        }

        protected override void OnPause()
        {
            base.OnPause();
            thread.Suspend();
        }
    }
}
