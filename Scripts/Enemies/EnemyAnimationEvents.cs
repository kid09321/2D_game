using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimationEvents : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void AE_DeathEnd()
    {
        transform.parent.gameObject.SetActive(false);
    }
}
