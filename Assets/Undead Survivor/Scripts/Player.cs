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
        //GetAxis를 GetAxisRaw로 변경
        inputVec.x = Input.GetAxisRaw("Horizontal");
        inputVec.y = Input.GetAxisRaw("Vertical");
    }

    //물리 연산 프레임마다 호출되는 생명주기 함수
    private void FixedUpdate()
    {
        //1. 힘을 준다.
        //rigid.AddForce(inputVec);

        //2. 속도 제어
        //rigid.velocity = inputVec;

        //3. 위치 이동
        Vector2 nextVec = inputVec.normalized * speed * Time.fixedDeltaTime;

        //rlgid바디의 위치에서 더해줘야한다.
        rigid.MovePosition(rigid.position+ nextVec);

    }
}
