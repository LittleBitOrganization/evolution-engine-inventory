using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Models
{
    public class PackingInventory
    {
        private readonly int _width;
        private readonly int _height;
        private readonly List<SlotItem> _items;

        private int CurrentRow;


        public PackingInventory(int width, int height, List<SlotItem> items)
        {
            _width = width;
            _height = height;
            _items = new List<SlotItem>();

            foreach (var item in items)
            {
                _items.Add(item);
            }
        }


        public bool TryPacking()
        {
            _items.Sort(new SlotItemComparer());

            CurrentRow = 0;
            
            while (true)
            {
                if (_height - CurrentRow < 0) return false;
                if(_items.Count == 0) return true;
                
                
                var sector = FillVerticalSector();
                CurrentRow += sector.Height;
            }
        }

        private Sector FillVerticalSector()
        {
            var mainSector = CreateNewSector();
            Queue<SlotItem> queue = new Queue<SlotItem>(_items);
            while (true)
            {
                if (queue.Count == 0)
                    break;

                if (mainSector.AllColumnsInited())
                    break;

                var item = queue.Dequeue();
                if (mainSector.TryAddToLeftDown(item))
                {
                    //TODO Изменить позицию слот итема
                   
                    _items.Remove(item);
                }
            }
            
            EmptySlotsInSector(mainSector);
            mainSector.Log();
            return mainSector;

        }

        private void EmptySlotsInSector(Sector mainSector)
        {
            if(_items.Count == 0) return;
            while (true)
            {
                if(mainSector.CanCreateSubSector() == false) break;
                if(_items.Count == 0) break;
               
                var subSector = mainSector.CreateSubSector();
                bool isChange = false;
                
                Queue<SlotItem> queue = new Queue<SlotItem>(_items);
                while (true)
                {
                    if (queue.Count == 0)
                    {
                        break;
                    }

                    if (subSector.AllColumnsCompleted())
                    {
                        break;
                    }
                      
                    var item = queue.Dequeue();
                    if (subSector.TryAddToLeftDown(item))
                    {
                        isChange = true;
                        _items.Remove(item);
                        Debug.LogError($"Add {item.Size}");
                    }
                }
                
                if(isChange)
                    mainSector.Join(subSector);
            }
        }


        private Sector CreateNewSector()
        {
            var firstItem = _items.First();
            var emptySpace = _height - CurrentRow;
            var height = Math.Min(firstItem.Size.y, emptySpace);
            
            return new Sector(_width, height, 0, CurrentRow);
        }
    }
    
    public class Sector
    {
        private readonly int _indexYStart;
        private readonly int _height;
        private readonly int _indexXStart;
        private int[]  _freeSpace;

        public int Height => _height;
        public Sector(int width, int height, int indexXStart, int indexYStart)
        {
            _indexYStart = indexYStart;
            _height = height;
            _indexXStart = indexXStart;
            _freeSpace = new int[width];
            for (int i = 0; i < _freeSpace.Length; i++)
            {
                _freeSpace[i] = height;
            }
        }

        public bool AllColumnsCompleted()
        {
            return _freeSpace.All(v => v == 0);
        }

        public bool AllColumnsInited()
        {
            return _freeSpace.All(v => v < _height);
        }

        public bool TryAddToLeftDown(SlotItem item)
        {
            var leftIndex = FindLeftIndexWithMaxFreeSpace();
            var widthElement = item.Size.x;
            var heightElement = item.Size.y;

            //Проверка на переполнение по горизонтали
            if (leftIndex + widthElement > _freeSpace.Length)
                return false;
            //Полвеока на переполнение по вертикали
            if (heightElement > _freeSpace[leftIndex])
                return false;

            for (int i = leftIndex; i < leftIndex + widthElement; i++)
            {
                _freeSpace[i] -= item.Size.y;
            }

            return true;
        }
        

        private int FindLeftIndexWithMaxFreeSpace()
        {
            int indexWithMaxFreeSpace = 0;
            int MaxValue() => _freeSpace[indexWithMaxFreeSpace];

            for (int index = 0; index < _freeSpace.Length; index++)
            {
                var freeSpace = _freeSpace[index];
                if (freeSpace > MaxValue())
                    indexWithMaxFreeSpace = index;
            }

            return indexWithMaxFreeSpace;
        }

        private int FindRightIndexWithMaxFreeSpace()
        {
            int indexWithMaxFreeSpace = _freeSpace.Length - 1;
            int MaxValue() => _freeSpace[indexWithMaxFreeSpace];

            for (int index = _freeSpace.Length - 1; index >= 0; index--)
            {
                var freeSpace = _freeSpace[index];
                if (freeSpace > MaxValue())
                {
                    indexWithMaxFreeSpace = index;
                }
            }

            return indexWithMaxFreeSpace;
        }

        public bool CanCreateSubSector()
        {
            return _freeSpace.Any(v => v > 0);
        }

        public Sector CreateSubSector()
        {
            var indexXStart = FindRightIndexWithMaxFreeSpace();
            var width = GetWidthEmptyRowOnLeftRelativeIndex(indexXStart);
            var height = GetHeightEmptyColOnTopRelativeIndex(indexXStart);
            

            var indexYStart = _height - _indexYStart - height;
            
            Debug.LogError($"CreateSubSector: [{indexXStart}, {indexYStart}], [{width}x{height}]");

            return new Sector(width, height, indexXStart - width + 1, indexYStart);

        }

        private int GetHeightEmptyColOnTopRelativeIndex(int index)
        {
            return _freeSpace[index];
        }

        private int GetWidthEmptyRowOnLeftRelativeIndex(int index)
        {
            var emptyValue = _freeSpace[index];
            int width = 0;

            for (int i = index; i >= 0; i--)
            {
                if (_freeSpace[i] == emptyValue)
                    width++;
                else
                    break;
            }

            return width;
        }

        public void Join(Sector subSector)
        {
            var subSectorFreeSpace = subSector._freeSpace;
            var subSectorHeight = subSector._height;

            for (int i = 0; i < subSectorFreeSpace.Length; i++)
            {
                var freeSpaceInSubSector = subSectorFreeSpace[i];
                int index = subSector._indexXStart + i;
                _freeSpace[index] -= subSectorHeight - freeSpaceInSubSector;
            }
        }

        public void Log()
        {
            
            string log = "";

            foreach (var free in _freeSpace)
            {
                log += free + " ";
            }
            Debug.LogError(log);
        }
    }

    internal class SlotItemComparer : IComparer<SlotItem>
    {
        public int Compare(SlotItem item1, SlotItem item2)
        {
            if (item1 == null) throw new Exception("X = null");
            if (item2 == null) throw new Exception("Y = null");

            if (item1.Size.y > item2.Size.y)
                return -1;

            if (item1.Size.y == item2.Size.y)
            {
                if (item1.Size.x > item2.Size.x)
                    return -1;
                if (item1.Size.x < item2.Size.x)
                    return 1;
                return 0;
            }

            return 1;
        }
    }
}