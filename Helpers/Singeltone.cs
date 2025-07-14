using UnityEngine;

namespace ThreeLines.Helpers
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<T>();

                    if (instance == null)
                    {
                        TLDebug.LogRedBold("An instance of " + typeof(T) + " is needed in the scene, but there is none. Creating a new instance.");
                        GameObject singletonObject = new GameObject(typeof(T).Name);
                        instance = singletonObject.AddComponent<T>();
                    }
                }

                return instance;
            }
        }

        protected virtual void Awake()
        {
            if(instance !=null){
                if(instance == this as T) 
                    return;
                TLDebug.LogYellow($"you already have an instance of {typeof(T)} in the scene. Destroying this one which name is {name} and Instance = {Instance.name}.");
                Destroy(this);
                return;
            }
            instance = this as T;
            TLDebug.LogGreen("Instance of " + typeof(T) + " is created.");
            //DontDestroyOnLoad(this.);
        }
    }

}