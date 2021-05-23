using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionPainter : MonoBehaviour
{
	[SerializeField] private Color _paintColour;

	[SerializeField] private float _minRadius = 1;
	[SerializeField] private float _maxRadius = 1;
	[SerializeField] private float _strength = 1;
	[SerializeField] private float _hardness = 1;

	private ParticleSystem _part = null;
	private List<ParticleCollisionEvent> _collisionEvents = new List<ParticleCollisionEvent>();

	void Awake()
	{
		_part = GetComponent<ParticleSystem>();
	}

	void OnParticleCollision(GameObject other)
	{
		int numCollisionEvents = _part.GetCollisionEvents(other, _collisionEvents);

		Paintable p = other.GetComponent<Paintable>();
		if(p != null)
		{
			Debug.Log("Particle Collision!");
			for (int i = 0; i< numCollisionEvents; i++)
			{
				Vector3 pos = _collisionEvents[i].intersection;
				float radius = Random.Range(_minRadius, _maxRadius);
				PaintManager.GetInstance().Paint(p, pos, radius, _hardness, _strength, _paintColour);
			}
		}
	}
}