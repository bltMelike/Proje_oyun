using UnityEngine;



[RequireComponent(typeof(Rigidbody2D))]

[RequireComponent(typeof(Animator))]

[RequireComponent(typeof(SpriteRenderer))]
public class OyuncuKontrol : MonoBehaviour

{

    [SerializeField] private float speed = 5f;



    private Rigidbody2D rb;

    private Animator animator;

    private SpriteRenderer spriteRenderer;

    private Vector2 input;

    private int yon = 0; // 0: aşağı, 1: yukarı, 2: sağ/sol

    private bool kontrolAktif = true;



    private void Awake()

    {

        rb = GetComponent<Rigidbody2D>();

        animator = GetComponent<Animator>();

        spriteRenderer = GetComponent<SpriteRenderer>();

    }



    public void KontrolAktif(bool aktif)

    {

        kontrolAktif = aktif;

        if (!aktif)

        {

            input = Vector2.zero;

            animator.SetBool("Yuruyor", false);

        }

    }



    public void SagaBak(bool sag)

    {

        yon = 2;

        spriteRenderer.flipX = !sag;

        animator.SetInteger("Yon", yon);

    }



    private void Update()

    {

        if (!kontrolAktif) return;



        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        bool yuruyor = input.sqrMagnitude > 0.01f;



        if (yuruyor)

        {

            if (Mathf.Abs(input.x) > 0.01f)

            {

                yon = 2;

                spriteRenderer.flipX = input.x < 0f;

            }

            else

            {

                yon = input.y > 0f ? 1 : 0;

            }

        }



        animator.SetBool("Yuruyor", yuruyor);

        animator.SetInteger("Yon", yon);

    }



    private void FixedUpdate()

    {

        rb.MovePosition(rb.position + input * speed * Time.fixedDeltaTime);

    }

}