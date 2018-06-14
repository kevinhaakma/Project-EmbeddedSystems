using Android.App;
using Android.Bluetooth;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using Java.IO;
using Java.Util;
using System;
using System.Threading;

namespace Digital_Dash_Droid
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private TextView TPSText;
        private TextView RPMText;
        private TextView IATText;
        private TextView VSSText;
        private TextView VOLTText;
        private TextView MAPText;
        private TextView AFRText;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_main);

            Bluetooth bluetooth = new Bluetooth();

            string[] output = new string[7];

            TPSText = FindViewById<TextView>(Resource.Id.TPS);
            RPMText = FindViewById<TextView>(Resource.Id.RPM);
            IATText = FindViewById<TextView>(Resource.Id.IAT);
            VSSText = FindViewById<TextView>(Resource.Id.VSS);
            VOLTText = FindViewById<TextView>(Resource.Id.VOLT);
            MAPText = FindViewById<TextView>(Resource.Id.MAP);
            AFRText = FindViewById<TextView>(Resource.Id.AFR);

            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    output = bluetooth.GetData();
                }
            });

            Thread UiThread = new Thread(() =>
            {
                while (true)
                {
                    RunOnUiThread(() =>
                    {
                        if (!output[0].Contains("_") || output[0] == null)
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
                            TPSText.Text = output[0];
                    });

                    Thread.Sleep(100);
                }
            });

            Thread.Sleep(1000);

            thread.IsBackground = true;
            thread.Start();

            UiThread.IsBackground = true;
            UiThread.Start();
        }
    }
}
