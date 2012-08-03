using UnityEngine;
using System.Collections;

public class DisabledCamera {
	static Camera camera;
	
	public static Camera Camera {
		get {
			return camera;
		}
		set {
			camera = value;
		}
	}
}
