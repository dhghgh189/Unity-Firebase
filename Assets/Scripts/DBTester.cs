using Firebase.Database;
using Firebase.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.EventSystems;

public class DBTester : MonoBehaviour
{
    [SerializeField] UserData userData;

    // db���� Uid�� �����ϱ� ���� Reference
    DatabaseReference _uidDataRef;
    // db���� userData�� �����ϱ� ���� Reference
    DatabaseReference _userDataRef;
    // key�� ����� uid
    string _uid;

    void Start()
    {
        _uid = BackendManager.Auth.CurrentUser.UserId;

        // �����͸� �б����� DB�� Reference�� �����´�.
        _uidDataRef = BackendManager.Db.RootReference.Child($"UserID/{BackendManager.Auth.CurrentUser.DisplayName}");
        _userDataRef = BackendManager.Db.RootReference.Child($"UserData/{_uid}");

        // GetValueAsync : Reference�� ���� ������ �о� ����
        _uidDataRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("GetValueAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError($"GetValueAsync encountered an error : {task.Exception.Message}");
                return;
            }

            // ������ �б� �۾��� �Ϸ�Ǹ� DataSnapshot ��ü�� ����
            // ���� �����͸� Ȯ���� �� �ִ�.
            DataSnapshot snapshot = task.Result;

            // ���� ������ ���� ���� ���
            if (snapshot.Value == null)
            {
                _uidDataRef.Child("Uid").SetValueAsync(_uid);
                Debug.Log($"$New Uid Data Saved! : {_uid}");
            }
            else
            {
                Debug.Log("uid already exist");
            }
        });

        // GetValueAsync : Reference�� ���� ������ �о� ����
        _userDataRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("GetValueAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError($"GetValueAsync encountered an error : {task.Exception.Message}");
                return;
            }

            // ������ �б� �۾��� �Ϸ�Ǹ� DataSnapshot ��ü�� ����
            // ���� �����͸� Ȯ���� �� �ִ�.
            DataSnapshot snapshot = task.Result;

            // ���� ������ ���� ���� ���
            if (snapshot.Value == null)
            {
                // ���ο� ������ ����
                UserData newData = new UserData();
                newData.Name = BackendManager.Auth.CurrentUser.DisplayName;
                newData.Level = 1;
                // ���� ���Ǹ� ���� ���Ƿ� Job ����
                newData.Job = (Enums.Jobs)Random.Range(0, (int)Enums.Jobs.Length);
                newData.Stat = new PlayerStat();
                newData.Stat.Init();

                // �����͸� ���� ���� json �������� ��ȯ 
                string jsonStr = JsonUtility.ToJson(newData);

                // SetRawJsonValueAsync : json ���� �����͸� �Ѳ����� ����
                // ���� ��� �����Ƿ� �ʱ�ȭ���� ����ϴ°� ����.
                _userDataRef.SetRawJsonValueAsync(jsonStr);
                userData = newData;

                Debug.Log($"$New User Data Saved! : {jsonStr}");
            }
            else
            {
                // ���� �����͸� json ���·� ��ȯ
                string jsonStr = snapshot.GetRawJsonValue();
                Debug.Log(jsonStr);

                // json ���� �����͸� UserData ��ü�� ������ȭ
                userData = JsonUtility.FromJson<UserData>(jsonStr);

                Debug.Log($"Data Loaded : {userData}");
            }
        });
    }

    #region TEST
    public void LvUpTest()
    {
        if (userData.Level >= 100)
            return;

        SetLevel(userData.Level+1);
    }

    public void LvDownTest()
    {
        if (userData.Level <= 1)
            return;

        SetLevel(userData.Level-1);
    }

    public void SetLevel(int level)
    {
        if (level == userData.Level)
            return;

        userData.Level = level;
        userData.Stat.UpdateStat(level);

        // DB���� ���� ����
        _userDataRef.Child("Level").SetValueAsync(userData.Level);

        // DB���� ���� ����
        DatabaseReference statRef = _userDataRef.Child("Stat");
        statRef.Child("Strength").SetValueAsync(userData.Stat.Strength);
        statRef.Child("Dex").SetValueAsync(userData.Stat.Dex);
        statRef.Child("Int").SetValueAsync(userData.Stat.Int);
        statRef.Child("Luck").SetValueAsync(userData.Stat.Luck);
    }
    #endregion
}
