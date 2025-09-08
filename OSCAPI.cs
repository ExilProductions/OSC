using System;

namespace OSC
{
    /// <summary>
    /// Public API for other mods to interact with the OSC system
    /// This class provides a simple interface for other mods to use as a dependency
    /// </summary>
    public static class OSCAPI
    {
        private static OSCManager manager;

        /// <summary>
        /// Get the OSC Manager instance (only available after the OSC plugin is loaded)
        /// </summary>
        public static OSCManager Manager
        {
            get
            {
                if (manager == null)
                {
                    // Try to get it from the Core plugin
                    manager = Core.OSCManager;
                    if (manager == null)
                    {
                        throw new InvalidOperationException("OSC Manager is not available. Make sure the Universal OSC plugin is loaded.");
                    }
                }
                return manager;
            }
        }

        /// <summary>
        /// Check if the OSC system is available
        /// </summary>
        public static bool IsAvailable => Core.OSCManager != null;

        /// <summary>
        /// Send an OSC message
        /// </summary>
        /// <param name="address">OSC address pattern</param>
        /// <param name="arguments">Message arguments</param>
        public static void Send(string address, params object[] arguments)
        {
            try
            {
                Manager.SendMessage(address, arguments);
            }
            catch (Exception ex)
            {
                Core.Logger?.Error($"OSCAPI.Send failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Send an OSC message object
        /// </summary>
        /// <param name="message">The OSC message to send</param>
        public static void Send(OSCMessage message)
        {
            try
            {
                Manager.SendMessage(message);
            }
            catch (Exception ex)
            {
                Core.Logger?.Error($"OSCAPI.Send failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Register a handler for incoming OSC messages with a specific address
        /// </summary>
        /// <param name="address">OSC address pattern to listen for</param>
        /// <param name="handler">Handler function to call when message is received</param>
        public static void RegisterHandler(string address, Action<OSCMessage> handler)
        {
            try
            {
                Manager.RegisterHandler(address, handler);
            }
            catch (Exception ex)
            {
                Core.Logger?.Error($"OSCAPI.RegisterHandler failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Unregister a handler for a specific address
        /// </summary>
        /// <param name="address">OSC address pattern</param>
        /// <param name="handler">Handler function to remove</param>
        public static void UnregisterHandler(string address, Action<OSCMessage> handler)
        {
            try
            {
                Manager.UnregisterHandler(address, handler);
            }
            catch (Exception ex)
            {
                Core.Logger?.Error($"OSCAPI.UnregisterHandler failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Register a handler for all incoming OSC messages
        /// </summary>
        /// <param name="handler">Handler function to call for any message</param>
        public static void RegisterGlobalHandler(Action<OSCMessage> handler)
        {
            try
            {
                Manager.MessageReceived += handler;
            }
            catch (Exception ex)
            {
                Core.Logger?.Error($"OSCAPI.RegisterGlobalHandler failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Unregister a global handler
        /// </summary>
        /// <param name="handler">Handler function to remove</param>
        public static void UnregisterGlobalHandler(Action<OSCMessage> handler)
        {
            try
            {
                Manager.MessageReceived -= handler;
            }
            catch (Exception ex)
            {
                Core.Logger?.Error($"OSCAPI.UnregisterGlobalHandler failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Configure the OSC sender target
        /// </summary>
        /// <param name="targetIP">Target IP address</param>
        /// <param name="targetPort">Target port</param>
        public static void ConfigureSender(string targetIP, int targetPort)
        {
            try
            {
                Manager.ConfigureSender(targetIP, targetPort);
            }
            catch (Exception ex)
            {
                Core.Logger?.Error($"OSCAPI.ConfigureSender failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Configure the OSC receiver port
        /// </summary>
        /// <param name="listenPort">Port to listen on</param>
        public static void ConfigureReceiver(int listenPort)
        {
            try
            {
                Manager.ConfigureReceiver(listenPort);
            }
            catch (Exception ex)
            {
                Core.Logger?.Error($"OSCAPI.ConfigureReceiver failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Get OSC system statistics
        /// </summary>
        /// <returns>OSC statistics object</returns>
        public static OSCStats GetStats()
        {
            try
            {
                return Manager.GetStats();
            }
            catch (Exception ex)
            {
                Core.Logger?.Error($"OSCAPI.GetStats failed: {ex.Message}");
                return new OSCStats();
            }
        }

        /// <summary>
        /// Start receiving OSC messages
        /// </summary>
        public static void StartReceiving()
        {
            try
            {
                Manager.StartReceiving();
            }
            catch (Exception ex)
            {
                Core.Logger?.Error($"OSCAPI.StartReceiving failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Stop receiving OSC messages
        /// </summary>
        public static void StopReceiving()
        {
            try
            {
                Manager.StopReceiving();
            }
            catch (Exception ex)
            {
                Core.Logger?.Error($"OSCAPI.StopReceiving failed: {ex.Message}");
            }
        }
    }
}