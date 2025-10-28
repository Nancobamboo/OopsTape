using System;
using System.Collections.Generic;
using UnityEngine;

public static class BeatUtility
{
    public struct Settings
    {
        public int WindowSize;
        public int HopSize;
        public float BpmMin;
        public float BpmMax;
        public int PhaseSearchSteps;
        public int SmoothSize;

        public static Settings Default => new Settings
        {
            WindowSize = 1024,
            HopSize = 512,
            BpmMin = 60f,
            BpmMax = 200f,
            PhaseSearchSteps = 200,
            SmoothSize = 3
        };
    }

    public struct BeatTimeline
    {
        public float Bpm;
        public double SecondsPerBeat;
        public double OffsetSeconds;
        public double LengthSeconds;
        public List<double> BeatTimes;

        public double GetTimeOfBeat(double beatIndex)
        {
            return OffsetSeconds + beatIndex * SecondsPerBeat;
        }

        public double GetBeatOfTime(double timeSeconds)
        {
            return (timeSeconds - OffsetSeconds) / SecondsPerBeat;
        }

        public int GetNearestBeatIndex(double timeSeconds)
        {
            double b = GetBeatOfTime(timeSeconds);
            return (int)Math.Round(b);
        }
    }

    public static BeatTimeline AnalyzeBeats(AudioClip clip, Settings settings = default)
    {
        if (clip == null) throw new ArgumentNullException(nameof(clip));
        if (settings.WindowSize == 0) settings = Settings.Default;

        int channels = clip.channels;
        int totalSamples = clip.samples * channels;
        float[] interleaved = new float[totalSamples];
        clip.GetData(interleaved, 0);

        float[] mono = new float[clip.samples];
        for (int i = 0, si = 0; i < clip.samples; i++)
        {
            float sum = 0f;
            for (int c = 0; c < channels; c++) sum += interleaved[si++];
            mono[i] = sum / channels;
        }

        int win = settings.WindowSize;
        int hop = settings.HopSize;
        int frames = 1 + Math.Max(0, (mono.Length - win) / hop);
        float[] energy = new float[frames];
        for (int f = 0; f < frames; f++)
        {
            int start = f * hop;
            double acc = 0.0;
            for (int s = 0; s < win; s++)
            {
                float v = mono[start + s];
                acc += v * v;
            }
            energy[f] = (float)Math.Sqrt(acc / win);
        }

        float[] onset = new float[frames];
        float prev = 0f;
        for (int i = 0; i < frames; i++)
        {
            float diff = energy[i] - prev;
            prev = energy[i];
            onset[i] = diff > 0 ? diff : 0f;
        }

        if (settings.SmoothSize > 1)
        {
            int k = settings.SmoothSize;
            float[] tmp = new float[frames];
            for (int i = 0; i < frames; i++)
            {
                double sum = 0.0; int cnt = 0;
                for (int j = -k; j <= k; j++)
                {
                    int t = i + j;
                    if (t < 0 || t >= frames) continue;
                    sum += onset[t]; cnt++;
                }
                tmp[i] = (float)(sum / Math.Max(1, cnt));
            }
            onset = tmp;
        }

        double envRate = (double)clip.frequency / hop;
        int minLag = (int)Math.Round(envRate * 60.0 / settings.BpmMax);
        int maxLag = (int)Math.Round(envRate * 60.0 / settings.BpmMin);
        minLag = Math.Max(1, minLag);
        maxLag = Math.Min(frames - 1, Math.Max(minLag + 1, maxLag));

        int bestLag = minLag;
        double bestScore = double.NegativeInfinity;
        for (int lag = minLag; lag <= maxLag; lag++)
        {
            double sum = 0.0;
            for (int i = lag; i < frames; i++) sum += onset[i] * onset[i - lag];
            if (sum > bestScore) { bestScore = sum; bestLag = lag; }
        }

        double secondsPerBeat = bestLag / envRate;
        float bpm = (float)(60.0 / secondsPerBeat);

        int steps = Math.Max(8, settings.PhaseSearchSteps);
        double bestPhase = 0.0;
        bestScore = double.NegativeInfinity;
        for (int s = 0; s < steps; s++)
        {
            double phase = s / (double)steps * secondsPerBeat;
            double score = 0.0;
            for (double t = phase; t < clip.length; t += secondsPerBeat)
            {
                int idx = (int)Math.Round(t * envRate);
                if (idx >= 0 && idx < onset.Length) score += onset[idx];
            }
            if (score > bestScore) { bestScore = score; bestPhase = phase; }
        }

        List<double> beatTimes = new List<double>();
        for (double t = bestPhase; t <= clip.length + 1e-6; t += secondsPerBeat)
            beatTimes.Add(t);

        return new BeatTimeline
        {
            Bpm = bpm,
            SecondsPerBeat = secondsPerBeat,
            OffsetSeconds = bestPhase,
            LengthSeconds = clip.length,
            BeatTimes = beatTimes
        };
    }

