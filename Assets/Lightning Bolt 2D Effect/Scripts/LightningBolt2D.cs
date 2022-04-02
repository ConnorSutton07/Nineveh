using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LightningBolt2D:MonoBehaviour{

	#region Configurable variables

	//Start and end point of the linghtning bolt
	public Vector2 startPoint=Vector2.up*5f;
	public Vector2 endPoint=Vector2.down*5f;

	public float arcLifetimeMin=0.1f; //Minimal lifetime of a single arc
	public float arcLifetimeMax=1f; //Maximal lifetime of a single arc

	public int arcCount=1; //Number of arcs to create
	public int pointCount=20; //Number of points per each arc including start and end points
	public float lineWidth=0.15f; //Width of the lightning line
	public float glowWidth=0.1f; //Width of the "glow" gradient
	
	public Color lineColor; //Color of the line itself
	public Color glowColor; //Color of the glow
	public Color glowEdgeColor; //Transparent color at the edge of the glow gradient. Generated automatically
	
	public float distort=0.03f; //How much to randomize the points of the straight line. Set to zero to schive smooth line
	public float jitter=0.1f; //How much the points will randomly move when the scene is in the play mode

	public float bend=0.2f; //How bent will each new lightning arc initially be
	public float bendSpeed=0.05f; //How much the lightning arc will bend when the scene is in the play mode

	public float FPSLimit=60f; //Limit how many frames per second this object is allowed to update

	public bool isPlaying=true; //This variable sets if the object should keep generating new lightnings

	public int sortingLayer=0;
	public int orderInLayer=0;

	[SerializeField] 
	int instanceID=0;

	#endregion

	#region Non-configurable variables

	private long lastTime=System.DateTime.Now.Ticks; //For FPS limiter

	public Vector3 lastPosition;

	//Component settings
	[SerializeField]
	private bool isCanvas=false;
	[SerializeField]
	private CanvasRenderer cr;
	[SerializeField]
	private MeshFilter mf;
	[SerializeField]
	private MeshRenderer mr;
	public Material useMaterial=null;

	//For mesh generation
	[SerializeField]
	private Mesh mesh;
	[SerializeField]
	private List<Vector3> vertices=new List<Vector3>(200);
	[SerializeField]
	private List<Vector3> uvs=new List<Vector3>(200);
	[SerializeField]
	private List<Color> colors=new List<Color>(200);
	[SerializeField]
	private int[] triangles;
	public int triangleCount; //Keeps triangle count for displaying in inspector

	//Arc objects. They hold information whic is used to calculate how the arcs are drawn
	public List<arc> arcs=new List<arc>(5);

	public class arc{
		public int seed; //Seed for the random generator
		public float timeStart; //Keep time when arc was created
		public float timeEnd; //Keep time when arc will be removed
		public float timeRatio; //Arc's lifetime in 0 to 1 ratio
		public Vector2 point1; //Start point in relative space
		public Vector2 point2; //End point in relative space
		public Vector2 handle1; //Bezier handle of start point
		public Vector2 handle2; //Bezier handle of end point
		public Vector2 handle1direction; //Direction in which start point handle will move when being animated
		public Vector2 handle2direction; //Direction in which end point handle will move when being animated
		public float segmentLength; //Holds average segment width
		public Vector2[] points; //Holds points of the line we calculate (not final vertices but an original line)
		public Vector2[] normals; //Holds normals for each segment
		public float[] widths; //Holds width of each segment
		public Vector2[] miters; //Needed to calculate corners. It's a normalized vector
		public float[,] mitersLength; //Lengths of meter for points. 4 per point
	}

	#endregion

	#region Manage components and updates

	/*
		Set some variables if they're not set already
	*/

	private void Awake(){
		//Check if object is a copy
		if(instanceID!=GetInstanceID()){
			if(instanceID==0){
				instanceID=GetInstanceID();
			}else{
				instanceID=GetInstanceID();
				if(instanceID<0){
					//arcs.Clear();
					mesh=null;
					//if(mf!=null && mf.sharedMesh!=null && mf.sharedMesh.vertexCount>0) mf.sharedMesh.Clear();
				}
			}
		}
		//Get material to use
		if(useMaterial==null) useMaterial=(Material)Resources.Load("LightningBolt2D",typeof(Material));
		//Check there's a Canvas in parents of the object
		if(isCanvas!=isChildOfCanvas(transform)) isCanvas=!isCanvas;
		//Generate lightning color if not yet set
		if(lineColor.a==0f){
			glowColor=RandomColor();
			glowEdgeColor=new Color(glowColor.r,glowColor.g,glowColor.b,0f);
			lineColor=Color.Lerp(glowColor,Color.white,0.8f);
		}
		//Adds the components
		TakeCareOfComponenets();
		//If object starts in no play mode, we clear the mesh jsut in case
		if(Application.isPlaying && !isPlaying){
			arcs.Clear();
			if(mf!=null && mf.sharedMesh!=null && mf.sharedMesh.vertexCount>0) mf.sharedMesh.Clear();
		}
		//Calculates and generates the geometry
		Generate();
	}

	/*
		Runs the Generate() function every frame
	*/

	void Update(){
		if(FPSLimit>0){ 
			if(System.DateTime.Now.Ticks<lastTime+System.TimeSpan.FromSeconds(1f/FPSLimit).Ticks){
				return;
			}else{ 
				lastTime=System.DateTime.Now.Ticks;
			}
		}
		//Get material to use
		if(useMaterial==null) useMaterial=(Material)Resources.Load("LightningBolt2D",typeof(Material));
		//Check there's a Canvas in parents of the object
		if(!Application.isPlaying){
			if(isCanvas!=isChildOfCanvas(transform)){
				isCanvas=!isCanvas;
				TakeCareOfComponenets();
			}
		}
		//Generate only if scene is in play mode. In edit mode this function gets called by editor script
		if(Application.isPlaying) Generate();
	}

	/*
		Add/remove componenets depending on parent
	*/

	void TakeCareOfComponenets(){
		if(isCanvas){
			DestroyImmediate(GetComponent<MeshFilter>());
			DestroyImmediate(GetComponent<MeshRenderer>());
			if(GetComponent<CanvasRenderer>()==null) gameObject.AddComponent<CanvasRenderer>();
			cr=GetComponent<CanvasRenderer>();
			cr.SetMaterial(useMaterial,null);
		}else{
			DestroyImmediate(GetComponent<CanvasRenderer>());
			if(GetComponent<MeshFilter>()==null) gameObject.AddComponent<MeshFilter>();
			if(GetComponent<MeshRenderer>()==null) gameObject.AddComponent<MeshRenderer>();
			mf=GetComponent<MeshFilter>();
			mr=GetComponent<MeshRenderer>();
			mr.sharedMaterial=useMaterial;
			sortingLayer=mr.sortingLayerID;
			orderInLayer=mr.sortingOrder;
		}
	}

	#endregion

	#region Actual calculation and mesh generation

	/*
		Starts generating lightnings and immediately stops producing new ones
		so the ones that are created will play their animation to the end but
		the new ones will not be created
	*/

	public void FireOnce(){
		if(!isPlaying){
			isPlaying=true;
			Generate();
			isPlaying=false;
		}
	}

	/*
		This function calculates positions of the points based on the configuration.
		The result of this function is arcs[].points arrays for each arc
	*/

	public void Generate(){
		//If number of arcs was changed, we regenerate arc objects
		if(arcs.Count!=arcCount && (isPlaying || !Application.isPlaying)){ 
			arcs.Clear();
			for(int a=0;a<arcCount;a++) arcs.Add(new arc());
		}
		//Iterate through each arc, restart the expired ones, count which ones are active
		int activeArcs=0;
		for(int a=0;a<arcs.Count;a++){
			//Regenerate the arc if it reached it's end time or wasn't generated yet
			if(arcs[a].seed==0 || (Application.isPlaying && isPlaying && arcs[a].timeEnd<Time.time)){
				arcs[a].seed=(int)System.DateTime.Now.Ticks+a;
				arcs[a].timeStart=Time.time;
				arcs[a].timeEnd=Time.time+Random.Range(arcLifetimeMin,arcLifetimeMax);
				//Also update this object's position in case the points were moved
				transform.position=Vector3.Lerp((Vector3)startPoint,(Vector3)endPoint,0.5f);
				lastPosition=transform.position;
			}
			//Check if this arc can be considered as active and increment if so
			if(arcs[a].timeEnd>=Time.time || !Application.isPlaying) activeArcs++;
		}
		//If there are active arcs, we calculate their geometry for this frame
		if(activeArcs>0){
			for(int a=0;a<arcs.Count;a++){
				if(arcs[a].timeEnd>=Time.time || !Application.isPlaying){
					Random.InitState(arcs[a].seed); //Seed random to get consistent results every frame
					arcs[a].point1=transform.InverseTransformPoint(startPoint); //Set end and start for the arc in case they were moved
					arcs[a].point2=transform.InverseTransformPoint(endPoint);
					Vector2 direction=arcs[a].point2-arcs[a].point1;
					float length=direction.magnitude;
					arcs[a].segmentLength=length/(pointCount-1); //Length of a single segment without distortions
					//Generate handles which spread points evently and multiply them by curvePower
					arcs[a].handle1=Vector2.Lerp(arcs[a].point1,arcs[a].point2,0.3333333f)+Random.insideUnitCircle*(length*bend)*Random.value;
					arcs[a].handle2=Vector2.Lerp(arcs[a].point1,arcs[a].point2,0.6666666f)+Random.insideUnitCircle*(length*bend)*Random.value;
					//Set directions for bending animation
					arcs[a].handle1direction=Random.insideUnitCircle;
					arcs[a].handle2direction=Random.insideUnitCircle;
					//Animate bending the arc by moving bezier handles
					if(Application.isPlaying){
						arcs[a].handle1+=arcs[a].handle1direction*(Time.time-arcs[a].timeStart)*length*bendSpeed;
						arcs[a].handle2+=arcs[a].handle2direction*(Time.time-arcs[a].timeStart)*length*bendSpeed;
					}
					//If number of points changed, we regenrate point data from scratch
					if(arcs[a].points==null || arcs[a].points.Length!=pointCount){
						arcs[a].points=new Vector2[pointCount];
						arcs[a].normals=new Vector2[pointCount];
						arcs[a].widths=new float[pointCount];
						arcs[a].miters=new Vector2[pointCount];
						arcs[a].mitersLength=new float[pointCount,4];
					}
					//Create points based on bezier curve and add distortion
					for(int i=0;i<pointCount;i++){ 
						arcs[a].points[i]=CalculateBezierPoint(((float)i)/(pointCount-1),arcs[a].point1,arcs[a].handle1,arcs[a].handle2,arcs[a].point2);
						arcs[a].points[i]+=Random.insideUnitCircle*length*distort*Parabola((float)i/(pointCount-1),0.5f);
					}
				}
			}
			//Jitter only affects the object when the scene is in play mode
			if(Application.isPlaying){
				Random.InitState((int)System.DateTime.Now.Ticks);
				for(int a=0;a<arcs.Count;a++){
					for(int i=0;i<pointCount;i++){
						arcs[a].points[i]+=Random.insideUnitCircle*jitter*Parabola((float)i/(pointCount-1),0.5f);
					}
				}
			}
			//Build the geometry based on points we calculated
			BuildMesh();
		}else if(mesh!=null && mesh.vertexCount>0){ 
			mesh.Clear();
			if(cr!=null) cr.SetMesh(mesh);
		}
	}

	/*
		This function builds the mesh, combining all lines into one geometrical object.
		It is also responsible for calculation of line width based on all of the parameters
		and conditions (segment length, arc lifetime)
	*/

	public void BuildMesh(){
		//Reset and clear everything to rebuild from scratch
		if(mesh==null) mesh=new Mesh{name="Lightning Bolt 2D"};
		mesh.Clear();
		vertices.Clear();
		uvs.Clear();
		colors.Clear();

		//Do all the math needed to create vertices of the mesh
		int activeArcs=0; //Keeps the number of arcs we want to render
		for(int a=0;a<arcs.Count;a++){
			if(arcs[a].timeEnd>=Time.time || !Application.isPlaying){
				activeArcs++;
				//Calculate normals for all line segments
				for(int p=1;p<pointCount;p++){
					//Calculate normal for a line segment before this point
					arcs[a].normals[p]=(arcs[a].points[p-1]-arcs[a].points[p]).normalized;
					arcs[a].normals[p]=new Vector2(arcs[a].normals[p].y,-arcs[a].normals[p].x);
					//For the zero point we use normal of the first point
					if(p==1) arcs[a].normals[p-1]=arcs[a].normals[p];
				}
				//Calculate widths for line at every point
				arcs[a].timeRatio=(Time.time-arcs[a].timeStart)/(arcs[a].timeEnd-arcs[a].timeStart);
				for(int p=0;p<pointCount;p++){
					float position=(float)p/(pointCount-1);
					arcs[a].widths[p]=(lineWidth/2);
					arcs[a].widths[p]*=Parabola(position,0.5f); //Make line thinner towards the ends
					//Modify by lifetime
					if(Application.isPlaying){ 
						arcs[a].widths[p]*=EaseIn(arcs[a].timeRatio,0.1f); //Thin in the beginning
						arcs[a].widths[p]*=EaseOut(arcs[a].timeRatio,5f); //Thin in the end
						arcs[a].widths[p]*=1f-(Parabola(position,2f)*(arcs[a].timeRatio*arcs[a].timeRatio*arcs[a].timeRatio)); //Thin in the middle towards the end
					}
					//Modify by relative segment width
					//if(p>0) arcs[a].widths[p]*=(arcs[a].segmentLength/((arcs[a].points[p-1]-arcs[a].points[p]).magnitude))*curveFunkyness;
				}
				//Calculate miters for each corner. It's a normalized vector that shows direction of the corner point
				for(int p=1;p<pointCount-1;p++){
					//Calculate a normalized miter
					arcs[a].miters[p]=((arcs[a].points[p+1]-arcs[a].points[p]).normalized+(arcs[a].points[p]-arcs[a].points[p-1]).normalized).normalized;
					arcs[a].miters[p]=new Vector2(-arcs[a].miters[p].y,arcs[a].miters[p].x);
					//Calculate miter length for bottom line vertex
					arcs[a].mitersLength[p,0]=arcs[a].widths[p]/Vector2.Dot(arcs[a].miters[p],-arcs[a].normals[p]);
					if(Mathf.Abs(arcs[a].mitersLength[p,0])>lineWidth*2) arcs[a].mitersLength[p,0]=lineWidth*2*Mathf.Sign(arcs[a].mitersLength[p,0]); //Don't allow very sharp edges
					//Calculate miter length for top line vertex
					arcs[a].mitersLength[p,1]=arcs[a].widths[p]/Vector2.Dot(arcs[a].miters[p],arcs[a].normals[p]);
					if(Mathf.Abs(arcs[a].mitersLength[p,1])>lineWidth*2) arcs[a].mitersLength[p,1]=lineWidth*2*Mathf.Sign(arcs[a].mitersLength[p,1]);  //Don't allow very sharp edges
					//Miter length for the bottom glow vertex
					arcs[a].mitersLength[p,2]=(arcs[a].widths[p]+glowWidth*(arcs[a].widths[p]/(lineWidth/2)))/Vector2.Dot(arcs[a].miters[p],-arcs[a].normals[p]);
					if(Mathf.Abs(arcs[a].mitersLength[p,2])>(lineWidth*2+glowWidth*2)) arcs[a].mitersLength[p,2]=(lineWidth*2+glowWidth*2)*Mathf.Sign(arcs[a].mitersLength[p,2]); //Don't allow very sharp edges
					//Miter length for the top glow vertex
					arcs[a].mitersLength[p,3]=(arcs[a].widths[p]+glowWidth*(arcs[a].widths[p]/(lineWidth/2)))/Vector2.Dot(arcs[a].miters[p],arcs[a].normals[p]);
					if(Mathf.Abs(arcs[a].mitersLength[p,3])>(lineWidth*2+glowWidth*2)) arcs[a].mitersLength[p,3]=(lineWidth*2+glowWidth*2)*Mathf.Sign(arcs[a].mitersLength[p,3]); //Don't allow very sharp edges
				}
			}
		}

		//Create vertices for the glow
		if(glowWidth>0){
			for(int a=0;a<arcs.Count;a++){
				if(arcs[a].timeEnd>=Time.time || !Application.isPlaying){
					for(int p=0;p<pointCount;p++){
						vertices.Add(arcs[a].points[p]+arcs[a].miters[p]*arcs[a].mitersLength[p,2]);
						vertices.Add(arcs[a].points[p]+arcs[a].miters[p]*arcs[a].mitersLength[p,0]);
						colors.Add(glowEdgeColor);
						colors.Add((p>0 && p<pointCount-1)?glowColor:glowEdgeColor);
					}
					for(int p=0;p<pointCount;p++){
						vertices.Add(arcs[a].points[p]+arcs[a].miters[p]*arcs[a].mitersLength[p,1]);
						vertices.Add(arcs[a].points[p]+arcs[a].miters[p]*arcs[a].mitersLength[p,3]);
						colors.Add((p>0 && p<pointCount-1)?glowColor:glowEdgeColor);
						colors.Add(glowEdgeColor);
					}
				}
			}
		}

		//Create vertices and colros for the lines
		for(int a=0;a<arcs.Count;a++){
			if(arcs[a].timeEnd>=Time.time || !Application.isPlaying){
				for(int p=1;p<pointCount-1;p++){
					//Add the first point since we're skipping it in the for loop
					if(p==1){
						vertices.Add(arcs[a].points[p-1]-arcs[a].normals[p-1]*arcs[a].widths[p-1]);
						vertices.Add(arcs[a].points[p-1]+arcs[a].normals[p-1]*arcs[a].widths[p-1]);
						colors.Add(lineColor);
						colors.Add(lineColor);
					}
					vertices.Add(arcs[a].points[p]+arcs[a].miters[p]*arcs[a].mitersLength[p,0]);
					vertices.Add(arcs[a].points[p]+arcs[a].miters[p]*arcs[a].mitersLength[p,1]);
					colors.Add(lineColor);
					colors.Add(lineColor);
					//If we're on second to last point, we're also adding the last one
					if(p==pointCount-2){
						vertices.Add(arcs[a].points[p+1]-arcs[a].normals[p+1]*arcs[a].widths[p+1]);
						vertices.Add(arcs[a].points[p+1]+arcs[a].normals[p+1]*arcs[a].widths[p+1]);
						colors.Add(lineColor);
						colors.Add(lineColor);
					}
				}
			}
		}

		//Create triangles
		int lines=(activeArcs*(lineWidth>0?1:0))+(activeArcs*(glowWidth>0?2:0));
		triangleCount=lines*(pointCount-1)*2;
		if(triangles==null){ 
			triangles=new int[triangleCount*3];
		}else if(triangles.Length!=triangleCount*3){
			System.Array.Resize(ref triangles,triangleCount*3);
		}
		//triangles=new int[triangleCount*3];
		int ls=0,vs=0;
		for(int l=0;l<lines;l++){ 
			ls=(l*(pointCount-1)*6); //Starting index for this line's triangles
			vs=l*pointCount*2;
			for(int i=0;i<pointCount-1;i++){
				triangles[ls+(i*6)+0]=vs+i*2+0;
				triangles[ls+(i*6)+1]=vs+i*2+1;
				triangles[ls+(i*6)+2]=vs+i*2+3;
				triangles[ls+(i*6)+3]=vs+i*2+0;
				triangles[ls+(i*6)+4]=vs+i*2+3;
				triangles[ls+(i*6)+5]=vs+i*2+2;
			}
		}

		//Finalize the mesh
		mesh.SetVertices(vertices);
		mesh.SetUVs(0,uvs);
		mesh.SetColors(colors);
		mesh.SetTriangles(triangles,0);
		mesh.RecalculateBounds();
		if(isCanvas){
			if(cr==null) cr=GetComponent<CanvasRenderer>();
			cr.SetMesh(mesh);
		}else{
			if(mf==null) mf=GetComponent<MeshFilter>();
			mf.sharedMesh=mesh;
		}
		if(mr!=null){
			mr.sortingLayerID=sortingLayer;
			mr.sortingOrder=orderInLayer;
		}
	}

	#endregion

	#region Utility functions

	bool isChildOfCanvas(Transform t){
		if(t.GetComponent<Canvas>()!=null){
			return true;
		}else if(t.parent!=null){
			return isChildOfCanvas(t.parent);
		}else{
			return false;
		}
	}

	//Generate a random mild color
	private Color RandomColor(){
		float hue=Random.Range(0f,1f);
		while(hue*360f>=236f && hue*360f<=246f){
			hue=Random.Range(0f,1f);
		}
		return Color.HSVToRGB(hue,Random.Range(0.2f,0.7f),Random.Range(0.8f,1f));
	}

	//Convert Vector2 to angle in degrees (0-360)
	public static float Vector2Angle(Vector2 v){
		if(v.x<0) return 360-(Mathf.Atan2(v.x, v.y)*Mathf.Rad2Deg*-1);
		else return Mathf.Atan2(v.x,v.y)*Mathf.Rad2Deg;
	}

	//Convert degrees (0-360) to a Vector2
	public static Vector2 Angle2Vector(float angle,float power){
		return (new Vector2(Mathf.Sin(angle*Mathf.Deg2Rad),Mathf.Cos(angle*Mathf.Deg2Rad)))*power;
	}

	public float Parabola(float x){ 
		return Parabola(x,2f);
	}

	public float Parabola(float x,float p){ 
		return Mathf.Pow(4f*x*(1f-x),p);
	}

	public float EaseIn(float x){ 
		return EaseIn(x,2f);
	}

	public float EaseIn(float x,float p){ 
		return Mathf.Pow(x,p);
	}

	public float EaseOut(float x){ 
		return EaseOut(x,2f);
	}

	public float EaseOut(float x,float p){
		float f=x-1f;
		return 1f-Mathf.Pow(x,p);
	}

	//Calculates where two lines collide
	private Vector2 LineIntersectionPoint(Vector2 l1s, Vector2 l1e, Vector2 l2s, Vector2 l2e) {
		//Get A,B,C of first line
		float A1=l1e.y-l1s.y;
		float B1=l1s.x-l1e.x;
		float C1=A1*l1s.x+B1*l1s.y;
		//Get A,B,C of second line
		float A2=l2e.y-l2s.y;
		float B2=l2s.x-l2e.x;
		float C2=A2*l2s.x+B2*l2s.y;
		//Get delta and check if the lines are parallel
		float delta=A1*B2-A2*B1;
		//if(delta==0) throw new System.Exception("Lines are parallel");
		//Special case where the angle is too small
		if(delta<0.1f && delta>-0.1f && l1e==l2s) return l1e;
		else if(delta<0.1f && delta>-0.1f) return Vector2.Lerp(l1e,l2s,0.5f);
		//Now return the Vector2 intersection point
		return new Vector2(
			(B2*C1-B1*C2)/delta,
			(A1*C2-A2*C1)/delta
		);
	}

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

	#endregion
	
}