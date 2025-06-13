using Google.Protobuf;
using Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;


public enum PacketType
{
    PKT_C_LOGIN = 1000,
    PKT_C_MOVE = 1001,
    PKT_C_ROOM_DATA = 1002,
    PKT_C_ROOM_REQUEST = 1003,
    PKT_C_ATTACK = 1004,
    PKT_C_RTT_PING = 1005,
    PKT_S_RTT_PONG = 1006,
    PKT_S_LOGIN = 1007,
    PKT_S_ROOM_DATA = 1008,
    PKT_S_ROOM_RESPONSE = 1009,
    PKT_S_MOVE = 1010,
    PKT_S_OBJECT_SPAWN = 1011,
    PKT_S_OBJECT_DEAD = 1012,
    PKT_S_OBJECT_DAMAGE = 1013,
}

public static class PacketManager
{
    /// <summary>
    /// PacketType�� �´� �ڵ鷯 �Լ��� O(1)�� �����Ű�� ���� ��ųʸ��� ����. buffer�� �Ѱ��ְ� ��������ָ� IMessage������ ��ȯ���ش�.
    /// </summary>
    private static Dictionary<PacketType, Func<byte[], IMessage>> Handlers = new Dictionary<PacketType, Func<byte[], IMessage>>
    {
        { PacketType.PKT_S_RTT_PONG, (buffer) => PacketMaker<Protocol.S_RTT_PONG>.HandlePacket(buffer, PacketType.PKT_S_RTT_PONG) },

        { PacketType.PKT_S_LOGIN, (buffer) => PacketMaker<Protocol.S_LOGIN>.HandlePacket(buffer, PacketType.PKT_S_LOGIN) },

        { PacketType.PKT_S_ROOM_DATA, (buffer) => PacketMaker<Protocol.S_ROOM_DATA>.HandlePacket(buffer, PacketType.PKT_S_ROOM_DATA) },

        { PacketType.PKT_S_ROOM_RESPONSE, (buffer) => PacketMaker<Protocol.S_ROOM_RESPONSE>.HandlePacket(buffer, PacketType.PKT_S_ROOM_RESPONSE) },

        { PacketType.PKT_S_MOVE, (buffer) => PacketMaker<Protocol.S_MOVE>.HandlePacket(buffer, PacketType.PKT_S_MOVE) },

        { PacketType.PKT_S_OBJECT_SPAWN, (buffer) => PacketMaker<Protocol.S_OBJECT_SPAWN>.HandlePacket(buffer, PacketType.PKT_S_OBJECT_SPAWN) },

        { PacketType.PKT_S_OBJECT_DEAD, (buffer) => PacketMaker<Protocol.S_OBJECT_DEAD>.HandlePacket(buffer, PacketType.PKT_S_OBJECT_DEAD) },

        { PacketType.PKT_S_OBJECT_DAMAGE, (buffer) => PacketMaker<Protocol.S_OBJECT_DAMAGE>.HandlePacket(buffer, PacketType.PKT_S_OBJECT_DAMAGE) },
    };

    // SendXXXX : PKT_C_XXX ��Ŷ�� ������ִ� �Լ���� �Լ��� �������� �̿��� �Ѱܹ��� �Ű������� �ڷ����� ������ ������ �Լ��� �����Ų��. IMessage������ ���� �Ѱ��ָ� �ڵ����� ��Ŷ�� ������ش�.

    /// <summary>
    /// PKT_C_LOGIN ��Ŷ�� ������ִ� �Լ�
    /// </summary>
    public static void Send(Protocol.C_RTT_PING pkt)
    {
        PacketMaker<Protocol.C_RTT_PING>.MakeSendBuffer(pkt, PacketType.PKT_C_RTT_PING);
    }
    public static void Send(Protocol.C_LOGIN pkt)
    {
        PacketMaker<Protocol.C_LOGIN>.MakeSendBuffer(pkt, PacketType.PKT_C_LOGIN);
    }
    public static void Send(Protocol.C_MOVE pkt)
    {
        PacketMaker<Protocol.C_MOVE>.MakeSendBuffer(pkt, PacketType.PKT_C_MOVE);
    }
    public static void Send(Protocol.C_ROOM_DATA pkt)
    {
        PacketMaker<Protocol.C_ROOM_DATA>.MakeSendBuffer(pkt, PacketType.PKT_C_ROOM_DATA);
    }
    public static void Send(Protocol.C_ROOM_REQUEST pkt)
    {
        PacketMaker<Protocol.C_ROOM_REQUEST>.MakeSendBuffer(pkt, PacketType.PKT_C_ROOM_REQUEST);
    }

