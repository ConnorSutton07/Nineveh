using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Reflection;

[CustomEditor(typeof(LightningBolt2D))]
public class LightningBolt2DEditor:Editor{

	LightningBolt2D script;
	Plane objectPlane; //Mostly to position controls using plane's normal
	Rect windowRect; //For start and end points positions window

	enum PlayButtons{
		On,
		Off
	}

	#region Create and initialize the object

	[MenuItem("GameObject/Effects/Lightning Bolt 2D")]
	static void Create(){
		GameObject go=new GameObject();
		go.AddComponent<LightningBolt2D>();
		go.name="Lightning Bolt 2D";
		SceneView sc=SceneView.lastActiveSceneView!=null?SceneView.lastActiveSceneView:SceneView.sceneViews[0] as SceneView;
		go.transform.position=new Vector3(sc.pivot.x,sc.pivot.y,0f);
		if(Selection.activeGameObject!=null) go.transform.parent=Selection.activeGameObject.transform;
		Selection.activeGameObject=go;
	}

	void Awake(){
		script=(LightningBolt2D)target;
	}

	#endregion

	#region Inspector window

	public override void OnInspectorGUI(){
		bool forceRepaint=false;
		if(Event.current.type==EventType.ValidateCommand){
			if(Event.current.commandName=="UndoRedoPerformed") forceRepaint=true;
		}

		int arcCount=EditorGUILayout.IntSlider(new GUIContent("Arc count","How many lightning arcs to draw"),script.arcCount,1,30);
		if(arcCount!=script.arcCount){
			Undo.RecordObject(script,"Change arc count");
			script.arcCount=arcCount;
			EditorUtility.SetDirty(script);
		}

		int pointCount=EditorGUILayout.IntSlider(new GUIContent("Point count","How many points each individual lightning consists of, including start and end"),script.pointCount,3,50);
		if(pointCount!=script.pointCount){
			Undo.RecordObject(script,"Change point count");
			script.pointCount=pointCount;
			EditorUtility.SetDirty(script);
		}

		GUILayout.Space(8);

		Color lineColor=EditorGUILayout.ColorField(new GUIContent("Line color","Color of the lightning"),script.lineColor);
		if(script.lineColor!=lineColor){
			Undo.RecordObject(script,"Change line color");
			script.lineColor=lineColor;
			EditorUtility.SetDirty(script);
		}

		float lineWidth=EditorGUILayout.FloatField(new GUIContent("Line width","Thickness of the lightning line"),script.lineWidth);
		if(lineWidth!=script.lineWidth){
			lineWidth=Mathf.Max(0f,lineWidth);
			Undo.RecordObject(script,"Change line width");
			script.lineWidth=lineWidth;
			EditorUtility.SetDirty(script);
		}

		Color glowColor=EditorGUILayout.ColorField(new GUIContent("Glow color","Color of the glow around the lightning"),script.glowColor);
		if(script.glowColor!=glowColor){
			Undo.RecordObject(script,"Change glow color");
			script.glowColor=glowColor;
			script.glowEdgeColor=new Color(glowColor.r,glowColor.g,glowColor.b,0f);
			EditorUtility.SetDirty(script);
		}

		float glowWidth=EditorGUILayout.FloatField(new GUIContent("Glow width","The gradient outside of the line. Setting it to zero removes its geometry"),script.glowWidth);
		if(glowWidth!=script.glowWidth){
			glowWidth=Mathf.Max(0f,glowWidth);
			Undo.RecordObject(script,"Change glow width");
			script.glowWidth=glowWidth;
			EditorUtility.SetDirty(script);
		}

		GUILayout.Space(8);

		float distort=EditorGUILayout.FloatField(new GUIContent("Distort","How big the distortion spikes will be relative to the distance between endpoints. Set zeto to make completely smooth line"),script.distort);
		if(distort!=script.distort){
			distort=Mathf.Max(0f,distort);
			Undo.RecordObject(script,"Change distort");
			script.distort=distort;
			EditorUtility.SetDirty(script);
		}

		float jitter=EditorGUILayout.FloatField(new GUIContent("Jitter","Random displacements of the points when the scene is in play mode"),script.jitter);
		if(jitter!=script.jitter){
			jitter=Mathf.Max(0f,jitter);
			Undo.RecordObject(script,"Change jitter");
			script.jitter=jitter;
			EditorUtility.SetDirty(script);
		}

		GUILayout.Space(8);

		float bend=EditorGUILayout.FloatField(new GUIContent("Bend amount","How far to push the curve, relative to distance between your two points"),script.bend);
		if(bend!=script.bend){
			bend=Mathf.Max(0f,bend);
			Undo.RecordObject(script,"Change bend amount");
			script.bend=bend;
			EditorUtility.SetDirty(script);
		}

		float bendSpeed=EditorGUILayout.FloatField(new GUIContent("Bend speed","Speed of bending animation while the scene is in play mode. Zero will remove the animation"),script.bendSpeed);
		if(bendSpeed!=script.bendSpeed){
			bendSpeed=Mathf.Max(0f,bendSpeed);
			Undo.RecordObject(script,"Change bend speed");
			script.bendSpeed=bendSpeed;
			EditorUtility.SetDirty(script);
		}

		GUILayout.Space(8);

		float arcLifetimeMin=EditorGUILayout.FloatField(new GUIContent("Arc lifetime min","Lower limit of randomized arc lifetime"),script.arcLifetimeMin);
		if(arcLifetimeMin!=script.arcLifetimeMin){
			arcLifetimeMin=Mathf.Max(0f,arcLifetimeMin);
			arcLifetimeMin=Mathf.Min(arcLifetimeMin,script.arcLifetimeMax);
			Undo.RecordObject(script,"Change arc lifetime minimum");
			script.arcLifetimeMin=arcLifetimeMin;
			EditorUtility.SetDirty(script);
		}

		float arcLifetimeMax=EditorGUILayout.FloatField(new GUIContent("Arc lifetime max","Lower limit of randomized arc lifetime"),script.arcLifetimeMax);
		if(arcLifetimeMax!=script.arcLifetimeMax){
			arcLifetimeMax=Mathf.Max(0f,arcLifetimeMax);
			arcLifetimeMax=Mathf.Max(arcLifetimeMax,script.arcLifetimeMin);
			Undo.RecordObject(script,"Change arc lifetime maximum");
			script.arcLifetimeMax=arcLifetimeMax;
			EditorUtility.SetDirty(script);
		}

		GUILayout.Space(8);

		float FPSLimit=EditorGUILayout.FloatField(new GUIContent("FPS limit","How many times per second this object is allowed to update. Zero means that there's no limit"),script.FPSLimit);
		if(FPSLimit!=script.FPSLimit){
			FPSLimit=Mathf.Max(0f,FPSLimit);
			Undo.RecordObject(script,"Change FPS limit");
			script.FPSLimit=FPSLimit;
			EditorUtility.SetDirty(script);
		}

		//Show triangle count
		GUILayout.Box(new GUIContent(script.triangleCount>1?"The mesh has "+script.triangleCount.ToString()+" triangles":(script.triangleCount==1?"The mesh is just one triangle":"The mesh has no triangles")),EditorStyles.helpBox);

		
		//Turn on and off
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel(new GUIContent("Playback","Turn effect on and off. If set to off and there is still an animation going, it will finish it and then stop"));
		int switchState=GUILayout.Toolbar(script.isPlaying?0:1,PlayButtons.GetNames(typeof(PlayButtons)));
		if(switchState!=(script.isPlaying?0:1)){
			GUI.FocusControl(null);
			script.isPlaying=(switchState==0?true:false);
		}
		EditorGUILayout.EndHorizontal();

		//Fire once
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel(" ");
		EditorGUI.BeginDisabledGroup(script.isPlaying);
		if(GUILayout.Button(new GUIContent("Fire once","Create lightning(s), let them play out to the end and do not create new ones"))){
			script.FireOnce();
			forceRepaint=true;
		}
		EditorGUI.EndDisabledGroup();
		EditorGUILayout.EndHorizontal();

		//Clear and regenerate
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel(" ");
		if(GUILayout.Button(new GUIContent("Regenerate","Clear currently generated mesh and generate the new one"))){
			script.arcs.Clear();
			forceRepaint=true;
		}
		EditorGUILayout.EndHorizontal();

		//Sprite sorting
		GUILayout.Space(10);
		//Get sorting layers
		int[] layerIDs=GetSortingLayerUniqueIDs();
		string[] layerNames=GetSortingLayerNames();
		//Get selected sorting layer
		int selected=-1;
		for(int i=0;i<layerIDs.Length;i++){
			if(layerIDs[i]==script.sortingLayer){
				selected=i;
			}
		}
		//Select Default layer if no other is selected
		if(selected==-1){
			for(int i=0;i<layerIDs.Length;i++){
				if(layerIDs[i]==0){
					selected=i;
				}
			}
		}
		//Sorting layer dropdown
		EditorGUI.BeginChangeCheck();
		GUIContent[] dropdown=new GUIContent[layerNames.Length+2];
		for(int i=0;i<layerNames.Length;i++){
			dropdown[i]=new GUIContent(layerNames[i]);
		}
		dropdown[layerNames.Length]=new GUIContent();
		dropdown[layerNames.Length+1]=new GUIContent("Add Sorting Layer...");
		selected=EditorGUILayout.Popup(new GUIContent("Sorting Layer","Name of the Renderer's sorting layer"),selected,dropdown);
		if(EditorGUI.EndChangeCheck()){
			if(selected==layerNames.Length+1){
				SettingsService.OpenProjectSettings("Project/Tags and Layers");
			}else{
				Undo.RecordObject(script,"Change sorting layer");
				script.sortingLayer=layerIDs[selected];
				EditorUtility.SetDirty(script);
			}
		}
		//Order in layer field
		EditorGUI.BeginChangeCheck();
		int order=EditorGUILayout.IntField(new GUIContent("Order in Layer","Renderer's order within a sorting layer"),script.orderInLayer);
		if(EditorGUI.EndChangeCheck()){
			Undo.RecordObject(script,"Change order in layer");
			script.orderInLayer=order;
			EditorUtility.SetDirty(script);
		}

		//React to changes in GUI
		if(GUI.changed || forceRepaint){
			if(!Application.isPlaying) script.Generate();
			SceneView.RepaintAll();
		}
	}

