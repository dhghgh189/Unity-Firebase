using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginScene : MonoBehaviourPunCallbacks
{
    [SerializeField] UI_Login loginUI;
    // 서버에 연결 성공한 경우
    public override void OnConnectedToMaster()
    {
        // 로비 씬으로 단순 이동
        SceneManager.LoadScene("LobbyScene");
    }

    // 접속 끊긴 경우
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"LoginScene 접속 해제 : {cause}");
        loginUI.ShowInfoPanel($"Login Failed...\n( {cause} )", Color.red, false);
        loginUI.HideInfoPanel(true);
    }
}
