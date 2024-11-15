using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public static class CustomProperty
{
    static PhotonHashtable customProperty = new PhotonHashtable();
    public const string READY = "Ready";

    // 전달받은 플레이어의 커스텀 프로퍼티 'Ready' 값 설정
    public static void SetReady(this Player player, bool bReday)
    {
        customProperty.Clear();
        // Ready 값 설정
        customProperty.Add(READY, bReday);
        // 플레이어의 커스텀 프로퍼티 업데이트
        player.SetCustomProperties(customProperty);
    }
    
    // 전달받은 플레이어의 커스텀 프로퍼티 'Ready' 값 반환
    public static bool GetReady(this Player player)
    {
        PhotonHashtable playerProperty = player.CustomProperties;

        // 값이 존재하지 않으면 false
        if (playerProperty.ContainsKey(READY) == false)
            return false;

        // 플레이어의 Ready 값 반환
        return (bool)playerProperty[READY];
    }
}
