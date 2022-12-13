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
            slot = GetSlot(inventoryItem,v => v.CanAdd(inventoryItem));
            return slot != null;
        }

        private bool CanRepackingItem(InventoryItem inventoryItem, out PackingInventory inventory)
        {
            var packingInventory = CreatePackingInventory();
            inventory = packingInventory;
            return packingInventory.CheckPackingAndPrepare();
        }
        
        private bool CanAddItem(InventoryItem inventoryItem)
        {
            if (CanStackItem(inventoryItem, out _)) return true;
            if (CanPlaceInMatrix(inventoryItem, out _)) return true;

            if (CanRepackingItem(inventoryItem, out var packing))
            {
                CreateNewMatrix();
                packing.Packing(_matrix);

                if (CanPlaceInMatrix(inventoryItem, out _)) return true;
            }
            
            return false;
        }

        private PackingInventory CreatePackingInventory()
        {
            var slots = new List<SlotItem>();
            foreach (var list in _containerSlots.Values)
            {
                slots.AddRange(list);
            }
            return new PackingInventory(_inventoryConfig.Size.x, _inventoryConfig.Size.y, slots);;
        }

        public void TryAddInventoryItem(InventoryItem inventoryItem)
        {
            if (CanStackItem(inventoryItem, out var slot))
            {
                slot.TryAdd(inventoryItem, 1);
            }
            else if (CanPlaceInMatrix(inventoryItem, out var cells))
            {
                slot = CreateSlot(inventoryItem, cells);
                slot.TryAdd(inventoryItem, 1);
            }
            else if (CanRepackingItem(inventoryItem, out _))
            {
                if (CanPlaceInMatrix(inventoryItem, out cells))
                {
                    slot = CreateSlot(inventoryItem, cells);
                    slot.TryAdd(inventoryItem, 1);
                }
            }

            _matrix.Log();
            
            // if (slot == null)
            // {
            //     var groupCells = _matrix.FindAvailableCells(inventoryItem.Size, inventoryItem.Weight);
            //     if (groupCells == null)
            //     {
            //         Debug.LogError("GroupCells not found");
            //         return;
            //     }
            //
            //     slot = CreateSlot(inventoryItem, groupCells);
            //    
            // }
            //
            // if(slot != null)
            
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

        private SlotItem GetSlot(InventoryItem inventoryItem, Func<SlotItem, bool> predicate)
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
            Debug.LogError("Get null slot");

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