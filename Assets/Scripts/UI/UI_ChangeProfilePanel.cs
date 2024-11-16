using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ChangeProfilePanel : UIBase
{
    private void OnEnable()
    {
        Get<InputField>("ChangeNickNameInputField").text = BackendManager.Auth.CurrentUser.DisplayName;
        Get<InputField>("ChangePasswordInputField").text = string.Empty;
        Get<InputField>("ChangePasswordConfirmInputField").text = string.Empty;
    }
}
