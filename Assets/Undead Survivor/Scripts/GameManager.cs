using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Firebase.Firestore ���
using Firebase.Firestore;
using Firebase.Extensions;
using System.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;

// ���� ���
using System.Net;
using System.Net.Sockets;
using System.Threading;



public class GameManager : MonoBehaviour
{
    // DB �ν��Ͻ�, �ڷ��� ����
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

    // ��� ������ �������� ����
    public LevelUp levelUp;

    [Header("# �н��� ���� ������ ������")]
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
        // �ĺ��� ����
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

    // DB ���� �Լ�
    void writeData(string randomChars, int stageIndex)
    {
        // DB �ʱ�ȭ
        db = FirebaseFirestore.DefaultInstance;
        // DB ����
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
            // ������ ī��Ʈ +1
            Debug.Log(string.Format("data_count : {0}", data_count));
        });

        return;
    }

    // ������� �Լ�
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

        // ���� Ƚ�� �ʱ�ȭ
        hitCount = 0;

        if (isFirst)
        {
            // DB ����� ID �Ҵ�
            idf = makeIdentifier();

            // �������� Ŭ������� �� ���� �ð��� �� �ʱ�ȭ
            totalAliveTime = 0;
            
            // �������� Ŭ������� ��  ��� ������ �� �ʱ�ȭ
            totalEpvLevel = 0;

            // �������� Ŭ������� �� ���� Ƚ�� �ʱ�ȭ
            totalHitCount = 0;

            // �������� Ŭ������� �� �õ� Ƚ�� �ʱ�ȭ
            totaltryCount = 1;
        }

        if(isStageFirst)
        {
            stageStaticIndex = 0;
            isStageFirst = false;
        }

        // ���� ����
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
            uiLevelUp.Select(playerId % 2);    //�ӽ� ��ũ��Ʈ (ù��° ĳ���� ����)

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

            uiLevelUp.Select(playerId % 2);    //�ӽ� ��ũ��Ʈ (ù��° ĳ���� ����)

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
        // �������ʹ� ��ȸ��
        isFirst = false;

        // �� �õ� Ƚ�� ���ϱ�
        totaltryCount++;
        // �� ���� �ð� ���ϱ�
        totalAliveTime += gameTime;

        // ��� ��շ��� ���ϱ�
        foreach (Item item in levelUp.items)
        {
            totalEpvLevel += item.level;
        }

        // �� �ǰ� Ƚ�� ���ϱ�
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
        // �� �õ� Ƚ�� ���ϱ�
        totaltryCount++;
        // �� ���� �ð� ���ϱ�
        totalAliveTime += gameTime;

        // ��� ��շ��� ���ϱ�
        foreach (Item item in levelUp.items)
        {
            totalEpvLevel += item.level;
        }

        // �� �ǰ� Ƚ�� ���ϱ�
        totalHitCount += hitCount;

        // �������� Ŭ������� ��� ���� �ð�
        avgAliveTime = totalAliveTime / totaltryCount;
        // �������� Ŭ������� ��� ��� ����
        avgEpvLevel = totalEpvLevel / levelUp.items.Length / totaltryCount;
        // �������� Ŭ������� ��� ���ݹ��� Ƚ��
        avgHitCount = totalHitCount / totaltryCount;

        writeData(idf, stageIndex);

        //�������� ����
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