using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    private Transform m_characterTransform;
    private float m_cameraMovingSpeed = 3f;

    [SerializeField] float maxHorizontalOffset = 2f;
    [SerializeField] float maxVerticalOffset = 3f;

    // Start is called before the first frame update
    void Start()
    {
        m_characterTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        FollowCharacter();
    }

    void FollowCharacter()
    {
        Vector2 playerPosition = new Vector2(m_characterTransform.position.x, m_characterTransform.position.y);
        Vector2 cameraPosition = new Vector2(this.transform.position.x, this.transform.position.y);
        float horizontalDistance = playerPosition.x - cameraPosition.x;
        float verticalDistance = playerPosition.y - cameraPosition.y;

        // Follow player horizontally
        if (horizontalDistance > maxHorizontalOffset || horizontalDistance < -maxHorizontalOffset)
        {       
            Vector2 target = Vector2.Lerp(cameraPosition, playerPosition, m_cameraMovingSpeed * Time.deltaTime);
            transform.position = new Vector3(target.x, transform.position.y, transform.position.z);
        }
        // Follow player vertically
        if (verticalDistance > maxVerticalOffset || verticalDistance < -maxVerticalOffset)
        {
            Vector2 target = Vector2.Lerp(cameraPosition, playerPosition, m_cameraMovingSpeed * Time.deltaTime);
            transform.position = new Vector3(transform.position.x, target.y, transform.position.z);            
        }
    }
}
