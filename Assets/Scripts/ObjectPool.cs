using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : Singleton<ObjectPool>
{
    [SerializeField]
    private GameObject[] usePrefabObjs;//

    [SerializeField]
    private Dictionary<int, Queue<GameObject>> objPools = new Dictionary<int, Queue<GameObject>>();

    void Awake()
    {
        for (int nowObjIndex = 0; nowObjIndex < usePrefabObjs.Length; nowObjIndex++)
        {
            objPools.Add(nowObjIndex, new Queue<GameObject>());
            Initialize(1, nowObjIndex);//
        }
    }
    private void Initialize(int count, int nowPoolIndex) //시작 생성 (오브젝트 생성 개수, 현재 설정중인 풀 인덱스)
    {
        for (int nowCount = 0; nowCount < count; nowCount++)
        {
            objPools[nowPoolIndex].Enqueue(CreateNewObjs(nowPoolIndex));
            //SwordAuraQueue.Enqueue(CreateNewObjs());//
        }
    }

    private GameObject CreateNewObjs(int nowObjIndex) //오브젝트 생성 함수
    {
        //var newCreateObj = Instantiate(SwordAuraObj[0], transform).GetComponent<SwordAura>();
        GameObject newCreateObj = Instantiate(usePrefabObjs[nowObjIndex], transform);
        newCreateObj.gameObject.SetActive(false);
        return newCreateObj;
    }

    
    public GameObject GetObject(int objIndex) //오브젝트가 있으면 꺼내가고 없다면 새로 생성해서 가져감 (사용할 겜 오브젝트 배열 인덱스 맞춰서 넣기)
    {
        if (objPools[objIndex].Count > 0)
        {
            //var obj = SwordAuraQueue.Dequeue();
            GameObject obj = objPools[objIndex].Dequeue();
            obj.transform.SetParent(null);
            print("작동");
            obj.gameObject.SetActive(true);
            return obj;
        }
        else 
        {
            //var newObj = CreateNewObjs();
            GameObject newObj = CreateNewObjs(objIndex);
            newObj.transform.SetParent(null);
            newObj.gameObject.SetActive(true);
            return newObj;
        }
    }

    public void ReturnObject(GameObject usedObj, int objIndex) //사용 완료한 오브젝트와 그 오브젝트의 프리펩 인덱스
    {
        usedObj.SetActive(false);
        usedObj.transform.SetParent(transform);
        objPools[objIndex].Enqueue(usedObj);
    }
}
