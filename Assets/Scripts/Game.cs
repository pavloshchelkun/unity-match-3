using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class Game : MonoBehaviour
    {
        public enum State
        {
            Default,
            Swiping,
            Updating
        }

        public Board board;
        public Text scoreText;
        public Item[] itemPrefabs;
        public GameObject[] effectPrefabs;
        public float swapDuration;
        public float moveDuration;
        public float effectDuration;
        public int matchScore = 100;

        private Vector2[] spawnPoints;
        private State state = State.Default;
        private Item hitItem;

        private int score;

        private void Start()
        {
            Restart();
        }

        public void Restart()
        {
            score = 0;
            GenerateItems();
            UpdateScore();
        }

        private void GenerateItems()
        {
            for (int row = 0; row < board.rows; row++)
            {
                for (int col = 0; col < board.columns; col++)
                {
                    if (!board[row, col].IsEmpty)
                    {
                        Destroy(board[row, col].Item.gameObject);
                    }
                }
            }

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

        private GameObject GetRandomEffect()
        {
            return effectPrefabs[Random.Range(0, effectPrefabs.Length)];
        }

        private void CreateItem(int row, int col, Item itemPrefab)
        {
            Vector2 position = board.bottomRight + (new Vector2(col * board.cellSize.x, row * board.cellSize.y));
            Item item = ((GameObject)Instantiate(itemPrefab.gameObject, position, Quaternion.identity)).GetComponent<Item>();

            item.transform.SetParent(board.transform);

            board[row, col].SetItem(item);
        }

        private void DestroyItem(Item item)
        {
            item.Cell.Clear();
            
            GameObject effect = GetRandomEffect();
            Destroy(Instantiate(effect, item.transform.position, Quaternion.identity), effectDuration);
            Destroy(item.gameObject);
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
                            state = State.Swiping;
                        }
                    }
                    break;
                case State.Swiping:
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

        private IEnumerator FindMatchesAndCollapse(RaycastHit2D hit2)
        {
            //get the second item that was part of the swipe
            Item hitItem2 = hit2.collider.gameObject.GetComponent<Item>();
            board.Swap(hitItem.Cell, hitItem2.Cell);

            //animate swapping
            hitItem.transform.TweenPosition(swapDuration, hitItem2.transform.localPosition);
            hitItem2.transform.TweenPosition(swapDuration, hitItem.transform.localPosition);
            yield return new WaitForSeconds(swapDuration);

            //get the matches
            var hitItemMatch = board.GetMatch(hitItem.Cell);
            var hitItem2Match = board.GetMatch(hitItem2.Cell);

            //gather matches in one list
            var totalMatches = hitItemMatch.Cells.Union(hitItem2Match.Cells).Distinct();

            //if user's swap didn't create at least a min matches, undo their swap
            if (totalMatches.Count() < board.minMatches)
            {
                hitItem.transform.TweenPosition(swapDuration, hitItem2.transform.localPosition);
                hitItem2.transform.TweenPosition(swapDuration, hitItem.transform.localPosition);
                yield return new WaitForSeconds(swapDuration);

                board.UndoLastSwap();
            }

            while (totalMatches.Count() >= board.minMatches)
            {
                AddScore((totalMatches.Count() - board.minMatches + 1) * matchScore);

                audio.Play();

                foreach (var cell in totalMatches)
                {
                    DestroyItem(cell.Item);
                }

                var columns = totalMatches.Select(cell => cell.Column).Distinct();

                //the order the 2 methods below get called is important!!!
                //collapse the ones gone
                var collapse = board.CollapseColumns(columns);
                //create new ones
                var newItems = GenerateNewItems(columns);

                int maxDistance = Mathf.Max(collapse.MaxDistance, newItems.MaxDistance);

                MoveAndAnimate(newItems.Cells, maxDistance);
                MoveAndAnimate(collapse.Cells, maxDistance);

                //will wait for both of the above animations
                yield return new WaitForSeconds(moveDuration * maxDistance);

                totalMatches = board.GetMatches(collapse.Cells).Union(board.GetMatches(newItems.Cells)).Distinct();
            }
            
            state = State.Default;
        }

        private void MoveAndAnimate(IEnumerable<Cell> cells, int distance)
        {
            foreach (var cell in cells)
            {
                cell.Item.transform.TweenPosition(moveDuration * distance, board.bottomRight + new Vector2(cell.Column * board.cellSize.x, cell.Row * board.cellSize.y));
            }
        }

        private Collapse GenerateNewItems(IEnumerable<int> columns)
        {
            var collapse = new Collapse();

            //find how many null values the column has
            foreach (int column in columns)
            {
                var emptyCells = board.GetEmptyCellsOnColumn(column);
                foreach (var cell in emptyCells)
                {
                    var prefab = GetRandomItemPrefab();
                    var item = ((GameObject)Instantiate(prefab.gameObject, spawnPoints[column], Quaternion.identity)).GetComponent<Item>();

                    item.transform.SetParent(board.transform);

                    cell.SetItem(item);

                    if (board.rows - cell.Row > collapse.MaxDistance)
                    {
                        collapse.MaxDistance = board.rows - cell.Row;
                    }

                    collapse.AddCell(cell);
                }
            }

            return collapse;
        }

        private void AddScore(int points)
        {
            score += points;
            UpdateScore();
        }

        private void UpdateScore()
        {
            scoreText.text = "Score: " + score;
        }
    }
}
