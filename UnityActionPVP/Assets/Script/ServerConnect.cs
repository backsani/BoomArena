using Google.Protobuf;
using Protocol;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

enum PK_Data
{
    MESSAGE,
    MOVE,
    ATTACK,
    STATE
}

public class ClientInfo
{
    public float hp;
    public float rotation;
    public int sessionIndex;
    public UnityEngine.Vector3 mTransform;
    public string mName;
}

public class ServerConnect : MonoBehaviour
{

    private static ServerConnect instance;

    public static ServerConnect Instance
    {
        get
        {
            // �ν��Ͻ��� ������ ����
            if (instance == null)
            {
                instance = FindObjectOfType<ServerConnect>();

                // �ν��Ͻ��� ���� ���ٸ� ���� ����
                if (instance == null)
                {
                    GameObject go = new GameObject("ServerConnect");
                    instance = go.AddComponent<ServerConnect>();
                }
            }

            return instance;
        }
    }

    // �����ڿ� �ʱ�ȭ
    private void Awake()
    {
        // �̱��� �ν��Ͻ��� �ٸ� �ν��Ͻ��� �浹�ϸ� ���� ��ü�� �ı�
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            // ���� ��ȯ�ص� �̱����� �����Ϸ��� DontDestroyOnLoad ���
            DontDestroyOnLoad(gameObject);
        }
    }

    // �۽� �� ���� ť ����
    private static Queue<byte[]> sendQueue = new Queue<byte[]>();
    private static Queue<byte[]> recvQueue = new Queue<byte[]>();

    //ClientInfo
    public List<ClientInfo> clientInfo = new List<ClientInfo>();
    public uint playerObjectId;

    private PacketReceiver packetReceiver;

    private string _userId;
    public string UserId { get { return _userId; } 
        set { 
            if(value.Length > IdLength)
            {
                Debug.Log("���̵� �ʹ� ���.(10��)");
            }
            else
            {
                _userId = value;
            }
                 } }
    public int IdLength = 10;

    //��Ʈ��ũ ���� ����
    private TcpClient socketConnection;
    private NetworkStream stream;

    private Socket clientSocket;
    private IPEndPoint socketAdress;
    private AsyncSocketClient asyncSocketClient;

    //��Ŷ ���� ����

    private Buffer_Converter bufferCon = new Buffer_Converter();

    [SerializeField]
    private Text message;

    //room ���� �Լ�
    public Action<int, uint> showRoomInfoAction;
    public int currentRoomCode;

    //������ Unit�� ����
    public Queue<Action> spawnQueue = new Queue<Action>();

    void Start()
    {
        asyncSocketClient = new AsyncSocketClient();
        clientInfo.Add(new ClientInfo());
        clientInfo.Add(new ClientInfo());

        ConnectToTcpServer();

        packetReceiver = new PacketReceiver();
        packetReceiver.Init(clientSocket);

        UserId = "Unconnected User";
        currentRoomCode = -1;

        packetReceiver.StartReceive();
    }

    private void ConnectToTcpServer()
    {
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        //socketConnection = new TcpClient("127.0.0.1", 9000);
        socketAdress = new IPEndPoint(IPAddress.Parse("192.168.0.238"), 7777);
        asyncSocketClient.ConnectToServer(clientSocket, socketAdress);
        Debug.Log("Connected to server");
        
        
    }

    void StartSend(byte[] message)
    {
        SocketAsyncEventArgs sendEventArgs = new SocketAsyncEventArgs();

        sendEventArgs.Completed += OnsendCompleted;

        sendEventArgs.SetBuffer(message, 0, message.Length);

        if(!clientSocket.SendAsync(sendEventArgs))
        {
            OnsendCompleted(this, sendEventArgs);
        }
    }

    private void OnsendCompleted(object sender, SocketAsyncEventArgs e) 
    {
        // ���� �۾� ���� ���� Ȯ��
        if (e.SocketError == SocketError.Success)
        {
            Debug.Log("�޽��� ���� �Ϸ�!");
        }
        else
        {
            // ���� ���� �� ���� �޽��� ���
            Debug.LogError($"�޽��� ���� ����: {e.SocketError}");
        }
    }

    

    void Update()
    {
        if (sendQueue.Count > 0)
        {
            StartSend(sendQueue.Dequeue());
        }
        if (recvQueue.Count > 0)
        {
            PacketManager.RecvPacket(recvQueue.Dequeue());
        }
    }

    /*-----------------------
        Queue(Send,Recv)
    ------------------------*/

    public void EnqueueSendData(byte[] data)
    {
        sendQueue.Enqueue(data);
    }

    public void EnqueueRecvData(byte[] data)
    {
        recvQueue.Enqueue(data);
    }

    /*-----------------------
        Disconnect Server
    ------------------------*/

    public void DisconnectServer()
    {
        try
        {
            if (clientSocket.Connected)
            {
                clientSocket.Shutdown(SocketShutdown.Both);
            }

            clientSocket.Close();
            Debug.Log("������ ���� ����.");

            Application.Quit();
        }

        catch(Exception ex) 
        {
            Debug.LogError("������ ���� ���� �� ����" + ex.Message);
        }
    }

    private void OnApplicationQuit()
    {
        if (clientSocket != null)
        {
            clientSocket.Close();
        }
    }

    public void SceneChange(string scenceName)
    {
        SceneManager.LoadScene(scenceName);
    }


    public void EnqueueSpawn(UnitCode code, uint id, UnityEngine.Vector3 pos, Quaternion dir, long spawnTime = 0)
    {
        spawnQueue.Enqueue(() => UnitManager.Instance.SpawnUnit(code, id, pos, dir, spawnTime));
    }
}
