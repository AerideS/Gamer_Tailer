using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{

    public Transform[] spawnPoint;
    public SpawnData[] spawnData;
    public float levelTime;

    int level;
    float timer;

    private void Awake()
    {
        spawnPoint=GetComponentsInChildren<Transform>();
        levelTime = GameManager.instance.maxGameTime / spawnData.Length;
    }

    void Update()
    {
        if (!GameManager.instance.isLive)
            return;
        timer += Time.deltaTime;
        level = Mathf.Min(Mathf.FloorToInt(GameManager.instance.gameTime / levelTime), spawnData.Length - 1);

        if (timer > (spawnData[level].spawnTime))
        {
            timer = 0;
            spawn();
        }

    }

    void spawn()                        //0번째부터시작 1,2번째는 총알이라 놔두기 
    {
        GameObject enemy;
        if (GameManager.instance.stageIndex == 0)           //1스테이지는 레벨에 따라
        {
            if (level == 0)
            {
                enemy = GameManager.instance.pool.Get(level);    //SpawnData 인스펙터창
            }
            else
            {
                enemy = GameManager.instance.pool.Get(level + 2);    //SpawnData 인스펙터창
            }
            enemy.transform.position = spawnPoint[Random.Range(1, spawnPoint.Length)].position;
            enemy.GetComponent<Enemy_new>().Init(spawnData[level]);
        }
        else
        {                                                           //2스테이지부터는 레벨+스테이지 인덱스번째의 몹부터 소환
            if (level == 0)
            {
                enemy = GameManager.instance.pool.Get(level+2+GameManager.instance.stageIndex);    //SpawnData 인스펙터창
            }
            else
            {
                enemy = GameManager.instance.pool.Get(level +2+ GameManager.instance.stageIndex);    //SpawnData 인스펙터창
            }
            enemy.transform.position = spawnPoint[Random.Range(1, spawnPoint.Length)].position;
            enemy.GetComponent<Enemy_new>().Init(spawnData[level]);
        }
    }
       
}

[System.Serializable]
public class SpawnData
{
    public float spawnTime;
    public int spriteType;
    public int health;
    public float speed;
}