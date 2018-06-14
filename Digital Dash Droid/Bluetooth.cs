using Android.Bluetooth;
using Java.IO;
using Java.Lang;
using Java.Util;
using System;

namespace Digital_Dash_Droid
{
    class Bluetooth
    {

        private BluetoothAdapter btAdapter;
        private BluetoothSocket socket;

        private InputStreamReader InStream;
        private BufferedReader buffer;

        public Bluetooth()
        {
            btAdapter = BluetoothAdapter.DefaultAdapter;

            if (!btAdapter.IsEnabled)
                btAdapter.Enable();

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
        }

        public string[] GetData()
        {
            return buffer.ReadLine().Split(';');
        }
    }
}