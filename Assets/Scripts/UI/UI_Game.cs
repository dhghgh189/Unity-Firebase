using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Game : UIBase
{
    private void OnEnable()
    {
        // Leave Room 버튼 이벤트 추가
        AddUIEvent(Get("btnLeaveRoom"), Enums.UIEvent.PointerClick, LeaveRoom);
    }

    private void OnDisable()
    {
        // Leave Room 버튼 이벤트 제거
        RemoveUIEvent(Get("btnLeaveRoom"), Enums.UIEvent.PointerClick, LeaveRoom);
    }

    public void LeaveRoom(PointerEventData eventData)
    {
        // 방에서 나가기 요청
        PhotonNetwork.LeaveRoom();
    }
}
