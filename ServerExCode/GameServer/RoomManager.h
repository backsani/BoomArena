#pragma once

class Room;
using RoomRef = std::shared_ptr<Room>;

class RoomManager {
public:
    std::unordered_map<int, RoomRef> rooms;

    Set<int> playerIdList;
    int roomIdCount;

public:
    RoomManager();

    static RoomManagerRef GetInstance() {
        static RoomManagerRef instance = MakeShared<RoomManager>();
        return instance;
    }

    /// <summary>
    /// ���� ������ִ� �Լ�
    /// </summary>
    /// <param name="room"> room ��ü </param>
    void AddRoom(RoomRef room);

    /// <summary>
    /// Ư�� �뿡 ���� ������ �������� �Լ�
    /// </summary>
    /// <param name="id"> ���ϴ� roomId </param>
    /// <returns> RoomRef�� ��ȯ. �ش� ���� ������ nullptr </returns>
    RoomRef GetRoom(int id);

    /// <summary>
    /// �����ϴ� ��� room�� update
    /// </summary>
    void Update();
};

