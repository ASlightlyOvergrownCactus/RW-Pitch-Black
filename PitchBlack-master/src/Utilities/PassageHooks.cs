using RWCustom;
using UnityEngine;
using System.Collections.Generic;
using System.IO;



namespace PitchBlack;

public static class PassageHooks
{

	public static void Apply()
    {
        On.WinState.CycleCompleted += BP_CycleCompleted;
        On.WinState.CreateAndAddTracker += BP_CreateAndAddTracker;
        On.WinState.PassageDisplayName += WinState_PassageDisplayName;
		
		On.Menu.MenuScene.BuildScene += BP_BuildScene;
        On.Menu.CustomEndGameScreen.GetDataFromSleepScreen += BP_GetDataFromSleepScreen;

        On.Menu.EndgameMeter.FloatMeter.GrafUpdate += FloatMeter_GrafUpdate;
        On.Menu.EndgameMeter.GrafUpdate += EndgameMeter_GrafUpdate;
    }

    private static void EndgameMeter_GrafUpdate(On.Menu.EndgameMeter.orig_GrafUpdate orig, Menu.EndgameMeter self, float timeStacker)
    {
        orig(self, timeStacker);
		//SPECIAL COLORS FOR THIS ONE
        if (self.tracker.ID == EnumExt_MyMod.Pursued)
		{
            float num2 = Mathf.Lerp(self.lastShowAsFullFilled, self.showAsFullfilled, timeStacker);
            float num3 = Mathf.Lerp(self.lastAnimationLightUp, self.animationLightUp, timeStacker);
            Color color = Color.Lerp(Menu.Menu.MenuRGB(Menu.Menu.MenuColors.VeryDarkGrey), Menu.Menu.MenuRGB(self.fullfilledNow ? Menu.Menu.MenuColors.DarkRed : Menu.Menu.MenuColors.MediumGrey), Mathf.Max(Mathf.Pow(num2, 0.2f), num3));
            self.symbolSprite.color = color;
            self.circleSprite.color = color;
            self.glowSprite.color = Color.red;
            self.glowSprite.alpha = self.symbolSprite.alpha * 0.65f;
            self.label.color = Color.red;
        }
    }

    private static void FloatMeter_GrafUpdate(On.Menu.EndgameMeter.FloatMeter.orig_GrafUpdate orig, Menu.EndgameMeter.FloatMeter self, float timeStacker)
    {
		orig(self, timeStacker);
        //RERUN THESE
		if (self.owner.tracker.ID == EnumExt_MyMod.Pursued)
		{
            float num = Mathf.Lerp(self.owner.lastMeterAnimation, self.owner.meterAnimation, timeStacker);
            float num2 = Mathf.Pow(Mathf.InverseLerp(0.5f, 1f, num), 3f);
			self.meterSprites[0].color = Color.red; // CurseColor(self, timeStacker, num2); //self.LossColor(timeStacker, num2);
            self.meterSprites[1].color = Color.red; // CurseColor(self, timeStacker, num2);
            self.tipSprite.color = Color.red;
            self.sideBarSprite.color = Color.red;
        }
        
    }

	//LIKE LOSSCOLOR OR GAINCOLOR, BUT VERY RED
    public static Color CurseColor(Menu.EndgameMeter.FloatMeter myMeter, float timeStacker, float colorCue)
    {
        return myMeter.AllColorsViaThis(Color.Lerp(myMeter.FilledColor(timeStacker), new Color(1f, 0f, 0f), (1f - myMeter.pulse) * 0.5f * colorCue), timeStacker);
    }

    private static string WinState_PassageDisplayName(On.WinState.orig_PassageDisplayName orig, WinState.EndgameID ID)
	{
		if (ID == EnumExt_MyMod.Pursued)
			return "The Pursued";
		else
			return orig.Invoke(ID);
	}



