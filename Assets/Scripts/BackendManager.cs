using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class BackendManager : Singleton<BackendManager>
{
    FirebaseApp app;
    FirebaseAuth auth;
    public static FirebaseApp App { get { return Instance.app; } }
    public static FirebaseAuth Auth { get { return Instance.auth; } }

    protected override void Init()
    {
        CheckDependency();
    }

    private void CheckDependency()
    {
        // FireBase에서 서비스를 요청할 때는 비동기로 요청을 진행
        // ContinueWithOnMainThread 함수를 통해 비동기 작업 완료 후 
        // 이어서 수행할 작업을 바인딩 해놓을 수 있다.
        // CheckAndFixDependenciesAsync : 서비스 버전 요구사항 확인 및 Fix
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            // 서비스가 가능한 상황
            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("[BackendManager] <color=green>Check Dependencies OK</color>");
                try
                {
                    // 인스턴스를 캐싱한다
                    app = FirebaseApp.DefaultInstance;
                    Debug.Log("[BackendManager] <color=green>Get App Successfully</color>");

                    auth = FirebaseAuth.DefaultInstance;
                    Debug.Log("[BackendManager] <color=green>Get Auth Successfully</color>");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[BackendManager] {e.Message}");

                    // 결과 Log 출력
                    if (app == null)
                        Debug.LogError("[BackendManager] Can't get App");

                    if (auth == null)
                        Debug.LogError("[BackendManager] Can't get Auth");
                }
            }
            // 서비스하기에 불안정한 상황
            else
            {
                Debug.LogError($"[BackendManager] Check Dependencies Failed.. {task.Result}");
                app = null;
                auth = null;
            }

            // 인증 서비스가 불가능한 경우
            if (auth == null)
            {
                // 에러 상황을 표시
                FindAnyObjectByType<UI_Login>().ShowInfoPanel("Backend Error!\nPlease Exit Game", Color.red);
            }
        });
    }
}
