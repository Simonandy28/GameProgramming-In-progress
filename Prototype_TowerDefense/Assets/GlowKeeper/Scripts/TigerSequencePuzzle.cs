using UnityEngine;

public class TigerSequencePuzzle : MonoBehaviour
{
    public TigerTile[] tiles;
    public int[] order = { 3, 1, 4, 2 };

    private int step = 0;
    private bool completed = false;
    public bool IsComplete => completed;
    public bool IsMelodyPlaying { get; set; }
    public int WrongTileCount { get; private set; }

    public void CheckTile(int number)
    {
        if (completed || IsMelodyPlaying) return;

        if (number != order[step])
        {
            step = 0;
            foreach (TigerTile tile in tiles)
                tile.ResetTile();
            WrongTileCount++;
            return;
        }

        foreach (TigerTile tile in tiles)
        {
            if (tile.tileNumber == number)
            {
                tile.Activate();
                break;
            }
        }

        step++;
        if (step == order.Length)
        {
            completed = true;
            Debug.Log("Puzzle Complete!");
        }
    }
}
