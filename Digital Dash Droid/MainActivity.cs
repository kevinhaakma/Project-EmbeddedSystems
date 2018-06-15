using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using System.Threading;

namespace Digital_Dash_Droid
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
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

            thread = new Thread(() =>
            {
                bool value = false;
                string[] output = new string[7];
                while (true)
                {
                    output = bluetooth.GetData();
                    bool value = false;
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
                        Intent intent = new Intent(this, typeof(SecondActivity));
                        Thread.Sleep(500);
                        StartActivity(intent);
                        value = true;
                        SecondActivity.waitHandle.Set();
                        waitHandle.Reset();
                    }
                }
            });

            thread.IsBackground = true;
            thread.Start();
        }
    }
}
