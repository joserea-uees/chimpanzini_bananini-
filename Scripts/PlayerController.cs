using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float runSpeed = 8f;
    public float jumpForce = 12f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        groundLayer = LayerMask.GetMask("Ground");
    }

    void Update()
    {
        // Corre siempre hacia la derecha (endless runner)
        rb.linearVelocity = new Vector2(runSpeed, rb.linearVelocity.y);

        // Salto con espacio o click/tap
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    // Detectar suelo muy simple
    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }
}