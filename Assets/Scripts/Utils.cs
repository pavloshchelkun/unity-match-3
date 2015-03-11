using UnityEngine;

namespace Assets.Scripts
{
    public static class Utils
    {
        public static void TweenPosition(this Transform transform, float duration, Vector3 position)
        {
            Tweener tweener = transform.GetComponent<Tweener>() ?? transform.gameObject.AddComponent<Tweener>();
            tweener.TweenPosition(duration, position);
        }

        //Checks if an item is next to another one, either horizontally or vertically
        public static bool AreVerticalOrHorizontalNeighbors(Item item1, Item item2)
        {
            return 
                (item1.Cell.Column == item2.Cell.Column || item1.Cell.Row == item2.Cell.Row) && 
                Mathf.Abs(item1.Cell.Column - item2.Cell.Column) <= 1 && 
                Mathf.Abs(item1.Cell.Row - item2.Cell.Row) <= 1;
        }
    }
}
