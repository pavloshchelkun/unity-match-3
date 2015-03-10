using System.Collections.Generic;

namespace Assets.Scripts
{
    public class Match
    {
        private List<Cell> cells = new List<Cell>();

        public IEnumerable<Cell> Cells
        {
            get { return cells.ToArray(); }
        }

        public void AddCell(Cell cell)
        {
            if (!cells.Contains(cell))
            {
                cells.Add(cell);
            }
        }

        public void AddCellRange(IEnumerable<Cell> cellArray)
        {
            foreach (var cell in cellArray)
            {
                AddCell(cell);
            }
        }
    }
}