	public static void BP_BuildScene(On.Menu.MenuScene.orig_BuildScene orig, Menu.MenuScene self)
	{
		if (self.sceneID == EnumExt_MyScene.Endgame_Pursued)
		{
			//WE DIDN'T BUILD ONE YET, BUT IF YOU WANT TO....
			/*
			//FIRST PART ALL OF THEM GET
			if (self is Menu.InteractiveMenuScene)
			{
				(self as Menu.InteractiveMenuScene).idleDepths = new List<float>();
			}
			Vector2 vector = new Vector2(0f, 0f);
			// vector..ctor(0f, 0f);

			//NOW THE CUSTOM PART
			self.sceneFolder = "Scenes" + Path.DirectorySeparatorChar.ToString() + "Endgame - Pursued";
			if (self.flatMode)
			{
				self.AddIllustration(new Menu.MenuIllustration(self.menu, self, self.sceneFolder, "Endgame - The Pursued - Flat", new Vector2(683f, 384f), false, true));
			}
			else
			{
				self.AddIllustration(new Menu.MenuDepthIllustration(self.menu, self, self.sceneFolder, "Pursued - 6", new Vector2(71f, 49f), 2.2f, Menu.MenuDepthIllustration.MenuShader.Lighten));
				self.AddIllustration(new Menu.MenuDepthIllustration(self.menu, self, self.sceneFolder, "Pursued - 5", new Vector2(71f, 49f), 1.5f, Menu.MenuDepthIllustration.MenuShader.Normal));
				self.AddIllustration(new Menu.MenuDepthIllustration(self.menu, self, self.sceneFolder, "Pursued - 4", new Vector2(71f, 49f), 1.7f, Menu.MenuDepthIllustration.MenuShader.Normal));
				//self.depthIllustrations[self.depthIllustrations.Count - 1].setAlpha = new float?(0.5f);
				self.AddIllustration(new Menu.MenuDepthIllustration(self.menu, self, self.sceneFolder, "Pursued - 3", new Vector2(71f, 49f), 1.7f, Menu.MenuDepthIllustration.MenuShader.LightEdges));
				self.AddIllustration(new Menu.MenuDepthIllustration(self.menu, self, self.sceneFolder, "Pursued - 2", new Vector2(71f, 49f), 1.5f, Menu.MenuDepthIllustration.MenuShader.Normal));
				self.AddIllustration(new Menu.MenuDepthIllustration(self.menu, self, self.sceneFolder, "Pursued - 1", new Vector2(171f, 49f), 1.3f, Menu.MenuDepthIllustration.MenuShader.Normal)); //LightEdges
				//(self as Menu.InteractiveMenuScene).idleDepths.Add(2.2f);
				(self as Menu.InteractiveMenuScene).idleDepths.Add(2.2f);
				(self as Menu.InteractiveMenuScene).idleDepths.Add(1.7f);
				(self as Menu.InteractiveMenuScene).idleDepths.Add(1.7f);
				(self as Menu.InteractiveMenuScene).idleDepths.Add(1.5f);
				(self as Menu.InteractiveMenuScene).idleDepths.Add(1.3f);
			}
			self.AddIllustration(new Menu.MenuIllustration(self.menu, self, self.sceneFolder, "Pursued - Symbol", new Vector2(683f, 35f), true, false));
			Menu.MenuIllustration MenuIllustration4 = self.flatIllustrations[self.flatIllustrations.Count - 1];
			MenuIllustration4.pos.x = MenuIllustration4.pos.x - (0.01f + self.flatIllustrations[self.flatIllustrations.Count - 1].size.x / 2f);
			*/
		}
		else
			orig.Invoke(self);
	}


