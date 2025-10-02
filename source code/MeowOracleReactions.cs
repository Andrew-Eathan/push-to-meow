using BepInEx.Logging;
using System;
using System.Timers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PushToMeowMod
{
	public static class MeowOracleReactions
	{
		#region Dialogue Lines

		// Looks to the Moon (SL) - Dislikes responses
		public static readonly string[] _meowInterruptionsSLDislikes =
		{
			"...",
			"Are you taunting me?",
			"...What is it? Oh, forget it, I don't care..."
		};

		// goofy
		/*public static readonly string[] _meowInterruptionsSLNeutral =
		{
			"What are you trying to say, <PlayerName>?",
			"What is it?",
			"What do you want, <PlayerName>?",
			"I can't understand you.",
			"I don't understand your continuous meowing.",
			"What are you meowing at?",
			"...",
			"..."
		};

		public static readonly string[] _meowInterruptionsSLLikes =
		{
			"What are you trying to say, <PlayerName>?",
			"I wish I could understand you, <PlayerName>...",
			"Hm...?",
			"Oh, what do you want, <PlayerName>?",
			"I apologise, <PlayerName>... I don't understand you.",
			"Is there something you want, <PlayerName>?",
			"Does something bother you, <PlayerName>?",
			"...?",
			"...",
			"..."
		};*/

		// Five Pebbles (SS) - Dialogue lines
		public static readonly string SSMeowAfterIsThisReachingYou = "...I assume from your meowing that you understand me now.";
		public static readonly string SSMeowALotDuringHisDialogue1 = "...Can you stop with the yells? This information is relevant to you.";
		public static readonly string SSMeowALotDuringHisDialogue2 = "Please quit that immediately.";
		public static readonly string SSMeowALotDuringHisDialogue3 = "You had your chances. Leave now.";
		public static readonly string SSMeowReaction1 = "What is it?";
		public static readonly string SSMeowReaction2 = "I have nothing for you. Please stop meowing.";
		public static readonly string SSMeowReadingPearl1 = "Please quit meowing, I'm reading you your pearl.";
		public static readonly string SSMeowReadingPearl2 = "If you keep going with this, I will not continue reading it.";
		public static readonly string SSMeowReadingPearl3 = "...";

		// Looks to the Moon (SL) - Neutral/Likes responses
		public static readonly string SLMeow1 = "Yes? What's wrong?";
		public static readonly string SLMeow2 = "Meow?";
		public static readonly string[] SLMeow3 = {
			"Meow.",
			"Meow...?",
			"Meow.",
			"Meow.",
			"Meow!",
			"Meow."
		};

		#endregion

		#region State Variables

		// Five Pebbles state
		public static bool MeowFlagSSReaching = false;
		public static int MeowSSAngerStage = 0;
		public static int MeowSSAngerStage1 = 0;
		public static float MeowSSLastTimeBothered = 0;
		public static int MeowSSCounter = 0;

		// Looks to the Moon state
		public static int MeowSLCounter = 0;
		public static int MeowSLCanRespondCounter = 0;

		#endregion

		#region Oracle Reaction Handling

		public static void HandleThisOraclesReaction(Player self, Oracle oracle)
		{
			if (oracle.health == 0) return;
			if (!PushToMeowMain.ModSettings.DoOraclesReact.Value) return;
			if (oracle == null || oracle.oracleBehavior == null) return;
			ManualLogSource Logger = PushToMeowMain.PLogger;

			if (oracle.oracleBehavior is SLOracleBehaviorHasMark) // looks to the moon with mark
			{
				var sl = oracle.oracleBehavior as SLOracleBehaviorHasMark;
				if (sl.currentConversation != null) return;

				var oraclePos = oracle.abstractPhysicalObject.pos.Tile.ToVector2();
				var playerPos = self.abstractPhysicalObject.pos.Tile.ToVector2();
				var dist = Vector2.Distance(oraclePos, playerPos);
				var opinion = sl.State.GetOpinion;

				if (opinion == SLOrcacleState.PlayerOpinion.NotSpeaking)
				{
					if (Random.value < 0.06)
					{
						sl.dialogBox.Interrupt("...", 0);
					}
					return;
				}

				var likes = SLOrcacleState.PlayerOpinion.Likes;
				var neutral = SLOrcacleState.PlayerOpinion.Neutral;
				var dislikes = SLOrcacleState.PlayerOpinion.Dislikes;
#if DEBUG
				Logger?.LogInfo(oracle + " is sl mark, dist: " + dist + ", slcounter: " + MeowSLCounter);
#endif
				if (dist > 25) return; // ignore meow if slugcat is too far

				if (MeowSLCanRespondCounter > 0)
				{
					MeowSLCanRespondCounter--;
					return;
				}

				string slReaction = "";
				MeowSLCounter++;

				switch (MeowSLCounter)
				{
					case 3:
						if (opinion == dislikes)
							slReaction = _meowInterruptionsSLDislikes[0];
						else slReaction = SLMeow1;
						break;
					case 5:
						if (opinion == dislikes)
							slReaction = _meowInterruptionsSLDislikes[1];
						else slReaction = SLMeow2;
						break;
					case 7:
						if (opinion == dislikes)
							slReaction = _meowInterruptionsSLDislikes[2];
						break;
					default:
						if (MeowSLCounter > 7)
							if (opinion != dislikes && Random.value < 0.6)
								slReaction = SLMeow3[Random.Range(0, SLMeow3.Length - 1)];
						break;
				}

				if (slReaction.Length == 0) return;

				MeowSLCanRespondCounter = 4;

				Timer doReactionTimer =
					new Timer(200 + UnityEngine.Random.value * 150 + (MeowSLCounter > 7 ? 350 : 0)) { AutoReset = false, Enabled = true };

				doReactionTimer.Elapsed += (object _, ElapsedEventArgs e) =>
				{
					var text = Translator.Translate(slReaction);

                    if (JollyCoop.JollyCustom.GetPlayerName(self.playerState.playerNumber) == "Katzen")
					{
						if (text.StartsWith("Meow"))
						{
                            text = text.Substring("Meow".Length);
                            text = "Porl" + text;
                        }
					}
					sl.dialogBox.Interrupt(text, 0);
					MeowSLCanRespondCounter = 0;
				};
			}
			else if (oracle.oracleBehavior is SLOracleBehaviorNoMark) // looks to the moon without mark
			{
#if DEBUG
				Logger?.LogInfo(oracle + " is sl no mark");
#endif
				Timer doSoundTimer =
					new Timer(300 + UnityEngine.Random.value * 400) { AutoReset = false, Enabled = true };

				doSoundTimer.Elapsed += (object _, ElapsedEventArgs e) =>
				{
					var sl = oracle.oracleBehavior as SLOracleBehaviorNoMark;

					switch (Random.Range(0, 5))
					{
						case 0: sl.AirVoice(SoundID.SL_AI_Talk_1); break;
						case 1: sl.AirVoice(SoundID.SL_AI_Talk_2); break;
						case 2: sl.AirVoice(SoundID.SL_AI_Talk_3); break;
						case 3: sl.AirVoice(SoundID.SL_AI_Talk_4); break;
						case 4: sl.AirVoice(SoundID.SL_AI_Talk_5); break;
					}
#if DEBUG
                    Logger?.LogInfo("played SL markless reaction :D");
#endif
				};
			}
			else if (oracle.oracleBehavior is SSOracleBehavior) // five pebbles
			{
				var ss = oracle.oracleBehavior as SSOracleBehavior;

#if DEBUG
				Logger?.LogInfo("CONVO " + ss.conversation);
#endif
				if (ss.conversation != null)
				{
					if (ss.conversation.events.Count > 0 && ss.conversation.events[0] is Conversation.TextEvent)
					{
						var textEvent = ss.conversation.events [0] as Conversation.TextEvent;

						if (!MeowFlagSSReaching && textEvent.text == ss.conversation.Translate("...is this reaching you?"))
						{
							MeowFlagSSReaching = true;
							// he acknowledges that you meowed as a response
							ss.conversation.events.Insert(2, new Conversation.TextEvent(ss.conversation, 0, Translator.Translate(SSMeowAfterIsThisReachingYou), 0));
							return;
						}

						// ignore spam meows to help with multiplayer
						if (Time.time - MeowSSLastTimeBothered < 1.3)
						{
#if DEBUG
							Logger?.LogInfo("Ignoring oracle meow spam");
#endif
							return;
						}

						ss.conversation.paused = true;
						ss.restartConversationAfterCurrentDialoge = true;

						if (Random.Range(0, 24) < 2)
							ss.conversation.Interrupt("...", 0);

						MeowSSCounter++;
						MeowSSLastTimeBothered = Time.time;
#if DEBUG
						Logger?.LogInfo(MeowSSCounter + " meow counter");
						Logger?.LogInfo(MeowSSAngerStage + " anger stage");
#endif

						if (MeowSSCounter == 5)
						{
							MeowSSCounter = 0;
							MeowSSAngerStage++;

							switch (MeowSSAngerStage)
							{
								case 1: ss.conversation.Interrupt(Translator.Translate(SSMeowALotDuringHisDialogue1), 0); break;
								case 2: ss.conversation.Interrupt(Translator.Translate(SSMeowALotDuringHisDialogue2), 0); break;
								case 3:
								ss.conversation.Interrupt(Translator.Translate(SSMeowALotDuringHisDialogue3), 0);
								ss.conversation.Destroy();
								ss.conversation = null;
								ss.NewAction(SSOracleBehavior.Action.ThrowOut_ThrowOut);

								MeowSSAngerStage = 0;
								break;
							}
						}
						else
						{
							ss.conversation.paused = false;
							ss.restartConversationAfterCurrentDialoge = true;
						}
					}
				}
				// check for the mark so that pebbles doesnt react to the meows before he gives you the mark
				else if (ss.conversation == null && self.room.game.GetStorySession.saveState.deathPersistentSaveData.theMark)
				{
					if (Random.Range(0, 10) < 4)
					{
						ss.dialogBox.Interrupt("...", 0);
						return;
					}

					MeowSSCounter++;

					if (MeowSSCounter == 5)
					{
						MeowSSCounter = 0;
						MeowSSAngerStage1++;

						switch (MeowSSAngerStage1)
						{
							case 1: ss.dialogBox.Interrupt(Translator.Translate(SSMeowReaction1), 0); break;
							case 2: ss.dialogBox.Interrupt(Translator.Translate(SSMeowReaction2), 0); break;
						}
					}
				}
			}
		}

		#endregion
	}
}