	#endregion

	#region Scene view

	private void OnSceneGUI(){
		EventType et=Event.current.type; //Need to save this because it can be changed to Used by other functions
		float size=HandleUtility.GetHandleSize(script.transform.position)*0.1f;
		objectPlane=new Plane(
			script.transform.TransformPoint(new Vector3(0,0,0)),
			script.transform.TransformPoint(new Vector3(0,1,0)),
			script.transform.TransformPoint(new Vector3(1,0,0))
		);
		//Moveable handles for start and end points of the lightning
		Handles.color=Color.white;
		drawMoveHandle(ref script.startPoint);
		drawMoveHandle(ref script.endPoint);

		//If object is being dragged, offset start and end points by the drag amount since those point use absolute coordinates
		if((et==EventType.MouseDrag || et==EventType.MouseUp) || script.lastPosition!=script.transform.position){ 
			Vector3 diff=script.transform.position-script.lastPosition;
			script.startPoint+=(Vector2)diff;
			script.endPoint+=(Vector2)diff;
			script.lastPosition=script.transform.position;
		}

		/*
		//Just for debugging. Displays points, lines and bezier handles
		for(int a=0;a<script.arcs.Count;a++){
			//Draw arc's bezier handles
			Handles.DrawLine(script.transform.TransformPoint(script.arcs[a].point1),script.transform.TransformPoint(script.arcs[a].handle1));
			Handles.DrawLine(script.transform.TransformPoint(script.arcs[a].point2),script.transform.TransformPoint(script.arcs[a].handle2));
			Handles.DrawSolidDisc(script.transform.TransformPoint(script.arcs[a].handle1),Vector3.back,size*0.4f);
			Handles.DrawSolidDisc(script.transform.TransformPoint(script.arcs[a].handle2),Vector3.back,size*0.4f);
			for(int p=0;p<script.arcs[a].points.Length-1;p++){ 
				//Draw the generated line
				Handles.color=Color.gray;
				Handles.DrawLine(
					script.transform.TransformPoint(script.arcs[a].points[p]),
					script.transform.TransformPoint(script.arcs[a].points[p+1])
				);
				if(p!=0) Handles.DrawWireDisc(script.transform.TransformPoint(script.arcs[a].points[p]),Vector3.back,size*0.4f);
			}
		}
		*/
		DrawPointsProperties();
	}

