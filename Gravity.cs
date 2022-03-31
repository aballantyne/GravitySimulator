using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 
public class Gravity : MonoBehaviour
{

    [SerializeField]
    public List<LunarObjectAttributes> listOfLunarObjects = new List<LunarObjectAttributes>();

    private void FixedUpdate() {
        foreach (var lunarObject in listOfLunarObjects)
        {
            foreach (var lunarObjectB in listOfLunarObjects)
            {
                if (lunarObjectB != lunarObject)
                    lunarObject.LoadGravityVector(lunarObjectB);
            }
            lunarObject.ApplyPhysics();
            lunarObject.LoadVelocityVector(); 
        }
    }   
}

[System.Serializable]
public class LunarObjectAttributes {
    public Transform transform; 
    private Dictionary<LunarObjectAttributes, GravityVector> gravityVector = new Dictionary<LunarObjectAttributes, GravityVector>();
    private UnityEngine.LineRenderer velocityVector;

    public float mass; 
    private Vector2 gravity;
    public Vector2 velocity;

    public bool isFixedPosition = false;

    public bool createDebugVectors = true; 

    public LunarObjectAttributes(Transform _transform, float _mass, Vector2 _velocity, bool _isFixedPoisition, bool _createDebugVectors){
        transform = _transform; mass = _mass; isFixedPosition = _isFixedPoisition; velocity = _velocity; createDebugVectors = _createDebugVectors; 
    }

    public void LoadGravityVector(LunarObjectAttributes direction){
        if (!isFixedPosition && !(transform is null)){
            if (gravityVector is null)
                gravityVector = new Dictionary<LunarObjectAttributes, GravityVector>();
            
            if (!gravityVector.ContainsKey(direction))
                gravityVector.Add(direction, new GravityVector(this,direction));

            gravityVector[direction].CalculateGravity(); 
            
        }    
    }
    public void LoadVelocityVector(){
        if (!isFixedPosition && !(transform is null)){
            if (velocityVector == null){
                GameObject lineObject = new GameObject("velocityVector");
                lineObject.transform.SetParent(transform);
                lineObject.transform.localPosition = new Vector2(0,0);
                velocityVector = lineObject.AddComponent<UnityEngine.LineRenderer>();
            
                
                velocityVector.useWorldSpace = false; 
                velocityVector.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
                velocityVector.startColor = new Color(25/255f,171/255f,20/255f,1);
                velocityVector.endColor = new Color(25/255f,171/255f,20/255f,1); 
                velocityVector.endWidth = 0.06f; 
                velocityVector.startWidth = 0.06f; 

                velocityVector.positionCount = 2;
            }
            if (createDebugVectors){
                velocityVector.gameObject.SetActive(true);
                velocityVector.SetPosition(1, velocity);
            }else{
                velocityVector.gameObject.SetActive(false);
            }
        }
    }
    public void ApplyPhysics(){
        if (!isFixedPosition && !(transform is null)){
            gravity = new Vector2(0,0); 
            foreach (KeyValuePair<LunarObjectAttributes, GravityVector> kvp in gravityVector)
            {
                gravity += kvp.Value.gravity;
            }

            velocity += gravity/3f;
            transform.position = new Vector2 (transform.position.x,transform.position.y) + velocity  * Time.deltaTime;
            
        }
    }
    
}
public class GravityVector{
    public UnityEngine.LineRenderer line; 
    
    public Vector2 gravity; 

    public LunarObjectAttributes origin; 
    public LunarObjectAttributes direction; 

    public GravityVector(LunarObjectAttributes _origin, LunarObjectAttributes _direction){
        origin = _origin; direction = _direction;

        line = new GameObject().AddComponent<UnityEngine.LineRenderer>(); 
        
        line.gameObject.name = String.Format("GravityVector({0}, {1})", origin.transform.gameObject.name, direction.transform.gameObject.name);
        line.transform.SetParent(origin.transform); line.transform.localPosition = new Vector2(0,0);
        line.useWorldSpace = false; 
        
        line.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        line.startColor = new Color(20/255f,120/255f,171/255f,1);
        line.endColor = new Color(20/255f,120/255f,171/255f,1); 
        line.endWidth = 0.06f; 
        line.startWidth = 0.06f; 

        line.positionCount = 2;
    }
    public void CalculateGravity(){
        float angle = FindRadian(origin.transform.position, direction.transform.position);
        float lengthOfVector = (origin.mass * direction.mass)/Mathf.Pow(Vector3.Distance(direction.transform.position, origin.transform.position),2)/166.5f;
        gravity = PolarToRecangular(lengthOfVector, angle);

        UpdateGravityVector();
    }
    public void UpdateGravityVector(){
        if (origin.createDebugVectors){
            line.gameObject.SetActive(true);
            line.SetPosition(1, gravity);
        }else{
            line.gameObject.SetActive(false);
        }
    }
    public Vector2 PolarToRecangular(float r, float theta){
        return new Vector2(r * Mathf.Cos(theta), r* Mathf.Sin(theta));
    }
    public float FindRadian(Vector2 A, Vector2 B){
        Vector2 C = B - A;
        float Angle = Mathf.Atan2(C.y,C.x);
        float AngleInDegrees = Angle * Mathf.Rad2Deg;
        float AngleInRadian =  0.01745f * AngleInDegrees; 
        return AngleInRadian; 
    }
}