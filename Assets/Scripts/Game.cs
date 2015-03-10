using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class Game : MonoBehaviour
    {
        public enum State
        {
            Default,
            Swapping,
            Updating
        }

        public Board board;
        public Item[] itemPrefabs;

        private Vector2[] spawnPoints;
        private State state;
        private Item hitItem;

        private void Start()
        {
            GenerateItems();
        }

        private void GenerateItems()
        {
            board.Clear();

            spawnPoints = new Vector2[board.columns];

            for (int row = 0; row < board.rows; row++)
            {
                for (int col = 0; col < board.columns; col++)
                {
                    Item itemPrefab = GetRandomItemPrefab();

                    //check if two previous horizontal items are of the same type and if so, get another random item
                    while (col >= 2 && board[row, col - 1].Item.IsEqual(itemPrefab) && board[row, col - 2].Item.IsEqual(itemPrefab))
                    {
                        itemPrefab = GetRandomItemPrefab();
                    }

                    //check if two previous vertical items are of the same type and if so, get another random item
                    while (row >= 2 && board[row - 1, col].Item.IsEqual(itemPrefab) && board[row - 2, col].Item.IsEqual(itemPrefab))
                    {
                        itemPrefab = GetRandomItemPrefab();
                    }

                    CreateItem(row, col, itemPrefab);
                }
            }

            InitSpawnPoints();
        }

        private Item GetRandomItemPrefab()
        {
            return itemPrefabs[Random.Range(0, itemPrefabs.Length)];
        }

        private void CreateItem(int row, int col, Item itemPrefab)
        {
            Vector2 position = board.bottomRight + (new Vector2(col * board.cellSize.x, row * board.cellSize.y));
            Item item = ((GameObject)Instantiate(itemPrefab.gameObject, position, Quaternion.identity)).GetComponent<Item>();

            item.transform.SetParent(board.transform);

            board[row, col].SetItem(item);
        }

        private void InitSpawnPoints()
        {
            //initialize spawn points for the new items
            for (int column = 0; column < board.columns; column++)
            {
                spawnPoints[column] = board.bottomRight + new Vector2(column * board.cellSize.x, board.rows * board.cellSize.y);
            }
        }

        private void Update()
        {
            switch (state)
            {
                case State.Default:
                    if (Input.GetMouseButtonDown(0))
                    {
                        var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

                        if (hit.collider != null)
                        {
                            hitItem = hit.collider.GetComponent<Item>();
                            state = State.Swapping;
                        }
                    }
                    break;
                case State.Swapping:
                    if (Input.GetMouseButton(0))
                    {
                        var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

                        if (hit.collider != null && hitItem.gameObject != hit.collider.gameObject)
                        {
                            //if the two items are diagonally aligned (different row and column), just return
                            if (!Utils.AreVerticalOrHorizontalNeighbors(hitItem, hit.collider.gameObject.GetComponent<Item>()))
                            {
                                state = State.Default;
                            }
                            else
                            {
                                state = State.Updating;
                                FixSortingLayer(hitItem.gameObject, hit.collider.gameObject);
                                StartCoroutine(FindMatchesAndCollapse(hit));
                            }
                        }
                    }
                    break;
            }
        }

        private void FixSortingLayer(GameObject hitGo, GameObject hitGo2)
        {
            SpriteRenderer sp1 = hitGo.GetComponent<SpriteRenderer>();
            SpriteRenderer sp2 = hitGo2.GetComponent<SpriteRenderer>();

            if (sp1.sortingOrder <= sp2.sortingOrder)
            {
                sp1.sortingOrder = 1;
                sp2.sortingOrder = 0;
            }
        }

        private IEnumerator FindMatchesAndCollapse(RaycastHit2D hit)
        {
            throw new System.NotImplementedException();
        }
    }
}
