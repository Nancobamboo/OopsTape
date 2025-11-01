using UnityEngine;
using UnityEditor;

namespace DayDream
{
    
    public class BaseSpineShaderGUI : ShaderGUI
    {

        private MaterialProperty _Cutoff;
        private MaterialProperty _MainTex;
        private MaterialProperty _StraightAlphaInput;

        private MaterialProperty _GlowOn;
        private MaterialProperty _GlowColor;
        private MaterialProperty _GlowIntensity;
        private MaterialProperty _GlowMask;
        private MaterialProperty _GlowThreshold;

        private MaterialProperty _DissolveOn;
        private MaterialProperty _DissolveTex;
        private MaterialProperty _DissolveAmount;
        private MaterialProperty _DissolveGlowColor;
        private MaterialProperty _GlowWidth;

        private MaterialProperty _FaultOn;
        private MaterialProperty _FaultAmount;
        private MaterialProperty _FaultAlpha;

        private MaterialProperty _AddTextureOn;
        private MaterialProperty _AddGlowTex;
        private MaterialProperty _AddColor;
        private MaterialProperty _DistortTex;
        private MaterialProperty _DistortAmount;
        private MaterialProperty _DistortTexXSpeed;
        private MaterialProperty _DistortTexYSpeed;
        private MaterialProperty _AddTextureBlendMode;
        private MaterialProperty _UseGlowAlphaWeight;

        private MaterialProperty _StencilRef;
        private MaterialProperty _StencilComp;

        private const string KEY_STRAIGHT_ALPHA = "_STRAIGHT_ALPHA_INPUT";
        private const string KEY_GLOW = "_GLOW_ON";
        private const string KEY_DISSOLVE = "_DISSOLVE_ON";
        private const string KEY_FAULT = "_FAULT_ON";
        private const string KEY_ADDTEX = "_ADDTEXTURE_ON";

		public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            FindProperties(props);

			EditorGUILayout.LabelField("Base", EditorStyles.boldLabel);
			materialEditor.ShaderProperty(_Cutoff, _Cutoff.displayName);
			materialEditor.TexturePropertySingleLine(new GUIContent(_MainTex.displayName), _MainTex);
			materialEditor.TextureScaleOffsetProperty(_MainTex);
			EditorGUI.BeginChangeCheck();
			materialEditor.ShaderProperty(_StraightAlphaInput, _StraightAlphaInput.displayName);
			if (EditorGUI.EndChangeCheck())
			{
				foreach (var obj in materialEditor.targets)
				{
					var mat = obj as Material;
					if (mat == null) continue;
					SetKeyword(mat, KEY_STRAIGHT_ALPHA, IsToggleOn(_StraightAlphaInput));
				}
			}
			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Glow", EditorStyles.boldLabel);
			EditorGUI.BeginChangeCheck();
			materialEditor.ShaderProperty(_GlowOn, _GlowOn.displayName);
			if (EditorGUI.EndChangeCheck())
			{
				foreach (var obj in materialEditor.targets)
				{
					var mat = obj as Material;
					if (mat == null) continue;
					SetKeyword(mat, KEY_GLOW, IsToggleOn(_GlowOn));
				}
			}
			if (IsToggleOn(_GlowOn))
			{
				materialEditor.ColorProperty(_GlowColor, _GlowColor.displayName);
				materialEditor.ShaderProperty(_GlowIntensity, _GlowIntensity.displayName);
				materialEditor.TexturePropertySingleLine(new GUIContent(_GlowMask.displayName), _GlowMask);
				materialEditor.TextureScaleOffsetProperty(_GlowMask);
				materialEditor.ShaderProperty(_GlowThreshold, _GlowThreshold.displayName);
			}
			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Dissolve", EditorStyles.boldLabel);
			EditorGUI.BeginChangeCheck();
			materialEditor.ShaderProperty(_DissolveOn, _DissolveOn.displayName);
			if (EditorGUI.EndChangeCheck())
			{
				foreach (var obj in materialEditor.targets)
				{
					var mat = obj as Material;
					if (mat == null) continue;
					SetKeyword(mat, KEY_DISSOLVE, IsToggleOn(_DissolveOn));
				}
			}
			if (IsToggleOn(_DissolveOn))
			{
				materialEditor.TexturePropertySingleLine(new GUIContent(_DissolveTex.displayName), _DissolveTex);
				materialEditor.TextureScaleOffsetProperty(_DissolveTex);
				materialEditor.ShaderProperty(_DissolveAmount, _DissolveAmount.displayName);
				materialEditor.ColorProperty(_DissolveGlowColor, _DissolveGlowColor.displayName);
				materialEditor.ShaderProperty(_GlowWidth, _GlowWidth.displayName);
			}
			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Fault", EditorStyles.boldLabel);
			EditorGUI.BeginChangeCheck();
			materialEditor.ShaderProperty(_FaultOn, _FaultOn.displayName);
			if (EditorGUI.EndChangeCheck())
			{
				foreach (var obj in materialEditor.targets)
				{
					var mat = obj as Material;
					if (mat == null) continue;
					SetKeyword(mat, KEY_FAULT, IsToggleOn(_FaultOn));
				}
			}
			if (IsToggleOn(_FaultOn))
			{
				materialEditor.ShaderProperty(_FaultAmount, _FaultAmount.displayName);
				materialEditor.ShaderProperty(_FaultAlpha, _FaultAlpha.displayName);
			}
			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Add Texture", EditorStyles.boldLabel);
			EditorGUI.BeginChangeCheck();
			materialEditor.ShaderProperty(_AddTextureOn, _AddTextureOn.displayName);
			if (EditorGUI.EndChangeCheck())
			{
				foreach (var obj in materialEditor.targets)
				{
					var mat = obj as Material;
					if (mat == null) continue;
					SetKeyword(mat, KEY_ADDTEX, IsToggleOn(_AddTextureOn));
				}
			}
			if (IsToggleOn(_AddTextureOn))
			{
				materialEditor.TexturePropertySingleLine(new GUIContent(_AddGlowTex.displayName), _AddGlowTex, _AddColor);
				materialEditor.TextureScaleOffsetProperty(_AddGlowTex);
				materialEditor.TexturePropertySingleLine(new GUIContent(_DistortTex.displayName), _DistortTex);
				materialEditor.TextureScaleOffsetProperty(_DistortTex);
				materialEditor.ShaderProperty(_DistortAmount, _DistortAmount.displayName);
				materialEditor.ShaderProperty(_DistortTexXSpeed, _DistortTexXSpeed.displayName);
				materialEditor.ShaderProperty(_DistortTexYSpeed, _DistortTexYSpeed.displayName);
				materialEditor.ShaderProperty(_AddTextureBlendMode, _AddTextureBlendMode.displayName);
				materialEditor.ShaderProperty(_UseGlowAlphaWeight, _UseGlowAlphaWeight.displayName);
			}
			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Stencil", EditorStyles.boldLabel);
			materialEditor.ShaderProperty(_StencilRef, _StencilRef.displayName);
			materialEditor.ShaderProperty(_StencilComp, _StencilComp.displayName);

