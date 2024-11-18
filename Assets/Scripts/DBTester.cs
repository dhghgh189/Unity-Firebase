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

    // db에서 Uid를 참조하기 위한 Reference
    DatabaseReference _uidDataRef;
    // db에서 userData를 참조하기 위한 Reference
    DatabaseReference _userDataRef;
    // key로 사용할 uid
    string _uid;

    void Start()
    {
        _uid = BackendManager.Auth.CurrentUser.UserId;

        // 데이터를 읽기위해 DB의 Reference를 가져온다.
        _uidDataRef = BackendManager.Db.RootReference.Child($"UserID/{BackendManager.Auth.CurrentUser.DisplayName}");
        _userDataRef = BackendManager.Db.RootReference.Child($"UserData/{_uid}");

        // GetValueAsync : Reference를 통해 데이터 읽어 오기
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

            // 데이터 읽기 작업이 완료되면 DataSnapshot 객체를 통해
            // 읽은 데이터를 확인할 수 있다.
            DataSnapshot snapshot = task.Result;

            // 아직 설정된 값이 없는 경우
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

        // GetValueAsync : Reference를 통해 데이터 읽어 오기
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

            // 데이터 읽기 작업이 완료되면 DataSnapshot 객체를 통해
            // 읽은 데이터를 확인할 수 있다.
            DataSnapshot snapshot = task.Result;

            // 아직 설정된 값이 없는 경우
            if (snapshot.Value == null)
            {
                // 새로운 데이터 생성
                UserData newData = new UserData();
                newData.Name = BackendManager.Auth.CurrentUser.DisplayName;
                newData.Level = 1;
                // 구현 편의를 위해 임의로 Job 선정
                newData.Job = (Enums.Jobs)Random.Range(0, (int)Enums.Jobs.Length);
                newData.Stat = new PlayerStat();
                newData.Stat.Init();

                // 데이터를 쓰기 위해 json 형식으로 변환 
                string jsonStr = JsonUtility.ToJson(newData);

                // SetRawJsonValueAsync : json 형식 데이터를 한꺼번에 쓰기
                // 값을 모두 덮어씌우므로 초기화때만 사용하는게 좋다.
                _userDataRef.SetRawJsonValueAsync(jsonStr);
                userData = newData;

                Debug.Log($"$New User Data Saved! : {jsonStr}");
            }
            else
            {
                // 읽은 데이터를 json 형태로 반환
                string jsonStr = snapshot.GetRawJsonValue();
                Debug.Log(jsonStr);

                // json 형식 데이터를 UserData 객체로 역직렬화
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

        // DB에서 레벨 변경
        _userDataRef.Child("Level").SetValueAsync(userData.Level);

        // DB에서 스탯 변경
        DatabaseReference statRef = _userDataRef.Child("Stat");
        statRef.Child("Strength").SetValueAsync(userData.Stat.Strength);
        statRef.Child("Dex").SetValueAsync(userData.Stat.Dex);
        statRef.Child("Int").SetValueAsync(userData.Stat.Int);
        statRef.Child("Luck").SetValueAsync(userData.Stat.Luck);
    }
    #endregion
}