	private void drawMoveHandle(ref Vector2 position){ 
		float size=HandleUtility.GetHandleSize(position)*0.1f;
		EditorGUI.BeginChangeCheck();
		Vector3 point=Handles.FreeMoveHandle(
			position,
			script.transform.rotation,
			size,
			Vector3.zero,
			Handles.CircleHandleCap
		);
		bool changed=EditorGUI.EndChangeCheck();
		position=(Vector2)(point);
		if(Vector2.Distance(position,GetMouseWorldPosition())<size){
			SetCursor(MouseCursor.Arrow);
		}
		if(changed && !Application.isPlaying) script.Generate();
	}

	//Draw a windows with start and end properties
	void DrawPointsProperties(){
		EventType et=Event.current.type;
		if(et!=EventType.Repaint){
			windowRect=new Rect(
				Camera.current.pixelRect.width/EditorGUIUtility.pixelsPerPoint-183,
				Camera.current.pixelRect.height/EditorGUIUtility.pixelsPerPoint-45,
				182,
				60
			);
			int cid=GUIUtility.GetControlID(FocusType.Passive);
			GUILayout.Window(
				cid,
				windowRect,
				(id)=>{
					EditorGUI.BeginChangeCheck();
					//Start point
					EditorGUILayout.BeginHorizontal();
					EditorGUIUtility.labelWidth=35;
					EditorGUILayout.PrefixLabel(new GUIContent("Start","Start of the line. Accessible through scripting as Vector2 named startPoint"));
					EditorGUIUtility.labelWidth=20;
					script.startPoint.x=EditorGUILayout.FloatField("X",script.startPoint.x,GUILayout.Width(64));
					script.startPoint.y=EditorGUILayout.FloatField("Y",script.startPoint.y,GUILayout.Width(64));
					EditorGUILayout.EndHorizontal();
					//End point
					EditorGUILayout.BeginHorizontal();
					EditorGUIUtility.labelWidth=35;
					EditorGUILayout.PrefixLabel(new GUIContent("End","Ending of the line. Accessible through scripting as Vector2 named endPoint"));
					EditorGUIUtility.labelWidth=20;
					script.endPoint.x=EditorGUILayout.FloatField("X",script.endPoint.x,GUILayout.Width(64));
					script.endPoint.y=EditorGUILayout.FloatField("Y",script.endPoint.y,GUILayout.Width(64));
					EditorGUILayout.EndHorizontal();
					//Redraw on change
					bool changed=EditorGUI.EndChangeCheck();
					if(changed && !Application.isPlaying) script.Generate();
				},
				"Endpoints",
				new GUIStyle(GUI.skin.window),
				GUILayout.MinWidth(windowRect.width),
				GUILayout.MaxWidth(windowRect.width),
				GUILayout.MinHeight(windowRect.height),
				GUILayout.MaxHeight(windowRect.height)
			);
			GUI.FocusWindow(cid);
		}
	}

