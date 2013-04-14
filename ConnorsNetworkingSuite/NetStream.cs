using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ConnorsNetworkingSuite {
    public delegate void AsyncReadCallback(NetStream sender, string message);

    public abstract class NetStream {

        /// <summary>
        /// Gets the underlying stream
        /// </summary>
        public virtual Stream UnderlyingStream { get; protected set; }

        /// <summary>
        /// Gets or sets the encoding that should be used for communication
        /// </summary>
        public virtual Encoding Encoding { get; set; }

        /// <summary>
        /// Gets the last exception incurred by a read or write operation
        /// </summary>
        public virtual Exception LastError { get; protected set; }

        /// <summary>
        /// When overridden in a derived class, returns when a new message has
        /// been sent through the underlying stream
        /// </summary>
        /// <returns>The value sent, or null if there was an exception</returns>
        public abstract string Read();

        /// <summary>
        /// When overridden in a derived class, writes the message to the underlying stream
        /// and returns whether it was successful
        /// </summary>
        /// <param name="message">The message to be written to the stream</param>
        /// <returns>true if the write was successful, false if there was an exception</returns>
        public abstract bool Write(string message);

        /// <summary>
        /// When overridden in a derived class, writes the message to the stream
        /// asyncronously
        /// </summary>
        /// <param name="message">The message to be written to the stream</param>
        public abstract void BeginWrite(string message);

        /// <summary>
        /// When overridden in a derived class, begins asyncronous reading and will invoke
        /// the callback each time there is a new message. The message will return as null
        /// when the stream is closed, and no further callbacks will occur.
        /// </summary>
        /// <param name="callback">The method to invoke each time a message is recieved</param>
        public abstract void BeginRead(AsyncReadCallback callback);

        /// <summary>
        /// Stops the BeginRead from returning any more data
        /// </summary>
        public abstract void EndRead();

        public abstract Task<string> ReadAsync();
        public abstract Task<bool> WriteAsync(string message);

        /// <summary>
        /// Closes the underlying stream
        /// </summary>
        public abstract void Close();

        /// <summary>
        /// Gets whether the stream is still open
        /// </summary>
        public abstract bool Connected { get; }
    }
}