using UnityEngine;

namespace DesignPatterns.Strategy
{
    public class Collectible : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            // 플레이어 오브젝트에 "Player" 태그를 지정해야 한다
            if (other.CompareTag("Player"))
            {
                // 이벤트 채널을 통해 리스너에게 알린다
                GameEvents.CollectibleCollected();

                // 수집된 후 오브젝트를 파괴한다
                Destroy(gameObject);
            }
        }
    }
}
