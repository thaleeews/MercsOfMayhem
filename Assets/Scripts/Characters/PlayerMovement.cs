using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rigidBody;

    private Vector2 movement;
    private Vector2 screenBounds;
    private float playerHalfWidth;
    private float xPosLastFrame;

    public float wallJumpCooldown { get; set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        playerHalfWidth = spriteRenderer.bounds.extents.x;
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        //ClampMovement();
        FlipCharacterX();

        if (wallJumpCooldown > 0f)
        {
            wallJumpCooldown -= Time.deltaTime;
        }
    }

    private void FlipCharacterX()
    {
        if (xPosLastFrame < transform.position.x)
        {
            spriteRenderer.flipX = false;
        }
        else if (xPosLastFrame > transform.position.x)
        {
            spriteRenderer.flipX = true;    
        }

        xPosLastFrame = transform.position.x;
    }

    private void ClampMovement()
    {
        float clampedX = Mathf.Clamp(transform.position.x, -screenBounds.x + playerHalfWidth, screenBounds.x - playerHalfWidth);
        Vector2 pos = transform.position;
        pos.x = clampedX;
        transform.position = pos;
    }

    public void KnockbackPlayer(Vector2 knockbackForce, int direction)
    {
        knockbackForce.x *= direction;
        rigidBody.linearVelocity = Vector2.zero;
        rigidBody.angularVelocity = 0f;
        rigidBody.AddForce(knockbackForce, ForceMode2D.Impulse);
    }

    private void HandleMovement()
    {
        if (wallJumpCooldown > 0) return;
        float input = Input.GetAxisRaw("Horizontal");
        movement.x = input * speed * Time.deltaTime;
        transform.Translate(movement);        
    }
}
