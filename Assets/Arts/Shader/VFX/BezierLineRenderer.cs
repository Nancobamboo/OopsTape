using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(LineRenderer))]
public class BezierLineRenderer : MonoBehaviour
{
    [Header("Curve (Cubic Bezier)")]
    public Transform p0;
    public Transform p1;
    public Transform p2;
    public Transform p3;

    [Header("Rendering")]
    public LineRenderer lineRenderer;
    [Tooltip("Desired spacing (world units) between consecutive points along arc length.")]
    public float pointSpacing = 0.1f;
    [Tooltip("Maximum number of points to draw (safety cap).")]
    public int maxPoints = 2048;

    [Header("Sampling")]
    [Tooltip("Lookup granularity for the arc-length table (higher = more accurate).")]
    [Range(16, 4096)]
    public int lookupSamples = 256;
    
    public enum SampleMode { BySpacing, ByCount }
    [Header("Sampling Mode")]
    public SampleMode sampleMode = SampleMode.BySpacing;
    [Tooltip("Number of points when using ByCount sampling (inclusive of endpoints).")]
    [Min(2)]
    public int fixedPointCount = 32;

    [Header("Multi-Segment")]
    [Min(1)]
    [Tooltip("Number of cubic Bezier segments from p0 to p3.")]
    public int segmentCount = 1;
    [System.Serializable]
    public class SegmentSpec
    {
        [Tooltip("Curvature side for this segment")] public HalfCurvature sign = HalfCurvature.Random;
        [Tooltip("Sideways offset range as fraction of segment length")] public Vector2 offsetFractionRange = new Vector2(0.05f, 0.25f);
    }
    [Tooltip("Per-segment sign and offset range. Size auto-matches segmentCount.")]
    public List<SegmentSpec> segmentSpecs = new List<SegmentSpec>();

	[Min(1)]
	[Tooltip("Number of lightning strands (independent curves) from p0 to p3.")]
	public int strandCount = 1;
	[Tooltip("If true, keep the main LineRenderer visible alongside strands.")]
	public bool renderMainWhenStrands = false;
	[Tooltip("Random width multiplier range for each strand (applied to main LineRenderer widthMultiplier).")]
	public Vector2 strandWidthMultiplierRange = new Vector2(0.9f, 1.1f);

	public enum PathStyle { Smooth, Sharp }
	[Header("Shape & Amplitude")]
	[Tooltip("Smooth uses Bezier handles; Sharp creates piecewise straight segments with corners.")]
	public PathStyle pathStyle = PathStyle.Smooth;
	[Tooltip("Multiplies offsetFractionRange to increase overall amplitude.")]
	[Min(0f)] public float amplitudeMultiplier = 1f;

	[Header("Randomization")]
	[Tooltip("Master switch to allow any automatic randomization.")]
	public bool enableRandomization = true;
	[Tooltip("If true, control points are randomized on rebuild/update (obeys interval).")]
	public bool randomizeOnEachRebuild = false;
	[Tooltip("Minimum seconds between automatic randomizations when enabled. 0 = every update.")]
	[Min(0f)] public float randomizeIntervalSeconds = 0.05f;

	[Header("Procedural Controls (Two-Half Curvature)")]
	[Tooltip("Randomize p1/p2 to bend first half and second half with chosen signs.")]
	[HideInInspector] public bool randomizeOnEnable = false; // deprecated
	[Tooltip("Corner position range along each segment for Sharp style (fraction of segment length).")]
	public Vector2 sharpCornerAlongRange = new Vector2(0.45f, 0.55f);
    [Tooltip("Normal defining the plane of the curve. For XY curves use (0,0,1).")]
    public Vector3 planeNormal = Vector3.forward;
    [Range(0f, 1f)]
    [Tooltip("How far from p0 along the chord to place p1 (fraction of distance p0->p3).")]
    public float firstHandleAlong = 0.25f;
    [Range(0f, 1f)]
    [Tooltip("How far from p3 along the chord to place p2 (fraction of distance p0->p3).")]
    public float secondHandleAlong = 0.25f;
    public enum HalfCurvature { Positive, Negative, Random }

