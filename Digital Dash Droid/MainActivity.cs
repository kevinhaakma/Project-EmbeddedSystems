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
        private int bt = 0;

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
                    if (bluetooth.IsConnected())
                    {
                        output = bluetooth.GetData();
                        Thread.Sleep(10);

                        if (output[0] != null && output[0] != "")
                        {
                            if (output[4] == "0")
                            {
                                RunOnUiThread(() =>
                                {
                                    bt++;
                                    if (bt % 3 == 1)
                                    {
                                        //no dlc connected
                                        statusText.Text = "No Data-Link connected / Turn key to ignition (•_•)";
                                        statusText.SetTextColor(Color.Red);
                                    }

                                    else if (bt % 3 == 0)
                                    {
                                        //no dlc connected
                                        statusText.Text = "No Data-Link connected / Turn key to ignition ( •_•)>⌐■-■";
                                        statusText.SetTextColor(Color.Red);
                                    }

                                    else
                                    {
                                        //no dlc connected
                                        statusText.Text = "No Data-Link connected / Turn key to ignition (⌐■_■)";
                                        statusText.SetTextColor(Color.Red);
                                    }
                                });
                                Thread.Sleep(100);
                            }

                            else if (output[4] == "-1")
                            {
                                RunOnUiThread(() =>
                                {
                                    bt++;
                                    if(bt % 2 == 0)
                                    {
                                        statusText.Text = "No Bluetooth Connection ~(˘▾˘~)";
                                        statusText.SetTextColor(Color.LightBlue);
                                    }
                                    else
                                    {
                                        statusText.Text = "No Bluetooth Connection (~˘▾˘)~";
                                        statusText.SetTextColor(Color.LightBlue);
                                    }
                                });
                                bluetooth = new Bluetooth();
                                Thread.Sleep(5);
                            }

                            else
                            {
                                Thread.Sleep(500);
                                StartActivity(intent);
                            }
                        }
                    }
                    else
                    {
                        RunOnUiThread(() =>
                        {
                            bt++;
                            if (bt % 2 == 0)
                            {
                                statusText.Text = "No Bluetooth Connection (~˘▾˘)~";
                                statusText.SetTextColor(Color.LightBlue);
                            }
                            else
                            {
                                statusText.Text = "No Bluetooth Connection ~(˘▾˘~)";
                                statusText.SetTextColor(Color.LightBlue);
                            }
                        });
                        bluetooth = new Bluetooth();
                        Thread.Sleep(5);
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
