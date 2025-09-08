# OSC

A MelonLoader plugin that provides Open Sound Control (OSC) networking capabilities for other mods. This plugin allows mods to send and receive OSC messages over UDP for real-time communication with external applications like TouchOSC, VRChat OSC, audio software, and other OSC-compatible tools.

## What it does

The OSC plugin provides a complete OSC implementation including:

- **OSC Message Sending**: Send OSC messages to external applications
- **OSC Message Receiving**: Listen for incoming OSC messages and handle them
- **Message Routing**: Register handlers for specific OSC address patterns
- **Thread-Safe Operations**: All operations are safely handled across threads
- **Auto-Configuration**: Automatic setup with sensible defaults

## Usage for Other Mods

To use this OSC plugin as a dependency in your mod, follow these steps:

### 1. Add Reference

Add a reference to the OSC plugin in your mod project and ensure it loads before your mod.

### 2. Basic Usage

```csharp
using OSC;

// Check if OSC is available
if (OSCAPI.IsAvailable)
{
    // Send a simple OSC message
    OSCAPI.Send("/avatar/parameter/MyParameter", 1.0f);
    
    // Send a message with multiple arguments
    OSCAPI.Send("/game/player/position", 10.5f, 25.0f, 5.2f);
}
```

### 3. Receiving Messages

```csharp
// Register a handler for specific address patterns
OSCAPI.RegisterHandler("/input/button/*", (message) => {
    MelonLogger.Msg($"Button input: {message.Address} with {message.Arguments.Count} arguments");
});

// Register a global handler for all messages
OSCAPI.RegisterGlobalHandler((message) => {
    MelonLogger.Msg($"Received: {message}");
});
```

### 4. Configuration

```csharp
// Configure where to send OSC messages
OSCAPI.ConfigureSender("192.168.1.100", 9000);

// Configure which port to listen on
OSCAPI.ConfigureReceiver(9001);

// Start/stop receiving messages
OSCAPI.StartReceiving();
OSCAPI.StopReceiving();
```

### 5. Advanced Usage

```csharp
// Create custom OSC messages
var message = new OSCMessage("/custom/address");
message.AddArgument("Hello");
message.AddArgument(42);
message.AddArgument(3.14f);
OSCAPI.Send(message);

// Get OSC system statistics
var stats = OSCAPI.GetStats();
MelonLogger.Msg($"Sender connected: {stats.IsSenderConnected}");
MelonLogger.Msg($"Receiver listening: {stats.IsReceiverListening}");

// Clean up handlers when your mod unloads
OSCAPI.UnregisterHandler("/input/button/*", myHandler);
OSCAPI.UnregisterGlobalHandler(myGlobalHandler);
```

## Default Configuration

- **Send Target**: `127.0.0.1:9000` (localhost)
- **Receive Port**: `9001`
- **Auto-start Receiver**: `true`

## Error Handling

All OSCAPI methods include built-in error handling and will log errors to the MelonLoader console. Your mod won't crash if the OSC plugin is unavailable or if network errors occur.

## Common Use Cases

- **VRChat Integration**: Send avatar parameters and receive input from VRChat's OSC system
- **External Control**: Control your mod from mobile apps like TouchOSC
- **Audio Software**: Sync with DAWs and audio applications
- **Live Performance**: Real-time parameter control for live streaming or performance
- **Hardware Integration**: Connect with OSC-compatible hardware controllers

## Dependencies

- MelonLoader
- .NET Framework 4.7.2+

## License

This plugin is is under the MIT License. See the LICENSE file for details.