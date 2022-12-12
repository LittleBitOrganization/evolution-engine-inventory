using System;
using System.Collections.Generic;
using System.Linq;
using Configs;
using UnityEngine;

namespace Models
{
    public class InventoryModel
    {
        private readonly InventoryConfig _inventoryConfig;
        private readonly Matrix _matrix;

        private Dictionary<string, List<SlotItem>> _containerSlots;

        private PackingInventory _packingInventory;

        public InventoryModel(InventoryConfig inventoryConfig)
        {
            _inventoryConfig = inventoryConfig;
            _containerSlots = new Dictionary<string, List<SlotItem>>();
            _matrix = new Matrix(_inventoryConfig.Size.x, _inventoryConfig.Size.y, _inventoryConfig.Size.z);
           
            _matrix.Log();
        }

        private SlotItem GetSlot(InventoryItem inventoryItem, Func<SlotItem, bool> predicate)
        {
            var slots = GetSlots(inventoryItem);
            SlotItem slot = null;
            if (slots.Count > 0)
            {
                slot = slots.FirstOrDefault(predicate);
                if (slot != null)
                {
                    Debug.LogError("Get exist slot");
                    return slot;
                }
            }
            Debug.LogError("Get null slot");

            return null;
        }

        public void AddInventoryItem(InventoryItem inventoryItem)
        {
            SlotItem slot = GetSlot(inventoryItem,v => v.CanAdd(inventoryItem));
            if (slot == null)
            {
                var groupCells = _matrix.FindAvailableCells(inventoryItem.Size, inventoryItem.Weight);
                if (groupCells == null)
                {
                    Debug.LogError("GroupCells not found");
                    return;
                }

                slot = CreateSlot(inventoryItem, groupCells);
               
            }
            
            slot.TryAdd(inventoryItem, 1);
            _matrix.Log();
        }

        public void RemoveInventoryItem(InventoryItem inventoryItem)
        {
            SlotItem slot = GetSlot(inventoryItem, v => v.CanRemove(inventoryItem, 1));
            if (slot == null)
            {
                Debug.LogError("Cannot remove item. Not found slots with key: " + inventoryItem.Key);
                return;
            }
            slot.TryRemove(inventoryItem, 1);
            _matrix.Log();
        }

        private List<SlotItem> GetSlots(InventoryItem item)
        {
            if (_containerSlots.ContainsKey(item.Key) == false)
            {
                _containerSlots.Add(item.Key, new List<SlotItem>());
            }
            return _containerSlots[item.Key];
        }
        private SlotItem CreateSlot(InventoryItem item, List<Cell> cells)
        {
            var slot = new SlotItem(item, item.Weight, cells);
            slot.OnDispose += RemoveSlot;
            GetSlots(item).Add(slot);

            return slot;
        }

        private void RemoveSlot(string key, SlotItem slotItem)
        {
            _containerSlots[key].Remove(slotItem);
            slotItem.OnDispose -= RemoveSlot;
        }
        
    }

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

    public class InventoryItem
    {
        private readonly InventoryItemConfig _config;
        public Vector2Int Size => _config.Size;
        public int Weight => _config.Weight;

        public string Key => _config.Key;

        public InventoryItem(InventoryItemConfig config)
        {
            _config = config;
        }
    }

    public class Matrix
    {
        private Cell[,] _cells;
        public Matrix(int x, int y, int z)
        {
            _cells = new Cell[x, y];
            Foreach((i, j) =>
            {
                _cells[i, j] = new Cell(z, new Vector2Int(i, j));
            });
         
        }

        private void Foreach(Action<int, int> index)
        {
            for (int i = 0; i < _cells.GetLength(0); i++)
            {
                for (int j = 0; j < _cells.GetLength(1); j++)
                {
                    index?.Invoke(i, j);
                }
            }
        }

        public void Log()
        {
            string text = "Log matrix:\n";
            
            for (int i = 0; i < _cells.GetLength(0); i++)
            {
                for (int j = 0; j < _cells.GetLength(1); j++)
                {
                    var cell = _cells[i, j];
                    string cellText = $"[{cell.Value}]|[{cell.Capacity}]\t";
                    text += cellText;
                }

                text += "\n";
            }
            Debug.LogError(text);
        }

        public List<Cell> FindAvailableCells(Vector2Int inventoryItemSize, int weight)
        {
            var dimension0 = _cells.GetLength(0);
            var dimension1 = _cells.GetLength(1);
            
            if (dimension0 < inventoryItemSize.x) return null;
            if (dimension1 < inventoryItemSize.y) return null;


            for (int i = 0; i < dimension0 - (inventoryItemSize.x -1); i++)
            {
                for (int j = 0; j < dimension1 - (inventoryItemSize.y - 1); j++)
                {
                    var cells = GetAvailableCells(i, j, inventoryItemSize.x, inventoryItemSize.y, weight);
                    if (cells != null)
                    {
                        return cells;
                    }
                }
            }
            
            for (int i = 0; i < dimension0 - inventoryItemSize.x; i++)
            {
                for (int j = 0; j < dimension1 - inventoryItemSize.y; j++)
                {
                    var cells = GetFreeCells(i, j, inventoryItemSize.x, inventoryItemSize.y);
                    if (cells != null)
                    {
                        return cells;
                    }
                }
            }

            return null;
        }

        private List<Cell> GetBlockCells(int xStart, int yStart, int xSize, int ySize)
        {
            List<Cell> cells = new List<Cell>();
            
            
            for (int i = xStart; i < xStart + xSize; i++)
            {
                for (int j = yStart; j < yStart + ySize; j++)
                {
                    cells.Add(_cells[i, j]);
                }
            }

            return cells;
        }

        private  List<Cell> GetFreeCells(int x, int y, int width, int height)
        {
            List<Cell> cells = GetBlockCells(x, y, width, height);
            if (cells.Count == 0) return null;
            if (cells.All(v => v.Value == 0)) return cells;
            return null;
        }

        private List<Cell> GetAvailableCells(int x, int y, int width, int height, int weight)
        {
            List<Cell> cells = GetBlockCells(x, y, width, height);
            if (cells.Count == 0) return null;
            var head = cells.First();
            if (cells.All(v =>
                    v.FreeSpace >= weight &&
                    v.FreeSpace == head.FreeSpace &&
                    v.Id == head.Id &&
                    v.HashSlot == head.HashSlot)) 
                return cells;
            
            
            return null;
        }
    }

    public class Cell : IDisposable
    {
        private readonly int _capacity;
     
        private int _value;
        private int _hashSlot;

        public int Capacity => _capacity / Weight;
        public int Value => _value;

        public int FreeSpace => Capacity - Value;

        public string Id { get; private set; }

        public int HashSlot => _hashSlot;
        
        public Vector2Int Position { get; private set; }

        

        public int Weight { get; private set; } = 1;

        public Cell(int capacity, Vector2Int position)
        {
            Position = position;
            _capacity = capacity;
            _value = 0;
            _hashSlot = -1;
            Id = string.Empty;
            Weight = 1;
        }

        public void SetWeight(int weight)
        {
            Weight = weight;
        }

        public void SetHash(int hashSlot)
        {
            _hashSlot = hashSlot;
        }

        public void Dispose()
        {
            _hashSlot = -1;
            _value = 0;
            Weight = 1;
            Id = string.Empty;
        }

        public void SetId(string key)
        {
            Id = key;
        }

        public void AddValue(int value)
        {
            _value += value;
        }
    }
    
}