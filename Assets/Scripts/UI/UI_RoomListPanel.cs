using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_RoomListPanel : UIBase
{
    [SerializeField] UI_RoomEntry roomEntryPrefab;

    Dictionary<string, UI_RoomEntry> roomDict = new Dictionary<string, UI_RoomEntry>();

    public void UpdateRoomList(List<RoomInfo> roomList)
    {
        foreach (RoomInfo room in roomList)
        {
            // 리스트에서 보여지지 않아야 할 Room인 경우
            if (room.RemovedFromList || room.IsVisible == false || room.IsOpen == false)
            {
                // 아직 생성되지 않은 상황이였다면 무시해도 괜찮음
                if (roomDict.ContainsKey(room.Name) == false)
                    continue;

                // Entry 삭제
                Destroy(roomDict[room.Name].gameObject);
                roomDict.Remove(room.Name);
            }
            // 방이 갱신되는 경우 (이미 dictionary에 존재한다면 갱신되는 상황)
            else if (roomDict.ContainsKey(room.Name))
            {
                // 정보 갱신
                roomDict[room.Name].SetInfo(room);
            }
            // 새로 생성되야 하는 방의 경우 (dictionary에 존재하지 않음)
            else if (roomDict.ContainsKey(room.Name) == false)
            {
                // 새 Entry 생성
                UI_RoomEntry entry = Instantiate(roomEntryPrefab, Get("ContentArea").transform);
                // 정보 설정
                entry.SetInfo(room);
                // Entry 추가
                roomDict.Add(room.Name, entry);
            }
        }
    }

    // 모든 Entry 삭제
    public void ClearRoomEntries()
    {
        foreach (UI_RoomEntry entry in roomDict.Values)
        {
            Destroy(entry.gameObject);
        }
        roomDict.Clear();
    }
}
