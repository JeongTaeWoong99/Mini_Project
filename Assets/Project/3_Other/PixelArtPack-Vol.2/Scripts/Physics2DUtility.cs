using UnityEngine;

// 2D 물리 관련 보조 계산을 모아둔 정적 유틸리티 클래스
public static class Physics2DUtility
{
	// 지정한 시작 위치에서 특정 방향으로 레이캐스트를 쏴, 해당 레이어까지의 거리를 구한다.
	// 무언가에 맞으면 그 거리를, 아무것도 맞히지 못하면 NO_HIT_DISTANCE 를 반환한다.
	// startPos    : 레이캐스트 시작 위치
	// dir         : 레이캐스트 방향
	// maxDistance : 최대 탐지 거리
	// layerMask   : 탐지 대상 레이어 마스크
	public static float Calculate2DDistanceToLayer(Vector2 startPos, Vector2 dir, float maxDistance, int layerMask)
	{
		RaycastHit2D hit  = Physics2D.Raycast(startPos, dir, maxDistance, layerMask);
		float        dist = PixelArtPackConstants.NO_HIT_DISTANCE;

		// 충돌체가 존재하면(무언가에 맞았으면) 실제 거리로 갱신
		if (null != hit.collider)
		{
			dist = hit.distance;
		}

		return dist;
	}
}
