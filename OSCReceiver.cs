using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace OSC
{
    /// <summary>
    /// Handles receiving OSC messages over UDP
    /// </summary>
    public class OSCReceiver : IDisposable
    {
        private UdpClient udpClient;
        private Thread receiveThread;
        private bool isRunning = false;
        private bool disposed = false;

        public int ListenPort { get; private set; }
        public bool IsListening { get; private set; }

        public event Action<OSCMessage> MessageReceived;

        public OSCReceiver(int listenPort = 9001)
        {
            ListenPort = listenPort;
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                udpClient = new UdpClient(ListenPort);
                Core.Logger?.Msg($"OSC Receiver initialized - Listening on port: {ListenPort}");
            }
            catch (Exception ex)
            {
                Core.Logger?.Error($"Failed to initialize OSC Receiver: {ex.Message}");
            }
        }

        /// <summary>
        /// Start listening for OSC messages
        /// </summary>
        public void StartListening()
        {
            if (IsListening || disposed)
                return;

            try
            {
                isRunning = true;
                IsListening = true;
                receiveThread = new Thread(ReceiveLoop)
                {
                    IsBackground = true,
                    Name = "OSC Receiver Thread"
                };
                receiveThread.Start();
                Core.Logger?.Msg($"OSC Receiver started listening on port {ListenPort}");
            }
            catch (Exception ex)
            {
                Core.Logger?.Error($"Failed to start OSC Receiver: {ex.Message}");
                IsListening = false;
                isRunning = false;
            }
        }

        /// <summary>
        /// Stop listening for OSC messages
        /// </summary>
        public void StopListening()
        {
            if (!IsListening)
                return;

            isRunning = false;
            IsListening = false;

            try
            {
                udpClient?.Close();
                receiveThread?.Join(1000); // Wait up to 1 second for thread to finish
                Core.Logger?.Msg("OSC Receiver stopped listening");
            }
            catch (Exception ex)
            {
                Core.Logger?.Error($"Error stopping OSC Receiver: {ex.Message}");
            }
        }

        private void ReceiveLoop()
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

            while (isRunning)
            {
                try
                {
                    var data = udpClient.Receive(ref remoteEndPoint);
                    var message = OSCParser.Decode(data);
                    
                    // Invoke event on main thread if possible, otherwise invoke directly
                    MessageReceived?.Invoke(message);
                }
                catch (SocketException)
                {
                    // Socket was closed, exit loop
                    break;
                }
                catch (ObjectDisposedException)
                {
                    // UDP client was disposed, exit loop
                    break;
                }
                catch (Exception ex)
                {
                    Core.Logger?.Error($"Error receiving OSC message: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Change the listening port (requires restart)
        /// </summary>
        public void SetListenPort(int port)
        {
            if (IsListening)
            {
                StopListening();
                ListenPort = port;
                udpClient?.Dispose();
                Initialize();
                StartListening();
            }
            else
            {
                ListenPort = port;
                udpClient?.Dispose();
                Initialize();
            }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                StopListening();
                udpClient?.Dispose();
                disposed = true;
                Core.Logger?.Msg("OSC Receiver disposed");
            }
        }
    }
}