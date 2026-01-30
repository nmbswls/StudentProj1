using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(CharacterController))]
public class ImmersiveFpsController : MonoBehaviour
{
    [Header("--- 基础设置 ---")]
    public float walkSpeed = 5.0f;
    public float sprintSpeed = 8.0f;
    [Range(0, 1)] public float airControlPercent = 0.5f; // 空中控制力

    [Header("--- 视角设置 ---")]
    public float mouseSensitivity = 2.0f;
    public float lookUpLimit = 80f;
    public float lookDownLimit = -80f;

    [Header("--- 物理设置 ---")]
    public float gravity = -20.0f;  // 游戏里的重力通常比现实大才舒服(-9.8太飘)
    public float jumpHeight = 1.2f;

    [Header("--- 沉浸感 (Head Bob) ---")]
    public bool enableHeadBob = true;
    public Transform cameraRoot;    // 拖入 CameraRoot
    public float bobFrequency = 10.0f; // 晃动频率 (脚步快慢)
    public float bobAmplitude = 0.05f; // 晃动幅度 (颠簸程度)

    // 内部变量
    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;
    private float defaultYPos = 0;
    private float bobTimer = 0;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // 记录相机初始高度
        if (cameraRoot != null) defaultYPos = cameraRoot.localPosition.y;

        // 隐藏并锁定鼠标
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    void Update()
    {
        HandleLook();
        HandleMovement();
        if (enableHeadBob) HandleHeadBob();
    }

    // 1. 视角逻辑
    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 上下看：转动 CameraRoot (X轴)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, lookDownLimit, lookUpLimit);
        cameraRoot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 左右看：转动 玩家身体 (Y轴)
        transform.Rotate(Vector3.up * mouseX);
    }

    // 2. 移动与重力逻辑
    void HandleMovement()
    {
        // --- 地面检测 ---
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // 施加微小下压力确保贴地
        }

        // --- 输入 ---
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // 只有按住Shift且移动时才加速
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && z > 0;
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        Vector3 move = transform.right * x + transform.forward * z;

        // --- 移动 ---
        // 简单的地面移动
        controller.Move(move * currentSpeed * Time.deltaTime);

        //// --- 跳跃 ---
        //if (Input.GetButtonDown("Jump") && controller.isGrounded)
        //{
        //    // 物理公式：v = sqrt(h * -2 * g)
        //    velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        //}

        // --- 重力应用 ---
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // 3. 沉浸感：头部晃动 (模拟走路时的颠簸)
    void HandleHeadBob()
    {
        if (!controller.isGrounded) return; // 空中不晃动

        // 只有在移动时才晃动
        if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f || Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f)
        {
            // 计算正弦波
            bobTimer += Time.deltaTime * bobFrequency;

            // 核心算法：y = 默认高度 + sin(时间) * 幅度
            float newY = defaultYPos + Mathf.Sin(bobTimer) * bobAmplitude;

            cameraRoot.localPosition = new Vector3(cameraRoot.localPosition.x, newY, cameraRoot.localPosition.z);
        }
        else
        {
            // 停止移动时，平滑归位
            bobTimer = 0;
            Vector3 targetPos = new Vector3(cameraRoot.localPosition.x, defaultYPos, cameraRoot.localPosition.z);
            cameraRoot.localPosition = Vector3.Lerp(cameraRoot.localPosition, targetPos, Time.deltaTime * 10f);
        }
    }
}