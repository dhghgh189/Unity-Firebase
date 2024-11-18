using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UserData
{
    public string Name;
    public int Level;
    public Enums.Jobs Job;
    public PlayerStat Stat;
}

[Serializable]
public class PlayerStat
{
    public int Strength;    // ��
    public int Dex;         // ��ø
    public int Int;         // ����
    public int Luck;        // ���

    public void Init()
    {
        Strength = 4;
        Dex = 4;
        Int = 4;
        Luck = 4;
    }
}
