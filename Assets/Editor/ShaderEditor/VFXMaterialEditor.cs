using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class VFXMaterialEditor : ShaderGUI
{
	public MaterialEditor m_MaterialEditor;
	public Material targetMat;

	#region Material
	MaterialProperty _BlendMode_prop = null;
	MaterialProperty _BlendSrc_prop = null;
	MaterialProperty _BlendDst_prop = null;

	MaterialProperty _NULL_ColorFold_prop = null;
	MaterialProperty _Color_prop = null;
	public MaterialProperty _FixValue_prop = null;
	MaterialProperty _ColorAlphaBrightnessContrast_prop = null;

	MaterialProperty _NULL_OptionFold_prop = null;
	MaterialProperty _ZWrite_prop = null;
	MaterialProperty _ZTest_prop = null;
	MaterialProperty _CullMode_prop = null;

	MaterialProperty _MainRot_prop = null;
	#endregion

	public GUILayoutOption[] labelOp = new[] { GUILayout.MinWidth(50), GUILayout.MaxWidth(110) };

	public float FloatLayout(MaterialProperty mp, string name)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(name, labelOp);
		GUILayout.FlexibleSpace();
		EditorGUI.BeginChangeCheck();
		float temp = EditorGUILayout.FloatField(" ", mp.floatValue, GUILayout.ExpandWidth(false));
		if (EditorGUI.EndChangeCheck())
		{
			mp.floatValue = temp;
		}
		GUILayout.EndHorizontal();
		return mp.floatValue;
	}

	public float FloatLayout(float value, string name)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(name, labelOp);
		GUILayout.FlexibleSpace();
		value = EditorGUILayout.FloatField(" ", value, GUILayout.ExpandWidth(false));
		GUILayout.EndHorizontal();
		return value;
	}
	public float RangeLayout(MaterialProperty mp, string name, float min, float max)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(name, labelOp);
		GUILayout.FlexibleSpace();
		EditorGUI.BeginChangeCheck();
		float temp = EditorGUILayout.Slider(mp.floatValue, min, max);
		if (EditorGUI.EndChangeCheck())
		{
			mp.floatValue = temp;
		}
		GUILayout.EndHorizontal();
		return mp.floatValue;
	}
	public float RangeLayout(MaterialProperty mpVector4, string name, float min, float max, int index)
	{
		Vector4 temp = mpVector4.vectorValue;
		GUILayout.BeginHorizontal();
		GUILayout.Label(name, labelOp);
		GUILayout.FlexibleSpace();
		EditorGUI.BeginChangeCheck();
		switch (index)
		{
			case (1):
				temp.x = EditorGUILayout.Slider(temp.x, min, max);
				break;
			case (2):
				temp.y = EditorGUILayout.Slider(temp.y, min, max);
				break;
			case (3):
				temp.z = EditorGUILayout.Slider(temp.z, min, max);
				break;
			case (4):
				temp.w = EditorGUILayout.Slider(temp.w, min, max);
				break;
		}
		GUILayout.EndHorizontal();
		if (EditorGUI.EndChangeCheck())
		{
			mpVector4.vectorValue = temp;
		}
		return mpVector4.floatValue;
	}
	public float RangeLayout(float value, string name, float min, float max)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(name, labelOp);
		GUILayout.FlexibleSpace();
		value = EditorGUILayout.Slider(value, min, max);
		GUILayout.EndHorizontal();
		return value;
	}
	public Vector4 VectorLayout(MaterialProperty mp, string name1, string name2, string name3, string name4)
	{
		Vector4 temp = mp.vectorValue;
		if (name1 != "NULL") temp.x = FloatLayout(mp.vectorValue.x, name1);
		if (name2 != "NULL") temp.y = FloatLayout(mp.vectorValue.y, name2);
		if (name3 != "NULL") temp.z = FloatLayout(mp.vectorValue.z, name3);
		if (name4 != "NULL") temp.w = FloatLayout(mp.vectorValue.w, name4);
		mp.vectorValue = temp;
		return mp.vectorValue;
	}
	public Vector4 Vector2Layout(MaterialProperty mp, string name1, string name2)
	{
		Vector2 Panner1 = new Vector2(mp.vectorValue.x, mp.vectorValue.y);
		Vector2 Panner2 = new Vector2(mp.vectorValue.z, mp.vectorValue.w);
		EditorGUI.BeginChangeCheck();
		if (name1 != "NULL")
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(name1, labelOp);
			GUILayout.FlexibleSpace();
			Panner1 = EditorGUILayout.Vector2Field("", new Vector2(mp.vectorValue.x, mp.vectorValue.y), GUILayout.MaxWidth(200));
			GUILayout.EndHorizontal();
		}

		if (name2 != "NULL")
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(name2, labelOp);
			GUILayout.FlexibleSpace();
			Panner2 = EditorGUILayout.Vector2Field("", new Vector2(mp.vectorValue.z, mp.vectorValue.w), GUILayout.MaxWidth(200));
			GUILayout.EndHorizontal();
		}
		if (EditorGUI.EndChangeCheck())
		{
			mp.vectorValue = new Vector4(Panner1.x, Panner1.y, Panner2.x, Panner2.y);
		}
		return mp.vectorValue;
	}
	public Vector4 Vector3Layout(MaterialProperty mpVector4, string name1)
	{
		Vector4 pos = mpVector4.vectorValue;
		GUILayout.BeginHorizontal();
		GUILayout.Label(name1, labelOp);
		GUILayout.FlexibleSpace();
		EditorGUI.BeginChangeCheck();
		pos = EditorGUILayout.Vector3Field("", new Vector3(pos.x, pos.y, pos.z), GUILayout.MaxWidth(200));
		GUILayout.EndHorizontal();
		if (EditorGUI.EndChangeCheck())
		{
			mpVector4.vectorValue = pos;
		}
		return pos;
	}

	public float ToggleLayout(MaterialProperty mp, string name)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(name, labelOp);
		GUILayout.FlexibleSpace();
		EditorGUI.BeginChangeCheck();
		float temp = GUILayout.Toggle(mp.floatValue != 0, "") ? 1 : 0;
		if (EditorGUI.EndChangeCheck())
		{
			mp.floatValue = temp;
		}
		GUILayout.Space(48);
		GUILayout.EndHorizontal();
		return mp.floatValue;
	}
	public float ToggleLayout(float toggle, string name)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(name, labelOp);
		GUILayout.FlexibleSpace();
		toggle = GUILayout.Toggle(toggle != 0, "") ? 1 : 0;
		GUILayout.Space(48);
		GUILayout.EndHorizontal();
		return toggle;
	}
	public Vector4 Toggle4Layout(MaterialProperty mp, string name, int index)
	{
		Vector4 temp = mp.vectorValue;
		GUILayout.BeginHorizontal();
		GUILayout.Label(name, labelOp);
		GUILayout.FlexibleSpace();
		EditorGUI.BeginChangeCheck();
		switch (index)
		{
			case (1):
				temp.x = GUILayout.Toggle(temp.x != 0, "") ? 1 : 0;
				break;
			case (2):
				temp.y = GUILayout.Toggle(temp.y != 0, "") ? 1 : 0;
				break;
			case (3):
				temp.z = GUILayout.Toggle(temp.z != 0, "") ? 1 : 0;
				break;
			case (4):
				temp.w = GUILayout.Toggle(temp.w != 0, "") ? 1 : 0;
				break;
		}
		GUILayout.Space(48);
		GUILayout.EndHorizontal();
		if (EditorGUI.EndChangeCheck())
		{
			mp.vectorValue = temp;
		}
		return temp;
	}
	public float EnumLayout(MaterialProperty mp, string name, string[] enumNames)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(name, labelOp);
		GUILayout.FlexibleSpace();
		EditorGUI.BeginChangeCheck();

		float temp = EditorGUILayout.Popup("", (int)mp.floatValue, enumNames, labelOp);
		if (EditorGUI.EndChangeCheck())
		{
			mp.floatValue = temp;
		}
		GUILayout.EndHorizontal();
		return mp.floatValue;
	}
	public Vector4 EnumLayout(MaterialProperty mp, string name, string[] enumNames, int index)
	{
		Vector4 temp = mp.vectorValue;
		GUILayout.BeginHorizontal();
		GUILayout.Label(name, labelOp);
		GUILayout.FlexibleSpace();
		EditorGUI.BeginChangeCheck();
		switch (index)
		{
			case (1):
				temp.x = EditorGUILayout.Popup("", (int)temp.x, enumNames, labelOp);
				break;
			case (2):
				temp.y = EditorGUILayout.Popup("", (int)temp.y, enumNames, labelOp);
				break;
			case (3):
				temp.z = EditorGUILayout.Popup("", (int)temp.z, enumNames, labelOp);
				break;
			case (4):
				temp.w = EditorGUILayout.Popup("", (int)temp.w, enumNames, labelOp);
				break;
		}
		if (EditorGUI.EndChangeCheck())
		{
			mp.vectorValue = temp;
		}
		GUILayout.EndHorizontal();
		return mp.vectorValue;
	}
	public float ToolBarLayout(MaterialProperty mp, string name, string[] texts)
	{
		GUILayout.Space(8);
		GUILayout.BeginHorizontal();
		GUILayout.Label(name);
		EditorGUI.BeginChangeCheck();
		float temp = GUILayout.Toolbar((int)mp.floatValue, texts, GUILayout.Width(80));
		if (EditorGUI.EndChangeCheck())
		{
			mp.floatValue = temp;
		}
		GUILayout.EndHorizontal();
		GUILayout.Space(4);
		return mp.floatValue;
	}
	public float ToolBarLayout(float index, string name, string[] texts)
	{
		GUILayout.Space(8);
		GUILayout.BeginHorizontal();
		GUILayout.Label(name);
		index = GUILayout.Toolbar((int)index, texts, GUILayout.Width(80));
		GUILayout.EndHorizontal();
		GUILayout.Space(4);
		return index;
	}
	public bool FoldOutLayout(MaterialProperty mp, string name)
	{
		GUILayout.BeginVertical("box");
		GUILayout.BeginHorizontal();
		GUILayout.Space(14);
		EditorGUI.BeginChangeCheck();
		bool foldOut = EditorGUILayout.Foldout(mp.floatValue == 1, name, true);
		if (EditorGUI.EndChangeCheck())
		{
			mp.floatValue = foldOut ? 1 : 0;
		}
		GUILayout.Space(14);
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
		return foldOut;
	}
	public void OnDrawHelpBtn(string url)
	{
		if (GUILayout.Button("Help Document"))
		{
			Application.OpenURL(url);
		}
	}
	public float OnDrawBlendMode(MaterialProperty[] props)
	{
		_BlendMode_prop = FindProperty("_BlendMode", props);
		GUILayout.BeginVertical("box");
		GUILayout.Space(4);
		GUILayout.BeginHorizontal();
		GUILayout.Label("BlendMode");
		EditorGUI.BeginChangeCheck();
		_BlendMode_prop.floatValue = GUILayout.Toolbar((int)_BlendMode_prop.floatValue, new[] { "Addtive", "AlphaBlend", "Opaque" }, GUILayout.Width(234));
		GUILayout.EndHorizontal();
		GUILayout.Space(4);
		GUILayout.EndVertical();
		if (EditorGUI.EndChangeCheck())
		{
			_BlendSrc_prop = FindProperty("_BlendSrc", props);
			_BlendDst_prop = FindProperty("_BlendDst", props);
			_ZWrite_prop = FindProperty("_MyZWrite", props);
			Vector4 temp = Vector4.zero;
			
			switch ((int)_BlendMode_prop.floatValue)
			{
				case 0:
					_BlendSrc_prop.floatValue = (float)BlendMode.SrcAlpha;
					_BlendDst_prop.floatValue = (float)BlendMode.One;
					_ZWrite_prop.floatValue = 0;
					targetMat.renderQueue = 3000;
					temp.y = 1;
					break;
				case 1:
					_BlendSrc_prop.floatValue = (float)BlendMode.SrcAlpha;
					_BlendDst_prop.floatValue = (float)BlendMode.OneMinusSrcAlpha;
					_ZWrite_prop.floatValue = 0;
					targetMat.renderQueue = 3000;
					temp.y = 0;
					break;
				case 2:
					_BlendSrc_prop.floatValue = (float)BlendMode.One;
					_BlendDst_prop.floatValue = (float)BlendMode.Zero;
					_ZWrite_prop.floatValue = 1;
					targetMat.renderQueue = 2000;
					temp.y = 1;
					break;
			}
			if (_FixValue_prop != null)
			{
				temp.x = _FixValue_prop.vectorValue.x;
				temp.z = _FixValue_prop.vectorValue.z;
				temp.w = _FixValue_prop.vectorValue.w;
				_FixValue_prop.vectorValue = temp;

			}
		}
		return _BlendMode_prop.floatValue;
	}
	public float OnDrawColorCorrection(MaterialProperty[] props)
	{
		_NULL_ColorFold_prop = FindProperty("_NULL_ColorFold", props);
		_Color_prop = FindProperty("_Color", props);
		_FixValue_prop = FindProperty("_FixValue", props);
		_ColorAlphaBrightnessContrast_prop = FindProperty("_ColorAlphaBrightnessContrast", props);

		GUILayout.BeginVertical("box");
		GUILayout.BeginHorizontal();
		GUILayout.Space(14);
		_NULL_ColorFold_prop.floatValue = EditorGUILayout.Foldout(_NULL_ColorFold_prop.floatValue == 1, "ColorCorrection", true) ? 1 : 0;
		GUILayout.Space(14);
		GUILayout.EndHorizontal();
		if (_NULL_ColorFold_prop.floatValue == 1)
		{
			m_MaterialEditor.ColorProperty(_Color_prop, "Color");
			if (_FixValue_prop!=null)
			{
				Vector4 temp = _FixValue_prop.vectorValue;
				temp.x = RangeLayout(_FixValue_prop.vectorValue.x, "CC", 0.0f, 1.0f);
				_FixValue_prop.vectorValue = temp;
			}
			VectorLayout(_ColorAlphaBrightnessContrast_prop, "ColorBrightness", "ColorContrast", "AlphaBrightness", "AlphaContrast");
		}
		GUILayout.EndVertical();
		return _NULL_ColorFold_prop.floatValue;
	}

	public void OnDrawMainRot(MaterialProperty[] props)
	{
		_MainRot_prop = FindProperty("_MainRot", props);

		Vector4 mainRotTemp = _MainRot_prop.vectorValue;
		GUILayout.BeginHorizontal();
		GUILayout.Label("MainRotation", labelOp);
		mainRotTemp.z = GUILayout.Toggle(mainRotTemp.z != 0, "") ? 1 : 0;
		GUILayout.FlexibleSpace();
		if (mainRotTemp.z < 0.5) GUI.enabled = false;
		mainRotTemp.w = EditorGUILayout.Slider(mainRotTemp.w, 0, 1);
		GUI.enabled = true;
		GUILayout.EndHorizontal();
		float rotTemp = mainRotTemp.w * Mathf.PI * 2f;
		mainRotTemp.x = Mathf.Sin(rotTemp);
		mainRotTemp.y = Mathf.Cos(rotTemp);
		_MainRot_prop.vectorValue = mainRotTemp;
	}
	public float OnDrawOption(MaterialProperty[] props)
	{
		_NULL_OptionFold_prop = FindProperty("_NULL_OptionFold", props);
		_ZWrite_prop = FindProperty("_MyZWrite", props);
		_ZTest_prop = FindProperty("_MyZTest", props);
		_CullMode_prop = FindProperty("_MyCullMode", props);

		GUILayout.BeginVertical("box");
		GUILayout.BeginHorizontal();
		GUILayout.Space(14);
		_NULL_OptionFold_prop.floatValue = EditorGUILayout.Foldout(_NULL_OptionFold_prop.floatValue == 1, "Option", true) ? 1 : 0;
		GUILayout.Space(14);
		GUILayout.EndHorizontal();
		if (_NULL_OptionFold_prop.floatValue == 1)
		{
			EnumLayout(_ZWrite_prop, "ZWrite", new string[] { "Off", "On" });
			EnumLayout(_ZTest_prop, "ZTest", System.Enum.GetNames(typeof(UnityEngine.Rendering.CompareFunction)));
			EnumLayout(_CullMode_prop, "CullMode", System.Enum.GetNames(typeof(UnityEngine.Rendering.CullMode)));
			GUILayout.BeginHorizontal();
			GUILayout.Label("Render Queue", labelOp);
			GUILayout.FlexibleSpace();
			targetMat.renderQueue = EditorGUILayout.IntField(" ", targetMat.renderQueue, GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();
		return _NULL_OptionFold_prop.floatValue;
	}
	public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
	{
		EditorGUIUtility.fieldWidth = 65;

		m_MaterialEditor = materialEditor;
		targetMat = materialEditor.target as Material;

		OnDrawHelpBtn("http://wiki.jingle.cn/x/lKkFC");

		base.OnGUI(materialEditor, properties);


	}
}
