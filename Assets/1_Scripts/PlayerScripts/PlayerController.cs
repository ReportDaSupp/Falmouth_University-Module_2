using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;

    Rigidbody2D rb;
    Vector2 moveInput;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    void Update()
    {
        // Basic 2D movement (no iso math)
        moveInput.x = Input.GetAxisRaw("Horizontal"); // A/D or ←/→
        moveInput.y = Input.GetAxisRaw("Vertical");   // W/S or ↑/↓

        moveInput = moveInput.normalized;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }
}