using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Firebase.Firestore
using Firebase.Firestore;
using Firebase.Extensions;
using System.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;

// Firebase Realtime Database
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

// Using Server
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics.Tracing;

// Using Encoding
using System.Text;

public class GameManager : MonoBehaviour
{
    // DB instances, number of data.
    FirebaseFirestore db;
    // FirebaseDatabase db;
    DatabaseReference m_Reference;

    // TcpClient Socket.
    private TcpClient socketConnection;

    public static GameManager instance;
    [Header("# Game Control")]
    public bool isLive;
    public float gameTime;
    public float maxGameTime;
    public GameObject[] Stages;

    [Header("# Player Info")]
    public int playerId;
    public float health;
    public float maxHealth = 100;
    public int level;
    public int kill;
    public int exp;
    public int[] nextExp = { 10, 30, 90, 270, 810, 2430, 7290, 21870 };

    [Header("# Game Object")]
    public Player player;
    public PoolManager pool;
    public LevelUp uiLevelUp;
    public Result uiResult;
    public GameObject enemyCleaner;

    // To get the average level.
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

    public float[] timeRange = { 30f, 45f, 60f, 75 };


    [Header("변수 조절값")]
    private const float SPEEDV = 0.4f;
    private const float SPAWNV = 0.2f;
    private const float DAMAGEV = 0.2f;
    private const float HPV = 0.2f;
    private const float MONSPEEDV = 0.2f;

    string makeIdentifier()
    {
        // Create Identifier.
        int length = 8;
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        System.Random random = new System.Random();
        char[] randomChars = new char[length + 2];

        for (int i = 0; i < length; i++)
        {
            randomChars[i] = chars[random.Next(chars.Length)];
        }
        return new string(randomChars);
    }

    public class Data
    {
        public int avgHitCount;
        public int stageIndex;
        public int totalTryCount;
        public float avgEpvLevel;
        public float avgAliveTime;

        public Data(int avgHitCount, int stageIndex, int totalTryCount, float avgEpvLevel, float avgAliveTime)
        {
            this.avgHitCount = avgHitCount;
            this.stageIndex = stageStaticIndex;
            this.totalTryCount = totalTryCount;
            this.avgEpvLevel = avgEpvLevel;
            this.avgAliveTime = avgAliveTime;
        }
    }

    // DB write function
    void writeData(string randomChars, int stageIndex)
    {
        // DB Initialize
        /*        db = FirebaseFirestore.DefaultInstance;
                // DB Write
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
                docRef.SetAsync(dat).ContinueWithOnMainThread(task =>
                {
                    // Data Count +1

                    Debug.Log(string.Format("stage count : {0}", stageStaticIndex));
                });*/

        m_Reference = FirebaseDatabase.DefaultInstance.RootReference;
        Data data = new Data(avgHitCount, stageIndex, totaltryCount, avgEpvLevel, avgAliveTime);
        string json = JsonUtility.ToJson(data);

        m_Reference.Child("data").Child(idf).Child(stageStaticIndex.ToString()).SetRawJsonValueAsync(json);
        return;
    }

    // Server communication function.
    private void ConnectToTcpServer()
    {
        try
        {
            socketConnection = new TcpClient("127.0.0.1", 8080);
        }
        catch (Exception e)
        {
            Debug.Log("On client connect exception " + e);
        }
    }

