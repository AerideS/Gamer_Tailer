using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Vector2 inputVec;
    public float speed;

    Rigidbody2D rigid;

    void Awake()
    {
        rigid= GetComponent<Rigidbody2D>();
    }


    void Update()
    {
        //GetAxis�� GetAxisRaw�� ����
        inputVec.x = Input.GetAxisRaw("Horizontal");
        inputVec.y = Input.GetAxisRaw("Vertical");
    }

    //���� ���� �����Ӹ��� ȣ��Ǵ� �����ֱ� �Լ�
    private void FixedUpdate()
    {
        //1. ���� �ش�.
        //rigid.AddForce(inputVec);

        //2. �ӵ� ����
        //rigid.velocity = inputVec;

        //3. ��ġ �̵�
        Vector2 nextVec = inputVec.normalized * speed * Time.fixedDeltaTime;

        //rlgid�ٵ��� ��ġ���� ��������Ѵ�.
        rigid.MovePosition(rigid.position+ nextVec);

    }
}
