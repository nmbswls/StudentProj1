using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class SimpleCharacterController : MonoBehaviour
{
    [Header("=== 核心手感配置 ===")]
    [SerializeField] private float m_MoveSpeed = 10f;
    [SerializeField] private float m_TurnSpeed = 20f;

    [Header("=== 暴力跳跃系统 ===")]
    // 注意：因为重力变大了，需要的力必须大幅增加！
    [Tooltip("跳跃爆发力 (建议设为 30 - 40)")]
    [SerializeField] private float m_JumpForce = 35f;

    [Tooltip("上升时的重力倍率 (建议 3 - 4，越大起跳越干脆)")]
    [SerializeField] private float m_RiseGravityMultiplier = 3f;

    [Tooltip("下落时的重力倍率 (建议 4 - 6，越大落地越快)")]
    [SerializeField] private float m_FallMultiplier = 5f;

    [Tooltip("最大下落速度限制")]
    [SerializeField] private float m_MaxFallSpeed = 30f;

    [Header("=== 平滑设置 ===")]
    [SerializeField] private float m_GroundSmoothing = 0.05f;
    [SerializeField] private float m_AirSmoothing = 0.1f;

    [Header("=== 检测设置 ===")]
    [SerializeField] private LayerMask m_GroundLayer;
    [SerializeField] private Transform m_GroundCheck;
    [SerializeField] private float m_GroundCheckRadius = 0.25f;
    [SerializeField] private Transform m_ModelTransform;

    private Rigidbody m_Rb;
    private bool m_IsGrounded;
    private Vector3 m_Velocity = Vector3.zero;
    private float m_InputX;
    private bool m_JumpRequest;

    private void Awake()
    {
        m_Rb = GetComponent<Rigidbody>();
        m_Rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        m_Rb.useGravity = true;
        m_Rb.interpolation = RigidbodyInterpolation.Interpolate;
        m_Rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        if (m_ModelTransform == null && transform.childCount > 0)
            m_ModelTransform = transform.GetChild(0);
    }

    private void Update()
    {
        m_InputX = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space) && m_IsGrounded)
        {
            m_JumpRequest = true;
        }
    }

    private void FixedUpdate()
    {
        CheckGrounded();
        ApplyGravityModifier();
        Move();

        if (m_JumpRequest)
        {
            Jump();
            m_JumpRequest = false;
        }
    }

    private void CheckGrounded()
    {
        m_IsGrounded = false;
        Collider[] colliders = Physics.OverlapSphere(m_GroundCheck.position, m_GroundCheckRadius, m_GroundLayer);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
                m_IsGrounded = true;
        }
    }

    private void Move()
    {
        float currentSmoothing = m_IsGrounded ? m_GroundSmoothing : m_AirSmoothing;
        Vector3 targetVelocity = new Vector3(m_InputX * m_MoveSpeed, m_Rb.velocity.y, 0);
        m_Rb.velocity = Vector3.SmoothDamp(m_Rb.velocity, targetVelocity, ref m_Velocity, currentSmoothing);

        if (Mathf.Abs(m_InputX) > 0.01f && m_ModelTransform != null)
        {
            float targetAngle = m_InputX > 0 ? 90f : -90f;
            Quaternion targetRot = Quaternion.Euler(0, targetAngle, 0);
            m_ModelTransform.rotation = Quaternion.Slerp(m_ModelTransform.rotation, targetRot, Time.fixedDeltaTime * m_TurnSpeed);
        }
    }

    private void Jump()
    {
        m_Rb.velocity = new Vector3(m_Rb.velocity.x, 0, 0);
        // 因为我们加大了上升重力，这里需要更大的爆发力来对抗它
        m_Rb.AddForce(Vector3.up * m_JumpForce, ForceMode.Impulse);
    }

    private void ApplyGravityModifier()
    {
        // === 核心逻辑修改 ===

        // 1. 如果正在下落 (速度向下)
        if (m_Rb.velocity.y < 0)
        {
            // 使用下落倍率 (FallMultiplier) - 快速砸向地面
            m_Rb.velocity += Vector3.up * Physics.gravity.y * (m_FallMultiplier - 1) * Time.fixedDeltaTime;
        }
        // 2. 如果正在上升 (速度向上)
        else if (m_Rb.velocity.y > 0)
        {
            // 使用上升倍率 (RiseGravityMultiplier) - 解决"上升轻飘飘"的问题
            // 这会让你即使在上升时，也能感受到强大的地心引力，必须靠更大的 Force 冲上去
            m_Rb.velocity += Vector3.up * Physics.gravity.y * (m_RiseGravityMultiplier - 1) * Time.fixedDeltaTime;
        }

        // 速度限制
        if (m_Rb.velocity.y < -m_MaxFallSpeed)
        {
            m_Rb.velocity = new Vector3(m_Rb.velocity.x, -m_MaxFallSpeed, m_Rb.velocity.z);
        }
    }
}