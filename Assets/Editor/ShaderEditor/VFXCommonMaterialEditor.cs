using UnityEngine;
using UnityEditor;
public class VFXCommonMaterialEditor : VFXMaterialEditor
{
	#region Material
	MaterialProperty _NULL_ShaderID_prop = null;

	MaterialProperty _NULL_MainFold_prop = null;
	MaterialProperty _MainTex_prop = null;
	MaterialProperty _Mask_prop = null;

	MaterialProperty _DissolveTex_prop = null;
	MaterialProperty _DissolveTex2_prop = null;
	MaterialProperty _SeamTex_prop = null;
	MaterialProperty _DissolvePanner_prop = null;

	MaterialProperty _ToggleAdvanced_prop = null;
	MaterialProperty _Value_prop = null;
	MaterialProperty _AnimationSettings_prop = null;
	MaterialProperty _CustomDataUV_prop = null;
	
	MaterialProperty _AnimMode_prop = null;
	MaterialProperty _NULL_SeamFold_prop = null;
	MaterialProperty _NULL_AnimFold_prop = null;

	MaterialProperty _DetailTex_prop = null;

	MaterialProperty _DistortTexRG_prop = null;
	MaterialProperty _DistortMaskRG_prop = null;
	MaterialProperty _DistortPanner_prop = null;
	MaterialProperty _DistortXY_prop = null;

	MaterialProperty _Mask2_prop = null;


	MaterialProperty _Clamp1_prop = null;
	MaterialProperty _Panner1_prop = null;
	MaterialProperty _Panner2_prop = null;
	MaterialProperty _ToggleDissolve_prop = null;

	MaterialProperty _NULL_DistortFold_prop = null;
	MaterialProperty _NULL_MaskFold_prop = null;
	
	MaterialProperty _NULL_DissolveFold_prop = null;
	MaterialProperty _DirMap_prop = null;
	MaterialProperty _EdgeColor_prop = null;
	MaterialProperty _Spread_prop = null;
	MaterialProperty _Softness_prop = null;
	MaterialProperty _Width_prop = null;

	private MaterialProperty _UseUnscaledTime_prop = null;
	
	#endregion
	void FindSimpleProperties(MaterialProperty[] props)
	{
		_NULL_MainFold_prop = FindProperty("_NULL_MainFold", props);
		_MainTex_prop = FindProperty("_MainTex", props);
		_Mask_prop = FindProperty("_Mask", props);
		_CustomDataUV_prop = FindProperty("_CustomDataUV", props);
	}
	void FindAdvancedProperties(MaterialProperty[] props)
	{
		
		_MainTex_prop = FindProperty("_MainTex", props);
		_DetailTex_prop = FindProperty("_DetailTex", props);

		_DistortTexRG_prop = FindProperty("_DistortTexRG", props);
		_DistortMaskRG_prop = FindProperty("_DistortMaskRG", props);
		_DistortPanner_prop = FindProperty("_DistortPanner", props);
		_DistortXY_prop = FindProperty("_DistortXY", props);

		_DissolveTex_prop = FindProperty("_DissolveTex", props);
		_DissolvePanner_prop = FindProperty("_DissolvePanner", props);
		_EdgeColor_prop = FindProperty("_EdgeColor", props);
		_Spread_prop = FindProperty("_Spread", props);
		_Softness_prop = FindProperty("_Softness", props);
		_Width_prop = FindProperty("_Width", props);
		
		_Mask_prop = FindProperty("_Mask", props);
		_Mask2_prop = FindProperty("_Mask2", props);

		_AnimationSettings_prop = FindProperty("_AnimationSettings", props);
		_CustomDataUV_prop = FindProperty("_CustomDataUV", props);
		
		_Panner1_prop = FindProperty("_Panner1", props);
		_Panner2_prop = FindProperty("_Panner2", props);
		_ToggleAdvanced_prop = FindProperty("_ToggleAdvanced", props);

		_AnimMode_prop = FindProperty("_AnimMode", props);
		_NULL_MainFold_prop = FindProperty("_NULL_MainFold", props);
		_NULL_DistortFold_prop = FindProperty("_NULL_DistortFold", props);
		_NULL_MaskFold_prop = FindProperty("_NULL_MaskFold", props);
		_NULL_AnimFold_prop = FindProperty("_NULL_AnimFold", props);
		_NULL_DissolveFold_prop = FindProperty("_NULL_DissolveFold", props);
		
		_UseUnscaledTime_prop = FindProperty("_UseUnscaledTime", props);
	}
	void FindVFXAdvancedProperties(MaterialProperty[] props)
	{
		_MainTex_prop = FindProperty("_MainTex", props);

		_DistortTexRG_prop = FindProperty("_DistortTexRG", props);
		_DistortPanner_prop = FindProperty("_DistortPanner", props);
		_DistortXY_prop = FindProperty("_DistortXY", props);

		_Mask_prop = FindProperty("_Mask", props);

		
		
		_AnimationSettings_prop = FindProperty("_AnimationSettings", props);
		_CustomDataUV_prop = FindProperty("_CustomDataUV", props);
		
		_Clamp1_prop = FindProperty("_Clamp1", props);
		_Panner1_prop = FindProperty("_Panner1", props);
		_ToggleAdvanced_prop = FindProperty("_ToggleAdvanced", props);

		_AnimMode_prop = FindProperty("_AnimMode", props);
		_NULL_MainFold_prop = FindProperty("_NULL_MainFold", props);
		_NULL_DistortFold_prop = FindProperty("_NULL_DistortFold", props);
		_NULL_MaskFold_prop = FindProperty("_NULL_MaskFold", props);
		_NULL_AnimFold_prop = FindProperty("_NULL_AnimFold", props);
	}
	void FindDissolveProperties(MaterialProperty[] props)
	{
		_MainTex_prop = FindProperty("_MainTex", props);
		_DissolveTex_prop = FindProperty("_DissolveTex", props);
		_DissolveTex2_prop = FindProperty("_DissolveTex2", props);
		_SeamTex_prop = FindProperty("_SeamTex", props);
		_Mask_prop = FindProperty("_Mask", props);
		
		_ToggleDissolve_prop = FindProperty("_ToggleDissolve", props);
		_Value_prop = FindProperty("_Value", props);
		_AnimationSettings_prop = FindProperty("_AnimationSettings", props);
		_CustomDataUV_prop = FindProperty("_CustomDataUV", props);
		
		_AnimMode_prop = FindProperty("_AnimMode", props);
		_NULL_MainFold_prop = FindProperty("_NULL_MainFold", props);
		_NULL_SeamFold_prop = FindProperty("_NULL_SeamFold", props);
		_NULL_AnimFold_prop = FindProperty("_NULL_AnimFold", props);
	}
	
