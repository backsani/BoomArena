using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using System.Linq;

public class PacketReceiver : MonoBehaviour
{
    private Queue<byte> receiveQueue = new Queue<byte>();
    private Socket clientSocket;

    public void Init(Socket socket)
    {
        clientSocket = socket;
    }

    /// <summary>
    /// �񵿱� ���� ��� �Լ��� �����κ��� �񵿱�� ��Ŷ�� �޴´�.
    /// </summary>
    public void StartReceive()
    {
        SocketAsyncEventArgs receiveEventArgs = new SocketAsyncEventArgs();

        receiveEventArgs.Completed += OnReceiveCompleted;

        byte[] buffer = new byte[1024];
        receiveEventArgs.SetBuffer(buffer, 0, buffer.Length);

        if (!clientSocket.ReceiveAsync(receiveEventArgs))
        {
            OnReceiveCompleted(this, receiveEventArgs);
        }
    }

    /// <summary>
    /// ���� ��Ŷ�� ũ�⸦ �о ũ�⸸ŭ �ڸ� �� receiveQueue�� ����. ���� receiveQueue�� ������� �ʴٸ� ���� �����忡�� ������ ó��
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
    {

        if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
        {
            Debug.Log("���� ��Ŷ ���� : " + BitConverter.ToString(e.Buffer.Take(e.BytesTransferred).ToArray()));

            foreach (var b in e.Buffer.Take(e.BytesTransferred))
            {
                receiveQueue.Enqueue(b);
            }

            while (receiveQueue.Count >= 4)
            {
                byte[] header = new byte[2];
                for (int i = 0; i < 2; i++) header[i] = receiveQueue.ToArray()[i];
                int packetSize = BitConverter.ToInt16(header, 0);

                if (receiveQueue.Count< packetSize)
                {
                    break;
                }

                byte[] packet = new byte[packetSize];
                for (int i = 0; i < packetSize; i++)
                {
                    packet[i] = receiveQueue.Dequeue();
                }

                ServerConnect.Instance.EnqueueRecvData(packet);
            }
        }

        StartReceive();
    }
}