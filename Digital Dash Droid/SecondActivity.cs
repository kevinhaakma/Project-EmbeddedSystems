using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Digital_Dash_Droid
{
    [Activity(Label = "SecondActivity")]
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

            Thread = new Thread(() =>
            {
                while (true)
                {
                    output = bluetooth.GetData();
                    RunOnUiThread(() =>
                    {
                        if (output[0] != null)
                        {
                            if(!output[0].Contains("E"))
                            {
                                TPSText.Text = "TPS: " + output[0] + "%";
                                RPMText.Text = "RPM: " + output[1];
                                IATText.Text = "IAT: " + output[2] + " °C";
                                VSSText.Text = "VSS: " + output[3] + " KM/H";
                                VOLTText.Text = "VOLT: " + output[4];
                                MAPText.Text = "MAP: " + output[5] + " kPa";
                                AFRText.Text = "AFR: " + output[6];
                            }

                            else
                            {
                                Thread.Sleep(100);
                                close();
                            }
                        }
                    });

                    Thread.Sleep(100);
                }
            });

            Thread.IsBackground = true;
            Thread.Start();
        }

        private void close()
        {
            Thread.Abort();
            Finish();
        }
    }
}