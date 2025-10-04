using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class CameraFuc : MonoBehaviour
{
	public CinemachineFollow     cinemaFollow;
	public CinemachineHardLookAt cinemaLookAt;
	
	public List<Vector3> followViewList;
	public List<Vector3> lookAtViewList;
	
	public void ChangeSideViewOffset()
	{
		if (cinemaFollow != null)
		{
			cinemaLookAt.LookAtOffset =	lookAtViewList[0];
			cinemaFollow.FollowOffset = followViewList[0];
		}
	}
	
	public void ChangeAttackViewOffset()
	{
		if (cinemaFollow != null)
		{
			cinemaLookAt.LookAtOffset =	lookAtViewList[1];
			cinemaFollow.FollowOffset = followViewList[1];
		}
	}
}