    public static void Send(Protocol.C_ATTACK pkt)
    {
        PacketMaker<Protocol.C_ATTACK>.MakeSendBuffer(pkt, PacketType.PKT_C_ATTACK);
    }

    /// <summary>
    /// ���� ��Ŷ�� ��� PacketType�� ���� ������ Process �����ϴ� �Լ�
    /// </summary>
    /// <param name="message">������ ���� ���� ��Ŷ</param>
    /// <returns></returns>
    public static bool RecvPacket(byte[] message)
    {
        // PacketType ����
        PacketType type = (PacketType)BitConverter.ToUInt16(message, sizeof(ushort));

        // type�� PacketType�� �����ϴ� ������ �˻�
        if(!Enum.IsDefined(typeof(PacketType), type))
        {
            Debug.Log("Packet is not Definition");
            return false;
        }

        // Handlers(type�� �����ؾߵ� �ڵ鷯 �Լ��� ��Ƶ� ��ųʸ�)���� type�� �´� ���� ã�Ƽ� �Լ� �����Ű��
        if(Handlers.TryGetValue(type, out var handler))
        {
            IMessage packet = handler.Invoke(message);
            Process(type, packet);
        }
        else
        {
            Debug.Log("�������� �ʴ� ��ŶŸ��");
        }

        return true;
    }

    /// <summary>
    /// ���������� ���� �����͸� �ΰ��ӿ� �����ϴ� �Լ�.
    /// �̱����� ServerConnect�� Instance�� ���� �ش� ������Ʈ�鿡 ������ �����͸� �ݿ��Ѵ�.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="packet"></param>
    private static void Process(PacketType type, IMessage packet)
    {
        Debug.Log("���� ��Ŷ�� type : " + type.ToString());
        switch (type)
        {
            case PacketType.PKT_S_RTT_PONG:
                S_RTT_PONG s_RTT_PONG = packet as S_RTT_PONG;

                //��Ŷ�� �޾��� �� �ð�
                long recvTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                //��Ŷ�� ������ ������ ������� �ɸ� �ð�
                long rtt = recvTimeMs - s_RTT_PONG.ClientTime;
                
                //������ ��Ŷ�� ���� �ð��� Ŭ���̾�Ʈ�� ��Ŷ�� ���� �ð��� ����
                long offset = s_RTT_PONG.ServerTime - (s_RTT_PONG.ClientTime + rtt / 2);

                UnitManager.Instance.serverOffset = offset;
                break;

            case PacketType.PKT_S_LOGIN:
                S_LOGIN s_LOGIN = packet as S_LOGIN;
                if (s_LOGIN.LoginAccept)
                {
                    Debug.Log("!!!!!�α��� ����!!!!!");
                    ServerConnect.Instance.UserId = s_LOGIN.GameId.ToString();
                    ServerConnect.Instance.SceneChange("MatchingScene");
                }
                else
                {
                    LoginManager.Instance.FailLogin();
                    Debug.Log("?????�α��� ����?????");
                }
                break;

            case PacketType.PKT_S_ROOM_DATA:
                S_ROOM_DATA s_ROOM_DATA = packet as S_ROOM_DATA;
                
                foreach(Protocol.RoomData data in s_ROOM_DATA.RoomData)
                {
                    ServerConnect.Instance.showRoomInfoAction(data.RoomCode, data.PlayerCount);
                }

                
                break;

            case PacketType.PKT_S_ROOM_RESPONSE:
                S_ROOM_RESPONSE s_ROOM_RESPONSE = packet as S_ROOM_RESPONSE;

                if(s_ROOM_RESPONSE.RoomAccept)
                {
                    ServerConnect.Instance.playerObjectId = s_ROOM_RESPONSE.PlayerObjectId;

                    //UnitManager.Instance;

                    ServerConnect.Instance.SceneChange("BattleScene");

                    ServerConnect.Instance.currentRoomCode = s_ROOM_RESPONSE.RoomCode;

                    foreach (Protocol.ObjectData data in s_ROOM_RESPONSE.ObjectData)
                    {
                        UnityEngine.Vector3 pos = new UnityEngine.Vector3(data.Position.X, data.Position.Y, data.Position.Z);

                        UnityEngine.Vector3 dir = new UnityEngine.Vector3(data.Direction.X, data.Direction.Y, data.Direction.Z);

                        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                        ServerConnect.Instance.EnqueueSpawn((UnitCode)data.Type, data.ObjectId, pos, Quaternion.Euler(0, 0, angle));
                    }
                }
                else
                {
                    Debug.Log("�� ���� �źε�");
                }

                break;

            case PacketType.PKT_S_MOVE:
                S_MOVE s_MOVE = packet as S_MOVE;

                UnitManager unitManager = UnitManager.Instance;
                
                if (unitManager.units.TryGetValue(s_MOVE.ObjectId, out Unit unit))
                {
                    UnityEngine.Vector3 pos = new UnityEngine.Vector3(s_MOVE.Position.X, s_MOVE.Position.Y, s_MOVE.Position.Z);

                    unit.Move((GameObjectState)s_MOVE.State, pos);
                }

                break;

            case PacketType.PKT_S_OBJECT_SPAWN:
                {
                    Debug.Log("BulletSpawn");
                    S_OBJECT_SPAWN s_OBJECT_SPAWN = packet as S_OBJECT_SPAWN;

                    Protocol.ObjectData data = s_OBJECT_SPAWN.ObjectData;

                    UnityEngine.Vector3 pos = new UnityEngine.Vector3(data.Position.X, data.Position.Y, data.Position.Z);

                    Debug.Log("============== " + pos.ToString());

                    UnityEngine.Vector3 dir = new UnityEngine.Vector3(data.Direction.X, data.Direction.Y, data.Direction.Z);

                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                    ServerConnect.Instance.EnqueueSpawn((UnitCode)data.Type, data.ObjectId, pos, Quaternion.Euler(0, 0, angle - 90.0f), s_OBJECT_SPAWN.SpawnTime);

                    break;
                }

            case PacketType.PKT_S_OBJECT_DEAD:
                S_OBJECT_DEAD s_OBJECT_DEAD = packet as S_OBJECT_DEAD;

                if (s_OBJECT_DEAD.State == Protocol.GameObjectState.Dead)
                    UnitManager.Instance.RemoveUnit(s_OBJECT_DEAD.ObjectId);

                break;

            case PacketType.PKT_S_OBJECT_DAMAGE:
                S_OBJECT_DAMAGE s_OBJECT_DAMAGE = packet as S_OBJECT_DAMAGE;

                Unit target = UnitManager.Instance.units[s_OBJECT_DAMAGE.ObjectId];

                if (target is PlayerUnit player)
                {
                    player.SetHp(s_OBJECT_DAMAGE.Hp); 
                }
                else
                {
                    target.SetHp(s_OBJECT_DAMAGE.Hp); 
                }

                break;

            default:
                Debug.Log("-------------�������� �ʴ� ��Ŷ------------");
                break;
        }
    }
}

