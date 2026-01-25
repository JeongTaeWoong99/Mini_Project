using UnityEngine;

namespace DesignPatterns.ObjectPool
{
    /// <summary>
    /// 직접 구현한 오브젝트 풀을 사용하는 총기 클래스
    /// ObjectPool에서 발사체를 가져와서 발사한다.
    /// </summary>
    public class ExampleGun : MonoBehaviour
    {
        [Header("Gun Settings - 총기 설정")]
        [Tooltip("발사할 프리팹")]
        [SerializeField] private GameObject projectile;
        [Tooltip("발사체에 가해지는 힘")]
        [SerializeField] private float      muzzleVelocity = 700f;
        [Tooltip("발사체가 생성되는 총구 위치")]
        [SerializeField] private Transform  muzzlePosition;
        [Tooltip("발사 간격 (작을수록 연사 속도 증가)")]
        [SerializeField] private float      cooldownWindow = 0.1f;

        [Header("Pool Reference - 풀 참조")]
        [Tooltip("오브젝트 풀 참조")]
        [SerializeField] private ObjectPool objectPool;

        // 다음 발사 가능 시간
        private float nextTimeToShoot;

        private void FixedUpdate()
        {
            // 쿨다운이 지났고 Fire1 버튼을 누르면 발사
            if (Input.GetButton("Fire1") && Time.time > nextTimeToShoot && objectPool != null)
            {
                // Instantiate 대신 풀에서 오브젝트를 가져옴
                GameObject bulletObject = objectPool.GetPooledObject().gameObject;

                if (bulletObject == null)
                    return;

                bulletObject.SetActive(true);

                // 총구 위치와 회전에 맞춤
                bulletObject.transform.SetPositionAndRotation(muzzlePosition.position, muzzlePosition.rotation);

                // 발사체를 앞으로 밀어냄
                bulletObject.GetComponent<Rigidbody>().AddForce(bulletObject.transform.forward * muzzleVelocity, ForceMode.Acceleration);

                // 일정 시간 후 풀에 반환되도록 Deactivate 호출
                ExampleProjectile projectile = bulletObject.GetComponent<ExampleProjectile>();
                projectile?.Deactivate();

                // 다음 발사 시간 설정 (쿨다운)
                nextTimeToShoot = Time.time + cooldownWindow;
            }
        }
    }
}
