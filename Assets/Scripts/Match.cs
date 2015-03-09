using System.Collections.Generic;

namespace Assets.Scripts
{
    public class Match
    {
        private List<Item> items = new List<Item>();

        public IEnumerable<Item> Items
        {
            get { return items.ToArray(); }
        }

        public void AddItem(Item item)
        {
            if (!items.Contains(item))
            {
                items.Add(item);
            }
        }

        public void AddItemRange(IEnumerable<Item> itemArray)
        {
            foreach (var item in itemArray)
            {
                AddItem(item);
            }
        }
    }
}