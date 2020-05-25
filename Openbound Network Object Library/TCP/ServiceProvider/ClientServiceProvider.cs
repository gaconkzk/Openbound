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
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;

namespace Openbound_Network_Object_Library.TCP.ServiceProvider
{
    public class ClientServiceProvider
    {
        public ConcurrentQueue<byte[]> RequestList { get; set; }

        protected string serverAddress;
        protected int serverPort;
        protected int producerBufferSize;
        protected int consumerBufferSize;

        protected Action<ClientServiceProvider, string[]> consumerAction;

        protected Thread operationThread;
        protected Thread consumerThread;
        protected Thread producerThread;

        protected NetworkStream stream;

        public ClientServiceProvider(string serverAddress, int serverPort, int producerBufferSize, int consumerBufferSize, Action<ClientServiceProvider, string[]> consumerAction)
        {
            this.serverAddress = serverAddress;
            this.serverPort = serverPort;
            this.producerBufferSize = producerBufferSize;
            this.consumerBufferSize = consumerBufferSize;
            this.consumerAction = consumerAction;

            RequestList = new ConcurrentQueue<byte[]>();
        }

        ~ClientServiceProvider()
        {
            StopOperation();
        }

        public void StartOperation()
        {
            if (operationThread == null || !operationThread.IsAlive)
                operationThread = new Thread(ClientServerOperationThread);

            operationThread.Start();
        }

        public void StopOperation()
        {
            if (operationThread != null) operationThread.Interrupt();
            if (consumerThread != null) consumerThread.Interrupt();
            if (producerThread != null) producerThread.Interrupt();

            if (stream != null) stream.Close();
        }

        void ClientServerOperationThread()
        {
            try
            {
                TcpClient client = new TcpClient();

                client.Connect(serverAddress, serverPort);
                stream = client.GetStream();

                producerThread = new Thread(ProducerThread);
                consumerThread = new Thread(ConsumerThread);

                producerThread.Start();
                consumerThread.Start();
            }
            catch (ThreadInterruptedException) { }
            catch (Exception ex)
            {
                Console.WriteLine($"Message: {ex.Message}");
            }
        }

        void ProducerThread()
        {
            try
            {
                byte[] request;

                while (true)
                {
                    request = RequestList.Peek();
                    if (request == default) { Thread.Sleep(100); continue; }
                    Array.Resize(ref request, producerBufferSize);
                    RequestList.Dequeue();
                    stream.Write(request, 0, producerBufferSize);
                }
            }
            catch (ThreadInterruptedException) { }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        void ConsumerThread()
        {
            try
            {
                while (true)
                {
                    int received = 0;
                    byte[] response = new byte[consumerBufferSize];

                    while (!stream.DataAvailable)
                        Thread.Sleep(10);

                    while (received < consumerBufferSize)
                        received += stream.Read(response, received, consumerBufferSize - received);

                    string[] message = ObjectWrapper.ConvertByteArrayToObject<string>(response).Split('|');
                    consumerAction(this, message);
                }
            }
            //catch (ThreadInterruptedException) { }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
