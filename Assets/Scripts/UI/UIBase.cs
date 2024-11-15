using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UIBase : MonoBehaviour
{
    private Dictionary<string, GameObject> _objects = new Dictionary<string, GameObject>();
    private Dictionary<string, Component> _components = new Dictionary<string, Component>();

    private void Awake()
    {
        Bind();
    }

    private void Bind()
    {
        // 자식으로 있는 모든 transform을 얻어온다.
        Transform[] arrTransform = transform.GetComponentsInChildren<Transform>(true);

        // 순회하며 Bind
        for (int i = 0; i < arrTransform.Length; i++)
        {
            _objects.TryAdd(arrTransform[i].gameObject.name, arrTransform[i].gameObject);
        }
    }

    public GameObject Get(string name)
    {
        // name을 통해 GameObject를 받아온다.
        if (_objects.TryGetValue(name, out GameObject go) == false)
            Debug.LogWarning($"Can't find \"{name}\"");

        // 반환
        return go;
    }

    public T Get<T>(string name) where T : Component
    {
        // 키 생성
        string key = $"{name}_{typeof(T).Name}";
        if (_components.TryGetValue(key, out Component component))
        {
            return component as T;
        }

        // 딕셔너리에 캐싱된 컴포넌트가 없으면 GameObject 부터 받아온다.
        GameObject go = Get(name);

        // GameObject가 유효하지 않으면 null 반환
        if (go == null)
            return null;

        // GameObject로 부터 GetComponent 진행
        component = go.GetComponent<T>();

        // 해당 컴포넌트가 없으면 null 반환
        if (component == null)
        {
            Debug.LogWarning($"Can't find Component \"{typeof(T).Name}\" in \"{name}\"");
            return null;
        }

        // 컴포넌트를 찾았다면 캐싱
        _components.Add(key, component);

        // 컴포넌트 반환
        return component as T;
    }

    public void AddUIEvent(GameObject go, Enums.UIEvent eType, UnityAction<PointerEventData> action)
    {
        // 이벤트를 추가할 게임오브젝트로부터 UIEventHandler 컴포넌트를 받아온다
        // 만약 컴포넌트가 없다면 추가한다.
        UIEventHandler handler = go.GetOrAddComponent<UIEventHandler>();

        switch (eType)
        {
            case Enums.UIEvent.PointerClick:
                handler.OnPointerClicked += action;
                break;
        }
    }

    public void RemoveUIEvent(GameObject go, Enums.UIEvent eType, UnityAction<PointerEventData> action)
    {
        UIEventHandler handler = go.GetComponent<UIEventHandler>();
        if (handler == null)
            return;

        switch (eType)
        {
            case Enums.UIEvent.PointerClick:
                handler.OnPointerClicked -= action;
                break;
        }
    }
}
