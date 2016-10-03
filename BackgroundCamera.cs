using UnityEngine;
using System.Collections;

public class BackgroundCamera : MonoBehaviour
{

	public float scrollingPercent;

	void FixedUpdate()
	{
		Vector2 target = BeardCamera.Instance.transform.position * scrollingPercent;
		transform.position = new Vector3(target.x,target.y,transform.position.z);
	}
}
