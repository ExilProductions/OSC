using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace OSC
{
    /// <summary>
    /// Handles sending OSC messages over UDP
    /// </summary>
    public class OSCSender : IDisposable
    {
        private UdpClient udpClient;
        private IPEndPoint targetEndPoint;
        private bool disposed = false;

        public string TargetIP { get; private set; }
        public int TargetPort { get; private set; }
        public bool IsConnected { get; private set; }

        public OSCSender(string targetIP = "127.0.0.1", int targetPort = 9000)
        {
            TargetIP = targetIP;
            TargetPort = targetPort;
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                udpClient = new UdpClient();
                targetEndPoint = new IPEndPoint(IPAddress.Parse(TargetIP), TargetPort);
                IsConnected = true;
                Core.Logger?.Msg($"OSC Sender initialized - Target: {TargetIP}:{TargetPort}");
            }
            catch (Exception ex)
            {
                Core.Logger?.Error($"Failed to initialize OSC Sender: {ex.Message}");
                IsConnected = false;
            }
        }

        /// <summary>
        /// Send an OSC message
        /// </summary>
        public void Send(OSCMessage message)
        {
            if (!IsConnected || disposed)
            {
                Core.Logger?.Warning("Cannot send OSC message - sender not connected");
                return;
            }

            try
            {
                var data = OSCParser.Encode(message);
                udpClient.Send(data, data.Length, targetEndPoint);
            }
            catch (Exception ex)
            {
                Core.Logger?.Error($"Failed to send OSC message: {ex.Message}");
            }
        }

        /// <summary>
        /// Send a simple OSC message with address and arguments
        /// </summary>
        public void Send(string address, params object[] arguments)
        {
            Send(new OSCMessage(address, arguments));
        }

        /// <summary>
        /// Change the target endpoint
        /// </summary>
        public void SetTarget(string targetIP, int targetPort)
        {
            TargetIP = targetIP;
            TargetPort = targetPort;
            targetEndPoint = new IPEndPoint(IPAddress.Parse(TargetIP), TargetPort);
            Core.Logger?.Msg($"OSC Sender target changed to: {TargetIP}:{TargetPort}");
        }

        public void Dispose()
        {
            if (!disposed)
            {
                udpClient?.Close();
                udpClient?.Dispose();
                IsConnected = false;
                disposed = true;
                Core.Logger?.Msg("OSC Sender disposed");
            }
        }
    }
}