	void FindDissolveSoftProperties(MaterialProperty[] props)
	{
		_MainTex_prop = FindProperty("_MainTex", props);
		_DissolveTex_prop = FindProperty("_DissolveTex", props);
		_DirMap_prop = FindProperty("_DirMap", props);
		_Mask_prop = FindProperty("_Mask", props);
		_AnimationSettings_prop = FindProperty("_AnimationSettings", props);
		_CustomDataUV_prop = FindProperty("_CustomDataUV", props);
		_EdgeColor_prop = FindProperty("_EdgeColor", props);
		_Spread_prop = FindProperty("_Spread", props);
		_Softness_prop = FindProperty("_Softness", props);
		_Width_prop = FindProperty("_Width", props);
		_NULL_MainFold_prop = FindProperty("_NULL_MainFold", props);
		_NULL_DissolveFold_prop = FindProperty("_NULL_DissolveFold", props);
		_NULL_AnimFold_prop = FindProperty("_NULL_AnimFold", props);
		_AnimMode_prop = FindProperty("_AnimMode", props);
	}
	
	public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
	{
		EditorGUIUtility.fieldWidth = 65;

		m_MaterialEditor = materialEditor;
		targetMat = materialEditor.target as Material;

		_NULL_ShaderID_prop = FindProperty("_NULL_ShaderID", properties);
		EditorGUI.BeginChangeCheck();
		int ShaderID = (int)_NULL_ShaderID_prop.floatValue;
		if(ShaderID<4)_NULL_ShaderID_prop.floatValue = GUILayout.Toolbar(ShaderID, new[] { "Simple", "Advanced" , "Dissolve", "Soft Dissolve"});
		bool shaderIDChanged = EditorGUI.EndChangeCheck();

		OnDrawHelpBtn("http://wiki.jingle.cn/x/lKkFC");
		OnDrawBlendMode(properties);
		OnDrawColorCorrection(properties);
		switch (ShaderID)
		{
			case (0):
				SimpleMaterialEditor(properties);
				break;
			case (1):
				AdvancedMaterialEditor(properties);
				break;
			case (2):
				DissolveMaterialEditor(properties);
				break;
			// case (3):
			// 	VFXAdvancedMaterialEditor(properties);
			// 	break;
			case (3) :
				DissolveSoftMaterialEditor(properties);
				break;
		}

		if (shaderIDChanged)
		{
			int renderQ = targetMat.renderQueue;
			switch ((int)_NULL_ShaderID_prop.floatValue)
			{
				case (0):
					targetMat.shader = Shader.Find("FX/VFX_Simple");
					break;
				case (1):
					targetMat.shader = Shader.Find("FX/VFX_Advanced");
					break;
				case (2):
					targetMat.shader = Shader.Find("FX/VFX_Dissolve");
					break;
				case (3):
					targetMat.shader = Shader.Find("FX/VFX_Dissolve_Soft");
					break;
			}
			targetMat.renderQueue = renderQ;
		}
		OnDrawOption(properties);
	}

