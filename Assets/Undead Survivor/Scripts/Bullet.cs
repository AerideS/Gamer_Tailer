using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    //프리펩 친구들은 변수 초기화를 하는게 좋다
    public float damage;
    public int per;

    public void Init(float daamage,int per)
    {
        this.damage = daamage;
        this.per = per; 
    } 

}
