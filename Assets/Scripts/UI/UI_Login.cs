using Firebase.Auth;
using Firebase.Extensions;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UI_Login : UIBase
{
    [SerializeField] float hideDelay;
    WaitForSeconds _delay;

    // email check 여부
    bool bCheckEmail = false;

    private void Start()
    {
        // InfoPanel은 초기에 숨긴다.
        HideInfoPanel();

        // Login 버튼에 이벤트 추가
        AddUIEvent(Get("btnLogin"), Enums.UIEvent.PointerClick, Login);
        // SignUp 버튼에 이벤트 추가
        AddUIEvent(Get("btnSignUp"), Enums.UIEvent.PointerClick, ShowSignUpPanel);
        // Check Email 버튼에 이벤트 추가
        AddUIEvent(Get("btnCheckEmail"), Enums.UIEvent.PointerClick, CheckEmail);
        // Sing up Confirm 버튼에 이벤트 추가
        AddUIEvent(Get("btnConfirm"), Enums.UIEvent.PointerClick, ConfirmSignUp);
        // NickName Confirm 버튼에 이벤트 추가
        AddUIEvent(Get("btnConfirmNickName"), Enums.UIEvent.PointerClick, ConfirmNickName);

        // signUpIdInputField 이벤트 추가
        Get<InputField>("signUpIdInputField").onValueChanged.AddListener(str => 
        {
            // 인증했던 이메일을 변경하면 다시 인증필요
            if (bCheckEmail != false)
            {
                Debug.Log("Need to Email Check Again");
                SetEmailCheck(false);
            }
        });

        _delay = new WaitForSeconds(hideDelay);
    }

    public void Login(PointerEventData eventData)
    {
        // 입력값을 받아온다.
        string email = Get<InputField>("idInputField").text;
        string pass = Get<InputField>("passInputField").text;

        // 유효한지 검사
        if (string.IsNullOrEmpty(email))
        {
            Debug.LogWarning("Please Enter E-mail!");
            ShowInfoPopup("Please Enter E-mail!");
            return;
        }

        if (string.IsNullOrEmpty(pass))
        {
            Debug.LogWarning("Please Enter Password!");
            ShowInfoPopup("Please Enter Password!");
            return;
        }

        ShowInfoPanel("Please Wait...", Color.black);

        // 로그인 요청
        BackendManager.Auth.SignInWithEmailAndPasswordAsync(email, pass)
            .ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                HideInfoPanel();
                ShowInfoPopup("SignInWithEmailAndPasswordAsync was canceled.");
                return;
            }
            // 로그인 실패 시
            if (task.IsFaulted)
            {
                HideInfoPanel();
                Debug.Log("SignInWithEmailAndPasswordAsync encountered an error.");

                // Exception을 FirebaseException으로 변환하여 ErrorCode를 얻어오고
                // AuthError 열거형으로 변환하여 로그인 실패 사유를 확인
                AuthError eError = (AuthError)(task.Exception.GetBaseException() as Firebase.FirebaseException).ErrorCode;

                // 에러를 통한 팝업 처리
                CheckAuthError(eError);
                return;
            }

            // 로그인 성공 시
            Debug.Log("Login Successfully!");

            // 로그인한 유저를 가져온다.
            FirebaseUser user = task.Result.User;

            // 아직 이메일 인증을 하지 않은 경우
            if (!user.IsEmailVerified)
            {
                HideInfoPanel();
                // 이메일 인증을 위한 패널 표시
                Get("VerifyEmailPanel").SetActive(true);
            }
            // 아직 닉네임을 설정하지 않은 경우
            else if (string.IsNullOrEmpty(user.DisplayName))
            {
                HideInfoPanel();
                // 닉네임 설정을 위한 패널 표시
                Get("NickNamePanel").SetActive(true);
            }
            // 이메일 인증 및 닉네임 설정이 완료된 유저의 경우
            else
            {
                // 서버 연결 시도
                ConnectToMasterServer(user);
            }
        });
    }

    public void ConfirmNickName(PointerEventData eventData)
    {
        string nickName = Get<InputField>("NickNameInputField").text;
        if (string.IsNullOrEmpty(nickName))
        {
            ShowInfoPopup("Please Enter NickName!", Color.red);
            return;
        }

        FirebaseUser user = BackendManager.Auth.CurrentUser;

        // 유저 프로필 설정을 위한 UserProfile 객체
        UserProfile profile = new UserProfile();
        // nickName필드 설정
        profile.DisplayName = nickName;
        // 프로필 갱신 요청
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
                Debug.LogError($"UpdateUserProfileAsync encountered an error");
                string msg = task.Exception.Message;

                // 완료되지 못한 사유 출력
                ShowInfoPopup($"{msg.Substring(msg.IndexOf('(') + 1).Replace(')', ' ')}");
                return;
            }

            Debug.Log($"Set NickName Successfully! : {nickName}");

            // 마스터 서버 연결 요청
            ConnectToMasterServer(user);
        });
    }

    public void ConnectToMasterServer(FirebaseUser user)
    {
        // 닉네임 지정 및 연결
        PhotonNetwork.NickName = user.DisplayName;
        PhotonNetwork.ConnectUsingSettings();
    }

    public void CheckAuthError(AuthError eError)
    {
        switch (eError)
        {
            // 존재하지 않는 이메일인 경우 별도 메시지 출력
            // Wrong Password로 인한 실패 사실을 알리는 경우 해킹의 위험이 있으므로
            // Wrong Password case는 제외하였음
            case AuthError.UserNotFound:
                ShowInfoPopup("User not exist!", Color.red);
                break;
            case AuthError.Failure:
            case AuthError.ApiNotAvailable:
                ShowInfoPopup("System Error!", Color.red);
                break;
            default:
                ShowInfoPopup("Check Your Account!", Color.red);
                break;
        }
    }

    public void ShowInfoPanel(string msg, Color color, bool bShow = true)
    {
        Get<Text>("txtMessage").text = msg;
        Get<Text>("txtMessage").color = color;

        if (bShow)
            Get("InfoPanel").SetActive(true);
    }

    public void HideInfoPanel(bool bDelay = false)
    {
        if (!bDelay)
            Get("InfoPanel").SetActive(false);
        else
            StartCoroutine(HideRoutine());
    }

    IEnumerator HideRoutine()
    {
        yield return _delay;
        HideInfoPanel();
    }

    #region Sign Up
    public void ShowSignUpPanel(PointerEventData eventData)
    {
        // 회원가입 패널 활성화
        Get("SignUpPanel").SetActive(true);

        // 초기화
        Get<InputField>("signUpIdInputField").text = string.Empty;
        Get<InputField>("signUpPassInputField").text = string.Empty;
        Get<InputField>("signUpConfirmPassInputField").text = string.Empty;
    }

    public void CheckEmail(PointerEventData eventData)
    {
        // 이메일 필드값을 가져온다
        string email = Get<InputField>("signUpIdInputField").text;
        if (string.IsNullOrEmpty(email))
        {
            ShowInfoPopup("Please Enter E-mail!");
            return;
        }

        // email을 통해 서비스 제공업체 목록을 가져온다.
        // 해당 email을 통해 가져온 목록에 값이 하나라도 있으면 기존에 존재하는 email이며
        // 목록에 값이 하나도 없다면 아직 존재하지 않는 email으로 판단한다.
        BackendManager.Auth.FetchProvidersForEmailAsync(email)
            .ContinueWithOnMainThread(task =>
        {
            // 작업이 취소된 경우
            if (task.IsCanceled)
            {
                ShowInfoPopup("FetchProvidersForEmailAsync was canceled.");
                return;
            }
            // 작업이 완료되지 않은 경우
            if (task.IsFaulted)
            {
                Debug.LogError($"FetchProvidersForEmailAsync encountered an error");
                string msg = task.Exception.Message;

                // 완료되지 못한 사유 출력
                ShowInfoPopup($"{msg.Substring(msg.IndexOf('(') + 1).Replace(')', ' ')}");
                return;
            }

            // 서비스 제공업체 목록이 존재하면 email 중복으로 판단
            if (task.Result.Count() > 0)
            {
                SetEmailCheck(false);
                ShowInfoPopup("This e-mail already exist!\nUse another e-mail", Color.red);
            }
            else
            {
                // 이메일 중복체크 통과
                SetEmailCheck(true);
                ShowInfoPopup("E-mail Check OK!", Color.green);
            }
        });
            
    }

    // email check 여부 설정
    public void SetEmailCheck(bool bCheck)
    {
        bCheckEmail = bCheck;
        Get("imgCheckEmail").SetActive(bCheckEmail);
    }

    public void ConfirmSignUp(PointerEventData eventData)
    {
        // text field의 값들을 가져온다.
        string email = Get<InputField>("signUpIdInputField").text;
        string pass = Get<InputField>("signUpPassInputField").text;
        string confirmPass = Get<InputField>("signUpConfirmPassInputField").text;

        // 이메일 중복 확인을 통과하지 않은 경우
        if (!bCheckEmail)
        {
            ShowInfoPopup("Please Check Email!");
            return;
        }

        // 패스워드 입력이 안된 경우
        if (string.IsNullOrEmpty(pass))
        {
            ShowInfoPopup("Please Enter Password!");
            return;
        }

        // 패스워드 입력값들이 일치하지 않는 경우
        if (pass != confirmPass)
        {
            ShowInfoPopup("Please Check Confirm Password!");
            return;
        }

        // Firebase Auth를 통해 계정 생성 요청 (eamil, password를 통한 계정 생성)
        BackendManager.Auth.CreateUserWithEmailAndPasswordAsync(email, pass)
            .ContinueWithOnMainThread(task =>
        {
            // 작업이 취소된 경우
            if (task.IsCanceled)
            {
                ShowInfoPopup("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            // 작업이 완료되지 않은 경우
            if (task.IsFaulted)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error");
                string msg = task.Exception.Message;

                // 완료되지 못한 사유 출력
                ShowInfoPopup($"{msg.Substring(msg.IndexOf('(') + 1).Replace(')', ' ')}");
                return;
            }

            // 계정 생성 성공 시
            Debug.Log("Create User Successfully!");
            Get("SignUpPanel").SetActive(false);
        });
    }

    public void ShowInfoPopup(string msg, Color? color = null)
    {
        Get<Text>("InfoPopupMessageText").text = msg;
        Get<Text>("InfoPopupMessageText").color = color ?? Color.black;
        Get("InfoPopup").SetActive(true);
    }
    #endregion
}
