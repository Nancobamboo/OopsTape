public enum ERatingGrade
{
    F = 0,
    D = 1,
    C = 2,
    B = 3,
    A = 4,
    S = 5
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
        else if (accuracy >= 0.4f)
        {
            return ERatingGrade.D;
        }
        else
        {
            return ERatingGrade.F;
        }
    }

		public static string GetSpriteNameByRating(ERatingGrade rating)
	{
		switch (rating)
		{
			case ERatingGrade.S:
				return "number_S";
			case ERatingGrade.A:
				return "number_A";
			case ERatingGrade.B:
				return "number_B";
			case ERatingGrade.C:
				return "number_C";
			case ERatingGrade.D:
				return "number_D";
			default:
				return "number_D";
		}
	}
	
	
	public static string GetRatingCommentText(ERatingGrade grade)
	{
		switch (grade)
		{
			case ERatingGrade.S:
				return "你才是真正的 KEY MASTER！";
			case ERatingGrade.A:
				return "Almost in Key！节奏准得像开锁只差半圈～";
			case ERatingGrade.B:
				return "You found a key… 但不是正确那把。";
			case ERatingGrade.C:
				return "Key？在哪？节奏有点歪，不过门好像还开了一条缝。";
			case ERatingGrade.D:
				return "Out of Key！Out of Line！你和节奏完全没对上锁孔……";
			case ERatingGrade.F:
				return "Wrong key! 门都不给你进！重来吧，也许下一次能找到正确的Key～";
			default:
				return "";
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
