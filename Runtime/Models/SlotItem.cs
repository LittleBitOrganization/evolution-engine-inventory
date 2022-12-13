using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Models
{
    public sealed class SlotItem : IDisposable
    {
        private readonly InventoryItem _item;
        private readonly int _weight;
        private readonly List<Cell> _cells;
        public readonly int Hash;
        public string Key { get; }

        private readonly Cell _head;
        public event Action<string, SlotItem> OnDispose;

        public Vector2Int Size => _item.Size;
        public Vector2Int Position => _head.Position;
        
        
        
        public SlotItem(InventoryItem item, int weight, List<Cell> cells)
        {
            _item = item;
            _weight = weight;
            _cells = cells;
            Hash = GetHashCode();
            Key = item.Key;
            

            foreach (var cell in _cells)
            {
                cell.SetWeight(_weight);
                cell.SetHash(Hash);
                cell.SetId(Key);
            }
            
            _head = _cells.First();
        }

        public void Dispose()
        {
            foreach (var cell in _cells)
            {
                cell.Dispose();
            }
            OnDispose?.Invoke(Key, this);
        }


        private void CheckKey(InventoryItem item)
        {
            if (_cells.All(v => v.Id == item.Key) == false)
            {
                throw new Exception("Key in cell not equals addable");
            }
        }

        private void CheckValue()
        {
            if (_cells.All(v => v.Value == _head.Value) == false)
                throw new Exception("Cells value not equals");
        }
        
        public bool CanAdd(InventoryItem inventoryItem, int value = 1)
        {
            CheckKey(inventoryItem);
            CheckValue();
            
            Debug.LogError($"FreeSpace: {_head.FreeSpace}");
            return _head.FreeSpace >= value;

        }

        public void TryAdd(InventoryItem inventoryItem, int value = 1)
        {
            
            if (CanAdd(inventoryItem, value))
            {
                Debug.LogError($"TryAdd {inventoryItem.Key} in {Hash}: {value}");
                foreach (var cell in _cells)
                {
                    cell.AddValue(value);
                }
            }
            else
            {
                Debug.LogError($"Cannot add {inventoryItem.Key} in {Hash}. FreeSpace: {_head.FreeSpace}");
            }
           
        }

        public bool CanRemove(InventoryItem inventoryItem, int value = 1)
        {
            CheckKey(inventoryItem);
            CheckValue();
            return _head.Value >= value;
        }

        public void TryRemove(InventoryItem inventoryItem, int value = 1)
        {
            if (CanRemove(inventoryItem, value))
            {
                foreach (var cell in _cells)
                {
                    cell.AddValue(-value);
                }
                if(_head.Value == 0)
                    Dispose();
            }
        }
    }
}