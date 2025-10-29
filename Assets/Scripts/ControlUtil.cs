using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ControlUtil
{	
	public static Vector3 GetRandomDireaction()
	{
		return DirectionValue[UnityEngine.Random.Range(0, DirectionValue.Length)];
	}

	public static T CreateView<T>(Transform trans) where T : YBaseView, new()
	{
		T view = Activator.CreateInstance<T>();
		view.OnInit(trans);
		return (T)((object)view);
	}

	
	public static Color GetColorByHexString(string colorString)
	{
		Color white = Color.white;
		ColorUtility.TryParseHtmlString(colorString, out white);
		return white;
	}
	
	public static void ExchangeListItem<T>(List<T> list, int indexA, int indexB)
	{
		T value = list[indexB];
		list[indexB] = list[indexA];
		list[indexA] = value;
	}



	public static Vector2 GetMousePosition()
	{
        if (Input.touches.Length != 0)
        {
            return Input.GetTouch(0).position; ;
        }
		return Input.mousePosition;
    }


	public static bool GetMouseLeftInput(ref Vector2 mousePos)
	{
		if (Input.GetMouseButtonDown(0))
		{
			mousePos = Input.mousePosition;
			return true;
		}
		if (Input.touches.Length != 0)
		{
			mousePos = Input.GetTouch(0).position;
			return true;
		}
		return false;
	}

	
	public static bool GetMouseLeftKeepInput(ref Vector2 mousePos)
	{
		if (Input.GetMouseButton(0))
		{
			mousePos = Input.mousePosition;
			return true;
		}
		if (Input.touches.Length != 0)
		{
			mousePos = Input.GetTouch(0).position;
			return true;
		}
		return false;
	}
	
	public static Vector3[] DirectionValue = new Vector3[]
	{
		Vector3.right,
		-Vector3.right,
		Vector3.forward,
		-Vector3.forward,
		new Vector3(1f, 0f, -1f),
		new Vector3(1f, 0f, 1f),
		new Vector3(-1f, 0f, -1f),
		new Vector3(-1f, 0f, 1f)
	};


    public static void ShuffleArray<T>(T[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            var targetPos = Random.Range(0, array.Length);
            if (targetPos != i)
            {
                var tmpVal = array[targetPos];
                array[targetPos] = array[i];
                array[i] = tmpVal;
            }
        }
    }

    public static void ShuffleList<T>(List<T> list, int startIndex = 0)
	{
		for (int i = startIndex; i < list.Count; i++)
		{
			var targetPos = Random.Range(startIndex, list.Count);
			if (targetPos != i)
			{
				var tmpVal = list[targetPos];
				list[targetPos] = list[i];
				list[i] = tmpVal;
			}
		}
    }

	
	public static float SqrDistance(Transform aTrans, Transform bTrans)
	{
		var posA = new Vector3(aTrans.position.x, 0, aTrans.position.z);
		var posB = new Vector3(bTrans.position.x, 0, bTrans.position.z);

		return Vector3.SqrMagnitude(posB - posA);
	}

	public static AssetSystem GetAsset()
	{
		return GameObject.FindObjectOfType<AssetSystem>();
	}

	
 


    public static List<string> GetStringValueListByString(string value)
    {
        var resultList = new List<int>();
        return value.Split('|').ToList();
    }

    public static List<int> GetIntValueListByString(string value)
    {
        var strList = value.Split('|');

        var resultList = new List<int>(strList.Length);
        foreach (var str in strList)
        {
            int intValue = 0;
            if (!int.TryParse(str, out intValue))
            {
                Debug.Log(str);
            }
            resultList.Add(intValue);
        }
        return resultList;
    }

    public static List<float> GetFloatValueListByString(string value)
    {
        var strList = value.Split('|');
        var resultList = new List<float>(strList.Length);
        foreach (var str in strList)
        {
            float val = 0;
            if (!float.TryParse(str, out val))
            {
                Debug.Log(str);
            }
            resultList.Add(val);
        }
        return resultList;
    }

    public static bool IsRandomSucceed(int ratio)
    {
        return ratio >= Random.Range(1, 101);
    }

    public static IEnumerator FadeIn(CanvasGroup group, float alpha, float duration)
    {
        var time = 0.0f;
        var originalAlpha = group.alpha;
        while (time < duration)
        {
            time += Time.deltaTime;
            group.alpha = Mathf.Lerp(originalAlpha, alpha, time / duration);
            yield return new WaitForEndOfFrame();
        }

        group.alpha = alpha;
    }

    public static IEnumerator FadeOut(CanvasGroup group, float alpha, float duration)
    {
        var time = 0.0f;
        var originalAlpha = group.alpha;
        while (time < duration)
        {
            time += Time.deltaTime;
            group.alpha = Mathf.Lerp(originalAlpha, alpha, time / duration);
            yield return new WaitForEndOfFrame();
        }

        group.alpha = alpha;
    }



    public static string[] MaterialNameArray =
    {
        "DoodleDraw","CircleFade", "ShinyFX", "Glitch", "Fluo", "HolographicTint", "Perspective3D", "ColorHSV","TurnFire"
    };



}
