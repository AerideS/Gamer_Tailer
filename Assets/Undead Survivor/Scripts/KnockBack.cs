using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class KnockBack : MonoBehaviour
{
    //void OnTriggerEnter2D(Collider2D collision)
    //{
    // ;

    //    //StartCoroutine("KnockBack");
    //    //StartCoroutine(KnockBack());

    //    if (health > 0)
    //    {
    //        anim.SetTrigger("Hit");
    //        AudioManager.instance.PlaySfx(AudioManager.Sfx.Hit);
    //    }
    //    else
    //    {
    //        isLive = false;
    //        coll.enabled = false;
    //        rigid.simulated = false;
    //        anim.SetBool("Dead", true);
    //        StartCoroutine(Dead());
    //        GameManager.instance.kill++;
    //        GameManager.instance.GetExp();

    //        if (GameManager.instance.isLive)
    //            AudioManager.instance.PlaySfx(AudioManager.Sfx.Dead);
    //    }
    //}
    //IEnumerator knockBack()
    //{

    //    //yield return null;  // 1프레임 쉬기
    //    //yield return new WaitForSeconds(2f);    // 2초 쉬기
    //    yield return wait;//하나의 물리 프레임을 딜레이 주기
    //    Vector3 playerPos = GameManager.instance.player.transform.position;
    //    Vector3 dirVec = transform.position - playerPos;
    //    rigid.AddForce(dirVec.normalized * 3, ForceMode2D.Impulse);

    //}
}
