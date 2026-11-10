
using UnityEngine;

public class Puzzle3MovingPillar : MonoBehaviour
{
    [SerializeField] private float sinkDistance = 4f;
    [SerializeField] private float delay;

    private Vector3 topPosition;

    private void Start()
    {
        topPosition = transform.position;
    }

    private void Update()
    {
        float time = Mathf.Repeat(Time.time + delay, 4.8f);
        float down = 0f;

        if (time < 2f)
            down = 0f;
        else if (time < 2.8f)
            down = (time - 2f) / 0.8f;
        else if (time < 4f)
            down = 1f;
        else
            down = 1f - (time - 4f) / 0.8f;

        transform.position =
            topPosition - Vector3.up * down * sinkDistance;
    }
}
