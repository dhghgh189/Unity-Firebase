using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerEntry : UIBase
{
    public void Clear()
    {
        Get<Text>("txtNickname").text = string.Empty;
        Get("txtReady").SetActive(false);
    }

    public void SetEntry(Player player)
    {
        Get<Text>("txtNickname").text = player.NickName;

        if (player.IsMasterClient)
        {
            Get<Text>("txtReady").text = "HOST";
            Get("txtReady").SetActive(true);
        }
        else
        {
            Get<Text>("txtReady").text = "READY";
            Get("txtReady").SetActive(player.GetReady());
        }
    }
}
