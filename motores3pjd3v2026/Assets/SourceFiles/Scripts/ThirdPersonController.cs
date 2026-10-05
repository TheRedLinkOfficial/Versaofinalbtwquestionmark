﻿using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")]
        public int playerID = 1;

        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("Quanto a velocidade aumenta ao coletar uma estrela")]
        public float SpeedIncreasePerCoin = 0.5f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;


        [Header("Player Grounded")]

        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;


        [Header("Cinemachine")]

        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        public Vector2 LookSensitivity = new Vector2(7.5f, 5.0f);


        // Cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;


        // Camera starting position and rotation
        private Vector3 _cameraStartingPosition;
        private Quaternion _cameraStartingRotation;


        // Variable to indicate if we are resetting the camera
        public bool IsRespawning { get; set; } = false;


        // Player
        private float _speed;
        private float _animationBlend;

        // =========================================================
        // ALTERADO
        // _targetRotation agora representa a direção para a qual
        // o personagem deve virar.
        // =========================================================
        private float _targetRotation = 0.0f;

        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;


        // Timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;


        // Animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;


#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif

        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;


        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
                return false;
#endif
            }
        }


        private void Awake()
        {
            // Get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }


        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

            _hasAnimator = TryGetComponent(out _animator);

            _controller = GetComponent<CharacterController>();

            _input = GetComponent<StarterAssetsInputs>();

#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#else
            Debug.LogError("Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();


            // Save the starting camera position and rotation
            _cameraStartingPosition = CinemachineCameraTarget.transform.position;
            _cameraStartingRotation = CinemachineCameraTarget.transform.rotation;


            // =========================================================
            // ALTERADO
            // Começa usando a rotação atual do personagem como
            // direção inicial da câmera.
            // =========================================================

            _cinemachineTargetYaw = transform.eulerAngles.y;

            CinemachineCameraTarget.transform.rotation =
                Quaternion.Euler(
                    0.0f,
                    _cinemachineTargetYaw,
                    0.0f
                );


            // Reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }


        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            JumpAndGravity();

            GroundedCheck();

            Move();
        }


        private void LateUpdate()
        {
            CameraRotation();
        }


        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }


        private void GroundedCheck()
        {
            // Set sphere position, with offset
            Vector3 spherePosition = new Vector3(
                transform.position.x,
                transform.position.y - GroundedOffset,
                transform.position.z
            );


            Grounded = Physics.CheckSphere(
                spherePosition,
                GroundedRadius,
                GroundLayers,
                QueryTriggerInteraction.Ignore
            );


            // Update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }


        // =========================================================
        // CÂMERA
        // =========================================================
        // A câmera acompanha a direção do personagem quando ele
        // está andando para frente/lados.
        //
        // Quando S é pressionado, a câmera NÃO altera sua rotação.
        //
        // Isso evita que a rotação do personagem e a câmera entrem
        // em um ciclo de rotação.
        // =========================================================

        private void CameraRotation()
        {
            if (IsRespawning)
            {
                _cinemachineTargetYaw = transform.eulerAngles.y;
                _cinemachineTargetPitch = 0f;

                CinemachineCameraTarget.transform.rotation =
                    Quaternion.Euler(
                        _cinemachineTargetPitch,
                        _cinemachineTargetYaw,
                        0f
                    );

                IsRespawning = false;
                return;
            }


            // =====================================================
            // ALTERADO
            //
            // Quando estiver andando para trás, a câmera mantém
            // exatamente a rotação que já possuía.
            // =====================================================

            if (_input.move.y < 0.0f)
            {
                return;
            }


            // =====================================================
            // Quando não está andando para trás, a câmera acompanha
            // a direção para a qual o personagem está virado.
            // =====================================================

            _cinemachineTargetYaw = transform.eulerAngles.y;
            _cinemachineTargetPitch = 0f;

            CinemachineCameraTarget.transform.rotation =
                Quaternion.Euler(
                    _cinemachineTargetPitch,
                    _cinemachineTargetYaw,
                    0f
                );
        }


        // =========================================================
        // MOVIMENTO
        // =========================================================
        //
        // W:
        //      Personagem anda para frente.
        //      Personagem gira.
        //      Câmera acompanha.
        //
        // A/D:
        //      Personagem gira para o lado.
        //      Câmera acompanha.
        //
        // S:
        //      Personagem anda para trás.
        //      Personagem NÃO gira.
        //      Câmera NÃO gira.
        //
        // A grande diferença é que o movimento usa a rotação da
        // câmera como referência, e não a rotação atual do
        // personagem.
        // =========================================================

        private void Move()
        {
            float targetSpeed =
                _input.sprint ? SprintSpeed : MoveSpeed;


            if (_input.move == Vector2.zero)
            {
                targetSpeed = 0.0f;
            }


            float currentHorizontalSpeed =
                new Vector3(
                    _controller.velocity.x,
                    0.0f,
                    _controller.velocity.z
                ).magnitude;


            float speedOffset = 0.1f;

            float inputMagnitude =
                _input.analogMovement
                    ? _input.move.magnitude
                    : 1f;


            if (
                currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset
            )
            {
                _speed = Mathf.Lerp(
                    currentHorizontalSpeed,
                    targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate
                );

                _speed =
                    Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }


            _animationBlend = Mathf.Lerp(
                _animationBlend,
                targetSpeed,
                Time.deltaTime * SpeedChangeRate
            );


            if (_animationBlend < 0.01f)
            {
                _animationBlend = 0f;
            }


            // =====================================================
            // DIREÇÃO DO INPUT
            // =====================================================

            Vector3 inputDirection =
                new Vector3(
                    _input.move.x,
                    0.0f,
                    _input.move.y
                ).normalized;


            // =====================================================
            // ALTERADO
            //
            // A direção do movimento é calculada usando a câmera.
            //
            // Isso cria uma referência estável.
            //
            // Não usamos:
            //
            //     transform.eulerAngles.y
            //
            // para calcular o movimento.
            //
            // Isso é importante porque, quando o personagem gira,
            // a direção do movimento não muda junto com ele.
            // =====================================================

            Vector3 cameraForward =
                Quaternion.Euler(
                    0.0f,
                    _cinemachineTargetYaw,
                    0.0f
                ) * Vector3.forward;


            Vector3 cameraRight =
                Quaternion.Euler(
                    0.0f,
                    _cinemachineTargetYaw,
                    0.0f
                ) * Vector3.right;


            cameraForward.y = 0.0f;
            cameraRight.y = 0.0f;

            cameraForward.Normalize();
            cameraRight.Normalize();


            Vector3 targetDirection =
                cameraForward * inputDirection.z +
                cameraRight * inputDirection.x;


            if (_input.move != Vector2.zero)
            {
                // =================================================
                // ALTERADO
                //
                // S é tratado de maneira diferente.
                //
                // Se Y < 0:
                //
                //      personagem NÃO gira
                //
                //      câmera NÃO gira
                //
                //      personagem simplesmente anda na direção
                //      oposta à câmera.
                //
                // Isso resolve o problema de S fazer o personagem
                // ficar girando.
                // =================================================

                if (_input.move.y >= 0.0f)
                {
                    // =============================================
                    // W / A / D
                    //
                    // Nesses casos o personagem pode virar para
                    // acompanhar a direção do movimento.
                    // =============================================

                    _targetRotation =
                        Mathf.Atan2(
                            targetDirection.x,
                            targetDirection.z
                        ) * Mathf.Rad2Deg;


                    float rotation =
                        Mathf.SmoothDampAngle(
                            transform.eulerAngles.y,
                            _targetRotation,
                            ref _rotationVelocity,
                            RotationSmoothTime
                        );


                    transform.rotation =
                        Quaternion.Euler(
                            0.0f,
                            rotation,
                            0.0f
                        );
                }
            }


            // =====================================================
            // MOVIMENTO ABSOLUTO
            //
            // O CharacterController.Move recebe a direção final
            // calculada acima.
            //
            // Quando S é pressionado, targetDirection aponta para
            // trás em relação à câmera.
            //
            // Como não alteramos transform.rotation nesse caso,
            // o personagem anda para trás sem girar.
            // =====================================================

            if (targetDirection.sqrMagnitude > 0.001f)
            {
                targetDirection.Normalize();

                _controller.Move(
                    targetDirection *
                    (_speed * Time.deltaTime)
                    +
                    new Vector3(
                        0.0f,
                        _verticalVelocity,
                        0.0f
                    ) * Time.deltaTime
                );
            }
            else
            {
                // Mantém a gravidade mesmo sem movimento horizontal.

                _controller.Move(
                    new Vector3(
                        0.0f,
                        _verticalVelocity,
                        0.0f
                    ) * Time.deltaTime
                );
            }


            // =====================================================
            // ANIMAÇÃO
            // =====================================================

            if (_hasAnimator)
            {
                _animator.SetFloat(
                    _animIDSpeed,
                    _animationBlend
                );

                _animator.SetFloat(
                    _animIDMotionSpeed,
                    inputMagnitude
                );
            }
        }


        // =========================================================
        // PULO E GRAVIDADE
        // =========================================================

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // Reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;


                // Update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(
                        _animIDJump,
                        false
                    );

                    _animator.SetBool(
                        _animIDFreeFall,
                        false
                    );
                }


                // Stop velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }


                // Jump
                if (
                    _input.jump &&
                    _jumpTimeoutDelta <= 0.0f
                )
                {
                    _verticalVelocity =
                        Mathf.Sqrt(
                            JumpHeight *
                            -2f *
                            Gravity
                        );


                    // Update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(
                            _animIDJump,
                            true
                        );
                    }
                }


                // Jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -=
                        Time.deltaTime;
                }
            }
            else
            {
                // Reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;


                // Fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -=
                        Time.deltaTime;
                }
                else
                {
                    // Update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(
                            _animIDFreeFall,
                            true
                        );
                    }
                }


                // If we are not grounded, do not jump
                _input.jump = false;
            }


            // Apply gravity over time
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity +=
                    Gravity * Time.deltaTime;
            }
        }


        // =========================================================
        // AUMENTO DE VELOCIDADE AO PEGAR ESTRELA
        // =========================================================

        public void IncreaseSpeed()
        {
            MoveSpeed += SpeedIncreasePerCoin;
            SprintSpeed += SpeedIncreasePerCoin;
        }


        // =========================================================
        // CLAMP ANGLE
        // =========================================================

        private static float ClampAngle(
            float lfAngle,
            float lfMin,
            float lfMax
        )
        {
            if (lfAngle < -360f)
            {
                lfAngle += 360f;
            }


            if (lfAngle > 360f)
            {
                lfAngle -= 360f;
            }


            return Mathf.Clamp(
                lfAngle,
                lfMin,
                lfMax
            );
        }


        // =========================================================
        // GIZMOS
        // =========================================================

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen =
                new Color(
                    0.0f,
                    1.0f,
                    0.0f,
                    0.35f
                );


            Color transparentRed =
                new Color(
                    1.0f,
                    0.0f,
                    0.0f,
                    0.35f
                );


            if (Grounded)
            {
                Gizmos.color =
                    transparentGreen;
            }
            else
            {
                Gizmos.color =
                    transparentRed;
            }


            Gizmos.DrawSphere(
                new Vector3(
                    transform.position.x,
                    transform.position.y - GroundedOffset,
                    transform.position.z
                ),
                GroundedRadius
            );
        }


        // =========================================================
        // FOOTSTEP
        // =========================================================

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (
                animationEvent.animatorClipInfo.weight >
                0.5f
            )
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index =
                        Random.Range(
                            0,
                            FootstepAudioClips.Length
                        );


                    AudioSource.PlayClipAtPoint(
                        FootstepAudioClips[index],
                        transform.TransformPoint(
                            _controller.center
                        ),
                        FootstepAudioVolume
                    );
                }
            }
        }


        // =========================================================
        // LAND
        // =========================================================

        private void OnLand(AnimationEvent animationEvent)
        {
            if (
                animationEvent.animatorClipInfo.weight >
                0.5f
            )
            {
                AudioSource.PlayClipAtPoint(
                    LandingAudioClip,
                    transform.TransformPoint(
                        _controller.center
                    ),
                    FootstepAudioVolume
                );
            }
        }


        // =========================================================
        // RESET DA CÂMERA
        // =========================================================

        public void ResetCameraRotation(
            float targetYaw
        )
        {
            // Reset the yaw and pitch
            _cinemachineTargetYaw =
                targetYaw;

            _cinemachineTargetPitch =
                0f;


            // Reset camera target rotation
            CinemachineCameraTarget.transform.rotation =
                Quaternion.Euler(
                    _cinemachineTargetPitch,
                    _cinemachineTargetYaw,
                    0f
                );


            Debug.Log(
                $"Camera Yaw reset to {targetYaw} degrees."
            );
        }
    }
}