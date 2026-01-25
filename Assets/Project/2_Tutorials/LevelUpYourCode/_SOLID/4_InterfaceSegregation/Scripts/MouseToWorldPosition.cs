using DesignPatterns.Utilities;
using UnityEngine;

namespace DesignPatterns.ISP
{
    /// <summary>
    /// 마우스의 스크린 위치를 3D 월드 위치로 변환합니다. 스크린의 일부를
    /// "데드존"으로 무시할 수 있습니다.
    /// </summary>
    public class MouseToWorldPosition : MonoBehaviour
    {
        [Header("Sprite")]
        [Tooltip("마우스 월드 위치를 시각화할 스프라이트")]
        [SerializeField] private SpriteRenderer m_SpriteRenderer;
        [Tooltip("마우스에서 월드 위치를 오프셋할 정도")]
        [SerializeField] private Vector3 m_Offset;

        [Header("Raycasting parameters")]
        [Tooltip("레이캐스트에 사용할 카메라")]
        [SerializeField] Camera m_MainCamera;
        [Tooltip("레이 캐스팅을 이 레이어로 제한")]
        [SerializeField] private LayerMask m_LayerMask;
        [Tooltip("데드존으로 무시할 직사각형 스크린 영역 (스크린의 백분율); (x,y) = (하단, 좌측)")]
        [SerializeField] ScreenDeadZone m_ScreenDeadZone;


        public ScreenDeadZone ScreenDeadZone => m_ScreenDeadZone;
        public Vector3 Offset => m_Offset;
        
        private Vector3 m_Position;
        public Vector3 Position => m_Position;

        private void Awake()
        {
            if (m_MainCamera == null)
                m_MainCamera = Camera.main;
            
        }

        // Raycast를 사용하여 마우스 위치를 3D 월드 좌표로 변환
        private Vector3 GetMouseWorldSpacePosition()
        {
            if (m_MainCamera == null)
            {
                Debug.Log("[MouseToWorldPosition]: 메인 카메라가 없습니다");
                return Vector3.zero;
            }

            RaycastHit hit;
            // 카메라에서 마우스 위치로 레이 캐스트
            Ray ray = m_MainCamera.ScreenPointToRay(Input.mousePosition);

            // 레이가 객체에 충돌하면, 교차점 반환
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, m_LayerMask))
            {
                Transform objectHit = hit.transform;
                return new Vector3(hit.point.x, hit.point.y, hit.point.z);
            }

            return Vector3.zero;
        }
        
        
        private void Update()
        {
            if (m_ScreenDeadZone.IsMouseInDeadZone())
            {
                m_SpriteRenderer.enabled = false;
                return;
            }
            
            m_Position = GetMouseWorldSpacePosition();
            
            if (m_SpriteRenderer != null)
            {
                m_SpriteRenderer.enabled = true;
                m_SpriteRenderer.transform.position = Position + m_Offset;
            }
        }
    }
}