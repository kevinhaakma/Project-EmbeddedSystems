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
        private TextView TPSText;
        private TextView RPMText;
        private TextView IATText;
        private TextView VSSText;
        private TextView VOLTText;
        private TextView MAPText;
        private TextView AFRText;
        public static Thread Thread;

        private Bluetooth bluetooth = MainActivity.bluetooth;
        public static EventWaitHandle waitHandle = new ManualResetEvent(initialState: true);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.sensor_main);

            string[] output = new string[7] { "0", "0", "0", "0", "0", "0", "0" };

            TPSText = FindViewById<TextView>(Resource.Id.TPS);
            RPMText = FindViewById<TextView>(Resource.Id.RPM);
            IATText = FindViewById<TextView>(Resource.Id.IAT);
            VSSText = FindViewById<TextView>(Resource.Id.VSS);
            VOLTText = FindViewById<TextView>(Resource.Id.VOLT);
            MAPText = FindViewById<TextView>(Resource.Id.MAP);
            AFRText = FindViewById<TextView>(Resource.Id.AFR);

            ProgressBar progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar1);
            progressBar.Max = 7000;

            Thread = new Thread(() =>
            {
                while (true)
                {
                    output = bluetooth.GetData();

                    if (output[0] != null)
                    {
                        if (!output[0].Contains("E"))
                        {
                            RunOnUiThread(() =>
                            {
                                //TPSText.Text = "TPS: " + output[0] + "%";
                                progressBar.Progress = Convert.ToInt32(output[1]);
                                RPMText.Text = "RPM: " + output[1];
                                //IATText.Text = "IAT: " + output[2] + " °C";
                                //VSSText.Text = "VSS: " + output[3] + " KM/H";
                                //VOLTText.Text = "VOLT: " + output[4];
                                //MAPText.Text = "MAP: " + output[5] + " kPa";
                                //AFRText.Text = "AFR: " + output[6];
                            });
                            Thread.Sleep(5);
                        }

                        else
                        {
                            Thread.Sleep(100);
                            //close();
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

            //waitHandle.Set();
        }

        protected override void OnPause()
        {
            base.OnPause();
            Thread.Suspend();
            //waitHandle.Reset();
        }
    }
}