    [Header("Markers (optional)")]
    public GameObject markerPrefab;
    public Transform markersParent;
    public bool clearMarkersOnUpdate = true;

    [Header("Update")]
    public bool autoUpdateInEditor = true;
    [Tooltip("If control points are missing, create 4 child points automatically.")]
    public bool autoCreateControlPoints = true;

    // Cached lookup
    private readonly List<float> cumulativeLengths = new List<float>();
    private readonly List<float> tSamples = new List<float>();
    // Multi-segment storage (world space control points for each cubic)
    private readonly List<Vector3> multiA = new List<Vector3>();
    private readonly List<Vector3> multiB = new List<Vector3>();
    private readonly List<Vector3> multiC = new List<Vector3>();
    private readonly List<Vector3> multiD = new List<Vector3>();
	// Multi-strand storage (per strand cubic lists)
	private readonly List<List<Vector3>> sA = new List<List<Vector3>>();
	private readonly List<List<Vector3>> sB = new List<List<Vector3>>();
	private readonly List<List<Vector3>> sC = new List<List<Vector3>>();
	private readonly List<List<Vector3>> sD = new List<List<Vector3>>();
	[SerializeField]
	private List<LineRenderer> strandRenderers = new List<LineRenderer>();

	// Per-strand randomization overrides
	[System.Serializable]
	public class StrandOverride
	{
		public bool useOverride = false;
		public int seedOffset = 0;
		[Range(0f, 1f)] public float firstHandleAlong = -1f; // -1 => use global
		[Range(0f, 1f)] public float secondHandleAlong = -1f;
		public List<SegmentSpec> segmentSpecs = new List<SegmentSpec>();
	}
	[SerializeField]
	private List<StrandOverride> strandOverrides = new List<StrandOverride>();

	// Randomization timing
	private float _lastRandomizeTime = -999f;

	[SerializeField, HideInInspector]
	private int _lastStrandCount = -1;

