using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Threading;

namespace ConnorsNetworkingSuite {
    public abstract class NetStreamHost {
        /// <summary>
        /// Event called when a new client is accepted
        /// (called in its own thread)
        /// </summary>
        public event NewClient NewClient;
        /// <summary>
        /// Starts the listener on the given endpoint.
        /// Blocks until the listener is stopped.
        /// </summary>
        /// <param name="address">The end point to bind to</param>
        public abstract void Start(IPEndPoint address);
        /// <summary>
        /// Starts the listener on the given IPAddress and port.
        /// Blocks until the listener is stopped.
        /// </summary>
        /// <param name="addr">Address</param>
        /// <param name="port">Port</param>
        public void Start(IPAddress addr, int port) {
            Start(new IPEndPoint(addr, port));
        }
        /// <summary>
        /// Starts the listener on the given port bound
        /// to IPAddress.Any.
        /// Blocks until the listener is stopped.
        /// </summary>
        /// <param name="port">Port to listen on</param>
        public void Start(int port) {
            Start(new IPEndPoint(IPAddress.Any, port));
        }
        /// <summary>
        /// Starts the listener in a thread bound to the given
        /// IP Endpoint. Returns on thread creation.
        /// </summary>
        /// <param name="address">Endpoint</param>
        public virtual void StartInThread(IPEndPoint address) {
            new Thread(() => Start(address)).Start();
        }
        /// <summary>
        /// Starts the listener in a thread bound to the given
        /// IP Address and port. Returns on thread creation.
        /// </summary>
        /// <param name="addr">IP Address to bind to</param>
        /// <param name="port">Port to listen on</param>
        public void StartInThread(IPAddress addr, int port) {
            StartInThread(new IPEndPoint(addr, port));
        }
        /// <summary>
        /// Starts the listener in a thread bound to
        /// IPAddress.Any and listens on the given
        /// port. Returns on thread creation.
        /// </summary>
        /// <param name="port"></param>
        public void StartInThread(int port) {
            StartInThread(IPAddress.Any, port);
        }
        /// <summary>
        /// Stops the stream listener
        /// </summary>
        public abstract void Stop();
        protected void RaiseNewClient(NetStream client, IPEndPoint ip) {
            new Thread(() => {
                           try {
                               NewClient(client, ip);
                           } catch (Exception e) {
                               Debug.WriteLine("First chance exception occured in " +
                                   Assembly.GetAssembly(GetType()).GetName().FullName +
                                   " while trying to call NetStreamHost.NewClient. Details below.\r\n" + e);
                           }
                       }).Start();
        }
    }

    public delegate void NewClient(NetStream client, IPEndPoint Ip);
}