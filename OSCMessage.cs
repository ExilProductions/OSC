using System;
using System.Text;
using System.Collections.Generic;

namespace OSC
{
    /// <summary>
    /// Represents an OSC message containing an address pattern and arguments
    /// </summary>
    public class OSCMessage
    {
        public string Address { get; set; }
        public List<object> Arguments { get; set; }
        public DateTime Timestamp { get; set; }

        public OSCMessage()
        {
            Arguments = new List<object>();
            Timestamp = DateTime.Now;
        }

        public OSCMessage(string address, params object[] args) : this()
        {
            Address = address;
            Arguments.AddRange(args);
        }

        /// <summary>
        /// Add an argument to the message
        /// </summary>
        public void AddArgument(object arg)
        {
            Arguments.Add(arg);
        }

        /// <summary>
        /// Get argument at specified index with type casting
        /// </summary>
        public T GetArgument<T>(int index)
        {
            if (index < 0 || index >= Arguments.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            return (T)Arguments[index];
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"OSC Message: {Address}");
            if (Arguments.Count > 0)
            {
                sb.Append(" [");
                for (int i = 0; i < Arguments.Count; i++)
                {
                    if (i > 0) sb.Append(", ");
                    sb.Append(Arguments[i]?.ToString() ?? "null");
                }
                sb.Append("]");
            }
            return sb.ToString();
        }
    }
}