using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    enum EnemyStates
    {
        Patrol,
        Chase,
        Attack,
        Dead
    }
    [Header("Variables")]
    [SerializeField] int   m_maxHealth = 10;
    [SerializeField] float m_maxSpeed = 4.5f;
    [SerializeField] float m_strength = 10f; 


    private Animator     m_animator;
    private int          m_currentHealth;
    private float        m_currentStrength;
    private EnemyStates  m_currentState;
    private HealthBar    m_healthBar;
    // Start is called before the first frame update
    void Start()
    {
        m_animator = GetComponentInChildren<Animator>();
        m_healthBar = GetComponentInChildren<HealthBar>();
        InitializeProperties();
    }

    // Update is called once per frame
    void Update()
    {
        if(m_currentHealth <= 0 )
        {
            if (m_currentState != EnemyStates.Dead)
            {
                m_animator.SetTrigger("Death");
                m_currentState = EnemyStates.Dead;
                m_healthBar.gameObject.SetActive(false);
            }
            return;
        }
    }

    public void Damaged(int damageValue, GameObject attacker)
    {
        if (m_currentState == EnemyStates.Dead) return;
        // Damage calculation
        m_currentHealth -= damageValue;
        // Damaged Animation
        m_animator.SetTrigger("Hurt");
        m_healthBar.UpdateHealthBar(m_currentHealth, m_maxHealth);       
        Debug.Log("Enemy: " + this.gameObject.name + "get hurt!");
        Debug.Log("Enemy: " + this.gameObject.name + "Current Health: " + m_currentHealth);
    }

    void InitializeProperties()
    {
        m_currentHealth = m_maxHealth;
        m_currentStrength = m_strength;
        m_currentState = EnemyStates.Patrol;
    }   
}
