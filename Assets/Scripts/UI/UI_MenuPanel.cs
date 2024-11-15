using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_MenuPanel : UIBase
{
    [SerializeField] UI_CreateRoomPanel createRoomPanel;
    private void OnEnable()
    {
        // Create Room 버튼 이벤트 추가
        AddUIEvent(Get("btnCreateRoom"), Enums.UIEvent.PointerClick, CreateRoom);
        // Quick Join 버튼 이벤트 추가
        AddUIEvent(Get("btnQuickJoin"), Enums.UIEvent.PointerClick, QuickJoin);
        // Leave Lobby 버튼 이벤트 추가
        AddUIEvent(Get("btnLeaveLobby"), Enums.UIEvent.PointerClick, LeaveLobby);

        // Create Room Panel 비활성화
        createRoomPanel.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        // Create Room 버튼 이벤트 제거
        RemoveUIEvent(Get("btnCreateRoom"), Enums.UIEvent.PointerClick, CreateRoom);
        // Quick Join 버튼 이벤트 제거
        RemoveUIEvent(Get("btnQuickJoin"), Enums.UIEvent.PointerClick, QuickJoin);
        // Leave Lobby 버튼 이벤트 제거
        RemoveUIEvent(Get("btnLeaveLobby"), Enums.UIEvent.PointerClick, LeaveLobby);
    }

    public void CreateRoom(PointerEventData eventData)
    {
        // Create Room Panel 활성화
        createRoomPanel.gameObject.SetActive(true);
    }

    public void QuickJoin(PointerEventData eventData)
    {
        // 빠른 참가 (참가 가능한 방이 없는 경우 새로 개설)
        RoomOptions option = new RoomOptions();
        option.MaxPlayers = Define.MAX_PLAYER;
        PhotonNetwork.JoinRandomOrCreateRoom(roomName: $"{PhotonNetwork.LocalPlayer.NickName}'s Room", roomOptions: option);
    }

    public void LeaveLobby(PointerEventData eventData)
    {
        // 로비 나가기 요청
        PhotonNetwork.LeaveLobby();
    }
}
