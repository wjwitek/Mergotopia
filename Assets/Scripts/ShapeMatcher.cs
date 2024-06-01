using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeMatcher : MonoBehaviour
{
    public Player player;
    public MainCamera mainCamera;
    public RectTransform matchArea;

    private Vector2 touchStartVector = Vector2.zero;
    private Vector2 touchStartPosition = Vector2.zero;

    private float matchAreaTop = 0, matchAreaBottom = 0, matchAreaLeft = 0, matchAreaRight = 0;
    private float maxScale = 0;

    void Start()
    {
        matchAreaTop = mainCamera.PixelsToWorld(matchArea.rect.height * 0.5f);
        matchAreaBottom = mainCamera.PixelsToWorld(-matchArea.rect.height * 0.5f);
        matchAreaLeft = mainCamera.PixelsToWorld(-matchArea.rect.width * 0.5f);
        matchAreaRight = mainCamera.PixelsToWorld(matchArea.rect.width * 0.5f);

        var screenWidth = Mathf.Max(matchAreaTop - matchAreaBottom, matchAreaRight - matchAreaLeft);
        float maxVerticiesDis = 0;
        var verticies = player.mesh.vertices;
        for (int i = 0; i < verticies.Length; i++)
        {
            for (int j = i + 1; j < verticies.Length; j++)
            {
                var dis = (verticies[i] - verticies[j]).sqrMagnitude;
                if (dis > maxVerticiesDis)
                {
                    maxVerticiesDis = dis;
                }
            }
        }
        maxScale = screenWidth / Mathf.Sqrt(maxVerticiesDis);
    }

    private Vector3 ClampToMatchArea(Vector3 pos)
    {
        return new Vector3(
            Mathf.Clamp(pos.x, matchAreaLeft, matchAreaRight), Mathf.Clamp(pos.y, matchAreaBottom, matchAreaTop), pos.z);
    }

    private Vector3 ClampScale(Vector3 scale)
    {
        float sc = Mathf.Clamp(scale.x, 0.2f, maxScale);
        return new Vector3(sc, sc, sc);
    }

    void Update()
    {
        if (Input.touchCount == 1)
        {
            var touchOne = Input.GetTouch(0);
            if (touchOne.phase == TouchPhase.Began)
            {
                touchStartPosition = touchOne.position;
            }
            else if (touchOne.phase == TouchPhase.Moved)
            {
                var currPosition = touchOne.position;
                var diffPosition = (currPosition - touchStartPosition) * mainCamera.GetPixelsToWorld();
                player.transform.position = ClampToMatchArea(player.transform.position + (Vector3)diffPosition);
                touchStartPosition = currPosition;
            }
        }
        else if (Input.touchCount == 2)
        {
            var touchOne = Input.GetTouch(0);
            var touchTwo = Input.GetTouch(1);

            if (touchOne.phase == TouchPhase.Began || touchTwo.phase == TouchPhase.Began)
            {
                touchStartVector = touchTwo.position - touchOne.position;
                touchStartPosition = (touchTwo.position + touchOne.position) * 0.5f;
            }

            if (touchOne.phase == TouchPhase.Moved || touchTwo.phase == TouchPhase.Moved)
            {
                var currVector = touchTwo.position - touchOne.position;
                var currPosition = (touchTwo.position + touchOne.position) * 0.5f;
                var diffPosition = (currPosition - touchStartPosition) * mainCamera.GetPixelsToWorld();
                var scaleDiff = currVector.magnitude / touchStartVector.magnitude;
                var angle = Vector2.SignedAngle(touchStartVector, currVector);
                player.transform.rotation = Quaternion.Euler(0, 0, player.transform.rotation.eulerAngles.z + angle);
                player.transform.position = ClampToMatchArea(player.transform.position + (Vector3)diffPosition);
                player.transform.localScale = ClampScale(player.transform.localScale * scaleDiff);
                touchStartVector = currVector;
                touchStartPosition = currPosition;
            }
            
            if (touchOne.phase == TouchPhase.Ended)
            {
                touchStartPosition = touchTwo.position;
            }
            if (touchTwo.phase == TouchPhase.Ended)
            {
                touchStartPosition = touchOne.position;
            }
        }
    }
}
