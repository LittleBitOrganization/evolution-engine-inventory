
using System;
using Configs;
using Models;
using NaughtyAttributes;
using UnityEngine;

namespace Demo
{
    public class TestInstaller : MonoBehaviour
    {

        [SerializeField] private InventoryConfig _inventoryConfig;
        [SerializeField] private InventoryItemConfig _inventoryItem1;
        [SerializeField] private InventoryItemConfig _inventoryItem2;
        [SerializeField] private InventoryItemConfig _inventoryItem3;

        private InventoryModel _model;
        private PackingInventory _packingInventory;


        private void Awake()
        {
            _model = new InventoryModel(_inventoryConfig);
            
           
        }

        [Button()]
        private void AddItem1()
        {
            _model.TryAddInventoryItem(new InventoryItem(_inventoryItem1));
            
        }

        [Button()]
        private void RemoveItem1()
        {
            _model.RemoveInventoryItem(new InventoryItem(_inventoryItem1));
        }
        
        [Button()]
        private void AddItem2()
        {
            _model.TryAddInventoryItem(new InventoryItem(_inventoryItem2));
            
        }

        [Button()]
        private void RemoveItem2()
        {
            _model.RemoveInventoryItem(new InventoryItem(_inventoryItem2));
        }
        
        [Button()]
        private void AddItem3()
        {
            _model.TryAddInventoryItem(new InventoryItem(_inventoryItem3));
        }

        [Button()]
        private void RemoveItem3()
        {
            _model.RemoveInventoryItem(new InventoryItem(_inventoryItem3));
        }
    }
}