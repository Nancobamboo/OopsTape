using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class AssetSystem : MonoBehaviour
{
	public Transform UIRoot;
	public Transform SceneRoot;
	public Transform PoolRoot;
	public Transform UnitRoot;
	public Transform StaticRoot;
	public Transform DynamicRoot;
	public Dictionary<int, List<YViewControl>> m_PoolItemDict = new Dictionary<int, List<YViewControl>>();


	public AnimationCurve TweenTextAnimCurve;
	public void InitSystem()
	{
		Resources.UnloadUnusedAssets();
	}

	private void Awake()
	{
		YActionSystem.Instance.RegistAction(EActionId.TryLoadScene, new Action<object[]>(this.TryLoadScene));
	}

	private void OnDestroy()
	{
		YActionSystem.Instance.UnRegistAction(EActionId.TryLoadScene, new Action<object[]>(this.TryLoadScene));
		this.m_PoolItemDict.Clear();
	}
	public T CreateLevelObject<T>(Vector3 pos, Transform parent = null) where T : YViewControl
	{
		if (parent == null)
		{
			parent = this.SceneRoot;
		}

		EResType resType = (EResType)typeof(T).GetMethod("GetResType").Invoke(null, null);


		GameObject gameObject = this.CreateLevelObejct(resType, parent, pos);
		T t = gameObject.GetComponent<T>();
		if (t == null)
		{
			t = (gameObject.AddComponent(typeof(T)) as T);
		}
		return t;
	}

	public T OpenUI<T>(Transform parent, Vector3 localPos) where T : YViewControl
	{
		EResType resType = (EResType)typeof(T).GetMethod("GetResType").Invoke(null, null);
		GameObject gameObject = this.CreateUIPrefab(resType, parent);
		gameObject.transform.localPosition = localPos;
		T t = gameObject.GetComponent<T>();
		if (t == null)
		{
			t = (gameObject.AddComponent(typeof(T)) as T);
		}
		t.Asset = this;
		t.OnLoadAsset();
		return t;
	}

	// Open UI under StaticRoot
	public T OpenUIStatic<T>(Vector3? localPos = null) where T : YViewControl
	{
		var ctrl = OpenUI<T>(StaticRoot);
		if (localPos.HasValue)
		{
			ctrl.transform.localPosition = localPos.Value;
		}
		return ctrl;
	}

	// Open UI under DynamicRoot
	public T OpenUIDynamic<T>(Vector3? localPos = null) where T : YViewControl
	{
		var ctrl = OpenUI<T>(DynamicRoot);
		if (localPos.HasValue)
		{
			ctrl.transform.localPosition = localPos.Value;
		}
		return ctrl;
	}

	public T OpenUI<T>(Transform parent, float scale) where T : YViewControl
	{
		EResType resType = (EResType)typeof(T).GetMethod("GetResType").Invoke(null, null);
		GameObject gameObject = this.CreateUIPrefab(resType, parent);
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localScale = scale * Vector3.one;
		T t = gameObject.GetComponent<T>();
		if (t == null)
		{
			t = (gameObject.AddComponent(typeof(T)) as T);
		}
		t.Asset = this;
		t.OnLoadAsset();
		return t;
	}

	public T OpenUI<T>(Transform parent = null) where T : YViewControl
	{
		EResType resType = (EResType)typeof(T).GetMethod("GetResType").Invoke(null, null);
		GameObject gameObject = this.CreateUIPrefab(resType, parent);
		gameObject.transform.localPosition = Vector3.zero;
		T t = gameObject.GetComponent<T>();
		if (t == null)
		{
			t = (gameObject.AddComponent(typeof(T)) as T);
		}
		t.Asset = this;
		t.OnLoadAsset();
		return t;
	}

	private GameObject CreateLevelObejct(EResType resType, Transform parent, Vector3 pos)
	{
		if (parent == null)
		{
			parent = this.SceneRoot;
		}
		GameObject gameObject = Resources.Load<GameObject>("Level/" + resType.ToString());
		GameObject gameObject2 = Instantiate<GameObject>(gameObject, parent);
		gameObject2.transform.position = new Vector3(pos.x, gameObject.transform.position.y, pos.z);
		gameObject2.name = gameObject.name;
		return gameObject2;
	}

	public GameObject CreateArtObject(string artName, Transform parent)
	{
		GameObject gameObject = Resources.Load<GameObject>("Art/" + artName);
		GameObject gameObject2 = Instantiate<GameObject>(gameObject, parent);
		gameObject2.transform.localPosition = new Vector3(0f, gameObject.transform.position.y, 0f);
		return gameObject2;
	}

	public GameObject CreateEffectObject(string resType, Vector3 pos, Transform root = null)
	{
		if (root == null)
		{
			root = this.SceneRoot;
		}
		GameObject gameObject = Resources.Load<GameObject>("Effects/" + resType.ToString());
		GameObject gameObject2 = Instantiate<GameObject>(gameObject, this.SceneRoot);
		gameObject2.transform.position = new Vector3(pos.x, gameObject.transform.position.y, pos.z);
		return gameObject2;
	}

	public GameObject CreateUIPrefab(EResType resType, Transform parent = null)
	{
		if (parent == null)
		{
			parent = this.UIRoot;
		}
		GameObject gameObject = Resources.Load<GameObject>("UIPrefab/" + resType.ToString());

		GameObject gameObject2 = Instantiate<GameObject>(gameObject, parent);
		gameObject2.gameObject.name = gameObject.gameObject.name;
		return gameObject2;
	}

	public void TryLoadScene(params object[] data)
	{
		SceneManager.LoadScene((string)data[0]);
	}

	public T GetPoolItem<T>(Transform parent, Vector3 pos, bool isUIView = true) where T : YViewControl
	{
		T poolItem = this.GetPoolItem<T>(parent, isUIView);
		poolItem.transform.localPosition = pos;

		return poolItem;
	}

	public void ReturnPoolItem(YViewControl ctrl)
	{
		ctrl.gameObject.SetActive(false);
		ctrl.transform.SetParent(PoolRoot);
		ctrl.IsDirty = false;
	}

	public void ClearAutoReturnPoolItem(EResType resType)
	{
		m_PoolItemDict.Remove((int)resType);
	}

	public void RemoveReturnPoolItem(YViewControl ctrl)
	{
		m_PoolItemDict[(int)GetResType(ctrl)].Remove(ctrl);
	}

	//You Must AddCompoent At Prefab
	public AutoReturnControl GetAutoReturnPoolItem(EResType eresType, Transform parent, bool isUIView = true)
	{
		List<YViewControl> list = null;
		this.m_PoolItemDict.TryGetValue((int)eresType, out list);
		if (list == null)
		{
			list = new List<YViewControl>();
			this.m_PoolItemDict[(int)eresType] = list;
		}
		foreach (YViewControl yviewControl in list)
		{
			if (!yviewControl.IsDirty)
			{
				var trans = yviewControl.transform;
				trans.SetParent(parent);
				trans.localPosition = Vector3.zero;
				yviewControl.gameObject.SetActive(true);
				yviewControl.IsDirty = true;
				return yviewControl as AutoReturnControl;
			}
		}

		GameObject gameObject = isUIView ? this.CreateUIPrefab(eresType, parent) : this.CreateLevelObejct(eresType, parent, Vector3.zero);
		AutoReturnControl t = gameObject.GetComponent<AutoReturnControl>();
		if (t == null)
		{
			t = (gameObject.AddComponent(typeof(AutoReturnControl)) as AutoReturnControl);
		}
		t.transform.localPosition = Vector3.zero;

		t.Asset = this;
		t.OnLoadAsset();
		list.Add(t);
		t.IsDirty = true;
		return t;
	}

	public T GetPoolItem<T>(Transform parent, bool isUIView = true) where T : YViewControl
	{
		EResType eresType = (EResType)typeof(T).GetMethod("GetResType").Invoke(null, null);

		List<YViewControl> list = null;
		this.m_PoolItemDict.TryGetValue((int)eresType, out list);
		if (list == null)
		{
			list = new List<YViewControl>();
			this.m_PoolItemDict[(int)eresType] = list;
		}
		foreach (YViewControl yviewControl in list)
		{
			if (!yviewControl.IsDirty)
			{
				yviewControl.gameObject.SetActive(true);
				yviewControl.IsDirty = true;
				return yviewControl as T;
			}
		}
		GameObject gameObject = isUIView ? this.CreateUIPrefab(eresType, parent) : this.CreateLevelObejct(eresType, parent, Vector3.zero);
		T t = gameObject.GetComponent<T>();
		if (t == null)
		{
			t = (gameObject.AddComponent(typeof(T)) as T);
		}
		t.Asset = this;
		t.OnLoadAsset();
		list.Add(t);
		t.IsDirty = true;
		return t;
	}

	public Sprite GetSprite(string spriteName)
	{
		Sprite sprite = Resources.Load<Sprite>("Sprite/" + spriteName);
		if (sprite == null)
		{
			Debug.LogError("GetSprite Fail " + spriteName);
		}
		return sprite;
	}


	public Texture2D GetTextureByName(string texName, int width = 1024, int height = 1024)
	{
		Texture2D resultTex = null;

		string path = Application.dataPath + "/" + texName;

		//if (!Directory.Exists(path))
		//{
		//	Directory.CreateDirectory(path);
		//}

		//path = path + texName;

		if (!File.Exists(path))
		{
			return null;
		}

		FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
		fileStream.Seek(0, SeekOrigin.Begin);
		byte[] bytes = new byte[fileStream.Length];
		fileStream.Read(bytes, 0, (int)fileStream.Length);
		fileStream.Close();
		fileStream.Dispose();

		resultTex = new Texture2D(width, height);
		resultTex.LoadImage(bytes);

		return resultTex;
	}

	public Texture2D GetCaptureTexture(RectTransform rectTrans)
	{
		var mainCam = Camera.main;

		int width = Mathf.Abs((int)rectTrans.rect.size.x);
		int height = Mathf.Abs((int)rectTrans.rect.size.y);

		Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
		int x = (int)((mainCam.WorldToScreenPoint(rectTrans.position).x) - (width * rectTrans.pivot.x));
		int y = (int)((mainCam.WorldToScreenPoint(rectTrans.position).y) - (height * rectTrans.pivot.y));

		tex.ReadPixels(new Rect(x, y, width, height), 0, 0);
		tex.Apply();
		return tex;
	}

	public void SaveFileByTexutre(Texture2D tex, string texName)
	{
		byte[] bytes = tex.EncodeToPNG();
		string path = Application.dataPath + "/" + texName;
		File.WriteAllBytes(path, bytes);
	}

	public Sprite GetSpriteByTexture(Rect rect, string texName)
	{
		var tex = GetTextureByName(texName, (int)rect.width, (int)rect.height);

		if (tex != null)
		{
			return Sprite.Create(tex, rect, Vector2.zero);
		}
		return null;
	}

	public EResType GetResTypeByName(string resName)
	{
		EResType result = EResType.None;
		Enum.TryParse<EResType>(resName, true, out result);
		return result;
	}
	public EResType GetResType(YViewControl control)
	{
		EResType resType = (EResType)control.GetType().GetMethod("GetResType").Invoke(null, null);
		return resType;
	}

	

	Dictionary<string, UIGuideTipControl> m_GuideDict = new Dictionary<string, UIGuideTipControl>();

	UIGuideTipControl GetGuideCtrlBykey(string key)
	{
		UIGuideTipControl result = null;
		m_GuideDict.TryGetValue(key, out result);
		return result;
	}

	public bool NeedShowGuide(string key)
	{
		return !PlayerPrefs.HasKey(key);
	}

	public void ShowGuide(string key, Transform root, string txt = null, bool useMask = false, float size = 100)
	{
		if (!PlayerPrefs.HasKey(key))
		{
			UIGuideTipControl result = GetGuideCtrlBykey(key);

			if (result == null)
			{
				m_GuideDict[key] = OpenUI<UIGuideTipControl>(root);
				m_GuideDict[key].SetData(txt, useMask, size);
			}
		}
	}

	public void FinishGuideNoClear(string key)
	{
		UIGuideTipControl result = GetGuideCtrlBykey(key);

		result?.Close();
		m_GuideDict.Remove(key);
	}

	public void FinishGuide(string key)
	{
		if (!PlayerPrefs.HasKey(key))
		{
			PlayerPrefs.SetInt(key, 1);

			UIGuideTipControl result = GetGuideCtrlBykey(key);
			if (result != null)
			{
				result.Close();
				m_GuideDict.Remove(key);
			}
		}
	}

	public Sprite GetGISprite(string msg, int width = 512, int height = 512)
	{
		byte[] imageBytes = Convert.FromBase64String(msg);

		Texture2D texture = new Texture2D(width, height);
		if (texture.LoadImage(imageBytes))
		{
			return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

		}
		return null;
	}



}
