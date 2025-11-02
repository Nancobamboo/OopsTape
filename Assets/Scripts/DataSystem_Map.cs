public enum ERatingGrade
{
    D = 0,
    C = 1,
    B = 2,
    A = 3,
    S = 4
}

public partial class DataSystem
{
	public static float InputExtraTime = 0.5f; //按下阈值

	public static float BaseScore = 5000f;
	public static float ItemBonus = 0.20f;
	public static float DifficultyRate = 1.5f;

	public static float CalculateFinalScore(int combo)
	{
		float comboBonus = combo * 0.1f;
		return BaseScore * (1f + comboBonus + ItemBonus) * DifficultyRate;
	}

	 private ERatingGrade GetRatingGrade(float accuracy)
    {
        if (accuracy >= 0.8f)
        {
            return ERatingGrade.S;
        }
        else if (accuracy >= 0.7f)
        {
            return ERatingGrade.A;
        }
        else if (accuracy >= 0.6f)
        {
            return ERatingGrade.B;
        }
        else if (accuracy >= 0.5f)
        {
            return ERatingGrade.C;
        }
        else
        {
            return ERatingGrade.D;
        }
    }

	public void LoadGameData()
	{
		LoadDataLevel();
		var dataLevel = GetDataLevel();
		if (dataLevel != null && dataLevel.LevelUnlocked.Count == 0)
		{
			dataLevel.AddLevelUnlockedData((int)UISelectControl.ESceneName.Level04_Sing);
			SaveDataLevel();
		}
	}
}