    public static double QuantizeTimeToDivision(BeatTimeline t, double timeSeconds, int division)
    {
        if (division <= 0) division = 1;
        double beatPos = t.GetBeatOfTime(timeSeconds);
        double q = Math.Round(beatPos * division) / division;
        return t.GetTimeOfBeat(q);
    }

    public static bool IsTimeNearBeat(BeatTimeline t, double timeSeconds, double windowSeconds)
    {
        int i = t.GetNearestBeatIndex(timeSeconds);
        double nearestTime = t.GetTimeOfBeat(i);
        return Math.Abs(nearestTime - timeSeconds) <= windowSeconds;
    }

    public static string ToJson(BeatTimeline timeline, bool pretty = true)
    {
        BeatTimelineJson dto = ToDto(timeline);
        return JsonUtility.ToJson(dto, pretty);
    }

    public static string ToJson(BeatTimeline timeline, string audioName, bool pretty = true)
    {
        BeatTimelineJson dto = BuildJson(timeline, audioName);
        return JsonUtility.ToJson(dto, pretty);
    }

    public static BeatTimeline FromJson(string json)
    {
        BeatTimelineJson dto = JsonUtility.FromJson<BeatTimelineJson>(json);
        return FromDto(dto);
    }

    public static BeatTimelineJson BuildJson(BeatTimeline t, string audioName)
    {
        BeatTimelineJson dto = ToDto(t);
        dto.AudioName = audioName;
        if (dto.BeatUnits == null) dto.BeatUnits = new List<BeatUnit>();
        return dto;
    }

    static BeatTimelineJson ToDto(BeatTimeline t)
    {
        List<float> list = new List<float>(t.BeatTimes != null ? t.BeatTimes.Count : 0);
        if (t.BeatTimes != null)
            for (int i = 0; i < t.BeatTimes.Count; i++) list.Add((float)t.BeatTimes[i]);

        return new BeatTimelineJson
        {
            Bpm = t.Bpm,
            SecondsPerBeat = (float)t.SecondsPerBeat,
            OffsetSeconds = (float)t.OffsetSeconds,
            LengthSeconds = (float)t.LengthSeconds,
            BeatTimes = list,
            AudioName = string.Empty,
            BeatUnits = new List<BeatUnit>()
        };
    }

    static BeatTimeline FromDto(BeatTimelineJson d)
    {
        List<double> list = new List<double>(d.BeatTimes != null ? d.BeatTimes.Count : 0);
        if (d.BeatTimes != null)
            for (int i = 0; i < d.BeatTimes.Count; i++) list.Add(d.BeatTimes[i]);

        return new BeatTimeline
        {
            Bpm = d.Bpm,
            SecondsPerBeat = d.SecondsPerBeat,
            OffsetSeconds = d.OffsetSeconds,
            LengthSeconds = d.LengthSeconds,
            BeatTimes = list
        };
    }
}

[Serializable]
public class BeatTimelineJson
{
    public float Bpm;
    public float SecondsPerBeat;
    public float OffsetSeconds;
    public float LengthSeconds;
    public List<float> BeatTimes;
    public string AudioName;
    public List<BeatUnit> BeatUnits;
}

[Serializable]
public class BeatUnit
{
    public int BeatId;
    public List<string> SceneObjects;
    public List<string> AnimList;
}


