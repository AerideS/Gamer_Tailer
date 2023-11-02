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

    void spawn()
    {
        GameObject enemy;
        if (GameManager.instance.stageIndex == 0)
        {
            if (level == 0)
            {
                enemy = GameManager.instance.pool.Get(level);    //SpawnData ¿ŒΩ∫∆Â≈Õ√¢
            }
            else
            {
                enemy = GameManager.instance.pool.Get(level + 2);    //SpawnData ¿ŒΩ∫∆Â≈Õ√¢
            }
            enemy.transform.position = spawnPoint[Random.Range(1, spawnPoint.Length)].position;
            enemy.GetComponent<Enemy_new>().Init(spawnData[level]);
        }
        else
        {
            if (level == 0)
            {
                enemy = GameManager.instance.pool.Get(level+1+GameManager.instance.stageIndex);    //SpawnData ¿ŒΩ∫∆Â≈Õ√¢
            }
            else
            {
                enemy = GameManager.instance.pool.Get(level +2+ GameManager.instance.stageIndex);    //SpawnData ¿ŒΩ∫∆Â≈Õ√¢
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