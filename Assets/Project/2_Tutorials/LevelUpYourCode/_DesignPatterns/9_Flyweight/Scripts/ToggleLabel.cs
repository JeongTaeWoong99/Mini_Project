using System;
using UnityEngine;
using TMPro;


namespace DesignPatterns
{
    /// <summary>
    /// GameObject의 활성화 상태를 감시하여 TMPro UGUI 라벨을 업데이트하는 컴포넌트
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class ToggleLabel : MonoBehaviour
    {
        [Tooltip("활성화 상태를 감시할 오브젝트")]
        [SerializeField] private GameObject m_EnabledObject;
        [Header("라벨")]
        [Tooltip("활성화/비활성화 상태 텍스트 앞에 표시할 접두사")]
        [SerializeField] private string m_LabelPrefix;
        [Tooltip("감시 오브젝트가 활성화 상태일 때 표시할 텍스트")]
        [SerializeField] private string m_EnabledString;
        [Tooltip("감시 오브젝트가 비활성화 상태일 때 표시할 텍스트")]
        [SerializeField] private string m_DisabledString;

        // 필수 TextMeshPro UGUI 라벨
        TextMeshProUGUI m_TextLabel;

        private void Awake()
        {
            // 동일한 GameObject에서 TextMeshProUGUI 컴포넌트 탐색
            m_TextLabel = GetComponent<TextMeshProUGUI>();
        }

        void Start()
        {
            UpdateLabel();
        }

        // 감시 오브젝트의 활성화 상태에 따라 라벨 업데이트
        public void UpdateLabel()
        {
            // 라벨 또는 감시 오브젝트가 없으면 종료
            if (m_TextLabel == null || m_EnabledObject == null)
                return;

            bool state = m_EnabledObject.gameObject.activeSelf;

            m_TextLabel.text = $"{m_LabelPrefix} {(state ? m_EnabledString : m_DisabledString)}";
        }
    }
}
