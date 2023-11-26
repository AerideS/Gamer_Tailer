using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_new : MonoBehaviour
{
    public float speed;
    public float health;
    public float maxHealth;
    public Rigidbody2D target;
    public GameObject me;

    public bool isLive;

    Rigidbody2D rigid;
    Collider2D coll;
    Animator anim;
    RectTransform rect;
    WaitForFixedUpdate wait;


    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        anim = GetComponentInChildren<Animator>();
        rect = GetComponent<RectTransform>();
        wait = new WaitForFixedUpdate();
    }


    private void FixedUpdate()
    {
        if (!GameManager.instance.isLive)
            return;
        if (!isLive || anim.GetCurrentAnimatorStateInfo(0).IsName("Hit"))
            return;

        Vector2 dirVec = target.position - rigid.position;
        Vector2 nextVec = dirVec.normalized * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
        rigid.velocity = Vector2.zero;
    }

    private void LateUpdate()
    {
        if (!isLive)
            return;
        if (target.position.x < rigid.position.x)
            rect.rotation = Quaternion.Euler(0, 0, 0);
        else
            rect.rotation = Quaternion.Euler(0, 180, 0);
    }

    private void OnEnable()
    {
        target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        isLive = true;
        coll.enabled = true;
        rigid.simulated = true;
        anim.SetBool("Dead", false);
        health = maxHealth;
    }

    public void Init(SpawnData data)
    {
        speed = data.speed;
        maxHealth = data.health;
        health = data.health;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Bullet"))
            return;
        health -= collision.GetComponent<Bullet>().damage;
        StartCoroutine(KnockBack_1());
        if (health > 0)
        {
            anim.SetTrigger("Hit");
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Hit);
        }
        else
        {
            isLive = false;
            coll.enabled = false;
            rigid.simulated = false;
            anim.SetBool("Dead", true);
            StartCoroutine(Dead());
            GameManager.instance.kill++;
            GameManager.instance.GetExp();

            if (GameManager.instance.isLive)
                AudioManager.instance.PlaySfx(AudioManager.Sfx.Dead);
        }
    }



    IEnumerator KnockBack_1()
    {   

        yield return null;  // 1프레임 쉬기
        //yield return new WaitForSeconds(2f);    // 2초 쉬기
        //yield return wait;//하나의 물리 프레임을 딜레이 주기
        Vector3 playerPos = GameManager.instance.player.transform.position;
        Vector3 dirVec = rect.position- playerPos;
        rigid.AddForce(dirVec.normalized * 2000);

    }

    IEnumerator Dead()
    {
        yield return new WaitForSeconds(1f);    // 1초 쉬기
        gameObject.SetActive(false);
    }

}
