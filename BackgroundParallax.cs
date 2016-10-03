using UnityEngine;

public class BackgroundParallax : MonoBehaviour
{

	public Transform[] Backgrounds;

	public float ParallaxScale;

	public float ParallaxReductionFactor;

	public float Smoothing;

	private Vector3 _lastPosition;


	public void Start()
	{
		_lastPosition = transform.position;
	}

	public void Update()
	{
		//transform.localPosition = transform.parent.position / -ParallaxScale;
		//var parallaxX = (_lastPosition.x - transform.position.x) * ParallaxScale;
		//var parallaxY = (_lastPosition.y - transform.position.y) * ParallaxScale;

		for (var i = 1; i <= Backgrounds.Length; i++)
		{
			Backgrounds[i-1].position = new Vector3(transform.position.x * i * ParallaxReductionFactor, transform.position.y * i * ParallaxReductionFactor, Backgrounds[i-1].position.z);
			/*  var backgroundTargetPositionX = Backgrounds[i].position.x - parallaxX * (i * ParallaxReductionFactor + i);
			  var backgroundTargetPositionY = Backgrounds[i].position.y - parallaxY * (i * ParallaxReductionFactor + i);
			  Backgrounds[i].position = Vector3.Lerp(
				  Backgrounds[i].position, // from
				  new Vector3(backgroundTargetPositionX, backgroundTargetPositionY, Backgrounds[i].position.z),    // To
				  Smoothing * Time.deltaTime);*/
		}

		//_lastPosition = transform.position;

	}
}


