using UnityEngine;

namespace Lumber.Player
{
    /// FPS movement/look driven by external input fields so both the mobile
    /// on-screen joystick/look-pad AND keyboard+mouse (for testing in the Editor)
    /// can feed the same controller without fighting each other.
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Movement")]
        public float walkSpeed = 4.2f;
        public float sprintMultiplier = 1.4f;
        public float gravity = -18f;

        [Header("Look")]
        public float lookSensitivity = 2.2f;
        public float minPitch = -80f;
        public float maxPitch = 80f;

        // Set every frame by MobileJoystick (or read as a fallback by the keyboard path below).
        [HideInInspector] public Vector2 moveInput;
        // Accumulated by LookPad drag events; consumed and reset once per Update.
        [HideInInspector] public Vector2 lookInput;
        [HideInInspector] public bool sprintHeld;

        public Transform cameraPivot;

        private CharacterController controller;
        private float pitch;
        private float verticalVelocity;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            HandleLook();
            HandleMove();
            HandleEditorFallback();
        }

        private void HandleLook()
        {
            if (lookInput.sqrMagnitude > 0.0001f)
            {
                float yaw = lookInput.x * lookSensitivity;
                pitch -= lookInput.y * lookSensitivity;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

                transform.Rotate(Vector3.up, yaw, Space.World);
                if (cameraPivot != null)
                    cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }

            lookInput = Vector2.zero;
        }

        private void HandleMove()
        {
            Vector2 effectiveMove = moveInput;

#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.touchCount == 0 && effectiveMove.sqrMagnitude < 0.01f)
            {
                effectiveMove = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            }
#endif

            Vector3 forward = transform.forward;
            Vector3 right = transform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 wish = forward * effectiveMove.y + right * effectiveMove.x;
            if (wish.magnitude > 1f) wish.Normalize();

            float speed = walkSpeed * (sprintHeld ? sprintMultiplier : 1f);

            if (controller.isGrounded && verticalVelocity < 0f)
                verticalVelocity = -1f;

            verticalVelocity += gravity * Time.deltaTime;

            Vector3 motion = wish * speed;
            motion.y = verticalVelocity;

            controller.Move(motion * Time.deltaTime);
        }

        private void HandleEditorFallback()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.touchCount == 0)
            {
                if (Input.GetMouseButton(1))
                {
                    lookInput += new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * 5f;
                }
                sprintHeld = Input.GetKey(KeyCode.LeftShift);
            }
            else
            {
                sprintHeld = false;
            }
#endif
        }
    }
}