    // Send precent data to Server and get the predict of next stage.
    private void SendDataToServer()
    {
        try
        {
            // Sending data part
            NetworkStream stream = socketConnection.GetStream();

            string data = stageIndex.ToString() + ", " + avgEpvLevel.ToString() + ", " + avgAliveTime.ToString() + ", " + avgHitCount.ToString() + ", " + totaltryCount.ToString();

            byte[] bytesToSend = Encoding.UTF8.GetBytes(data);

            stream.Write(bytesToSend, 0, bytesToSend.Length);
            Debug.Log("Data send: " + data);

            // Recieve data part
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length); // read the data from server.
            string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            string a = receivedData;
            string cleanedString = a.Replace("[[", "").Replace("]]", ""); // without this line, It will occur error when you run float.pares.
            float predictValue = float.Parse(cleanedString);
            Debug.Log(predictValue);
        }
        catch(Exception e)
        {
            Debug.Log("On client Send exception " + e);
        }
    }

    void Awake()
    {
        instance = this;
        Application.targetFrameRate = 60;

        // Initialize the number of hits.
        hitCount = 0;

        if (isFirst)
        {
            // DB User Id.
            idf = makeIdentifier();

            // Initialize the sum of total survival time to stage clear.
            totalAliveTime = 0;

            // Initialize sum of total equipment levels until stage clear.
            totalEpvLevel = 0;

            // Initialize the number of shots to stage clear.
            totalHitCount = 0;

            // Initialize the total number of attempts to clear the stage.
            totaltryCount = 1;
        }

        if (isStageFirst)
        {
            stageStaticIndex = 0;
            maxGameTime = timeRange[stageStaticIndex];
            isStageFirst = false;
        }
        else
        {
            maxGameTime = timeRange[stageStaticIndex];
        }
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
            uiLevelUp.Select(playerId % 2);    // Temporary script (first character selection).


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


            uiLevelUp.Select(playerId % 2);    // Temporary script (first character selection).

            enemyCleaner.gameObject.SetActive(false);
            gameTime = 0;
            player.transform.position = Vector3.zero;

            Resume();
            AudioManager.instance.PlayBgm(true);
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
        }
    }

    public void GameOver()
    {
        // From now on, multi try.
        isFirst = false;

        // Add the total number of attempts.
        totaltryCount++;
        // Total survival time plus.
        totalAliveTime += gameTime;

        // Get the average level of equipment.
        foreach (Item item in levelUp.items)
        {
            totalEpvLevel += item.level;
        }

        // Get the total number of hits.
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
        enemyCleaner.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.3f);
        enemyCleaner.gameObject.SetActive(false);
        if (stageIndex == Stages.Length - 1)
        {
            GameRetry();
        }
        else
        {
            uiResult.gameObject.SetActive(true);
            uiResult.Win();
            Stop();
        }

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
            if (stageStaticIndex == 3)
            {
                NextStage();
            }
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
    }
    public void Resume()
    {
        isLive = true;
        Time.timeScale = 1;
    }

    public void NextStage()
    {
        // Total survival time plus.
        totalAliveTime += gameTime;

        // Find the average level of equipment.
        foreach (Item item in levelUp.items)
        {
            totalEpvLevel += item.level;
        }

        // Add the total number of hits
        totalHitCount += hitCount;

        // Average survival time to stage clear.
        avgAliveTime = totalAliveTime / totaltryCount;
        // Average equipment level to stage clear.
        avgEpvLevel = totalEpvLevel / levelUp.items.Length / totaltryCount;
        // Average number of attacks to stage clear.
        avgHitCount = totalHitCount / totaltryCount;

        Debug.Log(string.Format("{0} {1}", totalAliveTime, avgAliveTime));

        writeData(idf, stageIndex);


        // Connecting to a Server.
        ConnectToTcpServer();
        // Send to Server
        SendDataToServer();

        // Initialize all in the next round.
        totalHitCount = 0;
        totalAliveTime = 0;
        totalEpvLevel = 0;
        totalHitCount = 0;
        totaltryCount = 1;



        health = maxHealth;

        if (stageStaticIndex == 3)
        {
            stageStaticIndex = 0;
            isStageFirst = true;
            return;
        }

        // Stage Change.
        if (stageStaticIndex <= 3)
        {
            Stages[stageIndex].SetActive(false);
            stageIndex++; stageStaticIndex++;
            maxGameTime = timeRange[stageStaticIndex];
            Stages[stageIndex].SetActive(true);
            uiResult.gameObject.SetActive(false);
            player.gameObject.SetActive(true);
            //pool = Stages[stageIndex].transform.Find("PoolManager").GetComponent<PoolManager>();
            //if (pool_m.transform.Find("Bullet 1(Clone)") != null)
            //{
            //    pool_m.transform.Find("Bullet 1(Clone)").GetComponent<Bullet>().per = -1;
            //}
            //if (player.transform.Find("Weapon 1").GetComponent<Weapon>() != null)
            //{
            //    Weapon we = player.transform.Find("Weapon 1").GetComponent<Weapon>();
            //    we.prefabId = 2;
            //    we.Fire();
            //}
            gameTime = 0;
            player.transform.position = Vector3.zero;
            Resume();
            AudioManager.instance.PlayBgm(true);
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
        }
    }

    void SpeedUserCon(float value)
    {
        if (value > 1)
        {
            player.speed = player.speed + player.speed * SPEEDV;
        }
        else
        {
            player.speed = player.speed * (1 - SPEEDV);
        }

    }

    void SpawnTimeCon(float value)
    {
        Spawner spawner = new Spawner();
        level = Mathf.Min(Mathf.FloorToInt(gameTime / maxGameTime), spawner.spawnData.Length - 1);
        if (value > 1)
        {
            for (int i = 0; i < spawner.spawnData.Length; i++)
            {
                spawner.spawnData[i].spawnTime = spawner.spawnData[i].spawnTime + spawner.spawnData[i].spawnTime * SPAWNV;
            }

        }
        else
        {
            for (int i = 0; i < spawner.spawnData.Length; i++)
            {
                spawner.spawnData[i].spawnTime = spawner.spawnData[i].spawnTime * (1 - SPAWNV);
            }
        }

    }

    void MONSPCon(float value)
    {
        Spawner spawner = new Spawner();
        level = Mathf.Min(Mathf.FloorToInt(gameTime / maxGameTime), spawner.spawnData.Length - 1);
        if (value > 1)
        {
            for (int i = 0; i < spawner.spawnData.Length; i++)
            {
                spawner.spawnData[i].speed = spawner.spawnData[i].speed + spawner.spawnData[i].speed * MONSPEEDV;
            }

        }
        else
        {
            for (int i = 0; i < spawner.spawnData.Length; i++)
            {
                spawner.spawnData[i].speed = spawner.spawnData[i].speed * (1 - MONSPEEDV);
            }

        }

        //void SpawnTimeCon(float value)
        //{
        //    Spawner spawner = new Spawner();
        //    level = Mathf.Min(Mathf.FloorToInt(gameTime / maxGameTime), spawner.spawnData.Length - 1);
        //    if (value > 1)
        //    {
        //        spawner.spawnData[level].spawnTime = spawner.spawnData[level].spawnTime + spawner.spawnData[level].spawnTime * SPAWNV;

        //    }
        //    else
        //    {
        //        spawner.spawnData[level].spawnTime = spawner.spawnData[level].spawnTime * (1 - SPAWNV);
        //    }

        //}

        //void SpawnTimeCon(float value)
        //{
        //    Spawner spawner = new Spawner();
        //    level = Mathf.Min(Mathf.FloorToInt(gameTime / maxGameTime), spawner.spawnData.Length - 1);
        //    if (value > 1)
        //    {
        //        spawner.spawnData[level].spawnTime = spawner.spawnData[level].spawnTime + spawner.spawnData[level].spawnTime * SPAWNV;

        //    }
        //    else
        //    {
        //        spawner.spawnData[level].spawnTime = spawner.spawnData[level].spawnTime * (1 - SPAWNV);
        //    }

        //}

    }
}