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
    public float maxGameTime = 1.2f * 10f;
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

    // 평균 레벨을 가져오기 위해
    public LevelUp levelUp;

    [Header("# 학습을 위해 저장할 데이터")]
    public static bool isFirst = true;
    public static float totalEpvLevel;
    public static float totalAliveTime;
    public static int totalHitCount;
    public static int totaltryCount;
    public static string idf;
    public static int stageStaticIndex;
    public static bool isStageFirst = true;

    public int stageIndex;
    public float avgEpvLevel;
    public float avgAliveTime;
    public int avgHitCount;
    public int hitCount;

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
            { "avgHitCount", avgHitCount },
            { "stageIndex", stageIndex },
            {"totaltryCount", totaltryCount },
            {"avgAliveTime", avgAliveTime},
            {"avgEpvLevel", avgEpvLevel }
};
        docRef.SetAsync(dat).ContinueWithOnMainThread(task => {
            // 데이터 카운트 +1
            Debug.Log(string.Format("data_count : {0}", data_count));
        });

        return;
    }

    // 서버통신 함수
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

        instance = this;
        Application.targetFrameRate = 60;

        // 맞은 횟수 초기화
        hitCount = 0;

        if (isFirst)
        {
            // DB 사용자 ID 할당
            idf = makeIdentifier();

            // 스테이지 클리어까지 총 생존 시간의 합 초기화
            totalAliveTime = 0;
            
            // 스테이지 클리어까지 총  장비 레벨의 합 초기화
            totalEpvLevel = 0;

            // 스테이지 클리어까지 총 맞은 횟수 초기화
            totalHitCount = 0;

            // 스테이지 클리어까지 총 시도 횟수 초기화
            totaltryCount = 1;
        }

        if(isStageFirst)
        {
            stageStaticIndex = 0;
            isStageFirst = false;
        }

        // 서버 연결
        ConnectToTcpServer();
    }

    public void GameStart(int id)
    {
        if (stageStaticIndex == 0)
        {
            playerId = id;
            maxHealth = 100;
            health = maxHealth;

            gameTime = 0;
            isLive = true;

            player.gameObject.SetActive(true);
            uiLevelUp.Select(playerId % 2);    //임시 스크립트 (첫번째 캐릭터 선택)

            Resume();

            AudioManager.instance.PlayBgm(true);
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
        }
        else
        {
            playerId = id;
            maxHealth = 100;
            health = maxHealth;

            gameTime = 0;
            isLive = true;

            stageIndex = stageStaticIndex;
            Stages[stageIndex].SetActive(true);
            player.gameObject.SetActive(true);

            uiLevelUp.Select(playerId % 2);    //임시 스크립트 (첫번째 캐릭터 선택)

            enemyCleaner.gameObject.SetActive(false);
            gameTime = 0;
            maxGameTime = Mathf.Pow(1.2f, stageIndex+1) * maxGameTime;
            player.transform.position = Vector3.zero;

            Resume();
            AudioManager.instance.PlayBgm(true);
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
        }
    }

    public void GameOver()
    {
        // 이제부터는 다회차
        isFirst = false;

        // 총 시도 횟수 더하기
        totaltryCount++;
        // 총 생존 시간 더하기
        totalAliveTime += gameTime;

        // 장비 평균레벨 구하기
        foreach (Item item in levelUp.items)
        {
            totalEpvLevel += item.level;
        }

        // 총 피격 횟수 구하기
        totalHitCount += hitCount;

        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        isLive = false;
        yield return new WaitForSeconds(0.1f);
        GameRetry();


        /*uiResult.gameObject.SetActive(true);
        uiResult.Lose();
        Stop();

        AudioManager.instance.PlayBgm(false);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Lose);*/
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
        // 총 시도 횟수 더하기
        totaltryCount++;
        // 총 생존 시간 더하기
        totalAliveTime += gameTime;

        // 장비 평균레벨 구하기
        foreach (Item item in levelUp.items)
        {
            totalEpvLevel += item.level;
        }

        // 총 피격 횟수 더하기
        totalHitCount += hitCount;

        // 스테이지 클리어까지 평균 생존 시간
        avgAliveTime = totalAliveTime / totaltryCount;
        // 스테이지 클리어까지 평균 장비 레벨
        avgEpvLevel = totalEpvLevel / levelUp.items.Length / totaltryCount;
        // 스테이지 클리어까지 평균 공격받은 횟수
        avgHitCount = totalHitCount / totaltryCount;

        writeData(idf, stageIndex);

        //스테이지 변경
        if (stageIndex < Stages.Length)
        {
            Stages[stageIndex].SetActive(false);
            stageIndex++; stageStaticIndex++;
            Stages[stageIndex].SetActive(true);
            uiResult.gameObject.SetActive(false);
            player.gameObject.SetActive(true);
            enemyCleaner.gameObject.SetActive(false);
            gameTime = 0;
            maxGameTime = 1.2f * maxGameTime;
            player.transform.position = Vector3.zero;

            Resume();
            AudioManager.instance.PlayBgm(true);
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
        }
    }


}