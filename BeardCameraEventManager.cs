using System;
using UnityEngine;
using System.Collections;

namespace BeardCameraSystem
{
    public class BeardCameraEventManager
    {

        public static event Action OnStartCamera;
        public static void FireStartCamera() { if (OnStartCamera != null) OnStartCamera(); }

        public static event Action<Flag> OnPlayerHitFlag;
        public static void FirePlayerHitFlag(Flag flag) { if (OnPlayerHitFlag != null) OnPlayerHitFlag(flag); }

        public static event Action OnCinematicCamera;
        public static void FireCinematicCamera() { if (OnCinematicCamera != null) OnCinematicCamera(); }

        public static event Action<Transform> OnSmoothFollowCamera;
        public static void FireSmoothFollowCamera(Transform target) { if (OnSmoothFollowCamera != null) OnSmoothFollowCamera(target); }

        public static event Action<Item> OnTelescopeCamera;
        public static void FireTelesscopeCamera(Item item) { if (OnTelescopeCamera != null) OnTelescopeCamera(item); }

        public static event Action<NPC> OnNPCDialogCamera;
        public static void FireNPCDialogCamera(NPC npc) { if (OnNPCDialogCamera != null) OnNPCDialogCamera(npc); }

        public static event Action<QuestStart> OnStartingQuestCamera;
        public static void FireStartingQuestCamera (QuestStart questStart) { if (OnStartingQuestCamera != null) OnStartingQuestCamera(questStart); }

       
        public static event Action<NPC> OnDisengageNPC;
        public static void FireDisengageNPC(NPC npc) { if (OnDisengageNPC != null) OnDisengageNPC(npc); }

        public static event Action<QuestStart> OnDisengageQuestStart;
        public static void FireDisengageQuestStart(QuestStart qs) { if (OnDisengageQuestStart != null) OnDisengageQuestStart(qs); }

     

        public static event Action<float> OnCameraWhiteIn;
        public static void FireCameraWhiteIn(float time) { if (OnCameraWhiteIn != null) OnCameraWhiteIn(time); }

        public static event Action<float> OnCameraWhiteOut;
        public static void FireCameraWhiteOut(float time) { if (OnCameraWhiteOut != null) OnCameraWhiteOut(time); }

        public static event Action<float> OnCameraBlackIn;
        public static void FireCameraBlackIn(float time) { if (OnCameraBlackIn != null) OnCameraBlackIn(time); }

        public static event Action<float> OnCameraBlackOut;
        public static void FireCameraBlackOut(float time) { if (OnCameraBlackOut != null) OnCameraBlackOut(time); }

        public static event Action<Color, float> OnCameraFlash;
        public static void FireCameraFlash(Color color, float duration) { if (OnCameraFlash != null) OnCameraFlash(color, duration); }

        public static event Action<Color, float, float> OnCameraFlashScreen;
        public static void FireCameraFlash(Color color, float fadeIn, float fadeOut) { if (OnCameraFlashScreen != null) OnCameraFlashScreen(color, fadeIn, fadeOut); }

        public static event Action OnCameraLucyJuiceOn;
        public static void FireCameraLucyJuiceOn() { if (OnCameraLucyJuiceOn != null) OnCameraLucyJuiceOn(); }

        public static event Action OnCameraLucyJuiceOff;
        public static void FireCameraLucyJuiceOff() { if (OnCameraLucyJuiceOff != null) OnCameraLucyJuiceOff(); }

        public static event Action OnCameraOpenChest;
        public static void FireCameraOpenChests() { if (OnCameraOpenChest != null) OnCameraOpenChest(); }

    }
}