			foreach (var obj in materialEditor.targets)
			{
				var mat = obj as Material;
				if (mat == null) continue;
				SetKeyword(mat, KEY_STRAIGHT_ALPHA, IsToggleOn(_StraightAlphaInput));
				SetKeyword(mat, KEY_GLOW, IsToggleOn(_GlowOn));
				SetKeyword(mat, KEY_DISSOLVE, IsToggleOn(_DissolveOn));
				SetKeyword(mat, KEY_FAULT, IsToggleOn(_FaultOn));
				SetKeyword(mat, KEY_ADDTEX, IsToggleOn(_AddTextureOn));
			}
        }

        private void FindProperties(MaterialProperty[] props)
        {
            _Cutoff = FindProperty("_Cutoff", props);
            _MainTex = FindProperty("_MainTex", props);
            _StraightAlphaInput = FindProperty("_StraightAlphaInput", props, false);

            _GlowOn = FindProperty("_GlowOn", props, false);
            _GlowColor = FindProperty("_GlowColor", props, false);
            _GlowIntensity = FindProperty("_GlowIntensity", props, false);
            _GlowMask = FindProperty("_GlowMask", props, false);
            _GlowThreshold = FindProperty("_GlowThreshold", props, false);

            _DissolveOn = FindProperty("_DissolveOn", props, false);
            _DissolveTex = FindProperty("_DissolveTex", props, false);
            _DissolveAmount = FindProperty("_DissolveAmount", props, false);
            _DissolveGlowColor = FindProperty("_DissolveGlowColor", props, false);
            _GlowWidth = FindProperty("_GlowWidth", props, false);

            _FaultOn = FindProperty("_FaultOn", props, false);
            _FaultAmount = FindProperty("_FaultAmount", props, false);
            _FaultAlpha = FindProperty("_FaultAlpha", props, false);

            _AddTextureOn = FindProperty("_AddTextureOn", props, false);
            _AddGlowTex = FindProperty("_AddGlowTex", props, false);
            _AddColor = FindProperty("_AddColor", props, false);
            _DistortTex = FindProperty("_DistortTex", props, false);
            _DistortAmount = FindProperty("_DistortAmount", props, false);
            _DistortTexXSpeed = FindProperty("_DistortTexXSpeed", props, false);
            _DistortTexYSpeed = FindProperty("_DistortTexYSpeed", props, false);
            _AddTextureBlendMode = FindProperty("_AddTextureBlendMode", props, false);
            _UseGlowAlphaWeight = FindProperty("_UseGlowAlphaWeight", props, false);

            _StencilRef = FindProperty("_StencilRef", props);
            _StencilComp = FindProperty("_StencilComp", props);
        }

        private static bool IsToggleOn(MaterialProperty prop)
        {
            return prop != null && prop.floatValue > 0.5f;
        }

        private static void SetKeyword(Material mat, string keyword, bool enabled)
        {
            if (enabled)
            {
                mat.EnableKeyword(keyword);
            }
            else
            {
                mat.DisableKeyword(keyword);
            }
        }

        private static bool DrawToggleKeyword(MaterialEditor materialEditor, MaterialProperty toggleProp, string keyword)
        {
            if (toggleProp == null)
            {
                return false;
            }

            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUILayout.Toggle(toggleProp.displayName, IsToggleOn(toggleProp));
            if (EditorGUI.EndChangeCheck())
            {
                toggleProp.floatValue = newValue ? 1.0f : 0.0f;
            }

            return newValue;
        }
    }
}


