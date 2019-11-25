using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//实时检测屏幕中Input.touches的数量，当某点距离组合中心点超出一定范围则舍弃该点。
//算出中心点的位置
public class MutilplePointsDetect : MonoBehaviour {
    [SerializeField] UnityEngine.UI.Text text;
    [SerializeField] UnityEngine.UI.Text maxCountText;
    [SerializeField]
    private int centerOffset = 300;
    private int sqrCenterOffset = 0;
    List<GameObject> touchGoListPool;
    [SerializeField] GameObject touchPrefab;
    [SerializeField] Transform touchParent;
    [SerializeField] Transform Result;
    List<List<Touch>> listofList = new List<List<Touch>>();



    void Start()
    {
        listofList = new List<List<Touch>>();

        touchGoListPool = new List<GameObject>();
        sqrCenterOffset = centerOffset * centerOffset;
      
    }

    bool hasConform = false;
    private Vector2 preCenterPoint;
    private int PreMaxCount;
    private int MaxCount;
    private float time = 1f;
    bool flag = false;
    bool nearToLastCenterPointWhen0_1 = false;

    Vector2 GetCenter(List<Touch> list)
    {
        Vector2 center = Vector2.zero;
        foreach (var item in list)
            center += item.position;
        center /= list.Count;

        return center;
    }

    //手放上，0.5秒内检测点数最大值Max
    //放开手，点数在0-Max之间变化
    private void Update()
    {
        List<Touch> list= Detect();
        if (list != null)
        {
            text.text = "当前触控点数：" + Input.touchCount + "\n"    
                + "当前有效触控点数：" + list.Count;
            if (list.Count >= 3)
            {
                Vector2 center = GetCenter(list);
                if (flag == false)
                {
                    flag = true;
                    nearToLastCenterPointWhen0_1 = Vector2.Distance(preCenterPoint, center) < centerOffset;
                }

                bool detectingMAXCount = false;
                if (!nearToLastCenterPointWhen0_1)
                {
                    if (time > 0)//1秒检测最大点数时间
                    {
                        time -= Time.deltaTime;
                        if (list.Count > MaxCount) MaxCount = list.Count;
                        maxCountText.text = string.Format("正在检测...");
                        detectingMAXCount = true;
                    }
                    else
                    {
                        detectingMAXCount = false;
                        if (hasConform == false)
                        {
                            if (MaxCount != PreMaxCount) //显示MaxCount 对应的东西
                            {
                                PreMaxCount = MaxCount;
                                maxCountText.text = string.Format("显示{0} 对应的新东西", MaxCount);
                                Result.RectTransform().anchoredPosition = center;

                            }
                            else //把当前显示的东西慢慢移动到目标点
                                maxCountText.text = string.Format("与之前一样，显示{0} 对应的东西", MaxCount);

                            hasConform = true;
                        }
                     
                    }
                }
                if (detectingMAXCount == false)
                {
                    preCenterPoint = center;
                    Result.RectTransform().anchoredPosition = Vector2.Lerp(Result.RectTransform().anchoredPosition, center, Time.deltaTime * 5f);
                }
    

            }
            else
            {
                MaxCount = 0;
                time = 1f;
                hasConform = false;
                flag = false;
            }
        }
        ShowerTouch();
    }

    List<Touch> Detect()
    {

        listofList.Clear();

        if (Input.touches.Length < 3)
        {
            List<Touch> list = new List<Touch>();
            for (int i = 0; i < Input.touches.Length; i++)
                list.Add(Input.touches[i]);
            listofList.Add(list);
        }
        else
        {
            for (int i = 0; i < Input.touches.Length; i++)
            {
                List<Touch> list = new List<Touch>();
                list.Add(Input.touches[i]);
                listofList.Add(list);
            }
            while (listofList.Count > 1)
            {
                if (listofList.Count > 1)
                    listofList.Sort((list1, list2) => { return list2.Count.CompareTo(list1.Count); });
                List<Touch> endList = listofList[listofList.Count - 1];
                for (int i = 0; i < endList.Count; i++) //处理endList中每一个touch点，把他们加入到距离最近的组中去
                {
                    Touch touch = endList[i];
                    float minDistance = Mathf.Infinity; int minlistID = -1; // 用于记录touch到最近的listofList组及其到该组的最短距离
                    for (int j = 0; j < listofList.Count - 1; j++)
                    {
                        float minDistanceofJ = Mathf.Infinity;
                        foreach (var t in listofList[j])
                        {
                            float dis = Vector2.Distance(touch.position, t.position);
                            if (dis < centerOffset && dis < minDistanceofJ) minDistanceofJ = dis;

                        }
                        if (minDistanceofJ < minDistance)
                        {
                            minDistance = minDistanceofJ;
                            minlistID = j;
                        }
                    }
                    if (minDistance < sqrCenterOffset && listofList[minlistID].Contains(touch) == false)
                        listofList[minlistID].Add(touch);

                }
                listofList.Remove(endList);
            }
        }
        return listofList.Count>0?listofList[0]:null;
    }

    void ShowerTouch()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            if (touchGoListPool.Count - 1 < i)
            {
                GameObject touchGo = GameObject.Instantiate(touchPrefab, touchParent);
                touchGoListPool.Add(touchGo);
            }
            touchGoListPool[i].SetActive(true);
            touchGoListPool[i].transform.RectTransform().anchoredPosition = Input.touches[i].position;
        }

        if (Input.touchCount < touchGoListPool.Count)
        {
            for (int i = Input.touchCount; i < touchGoListPool.Count; i++)
            {
                touchGoListPool[i].SetActive(false);
            }
        }
    }

}


public static class RectExtension
{
    public static RectTransform  RectTransform( this Transform trans)
    {
        return trans as RectTransform;
    }
}
