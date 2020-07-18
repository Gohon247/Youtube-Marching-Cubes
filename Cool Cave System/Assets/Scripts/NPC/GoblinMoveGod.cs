using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoblinMoveGod : MonoBehaviour
{
    GoblinArmy goblinArmy;
    public float groundDistance;
    public LayerMask groundMask;
    public float jumpForce;
    public float horizontalJump;

    void Start()
    {
        transform.GetComponent<Transform>();
        Transform[] goblins;
        int count = transform.childCount;

        goblins = new Transform[count];
        for (int i = 0; i<count; i++)
        {
            goblins[i] = transform.GetChild(i).GetComponent<Transform>();
        }
        goblinArmy = new GoblinArmy(count, goblins);
    }

    void Update()
    {
        CommandCenter();
    }

    void CommandCenter()
    {
        for (int i=0; i< goblinArmy.count; i++)
        {
            MoveGoblin(goblinArmy.goblins[i]);
        }
    }

    void MoveGoblin(Transform goblin)
    {
        Transform groundCheck = goblin.Find("GroundChecker");
        Rigidbody rb = goblin.GetComponent<Rigidbody>();
        bool isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded==false) { return; }

        Vector3 force = Vector3.zero;

        force += goblin.up * jumpForce;
        force += -goblin.right * horizontalJump;

        rb.AddForce(force, ForceMode.VelocityChange);
    }

    struct GoblinArmy
    {
        public readonly int count;
        public readonly Transform[] goblins;
        public GoblinArmy(int num, Transform[] goblins)
        {
            count = num;
            this.goblins = goblins;
        }
    }

}
