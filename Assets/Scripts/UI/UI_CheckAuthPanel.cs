using Firebase.Auth;
using Firebase.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_CheckAuthPanel : UIBase
{
    UnityAction _successCallback;
    UnityAction _failedCallback;
    [SerializeField] UI_Lobby lobby;

    private void OnEnable()
    {
        Get<InputField>("EmailInputField").text = string.Empty;
        Get<InputField>("PasswordInputField").text = string.Empty;

        // Confirm Auth 버튼 이벤트 추가
        AddUIEvent(Get("btnConfirmAuth"), Enums.UIEvent.PointerClick, CheckAuth);
    }

    private void OnDisable()
    {
        _successCallback = null;
        _failedCallback = null;

        // Confirm Auth 버튼 이벤트 제거
        RemoveUIEvent(Get("btnConfirmAuth"), Enums.UIEvent.PointerClick, CheckAuth);
    }

    public void EnableAuthPanel(UnityAction successCallback, UnityAction failedCallback)
    {
        _successCallback = successCallback;
        _failedCallback = failedCallback;
        gameObject.SetActive(true);
    }

    public void CheckAuth(PointerEventData eventData)
    {
        string email = Get<InputField>("EmailInputField").text;
        string password = Get<InputField>("PasswordInputField").text;

        if (string.IsNullOrEmpty(email))
        {
            lobby.ShowInfoPopup("Please Enter E-mail!", Color.red);
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            lobby.ShowInfoPopup("Please Enter Password!", Color.red);
            return;
        }

        FirebaseUser user = BackendManager.Auth.CurrentUser;

        // 재 인증 여부 확인
        CheckInvalid(user, email, password, _successCallback, _failedCallback);
    }

    public void CheckInvalid(FirebaseUser user, in string email, in string password, UnityAction successCallback, UnityAction failedCallback)
    {
        // 재 인증 요청
        user.ReauthenticateAsync(EmailAuthProvider.GetCredential(email, password))
            .ContinueWithOnMainThread(task =>
            {
                // 작업이 취소된 경우
                if (task.IsCanceled)
                {
                    lobby.ShowInfoPopup("ReauthenticateAsync was canceled.");
                    return;
                }
                // 작업이 완료되지 않은 경우
                if (task.IsFaulted)
                {
                    failedCallback?.Invoke();
                    return;
                }

                // 인증 성공 시
                successCallback?.Invoke();
            });
    }
}
