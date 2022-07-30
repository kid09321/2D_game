using UnityEngine;
using System.Collections;

public class PrototypeHeroDemo : MonoBehaviour {

    [Header("Variables")]
    [SerializeField] float      m_maxSpeed = 4.5f;
    [SerializeField] float      m_jumpForce = 7.5f;
    [SerializeField] float      m_dashMultiplier = 10f;
    [SerializeField] float      m_dashCoolDown = 3f;
    [SerializeField] float      m_dashDuration = 0.2f;
    [SerializeField] float      m_attackDuration = 0.7f;
    [SerializeField] float      m_attackCoolDown = 0.5f;
    [SerializeField] float      m_attackTimeBetween = 0.1f;
    [SerializeField] bool       m_hideSword = false;
    [Header("Effects")]
    [SerializeField] GameObject m_RunStopDust;
    [SerializeField] GameObject m_JumpDust;
    [SerializeField] GameObject m_LandingDust;

    private Animator            m_animator;
    private Rigidbody2D         m_body2d;
    private Sensor_Prototype    m_groundSensor;
    private AudioSource         m_audioSource;
    private AudioManager_PrototypeHero m_audioManager;
    private bool                m_grounded = false;
    private bool                m_moving = false;
    private bool                m_dashing = false;
    private bool                m_attacking = false;
    private bool                m_movementLocked = false;
    private int                 m_facingDirection = 1;
    private float               m_disableMovementTimer = 0.0f;
    private float               m_dashCoolDownTimer = 0.0f;
    private float               m_dashDurationTimer = 0.0f;

    private int                 m_currentAttackState = 0;
    private float               m_attackDurationTimer = 0.0f;
    private float               m_attackCoolDownTimer = 0.0f;
    private float               m_attackTimeBetweenTimer = 0.0f;

