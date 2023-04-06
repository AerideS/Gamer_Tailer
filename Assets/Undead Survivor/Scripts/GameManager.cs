using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Player player;
    public static GameManager instance;

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        
    }
}
