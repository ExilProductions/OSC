# OSC Plugin for MelonLoader

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.7.2+-blue.svg)](https://dotnet.microsoft.com/download/dotnet-framework)
[![MelonLoader](https://img.shields.io/badge/MelonLoader-Compatible-green.svg)](https://melonwiki.xyz/)

A comprehensive MelonLoader plugin that provides Open Sound Control (OSC) networking capabilities for other mods. This plugin enables mods to send and receive OSC messages over UDP for real-time communication with external applications like TouchOSC, VRChat OSC, audio software, live performance tools, and other OSC-compatible applications.

## Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Usage for Other Mods](#usage-for-other-mods)
- [Configuration](#configuration)
- [Examples](#examples)
- [Project Structure](#project-structure)
- [Building from Source](#building-from-source)
- [Troubleshooting](#troubleshooting)
- [Performance & Compatibility](#performance--compatibility)
- [Contributing](#contributing)
- [License](#license)

## Features

The OSC plugin provides a complete OSC implementation including:

- **OSC Message Sending**: Send OSC messages to external applications with support for multiple data types
- **OSC Message Receiving**: Listen for incoming OSC messages and handle them with custom callbacks
- **Message Routing**: Register handlers for specific OSC address patterns using wildcards
- **Thread-Safe Operations**: All operations are safely handled across threads with proper synchronization
- **Auto-Configuration**: Automatic setup with sensible defaults for immediate use
- **Pattern Matching**: Support for OSC address pattern matching with wildcards (*)
- **Multiple Data Types**: Support for strings, integers, floats, and other OSC data types
- **Real-time Performance**: Optimized for low-latency real-time applications
- **Error Handling**: Comprehensive error handling with logging support

## Installation

### For End Users

1. **Install MelonLoader** (if not already installed):
   - Follow the [MelonLoader Installation Guide](https://melonwiki.xyz/#/README?id=installation)
   - Ensure you have .NET Framework 4.7.2 or later installed

2. **Download the OSC Plugin**:
   - Download the latest `OSC.dll` from the [Releases page](https://github.com/ExilProductions/OSC/releases)
   - Place `OSC.dll` in your game's `Mods` folder (usually `Game_Data/Managed/Mods/`)

3. **Launch your game**:
   - The OSC plugin will automatically initialize
   - Check the MelonLoader console for "OSC Initialized" message

### For Mod Developers

To use this OSC plugin as a dependency in your mod:

1. **Add Reference**: Add a reference to the OSC plugin in your mod project
2. **Set Load Priority**: Ensure the OSC plugin loads before your mod by using `MelonPriority` or dependency attributes
3. **Import Namespace**: Add `using OSC;` to your mod files

```csharp
[assembly: MelonInfo(typeof(YourMod), "YourModName", "1.0.0", "YourName")]
[assembly: MelonPriority(-1)] // Load after OSC plugin
```

## Quick Start

Here's a minimal example to get you started:

```csharp
using OSC;
using MelonLoader;

public class YourMod : MelonMod
{
    public override void OnInitializeMelon()
    {
        // Check if OSC is available
        if (OSCAPI.IsAvailable)
        {
            // Send a simple message
            OSCAPI.Send("/test/hello", "world");
            
            // Register a message handler
            OSCAPI.RegisterHandler("/input/*", OnInputReceived);
            
            MelonLogger.Msg("OSC integration ready!");
        }
    }
    
    private void OnInputReceived(OSCMessage message)
    {
        MelonLogger.Msg($"Received: {message.Address}");
    }
}
```

## Usage for Other Mods

### API Overview

The `OSCAPI` class provides a simple, static interface for other mods to interact with the OSC system. All methods include comprehensive error handling and will log issues without crashing your mod.

### 1. Basic Message Sending

```csharp
using OSC;

// Check if OSC is available before using
if (OSCAPI.IsAvailable)
{
    // Send a simple OSC message with one parameter
    OSCAPI.Send("/avatar/parameter/MyParameter", 1.0f);
    
    // Send a message with multiple arguments of different types
    OSCAPI.Send("/game/player/position", 10.5f, 25.0f, 5.2f);
    
    // Send string messages
    OSCAPI.Send("/chat/message", "Hello from my mod!");
    
    // Send boolean values (sent as integers: 1 for true, 0 for false)
    OSCAPI.Send("/game/settings/enabled", true);
}
```

### 2. Receiving Messages

#### Handler Registration

```csharp
// Register a handler for specific address patterns
OSCAPI.RegisterHandler("/input/button/*", OnButtonInput);

// Register handlers for exact addresses
OSCAPI.RegisterHandler("/game/reset", OnGameReset);

// Register a global handler for all messages (useful for debugging)
OSCAPI.RegisterGlobalHandler(OnAnyMessageReceived);

// Handler methods
private void OnButtonInput(OSCMessage message)
{
    MelonLogger.Msg($"Button input: {message.Address}");
    
    // Access message arguments
    if (message.Arguments.Count > 0)
    {
        var value = message.Arguments[0];
        MelonLogger.Msg($"Button value: {value}");
    }
}

private void OnGameReset(OSCMessage message)
{
    MelonLogger.Msg("Game reset requested via OSC");
    // Implement your reset logic here
}

private void OnAnyMessageReceived(OSCMessage message)
{
    MelonLogger.Msg($"OSC: {message.Address} with {message.Arguments.Count} args");
}
```

#### Handler Cleanup

```csharp
public override void OnApplicationQuit()
{
    // Clean up handlers when your mod unloads (important for proper cleanup)
    OSCAPI.UnregisterHandler("/input/button/*", OnButtonInput);
    OSCAPI.UnregisterGlobalHandler(OnAnyMessageReceived);
}
```

## Configuration

### 3. Network Configuration

```csharp
// Configure where to send OSC messages (default: 127.0.0.1:9000)
OSCAPI.ConfigureSender("192.168.1.100", 9000);

// Configure which port to listen on (default: 9001)
OSCAPI.ConfigureReceiver(9001);

// Control message receiving
OSCAPI.StartReceiving();  // Start listening for messages
OSCAPI.StopReceiving();   // Stop listening (saves resources)
```

### 4. Advanced Usage

```csharp
// Create custom OSC messages with precise control
var message = new OSCMessage("/custom/address");
message.AddArgument("Hello");      // String
message.AddArgument(42);           // Integer
message.AddArgument(3.14f);        // Float
message.AddArgument(true);         // Boolean (sent as int)
OSCAPI.Send(message);

// Get OSC system statistics for monitoring
var stats = OSCAPI.GetStats();
MelonLogger.Msg($"Sender connected: {stats.IsSenderConnected}");
MelonLogger.Msg($"Receiver listening: {stats.IsReceiverListening}");
MelonLogger.Msg($"Send target: {stats.SendIP}:{stats.SendPort}");
MelonLogger.Msg($"Receive port: {stats.ReceivePort}");
MelonLogger.Msg($"Registered handlers: {stats.RegisteredHandlers}");
```

### Default Configuration

The OSC plugin starts with these default settings:

- **Send Target**: `127.0.0.1:9000` (localhost)
- **Receive Port**: `9001`
- **Auto-start Receiver**: `true`
- **Thread Pool**: Automatic thread management for optimal performance

### Configuration Best Practices

1. **Check Availability**: Always check `OSCAPI.IsAvailable` before using OSC functions
2. **Error Handling**: OSC operations are wrapped in try-catch blocks, but check logs for issues
3. **Port Conflicts**: Ensure your receive port doesn't conflict with other applications
4. **Network Security**: Be cautious when exposing OSC to external networks
5. **Handler Cleanup**: Always unregister handlers when your mod shuts down

## Examples

### Example 1: VRChat-style Avatar Parameters

```csharp
public class AvatarOSC : MelonMod
{
    private float happiness = 0.5f;
    private bool isWaving = false;
    
    public override void OnInitializeMelon()
    {
        if (OSCAPI.IsAvailable)
        {
            // Listen for avatar parameter changes
            OSCAPI.RegisterHandler("/avatar/parameters/*", OnAvatarParameter);
            
            // Configure to send to VRChat's default OSC port
            OSCAPI.ConfigureSender("127.0.0.1", 9000);
        }
    }
    
    public override void OnUpdate()
    {
        // Send avatar parameters every frame
        if (OSCAPI.IsAvailable)
        {
            OSCAPI.Send("/avatar/parameters/Happiness", happiness);
            OSCAPI.Send("/avatar/parameters/IsWaving", isWaving);
        }
    }
    
    private void OnAvatarParameter(OSCMessage message)
    {
        string paramName = message.Address.Replace("/avatar/parameters/", "");
        if (message.Arguments.Count > 0)
        {
            MelonLogger.Msg($"Avatar parameter '{paramName}' set to: {message.Arguments[0]}");
        }
    }
}
```

### Example 2: TouchOSC Integration

```csharp
public class TouchOSCController : MelonMod
{
    public override void OnInitializeMelon()
    {
        if (OSCAPI.IsAvailable)
        {
            // Set up to receive from TouchOSC (typically on mobile device)
            OSCAPI.ConfigureReceiver(8000);
            OSCAPI.ConfigureSender("192.168.1.50", 9000); // Mobile device IP
            
            // Register handlers for TouchOSC controls
            OSCAPI.RegisterHandler("/1/fader*", OnFaderChange);
            OSCAPI.RegisterHandler("/1/push*", OnButtonPress);
            OSCAPI.RegisterHandler("/1/xy*", OnXYPad);
            
            OSCAPI.StartReceiving();
        }
    }
    
    private void OnFaderChange(OSCMessage message)
    {
        float value = (float)message.Arguments[0];
        MelonLogger.Msg($"Fader {message.Address} moved to: {value:F2}");
        
        // Map fader to game parameters
        if (message.Address.Contains("fader1"))
        {
            // Control game volume, speed, etc.
        }
    }
    
    private void OnButtonPress(OSCMessage message)
    {
        float pressed = (float)message.Arguments[0];
        if (pressed > 0.5f)
        {
            MelonLogger.Msg($"Button {message.Address} pressed!");
            // Trigger game actions
        }
    }
    
    private void OnXYPad(OSCMessage message)
    {
        float x = (float)message.Arguments[0];
        float y = (float)message.Arguments[1];
        MelonLogger.Msg($"XY Pad: X={x:F2}, Y={y:F2}");
        // Control 2D game parameters
    }
}
```

### Example 3: Audio Software Sync

```csharp
public class AudioSync : MelonMod
{
    public override void OnInitializeMelon()
    {
        if (OSCAPI.IsAvailable)
        {
            // Configure for Ableton Live, Reaper, or other DAW
            OSCAPI.ConfigureSender("127.0.0.1", 11000);
            OSCAPI.RegisterHandler("/live/tempo", OnTempoChange);
            OSCAPI.RegisterHandler("/live/play", OnPlayStateChange);
        }
    }
    
    private void OnTempoChange(OSCMessage message)
    {
        float bpm = (float)message.Arguments[0];
        MelonLogger.Msg($"Audio tempo changed to: {bpm} BPM");
        // Sync game animations to music tempo
    }
    
    private void OnPlayStateChange(OSCMessage message)
    {
        bool isPlaying = (float)message.Arguments[0] > 0.5f;
        MelonLogger.Msg($"Audio playback: {(isPlaying ? "Started" : "Stopped")}");
        // Pause/resume game effects based on audio
    }
    
    // Send game events back to audio software
    public void OnGameEvent(string eventName, float intensity)
    {
        OSCAPI.Send($"/game/events/{eventName}", intensity);
        OSCAPI.Send("/game/intensity", intensity);
    }
}
```

## Project Structure

```
OSC/
├── Core.cs              # Main plugin entry point and MelonLoader integration
├── OSCAPI.cs           # Public API for other mods to use
├── OSCManager.cs       # Central manager handling both sending and receiving
├── OSCMessage.cs       # OSC message data structure and utilities
├── OSCParser.cs        # OSC protocol parsing and serialization
├── OSCSender.cs        # UDP sender implementation
├── OSCReceiver.cs      # UDP receiver implementation with threading
├── OSC.csproj          # Project configuration
├── README.md           # This documentation
└── LICENSE.txt         # MIT license
```

### Architecture Overview

- **Core.cs**: MelonLoader plugin entry point, handles initialization and cleanup
- **OSCAPI.cs**: Static API facade providing simple interface for other mods
- **OSCManager.cs**: Central coordinator managing senders, receivers, and message routing
- **OSCMessage.cs**: Data structure representing OSC messages with type-safe argument handling
- **OSCParser.cs**: Low-level OSC protocol implementation (serialization/deserialization)
- **OSCSender.cs**: UDP client for sending messages to external applications
- **OSCReceiver.cs**: UDP server with background thread for receiving messages

The plugin uses a thread-safe design where receiving happens on background threads, but message processing occurs on the main thread to ensure compatibility with Unity/game engines.

## Building from Source

### Prerequisites

- [.NET Framework 4.7.2 SDK](https://dotnet.microsoft.com/download/dotnet-framework) or later
- [Visual Studio 2019+](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)
- MelonLoader references (included in `References/` folder)

### Build Steps

1. **Clone the repository**:
   ```bash
   git clone https://github.com/ExilProductions/OSC.git
   cd OSC
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

3. **Build the project**:
   ```bash
   dotnet build --configuration Release
   ```

4. **Output**: The compiled `OSC.dll` will be in `bin/Release/net472/`

### Development Setup

For development, you may want to:

1. **Set up automatic copying**: Configure your IDE to automatically copy the built DLL to your game's Mods folder
2. **Enable debugging**: Use the Debug configuration for development builds
3. **Reference setup**: Ensure MelonLoader.dll and other references are properly configured

### Build Configuration

The project includes:
- **XML Documentation**: Automatically generated for IntelliSense
- **Embedded Debug Info**: For better stack traces
- **Unsafe Code**: Enabled for performance-critical operations
- **Multiple Configurations**: Debug and Release builds

## Troubleshooting

### Common Issues

#### OSC Plugin Not Loading
```
Error: Could not load OSC plugin
```
**Solutions:**
- Ensure MelonLoader is properly installed
- Verify .NET Framework 4.7.2+ is installed
- Check that OSC.dll is in the correct Mods folder
- Look for dependency issues in the MelonLoader console

#### Network Connection Issues
```
Error: Failed to send OSC message
```
**Solutions:**
- Check firewall settings - ensure UDP ports are not blocked
- Verify target IP address and port are correct
- Test with localhost (127.0.0.1) first
- Use `OSCAPI.GetStats()` to check connection status

#### Message Handler Not Receiving
```
Handler registered but not receiving messages
```
**Solutions:**
- Ensure `OSCAPI.StartReceiving()` has been called
- Check if the receive port conflicts with other applications
- Verify the OSC address pattern matches what you're sending
- Test with a global handler first: `OSCAPI.RegisterGlobalHandler()`

#### Performance Issues
```
Game stuttering when using OSC
```
**Solutions:**
- Reduce message frequency - avoid sending in every Update()
- Use message batching for multiple parameters
- Check if too many handlers are registered
- Monitor CPU usage of OSC operations

### Pattern Matching

OSC address patterns support wildcards:
- `/input/button/*` matches `/input/button/1`, `/input/button/A`, etc.
- `/game/*/*` matches `/game/player/health`, `/game/world/time`, etc.
- Exact matches: `/specific/address` only matches that exact address

### Debugging Tips

1. **Enable Debug Logging**:
   ```csharp
   OSCAPI.RegisterGlobalHandler((msg) => {
       MelonLogger.Msg($"OSC DEBUG: {msg.Address} - {string.Join(", ", msg.Arguments)}");
   });
   ```

2. **Check System Status**:
   ```csharp
   var stats = OSCAPI.GetStats();
   MelonLogger.Msg($"OSC Status - Send: {stats.IsSenderConnected}, Receive: {stats.IsReceiverListening}");
   ```

3. **Test External Tools**: Use applications like [OSC Data Monitor](http://www.kasperkamperman.com/blog/archives/242) to verify message sending/receiving

### Known Limitations

- **UDP Only**: Currently only supports UDP transport (TCP not implemented)
- **Unity Threading**: Message handlers run on the main thread to ensure Unity compatibility
- **Port Binding**: Only one application can bind to a specific receive port at a time
- **Message Size**: Large messages (>64KB) may be fragmented or dropped by UDP

## Performance & Compatibility

### Performance Characteristics

- **Low Latency**: Optimized for real-time applications with minimal delay
- **Thread-Safe**: All public APIs are thread-safe and can be called from any thread
- **Memory Efficient**: Minimal allocations in the message processing pipeline
- **CPU Impact**: Negligible performance impact when idle, scales with message frequency

### Compatibility

**MelonLoader Versions:**
- Compatible with MelonLoader 0.5.7+
- Tested with MelonLoader 0.6.x series
- .NET Framework 4.7.2+ required

**Game Compatibility:**
- Works with any MelonLoader-compatible game
- Tested with Unity-based games
- No game-specific dependencies

**Operating System:**
- Windows 10/11 (primary target)
- Linux (via Mono - community tested)
- macOS (via Mono - limited testing)

**Network Requirements:**
- UDP networking support
- Firewall may need configuration for external connections
- Router port forwarding required for internet-wide access

### Best Practices

1. **Message Frequency**: Limit high-frequency messages (>30 FPS) to essential data only
2. **Handler Efficiency**: Keep message handlers lightweight and fast
3. **Error Handling**: Always check `OSCAPI.IsAvailable` before using OSC functions
4. **Cleanup**: Unregister handlers in your mod's shutdown code
5. **Network Security**: Be cautious when exposing OSC to external networks

## Contributing

We welcome contributions to improve the OSC plugin! Here's how you can help:

### Getting Started

1. **Fork the repository** on GitHub
2. **Create a feature branch**: `git checkout -b feature/amazing-feature`
3. **Make your changes** following the coding standards below
4. **Test thoroughly** with various OSC applications
5. **Submit a pull request** with a clear description

### Coding Standards

- **C# Conventions**: Follow standard C# naming conventions
- **Documentation**: Add XML documentation for public APIs
- **Error Handling**: Include comprehensive error handling with logging
- **Thread Safety**: Ensure any new code is thread-safe
- **Performance**: Consider performance impact, especially in message processing

### Testing

Before submitting:
1. **Build successfully**: Ensure project builds without errors
2. **Test with real applications**: Verify with TouchOSC, VRChat, or similar
3. **Thread safety**: Test concurrent access scenarios
4. **Memory leaks**: Check for proper resource cleanup

### Areas for Contribution

- **TCP Support**: Implement TCP transport for reliable delivery
- **Bundle Support**: Add OSC bundle message support
- **Configuration UI**: Create in-game configuration interface
- **Documentation**: Improve examples and tutorials
- **Platform Support**: Enhance Linux/macOS compatibility
- **Performance**: Optimize message processing pipeline

### Reporting Issues

When reporting bugs:
1. **MelonLoader Version**: Specify your MelonLoader version
2. **Game Information**: Which game and version you're using
3. **OSC Details**: What OSC application you're connecting to
4. **Reproduction Steps**: Clear steps to reproduce the issue
5. **Logs**: Include relevant MelonLoader console output

## License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.

```
MIT License

Copyright (c) 2025 Simon Seider

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

### Third-Party Licenses

- **MelonLoader**: [Apache License 2.0](https://github.com/LavaGang/MelonLoader/blob/master/LICENSE.md)
- **Newtonsoft.Json**: [MIT License](https://github.com/JamesNK/Newtonsoft.Json/blob/master/LICENSE.md)

---

**Author**: [Exil_S](https://github.com/ExilProductions)  
**Version**: 1.0.0  
**Last Updated**: 2025

For support and questions, please [open an issue](https://github.com/ExilProductions/OSC/issues) on GitHub.