	#endregion

	#region Utilities

	//Calculate point on a bezier line
	private Vector3 CalculateBezierPoint(float t,Vector3 p0,Vector3 p1,Vector3 p2,Vector3 p3){
		float u=1-t;
		float tt=t*t;
		float uu=u*u;
		float uuu=uu*u;
		float ttt=tt*t;
		Vector3 p=uuu*p0;
		p+=3*uu*t*p1;
		p+=3*u*tt*p2;
		p+=ttt*p3;
		return p;
	}

	//Get mouse position
	Vector2 GetMouseWorldPosition(){
		Ray mRay=HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
		float mRayDist;
		if(objectPlane.Raycast(mRay,out mRayDist)) return (Vector2)mRay.GetPoint(mRayDist);
		return Vector2.zero;
	}

	//Set cursor
	void SetCursor(MouseCursor cursor){
		EditorGUIUtility.AddCursorRect(Camera.current.pixelRect,cursor);
	}

	//Get the sorting layer IDs
	public int[] GetSortingLayerUniqueIDs() {
		Type internalEditorUtilityType=typeof(InternalEditorUtility);
		PropertyInfo sortingLayerUniqueIDsProperty=internalEditorUtilityType.GetProperty("sortingLayerUniqueIDs",BindingFlags.Static|BindingFlags.NonPublic);
		return (int[])sortingLayerUniqueIDsProperty.GetValue(null,new object[0]);
	}

	//Get the sorting layer names
	public string[] GetSortingLayerNames(){
		Type internalEditorUtilityType=typeof(InternalEditorUtility);
		PropertyInfo sortingLayersProperty=internalEditorUtilityType.GetProperty("sortingLayerNames",BindingFlags.Static|BindingFlags.NonPublic);
		return (string[])sortingLayersProperty.GetValue(null,new object[0]);
	}

	#endregion

}