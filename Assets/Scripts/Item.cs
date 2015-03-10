using UnityEngine;

namespace Assets.Scripts
{
    public class Item : MonoBehaviour
    {
        public string type;

        public Cell Cell { get; private set; }

        public void SetCell(Cell cell)
        {
            Cell = cell;
        }

        public bool IsEqual(Item item)
        {
            return item != null && item.type == type;
        }
    }
}
