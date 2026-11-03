using System.Collections;
using System.Reflection;
using UnityEngine;

static class Program
{
    static int checks;
    static void Check(bool condition, string label) { if (!condition) throw new Exception(label); checks++; }
    static void Call(object target, string method, params object[] args) => target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, args);
    static (Shooter shooter, PantryProwlerAnimation bridge, Animator animator) Make(bool animated = true)
    {
        var go = new GameObject(); var shooter = go.Add<Shooter>();
        go.Add<SpriteRenderer>(); go.Add<Rigidbody2D>(); var animator = go.Add<Animator>();
        var bridge = animated ? go.Add<PantryProwlerAnimation>() : null;
        shooter.shotPrefab = new GameObject(); shooter.reloadtime = .05f;
        Call(shooter, "Awake"); if (bridge != null) Call(bridge, "Awake");
        return (shooter, bridge, animator);
    }
    static void Trigger(Shooter shooter) => Call(shooter, "OnTriggerEnter2D", new Collider2D { gameObject = new GameObject { tag = "Food" } });
    static void Wait(IEnumerator routine, float seconds, string label) => Check(routine.Current is WaitForSeconds w && Math.Abs(w.seconds - seconds) < .0001f, label);
    public static void Main()
    {
        var (s,b,a)=Make(); UnityEngine.Object.Shots.Clear();
        GameObject.AudioManager=new GameObject();var audio=GameObject.AudioManager.Add<AudioManager2>();
        Trigger(s); Check(s.reloading && a.GetBool("Shooting") && !a.GetBool("Walking"), "Attack locks movement and begins charge");
        Check(UnityEngine.Object.Shots.Count==0, "No early projectile");
        Check(!audio.playLaser,"No early firing sound");
        var routine=s.routines.Single(); Wait(routine,.625f,"Wind-up timing");
        Trigger(s); Check(s.routines.Count==1, "Repeated contact cannot start another shot");
        routine.MoveNext(); Check(UnityEngine.Object.Shots.Count==1,"Exactly one shot at release");
        Check(audio.playLaser,"Firing sound on release");GameObject.AudioManager=null;
        Check(Math.Abs(UnityEngine.Object.Shots[0].y-s.transform.position.y)<.0001f,"Projectile keeps lane Y");
        Check(UnityEngine.Object.Shots[0].x<s.transform.position.x,"Projectile emitted in front, left");
        Wait(routine,.125f,"Follow-through timing");
        routine.MoveNext(); Check(!a.GetBool("Shooting") && s.reloading,"Recovery ends before cooldown unlock"); Wait(routine,.05f,"Existing reload duration retained");
        Check(!routine.MoveNext()&&!s.reloading,"Cooldown completes"); Call(b,"Update"); Check(a.GetBool("Walking"),"Walk resumes");
        var cancel=Make(); UnityEngine.Object.Shots.Clear();Trigger(cancel.shooter);
        Call(cancel.shooter,"OnCollisionEnter2D",new Collision2D { gameObject=new GameObject {tag="Pancakes"} });
        Call(cancel.bridge,"Update");Check(!cancel.animator.GetBool("Shooting"),"Pancake interrupts pose");
        var canceled=cancel.shooter.routines.Single();while(canceled.MoveNext()){} Check(UnityEngine.Object.Shots.Count==0,"Pancake cancels pending shot");
        var disabled=Make();Trigger(disabled.shooter);Call(disabled.shooter,"OnDisable");
        Check(disabled.shooter.routines.Count==0&&!disabled.shooter.reloading&&!disabled.animator.GetBool("Shooting"),"Disable cancels shot and resets state");
        var visualDisabled=Make();Trigger(visualDisabled.shooter);visualDisabled.bridge.enabled=false;Call(visualDisabled.bridge,"OnDisable");
        var v=visualDisabled.shooter.routines.Single();while(v.MoveNext()){} Check(UnityEngine.Object.Shots.Count==0,"Disabled animation cannot emit a pending orb");
        var toggled=Make();Trigger(toggled.shooter);Call(toggled.bridge,"OnDisable");toggled.bridge.enabled=true;
        var t=toggled.shooter.routines.Single();while(t.MoveNext()){}Check(UnityEngine.Object.Shots.Count==0,"Re-enabling a canceled charge cannot resurrect the shot");
        var legacy=Make(false);Trigger(legacy.shooter);Check(UnityEngine.Object.Shots.Count==1,"Legacy Shooter without animation still fires immediately");
        var unrelated=Make();Call(unrelated.shooter,"OnTriggerEnter2D",new Collider2D{gameObject=new GameObject{tag="Other"}});Check(unrelated.shooter.routines.Count==0,"Unrelated trigger ignored");
        var repeated=Make();UnityEngine.Object.Shots.Clear();for(int i=0;i<3;i++){Trigger(repeated.shooter);var shot=repeated.shooter.routines.Last();while(shot.MoveNext()){} }
        Check(UnityEngine.Object.Shots.Count==3,"Subsequent attacks fire once each");
        var projectileObject=new GameObject();var projectile=projectileObject.Add<AlienShot>();projectile.shot=projectileObject;projectile.impactPrefab=new GameObject();
        Call(projectile,"Start");Check(projectile.mSpeed==5,"Projectile speed retained");
        UnityEngine.Object.Shots.Clear();UnityEngine.Object.Destroyed.Clear();UnityEngine.Object.Scheduled.Clear();
        var food=new Collider2D {gameObject=new GameObject {tag="Food"}};
        Call(projectile,"OnTriggerEnter2D",food);Call(projectile,"OnTriggerEnter2D",food);
        Check(UnityEngine.Object.Shots.Count==1,"Overlapping hit callbacks create one impact");
        Check(UnityEngine.Object.Destroyed.Count==1&&UnityEngine.Object.Destroyed[0]==projectileObject,"Hit consumes projectile once");
        Check(UnityEngine.Object.Scheduled.Count==1&&Math.Abs(UnityEngine.Object.Scheduled[0]-.3f)<.0001f,"Impact cleanup matches clip duration");
        var missObject=new GameObject();var miss=missObject.Add<AlienShot>();miss.shot=missObject;miss.impactPrefab=new GameObject();UnityEngine.Object.Shots.Clear();
        Call(miss,"OnTriggerEnter2D",new Collider2D{gameObject=new GameObject {tag="Other"}});Check(UnityEngine.Object.Shots.Count==0,"Unrelated contacts do not burst");
        miss.transform.position=new Vector3(11,0,0);Call(miss,"Update");Check(UnityEngine.Object.Shots.Count==0,"Offscreen expiry does not burst");
        var oldObject=new GameObject();var oldShot=oldObject.Add<AlienShot>();oldShot.shot=oldObject;
        UnityEngine.Object.Destroyed.Clear();Call(oldShot,"OnTriggerEnter2D",food);Check(UnityEngine.Object.Destroyed.Count==1,"Missing optional VFX still consumes shot");
        Console.WriteLine($"Passed {checks} assertions against actual Shooter, animation bridge and AlienShot source using lightweight Unity API stubs. Not a Unity engine playtest.");
    }
}
public static class GameData { public static float playerScore,highScore; public static List<Shooter> shooters=new(); }
public class AudioManager2:MonoBehaviour { public bool playLaser; }
namespace UnityEngine
{
    public class Object
    {
        public static List<Vector3> Shots=new();
        public static List<Object> Destroyed=new();public static List<float> Scheduled=new();
        public static GameObject Instantiate(GameObject prefab,Vector3 position,Quaternion rotation){Shots.Add(position);return new GameObject();}
        public static void Destroy(Object target){Destroyed.Add(target);}public static void Destroy(Object target,float delay){Scheduled.Add(delay);}
    }
    public class GameObject:Object
    {
        readonly Dictionary<Type,Component> components=new(); public string tag; public bool activeSelf=true; public Transform transform=new();public GameObject gameObject=>this;
        public T Add<T>() where T:Component,new(){var c=new T{gameObject=this};components[typeof(T)]=c;return c;}
        public T GetComponent<T>() where T:Component => components.TryGetValue(typeof(T),out var c)?(T)c:null;
        public bool CompareTag(string t)=>tag==t;public static GameObject AudioManager;public static GameObject FindGameObjectWithTag(string t)=>AudioManager;
    }
    public class Component:Object { public GameObject gameObject;public Transform transform=>gameObject.transform;public T GetComponent<T>() where T:Component=>gameObject.GetComponent<T>(); }
    public class MonoBehaviour:Component
    {
        public bool enabled=true; public bool isActiveAndEnabled=>enabled&&gameObject.activeSelf;public List<IEnumerator> routines=new();
        public object StartCoroutine(IEnumerator routine){routines.Add(routine);routine.MoveNext();return routine;}public void StopAllCoroutines()=>routines.Clear();
    }
    public class Animator:Component { readonly Dictionary<int,bool> values=new();public static int StringToHash(string s)=>s.GetHashCode();public void SetBool(int id,bool value)=>values[id]=value;public bool GetBool(string s)=>values.TryGetValue(StringToHash(s),out var v)&&v; }
    public class SpriteRenderer:Component {public Color color;}
    public class Rigidbody2D:Component {public Vector2 velocity;}
    public class Collider2D:Component{}
    public class Collision2D {public GameObject gameObject;}
    public class WaitForSeconds {public float seconds;public WaitForSeconds(float value){seconds=value;}}
    public class Transform {public Vector3 position;public Quaternion rotation;public void Translate(Vector2 v){}public Vector3 TransformPoint(Vector3 v)=>new(position.x+v.x*.34129742f,position.y+v.y*.34129742f,position.z+v.z);}
    public struct Quaternion{public static Quaternion identity=>new();}
    public struct Vector2 {public float x,y;public Vector2(float x,float y){this.x=x;this.y=y;}public static Vector2 left=>new(-1,0);public static Vector2 zero=>new(0,0);public static Vector2 operator*(Vector2 a,float f)=>new(a.x*f,a.y*f);}
    public struct Vector3 {public float x,y,z;public Vector3(float x,float y,float z){this.x=x;this.y=y;this.z=z;}}
    public struct Color {public Color(float r,float g,float b,float a){}public static Color blue,red,white;}
    public static class Time {public static float deltaTime=.016f;}
    public static class Mathf {public static float Abs(float f)=>Math.Abs(f);}
    public class DisallowMultipleComponent:Attribute{}
    public class RequireComponent:Attribute{public RequireComponent(Type a,Type b){}}
}
namespace UnityEngine.SceneManagement {public static class SceneManager {public static void LoadScene(string s){}}}
