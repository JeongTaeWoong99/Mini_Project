using System;
using UnityEngine;

using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace DesignPatterns.ISP
{

    public class ExplosionEffect : MonoBehaviour
    {
        [SerializeField] private float explosionForce = 1000f;

        [Tooltip("모든 자식 파편 조각들")]
        [SerializeField] List<Rigidbody> m_Rigidbodies = new List<Rigidbody>();

        void Start()
        {
            Explode();
        }

        public void Explode()
        {
            foreach (Rigidbody rigidbody in m_Rigidbodies)
            {

                // 폭발 힘 적용
                Vector3 explosionDirection = (rigidbody.position - transform.position).normalized;
                Vector3 force = explosionDirection * explosionForce + Random.insideUnitSphere * (explosionForce * 0.5f);

                rigidbody.AddForce(force);

                // 폭발 토크 적용
                rigidbody.AddTorque(Random.insideUnitSphere * explosionForce);
            }
        }
    }
}
