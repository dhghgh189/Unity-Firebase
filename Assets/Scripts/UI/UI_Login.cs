using Firebase.Auth;
using Firebase.Extensions;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UI_Login : UIBase
{
    [SerializeField] float hideDelay;
    WaitForSeconds _delay;

    // email check ����
    bool bCheckEmail = false;

    private void Start()
    {
        // InfoPanel�� �ʱ⿡ �����.
        HideInfoPanel();

        // Login ��ư�� �̺�Ʈ �߰�
        AddUIEvent(Get("btnLogin"), Enums.UIEvent.PointerClick, Login);
        // SignUp ��ư�� �̺�Ʈ �߰�
        AddUIEvent(Get("btnSignUp"), Enums.UIEvent.PointerClick, ShowSignUpPanel);
        // Check Email ��ư�� �̺�Ʈ �߰�
        AddUIEvent(Get("btnCheckEmail"), Enums.UIEvent.PointerClick, CheckEmail);
        // Confirm ��ư�� �̺�Ʈ �߰�
        AddUIEvent(Get("btnConfirm"), Enums.UIEvent.PointerClick, ConfirmSignUp);

        // signUpIdInputField �̺�Ʈ �߰�
        Get<InputField>("signUpIdInputField").onValueChanged.AddListener(str => 
        {
            // �����ߴ� �̸����� �����ϸ� �ٽ� �����ʿ�
            if (bCheckEmail != false)
            {
                Debug.Log("Need to Email Check Again");
                SetEmailCheck(false);
            }
        });

        _delay = new WaitForSeconds(hideDelay);
    }

    public void Login(PointerEventData eventData)
    {
        // �Է°��� �޾ƿ´�.
        string name = Get<InputField>("idInputField").text;

        // ��ȿ���� �˻�
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogWarning("Please Enter E-mail!");
            ShowInfoPopup("Please Enter E-mail!");
            return;
        }

        ShowInfoPanel("Please Wait...", Color.black);

        // �г��� ���� �� ����õ�
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

    #region Sign Up
    public void ShowSignUpPanel(PointerEventData eventData)
    {
        // ȸ������ �г� Ȱ��ȭ
        Get("SignUpPanel").SetActive(true);

        // �ʱ�ȭ
        Get<InputField>("signUpIdInputField").text = string.Empty;
        Get<InputField>("signUpPassInputField").text = string.Empty;
        Get<InputField>("signUpConfirmPassInputField").text = string.Empty;
    }

    public void CheckEmail(PointerEventData eventData)
    {
        // �̸��� �ʵ尪�� �����´�
        string email = Get<InputField>("signUpIdInputField").text;
        if (string.IsNullOrEmpty(email))
        {
            ShowInfoPopup("Please Enter E-mail!");
            return;
        }

        // email�� ���� ���� ������ü ����� �����´�.
        // �ش� email�� ���� ������ ��Ͽ� ���� �ϳ��� ������ ������ �����ϴ� email�̸�
        // ��Ͽ� ���� �ϳ��� ���ٸ� ���� �������� �ʴ� email���� �Ǵ��Ѵ�.
        BackendManager.Auth.FetchProvidersForEmailAsync(email)
            .ContinueWithOnMainThread(task =>
        {
            // �۾��� ��ҵ� ���
            if (task.IsCanceled)
            {
                ShowInfoPopup("FetchProvidersForEmailAsync was canceled.");
                return;
            }
            // �۾��� �Ϸ���� ���� ���
            if (task.IsFaulted)
            {
                Debug.LogError($"FetchProvidersForEmailAsync encountered an error");
                string msg = task.Exception.Message;

                // �Ϸ���� ���� ���� ���
                ShowInfoPopup($"{msg.Substring(msg.IndexOf('(') + 1).Replace(')', ' ')}");
                return;
            }

            // ���� ������ü ����� �����ϸ� email �ߺ����� �Ǵ�
            if (task.Result.Count() > 0)
            {
                SetEmailCheck(false);
                ShowInfoPopup("This e-mail already exist!\nUse another e-mail", Color.red);
            }
            else
            {
                // �̸��� �ߺ�üũ ���
                SetEmailCheck(true);
                ShowInfoPopup("E-mail Check OK!", Color.green);
            }
        });
            
    }

    // email check ���� ����
    public void SetEmailCheck(bool bCheck)
    {
        bCheckEmail = bCheck;
        Get("imgCheckEmail").SetActive(bCheckEmail);
    }

    public void ConfirmSignUp(PointerEventData eventData)
    {
        // text field�� ������ �����´�.
        string email = Get<InputField>("signUpIdInputField").text;
        string pass = Get<InputField>("signUpPassInputField").text;
        string confirmPass = Get<InputField>("signUpConfirmPassInputField").text;

        // �̸��� �ߺ� Ȯ���� ������� ���� ���
        if (!bCheckEmail)
        {
            ShowInfoPopup("Please Check Email!");
            return;
        }

        // �н����� �Է��� �ȵ� ���
        if (string.IsNullOrEmpty(pass))
        {
            ShowInfoPopup("Please Enter Password!");
            return;
        }

        // �н����� �Է°����� ��ġ���� �ʴ� ���
        if (pass != confirmPass)
        {
            ShowInfoPopup("Please Check Confirm Password!");
            return;
        }

        // Firebase Auth�� ���� ���� ���� ��û (eamil, password�� ���� ���� ����)
        BackendManager.Auth.CreateUserWithEmailAndPasswordAsync(email, pass)
            .ContinueWithOnMainThread(task =>
        {
            // �۾��� ��ҵ� ���
            if (task.IsCanceled)
            {
                ShowInfoPopup("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            // �۾��� �Ϸ���� ���� ���
            if (task.IsFaulted)
            {
                Debug.LogError($"CreateUserWithEmailAndPasswordAsync encountered an error");
                string msg = task.Exception.Message;

                // �Ϸ���� ���� ���� ���
                ShowInfoPopup($"{msg.Substring(msg.IndexOf('(') + 1).Replace(')', ' ')}");
                return;
            }

            // ���� ���� ���� ��
            Debug.Log("Create User Successfully!");
            Get("SignUpPanel").SetActive(false);
        });
    }

    public void ShowInfoPopup(string msg, Color? color = null)
    {
        Get<Text>("InfoPopupMessageText").text = msg;
        Get<Text>("InfoPopupMessageText").color = color ?? Color.black;
        Get("InfoPopup").SetActive(true);
    }
    #endregion
}
