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
        public static Thread thread;

        public static EventWaitHandle waitHandle = new ManualResetEvent(initialState: true);
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            TextView statusText = FindViewById<TextView>(Resource.Id.Status);
            Intent intent = new Intent(this, typeof(SecondActivity));

            thread = new Thread(() =>
            {
                bool value = false;
                string[] output = new string[7];
                while (true)
                {
                    output = bluetooth.GetData();
                    if (output[0].Contains("E") && output[0] != null)
                    {
                        value = false;
                        if (output[0].Contains("2"))
                        {
                            RunOnUiThread(() =>
                            {
                                //no dlc connected
                                statusText.Text = "No Data-Link connected";
                                statusText.SetTextColor(Color.Red);
                            });
                            Thread.Sleep(100);
                        }

                        else if (output[0].Contains("3"))
                        {
                            RunOnUiThread(() =>
                            {
                                statusText.Text = "Please turn on your vehicle";
                                statusText.SetTextColor(Color.Orange);
                            });
                            Thread.Sleep(100);
                        }

                        else
                        {
                            Thread.Sleep(200);
                            continue;
                        }
                    }

                    else if (value == false)
                    {
                        Thread.Sleep(500);
                        StartActivity(intent);
                        value = true;
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
