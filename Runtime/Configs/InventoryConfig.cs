using UnityEngine;

[CreateAssetMenu(fileName = "InventoryConfig", menuName = "Configs/Inventory/Inventory")]
public class InventoryConfig : ScriptableObject
{
    [SerializeField] private Vector3Int _size = new Vector3Int(10, 10, 1);

    public Vector3Int Size => _size;
}