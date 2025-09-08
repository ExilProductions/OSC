using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace OSC
{
    /// <summary>
    /// Main OSC Manager that handles sending and receiving OSC messages
    /// This class provides the public API for other mods to use as a dependency
    /// </summary>
    public class OSCManager : IDisposable
    {
        private OSCSender sender;
        private OSCReceiver receiver;
        private readonly ConcurrentQueue<OSCMessage> incomingMessages = new ConcurrentQueue<OSCMessage>();
        private readonly Dictionary<string, List<Action<OSCMessage>>> messageHandlers = new Dictionary<string, List<Action<OSCMessage>>>();
        private bool disposed = false;

        // Configuration - make setters public
        public string SendIP { get; set; } = "127.0.0.1";
        public int SendPort { get; set; } = 9000;
        public int ReceivePort { get; set; } = 9001;
        public bool AutoStartReceiver { get; set; } = true;

        // Events
        public event Action<OSCMessage> MessageReceived;
        public event Action<string> OnError;

        // Properties
        public bool IsSenderConnected => sender?.IsConnected ?? false;
        public bool IsReceiverListening => receiver?.IsListening ?? false;

        /// <summary>
        /// Initialize the OSC Manager
        /// </summary>
        public void Initialize()
        {
            try
            {
                // Initialize sender
                sender = new OSCSender(SendIP, SendPort);

                // Initialize receiver
                receiver = new OSCReceiver(ReceivePort);
                receiver.MessageReceived += OnMessageReceived;

                if (AutoStartReceiver)
                {
                    receiver.StartListening();
                }

                Core.Logger?.Msg("OSC Manager initialized successfully");
            }
            catch (Exception ex)
            {
                Core.Logger?.Error($"Failed to initialize OSC Manager: {ex.Message}");
                OnError?.Invoke(ex.Message);
            }
        }

        /// <summary>
        /// Update method to be called from the main thread (handles message processing)
        /// </summary>
        public void Update()
        {
            // Process incoming messages on the main thread
            while (incomingMessages.TryDequeue(out var message))
            {
                ProcessIncomingMessage(message);
            }
        }

        /// <summary>
        /// Send an OSC message
        /// </summary>
        public void SendMessage(OSCMessage message)
        {
            try
            {
                sender?.Send(message);
            }
            catch (Exception ex)
            {
                Core.Logger?.Error($"Failed to send OSC message: {ex.Message}");
                OnError?.Invoke(ex.Message);
            }
        }

        /// <summary>
        /// Send an OSC message with address and arguments
        /// </summary>
        public void SendMessage(string address, params object[] arguments)
        {
            SendMessage(new OSCMessage(address, arguments));
        }

        /// <summary>
        /// Register a handler for messages with a specific address pattern
        /// </summary>
        public void RegisterHandler(string addressPattern, Action<OSCMessage> handler)
        {
            if (!messageHandlers.ContainsKey(addressPattern))
            {
                messageHandlers[addressPattern] = new List<Action<OSCMessage>>();
            }
            messageHandlers[addressPattern].Add(handler);
            Core.Logger?.Msg($"Registered OSC handler for address: {addressPattern}");
        }

        /// <summary>
        /// Unregister a handler for a specific address pattern
        /// </summary>
        public void UnregisterHandler(string addressPattern, Action<OSCMessage> handler)
        {
            if (messageHandlers.ContainsKey(addressPattern))
            {
                messageHandlers[addressPattern].Remove(handler);
                if (messageHandlers[addressPattern].Count == 0)
                {
                    messageHandlers.Remove(addressPattern);
                }
                Core.Logger?.Msg($"Unregistered OSC handler for address: {addressPattern}");
            }
        }

        /// <summary>
        /// Configure sender target
        /// </summary>
        public void ConfigureSender(string targetIP, int targetPort)
        {
            SendIP = targetIP;
            SendPort = targetPort;
            sender?.SetTarget(targetIP, targetPort);
            Core.Logger?.Msg($"OSC Sender configured to: {targetIP}:{targetPort}");
        }

        /// <summary>
        /// Configure receiver port
        /// </summary>
        public void ConfigureReceiver(int listenPort)
        {
            ReceivePort = listenPort;
            receiver?.SetListenPort(listenPort);
            Core.Logger?.Msg($"OSC Receiver configured to listen on port: {listenPort}");
        }

        /// <summary>
        /// Start receiving OSC messages
        /// </summary>
        public void StartReceiving()
        {
            receiver?.StartListening();
        }

        /// <summary>
        /// Stop receiving OSC messages
        /// </summary>
        public void StopReceiving()
        {
            receiver?.StopListening();
        }

        /// <summary>
        /// Get statistics about OSC usage
        /// </summary>
        public OSCStats GetStats()
        {
            return new OSCStats
            {
                IsSenderConnected = IsSenderConnected,
                IsReceiverListening = IsReceiverListening,
                SendIP = SendIP,
                SendPort = SendPort,
                ReceivePort = ReceivePort,
                RegisteredHandlers = messageHandlers.Count
            };
        }

        private void OnMessageReceived(OSCMessage message)
        {
            // Queue the message for processing on the main thread
            incomingMessages.Enqueue(message);
        }

        private void ProcessIncomingMessage(OSCMessage message)
        {
            try
            {
                // Notify global event handlers
                MessageReceived?.Invoke(message);

                // Process registered handlers
                foreach (var kvp in messageHandlers)
                {
                    if (AddressMatches(message.Address, kvp.Key))
                    {
                        foreach (var handler in kvp.Value)
                        {
                            try
                            {
                                handler.Invoke(message);
                            }
                            catch (Exception ex)
                            {
                                Core.Logger?.Error($"Error in OSC message handler: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Core.Logger?.Error($"Error processing OSC message: {ex.Message}");
                OnError?.Invoke(ex.Message);
            }
        }

        private bool AddressMatches(string messageAddress, string pattern)
        {
            // Simple pattern matching - can be enhanced with wildcards later
            return messageAddress == pattern || pattern == "*";
        }

        /// <summary>
        /// Shutdown the OSC Manager
        /// </summary>
        public void Shutdown()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!disposed)
            {
                receiver?.Dispose();
                sender?.Dispose();
                messageHandlers.Clear();
                
                // Clear message queue
                while (incomingMessages.TryDequeue(out _)) { }
                
                disposed = true;
                Core.Logger?.Msg("OSC Manager disposed");
            }
        }
    }

    /// <summary>
    /// Statistics about OSC usage
    /// </summary>
    public class OSCStats
    {
        public bool IsSenderConnected { get; set; }
        public bool IsReceiverListening { get; set; }
        public string SendIP { get; set; }
        public int SendPort { get; set; }
        public int ReceivePort { get; set; }
        public int RegisteredHandlers { get; set; }
    }
}