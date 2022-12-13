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
        private PackingInventory _packingInventory;

        public InventoryModel(InventoryConfig inventoryConfig)
        {
            _inventoryConfig = inventoryConfig;
            _containerSlots = new Dictionary<string, List<SlotItem>>();
            
            CreateNewMatrix();
            _matrix.Log();
        }

        public bool CanAdditem(InventoryItem inventoryItem)
        {
            var canStackItem = CanStackItem(inventoryItem);
            if (canStackItem) return true;

            var canPlaceInMatrix = CanPlaceInMatrix(inventoryItem);
            if (canPlaceInMatrix) return true;
            
            CreateNewMatrix();
            _packingInventory.TryPacking();

            return false;
        }

        private void CreateNewMatrix()
        {
            var size = _inventoryConfig.Size;
            _matrix = new Matrix(size.x, size.y, size.z);
        }

        private bool CanPlaceInMatrix(InventoryItem inventoryItem)
        {
            var groupCells = _matrix.FindAvailableCells(inventoryItem.Size, inventoryItem.Weight);
            return groupCells != null;
        }

        private bool CanStackItem(InventoryItem inventoryItem)
        {
            var slot = GetSlot(inventoryItem,v => v.CanAdd(inventoryItem));
            return slot != null;
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