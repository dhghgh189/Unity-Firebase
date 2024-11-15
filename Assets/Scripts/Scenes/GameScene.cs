using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : MonoBehaviourPunCallbacks
{
    public override void OnLeftRoom()
    {
        Debug.Log("방에서 퇴장하였습니다.");
        
        // 로비 씬으로 이동
        PhotonNetwork.LoadLevel("LobbyScene");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        // 로비 씬으로 이동
        PhotonNetwork.LoadLevel("LobbyScene");
    }
}
