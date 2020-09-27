using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    const float maxX = 7.7f;
    const float maxY = 5f;
    const float minX = maxX * -1;
    const float minY = maxY * -1;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (!Data.IsGamePlay())
        {
            return;
        }

        if (Input.GetMouseButtonUp(0))
        {
            Move();
        }
    }

    void Move()
    {
        Vector2 newPosition = GetMousePosition();

        if (newPosition.x < minX)
        {
            newPosition.x = minX;
        } else if (newPosition.x > maxX)
        {
            newPosition.x = maxX;
        }

        if (newPosition.y < minY)
        {
            newPosition.y = minY;
        }
        else if (newPosition.y > maxY)
        {
            newPosition.y = maxY;
        }

        transform.localPosition = newPosition;
    }

    Vector2 GetMousePosition()
    {
        Vector2 clickPosition = Input.mousePosition;
        return Camera.main.ScreenToWorldPoint(clickPosition);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!Data.IsGamePlay())
        {
            return;
        }

        GameObject colObject = col.gameObject;
        if (colObject.tag != "Fukidashi")
        {
            return;
        }

        colObject.GetComponent<FukidashiController>().HitPlayer();
    }
}
