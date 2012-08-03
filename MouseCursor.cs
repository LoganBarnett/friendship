using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MouseCursor : MonoBehaviour {
	static GameObject mouseOverObject;
	static Texture2D clickableTexture;
	static Texture2D cursorTexture;
	static MouseCursor mouse;
	static bool showCursor = false;
	static float dragThresholdInPixels = 3f;
	static Camera guiCamera;
	static Dictionary<Camera, GameObject> cameraRaycasts = new Dictionary<Camera, GameObject>();
	int mask = 0;
	
	Vector2 lastMouseScreenPosition;
	bool mouseHeldLastFrame;
	bool dragging;
	GameObject draggedObject;
	Camera[] cameras;
	
	
	public static bool IsMouseOver(GameObject target, Camera camera) {
		return IsMouseOver(target, camera, ~0);
	}
	
	// TODO: Distance param
	public static bool IsMouseOver(GameObject target, Camera camera, LayerMask mask) {
		EnsureMouseObject();
		if(camera == null) camera = Camera.main;
		if(camera == DisabledCamera.Camera) return false;
		
		return DidHitTargetOnCamera(camera, target);
	}
	
	public static bool IsMouseDown(GameObject target) {
		return IsMouseDown(target, null);
	}
	
	public static bool IsMouseDown(GameObject target, Camera camera) {
		return IsMouseDown(target, camera, ~0);
	}
	
	// TODO: Distance param
	public static bool IsMouseDown(GameObject target, Camera camera, LayerMask mask) {
		if(!Input.GetMouseButtonDown(0)) return false;
		
		if(camera == null) camera = Camera.main;
		if(camera == DisabledCamera.Camera) return false;
		
		EnsureMouseObject();
		return DidHitTargetOnCamera(camera, target);
	}
	
	public static bool IsMouseHeldDown(GameObject target, Camera camera) {
		return IsMouseHeldDown(target, camera, ~0);
	}
	
	public static bool IsMouseHeldDown(GameObject target, Camera camera, LayerMask mask) {
		EnsureMouseObject();
		if(camera == null) camera = Camera.main;
		if(camera == DisabledCamera.Camera) return false;
		
		if(!Input.GetMouseButton(0)) return false;
		
		return DidHitTargetOnCamera(camera, target);
	}
	
	public static bool IsMouseUp(GameObject target, Camera camera) {
		return IsMouseUp(target, camera, ~0);
	}
	
	public static bool IsMouseUp(GameObject target, Camera camera, LayerMask mask) {
		EnsureMouseObject();
		if(camera == null) camera = Camera.main;
		if(camera == DisabledCamera.Camera) return false;
		
		if(!Input.GetMouseButtonUp(0)) return false;
		
		return DidHitTargetOnCamera(camera, target);
	}
	
	public static bool IsMouseDragged(GameObject target, Camera camera) {
		return IsMouseDragged(target, camera, ~0);
	}
	
	public static bool IsMouseDragged(GameObject target, Camera camera, LayerMask mask) {
		EnsureMouseObject();
		if(camera == null) camera = Camera.main;
		if(camera == DisabledCamera.Camera) return false;
		
//		if(!Input.GetMouseButton(0)) {
//			lastMouseScreenPosition = Input.mousePosition;
//		}
		
		return mouse.draggedObject == target && mouse.dragging;
	}
	
	public static void ShowClickableCursor(bool show) {
		EnsureMouseObject();
		if(show) showCursor = true;
	}
	
	public static void EnsureMouseObject() {
		if(mouse == null) mouse = new GameObject("Mouse", typeof(MouseCursor)).GetComponent<MouseCursor>();
	}
	
	static bool DidHitTargetOnCamera(Camera camera, GameObject target) {
		if(!cameraRaycasts.ContainsKey(camera)) return false;
		var raycastHitResult = cameraRaycasts[camera];
		if(raycastHitResult == null) return false;
		return raycastHitResult == target || target.GetComponentsInChildren<Collider>().Any(c => c == raycastHitResult.collider);
	}
	
	public static Camera GuiCamera {
		get { return guiCamera; }
	}
	
	public void Start() {
//		yield return new WaitForEndOfFrame();
		cameras = Resources.FindObjectsOfTypeAll(typeof(Camera)) as Camera[];
		var camera = GameObject.Find("GUI/GUI Camera");
		if(camera == null) camera = GameObject.Find("Main Camera");
		guiCamera = camera.GetComponent<Camera>();
		mask = 1 << LayerMask.NameToLayer("Indoors") | 1 << LayerMask.NameToLayer("Ignore Raycast");
	}
	
	public void Update() {
		showCursor = false;
		
		
		CacheRaycastResultsOverMousePosition();
		
		DetermineDragResults();
	}
	
	public void OnGUI() {
		if(clickableTexture == null) clickableTexture = (Texture2D) Resources.Load("GUI/MouseCursor_Clickable01");				
		if(cursorTexture == null) cursorTexture = (Texture2D) Resources.Load("GUI/MouseCursor_Normal01");
		
		if(showCursor) {
			var currentEvent = Event.current;
			GUI.DrawTexture(new Rect(currentEvent.mousePosition.x - 14, (currentEvent.mousePosition.y) - 4, 32, 32), clickableTexture);
			Screen.showCursor = false;
		}
		else Screen.showCursor = true;
	}
	
	void DetermineDragResults() {
		var mouseHeld = Input.GetMouseButton(0);
		
		if(mouseHeld && !mouseHeldLastFrame) {
			draggedObject = cameraRaycasts[GuiCamera]; // TODO: Make this work for multiple cameras
		}
		else if(mouseHeld && mouseHeldLastFrame && (dragging || Vector2.Distance(lastMouseScreenPosition, Input.mousePosition) > dragThresholdInPixels)) {
			dragging = true;
		}
		else if(!mouseHeld) {
			mouseHeldLastFrame = false;
			dragging = false;
		}
		
		if(mouseHeld) {
			lastMouseScreenPosition = Input.mousePosition;
			mouseHeldLastFrame = true;
		}
	}
		
	
	void CacheRaycastResultsOverMousePosition() {
		RaycastHit hitInfo;
		foreach(var camera in cameras) {
			if(Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hitInfo, Mathf.Infinity, ~mask)) {
				cameraRaycasts[camera] = hitInfo.collider.gameObject;
			}
			else cameraRaycasts[camera] = null;
		}
	}
}