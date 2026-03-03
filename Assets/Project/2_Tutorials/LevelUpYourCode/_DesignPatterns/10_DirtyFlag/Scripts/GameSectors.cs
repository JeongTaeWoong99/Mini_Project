using System;
using UnityEngine;
using DesignPatterns.StatePattern;
using UnityEngine.Serialization;

namespace DesignPatterns.DirtyFlag
{
    /// <summary>
    /// 플레이어와의 근접도를 기반으로 게임 섹터의 로드/언로드를 관리합니다.
    /// 더티 플래그 패턴을 활용하여 불필요한 업데이트를 최소화함으로써 성능을 최적화합니다.
    /// </summary>
    public class GameSectors : MonoBehaviour
    {
        [Tooltip("PlayerController 참조")]
        public PlayerController m_Player;

        [Tooltip("게임 내 모든 섹터 배열")]
        public Sector[] m_Sectors;

        // 각 섹터의 플레이어 근접도를 확인하고 로드/언로드 상태를 업데이트합니다.
        private void Update()
        {
            if (m_Player == null)
                return;

            foreach (Sector sector in m_Sectors)
            {
                bool isPlayerClose = sector.IsPlayerClose(m_Player.transform.position);

                // 섹터 상태가 변경되어야 하는지 확인
                if (isPlayerClose != sector.IsLoaded)
                {
                    sector.MarkDirty();
                }

                // 더티 플래그를 기반으로 섹터를 업데이트
                // 불필요한 경우 비용이 큰 로드/언로드 연산을 건너뜁니다.
                if (sector.IsDirty)
                {
                    if (isPlayerClose)
                    {
                        sector.LoadContent();
                    }
                    else
                    {
                        sector.UnloadContent();
                    }

                    // 더티 플래그 초기화
                    sector.Clean();
                }
            }
        }

        private void UnloadAllScenes()
        {
            foreach (Sector sector in m_Sectors)
            {
                if (sector.IsLoaded)
                {
                    sector.UnloadContent();
                }
            }
        }

        void OnDestroy()
        {
            // LogLoadedSectors();
            // UnloadAllScenes();
            //
            // Debug.Log("모든 섹터 언로드 중...");
        }

        // 로드된 모든 섹터를 로그에 출력하는 진단 도구
        private void LogLoadedSectors()
        {
            Debug.Log("로드된 섹터 목록 :");
            foreach (Sector sector in m_Sectors)
            {
                if (sector.IsLoaded)
                {
                    Debug.Log($"로드된 섹터 : {sector.name}");
                }
            }
        }
    }
}
