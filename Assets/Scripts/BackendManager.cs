using Firebase;
using Firebase.Auth;
using Firebase.Database;
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
    FirebaseDatabase db;

    public static FirebaseApp App { get { return Instance.app; } }
    public static FirebaseAuth Auth { get { return Instance.auth; } }
    public static FirebaseDatabase Db { get { return Instance.db; } }

    protected override void Init()
    {
        CheckDependency();
    }

    private void CheckDependency()
    {
        // FireBase���� ���񽺸� ��û�� ���� �񵿱�� ��û�� ����
        // ContinueWithOnMainThread �Լ��� ���� �񵿱� �۾� �Ϸ� �� 
        // �̾ ������ �۾��� ���ε� �س��� �� �ִ�.
        // CheckAndFixDependenciesAsync : ���� ���� �䱸���� Ȯ�� �� Fix
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            // ���񽺰� ������ ��Ȳ
            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("[BackendManager] <color=green>Check Dependencies OK</color>");
                try
                {
                    // �ν��Ͻ��� ĳ���Ѵ�
                    app = FirebaseApp.DefaultInstance;
                    Debug.Log("[BackendManager] <color=green>Get App Successfully</color>");

                    auth = FirebaseAuth.DefaultInstance;
                    Debug.Log("[BackendManager] <color=green>Get Auth Successfully</color>");

                    db = FirebaseDatabase.DefaultInstance;
                    Debug.Log("[BackendManager] <color=green>Get Db Successfully</color>");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[BackendManager] {e.Message}");

                    // ��� Log ���
                    if (app == null)
                        Debug.LogError("[BackendManager] Can't get App");

                    if (auth == null)
                        Debug.LogError("[BackendManager] Can't get Auth");

                    if (db == null)
                        Debug.LogError("[BackendManager] Can't get Db");
                }
            }
            // �����ϱ⿡ �Ҿ����� ��Ȳ
            else
            {
                Debug.LogError($"[BackendManager] Check Dependencies Failed.. {task.Result}");
                app = null;
                auth = null;
                db = null;
            }

            // ���� ���񽺰� �Ұ����� ���
            if (auth == null || db == null)
            {
                // ���� ��Ȳ�� ǥ��
                FindAnyObjectByType<UI_Login>().ShowInfoPanel("Backend Error!\nPlease Exit Game", Color.red);
            }
        });
    }
}