	private static void BP_GetDataFromSleepScreen(On.Menu.CustomEndGameScreen.orig_GetDataFromSleepScreen orig, Menu.CustomEndGameScreen self, WinState.EndgameID endGameID)
	{
		if (endGameID == EnumExt_MyMod.Pursued)
		{
			//GOTTA REPLICATE THE MENU SCREEN
			Menu.MenuScene.SceneID sceneID = Menu.MenuScene.SceneID.Empty;
			sceneID = EnumExt_MyScene.Endgame_Pursued;
			self.scene = new Menu.InteractiveMenuScene(self, self.pages[0], sceneID);
			self.pages[0].subObjects.Add(self.scene);
			self.pages[0].Container.AddChild(self.blackSprite);
			if (self.scene.flatIllustrations.Count > 0)
			{
				self.scene.flatIllustrations[0].RemoveSprites();
				self.scene.flatIllustrations[0].Container.AddChild(self.scene.flatIllustrations[0].sprite);
				self.glyphIllustration = self.scene.flatIllustrations[0];
				self.glyphGlowSprite = new FSprite("Futile_White", true);
				self.glyphGlowSprite.shader = self.manager.rainWorld.Shaders["FlatLight"];
				self.pages[0].Container.AddChild(self.glyphGlowSprite);
				self.localBloomSprite = new FSprite("Futile_White", true);
				self.localBloomSprite.shader = self.manager.rainWorld.Shaders["LocalBloom"];
				self.pages[0].Container.AddChild(self.localBloomSprite);
			}
			self.titleLabel = new Menu.MenuLabel(self, self.pages[0], "", new Vector2(583f, 5f), new Vector2(200f, 30f), false, null);
			self.pages[0].subObjects.Add(self.titleLabel);
			self.titleLabel.text = self.Translate(WinState.PassageDisplayName(endGameID));
		}
		else
			orig.Invoke(self, endGameID);
	}

	private static void BP_GenerateAchievementScores(On.Expedition.ChallengeTools.orig_GenerateAchievementScores orig)
	{
		orig.Invoke();
		Expedition.ChallengeTools.achievementScores.Add(EnumExt_MyMod.Pursued, 50);
	}
	
	
	
	public static void BP_CycleCompleted(On.WinState.orig_CycleCompleted orig, WinState self, RainWorldGame game)
	{
		orig.Invoke(self, game);
		//ONLY FOR BACON
		if (game.session is StoryGameSession session && (session.saveStateNumber == Plugin.BeaconName))
		{
            WinState.IntegerTracker integerTracker4 = self.GetTracker(EnumExt_MyMod.Pursued, true) as WinState.IntegerTracker;
            if (integerTracker4 != null)
            {
                integerTracker4.SetProgress(100);
                //integerTracker4.lastShownProgress = 99; //PRETEND IT'S NEVER FULL... MAYBE? this makes us watch it every time though...
            }
        }
    }


    public static WinState.EndgameTracker BP_CreateAndAddTracker(On.WinState.orig_CreateAndAddTracker orig, WinState.EndgameID ID, List<WinState.EndgameTracker> endgameTrackers)
	{
		WinState.EndgameTracker endgameTracker = null;
		
		if (ID == EnumExt_MyMod.Pursued)
		{
			endgameTracker = new WinState.IntegerTracker(ID, 99, 0, 0, 100); //default, min, showFrom, max
			Debug.Log("GLUTTON TRACKER CREATED! ");
		}
        else
            return orig.Invoke(ID, endgameTrackers); //JUST RUN THE ORIGINAL AND NOTHING ELSE BELOW IT



        //AND THEN RUN THE ORIGINAL STUFF THAT WOULD OTHERWISE BE SKIPPED
        if (endgameTracker != null && endgameTrackers != null)
		{
			bool flag = false;
			for (int j = 0; j < endgameTrackers.Count; j++)
			{
				if (endgameTrackers[j].ID == ID)
				{
					flag = true;
					endgameTrackers[j] = endgameTracker;
					break;
				}
			}
			if (!flag)
			{
				endgameTrackers.Add(endgameTracker);
			}
		}
		return endgameTracker;
	}
	
	
	
	public static class EnumExt_MyMod
	{ // You can have multiple EnumExt_ classes in your assembly if you need multiple items with the same name for the different enum
		public static WinState.EndgameID Pursued = new WinState.EndgameID("Pursued", true);
    }
	
	
	public static class EnumExt_MyScene
	{
		public static Menu.MenuScene.SceneID Endgame_Pursued = new Menu.MenuScene.SceneID("Endgame_Pursued", true);
	}
}