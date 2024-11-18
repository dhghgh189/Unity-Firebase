using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_UserInfoPanel : UIBase
{
    private void Start()
    {
        // 처음부터 비활성화되어있으면 ui 바인딩이 안되므로
        // 활성화 된상태에서 바인딩 후 start 시 비활성화 하도록 함 
        gameObject.SetActive(false);
    }

    // 유저 정보 UI 표시
    public void ShowUserInfo(UserData userData)
    {
        // 데이터 설정
        Get<Text>("txtSearchUserName").text = $"User Name : {userData.Name}";
        Get<Text>("txtSearchUserLevel").text = $"Level : {userData.Level}";
        Get<Text>("txtSearchUserJob").text = $"Job : {userData.Job}";
        Get<Text>("txtSearchUserStrength").text = $"Strength : {userData.Stat.Strength}";
        Get<Text>("txtSearchUserDex").text = $"Dex : {userData.Stat.Dex}";
        Get<Text>("txtSearchUserInt").text = $"Int : {userData.Stat.Int}";
        Get<Text>("txtSearchUserLuck").text = $"Luck : {userData.Stat.Luck}";

        // 활성화
        gameObject.SetActive(true);
    }
}
