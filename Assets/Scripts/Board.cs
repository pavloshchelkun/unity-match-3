using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    public class Board : MonoBehaviour
    {
        public Vector2 bottomRight = new Vector2(-2.37f, -4.27f);
        public Vector2 cellSize = new Vector2(0.7f, 0.7f);
        public int rows = 12;
        public int columns = 8;
        public int minMatches = 3;

        private Cell[,] cells;

        private Cell lastSwappedCell1;
        private Cell lastSwappedCell2;

        public Cell this[int row, int column]
        {
            get { return cells[row, column]; }
            set { cells[row, column] = value; }
        }

        private void Awake()
        {
            cells = new Cell[rows, columns];

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    cells[row, col] = new Cell(row, col);
                }
            }
        }

        public void Swap(Cell cell1, Cell cell2)
        {
            lastSwappedCell1 = cell1;
            lastSwappedCell2 = cell2;

            Item item1 = cell1.Item;
            cell1.SetItem(cell2.Item);
            cell2.SetItem(item1);
        }

        public void UndoLastSwap()
        {
            Swap(lastSwappedCell1, lastSwappedCell2);
        }

        public IEnumerable<Cell> GetMatches(IEnumerable<Cell> cellArray)
        {
            List<Cell> matches = new List<Cell>();

            foreach (var cell in cellArray)
            {
                matches.AddRange(GetMatch(cell).Cells);
            }

            return matches.Distinct();
        }

        public Match GetMatch(Cell cell)
        {
            Match match = new Match();

            match.AddCellRange(GetMatchesHorizontally(cell));
            match.AddCellRange(GetMatchesVertically(cell));

            return match;
        }

        private IEnumerable<Cell> GetMatchesHorizontally(Cell cell)
        {
            List<Cell> matches = new List<Cell>();
            matches.Add(cell);

            //Left items
            if (cell.Column != 0)
            {
                for (int column = cell.Column - 1; column >= 0; column--)
                {
                    Cell left = cells[cell.Row, column];
                    if (left.IsMatched(cell))
                    {
                        matches.Add(left);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            //Right items
            if (cell.Column != columns - 1)
            {
                for (int column = cell.Column + 1; column < columns; column++)
                {
                    Cell right = cells[cell.Row, column];
                    if (right.IsMatched(cell))
                    {
                        matches.Add(right);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            //Check for minimum matches
            if (matches.Count < minMatches)
            {
                matches.Clear();
            }

            return matches.Distinct();
        }

        private IEnumerable<Cell> GetMatchesVertically(Cell cell)
        {
            List<Cell> matches = new List<Cell>();
            matches.Add(cell);

            //Bottom items
            if (cell.Row != 0)
            {
                for (int row = cell.Row - 1; row >= 0; row--)
                {
                    Cell bottom = cells[row, cell.Column];
                    if (bottom.IsMatched(cell))
                    {
                        matches.Add(bottom);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            //Top items
            if (cell.Row != columns - 1)
            {
                for (int row = cell.Row + 1; row < rows; row++)
                {
                    Cell top = cells[row, cell.Column];
                    if (top.IsMatched(cell))
                    {
                        matches.Add(top);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            //Check for minimum matches
            if (matches.Count < minMatches)
            {
                matches.Clear();
            }

            return matches.Distinct();
        }

        public Collapse CollapseColumns(IEnumerable<int> columnArray)
        {
            Collapse collapse = new Collapse();

            //search in every column
            foreach (var column in columnArray)
            {
                //begin from bottom row
                for (int row = 0; row < rows - 1; row++)
                {
                    //if you find a null item
                    if (cells[row, column].IsEmpty)
                    {
                        //start searching for the first non-null
                        for (int row2 = row + 1; row2 < rows; row2++)
                        {
                            //if you find one, bring it down (i.e. replace it with the null you found)
                            if (!cells[row2, column].IsEmpty)
                            {
                                Cell cell1 = cells[row, column];
                                Cell cell2 = cells[row2, column];

                                //assign the item
                                cell1.SetItem(cell2.Item);
                                cell2.Clear();

                                //calculate the biggest distance
                                collapse.MaxDistance = Mathf.Max(row2 - row, collapse.MaxDistance);

                                collapse.AddCell(cell1);
                                break;
                            }
                        }
                    }
                }
            }

            return collapse;
        }

        public IEnumerable<Cell> GetEmptyCellsOnColumn(int column)
        {
            List<Cell> emptyCells = new List<Cell>();

            for (int row = 0; row < rows; row++)
            {
                if (cells[row, column].IsEmpty)
                {
                    emptyCells.Add(cells[row, column]);
                }
            }

            return emptyCells;
        }

        public void Clear()
        {
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    cells[row, col].Clear();
                }
            }
        }
    }
}
