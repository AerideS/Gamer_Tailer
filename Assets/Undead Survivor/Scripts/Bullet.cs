using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    //프리펩 친구들은 변수 초기화를 하는게 좋다
    public float damage;
    public int per;

    Rigidbody2D rigid;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    public void Init(float daamage,int per,Vector3 dir)
    {
        this.damage = daamage;
        this.per = per; 

        if(per > -1)
        {
            rigid.velocity = dir * 15f;
        }

    }
    
    //관통
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!collision.CompareTag("Enemy") || per==-1)
            return;
        per--;
        if (per == -1)
        {
            rigid.velocity = Vector2.zero;
            gameObject.SetActive(false);
        }
    }

}
