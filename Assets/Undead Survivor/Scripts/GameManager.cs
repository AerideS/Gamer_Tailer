using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Firebase.Firestore 사용
using Firebase.Firestore;
using Firebase.Extensions;
using System.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;

// 서버 사용
using System.Net;
using System.Net.Sockets;
using System.Threading;



public class GameManager : MonoBehaviour
{
    // DB 인스턴스, 자료의 개수
    FirebaseFirestore db;
    int data_count;

    // TcpClient Socket
    private TcpClient socketConnection;

    public static GameManager instance;
    [Header("# Game Control")]
    public bool isLive;
    public float gameTime;
    public float maxGameTime = 2 * 10f;
    public GameObject[] Stages;

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

    [Header("# 학습을 위해 저장할 데이터")]
    public string id;
    public float avgEpvLevel;
    public float totalAliveTime;
    public float avgAliveTime;
    public int hitTime;
    public static int tryCount = 1;
    public int stageIndex;

    string makeIdentifier()
    {
        // 식별자 생성
        int length = 8;
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        System.Random random = new System.Random();
        char[] randomChars = new char[length];

        for (int i = 0; i < length; i++)
        {
            randomChars[i] = chars[random.Next(chars.Length)];
        }
        return new string(randomChars);
    }

    // DB 쓰기 함수
    void writeData(string randomChars, int stageIndex)
    {
        // DB 초기화
        db = FirebaseFirestore.DefaultInstance;
        // DB 쓰기
        DocumentReference docRef = db
            .Collection(("data")).Document(randomChars)
            .Collection(randomChars).Document(stageIndex.ToString());
        Dictionary<string, object> dat = new Dictionary<string, object>
{
        { "hitCount", player.hitCount },
        { "stageIndex", stageIndex },
        {"tryCount", tryCount },
        {"avgAliveTime", avgAliveTime}
};
        docRef.SetAsync(dat).ContinueWithOnMainThread(task => {
            // 데이터 카운트 +1
            Debug.Log(string.Format("data_count : {0}", data_count));
        });

        return;
    }

    private void ConnectToTcpServer()
    {
        try
        {
            socketConnection = new TcpClient("127.0.0.1", 8080);    
        }
        catch(Exception e)
        {
            Debug.Log("On client connect exception " + e);
        }
    }

    void Awake()
    {
        // DB 사용자 ID
        id = makeIdentifier();
        instance = this;
        Application.targetFrameRate = 60;

        // 서버 연결
        ConnectToTcpServer();
    }

    public void GameStart(int id)
    {
        playerId = id;
        maxHealth = 100;
        health = maxHealth;
        totalAliveTime = 0;

        player.gameObject.SetActive(true);
        uiLevelUp.Select(playerId % 2);    //임시 스크립트 (첫번째 캐릭터 선택)

        Resume();

        AudioManager.instance.PlayBgm(true);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
    }

    public void GameOver()
    {
        tryCount++;
        totalAliveTime += gameTime;
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

        uiResult.gameObject.SetActive(true);
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
            return;

        exp++;
        if (exp == nextExp[Mathf.Min(level, nextExp.Length - 1)])
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
        // titalAliveTime, avgAliveTime 초기화 후 데이터 쓰기
        totalAliveTime = gameTime;
        avgAliveTime = totalAliveTime / tryCount;
        writeData(id, stageIndex);
        //스테이지 변경
        if (stageIndex < Stages.Length)
        {
            Stages[stageIndex].SetActive(false);
            stageIndex++;
            Stages[stageIndex].SetActive(true);
            uiResult.gameObject.SetActive(false);
            player.gameObject.SetActive(true);
            enemyCleaner.gameObject.SetActive(false);
            gameTime = 0;
            maxGameTime = stageIndex * 2 * maxGameTime;
            player.transform.position = Vector3.zero;


            Resume();
            AudioManager.instance.PlayBgm(true);
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
        }


    }


}