using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Windows;

public class LoginManager : MonoBehaviour
{
    private static LoginManager instance;
    public static LoginManager Instance
    {
        get
        {
            // �ν��Ͻ��� ������ ����
            if (instance == null)
            {
                instance = FindObjectOfType<LoginManager>();

                // �ν��Ͻ��� ���� ���ٸ� ���� ����
                if (instance == null)
                {
                    GameObject go = new GameObject("ServerConnect");
                    instance = go.AddComponent<LoginManager>();
                }
            }

            return instance;
        }
    }

    [SerializeField] private TMP_InputField _userId;
    [SerializeField] private GameObject _failMessage;

    public void TryLogin()
    {
        Protocol.C_LOGIN login = new Protocol.C_LOGIN();

        if (ulong.TryParse(_userId.text, out ulong value))
        {
            Debug.Log($"��ȯ ����: {value}");
            login.LoginCode = value;
        }
        else
        {
            Debug.LogError("��ȯ ����: ��ȿ�� ���� �ƴ�");
            return;
        }

        PacketManager.Send(login);
    }

    public void FailLogin()
    {
        _failMessage.SetActive(true);
    }

    public void CloseBoard()
    {
        _failMessage.SetActive(false);
    }
}
