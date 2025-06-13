using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletUnit : Unit
{
    protected override void Start()
    {
        base.Start();
        speed = 0.3f / 0.025f;
    }

    // Update is called once per frame
    void Update()
    {
        float elapsedSec = (UnitManager.Instance.serverNow - spawnTime) / 1250f;

        // ���� ��ġ ���
        Vector3 predictedPos = spawnPos + direction * speed * elapsedSec;

        transform.position = predictedPos;
    }

    //���� ��ġ�� ���� ����
    public override void Move(GameObjectState state, Vector3 position)
    {
        base.Move(state, position);

        spawnPos = position;
        spawnTime = UnitManager.Instance.serverNow;
    }
}
