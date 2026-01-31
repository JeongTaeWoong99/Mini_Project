using UnityEngine;

namespace DesignPatterns.StatePattern
{
    /// <summary>
    /// 플레이어 입력을 처리하고 입력 벡터를 플레이어 컨트롤러에 제공합니다.
    /// </summary>
    public class PlayerInput : MonoBehaviour
    {
        // 기존 Input 클래스 사용
        [Header("Controls")]
        [SerializeField] private KeyCode forward = KeyCode.W;
        [SerializeField] private KeyCode back    = KeyCode.S;
        [SerializeField] private KeyCode left    = KeyCode.A;
        [SerializeField] private KeyCode right   = KeyCode.D;
        [SerializeField] private KeyCode jump    = KeyCode.Space;

        public Vector3 InputVector => inputVector;
        public bool    IsJumping   { get => isJumping; set => isJumping = value; }

        private Vector3 inputVector;
        private bool    isJumping;
        private float   xInput;
        private float   zInput;
        private float   yInput;

        public void HandleInput()
        {
            // 입력 초기화
            xInput = 0;
            yInput = 0;
            zInput = 0;

            if (Input.GetKey(forward))
            {
                zInput++;
            }

            if (Input.GetKey(back))
            {
                zInput--;
            }

            if (Input.GetKey(left))
            {
                xInput--;
            }

            if (Input.GetKey(right))
            {
                xInput++;
            }

            inputVector = new Vector3(xInput, yInput, zInput);

            isJumping = Input.GetKeyDown(jump);
        }

        private void Update()
        {
            HandleInput();
        }
    }
}
