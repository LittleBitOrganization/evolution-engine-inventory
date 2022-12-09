using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(fileName = "InventoryItem", menuName = "Configs/Inventory/Item")]
    public class InventoryItemConfig: ScriptableObject
    {
        [SerializeField] private Vector2Int _sizeCell = new Vector2Int(1, 1);
        [SerializeField] private int _weight = 1;
        [SerializeField] private string _key;

        public Vector2Int Size => _sizeCell;

        public int Weight => _weight;

        public string Key => _key;
    }
}