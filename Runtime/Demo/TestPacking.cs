using System.Collections.Generic;
using Configs;
using Models;
using NaughtyAttributes;
using UnityEngine;

namespace Demo
{
    public class TestPacking : MonoBehaviour
    {
        private List<SlotItem> _slotItems;

        private List<SlotItem> SlotItems
        {
            get
            {
                if (_slotItems == null)
                    _slotItems = new List<SlotItem>();
                return _slotItems;
            }
        }

        [SerializeField] private InventoryConfig _inventoryConfig;
        private PackingInventory _packingInventory;

        [SerializeField] private InventoryItemConfig _item_5x2;
        [SerializeField] private InventoryItemConfig _item_4x1;
        [SerializeField] private InventoryItemConfig _item_3x1;
        [SerializeField] private InventoryItemConfig _item_2x2;
        [SerializeField] private InventoryItemConfig _item_2x1;
        [SerializeField] private InventoryItemConfig _item_1x3;
        [SerializeField] private InventoryItemConfig _item_1x1;

        private void Awake()
        {
            _packingInventory =
                new PackingInventory(_inventoryConfig.Size.x, _inventoryConfig.Size.y, SlotItems);
        }


        private SlotItem Create(InventoryItemConfig config)
        {
            return new SlotItem(new InventoryItem(config), config.Weight,
                new List<Cell>() { new Cell(5, new Vector2Int(0, 0)) });
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
            
            Add1x3();
            Add1x3();
            

        }
        private void Add()
        {
            _packingInventory =
                new PackingInventory(_inventoryConfig.Size.x, _inventoryConfig.Size.y, SlotItems);
            if (_packingInventory.TryPacking())
            {
                Debug.LogError("Successfully");
            }
            else
            {
                Debug.LogError("Error");
            }
        }

        [Button()]
        private void Refresh()
        {
            SlotItems.Clear();
        }
        
        [Button()]
        private void Add5x2()
        {
            SlotItems.Add(Create(_item_5x2));
            Add();
        }
        [Button()]
        private void Add4x1()
        {
            SlotItems.Add(Create(_item_4x1));
            Add();
        }
        [Button()]
        private void Add3x1()
        {
            SlotItems.Add(Create(_item_3x1));
            Add();
        }
        [Button()]
        private void Add2x2()
        {
            SlotItems.Add(Create(_item_2x2));
            Add();
        }
        [Button()]
        private void Add2x1()
        {
            SlotItems.Add(Create(_item_2x1));
            Add();
        }
        [Button()]
        private void Add1x3()
        {
            SlotItems.Add(Create(_item_1x3));
            Add();
        }
        [Button()]
        private void Add1x1()
        {
            SlotItems.Add(Create(_item_1x1));
            Add();
        }
    }
}