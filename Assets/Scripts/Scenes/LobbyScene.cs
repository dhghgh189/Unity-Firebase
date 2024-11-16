using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class LobbyScene : MonoBehaviourPunCallbacks
{
    public enum Panel { LobbyPanel, RoomPanel }

    [SerializeField] GameObject menuPanel;
    [SerializeField] UI_RoomListPanel roomListPanel;
    [SerializeField] UI_RoomPanel roomPanel;
    [SerializeField] UI_CreateRoomPanel createRoomPanel;

    void Start()
    {
        // 호스트와 씬 상황을 동기화하도록 설정
        PhotonNetwork.AutomaticallySyncScene = true;

        // 로비 입장 시도
        // 연결 가능한 상태까지 대기했다가 연결
        StartCoroutine(LobbyConnectRoutine());
    }

    // 로비 입장 시
    public override void OnJoinedLobby()
    {
        Debug.Log("로비 입장 성공");

        // 마지막 인원이 방에서 나와서 다시 로비에 접속했을 때
        // 아직 지워지기 전의 방이 보이는 것이 부자연스러우므로
        // 로비에 접속하자마자 한번 entry를 clear 해주는 작업 진행
        // 이후 OnRoomListUpdate이 호출되므로 다시 최신화 됨
        roomListPanel.ClearRoomEntries();

        SetActivePanel(Panel.LobbyPanel);
    }

    // 로비 퇴장 시
    public override void OnLeftLobby()
    {
        Debug.Log("로비에서 퇴장");
        PhotonNetwork.Disconnect();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"LobbyScene 접속 해제 : {cause}");
        SceneManager.LoadScene("LoginScene");
    }

    // 로비에 있는 동안 방들에 대한 정보를 수신
    // 최초 1번 전체 방에 대한 정보를 받아오며
    // 이후에는 변경사항이 있는 방의 정보만을 받아온다.
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        roomListPanel.UpdateRoomList(roomList);
    }

    // 방 생성 시
    public override void OnCreatedRoom()
    {
        Debug.Log($"방 생성 성공 : {PhotonNetwork.CurrentRoom.Name}");
        createRoomPanel.gameObject.SetActive(false);
    }

    // 방에 들어간 경우
    public override void OnJoinedRoom()
    {
        Debug.Log($"방 입장 성공 : {PhotonNetwork.CurrentRoom.Name}");
        SetActivePanel(Panel.RoomPanel);
    }

    // 방 생성 실패
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log($"방 생성 실패 : {message}");
    }

    // 방 입장 실패
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log($"방 입장 실패 : {message}");

        // 방 입장 실패 시 Lobby에서 나가진 상태가 되므로 다시 Lobby로 접속 시도
        StartCoroutine(LobbyConnectRoutine());
    }

    // 빠른 입장 실패
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log($"빠른 입장 실패 {message}");
    }

    // 방에서 나간 경우
    public override void OnLeftRoom()
    {
        Debug.Log("방에서 퇴장");

        // 방에서 퇴장 후 바로 로비에 연결시도를 했을 때 네트워크가 준비되지 않아
        // 코루틴으로 연결가능한 상태가 될 때 까지 대기하도록 처리
        StartCoroutine(LobbyConnectRoutine());
    }

    // 새 플레이어가 방에 참가 시
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        roomPanel.EnterPlayer(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        roomPanel.ExitPlayer(otherPlayer);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, PhotonHashtable changedProps)
    {
        roomPanel.UpdateRoomInfo();
    }

    public void SetActivePanel(Panel panel)
    {
        menuPanel.gameObject.SetActive(panel == Panel.LobbyPanel);
        roomListPanel.gameObject.SetActive(panel == Panel.LobbyPanel);
        roomPanel.gameObject.SetActive(panel == Panel.RoomPanel);
    }

    IEnumerator LobbyConnectRoutine()
    {
        // 무한 대기를 막기 위한 Counter
        int iCount = 500;

        while (true)
        {
            // 현재 로비 연결이 가능 한 상태가 아니라면 (네트워크가 아직 준비되지 않음)
            if (PhotonNetwork.NetworkClientState != ClientState.ConnectedToMasterServer)
            {
                // 카운트 차감
                iCount--;
                // 카운트가 모두 차감되면 Timeout 처리
                if (iCount <= 0)
                {
                    Debug.LogError("Timeout! : LobbyConnectRoutine");
                    yield break;
                }

                // 한 프레임 대기후 처음부터 반복
                yield return null;
            }
            // 로비 연결이 가능한 상태가 된 경우
            else
            {
                // 로비 연결 시도
                PhotonNetwork.JoinLobby();
                break;
            }
        }
    }
}
