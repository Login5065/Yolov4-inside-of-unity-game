using System.Collections;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
   
    public float radius;
    /// maximal distance of fov
    [Range(0,360)]
    public float angle;
    /// max angle for fov
    [Range(0,6)]
    public int precision=2;
    /// sets how many raycast should fov make
    /// to check for player
    public GameObject Ref;
    /// player reference
    public CharacterController m_collider;
    /// player collider reference
    public LayerMask targetMask;
    /// player layer used in Raycast
    public LayerMask obstructionMask;
    /// layer for raycast
    /// sets what can block it
    /// please make sure that its only used as last resort
    /// like walls other requierd location to save on memory

    public bool ShowField = false;
    // if yes shows a mesh with enemy visibility only at eyes level
    [SerializeField] private AI_Behaviour _aiBehaviour;

    public bool canSeePlayer;
        // true if enemy sees player 
    public MeshFilter LOSMeshFilter;
    // model created realtime
    public GameObject field;
    // location of LOSMeshFilter
 
    
    private void Start()
    {
        

        StartCoroutine(FOVRoutine());
        
    }
    

    private IEnumerator FOVRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.05f);
        while (true)
        {
            yield return wait;
            if (_aiBehaviour.restart) continue;
            FieldOfViewCheck();
            if (canSeePlayer)
            {
                _aiBehaviour.Look(Ref.gameObject);
            }

        }
    }

    private void FieldOfViewCheck()
    {
        field.transform.rotation = Quaternion.identity;
        
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radius, targetMask);
        canSeePlayer = false;

        if (ShowField)
        {

            Vector3[] points = new Vector3[516+2];
        for (int i = 1; i <= 516 + 1; i++) {

            RaycastHit hitInfo;

            float degrees = (((float)(i-1) / 516) * angle);
            degrees -= angle / 2;
            degrees -= transform.rotation.eulerAngles.y;
            degrees += 90;
           
            if (Physics.Raycast(new Ray(transform.position, RadiansToVector3(degrees*Mathf.Deg2Rad)), out hitInfo, radius)) {
                points[i] = hitInfo.point - transform.position;
               
            } else {                
                points[i] = RadiansToVector3(degrees*Mathf.Deg2Rad).normalized * radius;
            }
            
            
        }

        LOSMeshFilter.mesh = CreateMeshFromPoints(points);
        
        }
        else
        {
            LOSMeshFilter.mesh = null;
        }

        if (rangeChecks.Length != 0)
        {
            foreach (var tar in rangeChecks)
            {
                Transform target = tar.transform;
                //normalized vecotr to player
                Vector3 directionToTarget = ((target.position+ m_collider.center )  - transform.position).normalized;
                //check if angle to player is smaller than angle set.
                if (Vector3.Angle(transform.forward, directionToTarget) < angle / 2)
                {
                    
                    float h = m_collider.height/2;
                    for (int i = 1; i <= precision; i++)
                    {
                        float distanceToTargetHitBox = Vector3.Distance(transform.position, target.position+ m_collider.center);

                        if (!Physics.Raycast(transform.position, directionToTarget, distanceToTargetHitBox, obstructionMask))
                        {
                                
                            Debug.DrawLine(transform.position, (target.position+ m_collider.center ), Color.red, 0, false);
                            canSeePlayer = true;
                        }
                        
                        

                        Vector3 offset = new Vector3(0, 0, 0);
                        for (int j = 0; j < 2; j++)
                        {
                            var pos = transform.position;
                            var pos2 = target.position;
                            var cen = m_collider.center;
                            
                            
                            offset.y = h;
                            directionToTarget = ((pos2+ cen + offset)  - pos).normalized;
                            distanceToTargetHitBox = Vector3.Distance(pos, pos2+offset+ cen);
                            if (!Physics.Raycast(transform.position, directionToTarget, distanceToTargetHitBox, obstructionMask))
                            {
                                Debug.DrawLine(transform.position, (target.position+ m_collider.center + offset), Color.red, 0, false);
                                canSeePlayer = true;
                            }
                            float r = m_collider.radius;

                            for (int x = 1; x <= precision; x++)
                            {


                                for (int y = 0; y < 2; y++)
                                {
                                    //set raycast destination to said x-axis location
                                    // before this code y-axis is set
                                    offset.x = r;
                                    //set normalized vector from source to destination
                                    directionToTarget = ((target.position+ m_collider.center + offset)  - transform.position).normalized;
                                    //set maximal distance from source to target
                                    distanceToTargetHitBox = Vector3.Distance(pos, pos2+offset+ cen);
                                    //check if
                                    if (!Physics.Raycast(transform.position, directionToTarget, distanceToTargetHitBox, obstructionMask))
                                    {
                                        Debug.DrawLine(transform.position, (target.position+ m_collider.center + offset), Color.red, 0, false);
                                        canSeePlayer = true;
                                    }

                                    offset.x = 0;
                                    
                                    offset.z = r;
                                    directionToTarget = ((target.position+ m_collider.center + offset)  - transform.position).normalized;
                                    distanceToTargetHitBox = Vector3.Distance(pos, pos2+offset+ cen);
                                    if (!Physics.Raycast(transform.position, directionToTarget, distanceToTargetHitBox, obstructionMask))
                                    {
                                        Debug.DrawLine(transform.position, (target.position+ m_collider.center + offset), Color.red, 0, false);
                                        canSeePlayer = true;
                                    }

                                    offset.z = 0;

                                    r = -r;
                                }
                                
                                r -= r / precision;

                            }

                            offset = Vector3.zero;
                            h = -h;
                        }

                        
                        h -= (m_collider.height/2) / precision;
                    }
  
                }

                if (canSeePlayer)
                {
                    Ref=tar.gameObject;
                }
            }
        }
        
    }
    
    Vector3 RadiansToVector3(float degrees) {
        return new Vector3(Mathf.Cos(degrees), 0, Mathf.Sin(degrees));
    }
    Mesh CreateMeshFromPoints(Vector3[] points) {
        Mesh m = new Mesh();
        m.name = "LOSMesh";

        points[0] = Vector3.zero;
        m.vertices = points;

        int[] trianglesArray = new int[(m.vertices.Length-1) * 3];

        int count = 1;
        for (int i = 0; i < trianglesArray.Length-3; i+=3) {
            trianglesArray[i] = count;
            trianglesArray[i + 1] = 0;
            trianglesArray[i + 2] = count + 1;
            count++;
        }

        m.triangles = trianglesArray;
        m.RecalculateNormals();

        return m;
    }
}
