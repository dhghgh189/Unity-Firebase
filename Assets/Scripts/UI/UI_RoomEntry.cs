using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_RoomEntry : UIBase
{
    private void OnEnable()
    {
        AddUIEvent(Get("btnJoin"), Enums.UIEvent.PointerClick, Join);
    }

    private void OnDisable()
    {
        RemoveUIEvent(Get("btnJoin"), Enums.UIEvent.PointerClick, Join);
    }

    public void SetInfo(RoomInfo room)
    {
        Get<Text>("txtRoomName").text = room.Name;
        Get<Text>("txtPlayers").text = $"{room.PlayerCount} / {room.MaxPlayers}";
        Get<Button>("btnJoin").interactable = room.IsOpen;
    }

    public void Join(PointerEventData eventData)
    {
        PhotonNetwork.JoinRoom(Get<Text>("txtRoomName").text);
    }
}
