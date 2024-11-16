using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_CheckAuthPanel : UIBase
{
    private void OnEnable()
    {
        Get<InputField>("EmailInputField").text = string.Empty;
        Get<InputField>("PasswordInputField").text = string.Empty;
    }
}
