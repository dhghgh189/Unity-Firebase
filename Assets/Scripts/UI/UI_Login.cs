using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Login : UIBase
{
    [SerializeField] float hideDelay;
    WaitForSeconds _delay;

    private void Start()
    {
        // InfoPanel은 초기에 숨긴다.
        HideInfoPanel();

        // Login 버튼에 이벤트 추가
        AddUIEvent(Get("btnLogin"), Enums.UIEvent.PointerClick, Login);

        _delay = new WaitForSeconds(hideDelay);
    }

    public void Login(PointerEventData eventData)
    {
        // 입력값을 받아온다.
        string name = Get<InputField>("idInputField").text;

        // 유효한지 검사
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogWarning("Please Check ID!");
            return;
        }

        ShowInfoPanel("Please Wait...", Color.black);

        // 닉네임 설정 후 연결시도
        PhotonNetwork.NickName = name;
        PhotonNetwork.ConnectUsingSettings();
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
}
