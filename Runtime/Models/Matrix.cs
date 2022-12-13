using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Models
{
    public class Matrix
    {
        private readonly int _z;
        private Cell[,] _cells;
        
        public Matrix(int x, int y, int z)
        {
            _z = z;
            _cells = new Cell[x, y];
            
        }
        public void Init()
        {
            Foreach((i, j) =>
            {
                _cells[i, j] = new Cell(_z, new Vector2Int(i, j));
            });
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
            if (cells.All(v => v.Id == head.Id &&
                               v.FreeSpace >= weight &&
                               v.FreeSpace == head.FreeSpace &&
                               v.HashSlot == head.HashSlot)) 
                return cells;

            return null;
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

        public void InitAfterRepackingSlots(List<SlotItemInfo> repackingSlots)
        {
            foreach (var repackingSlot in repackingSlots)
            {
                var indexStartX = repackingSlot.IndexStartX;
                var indexStartY = repackingSlot.IndexStartY;
                
                var width = repackingSlot.Width;
                var height = repackingSlot.Height;

                var indexEndX = indexStartX + width;
                var indexEndY = indexStartY + height;

                List<Cell> cells = new List<Cell>();
                for (int i = indexStartX; i < indexEndX; i++)
                {
                    for (int j = indexStartY; j < indexEndY; j++)
                    {
                        _cells[i, j] = new Cell(_z, new Vector2Int(i, j));
                        cells.Add(_cells[i,j]);
                        
                    }
                }
                repackingSlot.SlotItem.AddNewCells(cells);
            }
            
            Foreach((i, j) =>
            {
                if(_cells[i,j] == null)
                    _cells[i,j] = new Cell(_z, new Vector2Int(i, j));
            });
        }
    }
}