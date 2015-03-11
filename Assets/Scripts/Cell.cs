using UnityEngine;

namespace Assets.Scripts
{
    public class Cell
    {
        public int Row { get; private set; }
        public int Column { get; private set; }

        public Item Item { get; private set; }

        public bool IsEmpty { get { return Item == null; } }

        public Cell(int row, int col)
        {
            Row = row;
            Column = col;
        }

        public void SetItem(Item item)
        {
            Item = item;
            Item.SetCell(this);
        }

        public bool IsMatched(Cell cell)
        {
            return !IsEmpty && !cell.IsEmpty && Item.IsEqual(cell.Item);
        }

        public void Clear()
        {
            Item = null;
        }
    }
}
