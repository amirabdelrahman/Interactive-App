//
//Filename: maxCamera.cs
//
// original: http://wiki.unity3d.com/index.php?title=MouseOrbitZoom
//
// --01-18-2010 - create temporary target, if none supplied at start

using UnityEngine;
using System.Collections;
using UnityEngine.UI;


[AddComponentMenu("Camera-Control/3dsMax Camera Style")]
public class maxCamera : MonoBehaviour
{

    //public Text debug;
    public Transform target;
    //public Transform targetCopy;
    public Vector3 targetOffset;
    public float distance = 5.0f;
    public float maxDistance = 5000;
    public float minDistance = .6f;
    public float xSpeed = 0.5f;
    public float ySpeed = 0.5f;
    public int yMinLimit = -10;
    public int yMaxLimit = 90;
    //public int zoomRate = 40;
    public float panSpeed = 0.3f;
    public float zoomDampening = 5.0f;

    private float xDeg = 0.0f;
    private float yDeg = 0.0f;
    private float currentDistance;
    private float desiredDistance;
    private Quaternion currentRotation;
    private Quaternion desiredRotation;
    private Quaternion rotation;
    public Vector3 position;


    //Edit
    private Vector3 desiredHor = new Vector3();
    private Vector3 currentMoveX = new Vector3();
    private Vector3 desiredVer = new Vector3();
    public float zoomSpeed = 0.1f;
    public float maxPan = 300;
    public float minPan = 15;
    private float mouseXPosPrev;
    private float mouseYPosPrev;
    private Vector3 targetOriginal;
    public GameObject mesh;
    private Vector3 COM;
    public static bool interactable = true;
    private Vector3 firstpoint; //change type on Vector3
    private Vector3 secondpoint;
    private Vector3 initPos2;
    private Vector3 curPos2;
    private Vector3 startingDif;
    private float initDist;
    //private GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);

    void Start() { Init(); }
    void OnEnable() { Init(); }

    public void Init()
    {
        //If there is no target, create a temporary target at 'distance' from the cameras current viewpoint
        if (!target)
        {
            GameObject go = new GameObject("Cam Target");
            go.transform.position = transform.position + (transform.forward * distance);
            target = go.transform;
        }


        distance = Vector3.Distance(transform.position, target.position);
        currentDistance = distance;
        desiredDistance = distance;

        ////////////////////////
        targetOriginal = new Vector3(target.position.x, target.position.y, target.position.z);


        //be sure to grab the current rotations as starting points.
        position = transform.position;
        rotation = transform.rotation;
        currentRotation = transform.rotation;
        desiredRotation = transform.rotation;

        xDeg = Vector3.Angle(Vector3.right, transform.right);
        yDeg = Vector3.Angle(Vector3.up, transform.up);

        COM = getCOM(mesh.GetComponent<MeshFilter>().mesh);

        //go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //GameObject.Instantiate(go);
        //go.transform.position = COM;

    }

    public void Init(Vector3 pos, Quaternion rot, Vector3 tar)
    {
        target.position = tar;

        distance = Vector3.Distance(pos, tar);
        currentDistance = distance;
        desiredDistance = distance;

        ////////////////////////
        targetOriginal = new Vector3(tar.x, tar.y, tar.z);


        //be sure to grab the current rotations as starting points.
        position = pos;
        rotation = rot;
        currentRotation = rot;
        desiredRotation = rot;

        xDeg = Vector3.Angle(Vector3.right, transform.right);
        yDeg = Vector3.Angle(Vector3.up, transform.up);
        /*
        COM = getCOM(mesh.GetComponent<MeshFilter>().mesh);
        */
        //go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //GameObject.Instantiate(go);
        //go.transform.position = COM;

    }

    /*
     * Camera logic on LateUpdate to only update after all character movement logic has been handled.
     */

