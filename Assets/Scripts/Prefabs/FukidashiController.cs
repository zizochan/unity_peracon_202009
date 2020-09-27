using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FukidashiController : MonoBehaviour
{
    const float LIMIT_Y = -15f;
    const float GOOD_MESSAGE_RATE = 0.2f;

    float INITIAL_SPEED;
    float MIN_SPEED;
    float REDUCE_SPEED;
    float INITIAL_ROTATION_SPEED;
    float INITIAL_SCALE_SPEED;
    float SPEED_MAX_RATE;
    float SCALE_MAX_RATE;

    float deltaTime;
    float moveSpeed;
    float rotationSpeed;
    float scaleSpeedX;
    float scaleSpeedY;

    Vector3 initialScale;
    public List<Sprite> messages;
    public List<Sprite> goodMessages;

    public GameObject fukidashiObject;

    int messageType;

    GameController gameController;
    public Animator fukidashiDestroyAnimator;
    public Animator fukidashiBackgroundDestroyAnimator;

    float destroyRestTime = 0f;
    bool isDestroying = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void Initialize(int _messageType, int createCount)
    {
        SetStatus();

        gameController = GameObject.Find("GameController").GetComponent<GameController>();

        moveSpeed = INITIAL_SPEED;
        rotationSpeed = GetRotationSpeed();
        scaleSpeedX = scaleSpeedY = GetScaleSpeed();

        messageType = _messageType;
        SetMessage();

        initialScale = transform.localScale;

        SetOrderInLayer(createCount);
    }

    // Update is called once per frame
    void Update()
    {
        if (!Data.IsGamePlay())
        {
            return;
        }

        deltaTime = Time.deltaTime;

        CheckDestroyYPosition();
        CheckDestroying(deltaTime);

        ReduceSpeed();
        Move();
    }

    void CheckDestroyYPosition()
    {
        if (!IsDestroyYPosition())
        {
            return;
        }

        DestroyObject();
    }

    void DestroyObject()
    {
        Destroy(this.gameObject);
    }

    bool IsDestroyYPosition()
    {
        return transform.localPosition.y < LIMIT_Y;
    }

    void Move()
    {
        MoveY();
        Rotation();
        ChangeScale();
    }

    void MoveY()
    {
        Vector2 newPosition = transform.localPosition;

        newPosition.y -= moveSpeed * deltaTime;

        transform.localPosition = newPosition;
    }

    void ReduceSpeed()
    {
        moveSpeed -= REDUCE_SPEED;

        if (moveSpeed < MIN_SPEED)
        {
            moveSpeed = MIN_SPEED;
        }
    }

    void Rotation()
    {
        Vector3 currentRotation = transform.localEulerAngles;

        float newZ = currentRotation.z + rotationSpeed * deltaTime;

        transform.rotation = Quaternion.Euler(0f, 0f, newZ);
    }

    float GetRotationSpeed()
    {
        return 0f;

        /*
        float min = INITIAL_ROTATION_SPEED;
        float max = INITIAL_ROTATION_SPEED * SPEED_MAX_RATE;

        float speed = Random.Range(min, max);

        if (Random.Range(0, 2) == 0)
        {
            speed *= -1f;
        }

        return speed;
        */
    }

    void ChangeScale()
    {
        Vector3 scale = transform.localScale;

        scale.x += scaleSpeedX * deltaTime;
        if (IsReverseScale(scale.x, initialScale.x))
        {
            scaleSpeedX *= -1f;
        }

        scale.y += scaleSpeedY * deltaTime;
        if (IsReverseScale(scale.y, initialScale.y))
        {
            scaleSpeedY *= -1f;
        }

        transform.localScale = scale;
    }

    bool IsReverseScale(float currentSacle, float initScale)
    {
        if (currentSacle > initScale * SCALE_MAX_RATE)
        {
            return true;
        }

        if (currentSacle < initScale / SCALE_MAX_RATE)
        {
            return true;
        }

        return false;
    }

    float GetScaleSpeed()
    {
        return 0f;

        /*
        float min = INITIAL_SCALE_SPEED;
        float max = INITIAL_SCALE_SPEED * SPEED_MAX_RATE;

        return Random.Range(min, max);
        */
    }

    void SetStatus()
    {
        INITIAL_SPEED = 2f;
        MIN_SPEED = 0.8f;
        REDUCE_SPEED = 0.002f;
        INITIAL_ROTATION_SPEED = 5f;
        INITIAL_SCALE_SPEED = 0.2f;
        SPEED_MAX_RATE = 1.3f;
        SCALE_MAX_RATE = 1.1f;
    }


    void SetMessage()
    {
        Sprite message;

        switch (messageType)
        {
            case Data.MESSAGE_TYPE_GOOD:
                message = GetGoodMessage();
                break;
            default:
                message = GetMessage();
                break;
        }

        this.GetComponent<SpriteRenderer>().sprite = message;
    }

    Sprite GetMessage()
    {
        int index = Random.Range(0, messages.Count);
        return messages[index];
    }

    Sprite GetGoodMessage()
    {
        int index = Random.Range(0, goodMessages.Count);
        return goodMessages[index];
    }

    void SetOrderInLayer(int createCount)
    {
        int layerId = createCount * 2;

        fukidashiObject.GetComponent<SpriteRenderer>().sortingOrder = layerId;
        this.GetComponent<SpriteRenderer>().sortingOrder = layerId + 1;
    }

    public void HitPlayer()
    {
        if (isDestroying)
        {
            return;
        }

        if (IsGameOver())
        {
            gameController.ExecuteGameOver();
        }
        else
        {
            HitGoodMessage();
        }
    }

    bool IsGameOver()
    {
        return messageType == Data.MESSAGE_TYPE_BAD;
    }

    void HitGoodMessage()
    {
        gameController.GetGoodMessage();
        fukidashiDestroyAnimator.Play("Destroy");
        fukidashiBackgroundDestroyAnimator.Play("Destroy");
        SetDestroyTime(0.3f);
    }

    void SetDestroyTime(float restTime)
    {
        destroyRestTime = restTime;
        isDestroying = true;
    }

    void CheckDestroying(float deltaTime)
    {
        if (!isDestroying)
        {
            return;
        }

        destroyRestTime -= deltaTime;
        if (destroyRestTime <= 0f)
        {
            DestroyObject();
        }
    }
}
