using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Firebase.Firestore 사용
using Firebase.Firestore;
using Firebase.Extensions;


public class GameManager : MonoBehaviour
{
    FirebaseFirestore db;

    public static GameManager instance;
    [Header("# Game Control")]
    public bool isLive;
    public float gameTime;
    public float maxGameTime = 2 * 10f;
    public GameObject[] Stages;
    public int stageIndex;

    [Header("# Player Info")]
    public int playerId;
    public float health;
    public float maxHealth = 100;
    public int level;
    public int kill;
    public int exp;
    public int[] nextExp = { 10, 30, 60, 100, 150, 210, 280, 360, 450, 600 };

    [Header("# Game Object")]
    public Player player;
    public PoolManager pool;
    public LevelUp uiLevelUp;
    public Transform uiJoy;
    public Result uiResult;
    public GameObject enemyCleaner;
   

    void Awake()
    {
        instance = this;
        Application.targetFrameRate = 60;

        // DB 초기화
        db = FirebaseFirestore.DefaultInstance;

        // DB 쓰기 시험
        DocumentReference docRef = db.Collection("data").Document("2");
        Dictionary<string, object> dat = new Dictionary<string, object>
{
        { "health", maxHealth },
        { "playerId", playerId },
};
        docRef.SetAsync(dat).ContinueWithOnMainThread(task => {
            Debug.Log("Added data to the alovelace document in the users collection.");
        });
    }

    public void GameStart(int id)
    {
        playerId = id;
        maxHealth = 100;
        health = maxHealth;

        player.gameObject.SetActive(true);
        uiLevelUp.Select(playerId % 2);    //임시 스크립트 (첫번째 캐릭터 선택)

        Resume();

        AudioManager.instance.PlayBgm(true);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
    }

    public void GameOver()
    {
        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        isLive = false;

        yield return new WaitForSeconds(0.3f);

        uiResult.gameObject.SetActive(true);
        uiResult.Lose();
        Stop();

        AudioManager.instance.PlayBgm(false);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Lose);
    }

    public void GameVictory()
    {
        StartCoroutine(GameVictoryRoutine());

    }

    IEnumerator GameVictoryRoutine()
    {
        isLive = false;
        enemyCleaner.SetActive(true);
        

        yield return new WaitForSeconds(0.3f);

        uiResult.gameObject. SetActive(true);
        uiResult.Win();
        Stop();

        AudioManager.instance.PlayBgm(false);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Win);
    }

    public void GameRetry()
    {
        SceneManager.LoadScene(0);
    }

    public void GameQuit()
    {
        Application.Quit();
    }


    void Update()
    {
        if (!isLive)
            return;

        gameTime += Time.deltaTime;

        if (gameTime > maxGameTime)
        {
            gameTime = maxGameTime;
            GameVictory();
        }
    }

    public void GetExp()
    {
        if (!isLive)
            return ;

        exp++;
        if (exp == nextExp[Mathf.Min(level,nextExp.Length-1)])
        {
            level++;
            exp = 0;
            uiLevelUp.show();
        }
    }

    public void Stop()
    {
        isLive = false;
        Time.timeScale = 0;
        uiJoy.localScale = Vector3.zero;
    }
    public void Resume()
    {
        isLive = true;
        Time.timeScale = 1;
        uiJoy.localScale = Vector3.one;
    }

    public void NextStage()
    {
        //스테이지 변경
        if(stageIndex< Stages.Length)
        {
            Stages[stageIndex].SetActive(false);
            stageIndex++;
            Stages[stageIndex].SetActive(true);
            uiResult.gameObject.SetActive(false);
            player.gameObject.SetActive(true);
            enemyCleaner.gameObject.SetActive(false);
            gameTime = 0;
            maxGameTime = stageIndex * 2*maxGameTime;
            player.transform.position = Vector3.zero;


            Resume();
            AudioManager.instance.PlayBgm(true);
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
        }


    }


}
