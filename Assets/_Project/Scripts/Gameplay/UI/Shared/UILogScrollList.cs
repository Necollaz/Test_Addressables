using System.Collections.Generic;
using UnityEngine;

public class UILogScrollList : MonoBehaviour
{
    [SerializeField] private RectTransform _contentRoot;
    [SerializeField] private UILogView _logItemPrefab;
    [SerializeField] private int _maxVisibleItems = 200;
    [SerializeField] private bool _reusePooledItems = true;

    private readonly Queue<UILogView> _itemsQueue = new Queue<UILogView>(256);
    private readonly Stack<UILogView> _poolStack = new Stack<UILogView>(64);

    public void Append(string message)
    {
        if (string.IsNullOrEmpty(message) || _contentRoot == null || _logItemPrefab == null)
            return;

        UILogView instance = GetItemInstance();
        instance.SetMessage($"[{System.DateTime.Now:HH:mm:ss}] {message}");
        instance.gameObject.SetActive(true);
        instance.transform.SetAsLastSibling();

        _itemsQueue.Enqueue(instance);

        while (_itemsQueue.Count > _maxVisibleItems)
        {
            UILogView oldest = _itemsQueue.Dequeue();

            if (_reusePooledItems)
            {
                oldest.gameObject.SetActive(false);
                _poolStack.Push(oldest);
            }
            else
            {
                Destroy(oldest.gameObject);
            }
        }
    }

    private UILogView GetItemInstance()
    {
        if (_reusePooledItems && _poolStack.Count > 0)
        {
            UILogView pooled = _poolStack.Pop();
            pooled.transform.SetParent(_contentRoot, false);
            
            return pooled;
        }

        UILogView created = Instantiate(_logItemPrefab);
        created.transform.SetParent(_contentRoot, false);
        
        return created;
    }
}