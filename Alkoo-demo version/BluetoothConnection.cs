using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace BluetoothConnection
{
    /// <summary>
    /// Class to control the bluetooth connection to the Arduino.
    /// </summary>
    public class ConnectionManager
    {
        /// <summary>
        /// Socket used to communicate with Arduino.
        /// </summary>
        private StreamSocket socket;

        /// <summary>
        /// DataWriter used to send commands easily.
        /// </summary>
        private DataWriter dataWriter;

        /// <summary>
        /// DataReader used to receive messages easily.
        /// </summary>
        private DataReader dataReader;

        /// <summary>
        /// Thread used to keep reading data from socket.
        /// </summary>
        private BackgroundWorker dataReadWorker;

        /// <summary>
        /// Delegate used by event handler.
        /// </summary>
        /// <param name="message">The message received.</param>
        public delegate void MessageReceivedHandler(int message);

        /// <summary>
        /// Event fired when a new message is received from Arduino.
        /// </summary>
        public event MessageReceivedHandler MessageReceived;

        /// <summary>
        /// Initialize the manager, should be called in OnNavigatedTo of main page.
        /// </summary>
        public void Initialize()
        {
            socket = new StreamSocket();
            dataReadWorker = new BackgroundWorker();
            dataReadWorker.WorkerSupportsCancellation = true;
            dataReadWorker.DoWork += new DoWorkEventHandler(ReceiveMessages);
        }

        /// <summary>
        /// Finalize the connection manager, should be called in OnNavigatedFrom of main page.
        /// </summary>
        public void Terminate()
        {
            if (socket != null)
            {
                socket.Dispose();
            }
            if (dataReadWorker != null)
            {
                dataReadWorker.CancelAsync();
            }
        }

        /// <summary>
        /// Connect to the given host device.
        /// </summary>
        /// <param name="deviceHostName">The host device name.</param>
        public async void Connect(HostName deviceHostName)
        {
            if (socket != null)
            {
                await socket.ConnectAsync(deviceHostName, "1");
                dataWriter = new DataWriter(socket.OutputStream);
                dataReader = new DataReader(socket.InputStream);
                dataReadWorker.RunWorkerAsync();
                Debug.WriteLine(dataWriter != null);
            }
        }

        /// <summary>
        /// Receive messages from the Arduino through bluetooth.
        /// </summary>
        private async void ReceiveMessages(object sender, DoWorkEventArgs e)
        {
            try
            {
                while (true)
                {
                    uint sizeFieldCount = await dataReader.LoadAsync(1);
                    if (sizeFieldCount != 1)
                    {
                        // The underlying socket was closed before we were able to read the whole data. 
                        return;
                    }

                    uint value = dataReader.ReadByte();
                    MessageReceived((int)value);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

        }

        public async void SendByte(byte b)
        {
            if (dataWriter != null)
            {
                Debug.WriteLine("Sending byte");
                dataWriter.WriteByte(b);
                await dataWriter.StoreAsync();
            }
        }

    }
}
