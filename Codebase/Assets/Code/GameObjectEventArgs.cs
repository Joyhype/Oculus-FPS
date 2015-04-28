using System;
using UnityEngine;

public class GameObjectEventArgs : EventArgs {

	public Transform @Transform;
	public Collider @Collider;
	public Collider2D @Collider2D;
	public Renderer @Renderer;
	public Rigidbody @Rigidbody;
	public Rigidbody2D @Rigidbody2D;

	public GameObjectEventArgs( GameObject gameObject ) : base() {
		Transform = gameObject.transform;
		Collider2D = gameObject.GetComponent<Collider2D>();
		Collider = gameObject.GetComponent<Collider>();
		Renderer = gameObject.GetComponent<Renderer>();
		Rigidbody = gameObject.GetComponent<Rigidbody>();
		Rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
	}
}