using Android.App;
using Android.Bluetooth;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using Java.IO;
using Java.Util;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Digital_Dash_Droid
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private TextView outputText;
        private TextView countText;

        private BluetoothAdapter btAdapter;
        private BluetoothSocket socket;

        private int Count = 0;

        private InputStreamReader InStream;
        private BufferedReader buffer;

        private System.Timers.Timer timer = new System.Timers.Timer();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            countText = FindViewById<TextView>(Resource.Id.countText);
            outputText = FindViewById<TextView>(Resource.Id.output);

            int Count = 0;
            btAdapter = BluetoothAdapter.DefaultAdapter;

            if (!btAdapter.IsEnabled)
                btAdapter.Enable();

            Connect();

            Button button = FindViewById<Button>(Resource.Id.button);

            timer.Interval = 400;

            timer.Enabled = true;

            button.Click += delegate
            {
                timer.Stop();
            };

            timer.Elapsed += BeginListen;


        }

        public bool Connect()
        {
            string deviceName = "HC-05";
            UUID uuid = UUID.FromString("00001101-0000-1000-8000-00805f9b34fb");
            BluetoothDevice result = null;

            var devices = BluetoothAdapter.DefaultAdapter.BondedDevices;
            if (devices != null)
            {
                foreach (BluetoothDevice device in devices)
                {
                    if (deviceName == device.Name)
                    {
                        result = device;
                        break;
                    }
                }
            }

            socket = result.CreateInsecureRfcommSocketToServiceRecord(uuid);
            socket.Connect();

            btAdapter.CancelDiscovery();

            InStream = new InputStreamReader(socket.InputStream);
            buffer = new BufferedReader(InStream);
            return true;
        }


        public void BeginListen(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (buffer.Ready())
            {
                char[] buffer = new char[32];
                string[] output = new string[7];
                InStream.Read(buffer, 0, buffer.Length);

                bool skip = false;
                byte value = 0;

                foreach (char c in buffer)
                {
                    if (c == '\\')
                    {
                        skip = true;
                        continue;
                    }
                    else if (skip == true)
                    {
                        skip = false;
                        continue;
                    }
                    else if (c == ';')
                    {
                        value++;
                        continue;
                    }
                    else
                    {
                        output[value] += c;
                    }
                }

                if (output[0] != "")
                {
                    outputText.Text = "";
                    foreach (string val in output)
                    {
                        outputText.Text += val + '*';
                    }
                }
            }
            countText.Text = Count++.ToString();
        }
    }
}

