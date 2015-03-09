using UnityEngine;

namespace Assets.Scripts
{
    public class Item : MonoBehaviour
    {
        public string Type { get; private set; }
        public int Column { get; private set; }
        public int Row { get; private set; }

        public void Init(string type, int col, int row)
        {
            Type = type;
            SetPosition(col, row);
        }

        public void SetPosition(int col, int row)
        {
            Column = col;
            Row = row;
        }

        public bool IsEqual(Item item)
        {
            return item.Type == Type;
        }
    }
}
