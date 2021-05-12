/*
 * Copyright (c) 2017 Razeware LLC
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    public static BoardManager instance;
    // public List<Sprite> characters = new List<Sprite>();
    public List<Sprite> candies = new List<Sprite>();
    public GameObject tilePrefab;
    public int xSize, ySize;

    public GameObject[,] tiles;
    private Tile previousSelected;
    private Vector2Int[] DIRECTIONS = new Vector2Int[] {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    public bool IsPlayingAnim { get; set; }

    void Start()
    {
        instance = GetComponent<BoardManager>();
        Sprite[,] randomGrid = GetRandomGrid(xSize, ySize);
        InitializeBoard(randomGrid);
    }

    public Sprite[,] GetRandomGrid(int columns, int rows)
    {
        Sprite[,] grid = new Sprite[columns, rows];

        Sprite previousLeft = null;
        Sprite[] previousAbove = new Sprite[rows];

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                List<Sprite> choice = new List<Sprite>();
                choice.AddRange(candies);
                choice.Remove(previousLeft);
                choice.Remove(previousAbove[x]);

                Sprite candyType = choice[Random.Range(0, choice.Count)];
                grid[x, y] = candyType;

                previousLeft = candyType;
                previousAbove[x] = candyType;
            }
        }

        return grid;
    }

    public void InitializeBoard(Sprite[,] grid)
    {
        xSize = grid.GetLength(0);
        ySize = grid.GetLength(1);

        Rect rect = tilePrefab.GetComponent<RectTransform>().rect;
        tiles = new GameObject[xSize, ySize];

        float startX = -((xSize / 2) * rect.width);
        float startY = ((ySize / 2) * rect.height);
        if ((xSize % 2) == 0)
            startX += rect.width / 2;
        if ((ySize % 2) == 0)
            startY -= rect.height / 2;

        DestroyChildren();
        for (int y = 0; y < ySize; y++)
        {
            for (int x = 0; x < xSize; x++)
            {
                GameObject newTile = Instantiate(tilePrefab, transform);
                Vector2 position = new Vector2(startX + (rect.width * x), startY - (rect.height * y));
                newTile.GetComponent<RectTransform>().localPosition = position;

                var tile = newTile.GetComponent<Tile>();
                tile.SetCandyType(grid[x, y]);
                tile.boardManager = this;
                tile.gridPos = new Vector2Int(x, y);

                tiles[x, y] = newTile;
            }
        }
    }

    public void InitializeBoard(string[,] charGrid)
    {
        Sprite[,] grid = new Sprite[charGrid.GetLength(0), charGrid.GetLength(1)];
        for (int y = 0; y < charGrid.GetLength(0); y++)
        {
            for (int x = 0; x < charGrid.GetLength(1); x++)
            {
                var letter = charGrid[y, x];
                if (letter == "B")
                    grid[x, y] = candies[0];
                else if (letter == "G")
                    grid[x, y] = candies[1];
                else if (letter == "M")
                    grid[x, y] = candies[2];
                else if (letter == "P")
                    grid[x, y] = candies[3];
                else if (letter == "R")
                    grid[x, y] = candies[4];
                else if (letter == "Y")
                    grid[x, y] = candies[5];
            }
        }

        InitializeBoard(grid);
    }

    public void DestroyChildren()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void OnTileClick(Tile tile)
    {
        if (tile.isSelected)
        {
            tile.Deselect();
            previousSelected = null;
            EventSystem.current.SetSelectedGameObject(null);
        }
        else if (previousSelected == null)
        {
            tile.Select();
            previousSelected = tile;
        }
        else
        {
            if (GetAllAdjacentTiles(tile.gridPos).Contains(previousSelected.gameObject))
            { // Is it an adjacent tile?
                SwapTile(tile, previousSelected);
                GUIManager.instance.MoveCounter--;
                StartCoroutine(ClearAdjacentTiles(tile, previousSelected));
                previousSelected = null;
                EventSystem.current.SetSelectedGameObject(null);
            }
            else
            {
                previousSelected.Deselect();
                tile.Select();
                previousSelected = tile;
            }
        }
    }

    public IEnumerator ClearAdjacentTiles(Tile tile, Tile previousSelected)
    {
        IsPlayingAnim = true;
        List<GameObject> matches = FindMatches(tile.gridPos);
        var anims1 = ClearMatches(matches);
        matches = FindMatches(previousSelected.gridPos, matches);
        var anims2 = ClearMatches(matches);
        yield return anims1;
        yield return anims2;
        ShiftTilesDown();
        ReplaceEmptyTiles();
        tile.Deselect();
        previousSelected.Deselect();
        IsPlayingAnim = false;
    }

    public void SwapTile(Tile a, Tile b)
    {
        Sprite candyType = a.GetCandyType();
        Vector2Int gridPos = a.gridPos;
        a.SetCandyType(b.GetCandyType());
        b.SetCandyType(candyType);

        // a.gridPos = b.gridPos;
        // b.gridPos = gridPos;
        // tiles[a.gridPos.x, a.gridPos.y] = a.gameObject;
        // tiles[b.gridPos.x, b.gridPos.y] = b.gameObject;
    }

    public List<GameObject> GetAllAdjacentTiles(Vector2Int gridPos)
    {
        List<GameObject> adjacent = new List<GameObject>();
        for (int i = 0; i < DIRECTIONS.Length; i++)
        {
            var pos = gridPos + DIRECTIONS[i];
            if (pos.x < 0 || pos.x >= xSize || pos.y < 0 || pos.y >= ySize)
            {
                continue;
            }
            adjacent.Add(tiles[pos.x, pos.y]);
        }
        return adjacent;
    }

    public List<GameObject> FindMatches(Vector2Int gridPos, List<GameObject> excluded = null)
    {
        List<GameObject> matches = new List<GameObject>();
        Sprite typeToMatch = tiles[gridPos.x, gridPos.y].GetComponent<Tile>().GetCandyType();
        matches.Add(tiles[gridPos.x, gridPos.y]);

        if (excluded == null)
            excluded = new List<GameObject>();

        for (int i = 0; i < DIRECTIONS.Length; i++)
        {
            var currentPos = gridPos;
            while (true)
            {
                var adjacentPos = currentPos + DIRECTIONS[i];
                if (adjacentPos.x < 0 || adjacentPos.x >= xSize ||
                    adjacentPos.y < 0 || adjacentPos.y >= ySize)
                {
                    break;
                }
                Sprite candyType = tiles[adjacentPos.x, adjacentPos.y].GetComponent<Tile>().GetCandyType();
                if (candyType == typeToMatch)
                {
                    var toAdd = tiles[adjacentPos.x, adjacentPos.y];
                    if (!excluded.Contains(toAdd))
                    {
                        matches.Add(toAdd);
                    }
                    currentPos = adjacentPos;
                    continue;
                }
                else
                {
                    break;
                }
            }
        }

        if (matches.Count >= 3)
        {
            return matches;
        }

        return new List<GameObject>();
    }

    public IEnumerator ClearMatches(List<GameObject> matches)
    {
        List<Coroutine> anims = new List<Coroutine>();

        for (int i = 0; i < matches.Count; i++)
        {
            var tile = matches[i];
            var anim = StartCoroutine(tile.GetComponent<Tile>().ClearType());
            anims.Add(anim);
        }
        for (int i = 0; i < anims.Count; i++)
        {
            yield return anims[i];
        }
        GUIManager.instance.Score += matches.Count * 50;
    }

    public void ShiftTilesDown()
    {
        for (int x = 0; x < xSize; x++)
        {
            int empties = 0;
            for (int y = (ySize - 1); y >= 0; y--)
            {
                if (!tiles[x, y].GetComponent<Tile>().GetCandyType())
                {
                    empties += 1;
                }
                else
                {
                    var emptyTile = tiles[x, y].GetComponent<Tile>();
                    var tileToMove = tiles[x, y + empties].GetComponent<Tile>();
                    SwapTile(emptyTile, tileToMove);
                }
            }
        }
    }

    public void ReplaceEmptyTiles()
    {
        List<Sprite> choice = new List<Sprite>();
        choice.AddRange(candies);

        for (int y = 0; y < ySize; y++)
        {
            for (int x = 0; x < xSize; x++)
            {
                if (!tiles[x, y].GetComponent<Tile>().GetCandyType())
                {
                    Sprite candyType = choice[Random.Range(0, choice.Count)];
                    tiles[x, y].GetComponent<Tile>().SetCandyType(candyType);
                }
            }
        }
    }
}
