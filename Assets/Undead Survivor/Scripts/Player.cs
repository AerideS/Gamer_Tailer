using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Vector2 inputVec;
    public float speed;
    public Scanner scanner;
    public Hand[] hands;
    public RuntimeAnimatorController[] animCon;

    // GameManager
    public GameManager gameManager;

    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Animator anim;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        scanner = GetComponent<Scanner>();
        hands = GetComponentsInChildren<Hand>(true);
    }

    public void ResetAnim()
    {
        anim.ResetTrigger("Dead");
        anim.SetTrigger("Stand");
    }

    private void OnEnable()
    {
        speed *= Character.Speed;
        anim.runtimeAnimatorController = animCon[GameManager.instance.playerId % 2];
    }

    /*    void Update()
        {
            //GetAxis를 GetAxisRaw로 변경
            inputVec.x = Input.GetAxisRaw("Horizontal");
            inputVec.y = Input.GetAxisRaw("Vertical");
        }*/

    //물리 연산 프레임마다 호출되는 생명주기 함수
    private void FixedUpdate()
    {

        if (!GameManager.instance.isLive)
            return;
        //1. 힘을 준다.
        //rigid.AddForce(inputVec);

        //2. 속도 제어
        //rigid.velocity = inputVec;

        //3. 위치 이동
        Vector2 nextVec = inputVec.normalized * speed * Time.fixedDeltaTime;

        //rlgid바디의 위치에서 더해줘야한다.
        rigid.MovePosition(rigid.position+ nextVec);

    }

    void OnMove(InputValue value)
    {
        inputVec = value.Get<Vector2>();

    }
    private void LateUpdate()
    {
        //순수한 크기 값 Speed 애니메이터 변수에 넣어주기
        anim.SetFloat("Speed",inputVec.magnitude);

        if (inputVec.x != 0)
        {
            spriter.flipX = inputVec.x < 0;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        gameManager.hitCount++;
        if (!GameManager.instance.isLive)
            return;
        GameManager.instance.health -= Time.deltaTime * 100;

        if (GameManager.instance.health<0)
        {
            for(int index = 2; index < transform.childCount; index++)
            {
                transform.GetChild(index).gameObject.SetActive(false);
            }

            anim.SetTrigger("Dead");
            GameManager.instance.GameOver();
        }

    }

}