public class PacketMaker<T> where T : IMessage<T>, new()
{
    // HandlePacket���� ���׸� Ÿ���� T�� ������ȭ ����� �̿��ϱ� ���� MessageParser�� ���� �޸𸮿� ����. �̶� �����ڸ� �����Լ��� ����� �ڷ��� T�� �����ϰ� �̸� MessageParser�� �Ѱ���.
    private static readonly MessageParser<T> parser = new MessageParser<T>(() => new T());

    /// <summary>
    /// SendXXXX �Լ��κ��� ����Ǵ� ���� ��Ŷ�� ����� �Լ�.
    /// PacketType�� ���� �������� ��Ŷ�� Serialize�����ְ� sendQueue�� �־��ش�.
    /// </summary>
    /// <param name="pkt"> ����ȭ ������ ������ </param>
    /// <param name="type"> ����ȭ ��ų PacketType </param>
    public static void MakeSendBuffer(T pkt, PacketType type)
    {
        int size = pkt.CalculateSize();

        Debug.Log($"Serialized size: {size} bytes");

        byte[] payload = pkt.ToByteArray();
        ushort packetId = (ushort)type;
        ushort packetSize = (ushort)(sizeof(ushort) + sizeof(ushort) + payload.Length);

        using (MemoryStream ms = new MemoryStream())
        {
            ms.Write(BitConverter.GetBytes(packetSize), 0, 2);
            ms.Write(BitConverter.GetBytes(packetId), 0, 2);
            ms.Write(payload, 0, payload.Length);

            Debug.Log("���� ��Ŷ ���� : " + BitConverter.ToString(ms.ToArray()));

            ServerConnect.Instance.EnqueueSendData(ms.ToArray());
        }
    }

    /// <summary>
    /// �����κ��� ���� �����͸� IMessage �������� DeSerialize �����ִ� ���� �Լ�.
    /// </summary>
    /// <param name="buffer"> ������ ���� ���� ������ </param>
    /// <param name="type"> �ش� �������� PacketType </param>
    /// <returns> DeSerialize ������ ������ </returns>
    public static T HandlePacket(byte[] buffer, PacketType type)
    {
        ushort size = BitConverter.ToUInt16(buffer, 0);

        // packet���� size�� packetId�� �� ���� Ȯ��
        byte[] payload = new byte[size - 4];
        // payload�� ������ ����
        Array.Copy(buffer, 4, payload, 0, payload.Length);

        T data = parser.ParseFrom(payload);

        return data;
    }
}
