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
        // Create Room ��ư �̺�Ʈ �߰�
        AddUIEvent(Get("btnCreateRoom"), Enums.UIEvent.PointerClick, CreateRoom);
        // Quick Join ��ư �̺�Ʈ �߰�
        AddUIEvent(Get("btnQuickJoin"), Enums.UIEvent.PointerClick, QuickJoin);
        // Leave Lobby ��ư �̺�Ʈ �߰�
        AddUIEvent(Get("btnLeaveLobby"), Enums.UIEvent.PointerClick, LeaveLobby);
        // Change Profile ��ư �̺�Ʈ �߰�
        AddUIEvent(Get("btnChangeProfile"), Enums.UIEvent.PointerClick, ShowAuthPanel);
        // Confirm Account ��ư �̺�Ʈ �߰�
        AddUIEvent(Get("btnConfirmAuth"), Enums.UIEvent.PointerClick, CheckAuth);
        // Confirm Change Profile ��ư �̺�Ʈ �߰�
        AddUIEvent(Get("btnConfirmChangeProfile"), Enums.UIEvent.PointerClick, ConfirmChangeProfile);

        // Create Room Panel ��Ȱ��ȭ
        Get("CreateRoomPanel").gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        // Create Room ��ư �̺�Ʈ ����
        RemoveUIEvent(Get("btnCreateRoom"), Enums.UIEvent.PointerClick, CreateRoom);
        // Quick Join ��ư �̺�Ʈ ����
        RemoveUIEvent(Get("btnQuickJoin"), Enums.UIEvent.PointerClick, QuickJoin);
        // Leave Lobby ��ư �̺�Ʈ ����
        RemoveUIEvent(Get("btnLeaveLobby"), Enums.UIEvent.PointerClick, LeaveLobby);
        // Change Profile ��ư �̺�Ʈ ����
        RemoveUIEvent(Get("btnChangeProfile"), Enums.UIEvent.PointerClick, ShowAuthPanel);
        // Confirm Account ��ư �̺�Ʈ ����
        RemoveUIEvent(Get("btnConfirmAuth"), Enums.UIEvent.PointerClick, CheckAuth);
        // Confirm Change Profile ��ư �̺�Ʈ ����
        RemoveUIEvent(Get("btnConfirmChangeProfile"), Enums.UIEvent.PointerClick, ConfirmChangeProfile);
    }

    public void CreateRoom(PointerEventData eventData)
    {
        // Create Room Panel Ȱ��ȭ
        Get("CreateRoomPanel").gameObject.SetActive(true);
    }

    public void QuickJoin(PointerEventData eventData)
    {
        // ���� ���� (���� ������ ���� ���� ��� ���� ����)
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

        // �� ���� ���� Ȯ��
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
            // �Ϸ���� ���� ���� ���
            ShowInfoPopup("Check Account!", Color.red);
        });
    }

    // ������ ������ ���� ���� �� ���� �õ�
    public void CheckInvalid(FirebaseUser user, in string email, in string password, UnityAction successCallback, UnityAction failedCallback)
    {
        // �� ���� ��û
        user.ReauthenticateAsync(EmailAuthProvider.GetCredential(email, password))
            .ContinueWithOnMainThread(task =>
        {
            // �۾��� ��ҵ� ���
            if (task.IsCanceled)
            {
                ShowInfoPopup("ReauthenticateAndRetrieveDataAsync was canceled.");
                return;
            }
            // �۾��� �Ϸ���� ���� ���
            if (task.IsFaulted)
            {
                Debug.LogError("ReauthenticateAndRetrieveDataAsync encountered an error");
                failedCallback?.Invoke();
                return;
            }

            // ���� ���� ��
            successCallback?.Invoke();
        });
    }

    public void ConfirmChangeProfile(PointerEventData eventData)
    {
        // text field�� ������ �����´�.
        string nickName = Get<InputField>("ChangeNickNameInputField").text;
        string pass = Get<InputField>("ChangePasswordInputField").text;
        string confirmPass = Get<InputField>("ChangePasswordConfirmInputField").text;

        // �г����� �Է����� ���� ���
        if (string.IsNullOrEmpty(nickName))
        {
            ShowInfoPopup("Please Enter NickName!");
            return;
        }

        // �н����� �Է°����� ��ġ���� �ʴ� ���
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
        // �г��� ����
        if (user.DisplayName != nickName)
        {
            UserProfile profile = new UserProfile();
            profile.DisplayName = nickName;

            user.UpdateUserProfileAsync(profile).ContinueWithOnMainThread(task =>
            {
                // �۾��� ��ҵ� ���
                if (task.IsCanceled)
                {
                    ShowInfoPopup("UpdateUserProfileAsync was canceled.");
                    return;
                }
                // �۾��� �Ϸ���� ���� ���
                if (task.IsFaulted)
                {
                    Debug.LogError("UpdateUserProfileAsync encountered an error");
                    string msg = task.Exception.Message;

                    // �Ϸ���� ���� ���� ���
                    ShowInfoPopup($"{msg.Substring(msg.IndexOf('(') + 1).Replace(')', ' ')}");
                    return;
                }

                // ���� ���� nickname ���� �ǽð����� ����
                PhotonNetwork.LocalPlayer.NickName = nickName;
                ShowInfoPopup("Update NickName Successfully!", Color.green);
                ChangePassword(user, password);
            });
        }
        else
        {
            // �г��� �������� �ʴ� ��� �ٷ� �н����� ���� �õ�
            ChangePassword(user, password);
        }
    }

    public void ChangePassword(FirebaseUser user, string password)
    {
        // �н����� ����
        CheckInvalid(user, user.Email, password, null,
        // �Ѱܹ��� password�� ���� �������� �����ϴ� ��� ��й�ȣ�� ������й�ȣ�� �ƴ϶�� ���� Ȯ�ε�
        // �Ѱܹ��� password�� ���� password�� �ٸ� ��� ������ �õ�
        () =>
        {
            if (!string.IsNullOrEmpty(password))
            {
                user.UpdatePasswordAsync(password).ContinueWithOnMainThread(task =>
                {
                    // �۾��� ��ҵ� ���
                    if (task.IsCanceled)
                    {
                        ShowInfoPopup("UpdatePasswordAsync was canceled.");
                        return;
                    }
                    // �۾��� �Ϸ���� ���� ���
                    if (task.IsFaulted)
                    {
                        Debug.LogError("UpdatePasswordAsync encountered an error");
                        string msg = task.Exception.Message;

                        // �Ϸ���� ���� ���� ���
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
        // �κ� ������ ��û
        PhotonNetwork.LeaveLobby();
    }

    public void ShowInfoPopup(string msg, Color? color = null)
    {
        Get<Text>("InfoPopupMessageText").text = msg;
        Get<Text>("InfoPopupMessageText").color = color ?? Color.black;
        Get("InfoPopup").SetActive(true);
    }
}
