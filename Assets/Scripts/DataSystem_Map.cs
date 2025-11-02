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
	public static float InputExtraTime = 0.6f;
	public static float InputForwardTime = 0.3f;

	public static float BaseScore = 5000f;
	public static float ItemBonus = 0.20f;
	public static float DifficultyRate = 1.5f;

	public static float CalculateFinalScore(int combo)
	{
		float comboBonus = combo * 0.1f;
		float itemBonus = ItemBonus;
		if (Instance != null)
		{
			DataLevel dataLevel = Instance.GetDataLevel();
			if (dataLevel != null && dataLevel.UseTutorUI)
			{
				itemBonus = 0f;
			}
		}
		return BaseScore * (1f + comboBonus + itemBonus) * DifficultyRate;
	}

	public static ERatingGrade GetRatingGrade(float accuracy)
	{
		if (accuracy >= 0.92f)
		{
			return ERatingGrade.S;
		}
		else if (accuracy >= 0.85f)
		{
			return ERatingGrade.A;
		}
		else if (accuracy >= 0.7f)
		{
			return ERatingGrade.B;
		}
		else if (accuracy >= 0.65f)
		{
			return ERatingGrade.C;
		}
		else if (accuracy >= 0.55f)
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
				return "KEY MASTER！\n节奏完美对上锁孔！节奏之门为你唱开～";
			case ERatingGrade.A:
				return "Almost in Key！\n只差半圈就能开锁啦～继续找找关键节拍！";
			case ERatingGrade.B:
				return "You found a key…\n但好像插错门了，拍子再准点！";
			case ERatingGrade.C:
				return "Key？Where？\n你的手指在找Key，但节奏在隔壁房间……";
			case ERatingGrade.D:
				return "Out of Key！\n节奏之门拒绝开启……你的钥匙掉地上了！";
			case ERatingGrade.F:
				return "Key Lost! \n嘿bro~ 别睡了，起来嗨！";
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
