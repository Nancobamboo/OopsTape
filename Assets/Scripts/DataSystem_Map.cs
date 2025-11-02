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

    public static ERatingGrade GetRatingGrade(float accuracy)
    {
        if (accuracy >= 0.9f)
        {
            return ERatingGrade.S;
        }
        else if (accuracy >= 0.8f)
        {
            return ERatingGrade.A;
        }
        else if (accuracy >= 0.65f)
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
				return "你才是真正的 KEY MASTER！这门为你唱开！";
			case ERatingGrade.A:
				return "Almost in Key！节奏准得像开锁只差半圈～";
			case ERatingGrade.B:
				return "You found a key… 但不是正确那把。";
			case ERatingGrade.C:
				return "Key？在哪？节奏有点歪，幸好门好歹开了条缝。";
			case ERatingGrade.D:
				return "OOut of Key！你这是在敲锁不是在开锁！";
			case ERatingGrade.F:
				return "Wrong key! 节奏之门：访问被拒。重来吧，努力找到正确的Key～";
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
