using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_RoomPanel : UIBase
{
    [SerializeField] UI_PlayerEntry[] playerEntries;
    private void OnEnable()
    {
        // Ready Or Start 버튼 이벤트 추가
        AddUIEvent(Get("btnReadyOrStart"), Enums.UIEvent.PointerClick, StartOrReady);
        // Leave Room 버튼 이벤트 추가
        AddUIEvent(Get("btnLeaveRoom"), Enums.UIEvent.PointerClick, LeaveRoom);

        // 현재 방내 player numbering에 변동이 있는 경우 호출되는 이벤트
        PlayerNumbering.OnPlayerNumberingChanged += UpdateRoomInfo;

        // 최초 입장시 ready 상태 해제
        PhotonNetwork.LocalPlayer.SetReady(false);
    }

    private void OnDisable()
    {
        // Ready Or Start 버튼 이벤트 제거
        RemoveUIEvent(Get("btnReadyOrStart"), Enums.UIEvent.PointerClick, StartOrReady);
        // Leave Room 버튼 이벤트 제거
        RemoveUIEvent(Get("btnLeaveRoom"), Enums.UIEvent.PointerClick, LeaveRoom);

        PlayerNumbering.OnPlayerNumberingChanged -= UpdateRoomInfo;
    }

    public void LeaveRoom(PointerEventData eventData)
    {
        // 방 나가기 요청
        PhotonNetwork.LeaveRoom();
    }

    public void UpdateRoomInfo()
    {
        // Room UI 초기화
        Get<Text>("txtRoomName").text = PhotonNetwork.CurrentRoom.Name;
        Get<Text>("btnReadyOrStartText").text = "READY";

        // Entry 초기화
        foreach (UI_PlayerEntry entry in playerEntries)
        {
            entry.Clear();
        }

        // Entry 설정
        foreach(Player player in PhotonNetwork.PlayerList)
        {
            // 플레이어에 배정된 번호를 index로 사용
            int number = player.GetPlayerNumber();

            // 아직 number 배정이 안된 player의 경우 예외처리
            if (number < 0)
                continue;

            // entry 설정
            playerEntries[number].SetEntry(player);
        }

        // 방장인 경우 추가로 처리할 사항
        if (PhotonNetwork.IsMasterClient)
        {
            // 버튼 텍스트 변경
            Get<Text>("btnReadyOrStartText").text = "START";
            Get<Button>("btnReadyOrStart").interactable = CheckAllReady();
        }
    }

    // 새 플레이어 입장 시
    public void EnterPlayer(Player player)
    {
        Debug.Log($"{player.NickName} 입장");
        UpdateRoomInfo();
    }

    // 다른 플레이어 퇴장 시
    public void ExitPlayer(Player player)
    {
        Debug.Log($"{player.NickName} 퇴장");
        UpdateRoomInfo();
    }

    public bool CheckAllReady()
    {
        foreach(Player player in PhotonNetwork.PlayerList)
        {
            // 방장인 경우 체크에 포함하지 않는다.
            if (player.IsMasterClient)
                continue;

            // 한명이라도 준비하지 않았다면 false
            if (!player.GetReady())
                return false;
        }

        // 모두 준비한 상태
        return true;
    }

    public void StartOrReady(PointerEventData eventData)
    {
        // 방장인 경우 START 로직 처리
        if (PhotonNetwork.IsMasterClient)
        {
            if (!CheckAllReady())
                return;

            // 씬 이동
            PhotonNetwork.LoadLevel("GameScene");
        }
        // 방장이 아닌 경우 READY 로직 처리
        else
        {
            Player player = PhotonNetwork.LocalPlayer;

            // 레디상태 변경
            player.SetReady(!player.GetReady());
        }
    }
}
