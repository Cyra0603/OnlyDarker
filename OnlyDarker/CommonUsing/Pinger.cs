using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OnlyDarker.CommonUsing
{
    public class Pinger
    {
        public int Ping { get; private set; }
        private bool _shouldExit;
        private readonly string IPAddress;

        public Pinger()
        {
            IPAddress = "8.8.8.8";
            var thread = new Thread(CyclePingRemoteServer);
            thread.Start();
        }

        private async void CyclePingRemoteServer()
        {
            while (!_shouldExit)
            {
                try
                {
                    using Ping ping = new();
                    PingReply reply = await ping.SendPingAsync(IPAddress);
                    Ping = (int)reply.RoundtripTime;
                }
                catch (PingException)
                {
                    Ping = 999;
                }
                Thread.Sleep(500);
            }
        }

        public void Stop()
        {
            _shouldExit = true;
        }
    }
}