    private void Reset()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (autoCreateControlPoints) EnsureControlPoints();
    }

    private void OnValidate()
    {
        pointSpacing = Mathf.Max(0.0001f, pointSpacing);
        maxPoints = Mathf.Max(2, maxPoints);
        lookupSamples = Mathf.Clamp(lookupSamples, 16, 4096);
        fixedPointCount = Mathf.Max(2, fixedPointCount);
        segmentCount = Mathf.Max(1, segmentCount);
        strandCount = Mathf.Max(1, strandCount);
        SyncSegmentSpecsSize();
        SyncStrandOverridesSize();

        if (autoCreateControlPoints) EnsureControlPoints();

        if (autoUpdateInEditor && Application.isEditor && !Application.isPlaying)
        {
            TryUpdateLine();
        }
    }

    private void OnEnable()
    {
        if (autoCreateControlPoints) EnsureControlPoints();
        TryUpdateLine();
    }

    [ContextMenu("Rebuild Now")]
    public void RebuildNow()
    {
        TryUpdateLine();
    }

    private void Update()
    {
        if (Application.isPlaying)
        {
            TryUpdateLine();
        }
        else if (autoUpdateInEditor)
        {
            TryUpdateLine();
        }
    }

    public void TryUpdateLine()
    {
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null) lineRenderer = GetComponentInChildren<LineRenderer>();
        }
        if (!AreControlPointsValid())
        {
            Debug.LogWarning("BezierLineRenderer: Control points not assigned. Assign p0-p3 or enable autoCreateControlPoints.", this);
            return;
        }

        if (enableRandomization && randomizeOnEachRebuild && ShouldRandomizeNow())
        {
            GenerateRandomSegments(false);
        }

        // Handle strand lifecycle based on count changes
        if (strandCount > 1)
        {
            if (_lastStrandCount != strandCount)
            {
                // Count changed: delete old strands and recreate fresh ones
                ClearAllStrands();
            }
        }

        if (strandCount <= 1)
        {
            // Ensure no stray strand renderers remain when strands are disabled
            ClearAllStrands();
            BuildArcLengthTable();

            float totalLength = cumulativeLengths[cumulativeLengths.Count - 1];
            if (totalLength <= 0f)
            {
                Vector3[] fallback = new[] { EvaluateAt(0f), EvaluateAt(1f) };
                ApplyPositions(fallback);
                HandleMarkers(fallback);
                return;
            }

            Vector3[] positions;
            if (sampleMode == SampleMode.ByCount)
            {
                int count = Mathf.Clamp(fixedPointCount, 2, maxPoints);
                positions = new Vector3[count];

                float step = totalLength / (count - 1);
                for (int i = 0; i < count; i++)
                {
                    float s = Mathf.Min(totalLength, step * i);
                    float t = GetTForArcLength(s);
                    positions[i] = EvaluateAt(t);
                }

                // Ensure last point lands on t=1
                positions[count - 1] = EvaluateAt(1f);
            }
            else
            {
                int count = Mathf.Min(maxPoints, Mathf.Max(2, Mathf.FloorToInt(totalLength / pointSpacing) + 1));
                positions = new Vector3[count];

                float s = 0f;
                for (int i = 0; i < count; i++)
                {
                    float t = GetTForArcLength(s);
                    positions[i] = EvaluateAt(t);
                    s = Mathf.Min(totalLength, s + pointSpacing);
                }

                // Ensure last point lands on t=1
                positions[count - 1] = EvaluateAt(1f);
            }

            ApplyPositions(positions);
            HandleMarkers(positions);
        }
        else
        {
            EnsureStrandRenderers();
            if (enableRandomization && randomizeOnEachRebuild && ShouldRandomizeNow())
            {
                GenerateStrandCurves(false); // regenerate all
            }
            else
            {
                // Only generate curves for newly added/empty strands; keep existing shapes intact
                GenerateStrandCurves(true);
            }
            
            int count = (sampleMode == SampleMode.ByCount)
                ? Mathf.Clamp(fixedPointCount, 2, maxPoints)
                : Mathf.Clamp(lookupSamples * Mathf.Max(1, segmentCount), 2, maxPoints);

            for (int si = 0; si < strandRenderers.Count; si++)
            {
                Vector3[] positions = new Vector3[count];
                for (int i = 0; i < count; i++)
                {
                    float t = (float)i / (count - 1);
                    positions[i] = EvaluateAtStrand(si, t);
                }
                ApplyPositionsToRenderer(strandRenderers[si], positions);
            }

            if (!renderMainWhenStrands && lineRenderer != null) lineRenderer.positionCount = 0;
        }

        // Persist last known count for change detection
        _lastStrandCount = strandCount;
    }

    private bool AreControlPointsValid()
    {
        return p0 != null && p1 != null && p2 != null && p3 != null;
    }

    private void EnsureControlPoints()
    {
        if (AreControlPointsValid()) return;

        Transform FindOrCreate(string name, Vector3 localPos)
        {
            Transform child = transform.Find(name);
            if (child == null)
            {
                GameObject go = new GameObject(name);
                child = go.transform;
                child.SetParent(transform, false);
                child.localPosition = localPos;
            }
            return child;
        }

        if (p0 == null) p0 = FindOrCreate("P0", new Vector3(-1f, 0f, 0f));
        if (p1 == null) p1 = FindOrCreate("P1", new Vector3(-0.5f, 0.5f, 0f));
        if (p2 == null) p2 = FindOrCreate("P2", new Vector3(0.5f, 0.5f, 0f));
        if (p3 == null) p3 = FindOrCreate("P3", new Vector3(1f, 0f, 0f));
    }

    private Vector3 GetP0() { return p0.position; }
    private Vector3 GetP1() { return p1.position; }
    private Vector3 GetP2() { return p2.position; }
    private Vector3 GetP3() { return p3.position; }

    private static Vector3 EvaluateCubic(in Vector3 a, in Vector3 b, in Vector3 c, in Vector3 d, float t)
    {
        float u = 1f - t;
        float uu = u * u;
        float tt = t * t;
        return (uu * u) * a
             + (3f * uu * t) * b
             + (3f * u * tt) * c
             + (tt * t) * d;
    }

    private void BuildArcLengthTable()
    {
        tSamples.Clear();
        cumulativeLengths.Clear();

        int n = Mathf.Max(2, lookupSamples * Mathf.Max(1, segmentCount));
        Vector3 prev = EvaluateAt(0f);
        float length = 0f;

        for (int i = 0; i < n; i++)
        {
            float t = (float)i / (n - 1);
            Vector3 p = EvaluateAt(t);

            if (i > 0)
            {
                length += Vector3.Distance(prev, p);
            }

            tSamples.Add(t);
            cumulativeLengths.Add(length);
            prev = p;
        }

        // Guard: ensure strictly increasing end
        if (cumulativeLengths[cumulativeLengths.Count - 1] <= 0f)
        {
            cumulativeLengths[cumulativeLengths.Count - 1] = 0.00001f;
        }
    }

    private float GetTForArcLength(float s)
    {
        // Clamp s
        float total = cumulativeLengths[cumulativeLengths.Count - 1];
        if (s <= 0f) return 0f;
        if (s >= total) return 1f;

        // Binary search on cumulativeLengths
        int lo = 0;
        int hi = cumulativeLengths.Count - 1;
        while (lo < hi)
        {
            int mid = (lo + hi) >> 1;
            if (cumulativeLengths[mid] < s) lo = mid + 1;
            else hi = mid;
        }

        int idx = Mathf.Max(1, lo);
        float s0 = cumulativeLengths[idx - 1];
        float s1 = cumulativeLengths[idx];
        float t0 = tSamples[idx - 1];
        float t1 = tSamples[idx];

        float u = Mathf.Approximately(s1, s0) ? 0f : (s - s0) / (s1 - s0);
        return Mathf.Lerp(t0, t1, Mathf.Clamp01(u));
    }

    private void ApplyPositions(Vector3[] positions)
    {
        if (lineRenderer == null) return;
        ApplyPositionsToRenderer(lineRenderer, positions);
    }

    private void ApplyPositionsToRenderer(LineRenderer target, Vector3[] positions)
    {
        if (target == null) return;
        target.positionCount = positions.Length;
        if (target.useWorldSpace)
        {
            target.SetPositions(positions);
        }
        else
        {
            Transform t = target.transform;
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = t.InverseTransformPoint(positions[i]);
            }
            target.SetPositions(positions);
        }
    }

    private void EnsureStrandRenderers()
    {
        // Adopt any existing child LineRenderers named as strands
        AdoptExistingStrandRenderers();

        // Grow to target count
        while (strandRenderers.Count < strandCount)
        {
            GameObject go = new GameObject($"Strand_{strandRenderers.Count:D2}");
            go.transform.SetParent(transform, false);
            LineRenderer lr = go.AddComponent<LineRenderer>();
            CopyLineRendererSettings(lineRenderer, lr);
            strandRenderers.Add(lr);
        }

        // Trim extras beyond target count
        for (int i = strandRenderers.Count - 1; i >= strandCount; i--)
        {
            if (strandRenderers[i] != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) DestroyImmediate(strandRenderers[i].gameObject);
                else Destroy(strandRenderers[i].gameObject);
#else
                Destroy(strandRenderers[i].gameObject);
#endif
            }
            strandRenderers.RemoveAt(i);
        }
    }

    private void AdoptExistingStrandRenderers()
    {
        // Clean nulls
        for (int i = strandRenderers.Count - 1; i >= 0; i--)
        {
            if (strandRenderers[i] == null) strandRenderers.RemoveAt(i);
        }

        // Gather children that look like strands
        List<LineRenderer> found = new List<LineRenderer>();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            LineRenderer lr = child.GetComponent<LineRenderer>();
            if (lr == null) continue;
            if (lr == lineRenderer) continue; // skip main
            if (!child.name.StartsWith("Strand_")) continue; // be conservative
            found.Add(lr);
        }

        // Sort by name to stabilize ordering
        found.Sort((a, b) => string.CompareOrdinal(a.gameObject.name, b.gameObject.name));

        // Replace current list with found (up to target)
        strandRenderers.Clear();
        for (int i = 0; i < found.Count; i++)
        {
            strandRenderers.Add(found[i]);
        }
    }

    private void ClearAllStrands()
    {
        // Destroy all strand children and clear data structures
        for (int i = strandRenderers.Count - 1; i >= 0; i--)
        {
            if (strandRenderers[i] == null) continue;
#if UNITY_EDITOR
            if (!Application.isPlaying) DestroyImmediate(strandRenderers[i].gameObject);
            else Destroy(strandRenderers[i].gameObject);
#else
            Destroy(strandRenderers[i].gameObject);
#endif
        }
        strandRenderers.Clear();

        // Clear per-strand curve data
        sA.Clear(); sB.Clear(); sC.Clear(); sD.Clear();
    }

    private void CopyLineRendererSettings(LineRenderer src, LineRenderer dst)
    {
        if (dst == null) return;
        if (src == null)
        {
            dst.widthMultiplier = 0.05f;
            dst.useWorldSpace = true;
            return;
        }

        dst.shadowCastingMode = src.shadowCastingMode;
        dst.receiveShadows = src.receiveShadows;
        dst.motionVectorGenerationMode = src.motionVectorGenerationMode;
        dst.numCornerVertices = src.numCornerVertices;
        dst.numCapVertices = src.numCapVertices;
        dst.textureMode = src.textureMode;
        dst.alignment = src.alignment;
        dst.generateLightingData = src.generateLightingData;
        dst.widthMultiplier = src.widthMultiplier * RandomRange(strandWidthMultiplierRange);
        dst.colorGradient = src.colorGradient;
        dst.material = src.material;
        dst.sharedMaterials = src.sharedMaterials;
        dst.sortingLayerID = src.sortingLayerID;
        dst.sortingOrder = src.sortingOrder;
        dst.useWorldSpace = src.useWorldSpace;
    }

    private void GenerateStrandCurves(bool onlyGenerateIfEmpty)
    {
        // Ensure per-strand control lists
        while (sA.Count < strandCount) { sA.Add(new List<Vector3>()); sB.Add(new List<Vector3>()); sC.Add(new List<Vector3>()); sD.Add(new List<Vector3>()); }
        while (sA.Count > strandCount) { sA.RemoveAt(sA.Count - 1); sB.RemoveAt(sB.Count - 1); sC.RemoveAt(sC.Count - 1); sD.RemoveAt(sD.Count - 1); }
        SyncStrandOverridesSize();

        for (int i = 0; i < strandCount; i++)
        {
            if (onlyGenerateIfEmpty && sA[i].Count > 0) continue;
            // Deterministic per-strand seed if override provided
            if (strandOverrides[i].useOverride && strandOverrides[i].seedOffset != 0)
            {
                Random.InitState(strandOverrides[i].seedOffset);
            }

            sA[i].Clear(); sB[i].Clear(); sC[i].Clear(); sD[i].Clear();
            GenerateSegmentsIntoLists(GetSignsForStrand(i), sA[i], sB[i], sC[i], sD[i], i);

            // Update renderer settings in case main changed
            if (i < strandRenderers.Count)
            {
                CopyLineRendererSettings(lineRenderer, strandRenderers[i]);
            }
        }
        _lastRandomizeTime = Time.time;
    }

    private void GenerateSegmentsIntoLists(List<HalfCurvature> signs, List<Vector3> outA, List<Vector3> outB, List<Vector3> outC, List<Vector3> outD, int strandIndex = -1)
    {
        if (!AreControlPointsValid()) return;
        SyncSegmentSpecsSize();

        Vector3 start = GetP0();
        Vector3 end = GetP3();
        Vector3 chord = end - start;
        float totalLen = chord.magnitude;
        if (totalLen < 1e-4f)
        {
            outA.Add(start); outB.Add(start); outC.Add(end); outD.Add(end);
            return;
        }

        Vector3 dir = chord / totalLen;
        Vector3 n = planeNormal.sqrMagnitude < 1e-8f ? Vector3.forward : planeNormal.normalized;
        Vector3 side = Vector3.Cross(n, dir);
        if (side.sqrMagnitude < 1e-8f)
        {
            n = Vector3.up;
            side = Vector3.Cross(n, dir);
            if (side.sqrMagnitude < 1e-8f)
            {
                side = Vector3.right;
            }
        }
        side.Normalize();

        int segs = Mathf.Max(1, segmentCount);
        float segLen = totalLen / segs;
        for (int i = 0; i < segs; i++)
        {
            Vector3 a = start + dir * (segLen * i);
            Vector3 d = start + dir * (segLen * (i + 1));

            int sign = ResolveSign(signs[i]);
            float along1 = Mathf.Clamp01(GetFirstAlongForStrand(strandIndex));
            float along2 = Mathf.Clamp01(GetSecondAlongForStrand(strandIndex));
            Vector2 range = GetOffsetRangeForSegment(strandIndex, i);
            float amp = Mathf.Max(0f, amplitudeMultiplier);
            float offFrac1 = RandomRange(range) * amp;
            float offFrac2 = RandomRange(range) * amp;

            if (pathStyle == PathStyle.Sharp)
            {
                // Build two straight subsegments meeting at a corner offset to the chosen side
                float cornerAlong = Mathf.Clamp01(RandomRange(sharpCornerAlongRange));
                Vector3 corner = a + dir * (cornerAlong * segLen) + side * (sign * offFrac1 * segLen);
                // First: a -> corner
                outA.Add(a); outB.Add(a); outC.Add(corner); outD.Add(corner);
                // Second: corner -> d
                outA.Add(corner); outB.Add(corner); outC.Add(d); outD.Add(d);
            }
            else
            {
                Vector3 base1 = a + dir * (along1 * segLen);
                Vector3 base2 = d - dir * (along2 * segLen);
                Vector3 b = base1 + side * (sign * offFrac1 * segLen);
                Vector3 c = base2 + side * (sign * offFrac2 * segLen);
                outA.Add(a); outB.Add(b); outC.Add(c); outD.Add(d);
            }
        }
    }

    private List<HalfCurvature> GetSignsForStrand(int strandIndex)
    {
        if (strandIndex >= 0 && strandIndex < strandOverrides.Count && strandOverrides[strandIndex].useOverride)
        {
            var so = strandOverrides[strandIndex];
            if (so.segmentSpecs == null) so.segmentSpecs = new List<SegmentSpec>();
            while (so.segmentSpecs.Count < segmentCount) so.segmentSpecs.Add(new SegmentSpec());
            if (so.segmentSpecs.Count > segmentCount) so.segmentSpecs.RemoveRange(segmentCount, so.segmentSpecs.Count - segmentCount);
            List<HalfCurvature> signs = new List<HalfCurvature>(segmentCount);
            for (int i = 0; i < segmentCount; i++) signs.Add(so.segmentSpecs[i].sign);
            return signs;
        }
        List<HalfCurvature> defaultSigns = new List<HalfCurvature>(segmentCount);
        while (defaultSigns.Count < segmentCount) defaultSigns.Add(segmentSpecs.Count > defaultSigns.Count ? segmentSpecs[defaultSigns.Count].sign : HalfCurvature.Random);
        return defaultSigns;
    }

    private float GetFirstAlongForStrand(int strandIndex)
    {
        if (strandIndex >= 0 && strandIndex < strandOverrides.Count && strandOverrides[strandIndex].useOverride && strandOverrides[strandIndex].firstHandleAlong >= 0f)
            return strandOverrides[strandIndex].firstHandleAlong;
        return firstHandleAlong;
    }

    private float GetSecondAlongForStrand(int strandIndex)
    {
        if (strandIndex >= 0 && strandIndex < strandOverrides.Count && strandOverrides[strandIndex].useOverride && strandOverrides[strandIndex].secondHandleAlong >= 0f)
            return strandOverrides[strandIndex].secondHandleAlong;
        return secondHandleAlong;
    }

    private Vector2 GetOffsetRangeForSegment(int strandIndex, int segmentIndex)
    {
        // Override path: use per-strand segment specs if enabled
        if (strandIndex >= 0 && strandIndex < strandOverrides.Count && strandOverrides[strandIndex].useOverride)
        {
            var so = strandOverrides[strandIndex];
            if (so.segmentSpecs == null) so.segmentSpecs = new List<SegmentSpec>();
            while (so.segmentSpecs.Count < segmentCount) so.segmentSpecs.Add(new SegmentSpec());
            if (segmentIndex >= 0 && segmentIndex < so.segmentSpecs.Count)
                return so.segmentSpecs[segmentIndex].offsetFractionRange;
        }
        // Default to global per-segment spec list
        if (segmentIndex >= 0)
        {
            if (segmentSpecs == null) segmentSpecs = new List<SegmentSpec>();
            while (segmentSpecs.Count < segmentCount) segmentSpecs.Add(new SegmentSpec());
            if (segmentIndex < segmentSpecs.Count) return segmentSpecs[segmentIndex].offsetFractionRange;
        }
        return new Vector2(0.05f, 0.25f);
    }

    private void SyncStrandOverridesSize()
    {
        int target = Mathf.Max(1, strandCount);
        while (strandOverrides.Count < target) strandOverrides.Add(new StrandOverride());
        if (strandOverrides.Count > target) strandOverrides.RemoveRange(target, strandOverrides.Count - target);
        for (int i = 0; i < strandOverrides.Count; i++)
        {
            var so = strandOverrides[i];
            if (so.segmentSpecs == null) so.segmentSpecs = new List<SegmentSpec>();
            while (so.segmentSpecs.Count < segmentCount) so.segmentSpecs.Add(new SegmentSpec());
            if (so.segmentSpecs.Count > segmentCount) so.segmentSpecs.RemoveRange(segmentCount, so.segmentSpecs.Count - segmentCount);
        }
    }

    private bool ShouldRandomizeNow()
    {
        if (!Application.isPlaying) return true; // in editor, allow frequent regen
        if (randomizeIntervalSeconds <= 0f) return true;
        if (Time.time - _lastRandomizeTime >= randomizeIntervalSeconds) return true;
        return false;
    }

    private Vector3 EvaluateAtStrand(int strandIndex, float t)
    {
        if (strandIndex < 0 || strandIndex >= sA.Count || sA[strandIndex].Count == 0)
        {
            return EvaluateAt(t);
        }
        float u = Mathf.Clamp01(t);
        int segs = sA[strandIndex].Count;
        float scaled = u * segs;
        int idx = Mathf.Min(segs - 1, Mathf.FloorToInt(scaled));
        float localT = Mathf.Clamp01(scaled - idx);
        return EvaluateCubic(sA[strandIndex][idx], sB[strandIndex][idx], sC[strandIndex][idx], sD[strandIndex][idx], localT);
    }

    private void HandleMarkers(Vector3[] positions)
    {
        if (markerPrefab == null) return;

        if (clearMarkersOnUpdate && markersParent != null)
        {
            for (int i = markersParent.childCount - 1; i >= 0; i--)
            {
                Transform child = markersParent.GetChild(i);
#if UNITY_EDITOR
                if (!Application.isPlaying) DestroyImmediate(child.gameObject);
                else Destroy(child.gameObject);
#else
                Destroy(child.gameObject);
#endif
            }
        }

        Transform parent = markersParent != null ? markersParent : transform;

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject go = Instantiate(markerPrefab, positions[i], Quaternion.identity, parent);
            go.name = $"Marker_{i:D3}";
        }
    }

    [ContextMenu("Randomize Controls (Single or Multi-Segment)")]
    public void RandomizeControlsContext()
    {
        GenerateRandomSegments(true);
        TryUpdateLine();
    }

    private void GenerateRandomSegments(bool reseed)
    {
        if (!AreControlPointsValid()) return;
        SyncSegmentSpecsSize();

        multiA.Clear(); multiB.Clear(); multiC.Clear(); multiD.Clear();

        Vector3 start = GetP0();
        Vector3 end = GetP3();
        Vector3 chord = end - start;
        float totalLen = chord.magnitude;
        if (totalLen < 1e-4f)
        {
            multiA.Add(start); multiB.Add(start); multiC.Add(end); multiD.Add(end);
            return;
        }

        Vector3 dir = chord / totalLen;
        Vector3 n = planeNormal.sqrMagnitude < 1e-8f ? Vector3.forward : planeNormal.normalized;
        Vector3 side = Vector3.Cross(n, dir);
        if (side.sqrMagnitude < 1e-8f)
        {
            n = Vector3.up;
            side = Vector3.Cross(n, dir);
            if (side.sqrMagnitude < 1e-8f)
            {
                side = Vector3.right;
            }
        }
        side.Normalize();

        int segs = Mathf.Max(1, segmentCount);
        float segLen = totalLen / segs;

        for (int i = 0; i < segs; i++)
        {
            Vector3 a = start + dir * (segLen * i);
            Vector3 d = start + dir * (segLen * (i + 1));

            int sign = ResolveSign(segmentSpecs[i].sign);
            float along1 = Mathf.Clamp01(firstHandleAlong);
            float along2 = Mathf.Clamp01(secondHandleAlong);
            float offFrac1 = RandomRange(segmentSpecs[i].offsetFractionRange) * Mathf.Max(0f, amplitudeMultiplier);
            float offFrac2 = RandomRange(segmentSpecs[i].offsetFractionRange) * Mathf.Max(0f, amplitudeMultiplier);

            Vector3 base1 = a + dir * (along1 * segLen);
            Vector3 base2 = d - dir * (along2 * segLen);
            Vector3 b = base1 + side * (sign * offFrac1 * segLen);
            Vector3 c = base2 + side * (sign * offFrac2 * segLen);

            multiA.Add(a); multiB.Add(b); multiC.Add(c); multiD.Add(d);
        }
    }

    private void SyncSegmentSpecsSize()
    {
        int target = Mathf.Max(1, segmentCount);
        if (segmentSpecs == null) segmentSpecs = new List<SegmentSpec>(target);
        while (segmentSpecs.Count < target) segmentSpecs.Add(new SegmentSpec());
        if (segmentSpecs.Count > target) segmentSpecs.RemoveRange(target, segmentSpecs.Count - target);
    }

    private Vector3 EvaluateAt(float t)
    {
        if (Mathf.Clamp(segmentCount, 1, int.MaxValue) <= 1 || multiA.Count == 0)
        {
            return EvaluateCubic(GetP0(), GetP1(), GetP2(), GetP3(), Mathf.Clamp01(t));
        }

        float u = Mathf.Clamp01(t);
        int segs = multiA.Count;
        float scaled = u * segs;
        int idx = Mathf.Min(segs - 1, Mathf.FloorToInt(scaled));
        float localT = Mathf.Clamp01(scaled - idx);
        return EvaluateCubic(multiA[idx], multiB[idx], multiC[idx], multiD[idx], localT);
    }

    private static int ResolveSign(HalfCurvature h)
    {
        switch (h)
        {
            case HalfCurvature.Positive: return +1;
            case HalfCurvature.Negative: return -1;
            default: return Random.value < 0.5f ? +1 : -1;
        }
    }

    private static float RandomRange(Vector2 range)
    {
        float lo = Mathf.Min(range.x, range.y);
        float hi = Mathf.Max(range.x, range.y);
        return Random.Range(lo, hi);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!AreControlPointsValid()) return;

        Gizmos.color = Color.yellow;
        int gizmoSamples = 32;
        Vector3 prev = EvaluateAt(0f);
        for (int i = 1; i < gizmoSamples; i++)
        {
            float t = (float)i / (gizmoSamples - 1);
            Vector3 p = EvaluateAt(t);
            Gizmos.DrawLine(prev, p);
            prev = p;
        }
    }
#endif
}