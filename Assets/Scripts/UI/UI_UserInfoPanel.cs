using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_UserInfoPanel : UIBase
{
    private void Start()
    {
        // ó������ ��Ȱ��ȭ�Ǿ������� ui ���ε��� �ȵǹǷ�
        // Ȱ��ȭ �Ȼ��¿��� ���ε� �� start �� ��Ȱ��ȭ �ϵ��� �� 
        gameObject.SetActive(false);
    }

    // ���� ���� UI ǥ��
    public void ShowUserInfo(UserData userData)
    {
        // ������ ����
        Get<Text>("txtSearchUserName").text = $"User Name : {userData.Name}";
        Get<Text>("txtSearchUserLevel").text = $"Level : {userData.Level}";
        Get<Text>("txtSearchUserJob").text = $"Job : {userData.Job}";
        Get<Text>("txtSearchUserStrength").text = $"Strength : {userData.Stat.Strength}";
        Get<Text>("txtSearchUserDex").text = $"Dex : {userData.Stat.Dex}";
        Get<Text>("txtSearchUserInt").text = $"Int : {userData.Stat.Int}";
        Get<Text>("txtSearchUserLuck").text = $"Luck : {userData.Stat.Luck}";

        // Ȱ��ȭ
        gameObject.SetActive(true);
    }
}
