using System;
using UnityEngine;

namespace Models
{
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