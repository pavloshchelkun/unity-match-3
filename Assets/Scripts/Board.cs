using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class Board : MonoBehaviour
    {
        public int rows = 12;
        public int columns = 8;
        public int minMatches = 3;

        private Item[,] items;

        private Item lastSwappedItem1;
        private Item lastSwappedItem2;

        public Item this[int row, int column]
        {
            get { return items[row, column]; }
            set { items[row, column] = value; }
        }

        private void Start()
        {
            items = new Item[rows, columns];
        }

        public void Swap(Item item1, Item item2)
        {
            lastSwappedItem1 = item1;
            lastSwappedItem2 = item2;

            int item1Row = item1.Row;
            int item1Col = item1.Column;

            int item2Row = item2.Row;
            int item2Col = item2.Column;

            items[item1Row, item1Col] = item2;
            items[item2Row, item2Col] = item1;

            item1.SetPosition(item2Row, item2Col);
            item2.SetPosition(item1Row, item1Col);
        }

        public void UndoLastSwap()
        {
            Swap(lastSwappedItem1, lastSwappedItem2);
        }

        public IEnumerable<Item> GetMatches(IEnumerable<Item> itemArray)
        {
            List<Item> matches = new List<Item>();

            foreach (var item in itemArray)
            {
                matches.AddRange(GetMatch(item).Items);
            }

            return matches.ToArray();
        }

        public Match GetMatch(Item item)
        {
            Match match = new Match();

            match.AddItemRange(GetMatchesHorizontally(item));
            match.AddItemRange(GetMatchesVertically(item));

            return match;
        }

        private IEnumerable<Item> GetMatchesHorizontally(Item item)
        {
            List<Item> matches = new List<Item>();
            matches.Add(item);

            //Left items
            if (item.Column != 0)
            {
                for (int column = item.Column - 1; column >= 0; column--)
                {
                    Item leftItem = items[item.Row, column];
                    if (leftItem.IsEqual(item))
                    {
                        matches.Add(leftItem);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            //Right items
            if (item.Column != columns - 1)
            {
                for (int column = item.Column + 1; column < columns; column++)
                {
                    Item rightItem = items[item.Row, column];
                    if (rightItem.IsEqual(item))
                    {
                        matches.Add(rightItem);
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

            return matches.ToArray();
        }

        private IEnumerable<Item> GetMatchesVertically(Item item)
        {
            List<Item> matches = new List<Item>();
            matches.Add(item);

            //Bottom items
            if (item.Row != 0)
            {
                for (int row = item.Row - 1; row >= 0; row--)
                {
                    Item bottomItem = items[row, item.Column];
                    if (bottomItem.IsEqual(item))
                    {
                        matches.Add(bottomItem);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            //Top items
            if (item.Row != columns - 1)
            {
                for (int row = item.Row + 1; row < rows; row++)
                {
                    Item topItem = items[row, item.Column];
                    if (topItem.IsEqual(item))
                    {
                        matches.Add(topItem);
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

            return matches.ToArray();
        }

        private void Remove(Item item)
        {
            items[item.Row, item.Column] = null;
        }

        public Collapse CollapseColumns(IEnumerable<int> columnArray)
        {
            Collapse collapse = new Collapse();

            foreach (var column in columnArray)
            {
                for (int row = 0; row < rows - 1; row++)
                {
                    if (items[row, column] == null)
                    {
                        for (int row2 = row + 1; row2 < rows; row2++)
                        {
                            if (items[row2, column] != null)
                            {
                                Item item = items[row2, column];

                                items[row, column] = item;
                                items[row2, column] = null;
                                
                                collapse.MaxDistance = Mathf.Max(row2 - row, collapse.MaxDistance);

                                item.SetPosition(row, column);

                                collapse.AddItem(item);
                                break;
                            }
                        }
                    }
                }
            }

            return collapse;
        }
    }
}
