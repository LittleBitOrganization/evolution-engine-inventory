using System;
using System.Collections.Generic;
using System.Linq;
using Configs;
using Models;
using NaughtyAttributes;
using UnityEngine;

public class InventoryView : MonoBehaviour
{
    [Header("GameModel")] [SerializeField] private List<GameObject> _models;
    [SerializeField] private List<InventoryItemConfig> _configs;

    [SerializeField] private InventoryConfig _inventoryConfig;

    [Header("Spawn"), Space] [SerializeField]
    private GameObject _cell;

    [SerializeField] private Transform _startPoint;
    [SerializeField] private Vector2Int _size;

    private Dictionary<string, GameObject> _items = new Dictionary<string, GameObject>();
    private GameObject[,] _cells;
    private List<GameObject> _placedItems = new List<GameObject>();


    private InventoryModel _inventory;

    private void Awake()
    {
        _size = new Vector2Int(_inventoryConfig.Size.x, _inventoryConfig.Size.y);
        _inventory = new InventoryModel(_inventoryConfig);
        _inventory.OnRepacking += OnInventoryRepacking;
        _cells = new GameObject[_size.x, _size.y];
        for (var index = 0; index < _configs.Count; index++)
        {
            var config = _configs[index];
            var model = _models[index];

            _items.Add(config.Key, model);
        }
        
        Build();
    }

    private void OnInventoryRepacking(List<SlotItem> slots)
    {
        Clear();
        foreach (var slot in slots)
        {
            PlaceItem(slot.Position.x, slot.Position.y, _items[slot.Key]);
        }
    }
    

    [Button]
    public void Build()
    {
        for (int i = 0; i < _size.x; i++)
        {
            for (int j = 0; j < _size.y; j++)
            {
                var pos = new Vector3(_startPoint.position.x + i, _startPoint.position.y, _startPoint.position.z + j);
                var instantiate = Instantiate(_cell, pos, Quaternion.identity);
                instantiate.name = $"i{i}-j{j}";
                _cells[i, j] = instantiate;
            }
        }
    }

    [Button()]
    public void Clear()
    {
        _placedItems.ForEach(DestroyImmediate);
        _placedItems.Clear();
    }

    public void PlaceItem(int x, int y, GameObject item)
    {
        var position = _cells[x, y].transform.position - new Vector3(0.5f, 0, 0.5f);
        var instantiate = Instantiate(item, position, Quaternion.identity);
        _placedItems.Add(instantiate);
    }


    [Button()]
    private void Test()
    {
        Add5x2();
        Add4x1();
        Add3x1();
        Add3x1();
        Add3x1();

        Add2x1();
        Add2x1();
        Add2x1();

        Add2x2();

        Add1x1();
        
    }

    private void Add(InventoryItemConfig inventoryItemConfig)
    {
        if (_inventory.TryAddInventoryItem(new InventoryItem(inventoryItemConfig), out var slot))
        {
            Debug.Log(slot.Position);
            PlaceItem(slot.Position.x, slot.Position.y, _items[slot.Key]);
        }
    }
    
    private void Remove(InventoryItemConfig inventoryItemConfig)
    {
        _inventory.RemoveInventoryItem(new InventoryItem(inventoryItemConfig));
        var last = _placedItems.Last();
        _placedItems.Remove(last);
        Destroy(last);
    }


    [Button()]
    private void Add5x2()
    {
        Add(_configs[6]);
    }

    [Button()]
    private void Add4x1()
    {
        Add(_configs[5]);
    }

    [Button()]
    private void Add3x1()
    {
        Add(_configs[4]);
    }

    [Button()]
    private void Add2x2()
    {
        Add(_configs[3]);
    }

    [Button()]
    private void Add2x1()
    {
        Add(_configs[2]);
    }

    [Button()]
    private void Add1x3()
    {
        Add(_configs[1]);
    }

    [Button()]
    private void Add1x1()
    {
        Add(_configs[0]);
    }

    [Button()]
    private void Remove1x1()
    {
        Remove(_configs[0]);
    }
}