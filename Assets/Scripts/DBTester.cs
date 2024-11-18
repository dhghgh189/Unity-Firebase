using Firebase.Database;
using Firebase.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DBTester : MonoBehaviour
{
    [SerializeField] UserData userData;

    // db���� userData�� �����ϱ� ���� Reference
    DatabaseReference _userDataRef;
    // key�� ����� uid
    string _uid;

    void Start()
    {
        _uid = BackendManager.Auth.CurrentUser.UserId;

        // �����͸� �б����� DB�� Reference�� �����´�.
        _userDataRef = BackendManager.Db.RootReference.Child($"UserData/{_uid}");

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
                _userDataRef.SetRawJsonValueAsync(jsonStr);

                Debug.Log($"$New Data Saved! : {jsonStr}");

                userData = newData;
            }
            else
            {
                // ���� �����͸� json ���·� ��ȯ
                string jsonStr = snapshot.GetRawJsonValue();

                // json ���� �����͸� UserData ��ü�� ������ȭ
                userData = JsonUtility.FromJson<UserData>(jsonStr);

                Debug.Log($"Data Loaded : {userData}");
            }
        });
    }
}
