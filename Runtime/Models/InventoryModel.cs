using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Models
{
    public class InventoryModel
    {
        private readonly InventoryConfig _inventoryConfig;
        private Matrix _matrix;

        private Dictionary<string, List<SlotItem>> _containerSlots;

        public event Action<List<SlotItem>> OnRepacking;

        public InventoryModel(InventoryConfig inventoryConfig)
        {
            _inventoryConfig = inventoryConfig;
            _containerSlots = new Dictionary<string, List<SlotItem>>();
            
            CreateNewMatrix();
            _matrix.Init();
            _matrix.Log();
        }

        
        private void CreateNewMatrix()
        {
            var size = _inventoryConfig.Size;
            _matrix = new Matrix(size.x, size.y, size.z);
        }

        private bool CanPlaceInMatrix(InventoryItem inventoryItem, out List<Cell> cells)
        {
            cells = _matrix.FindAvailableCells(inventoryItem.Size, inventoryItem.Weight);
            return cells != null;
        }

        private bool CanStackItem(InventoryItem inventoryItem, out SlotItem slot)
        {
            slot = GetFirstSlot(inventoryItem,v => v.CanAdd(inventoryItem));
            return slot != null;
        }

        private bool RepackingInventory(out PackingInventory inventory)
        {
            var slots = new List<SlotItem>();
            foreach (var list in _containerSlots.Values)
            {
                slots.AddRange(list);
            }
            
            inventory = new PackingInventory(_inventoryConfig.Size.x, _inventoryConfig.Size.y, slots);;
            var canRepacking = inventory.CheckPackingAndPrepare();
            CreateNewMatrix();
            inventory.Packing(_matrix);
            

            return canRepacking;

        }

        
        // private bool CanAddItem(InventoryItem inventoryItem)
        // {
        //     if (CanStackItem(inventoryItem, out _)) return true;
        //     if (CanPlaceInMatrix(inventoryItem, out _)) return true;
        //
        //     if (CanRepackingItem(out var packing))
        //     {
        //         CreateNewMatrix();
        //         packing.Packing(_matrix);
        //         
        //        
        //         
        //         OnRepacking?.Invoke(slots);
        //         if (CanPlaceInMatrix(inventoryItem, out _))
        //         {
        //             return true;
        //         }
        //     }
        //     
        //     return false;
        // }
        
        public bool TryAddInventoryItem(InventoryItem inventoryItem, out SlotItem slotItem, bool tryRepack = false)
        {
            slotItem = null;
            if (CanStackItem(inventoryItem, out var slot))
            {
                slot.TryAdd(inventoryItem, 1);
                slotItem = slot;
                _matrix.Log();
                return true;
            }

            if (CanPlaceInMatrix(inventoryItem, out var cells))
            {
                slot = CreateSlot(inventoryItem, cells);
                slot.TryAdd(inventoryItem, 1);
                slotItem = slot;
                _matrix.Log();
                return true;
            }
            
            if (tryRepack)
            {
                var canRepacking = RepackingInventory(out var packingInventory);
                Debug.LogError("Repacking");
                _matrix.Log();
                var slots = new List<SlotItem>();
                foreach (var list in _containerSlots.Values)
                {
                    slots.AddRange(list);
                }
              
                
                if (CanPlaceInMatrix(inventoryItem, out cells))
                {
                    slot = CreateSlot(inventoryItem, cells);
                    slot.TryAdd(inventoryItem, 1);
                    slotItem = slot;
                    _matrix.Log();
                    OnRepacking?.Invoke(slots);
                    return true;
                }
                else
                {
                    OnRepacking?.Invoke(slots);
                }
                
            }

            _matrix.Log();

            return false;

        }

        public void RemoveInventoryItem(InventoryItem inventoryItem)
        {
            SlotItem slot = GetLastSlot(inventoryItem, v => v.CanRemove(inventoryItem, 1));
            if (slot == null)
            {
                Debug.LogError("Cannot remove item. Not found slots with key: " + inventoryItem.Key);
                return;
            }
            slot.TryRemove(inventoryItem, 1);
            _matrix.Log();
        }

        private SlotItem GetLastSlot(InventoryItem inventoryItem, Func<SlotItem, bool> predicate)
        {
            var slots = GetSlots(inventoryItem);
            if (slots.Count > 0)
            {
                var slot = slots.LastOrDefault(predicate);
                if (slot != null)
                {
                    Debug.LogError("Slot not found");
                    return slot;
                }
            }

            return null;
        }

        private SlotItem GetFirstSlot(InventoryItem inventoryItem, Func<SlotItem, bool> predicate)
        {
            var slots = GetSlots(inventoryItem);
            if (slots.Count > 0)
            {
                var slot = slots.FirstOrDefault(predicate);
                if (slot != null)
                {
                    Debug.LogError("Get exist slot");
                    return slot;
                }
            }

            return null;
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
}