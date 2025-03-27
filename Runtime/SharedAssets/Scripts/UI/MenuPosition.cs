using UnityEngine;

public class MenuPosition : MonoBehaviour
{
	public Vector3 BottomLeftOffset;
	public Vector3 TopLeftOffset;
	public Vector3 BottomRightOffset;
	public Vector3 TopRightOffset;

	void OnEnable()
	{
		//Bottom Left Screen Offset
		if (Input.mousePosition.x < Screen.width / 2 && Input.mousePosition.y < Screen.height / 2)
		{
			transform.position = Input.mousePosition + BottomLeftOffset;
		}

		//Top Left Screen Offset
		if (Input.mousePosition.x < Screen.width / 2 && Input.mousePosition.y > Screen.height / 2)
		{
			transform.position = Input.mousePosition + TopLeftOffset;
		}

		//Bottom Right Screen Offset
		if (Input.mousePosition.x > Screen.width / 2 && Input.mousePosition.y < Screen.height / 2)
		{
			transform.position = Input.mousePosition + BottomRightOffset;
		}

		//Top Right Screen Offset
		if (Input.mousePosition.x > Screen.width / 2 && Input.mousePosition.y > Screen.height / 2)
		{
			transform.position = Input.mousePosition + TopRightOffset;
		}
	}
}
