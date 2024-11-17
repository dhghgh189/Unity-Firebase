using Firebase.Auth;
using Firebase.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_QuestionPopup : UIBase
{
    UnityAction _yesCallback;

    private void Start()
    {
        // 처음부터 비활성화되어있으면 ui 바인딩이 안되므로
        // 활성화 된상태에서 바인딩 후 start 시 비활성화 하도록 함 
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        // Yes 버튼 이벤트 추가
        AddUIEvent(Get("btnYes"), Enums.UIEvent.PointerClick, Execute);
    }

    private void OnDisable()
    {
        _yesCallback = null;

        // Yes 버튼 이벤트 제거
        RemoveUIEvent(Get("btnYes"), Enums.UIEvent.PointerClick, Execute);
    }

    public void ShowQuestionPopup(UnityAction yesCallback, in string msg, Color? color = null)
    {
        _yesCallback = yesCallback;

        Get<Text>("QuestionPopupMessageText").text = msg;
        Get<Text>("QuestionPopupMessageText").color = color ?? Color.black;

        gameObject.SetActive(true);
    }

    public void Execute(PointerEventData eventData)
    {
        _yesCallback?.Invoke();
    }
}