    void LateUpdate()
    {
        if (interactable)
        {
            int touchCount = Input.touchCount;
            //Get the Delta of the Mouses
            Vector2 d = getMouseDelta();
            float deltaHor = d.x;
            float deltaVer = d.y;

            if (touchCount != 0)
            {
                // Right Click ORBIT
                if (touchCount == 2)
                {
                    Camera camera = GetComponent<Camera>();

                    // Store both touches.
                    Touch touchZero = Input.GetTouch(0);
                    Touch touchOne = Input.GetTouch(1);

                    // Find the position in the previous frame of each touch.
                    Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                    Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                    // Find the magnitude of the vector (the distance) between the touches in each frame.
                    float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                    float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                    float diff = prevTouchDeltaMag / touchDeltaMag;

                    if (Mathf.Abs(1 - diff) > .01f)
                    {

                        // Find the difference in the distances between each frame.
                        float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
                        /*
                        // Otherwise change the field of view based on the change in distance between the touches.
                        camera.fieldOfView += deltaMagnitudeDiff * 0.15f;

                        // Clamp the field of view to make sure it's between 0 and 180.
                        camera.fieldOfView = Mathf.Clamp(camera.fieldOfView, 30.0f, 120.9f);
                        */
                        desiredDistance += deltaMagnitudeDiff * 0.3f;
                    }
                    else
                    {
                        xDeg += deltaHor * xSpeed;
                        yDeg += deltaVer * ySpeed;

                        //Clamp the vertical axis for the orbit
                        yDeg = ClampAngle(yDeg, yMinLimit, yMaxLimit);
                    }

                    
                }
                else if (touchCount == 3)
                {

                    //grab the rotation of the camera so we can move in a psuedo local XY space
                    target.rotation = transform.rotation;
                    desiredHor += (Vector3.right * -deltaHor * panSpeed);
                    desiredVer += (transform.up * deltaVer * panSpeed);
                }
                else
                {
                    mouseXPosPrev = 0;
                    mouseYPosPrev = 0;
                }
            }
            else
            {
                // Right Click ORBIT
                if (Input.GetMouseButton(1))
                {

                    xDeg += deltaHor * xSpeed;
                    yDeg += deltaVer * ySpeed;

                    //Clamp the vertical axis for the orbit
                    yDeg = ClampAngle(yDeg, yMinLimit, yMaxLimit);
                }
                else if (Input.GetMouseButton(2))
                {

                    //grab the rotation of the camera so we can move in a psuedo local XY space
                    target.rotation = transform.rotation;
                    desiredHor += (Vector3.right * -deltaHor * panSpeed);
                    desiredVer += (transform.up * deltaVer * panSpeed);
                }
                else
                {
                    mouseXPosPrev = 0;
                    mouseYPosPrev = 0;
                }
            }

            ///////////////////////////////PAN//////////////////////////////
            desiredHor *= panSpeed;
            target.Translate(desiredHor);
            desiredVer *= panSpeed;
            target.Translate(desiredVer, Space.World);
            target.position = ClampY(target.position, minPan, maxPan);

            ///////////////////////////////ORBIT//////////////////////////////
            currentRotation = transform.rotation;
            desiredRotation = currentRotation;
            desiredRotation = Quaternion.Euler(yDeg, xDeg, 0);
            rotation = Quaternion.Lerp(currentRotation, desiredRotation, Time.deltaTime * zoomDampening);
            transform.rotation = rotation;
            /////////////////////////////////////////////////////////////////


            ///////////////////////////////ZOOM//////////////////////////////
            if (touchCount > 0)
            {

            }
            else if (touchCount == 0)
            {
                desiredDistance -= Input.mouseScrollDelta.y * Time.deltaTime * zoomSpeed;
            }
            //clamp the zoom min/max
            desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
            // For smoothing of the zoom, lerp distance
            currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * zoomDampening);
            // calculate position based on the new currentDistance
            position = target.position - (rotation * Vector3.forward * currentDistance + targetOffset);
            transform.position = position;

            ///////////////////////////////AUTO PAN//////////////////////////////
            Vector3 p = castRay();

            //GameObject gg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //GameObject.Instantiate(gg);
            //gg.transform.position = p;


            Vector3 move = COM - p;

            //move *= panSpeed * 0.01f;
            //transform.Translate(move);



            /////////////////////////////////////////////////////////////////
            transform.position = ClampDist(transform.position, targetOriginal, maxDistance);
            initDist = currentDistance;
        }
    }

    private Vector3 getCOM(Mesh myMesh)
    {
        Vector3[] myVertices = myMesh.vertices;
        Vector3 com = new Vector3();

        foreach (Vector3 v in myVertices)
        {
            com += v;
        }
        com /= myVertices.Length;

        return com;
    }

    private Vector3 ClampDist(Vector3 toClamp, Vector3 reference, float distance)
    {
        Vector3 myV = new Vector3(toClamp.x, toClamp.y, toClamp.z);

        if (myV.x > reference.x + distance) myV.x = reference.x + distance;
        if (myV.x < reference.x - distance) myV.x = reference.x - distance;

        if (myV.y > reference.y + distance) myV.y = reference.y + distance;
        if (myV.y < reference.y - distance) myV.y = reference.y - distance;

        if (myV.z > reference.z + distance) myV.z = reference.z + distance;
        if (myV.z < reference.z - distance) myV.z = reference.z - distance;

        return myV;
    }

    private Vector3 castRay()
    {
        RaycastHit hit;

        Ray ray = transform.GetComponent<Camera>().ScreenPointToRay(transform.forward);
        bool IsHit = Physics.Raycast(ray, out hit, 50000);
        return hit.point;
    }

    private Vector3 ClampY(Vector3 v, float min, float max)
    {
        Vector3 myV = new Vector3(v.x, v.y, v.z);

        if (myV.y > max)
        {
            myV.y = max;
        }
        if (myV.y < min)
        {
            myV.y = min;
        }

        return myV;
    }

    private Vector2 getMouseDelta()
    {
        Vector3 mousePosCur = Input.mousePosition;
        Vector2 delta = new Vector2();
        //Horizontal
        if (mouseXPosPrev == 0)
        {
            mouseXPosPrev = mousePosCur.x;
        }
        else
        {
            delta.x = mousePosCur.x - mouseXPosPrev;
            mouseXPosPrev = mousePosCur.x;
        }

        //Vertical
        if (mouseYPosPrev == 0)
        {
            mouseYPosPrev = mousePosCur.y;
        }
        else
        {
            delta.y = mouseYPosPrev - mousePosCur.y;
            mouseYPosPrev = mousePosCur.y;
        }

        return delta;
    }

    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
}