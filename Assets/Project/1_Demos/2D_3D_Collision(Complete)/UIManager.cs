using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace MiniGame2D3DCollision
{
    /// <summary>
    /// UI 관리 매니저
    /// - 플레이어 HP 슬라이더 갱신
    /// - 샤크별 HP 슬라이더 동적 생성 및 추적
    /// - 게임 오버 텍스트 표시
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager instance;

        [Header("Player HP UI")]
        [SerializeField] private Slider playerHPSlider       = null; // 플레이어 HP 슬라이더
        [SerializeField] private bool   showPlayerHPDebug    = false; // 플레이어 HP 디버그

        [Header("Game Over UI")]
        [SerializeField] private GameObject gameOverObject   = null;   // 게임 오버 
        [SerializeField] private bool       showGameOverDebug = false; // 게임 오버 디버그

        // ═══════════════════════════════════════════════════════════
        // Unity Lifecycle
        // ═══════════════════════════════════════════════════════════

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            // 게임 오버 텍스트 숨기기
            if (gameOverObject != null)
            {
                gameOverObject.gameObject.SetActive(false);
            }

            // 플레이어 HP 슬라이더 초기화
            InitializePlayerHPSlider();
        }

        private void Update()
        {
            // 플레이어 HP 갱신
            UpdatePlayerHPSlider();
        }

        // ═══════════════════════════════════════════════════════════
        // Player HP UI
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// 플레이어 HP 슬라이더 초기화
        /// </summary>
        private void InitializePlayerHPSlider()
        {
            if (playerHPSlider == null) return;
            if (RocketController.instance == null) return;

            int maxHP = RocketController.instance.GetMaxHP();

            playerHPSlider.minValue = 0;
            playerHPSlider.maxValue = maxHP;
            playerHPSlider.value    = maxHP;

            if (showPlayerHPDebug)
            {
                Debug.Log($"[UIManager] 플레이어 HP 슬라이더 초기화 완료 : {maxHP}/{maxHP}");
            }
        }

        /// <summary>
        /// 플레이어 HP 슬라이더 갱신
        /// </summary>
        private void UpdatePlayerHPSlider()
        {
            if (playerHPSlider == null) return;
            if (RocketController.instance == null) return;

            int currentHP = RocketController.instance.GetCurrentHP();

            playerHPSlider.value = currentHP;
        }
        
        // ═══════════════════════════════════════════════════════════
        // Game Over UI
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// 게임 오버 텍스트 표시 (외부 호출용)
        /// </summary>
        public void ShowGameOverText()
        {
            if (gameOverObject == null)
            {
                if (showGameOverDebug)
                {
                    Debug.LogWarning("[UIManager] 게임 오버 텍스트가 설정되지 않았습니다!");
                }
                return;
            }

            gameOverObject.gameObject.SetActive(true);

            if (showGameOverDebug)
            {
                Debug.Log("[UIManager] 게임 오버 텍스트 표시 완료");
            }
        }

        /// <summary>
        /// 게임 오버 텍스트 숨기기 (외부 호출용)
        /// </summary>
        public void HideGameOverText()
        {
            if (gameOverObject == null) return;

            gameOverObject.gameObject.SetActive(false);

            if (showGameOverDebug)
            {
                Debug.Log("[UIManager] 게임 오버 텍스트 숨김 완료");
            }
        }
    }
}
