﻿using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour
{
	[HideInInspector]
	public bool facingRight = true;			// For determining which way the player is currently facing.
	[HideInInspector]
	public bool jump = false;				// Condition for whether the player should jump.


	public float moveForce = 100f;			// Amount of force added to move the player left and right.
	public float maxSpeed = 5f;				// The fastest the player can travel in the x axis.
	public AudioClip[] jumpClips;			// Array of clips for when the player jumps.
	public float jumpForce = 1000f;			// Amount of force added when the player jumps.
	public AudioClip[] taunts;				// Array of clips for when the player taunts.
	public float tauntProbability = 50f;	// Chance of a taunt happening.
	public float tauntDelay = 1f;			// Delay for when the taunt should happen.

    public float playerSpeed;

    public float originalJump = 700;
    public float originalSpeed = 1;
    public float crouchJump = 350;
    public float crouchSpeed = 0.5f;

    public Transform ceilingCheck;
    private bool ceiled;                    //Check if ceiling is above player
    private float crouch;
    public bool crouching;

   

    private int tauntIndex;					// The index of the taunts array indicating the most recent taunt.
	private Transform groundCheck;			// A position marking where to check if the player is grounded.
	public bool grounded = false;			// Whether or not the player is grounded.
	private Animator anim;					// Reference to the player's animator component.
    private Rigidbody rb;
    private GameObject gameObject;


	void Awake()
	{
		// Setting up references.
		groundCheck = transform.Find("groundCheck");
		anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        gameObject = GetComponent<GameObject>();
	}


	void Update()
	{
		// The player is grounded if a linecast to the groundcheck position hits anything on the ground layer.
		grounded = Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground")) || Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Enemies")) || Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Objects"));

        //Check if ceiling is above player
        ceiled = Physics2D.Linecast(transform.position, ceilingCheck.position, 1 << LayerMask.NameToLayer("Ground"));

        if (grounded)
        {
            anim.SetBool("OnGround", true);
        }
        else
        {
            anim.SetBool("OnGround", false);
        }

        // If the jump button is pressed and the player is grounded then the player should jump.
        if (Input.GetButtonDown("Jump") && grounded)
        {
            jump = true;
                //flag for animator
            anim.SetBool("Jump", true);
        }
        else
        {
            anim.SetBool("Jump", false);
        }

        // Crouch
        crouch = Input.GetAxisRaw("Crouch");
        if (crouch != 0 || ceiled == true && grounded == true)
        {
            //flag for animator
            crouching = true;
            anim.SetBool("Crouch", true);
        }
        else
        {
            crouching = false;
            anim.SetBool("Crouch", false);
        }
    }


	void FixedUpdate ()
	{

        // Cache the horizontal input.
        float h = Input.GetAxis("Horizontal");

        //Flags character as moving for animator
        if (h != 0)
            anim.SetBool("IsMoving", true);
        else{
            anim.SetBool("IsMoving", false);
        }

        // making sure the charater does not fly off the screen
        playerSpeed = h * Time.deltaTime * maxSpeed;

		// The Speed animator parameter is set to the absolute value of the horizontal input.
		anim.SetFloat("Speed", Mathf.Abs(h));


        // If the player is changing direction (h has a different sign to velocity.x) or hasn't reached maxSpeed yet...
        //if (h * GetComponent<Rigidbody2D>().velocity.x < maxSpeed)
        //{
        //    // ... add a force to the player.
        //    //GetComponent<Rigidbody2D>().AddForce(Vector2.right * h * moveForce);
        //    GetComponent<Rigidbody2D>().velocity = new Vector2(10, GetComponent<Rigidbody2D>().velocity.y);
        //}

        //if (Input.GetKey(KeyCode.D))
        //{
        //    GetComponent<Rigidbody2D>().velocity = new Vector2(10, GetComponent<Rigidbody2D>().velocity.y);
        //}


        //    if (Input.GetKey(KeyCode.A))
        //{
        //    GetComponent<Rigidbody2D>().velocity = new Vector2(-10, GetComponent<Rigidbody2D>().velocity.y);
        //}
        //if (Input.GetKeyUp(KeyCode.D))
        //{
        //    GetComponent<Rigidbody2D>().velocity = new Vector2(0, GetComponent<Rigidbody2D>().velocity.y);
        //}

        // If the player's horizontal velocity is greater than the maxSpeed...
        //if(Mathf.Abs(GetComponent<Rigidbody2D>().velocity.x) > maxSpeed)
        //	// ... set the player's velocity to the maxSpeed in the x axis.
        //	GetComponent<Rigidbody2D>().velocity = new Vector2(Mathf.Sign(GetComponent<Rigidbody2D>().velocity.x) * maxSpeed, GetComponent<Rigidbody2D>().velocity.y);

        transform.Translate(playerSpeed, 0, 0);


        // If the input is moving the player right and the player is facing left...
        if (h > 0 && !facingRight)
			// ... flip the player.
			Flip();
		// Otherwise if the input is moving the player left and the player is facing right...
		else if(h < 0 && facingRight)
			// ... flip the player.
			Flip();

		// If the player should jump...
		if(jump)
		{
			// Set the Jump animator trigger parameter.
			anim.SetTrigger("Jump");

			// Play a random jump audio clip.
			int i = Random.Range(0, jumpClips.Length);
			AudioSource.PlayClipAtPoint(jumpClips[i], transform.position);

			// Add a vertical force to the player.
			GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, jumpForce));

			// Make sure the player can't jump again until the jump conditions from Update are satisfied.
			jump = false;
		}
	}
	
	
	void Flip ()
	{
		// Switch the way the player is labelled as facing.
		facingRight = !facingRight;

		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}


	public IEnumerator Taunt()
	{
		// Check the random chance of taunting.
		float tauntChance = Random.Range(0f, 100f);
		if(tauntChance > tauntProbability)
		{
			// Wait for tauntDelay number of seconds.
			yield return new WaitForSeconds(tauntDelay);

			// If there is no clip currently playing.
			if(!GetComponent<AudioSource>().isPlaying)
			{
				// Choose a random, but different taunt.
				tauntIndex = TauntRandom();

				// Play the new taunt.
				GetComponent<AudioSource>().clip = taunts[tauntIndex];
				GetComponent<AudioSource>().Play();
			}
		}
	}


	int TauntRandom()
	{
		// Choose a random index of the taunts array.
		int i = Random.Range(0, taunts.Length);

		// If it's the same as the previous taunt...
		if(i == tauntIndex)
			// ... try another random taunt.
			return TauntRandom();
		else
			// Otherwise return this index.
			return i;
	}
}
