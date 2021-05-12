using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public BoardManager boardManager;
    public Vector2Int gridPos;
    public bool isSelected = false;
    public string type = "Yellow";
    public List<Sprite> explosion = new List<Sprite>();

    private float FRAME_RATE = (1 / 60f) * 3f;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void SetCandyType(Sprite candy)
    {
        GetComponent<Image>().sprite = candy;
    }

    public Sprite GetCandyType()
    {
        return GetComponent<Image>().sprite;
    }

    public IEnumerator ClearType()
    {
        for (int i = 0; i < explosion.Count; i++)
        {
            transform.localScale = new Vector3(2.0f, 2.0f, 2.0f);
            GetComponent<Image>().sprite = explosion[i];
            yield return new WaitForSeconds(FRAME_RATE);
        }
        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        GetComponent<Image>().sprite = null;
    }

    public void OnClick()
    {
        boardManager.OnTileClick(this);
    }

    public void Select()
    {
        isSelected = true;
        string name;
        if (GetComponent<Image>().sprite)
        {
            name = GetComponent<Image>().sprite.name;
        }
        else
        {
            name = null;
        }
        // Debug.Log("Select " + gridPos + " " + name);
    }

    public void Deselect()
    {
        isSelected = false;
    }
}
