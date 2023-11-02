using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Result : MonoBehaviour
{
    public GameObject[] titles;
    public GameObject[] buttons;
    public void Lose()
    {
        titles[0].SetActive(true);
        buttons[0].SetActive(true);
    }

    public void Win()
    {
        titles[1].SetActive(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].SetActive(true);
        }
    }



}
