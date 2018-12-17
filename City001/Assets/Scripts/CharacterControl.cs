using UnityEngine;
using System.Collections.Generic;

public class CharacterControl : MonoBehaviour {
   
    
    [SerializeField] private float moveSpeed = 1;
    [SerializeField] private float turnSpeed = 1;
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody rigidBody;
    [SerializeField] private Path path;
   
    private List<Transform> nodes;
    private int currentNode = 0;

    private float m_currentV = 0;
    private float m_currentH = 0;

    private Quaternion lookRotation;
    private Vector3 direction;

  
    void Start()
    {
        Transform[] pathTransforms = path.GetComponentsInChildren<Transform>();
        nodes = new List<Transform>();

        for (int i = 0; i < pathTransforms.Length; i++)
        {
            if (pathTransforms[i] != path.transform)
                nodes.Add(pathTransforms[i]);
        }
        transform.position = new Vector3(nodes[currentNode].transform.position.x, transform.position.y, nodes[currentNode].transform.position.z);
    }

    private void FixedUpdate()
    {
        NodeCheck();
        Rotate();
        Move();
        
    }

    private void NodeCheck()
    {
        if ((Mathf.Abs(nodes[currentNode].transform.position.x - transform.position.x) < 0.1) && (Mathf.Abs(nodes[currentNode].transform.position.z - transform.position.z) < 0.1))
        {
            if ((currentNode + 1) < nodes.Count)
                currentNode++;
            else           
                currentNode = 0;
                           
        }                
    }

    private void Move()
    {
        Transform Target = nodes[currentNode].transform;

        animator.SetBool("isMoving", true);
        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, Target.position, step);
    }

    private void Rotate()
    {
        Transform Target = nodes[currentNode].transform;

        direction = new Vector3(Target.position.x - transform.position.x, 0, Target.position.z - transform.position.z).normalized;
        lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * turnSpeed);
    }
}
