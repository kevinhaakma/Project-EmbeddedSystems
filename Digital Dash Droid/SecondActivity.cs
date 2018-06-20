using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Digital_Dash_Droid
{
    [Activity(Label = "SecondActivity", Theme = "@style/AppTheme", ScreenOrientation = ScreenOrientation.Landscape)]
    public class SecondActivity : Activity
    {
        private static Thread Thread;

        private Bluetooth bluetooth = MainActivity.bluetooth;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.sensor_main);

            TextView TPSText = FindViewById<TextView>(Resource.Id.TPS);
            TextView RPMText = FindViewById<TextView>(Resource.Id.RPM);
            TextView IATText = FindViewById<TextView>(Resource.Id.IAT);
            TextView VSSText = FindViewById<TextView>(Resource.Id.VSS);
            TextView VOLTText = FindViewById<TextView>(Resource.Id.VOLT);
            TextView MAPText = FindViewById<TextView>(Resource.Id.MAP);
            TextView AFRText = FindViewById<TextView>(Resource.Id.AFR);

            ProgressBar progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar1);
            progressBar.Max = 7000;

            Thread = new Thread(() =>
            {
                string[] output = new string[7] { "0", "0", "0", "0", "0", "0", "0" };
                while (true)
                {
                    output = bluetooth.GetData();
                    Thread.Sleep(10);

                    if (output[0] != null && output[0] != "")
                    {
                        if (output[4] != "0")
                        {
                            RunOnUiThread(() =>
                            {
                                TPSText.Text = "TPS: " + output[0] + "%";
                                progressBar.Progress = Convert.ToInt32(output[1]);
                                RPMText.Text = "RPM: " + output[1];
                                IATText.Text = "IAT: " + output[2] + " °C";
                                VSSText.Text = "VSS: " + output[3] + " KM/H";
                                VOLTText.Text = "VOLT: " + output[4];
                                MAPText.Text = "MAP: " + output[5] + " kPa";
                                AFRText.Text = "AFR: " + output[6];
                            });
                            Thread.Sleep(5);
                        }

                        else
                        {
                            Thread.Sleep(100);
                            Finish();
                        }
                    }
                }
            });

            Thread.Start();
        }

        protected override void OnResume()
        {
            base.OnResume();

            if (Thread.ThreadState == ThreadState.Suspended)
                Thread.Resume();
        }

        protected override void OnPause()
        {
            base.OnPause();
            Thread.Suspend();
        }
    }
}