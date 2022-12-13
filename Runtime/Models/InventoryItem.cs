using Configs;
using UnityEngine;

namespace Models
{
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
}