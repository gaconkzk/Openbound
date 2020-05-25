/* 
 * Copyright (C) 2020, Carlos H.M.S. <carlos_judo@hotmail.com>
 * This file is part of OpenBound.
 * OpenBound is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or(at your option) any later version.
 * 
 * OpenBound is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with OpenBound. If not, see http://www.gnu.org/licenses/.
 */

using Openbound_Network_Object_Library.Common;
using Openbound_Network_Object_Library.Extension;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Openbound_Network_Object_Library.TCP.ServiceProvider
{
    public class ServerServiceProvider
    {
        protected int port;
        protected int producerBufferSize;
        protected int consumerBufferSize;

        protected Action<ConcurrentQueue<byte[]>, string[], Dictionary<int, object>> consumerAction;
        protected Action onConnect;
        protected Action<Dictionary<int, object>> onDisconnect;

        protected Thread operationThread;

        public ServerServiceProvider(int port, int producerBufferSize, int consumerBufferSize,
            Action<ConcurrentQueue<byte[]>, string[], Dictionary<int, object>> consumerAction,
            Action onConnect = default, Action<Dictionary<int, object>> onDisconnect = default)
        {
            this.port = port;
            this.producerBufferSize = producerBufferSize;
            this.consumerBufferSize = consumerBufferSize;
            this.consumerAction = consumerAction;

            this.onConnect = onConnect;
            this.onDisconnect = onDisconnect;

            operationThread = new Thread(ServiceRegisterOperationThread);
        }

        ~ServerServiceProvider()
        {
            StopOperation();
        }

        public void StartOperation()
        {
            if (operationThread == null)
                operationThread = new Thread(ServiceRegisterOperationThread);

            operationThread.Start();
        }

        public void StopOperation()
        {
            operationThread.Interrupt();
        }

        void ServiceRegisterOperationThread()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port);

            while (true)
            {
                try
                {
                    listener.Start();

                    while (true)
                    {
                        Socket socket = listener.AcceptSocket();

                        ConcurrentQueue<byte[]> queue = new ConcurrentQueue<byte[]>();
                        Dictionary<int, object> paramDictionary = new Dictionary<int, object>();

                        string producerThreadName = $"{socket.RemoteEndPoint} - Consumer";
                        string consumerThreadName = $"{socket.RemoteEndPoint} - Producer";

                        Thread consumerThread = new Thread(() => ConsumerThread(socket, producerThreadName, queue, paramDictionary));
                        Thread producerThread = new Thread(() => ProducerThread(socket, consumerThreadName, queue, paramDictionary));

                        consumerThread.Name = producerThreadName;
                        producerThread.Name = consumerThreadName;

                        producerThread.Start();
                        consumerThread.Start();
                    }
                }
                catch (Exception ex)
                {
                    listener.Stop();
                    Console.WriteLine($"Message: {ex.Message}");
                }
            }
        }

        void ProducerThread(Socket socket, string threadName, ConcurrentQueue<byte[]> queue, Dictionary<int, object> paramDictionary)
        {
            try
            {
                byte[] request;

                while (socket.Connected)
                {
                    int sent = 0;

                    request = queue.Peek();
                    if (request == default) { Thread.Sleep(100); continue; }
                    Array.Resize(ref request, producerBufferSize);
                    queue.Dequeue();

                    while (sent < producerBufferSize)
                        sent += socket.Send(request, sent, producerBufferSize - sent, SocketFlags.None);
                }
            }
            catch (ThreadInterruptedException) { }
            catch (Exception) { }

            Console.WriteLine($"{threadName} was terminated.");
        }

        protected virtual void ConsumerThread(Socket socket, string threadName, ConcurrentQueue<byte[]> queue, Dictionary<int, object> paramDictionary)
        {
            try
            {
                while (true)
                {
                    byte[] response = new byte[consumerBufferSize];

                    int received = 0;

                    while (received < consumerBufferSize)
                    {
                        received += socket.Receive(response, received, consumerBufferSize - received, SocketFlags.None);

                        /*
                         * If you received 0 bytes, that usually means the sender has closed their send socket. In a Socket, the send and receive channels are separate; I expect what
                         * is happening is that your send (their receive) is still open and available, hence clientSocket.Connected returning true (you can still send them a reply),
                         * but: they closed their send (your receive) as soon as they sent their payload (this is common, to indicate the end of a batch). Basically, you just need to
                         * detect the 0-byte receive, and treat that as the end: no more data will ever be incoming once you have had a non-positive reply from receive. So just write any
                         * response you need to write (they can still be listening, even though they will never speak again), and shutdown the socket.
                         * Source: https://stackoverflow.com/questions/19221199/c-sharp-socket-receive-continuously-receives-0-bytes-and-does-not-block-in-the-l
                         */
                        if (received == 0) break;
                    }

                    Console.WriteLine(ObjectWrapper.ConvertByteArrayToObject<string>(response));

                    string resp = ObjectWrapper.ConvertByteArrayToObject<string>(response);
                    consumerAction(queue, resp?.Split('|'), paramDictionary);
                }
            }
            catch (ThreadInterruptedException) { }
            catch (Exception) { socket.Dispose(); }

            onDisconnect?.Invoke(paramDictionary);

            Console.WriteLine($"{threadName} was terminated.");
        }
    }
}