	void SimpleMaterialEditor(MaterialProperty[] properties)
	{
		FindSimpleProperties(properties);

		if (_Mask_prop.textureValue == null)
		{
			targetMat.EnableKeyword("DISABLEMASK");
		}
		else
		{
			targetMat.DisableKeyword("DISABLEMASK");
		}
		
		MaterialProperty _SoftParticlesEnabled = FindProperty("_SoftParticlesEnabled", properties);
		MaterialProperty _InvFade = FindProperty("_InvFade", properties);
		MaterialProperty _SoftParticleFadeParams = FindProperty("_SoftParticleFadeParams", properties);

		GUILayout.Space(8);
		GUILayout.BeginVertical("box");
		GUILayout.BeginHorizontal();
		GUILayout.Label("Soft Particles", EditorStyles.boldLabel);
		GUILayout.FlexibleSpace();
		bool softParticlesEnabled = GUILayout.Toggle(_SoftParticlesEnabled.floatValue > 0.5f, "Enable");
		_SoftParticlesEnabled.floatValue = softParticlesEnabled ? 1 : 0;
		GUILayout.EndHorizontal();

		if (softParticlesEnabled)
		{
			targetMat.EnableKeyword("_SOFTPARTICLES_ON");
			m_MaterialEditor.ShaderProperty(_InvFade, "Soft Particles Power");
			m_MaterialEditor.ShaderProperty(_SoftParticleFadeParams, "Fade Params (Start, End, Scale, Unused)");
		}
		else
		{
			targetMat.DisableKeyword("_SOFTPARTICLES_ON");
		}
		GUILayout.EndVertical();

		//Main
		if (FoldOutLayout(_NULL_MainFold_prop, "Main"))
		{
			bool customDataUV = GUILayout.Toggle(_CustomDataUV_prop.floatValue > 0.5, "Custom Data UV Animation");	
			_CustomDataUV_prop.floatValue = customDataUV ? 1 : 0;
			m_MaterialEditor.TextureProperty(_MainTex_prop, "MainTex");
			OnDrawMainRot(properties);

			if (_Mask_prop.textureValue != null)
			{
				m_MaterialEditor.TextureProperty(_Mask_prop, "Mask(R)");
			}
			else
			{
				m_MaterialEditor.TexturePropertySingleLine(new GUIContent("Mask(R)"), _Mask_prop);
			}
		}
	}
	void VFXAdvancedMaterialEditor(MaterialProperty[] properties)
	{
		FindVFXAdvancedProperties(properties);

		if (_DistortTexRG_prop.textureValue == null)
		{
			targetMat.DisableKeyword("DISTORT");
		}
		else
		{
			targetMat.EnableKeyword("DISTORT");
		}


		//Main
		if (FoldOutLayout(_NULL_MainFold_prop, "Main"))
		{
			bool customDataUV = GUILayout.Toggle(_CustomDataUV_prop.floatValue > 0.5, "Custom Data UV Animation");	
			_CustomDataUV_prop.floatValue = customDataUV ? 1 : 0;
			m_MaterialEditor.TextureProperty(_MainTex_prop, "MainTex");
			ToggleUV(ref _Clamp1_prop, "ClampMainUV", 1);
			GUILayout.BeginHorizontal();
			GUILayout.Label("MainUVType", labelOp);
			GUILayout.FlexibleSpace();
			Vector4 fixValueTemp = _FixValue_prop.vectorValue;
			EditorGUI.BeginChangeCheck();
			fixValueTemp.z = EditorGUILayout.Popup("", (int)fixValueTemp.z, new[] { "Default", "use2U" }, labelOp);
			if (EditorGUI.EndChangeCheck())
			{
				_FixValue_prop.vectorValue = fixValueTemp;
			}
			GUILayout.EndHorizontal();
			OnDrawMainRot(properties);

			Vector2Layout(_Panner1_prop, "MainPanner", "NULL");
		}



		//Distort
		if (FoldOutLayout(_NULL_DistortFold_prop, "Distort"))
		{
			if (_DistortTexRG_prop.textureValue != null)
			{
				m_MaterialEditor.TextureProperty(_DistortTexRG_prop, "DistortTex(RG)");
				VectorLayout(_DistortXY_prop, "DistortX", "NULL", "NULL", "NULL");
				VectorLayout(_DistortXY_prop, "NULL", "DistortY", "NULL", "NULL");
				Vector2Layout(_DistortPanner_prop, "DistortPanner", "NULL");
			}
			else
			{
				m_MaterialEditor.TexturePropertySingleLine(new GUIContent("DistortTex(RG)"), _DistortTexRG_prop);
			}
		}



		//Mask
		if (FoldOutLayout(_NULL_MaskFold_prop, "Mask"))
		{
			m_MaterialEditor.TextureProperty(_Mask_prop, "Mask(R)");
			ToggleUV(ref _Clamp1_prop, "ClampMaskUV", 3);
			Toggle4Layout(_ToggleAdvanced_prop, "MaskDistort", 3);
			Vector2Layout(_Panner1_prop, "NULL", "MaskPanner");
		}


		//AnimationSettings
		GUILayout.BeginVertical("box");
		GUILayout.BeginHorizontal();
		GUILayout.Space(14);
		_NULL_AnimFold_prop.floatValue = EditorGUILayout.Foldout(_NULL_AnimFold_prop.floatValue == 1, "AnimationSettings", true) ? 1 : 0;
		GUILayout.Space(14);
		GUILayout.EndHorizontal();
		if (_NULL_AnimFold_prop.floatValue == 1)
		{
			Toggle4Layout(_AnimationSettings_prop, "Panner", 1);

			GUILayout.BeginHorizontal();
			Vector4 fixValueTemp = _FixValue_prop.vectorValue;
			fixValueTemp.w = GUILayout.Toggle(fixValueTemp.w > 0.5, "") ? 1 : 0;
			_FixValue_prop.vectorValue = fixValueTemp;
			Vector4 animTemp = _AnimationSettings_prop.vectorValue;
			EditorGUI.BeginChangeCheck();
			if (_FixValue_prop.vectorValue.w > 0.5f)
			{
				animTemp.w = FloatLayout(animTemp.w, "AnimSpeed");
			}
			else
			{
				animTemp.w = RangeLayout(animTemp.w, "AnimCtl", 0.0f, 1.0f);
			}
			if (EditorGUI.EndChangeCheck())
			{
				_AnimationSettings_prop.vectorValue = animTemp;
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("AnimMode");

			GUIContent noneText = new GUIContent("None");
			GUIContent particleText = new GUIContent("Particle", "使用Custom1.x控制动画");
			GUIContent uiText = new GUIContent("UI", "使用UI的Alpha控制动画");
			EditorGUI.BeginChangeCheck();
			int _AnimMode = (int)_AnimMode_prop.floatValue;
			_AnimMode_prop.floatValue = _AnimMode = GUILayout.Toolbar(_AnimMode, new[] { noneText, particleText, uiText }, GUILayout.MaxWidth(240));
			GUILayout.EndHorizontal();
			if (EditorGUI.EndChangeCheck())
			{
				Vector4 temp = _AnimationSettings_prop.vectorValue;
				switch (_AnimMode)
				{
					case 0:
						temp.y = 0;
						temp.z = 0;
						break;
					case 1:
						temp.y = 1;
						temp.z = 0;
						temp.w = 1;
						break;
					case 2:
						temp.y = 0;
						temp.z = 1;
						temp.w = 1;
						break;
				}
				_AnimationSettings_prop.vectorValue = temp;
			}
			GUILayout.Space(4);
		}
		GUILayout.EndVertical();
	}
	void ToggleUV(ref MaterialProperty mp, string name, int index)
	{
		Vector4 temp = mp.vectorValue;
		GUILayout.BeginHorizontal();
		GUILayout.Label(name, labelOp);
		GUILayout.FlexibleSpace();
		EditorGUI.BeginChangeCheck();
		switch (index)
		{
			case (1):
				temp.x = GUILayout.Toggle(temp.x != 0, "U") ? 1 : 0;
				temp.y = GUILayout.Toggle(temp.y != 0, "V") ? 1 : 0;
				break;
			case (2):
				temp.y = GUILayout.Toggle(temp.y != 0, "U") ? 1 : 0;
				temp.z = GUILayout.Toggle(temp.z != 0, "V") ? 1 : 0;
				break;
			case (3):
				temp.z = GUILayout.Toggle(temp.z != 0, "U") ? 1 : 0;
				temp.w = GUILayout.Toggle(temp.w != 0, "V") ? 1 : 0;
				break;
			case (4):
				temp.w = GUILayout.Toggle(temp.w != 0, "U") ? 1 : 0;
				temp.x = GUILayout.Toggle(temp.x != 0, "V") ? 1 : 0;
				break;
		}
		GUILayout.EndHorizontal();
		if (EditorGUI.EndChangeCheck())
		{
			mp.vectorValue = temp;
		}
	}
	void AdvancedMaterialEditor(MaterialProperty[] properties)
	{
		FindAdvancedProperties(properties);
		if (_DetailTex_prop.textureValue != null || _Mask2_prop.textureValue != null)
		{
			targetMat.EnableKeyword("USE2TEX");
		}
		else
		{
			targetMat.DisableKeyword("USE2TEX");
		}
		
		if (_DistortMaskRG_prop.textureValue != null)
		{
			targetMat.EnableKeyword("USEDISTORTMASK");
		}
		else
		{
			targetMat.DisableKeyword("USEDISTORTMASK");
		}

		if (_DistortTexRG_prop.textureValue == null)
		{
			targetMat.DisableKeyword("DISTORT");
		}
		else
		{
			targetMat.EnableKeyword("DISTORT");
		}

		if (_DissolveTex_prop.textureValue == null)
		{
			targetMat.DisableKeyword("DISSOLVE");
		}
		else
		{
			targetMat.EnableKeyword("DISSOLVE");
		}
		if (_DetailTex_prop.textureValue != null || _Mask2_prop.textureValue != null)
		{
			targetMat.EnableKeyword("USE2TEX");
		}
		else
		{
			targetMat.DisableKeyword("USE2TEX");
		}

		//soft particles
		MaterialProperty _SoftParticlesEnabled = FindProperty("_SoftParticlesEnabled", properties);
		MaterialProperty _InvFade = FindProperty("_InvFade", properties);
		MaterialProperty _SoftParticleFadeParams = FindProperty("_SoftParticleFadeParams", properties);
		
		GUILayout.Space(8);
		GUILayout.BeginVertical("box");
		GUILayout.BeginHorizontal();
		GUILayout.Label("Soft Particles", EditorStyles.boldLabel);
		GUILayout.FlexibleSpace();
		bool softParticlesEnabled = GUILayout.Toggle(_SoftParticlesEnabled.floatValue > 0.5f, "Enable");
		_SoftParticlesEnabled.floatValue = softParticlesEnabled ? 1 : 0;
		GUILayout.EndHorizontal();
		
		if (softParticlesEnabled)
		{
			targetMat.EnableKeyword("_SOFTPARTICLES_ON");
			m_MaterialEditor.ShaderProperty(_InvFade, "Soft Particles Power");
			m_MaterialEditor.ShaderProperty(_SoftParticleFadeParams, "Fade Params (Start, End, Scale, Unused)");
		}
		else
		{
			targetMat.DisableKeyword("_SOFTPARTICLES_ON");
		}
		GUILayout.EndVertical();
		
		//unscaled time
		GUILayout.Space(8);
		GUILayout.BeginVertical("box");
		GUILayout.BeginHorizontal();
		GUILayout.Label("Unscaled Time", EditorStyles.boldLabel);
		GUILayout.FlexibleSpace();
		bool useUnscaledTime = GUILayout.Toggle(_UseUnscaledTime_prop.floatValue > 0.5f, "Use Unscaled Time");
		_UseUnscaledTime_prop.floatValue = useUnscaledTime ? 1 : 0;
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
		
		//Main
		if (FoldOutLayout(_NULL_MainFold_prop, "Main"))
		{
			bool customDataUV = GUILayout.Toggle(_CustomDataUV_prop.floatValue > 0.5, "Custom Data UV Animation");	
			_CustomDataUV_prop.floatValue = customDataUV ? 1 : 0;
			m_MaterialEditor.TextureProperty(_MainTex_prop, "MainTex");
			GUILayout.BeginHorizontal();
			GUILayout.Label("MainUVType", labelOp);
			GUILayout.FlexibleSpace();
			Vector4 fixValueTemp = _FixValue_prop.vectorValue;
			EditorGUI.BeginChangeCheck();
			fixValueTemp.z = EditorGUILayout.Popup("", (int)fixValueTemp.z, new[] { "Default", "use2U" }, labelOp);
			if (EditorGUI.EndChangeCheck())
			{
				_FixValue_prop.vectorValue = fixValueTemp;
			}
			GUILayout.EndHorizontal();
			OnDrawMainRot(properties);

			Vector2Layout(_Panner1_prop, "MainPanner", "NULL");

			GUILayout.Space(8);
			 if (_DetailTex_prop.textureValue != null)
			 {
			 	m_MaterialEditor.TextureProperty(_DetailTex_prop, "DetailTex");
			 	Vector2Layout(_Panner2_prop, "DetailPanner", "NULL");
			 }
			 else
			 {
				m_MaterialEditor.TexturePropertySingleLine(new GUIContent("DetailTex"), _DetailTex_prop);
			 }
		}



		//Distort
		if (FoldOutLayout(_NULL_DistortFold_prop, "Distort"))
		{
			if (_DistortTexRG_prop.textureValue != null)
			{
				m_MaterialEditor.TextureProperty(_DistortTexRG_prop, "DistortTex(RG)");
				Vector2Layout(_DistortXY_prop, "DistortXY(Main)", "DistortXY(Dissolve)");
				Vector2Layout(_DistortPanner_prop, "DistortPanner", "NULL");
				GUILayout.Space(4);
				if (_DistortMaskRG_prop.textureValue != null)
				{
					m_MaterialEditor.TextureProperty(_DistortMaskRG_prop, "DistortMask(RG)");
					Toggle4Layout(_ToggleAdvanced_prop, "InvertDistortMask", 2);
					Vector2Layout(_DistortPanner_prop, "NULL", "DistortMaskPanner");
				}
				else
				{
					m_MaterialEditor.TexturePropertySingleLine(new GUIContent("DistortMask(RG)"), _DistortMaskRG_prop);
				}
			}
			else
			{
				m_MaterialEditor.TexturePropertySingleLine(new GUIContent("DistortTex(RG)"), _DistortTexRG_prop);
			}
		}



		//Mask
		if (FoldOutLayout(_NULL_MaskFold_prop, "Mask"))
		{
			m_MaterialEditor.TextureProperty(_Mask_prop, "Mask(R)");
			Toggle4Layout(_ToggleAdvanced_prop, "MaskDistort", 3);
			Vector2Layout(_Panner1_prop, "NULL", "MaskPanner");

			GUILayout.Space(8);
			if (_Mask2_prop.textureValue != null)
			{
				m_MaterialEditor.TextureProperty(_Mask2_prop, "Mask2(R)");
				Toggle4Layout(_ToggleAdvanced_prop, "MaskDistort2", 4);
				Vector2Layout(_Panner2_prop, "NULL", "Mask2Panner");
			}
			else
			{
				m_MaterialEditor.TexturePropertySingleLine(new GUIContent("Mask2(R)"), _Mask2_prop);
			}
		}

		if (FoldOutLayout(_NULL_DissolveFold_prop,"Dissolve"))
		{
			m_MaterialEditor.TextureProperty(_DissolveTex_prop, "Dissolve");
			if (_DissolveTex_prop.textureValue != null)
			{
				Vector2Layout(_DissolvePanner_prop, "DissolvePanner", "NULL");
				m_MaterialEditor.ColorProperty(_EdgeColor_prop, "Edge Color");
				m_MaterialEditor.ShaderProperty(_Spread_prop, "Edge Spread");
				m_MaterialEditor.ShaderProperty(_Width_prop, "Edge Width");
				m_MaterialEditor.ShaderProperty(_Softness_prop, "Edge Softness");
			}
		}

		//AnimationSettings
		GUILayout.BeginVertical("box");
		GUILayout.BeginHorizontal();
		GUILayout.Space(14);
		_NULL_AnimFold_prop.floatValue = EditorGUILayout.Foldout(_NULL_AnimFold_prop.floatValue == 1, "AnimationSettings", true) ? 1 : 0;
		GUILayout.Space(14);
		GUILayout.EndHorizontal();
		if (_NULL_AnimFold_prop.floatValue == 1)
		{
			Toggle4Layout(_AnimationSettings_prop, "Panner", 1);

			GUILayout.BeginHorizontal();
			Vector4 fixValueTemp = _FixValue_prop.vectorValue;
			fixValueTemp.w = GUILayout.Toggle(fixValueTemp.w > 0.5, "") ? 1 : 0;
			_FixValue_prop.vectorValue = fixValueTemp;
			Vector4 animTemp = _AnimationSettings_prop.vectorValue;
			EditorGUI.BeginChangeCheck();
			if (_FixValue_prop.vectorValue.w > 0.5f)
			{
				animTemp.w = FloatLayout(animTemp.w, "AnimSpeed");
			}
			else
			{
				animTemp.w = RangeLayout(animTemp.w, "AnimCtl", 0.0f, 1.0f);
			}
			if (EditorGUI.EndChangeCheck())
			{
				_AnimationSettings_prop.vectorValue = animTemp;
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("AnimMode");

			GUIContent noneText = new GUIContent("None");
			GUIContent particleText = new GUIContent("Particle", "使用Custom1.x控制动画");
			GUIContent uiText = new GUIContent("UI", "使用UI的Alpha控制动画");
			EditorGUI.BeginChangeCheck();
			int _AnimMode = (int)_AnimMode_prop.floatValue;
			_AnimMode_prop.floatValue = _AnimMode = GUILayout.Toolbar(_AnimMode, new[] { noneText, particleText, uiText }, GUILayout.MaxWidth(240));
			GUILayout.EndHorizontal();
			if (EditorGUI.EndChangeCheck())
			{
				Vector4 temp = _AnimationSettings_prop.vectorValue;
				switch (_AnimMode)
				{
					case 0:
						temp.y = 0;
						temp.z = 0;
						break;
					case 1:
						temp.y = 1;
						temp.z = 0;
						temp.w = 1;
						break;
					case 2:
						temp.y = 0;
						temp.z = 1;
						temp.w = 1;
						break;
				}
				_AnimationSettings_prop.vectorValue = temp;
			}
			GUILayout.Space(4);
		}
		GUILayout.EndVertical();
	}
	void DissolveMaterialEditor(MaterialProperty[] properties)
	{
		FindDissolveProperties(properties);

		if (_DissolveTex2_prop.textureValue != null)
		{
			targetMat.EnableKeyword("USEDISSOLVETEX2");
		}
		else
		{
			targetMat.DisableKeyword("USEDISSOLVETEX2");
		}
		if (_SeamTex_prop.textureValue != null)
		{
			targetMat.EnableKeyword("USESEAMTEX");
		}
		else
		{
			targetMat.DisableKeyword("USESEAMTEX");
		}
		
		MaterialProperty _SoftParticlesEnabled = FindProperty("_SoftParticlesEnabled", properties);
		MaterialProperty _InvFade = FindProperty("_InvFade", properties);
		MaterialProperty _SoftParticleFadeParams = FindProperty("_SoftParticleFadeParams", properties);

		GUILayout.Space(8);
		GUILayout.BeginVertical("box");
		GUILayout.BeginHorizontal();
		GUILayout.Label("Soft Particles", EditorStyles.boldLabel);
		GUILayout.FlexibleSpace();
		bool softParticlesEnabled = GUILayout.Toggle(_SoftParticlesEnabled.floatValue > 0.5f, "Enable");
		_SoftParticlesEnabled.floatValue = softParticlesEnabled ? 1 : 0;
		GUILayout.EndHorizontal();

		if (softParticlesEnabled)
		{
			targetMat.EnableKeyword("_SOFTPARTICLES_ON");
			m_MaterialEditor.ShaderProperty(_InvFade, "Soft Particles Power");
			m_MaterialEditor.ShaderProperty(_SoftParticleFadeParams, "Fade Params (Start, End, Scale, Unused)");
		}
		else
		{
			targetMat.DisableKeyword("_SOFTPARTICLES_ON");
		}
		GUILayout.EndVertical();

		//Main
		if (FoldOutLayout(_NULL_MainFold_prop, "Main"))
		{
			bool customDataUV = GUILayout.Toggle(_CustomDataUV_prop.floatValue > 0.5, "Custom Data UV Animation");	
			_CustomDataUV_prop.floatValue = customDataUV ? 1 : 0;
			m_MaterialEditor.TextureProperty(_MainTex_prop, "MainTex");
			EnumLayout(_FixValue_prop, "MainUVType", new[] { "Default", "use2U" }, 3);
			OnDrawMainRot(properties);

			m_MaterialEditor.TextureProperty(_DissolveTex_prop, "DissolveTex(R)");
			Toggle4Layout(_ToggleDissolve_prop, "InvertDissolveTex", 1);
			GUILayout.Space(8);
			if (_DissolveTex2_prop.textureValue != null)
			{
				m_MaterialEditor.TextureProperty(_DissolveTex2_prop, "DissolveTex2((R+G+B)/3)");
				RangeLayout(_Value_prop, "LerpDissolve", 0, 1, 1);
			}
			else
			{
				m_MaterialEditor.TexturePropertySingleLine(new GUIContent("DissolveTex2((R+G+B)/3)"), _DissolveTex2_prop);
			}
			if (_Mask_prop.textureValue != null)
			{
				m_MaterialEditor.TextureProperty(_Mask_prop, "MaskTex(R)");
			}
			else
			{
				m_MaterialEditor.TexturePropertySingleLine(new GUIContent("MaskTex(R)"), _Mask_prop);
			}
		}

		//Seam
		if (FoldOutLayout(_NULL_SeamFold_prop, "Seam"))
		{
			if (_SeamTex_prop.textureValue != null)
			{
				m_MaterialEditor.TextureProperty(_SeamTex_prop, "SeamTex");
				VectorLayout(_Value_prop, "NULL", "SeamSize", "NULL", "NULL");
				Toggle4Layout(_ToggleDissolve_prop, "SeamClampLeft", 3);
				Toggle4Layout(_ToggleDissolve_prop, "SeamClampRight", 4);
				VectorLayout(_Value_prop, "NULL", "NULL", "SeamBrightness", "SeamContrast");

				Vector4 toggleTemp = _ToggleDissolve_prop.vectorValue;
				GUILayout.BeginHorizontal();
				GUILayout.Label("SeamBlendMode");
				toggleTemp.y = GUILayout.Toolbar((int)toggleTemp.y, new[] { "Mul", "Add" }, GUILayout.Width(80));
				GUILayout.EndHorizontal();
				_ToggleDissolve_prop.vectorValue = toggleTemp;
			}
			else
			{
				m_MaterialEditor.TexturePropertySingleLine(new GUIContent("SeamTex"), _SeamTex_prop);
				VectorLayout(_Value_prop, "NULL", "SeamSize", "NULL", "NULL");
				Toggle4Layout(_ToggleDissolve_prop, "SeamClampLeft", 3);
				Toggle4Layout(_ToggleDissolve_prop, "SeamClampRight", 4);
			}
		}

		//AnimationSettings
		GUILayout.BeginVertical("box");
		GUILayout.BeginHorizontal();
		GUILayout.Space(14);
		_NULL_AnimFold_prop.floatValue = EditorGUILayout.Foldout(_NULL_AnimFold_prop.floatValue == 1, "AnimationSettings", true) ? 1 : 0;
		GUILayout.Space(14);
		GUILayout.EndHorizontal();

		if (_NULL_AnimFold_prop.floatValue == 1)
		{
			GUILayout.BeginHorizontal();
			Vector4 fixValueTemp = _FixValue_prop.vectorValue;
			fixValueTemp.w = GUILayout.Toggle(fixValueTemp.w > 0.5, "") ? 1 : 0;
			_FixValue_prop.vectorValue = fixValueTemp;
			Vector4 animTemp = _AnimationSettings_prop.vectorValue;
			EditorGUI.BeginChangeCheck();
			if (_FixValue_prop.vectorValue.w > 0.5f)
			{
				animTemp.w = FloatLayout(animTemp.w, "AnimSpeed");
			}
			else
			{
				animTemp.w = RangeLayout(animTemp.w, "AnimCtl", 0.0f, 1.0f);
			}
			if (EditorGUI.EndChangeCheck())
			{
				_AnimationSettings_prop.vectorValue = animTemp;
			}
			GUILayout.EndHorizontal(); GUILayout.BeginHorizontal();
			GUILayout.Label("AnimMode");

			GUIContent noneText = new GUIContent("None");
			GUIContent particleText = new GUIContent("Particle", "使用Custom1.x控制动画");
			GUIContent uiText = new GUIContent("UI", "使用UI的Alpha控制动画");
			EditorGUI.BeginChangeCheck();
			int _AnimMode = (int)_AnimMode_prop.floatValue;
			_AnimMode_prop.floatValue = _AnimMode = GUILayout.Toolbar(_AnimMode, new[] { noneText, particleText, uiText }, GUILayout.MaxWidth(240));
			GUILayout.EndHorizontal();
			if (EditorGUI.EndChangeCheck())
			{
				Vector4 temp = _AnimationSettings_prop.vectorValue;
				switch (_AnimMode)
				{
					case 0:
						temp.y = 0;
						temp.z = 0;
						break;
					case 1:
						temp.y = 1;
						temp.z = 0;
						temp.w = 1;
						break;
					case 2:
						temp.y = 0;
						temp.z = 1;
						temp.w = 1;
						break;
				}
				_AnimationSettings_prop.vectorValue = temp;
			}
			GUILayout.Space(4);
		}
		GUILayout.EndVertical();
	}
	
	void DissolveSoftMaterialEditor(MaterialProperty[] properties)
	{
		FindDissolveSoftProperties(properties);
		
		MaterialProperty _SoftParticlesEnabled = FindProperty("_SoftParticlesEnabled", properties);
		MaterialProperty _InvFade = FindProperty("_InvFade", properties);
		MaterialProperty _SoftParticleFadeParams = FindProperty("_SoftParticleFadeParams", properties);

		GUILayout.Space(8);
		GUILayout.BeginVertical("box");
		GUILayout.BeginHorizontal();
		GUILayout.Label("Soft Particles", EditorStyles.boldLabel);
		GUILayout.FlexibleSpace();
		bool softParticlesEnabled = GUILayout.Toggle(_SoftParticlesEnabled.floatValue > 0.5f, "Enable");
		_SoftParticlesEnabled.floatValue = softParticlesEnabled ? 1 : 0;
		GUILayout.EndHorizontal();

		if (softParticlesEnabled)
		{
			targetMat.EnableKeyword("_SOFTPARTICLES_ON");
			m_MaterialEditor.ShaderProperty(_InvFade, "Soft Particles Power");
			m_MaterialEditor.ShaderProperty(_SoftParticleFadeParams, "Fade Params (Start, End, Scale, Unused)");
		}
		else
		{
			targetMat.DisableKeyword("_SOFTPARTICLES_ON");
		}
		GUILayout.EndVertical();
		
		//Main
		if (FoldOutLayout(_NULL_MainFold_prop, "Main"))
		{
			bool customDataUV = GUILayout.Toggle(_CustomDataUV_prop.floatValue > 0.5, "Custom Data UV Animation");	
			_CustomDataUV_prop.floatValue = customDataUV ? 1 : 0;
			
			m_MaterialEditor.TextureProperty(_MainTex_prop, "MainTex");
			m_MaterialEditor.TextureProperty(_DissolveTex_prop, "DissolveTex(R)");
			m_MaterialEditor.TextureProperty(_DirMap_prop, "DirMap(R)");
			m_MaterialEditor.TextureProperty(_Mask_prop, "Mask(R)");
		}
		//Dissolve
		if (FoldOutLayout(_NULL_DissolveFold_prop, "Dissolve"))
		{
			//AnimationSettings
			GUILayout.BeginVertical("box");
			GUILayout.BeginHorizontal();
			GUILayout.Space(14);
			_NULL_AnimFold_prop.floatValue = EditorGUILayout.Foldout(_NULL_AnimFold_prop.floatValue == 1, "AnimationSettings", true) ? 1 : 0;
			GUILayout.Space(14);
			GUILayout.EndHorizontal();
			if (_NULL_AnimFold_prop.floatValue == 1)
			{
				GUILayout.BeginHorizontal();
				Vector4 fixValueTemp = _FixValue_prop.vectorValue;
				fixValueTemp.w = GUILayout.Toggle(fixValueTemp.w > 0.5, "") ? 1 : 0;
				_FixValue_prop.vectorValue = fixValueTemp;
				Vector4 animTemp = _AnimationSettings_prop.vectorValue;
				EditorGUI.BeginChangeCheck();
				if (_FixValue_prop.vectorValue.w > 0.5f)
				{
					animTemp.w = FloatLayout(animTemp.w, "AnimSpeed");
				}
				else
				{
					animTemp.w = RangeLayout(animTemp.w, "AnimCtl", 0.0f, 1.0f);
				}

				if (EditorGUI.EndChangeCheck())
				{
					_AnimationSettings_prop.vectorValue = animTemp;
				}

				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				GUILayout.Label("AnimMode");

				GUIContent noneText = new GUIContent("None");
				GUIContent particleText = new GUIContent("Particle", "使用Custom1.x控制动画");
				GUIContent uiText = new GUIContent("UI", "使用UI的Alpha控制动画");
				EditorGUI.BeginChangeCheck();
				int _AnimMode = (int)_AnimMode_prop.floatValue;
				_AnimMode_prop.floatValue = _AnimMode = GUILayout.Toolbar(_AnimMode,
					new[] { noneText, particleText, uiText }, GUILayout.MaxWidth(240));
				GUILayout.EndHorizontal();
				if (EditorGUI.EndChangeCheck())
				{
					Vector4 temp = _AnimationSettings_prop.vectorValue;
					switch (_AnimMode)
					{
						case 0:
							temp.y = 0;
							temp.z = 0;
							break;
						case 1:
							temp.y = 1;
							temp.z = 0;
							temp.w = 1;
							break;
						case 2:
							temp.y = 0;
							temp.z = 1;
							temp.w = 1;
							break;
					}

					_AnimationSettings_prop.vectorValue = temp;
				}

				GUILayout.Space(4);
			}
			GUILayout.EndHorizontal();
			m_MaterialEditor.ColorProperty(_EdgeColor_prop, "Edge Color");
			m_MaterialEditor.ShaderProperty(_Spread_prop, "Edge Spread");
			m_MaterialEditor.ShaderProperty(_Width_prop, "Edge Width");
			m_MaterialEditor.ShaderProperty(_Softness_prop, "Edge Softness");
		}
	}
}