    // Use this for initialization
    void Start ()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_audioSource = GetComponent<AudioSource>();
        m_audioManager = AudioManager_PrototypeHero.instance;
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_Prototype>();
    }

    // Update is called once per frame
    void Update ()
    {
        // Decrease timer that disables input movement. Used when attacking
        m_disableMovementTimer -= Time.deltaTime;

        //Check if character just landed on the ground
        if (!m_grounded && m_groundSensor.State())
        {
            m_grounded = true;
            m_animator.SetBool("Grounded", m_grounded);
        }

        //Check if character just started falling
        if (m_grounded && !m_groundSensor.State())
        {
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
        }

        // -- Handle input and movement --
        MovementHandler();

        // Set AirSpeed in animator
        m_animator.SetFloat("AirSpeedY", m_body2d.velocity.y);

        // Set Animation layer for hiding sword
        int boolInt = m_hideSword ? 1 : 0;
        m_animator.SetLayerWeight(1, boolInt);

        //Attack
        Attack();
        if (m_attacking)
        {
            m_animator.SetInteger("AnimState", 3);
            m_attackDurationTimer += Time.deltaTime;
        }

        if (!m_movementLocked)
        {                        
            // -- Handle Animations --
            //Jump
            if (Input.GetButtonDown("Jump") && m_grounded && m_disableMovementTimer < 0.0f)
            {
                m_animator.SetTrigger("Jump");
                m_grounded = false;
                m_animator.SetBool("Grounded", m_grounded);
                m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);
                m_groundSensor.Disable(0.2f);
            }

            //Run
            else if (m_moving)
                m_animator.SetInteger("AnimState", 1);

            //Idle
            else
                m_animator.SetInteger("AnimState", 0);

            if (m_dashing)
                m_animator.SetInteger("AnimState", 2);
        }              
    }

    // Handlers
    
    // Movement input handler
    void MovementHandler()
    {
        float inputX = 0.0f;
        float inputRaw = 0.0f;
        if (!m_movementLocked && m_disableMovementTimer < 0.0f)
        {
            inputX = Input.GetAxis("Horizontal");
        }
        // GetAxisRaw returns either -1, 0 or 1
        inputRaw = Input.GetAxisRaw("Horizontal");
        // Check if current move input is larger than 0 and the move direction is equal to the characters facing direction
        if (Mathf.Abs(inputRaw) > Mathf.Epsilon && Mathf.Sign(inputRaw) == m_facingDirection)
            m_moving = true;

        else
            m_moving = false;

        // Swap direction of sprite depending on move direction
        if (inputRaw > 0)
        {
            GetComponent<SpriteRenderer>().flipX = false;
            m_facingDirection = 1;
        }

        else if (inputRaw < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
            m_facingDirection = -1;
        }

        // SlowDownSpeed helps decelerate the characters when stopping
        float SlowDownSpeed = m_moving ? 1.0f : 0.5f;
        // Set movement
        m_body2d.velocity = new Vector2(inputX * m_maxSpeed * SlowDownSpeed, m_body2d.velocity.y);
        // Dash
        Dash(inputX);
    }
    // Function used to spawn a dust effect
    // All dust effects spawns on the floor
    // dustXoffset controls how far from the player the effects spawns.
    // Default dustXoffset is zero
    void SpawnDustEffect(GameObject dust, float dustXOffset = 0)
    {
        if (dust != null)
        {
            // Set dust spawn position
            Vector3 dustSpawnPosition = transform.position + new Vector3(dustXOffset * m_facingDirection, 0.0f, 0.0f);
            GameObject newDust = Instantiate(dust, dustSpawnPosition, Quaternion.identity) as GameObject;
            // Turn dust in correct X direction
            newDust.transform.localScale = newDust.transform.localScale.x * new Vector3(m_facingDirection, 1, 1);
        }
    }

    // Movements
    void Dash(float inputX)
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (!m_dashing && m_dashCoolDownTimer <= 0)
            {
                Debug.Log("is dashing");
                m_dashing = true;
                m_dashCoolDownTimer = m_dashCoolDown;
                m_dashDurationTimer = m_dashDuration;
            }
        }
        if (m_dashing)
        {
            if (m_dashDurationTimer > 0)
            {
                m_dashDurationTimer -= Time.deltaTime;
                m_body2d.velocity = new Vector2(inputX * m_maxSpeed * m_dashMultiplier, m_body2d.velocity.y);
            }
            else
            {
                Debug.Log("not dashing");
                m_dashing = false;
                m_body2d.velocity = new Vector2(inputX * m_maxSpeed, m_body2d.velocity.y);
            }
        }
        else
        {
            if (m_dashCoolDownTimer > 0)
            {
                m_dashCoolDownTimer -= Time.deltaTime;
            }
        }
    }

    // Attack
    void Attack()
    {
        if (Input.GetKeyDown(KeyCode.Z) && !m_attacking && m_attackCoolDownTimer <= 0)
        {            
            m_attacking = true;
            m_movementLocked = true;
            switch (m_currentAttackState % 3)
            {
                case 0:
                    m_animator.SetTrigger("Attack1");
                    break;
                case 1:
                    m_animator.SetTrigger("Attack2");
                    break;
                case 2:
                    m_animator.SetTrigger("Attack3");
                    m_attackCoolDownTimer = m_attackCoolDown;
                    break;

            }
            m_currentAttackState += 1;
            m_animator.SetInteger("AnimState", 3);
            m_attackDurationTimer = 0.0f;
            m_attackTimeBetweenTimer = 0.0f;
        }
        else if(Input.GetKeyDown(KeyCode.Z))
        {
            Debug.Log("m_attacking:" + m_attacking);
            Debug.Log("m_attackCoolDownTimer:" + m_attackCoolDownTimer);
        }
        // To lock the movement when comboing.
        // After one attack duration is over, attacking set to false first.
        // Then check if there is next attack in m_attackTimeBetween, if not, unlock the movement.
        if (m_attackDurationTimer >= m_attackDuration)
        {
            m_attacking = false;        
            if (m_attackTimeBetweenTimer > m_attackTimeBetween)
            {
                m_movementLocked = false;
            }
            m_attackTimeBetweenTimer += Time.deltaTime;
        }       
        m_attackCoolDownTimer -= Time.deltaTime;
    }

    // Animation Events
    // These functions are called inside the animation files
    void AE_runStop()
    {
        m_audioManager.PlaySound("RunStop");
        // Spawn Dust
        float dustXOffset = 0.6f;
        SpawnDustEffect(m_RunStopDust, dustXOffset);
    }

    void AE_footstep()
    {
        m_audioManager.PlaySound("Footstep");
    }

    void AE_Jump()
    {
        m_audioManager.PlaySound("Jump");
        // Spawn Dust
        SpawnDustEffect(m_JumpDust);
    }

    void AE_Landing()
    {
        m_audioManager.PlaySound("Landing");
        // Spawn Dust
        SpawnDustEffect(m_LandingDust);
    }

    void AE_AttackEnd()
    {
        Debug.Log("EndAttackEvent Called");
        //m_attackTimeBetweenTimer = m_attackTimeBetween;       
    }
}
