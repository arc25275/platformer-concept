using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]

public class Controller : MonoBehaviour
{
    // Move player in 2D space
    public float maxSpeed = 2.4f;
    public float jumpHeight = 6.5f;
    public float gravityScale = 1.5f;
    public Camera mainCamera;

    bool facingRight = true;
    float moveDirection = 0;
    bool isGrounded = false;
    int currentLayer = 7;
    Vector3 cameraPos;
    Rigidbody2D r2d;
    Collider2D mainCollider;
    // Check every collider except Player and Ignore Raycast
    LayerMask layerMask = ~(1 << 2 | 1 << 8);
    Transform t;
    bool changing = false;

    // Use this for initialization
    void Start()
    {
        t = transform;
        r2d = GetComponent<Rigidbody2D>();
        mainCollider = GetComponent<Collider2D>();
        r2d.freezeRotation = true;
        r2d.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        r2d.gravityScale = gravityScale;
        facingRight = t.localScale.x > 0;
        gameObject.layer = 8;

        if (mainCamera)
            cameraPos = mainCamera.transform.position; ;
        Physics2D.IgnoreLayerCollision(8, 6, true);

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (currentLayer == 7 && changing == false)
            {
                // Foreground to Background

                Physics2D.IgnoreLayerCollision(8, 7, true);
                Physics2D.IgnoreLayerCollision(8, 6, false);
                StartCoroutine(LerpFromTo(transform.localScale, new Vector3(transform.localScale.x / 2, transform.localScale.y / 2, transform.localScale.z), 1f));
                currentLayer = 6;
            }
            else if (currentLayer == 6 && changing == false)
            {
                // Background to foreground

                Physics2D.IgnoreLayerCollision(8, 7, false);
                Physics2D.IgnoreLayerCollision(8, 6, true);
                StartCoroutine(LerpFromTo(transform.localScale, new Vector3(transform.localScale.x * 2, transform.localScale.y * 2, transform.localScale.z), 1f));
                currentLayer = 7;
            }
        }

        // Movement controls
        if ((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)))
        {
            if (Input.GetKey(KeyCode.A))
            {
                moveDirection = -1f;
            }
            if (Input.GetKey(KeyCode.D))
            {
                moveDirection = 1f;
            }
        }
        else if (isGrounded || r2d.velocity.magnitude < 0.01f)
        {
            moveDirection = 0;
        }

        // Change facing direction
        if (moveDirection != 0)
        {
            if (moveDirection > 0 && !facingRight)
            {
                facingRight = true;
                t.localScale = new Vector3(Mathf.Abs(t.localScale.x), t.localScale.y, transform.localScale.z);
            }
            if (moveDirection < 0 && facingRight)
            {
                facingRight = false;
                t.localScale = new Vector3(-Mathf.Abs(t.localScale.x), t.localScale.y, t.localScale.z);
            }
        }

        // Jumping
        if (Input.GetKeyDown(KeyCode.W) && isGrounded)
        {
            r2d.velocity = new Vector2(r2d.velocity.x, jumpHeight);
        }
        else if (!isGrounded)
        {
            r2d.velocity = new Vector2(r2d.velocity.x, r2d.velocity.y);
        }

        // Camera follow

    }

    void FixedUpdate()
    {
        if (mainCamera)
            mainCamera.transform.position = new Vector3(t.position.x, cameraPos.y, cameraPos.z);
        Bounds colliderBounds = mainCollider.bounds;
        Vector3 groundCheckPos = colliderBounds.min + new Vector3(colliderBounds.size.x * 0.5f, 0.1f, 0);
        // Check if player is grounded
        isGrounded = Physics2D.OverlapCircle(groundCheckPos, currentLayer == 6 ? 0.2f : 0.4f, currentLayer == 6 ? (1 << 6) : (1 << 7));
        // Apply movement velocity;
        Debug.Log("Fine to go right = " + (r2d.velocity.x < (currentLayer == 6 ? maxSpeed / 2 : maxSpeed)));
        Debug.Log("Fine to go left = " + (r2d.velocity.x > (currentLayer == 6 ? -maxSpeed / 2 : -maxSpeed)));
        if ((r2d.velocity.x >= 0 && r2d.velocity.x < (currentLayer == 6 ? maxSpeed / 2 : maxSpeed)) || (r2d.velocity.x <= 0 && r2d.velocity.x > (currentLayer == 6 ? -maxSpeed / 2 : -maxSpeed)))
        {
            r2d.velocity += new Vector2((moveDirection) * ((currentLayer == 6) ? 0.5f : 1), 0);
        }
        if (r2d.velocity.x > (currentLayer == 6 ? maxSpeed / 2 : maxSpeed))
        {
            r2d.velocity = new Vector2((currentLayer == 6 ? maxSpeed / 2 : maxSpeed), r2d.velocity.y);
            Debug.Log("Set to max going right");
        }
        if (r2d.velocity.x < (currentLayer == 6 ? -maxSpeed / 2 : -maxSpeed))
        {
            r2d.velocity = new Vector2((currentLayer == 6 ? -maxSpeed / 2 : -maxSpeed), r2d.velocity.y);
            Debug.Log("Set to max going left");
        }
        // Simple debug
        Debug.DrawLine(groundCheckPos, groundCheckPos - new Vector3(0, 0.23f, 0), isGrounded ? Color.green : Color.red);
    }
    IEnumerator LerpFromTo(Vector3 pos1, Vector3 pos2, float duration)
    {
        changing = true;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            transform.localScale = Vector3.Lerp(pos1, pos2, t / duration);
            yield return 0;
        }
        transform.localScale = pos2;
        changing = false;
    }
}

