using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SlenderPlayerController : MonoBehaviour
{
    public Camera playerCam;
    public float walkSpeed = 3f;
    public float runSpeed = 5.5f;
    public float jumpPower = 0f;
    public float gravity = 10f;
    public float lookSpeed = 2f;
    public float lookXLimit = 75f;

    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    public int ZoomFOV = 35;
    public int initialFOV;
    public float cameraZoomSmooth = 1;

    private bool isZoomed = false;

    public bool canMove = true; // Made public

    CharacterController characterController;
    public AudioSource cameraZoomSound;
    private Animator animator;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        animator = GetComponent<Animator>();
        // Asegúrate de que initialFOV se establezca correctamente al inicio
        if (playerCam != null)
        {
            initialFOV = (int)playerCam.fieldOfView;
        }
    }

    void Update()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        // ==========================================================
        //         BLOQUE DE MOVIMIENTO CORREGIDO (TECLADO EXPLÍCITO)
        // ==========================================================

        float inputX = 0f; // Controla el movimiento Adelante/Atrás (Eje Z)
        float inputY = 0f; // Controla el movimiento Lateral (Eje X)

        // Verificación explícita de las teclas para evitar la interferencia del ratón
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            inputX += 1f;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            inputX -= 1f;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            inputY += 1f;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            inputY -= 1f;
        }

        // Aplicamos la velocidad solo si podemos movernos
        float curSpeedX = canMove ? currentSpeed * inputX : 0;
        float curSpeedY = canMove ? currentSpeed * inputY : 0;

        // Guardar la componente Y antes de recalcular la dirección horizontal
        float movementDirectionY = moveDirection.y;

        // Calcular el nuevo vector de movimiento horizontal
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        // ==========================================================
        //           FIN DEL BLOQUE DE MOVIMIENTO CORREGIDO
        // ==========================================================

        // Actualizar el Animator (usa la velocidad corregida)
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isWalking", curSpeedX != 0 || curSpeedY != 0);

        // Lógica de Salto y Gravedad
        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Aplicar el movimiento
        characterController.Move(moveDirection * Time.deltaTime);

        // Lógica de Rotación (Mouse Look - Esta sección no se modificó ya que estaba correcta)
        if (canMove)
        {
            rotationX -= Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCam.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }

        // Lógica de Zoom (Fire2)
        if (Input.GetButtonDown("Fire2"))
        {
            isZoomed = true;
            cameraZoomSound.Play();
        }

        if (Input.GetButtonUp("Fire2"))
        {
            isZoomed = false;
            cameraZoomSound.Play();
        }

        if (isZoomed)
        {
            playerCam.GetComponent<Camera>().fieldOfView = Mathf.Lerp(playerCam.fieldOfView, ZoomFOV, Time.deltaTime * cameraZoomSmooth);
        }
        else if (!isZoomed)
        {
            playerCam.GetComponent<Camera>().fieldOfView = Mathf.Lerp(playerCam.fieldOfView, initialFOV, Time.deltaTime * cameraZoomSmooth);
        }
    }
}