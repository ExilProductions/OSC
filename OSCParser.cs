using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace OSC
{
    /// <summary>
    /// Handles encoding and decoding of OSC messages to/from byte arrays
    /// </summary>
    public static class OSCParser
    {
        /// <summary>
        /// Encode an OSC message to a byte array
        /// </summary>
        public static byte[] Encode(OSCMessage message)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                // Write address pattern
                WriteString(writer, message.Address);

                // Build type tag string
                var typeTag = ",";
                foreach (var arg in message.Arguments)
                {
                    typeTag += GetTypeTag(arg);
                }
                WriteString(writer, typeTag);

                // Write arguments
                foreach (var arg in message.Arguments)
                {
                    WriteArgument(writer, arg);
                }

                return stream.ToArray();
            }
        }

        /// <summary>
        /// Decode a byte array to an OSC message
        /// </summary>
        public static OSCMessage Decode(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                var message = new OSCMessage();

                // Read address pattern
                message.Address = ReadString(reader);

                // Read type tag string
                var typeTag = ReadString(reader);
                if (!typeTag.StartsWith(","))
                    throw new InvalidDataException("Invalid OSC type tag");

                // Read arguments based on type tags
                for (int i = 1; i < typeTag.Length; i++)
                {
                    var arg = ReadArgument(reader, typeTag[i]);
                    message.Arguments.Add(arg);
                }

                return message;
            }
        }

        private static void WriteString(BinaryWriter writer, string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            writer.Write(bytes);
            writer.Write((byte)0); // Null terminator

            // Pad to 4-byte boundary
            var padding = 4 - ((bytes.Length + 1) % 4);
            if (padding < 4)
            {
                for (int i = 0; i < padding; i++)
                    writer.Write((byte)0);
            }
        }

        private static string ReadString(BinaryReader reader)
        {
            var bytes = new List<byte>();
            byte b;
            while ((b = reader.ReadByte()) != 0)
            {
                bytes.Add(b);
            }

            // Skip padding to 4-byte boundary
            var totalLength = bytes.Count + 1;
            var padding = 4 - (totalLength % 4);
            if (padding < 4)
            {
                reader.ReadBytes(padding);
            }

            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        private static string GetTypeTag(object arg)
        {
            switch (arg)
            {
                case int _: return "i";
                case float _: return "f";
                case string _: return "s";
                case bool b: return b ? "T" : "F";
                case double _: return "d";
                case long _: return "h";
                case byte[] _: return "b";
                default: return "s"; // Convert unknown types to string
            }
        }

        private static void WriteArgument(BinaryWriter writer, object arg)
        {
            switch (arg)
            {
                case int i:
                    writer.Write(SwapBytes(i));
                    break;
                case float f:
                    writer.Write(SwapBytes(BitConverter.ToInt32(BitConverter.GetBytes(f), 0)));
                    break;
                case string s:
                    WriteString(writer, s);
                    break;
                case bool b:
                    // Boolean values have no data in OSC, just the type tag
                    break;
                case double d:
                    writer.Write(SwapBytes(BitConverter.ToInt64(BitConverter.GetBytes(d), 0)));
                    break;
                case long l:
                    writer.Write(SwapBytes(l));
                    break;
                case byte[] blob:
                    writer.Write(SwapBytes(blob.Length));
                    writer.Write(blob);
                    // Pad to 4-byte boundary
                    var padding = 4 - (blob.Length % 4);
                    if (padding < 4)
                    {
                        for (int i = 0; i < padding; i++)
                            writer.Write((byte)0);
                    }
                    break;
                default:
                    WriteString(writer, arg?.ToString() ?? "null");
                    break;
            }
        }

        private static object ReadArgument(BinaryReader reader, char typeTag)
        {
            switch (typeTag)
            {
                case 'i':
                    return SwapBytes(reader.ReadInt32());
                case 'f':
                    var intBits = SwapBytes(reader.ReadInt32());
                    return BitConverter.ToSingle(BitConverter.GetBytes(intBits), 0);
                case 's':
                    return ReadString(reader);
                case 'T':
                    return true;
                case 'F':
                    return false;
                case 'd':
                    var longBits = SwapBytes(reader.ReadInt64());
                    return BitConverter.ToDouble(BitConverter.GetBytes(longBits), 0);
                case 'h':
                    return SwapBytes(reader.ReadInt64());
                case 'b':
                    var length = SwapBytes(reader.ReadInt32());
                    var blob = reader.ReadBytes(length);
                    // Skip padding
                    var padding = 4 - (length % 4);
                    if (padding < 4)
                        reader.ReadBytes(padding);
                    return blob;
                default:
                    throw new InvalidDataException($"Unknown OSC type tag: {typeTag}");
            }
        }

        // OSC uses big-endian byte order
        private static int SwapBytes(int value)
        {
            return ((value & 0xFF) << 24) |
                   (((value >> 8) & 0xFF) << 16) |
                   (((value >> 16) & 0xFF) << 8) |
                   ((value >> 24) & 0xFF);
        }

        private static long SwapBytes(long value)
        {
            return ((value & 0xFF) << 56) |
                   (((value >> 8) & 0xFF) << 48) |
                   (((value >> 16) & 0xFF) << 40) |
                   (((value >> 24) & 0xFF) << 32) |
                   (((value >> 32) & 0xFF) << 24) |
                   (((value >> 40) & 0xFF) << 16) |
                   (((value >> 48) & 0xFF) << 8) |
                   ((value >> 56) & 0xFF);
        }
    }
}