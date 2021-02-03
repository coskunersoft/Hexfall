using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectCamp : MonoBehaviour
{
    public static ObjectCamp instance;

    public List<Object> allFreeObjects;
    private List<System.Tuple<System.Type, System.Func<Object>>> creationRules;
    private List<System.Tuple<System.Type, System.Action<Object>>> pushActions;
    private List<System.Tuple<System.Type, System.Action<Object>>> takeActions;

    private void Awake()
    {
        instance = this;
        creationRules = new List<System.Tuple<System.Type, System.Func<Object>>>();
        creationRules.Add(new System.Tuple<System.Type, System.Func<Object>>(typeof(GameItem), () =>
        {
            GameItem Created = Instantiate(GameManager.instance.generalGameVars.gameItemPrefab).GetComponent<GameItem>();
            return Created;
        }));
        creationRules.Add(new System.Tuple<System.Type, System.Func<Object>>(typeof(FlyingScore), () =>
        {
            FlyingScore Created = Instantiate(GameManager.instance.generalGameVars.flyingScorePrefab).GetComponent<FlyingScore>();
            return Created;
        }));

        pushActions = new List<System.Tuple<System.Type, System.Action<Object>>>();
        pushActions.Add(new System.Tuple<System.Type, System.Action<Object>>(typeof(GameItem), (Object o) =>
        {
            GameItem objectx = o as GameItem;
            objectx.gameObject.SetActive(false);
            objectx.transform.position = Vector3.zero;
        }));
        pushActions.Add(new System.Tuple<System.Type, System.Action<Object>>(typeof(FlyingScore), (Object o) =>
        {
            FlyingScore objectx = o as FlyingScore;
            objectx.gameObject.SetActive(false);
            objectx.transform.position = Vector3.zero;
        }));

        takeActions = new List<System.Tuple<System.Type, System.Action<Object>>>();
        takeActions.Add(new System.Tuple<System.Type, System.Action<Object>>(typeof(GameItem), (Object o) =>
        {
            GameItem objectx = o as GameItem;
            objectx.gameObject.SetActive(true);
        }));
        takeActions.Add(new System.Tuple<System.Type, System.Action<Object>>(typeof(FlyingScore), (Object o) =>
        {
            FlyingScore objectx = o as FlyingScore;
            objectx.gameObject.SetActive(true);
        }));
    }

    public T GetObject<T>() where T: Object
    {
        T result = default;
        System.Type searchingType = typeof(T);
        result = allFreeObjects.Find(x => x.GetType() == searchingType) as T;
        if (!result)
        {
            System.Func<Object> findedRule = creationRules.Find(x => x.Item1 == searchingType)?.Item2;
            if (findedRule!=null)
            {
                result = (T)findedRule.Invoke();
            }
        }
        else
        {
            System.Action<Object> findedAction = takeActions.Find(x => x.Item1 == searchingType)?.Item2;
            findedAction?.Invoke(result);
            allFreeObjects.Remove(result);
        }
        return result;
    }

    public void TakeObjecy<T>(T O) where T:Object
    {
        allFreeObjects.Add(O);
        System.Type searchingType = typeof(T);
        Debug.Log(searchingType);
        System.Action<Object> findedAction = pushActions.Find(x => x.Item1 == searchingType)?.Item2;
        findedAction?.Invoke(O);
    }
}
