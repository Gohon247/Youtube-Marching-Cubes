using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    public float mouseSensitivity;
    public float moveSpeed;
    public float jumpForce;
    public float stickToGroundForce;
    public Camera cam;


    public GameObject goblinArmy;
    public GameObject gun;

    Rigidbody rb;
    float mouseX;
    float mouseY;
    Vector3 velocity;

    
    public Transform groundCheck;
    public float groundDistance;
    public LayerMask groundMask;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mouseX = 0;
        mouseY = 0;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        movement();
        if (Input.GetKeyDown("e")){
            goblinArmy.SetActive(!goblinArmy.activeSelf);
        }
        if (Input.GetKeyDown("f"))
        {
            gun.SetActive(!gun.activeSelf);
        }
    }


    void movement()
    {
        //look
        mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
        mouseY -= Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;
        mouseY = Mathf.Clamp(mouseY, -90f, 90f);
        cam.transform.localEulerAngles = Vector3.right * mouseY;
        transform.Rotate(Vector3.up * mouseX, Space.Self);


        //move
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        velocity = transform.TransformDirection(input.normalized) * moveSpeed;

        bool isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded)
        {
            if (Input.GetButtonDown("Jump"))
            {
                rb.AddForce(transform.up * jumpForce, ForceMode.VelocityChange);
                isGrounded = false;
            }
            else
            {
                rb.AddForce(-transform.up * stickToGroundForce, ForceMode.VelocityChange);
            }
        }
    }

    private void FixedUpdate()
    {

        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
       // rb.AddForce(Physics.gravity * (rb.mass * rb.mass));

    }
}
