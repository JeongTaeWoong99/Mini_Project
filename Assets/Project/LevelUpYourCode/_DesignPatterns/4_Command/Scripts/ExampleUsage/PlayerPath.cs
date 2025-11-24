using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace DesignPatterns.Command
{
    // 플레이어의 이동 경로를 시각화하는 도구 클래스
    // 명령 실행/취소 시 경로를 추가/제거하여 히스토리를 시각적으로 표현
    public class PlayerPath : MonoBehaviour
    {
        // 경로를 표시할 점(Point) 프리팹
        [SerializeField] private GameObject pathPointPrefab;
        [SerializeField] Transform          pathTransform;

        // 각 경로 점의 오프셋 (높이 조정 등)
        [SerializeField] Vector3 offset;
        private Stack<GameObject> pathObjects = new Stack<GameObject>();  // 생성된 경로 점들

        // 경로 점들을 연결하는 LineRenderer
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] List<Vector3>        pointList;

        private void Start()
        {
            // 시작 위치를 경로에 추가
            AddToPath(transform.position);
        }

        // 경로에 새로운 점 추가
        // 명령 실행(Execute) 시 호출됨
        public void AddToPath(Vector3 position)
        {
            if (pathPointPrefab == null)
            {
                return;
            }

            // 지정된 위치에 경로 점 생성
            GameObject newPathObject = Object.Instantiate(pathPointPrefab, position + offset, Quaternion.identity) as GameObject;

            // 스택에 추가 (Undo를 위해)
            pathObjects?.Push(newPathObject);

            // 부모 설정 (계층 구조 정리)
            if (pathTransform != null)
            {
                newPathObject.transform.parent = pathTransform;
            }

            // LineRenderer 업데이트 : 점들의 위치 갱신
            pointList = pathObjects.Select(x => x.transform.position).ToList();
            lineRenderer.positionCount = pointList.Count;
        }

        // 경로에서 마지막 점 제거
        // 명령 취소(Undo) 시 호출됨
        public void RemoveFromPath()
        {
            GameObject lastObject = pathObjects.Pop();  // 스택에서 꺼내기
            Destroy(lastObject.gameObject);             // 오브젝트 삭제

            // LineRenderer 업데이트 : 점 목록 갱신
            pointList = pathObjects.Select(x => x.transform.position).ToList();
            lineRenderer.positionCount = pointList.Count;
        }

        private void Update()
        {
            // LineRenderer는 매 프레임 위치 업데이트 필요
            lineRenderer.SetPositions(pointList.ToArray());
        }
    }
}