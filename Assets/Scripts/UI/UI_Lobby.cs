using Firebase.Auth;
using Firebase.Extensions;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class UI_Lobby : UIBase
{
    private void OnEnable()
    {
        // Create Room 버튼 이벤트 추가
        AddUIEvent(Get("btnCreateRoom"), Enums.UIEvent.PointerClick, CreateRoom);
        // Quick Join 버튼 이벤트 추가
        AddUIEvent(Get("btnQuickJoin"), Enums.UIEvent.PointerClick, QuickJoin);
        // Leave Lobby 버튼 이벤트 추가
        AddUIEvent(Get("btnLeaveLobby"), Enums.UIEvent.PointerClick, LeaveLobby);
        // Change Profile 버튼 이벤트 추가
        AddUIEvent(Get("btnChangeProfile"), Enums.UIEvent.PointerClick, ShowAuthPanel);
        // Confirm Account 버튼 이벤트 추가
        AddUIEvent(Get("btnConfirmAuth"), Enums.UIEvent.PointerClick, CheckAuth);
        // Confirm Change Profile 버튼 이벤트 추가
        AddUIEvent(Get("btnConfirmChangeProfile"), Enums.UIEvent.PointerClick, ConfirmChangeProfile);

        // Create Room Panel 비활성화
        Get("CreateRoomPanel").gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        // Create Room 버튼 이벤트 제거
        RemoveUIEvent(Get("btnCreateRoom"), Enums.UIEvent.PointerClick, CreateRoom);
        // Quick Join 버튼 이벤트 제거
        RemoveUIEvent(Get("btnQuickJoin"), Enums.UIEvent.PointerClick, QuickJoin);
        // Leave Lobby 버튼 이벤트 제거
        RemoveUIEvent(Get("btnLeaveLobby"), Enums.UIEvent.PointerClick, LeaveLobby);
        // Change Profile 버튼 이벤트 제거
        RemoveUIEvent(Get("btnChangeProfile"), Enums.UIEvent.PointerClick, ShowAuthPanel);
        // Confirm Account 버튼 이벤트 제거
        RemoveUIEvent(Get("btnConfirmAuth"), Enums.UIEvent.PointerClick, CheckAuth);
        // Confirm Change Profile 버튼 이벤트 제거
        RemoveUIEvent(Get("btnConfirmChangeProfile"), Enums.UIEvent.PointerClick, ConfirmChangeProfile);
    }

    public void CreateRoom(PointerEventData eventData)
    {
        // Create Room Panel 활성화
        Get("CreateRoomPanel").gameObject.SetActive(true);
    }

    public void QuickJoin(PointerEventData eventData)
    {
        // 빠른 참가 (참가 가능한 방이 없는 경우 새로 개설)
        RoomOptions option = new RoomOptions();
        option.MaxPlayers = Define.MAX_PLAYER;
        PhotonNetwork.JoinRandomOrCreateRoom(roomName: $"{PhotonNetwork.LocalPlayer.NickName}'s Room", roomOptions: option);
    }

    #region Change Profile
    public void ShowAuthPanel(PointerEventData eventData)
    {
        Get("CheckAuthPanel").SetActive(true);
    }

    public void CheckAuth(PointerEventData eventData)
    {
        string email = Get<InputField>("EmailInputField").text;
        string password = Get<InputField>("PasswordInputField").text;

        if (string.IsNullOrEmpty(email))
        {
            ShowInfoPopup("Please Enter E-mail!", Color.red);
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowInfoPopup("Please Enter Password!", Color.red);
            return;
        }

        FirebaseUser user = BackendManager.Auth.CurrentUser;

        // 재 인증 여부 확인
        CheckInvalid(user, email, password, 
        // success callback
        () => 
        {
            Get("CheckAuthPanel").SetActive(false);
            Get("ChangeProfilePanel").SetActive(true);
        },
        // failed callback
        () =>
        {
            // 완료되지 못한 사유 출력
            ShowInfoPopup("Check Account!", Color.red);
        });
    }

    // 프로필 변경을 위해 유저 재 인증 시도
    public void CheckInvalid(FirebaseUser user, in string email, in string password, UnityAction successCallback, UnityAction failedCallback)
    {
        // 재 인증 요청
        user.ReauthenticateAsync(EmailAuthProvider.GetCredential(email, password))
            .ContinueWithOnMainThread(task =>
        {
            // 작업이 취소된 경우
            if (task.IsCanceled)
            {
                ShowInfoPopup("ReauthenticateAndRetrieveDataAsync was canceled.");
                return;
            }
            // 작업이 완료되지 않은 경우
            if (task.IsFaulted)
            {
                Debug.LogError("ReauthenticateAndRetrieveDataAsync encountered an error");
                failedCallback?.Invoke();
                return;
            }

            // 인증 성공 시
            successCallback?.Invoke();
        });
    }

    public void ConfirmChangeProfile(PointerEventData eventData)
    {
        // text field의 값들을 가져온다.
        string nickName = Get<InputField>("ChangeNickNameInputField").text;
        string pass = Get<InputField>("ChangePasswordInputField").text;
        string confirmPass = Get<InputField>("ChangePasswordConfirmInputField").text;

        // 닉네임을 입력하지 않은 경우
        if (string.IsNullOrEmpty(nickName))
        {
            ShowInfoPopup("Please Enter NickName!");
            return;
        }

        // 패스워드 입력값들이 일치하지 않는 경우
        if (pass != confirmPass)
        {
            ShowInfoPopup("Please Check Confirm Password!");
            return;
        }

        FirebaseUser user = BackendManager.Auth.CurrentUser;

        ChangeProfile(user, nickName, pass);
    }

    public void ChangeProfile(FirebaseUser user, string nickName, string password)
    {
        // 닉네임 변경
        if (user.DisplayName != nickName)
        {
            UserProfile profile = new UserProfile();
            profile.DisplayName = nickName;

            user.UpdateUserProfileAsync(profile).ContinueWithOnMainThread(task =>
            {
                // 작업이 취소된 경우
                if (task.IsCanceled)
                {
                    ShowInfoPopup("UpdateUserProfileAsync was canceled.");
                    return;
                }
                // 작업이 완료되지 않은 경우
                if (task.IsFaulted)
                {
                    Debug.LogError("UpdateUserProfileAsync encountered an error");
                    string msg = task.Exception.Message;

                    // 완료되지 못한 사유 출력
                    ShowInfoPopup($"{msg.Substring(msg.IndexOf('(') + 1).Replace(')', ' ')}");
                    return;
                }

                // 포톤 서버 nickname 또한 실시간으로 변경
                PhotonNetwork.LocalPlayer.NickName = nickName;
                ShowInfoPopup("Update NickName Successfully!", Color.green);
                ChangePassword(user, password);
            });
        }
        else
        {
            // 닉네임 변경하지 않는 경우 바로 패스워드 변경 시도
            ChangePassword(user, password);
        }
    }

    public void ChangePassword(FirebaseUser user, string password)
    {
        // 패스워드 변경
        CheckInvalid(user, user.Email, password, null,
        // 넘겨받은 password를 통해 재인증이 실패하는 경우 비밀번호가 기존비밀번호가 아니라는 것이 확인됨
        // 넘겨받은 password가 기존 password와 다른 경우 변경을 시도
        () =>
        {
            if (!string.IsNullOrEmpty(password))
            {
                user.UpdatePasswordAsync(password).ContinueWithOnMainThread(task =>
                {
                    // 작업이 취소된 경우
                    if (task.IsCanceled)
                    {
                        ShowInfoPopup("UpdatePasswordAsync was canceled.");
                        return;
                    }
                    // 작업이 완료되지 않은 경우
                    if (task.IsFaulted)
                    {
                        Debug.LogError("UpdatePasswordAsync encountered an error");
                        string msg = task.Exception.Message;

                        // 완료되지 못한 사유 출력
                        ShowInfoPopup($"{msg.Substring(msg.IndexOf('(') + 1).Replace(')', ' ')}");
                        return;
                    }

                    Debug.Log("Update Password Successfully!");
                    ShowInfoPopup("Update Password Successfully!", Color.green);
                });
            }
        });
    }
    #endregion

    public void LeaveLobby(PointerEventData eventData)
    {
        // 로비 나가기 요청
        PhotonNetwork.LeaveLobby();
    }

    public void ShowInfoPopup(string msg, Color? color = null)
    {
        Get<Text>("InfoPopupMessageText").text = msg;
        Get<Text>("InfoPopupMessageText").color = color ?? Color.black;
        Get("InfoPopup").SetActive(true);
    }
}
