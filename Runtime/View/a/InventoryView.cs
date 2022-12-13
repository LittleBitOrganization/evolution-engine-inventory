using System;
using System.Collections.Generic;
using UnityEngine;
using Configs;
using NaughtyAttributes;

public class InventoryView : MonoBehaviour
{
    [Header("GameModel")]
    [SerializeField] private List<GameObject> _models = new();
    [SerializeField] private List<InventoryItemConfig> _configs = new();
    
    [Header("Spawn"), Space]
    [SerializeField] private GameObject _cell;
    [SerializeField] private Transform _startPoint;
    [SerializeField] private Vector2 _size;

    private Dictionary<InventoryItemConfig, GameObject> _items = new();
    private List<GameObject> _cells = new();
    private List<GameObject> _placedItems = new();

    [Button]
    public void Build()
    {
        _cells.ForEach(c => DestroyImmediate(c.gameObject));
        _cells.Clear();
        
        for (int j = 0; j < _size.x; j++)
        {
            for (int i = 0; i < _size.y; i++)
            {
                var pos = new Vector3(_startPoint.position.x + i, _startPoint.position.y,_startPoint.position.z + j)*1.3f;
                var instantiate = Instantiate(_cell, pos, Quaternion.identity);
                instantiate.name = $"i{i}-j{j}";
                _cells.Add(instantiate);
            }
        }
    }

    [Button()]
    public void Clear()
    {
        _placedItems.ForEach(DestroyImmediate);
        _placedItems.Clear();
    }

    [Button()]
    public void Place2_1()
    {
        var key = _configs[0];
        PlaceItem(0, 0, _items[key]);
    }
    
    [Button()]
    public void Place1_2()
    {
        var key = _configs[1];
        PlaceItem(2, 4, _items[key]);
    }
    
    [Button()]
    public void OnEnable()
    {
        for (var index = 0; index < _configs.Count; index++)
        {
            var config = _configs[index];
            var model = _models[index];
            
            _items.Add(config, model);
        }
    }

    public void PlaceItem(int x, int y, GameObject item)
    {
        var value = x-1 + (y-1) * _size.y;
        var index = Math.Clamp(value, 0, _cells.Count);
        var position = _cells[(int)index].transform.position;
        var instantiate = Instantiate(item, position, Quaternion.identity);
        _placedItems.Add(instantiate);
    }
}

