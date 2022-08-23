using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : Singleton<ObjectPool>
{
    [SerializeField]
    private GameObject SwordAuraObj;

    private Queue<SwordAura> SwordAuraQueue = new Queue<SwordAura>();

    void Awake()
    {
        Initialize(1);
    }

    private SwordAura CreateNewObjs() //오브젝트 생성 함수
    {
        var newCreateObj = Instantiate(SwordAuraObj, transform).GetComponent<SwordAura>();
        newCreateObj.gameObject.SetActive(false);
        return newCreateObj;
    }

    private void Initialize(int count) //시작 생성
    {
        for (int nowCount = 0; nowCount <= count; nowCount++)
        {
            SwordAuraQueue.Enqueue(CreateNewObjs());
        }
    }

    public SwordAura GetObject() //오브젝트가 있으면 꺼내가고 없다면 새로 생성해서 가져감
    {
        if (SwordAuraQueue.Count > 0)
        {
            var obj = SwordAuraQueue.Dequeue();
            obj.transform.SetParent(null);
            obj.gameObject.SetActive(true);
            return obj;
        }
        else 
        {
            var newObj = CreateNewObjs();
            newObj.transform.SetParent(null);
            newObj.gameObject.SetActive(true);
            return newObj;
        }
    }

    public void ReturnObject(SwordAura swordAura)
    {
        swordAura.gameObject.SetActive(false);
        swordAura.transform.SetParent(transform);
        SwordAuraQueue.Enqueue(swordAura);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
