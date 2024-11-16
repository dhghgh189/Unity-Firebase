using Firebase.Auth;
using Firebase.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_VerifyEmailPanel : UIBase
{
    Coroutine _verifyRoutine;
    WaitForSeconds _waitTime = new WaitForSeconds(1f);

    // 패널 활성화 시 
    void OnEnable()
    {
        FirebaseUser user = BackendManager.Auth.CurrentUser;

        Get<Text>("labelVerifyEmail").text = "An email has been sent for authentication.\r\nPlease Check Your E-mail !";

        // 인증을 위한 이메일 전송 요청
        user.SendEmailVerificationAsync().ContinueWithOnMainThread(task =>
        {
            // 작업이 취소된 경우
            if (task.IsCanceled)
            {
                Get<Text>("labelVerifyEmail").text = "SendEmailVerificationAsync was canceled.";
                return;
            }
            // 작업이 완료되지 않은 경우
            if (task.IsFaulted)
            {
                string msg = task.Exception.Message;
                Get<Text>("labelVerifyEmail").text = $"SendEmailVerificationAsync encountered an error\n({msg.Substring(msg.IndexOf('(') + 1).Replace(')', ' ')})";
                return;
            }

            // 이메일 송신 완료 시 인증 대기 코루틴 시작
            _verifyRoutine = StartCoroutine(VerifyRoutine());
        });
    }

    private void OnDisable()
    {
        // 패널 비활성화 시 코루틴 할당해제
        if (_verifyRoutine != null)
        {
            StopCoroutine(_verifyRoutine);
            _verifyRoutine = null;
        }
    }

    IEnumerator VerifyRoutine()
    {
        FirebaseUser user = BackendManager.Auth.CurrentUser;

        while (true)
        {
            // 이메일 인증 여부 확인 전 플레이어 정보 Reload 요청
            user.ReloadAsync().ContinueWithOnMainThread(task =>
            {
                // 작업이 취소된 경우
                if (task.IsCanceled)
                {
                    Get<Text>("labelVerifyEmail").text = "ReloadAsync was canceled.";
                    return;
                }
                // 작업이 완료되지 않은 경우
                if (task.IsFaulted)
                {
                    string msg = task.Exception.Message;
                    Get<Text>("labelVerifyEmail").text = $"ReloadAsync encountered an error\n({msg.Substring(msg.IndexOf('(') + 1).Replace(')', ' ')})";
                    return;
                }

                // 인증 여부 확인
                if (user.IsEmailVerified)
                {
                    Get<Text>("labelVerifyEmail").text = "Email Verified Successfully!\nPlease Login Again!";
                    return;
                }
            });

            // 잠시 대기
            yield return _waitTime;
        }
    }
}
