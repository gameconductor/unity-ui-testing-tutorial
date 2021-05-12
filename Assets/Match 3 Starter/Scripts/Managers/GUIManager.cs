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
using UnityEngine.UI;
using System.Collections;

public class GUIManager : MonoBehaviour
{
    public static GUIManager instance;

    public GameObject gameOverPanel;
    public Text yourScoreTxt;
    public Text highScoreTxt;

    public Text scoreTxt;
    public Text moveCounterTxt;

    private int score, moveCounter;

    void Awake()
    {
        instance = GetComponent<GUIManager>();
        MoveCounter = 5;
    }

    // Show the game over panel
    public void GameOver()
    {
        BoardManager.instance.gameObject.SetActive(false);
        GameManager.instance.gameOver = true;

        gameOverPanel.SetActive(true);

        if (score > PlayerPrefs.GetInt("HighScore"))
        {
            PlayerPrefs.SetInt("HighScore", score);
            highScoreTxt.text = "New Best: " + PlayerPrefs.GetInt("HighScore").ToString();
        }
        else
        {
            highScoreTxt.text = "Best: " + PlayerPrefs.GetInt("HighScore").ToString();
        }

        yourScoreTxt.text = score.ToString();
    }

    public int Score
    {
        get
        {
            return score;
        }

        set
        {
            score = value;
            scoreTxt.text = score.ToString();
        }
    }

    public int MoveCounter
    {
        get
        {
            return moveCounter;
        }

        set
        {
            moveCounter = value;
            if (moveCounter <= 0)
            {
                moveCounter = 0;
                StartCoroutine(WaitForShifting());
            }
            moveCounterTxt.text = moveCounter.ToString();
        }
    }

    private IEnumerator WaitForShifting()
    {
        yield return new WaitUntil(() => !BoardManager.instance.IsPlayingAnim);
        yield return new WaitForSeconds(.25f);
        GameOver();
    }

}
