using UnityEngine.SceneManagement;

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
	public static float InputExtraTime = 0.3f;
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
		string sceneName = SceneManager.GetActiveScene().name;
		if (System.Enum.TryParse<UISelectControl.ESceneName>(sceneName, out UISelectControl.ESceneName sceneEnum))
		{
			if (sceneEnum == UISelectControl.ESceneName.Level03_Fly)
			{
				switch (grade)
				{
					case ERatingGrade.S:
						return "KEY LOVERS！\n你们的心跳对上拍，连苍蝇都嫉妒了！";
					case ERatingGrade.A:
						return "Almost in Key！\n差一拍，就能听见爱情的咔哒声~";
					case ERatingGrade.B:
						return "节奏对一半，心门开一缝\n苍蝇还在笑你俩笨～";
					case ERatingGrade.C:
						return "Key？Where？\n你的手指在找Key，但节奏在隔壁房间……";
					case ERatingGrade.D:
						return "Out of Key！\n你奏断线，恋爱信号消失……";
					case ERatingGrade.F:
						return "Key Lost！\n爱情的锁打不开了……也许钥匙被苍蝇叼走了？";
					default:
						return "";
				}
			}
			else if (sceneEnum == UISelectControl.ESceneName.Level04_Sing)
			{
				switch (grade)
				{
					case ERatingGrade.S:
						return "KEY MASTER！\n节奏完美对上锁孔！节奏之门为你唱开～";
					case ERatingGrade.A:
						return "Almost in Key！\n只有那只绿鹦鹉跑了个半调～";
					case ERatingGrade.B:
						return "You found a key…\n但好像插错门了，拍子再准点！";
					case ERatingGrade.C:
						return "Key？Where？\n你的手指在找Key，但节奏在隔壁房间……";
					case ERatingGrade.D:
						return "Out of Key！\n听起来像鹦鹉大战录音机！";
					case ERatingGrade.F:
						return "Key Lost! \n音乐老师气到退团：‘这谁调的Key？！";
					default:
						return "";
				}
			}
		}
		return "";
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
