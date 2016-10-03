using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;
using Prime31;
using BeardCameraSystem;

public class BeardCamera : MonoBehaviour
{

    public enum State
    {
        Normal,
        Sprinting,
        Cinematic,
        TalkingToNPC,
        GameOver
    }

    public State CameraState { get; private set; }

    public FSM CameraAI { get; private set; }

    public FSM CameraAICalculations { get; private set; }

    public static BeardCamera Instance { get; private set; }

    private Camera mainCamera;

    public Transform target { get; private set; }

    public float smoothDampTime { get; private set; }

    public new Transform transform { get; private set; }

    public Vector3 CurrentCameraOffset { get; private set; }


    public Flag[] Flags = new Flag[4];
    public Flag CurrentFlagFocus { get; private set; }

    private CharacterController2D _playerController;
    private Vector3 _smoothDampVelocity;
    private Vector3 _transitionDampVelocity;


    public PlayerBehaviour Player { get; private set; }
    PlayerEventManager eventManager;

    public ContrastStretch contrastStretchEffect;


    #region ENHANCEMENT OBJECTS

    public UnityStandardAssets.ImageEffects.Bloom bloom;

    public Fisheye fisheye;

    public Vortex vortex;

    public Twirl twirl;

    public NoiseAndScratches noiseEffect;

    #endregion

    public float NormalCameraSmoothDamp { get; private set; }

    public float TransitionCameraDamp { get; private set; }

    public float TransitionCameraDistanceBelow { get; private set; }

    public Vector3 NormalCameraOffset;

    public Vector3 TalkingToNPCOffset;

    public float horizontalXModifier;
    public float sprintXModifier;

    public Vector3 controlHeight;

    float min;

    float max;

    float timeNotMoving = 0;

    float sprintLerpValue = 0;

    float resetLerpValue = 0;

    private float MaxSprintLerpTime = 1;

    private float lerpSprintSpeed = .3f;

    private float cameraLagOnStop = .1f;

    public UI2DSprite flasher;

    bool playerFound = false;

    void Awake()
    {
        init();
    }

    void init()
    {
        if (Instance == null)
        {
            mainCamera = GetComponent<Camera>();
            bloom = GetComponent<UnityStandardAssets.ImageEffects.Bloom>();
            bloom.enabled = false;
            transform = gameObject.transform;
            NormalCameraSmoothDamp = .1f;
            TransitionCameraDamp = .3f;
            TransitionCameraDistanceBelow = 3f;
            CameraAI = new FSM();
            CameraAICalculations = new FSM();
            CurrentFlagFocus = Flags[1];
            Stage.OnGenerated += SetPlayer;

            Instance = this;

        }
        else
            Destroy(this);
    }


    void OnEnable()
    {
        BeardCameraEventManager.OnStartCamera += StartCamera;

        BeardCameraEventManager.OnCameraLucyJuiceOn += LucyJuiceOn;
        BeardCameraEventManager.OnCameraLucyJuiceOff += LucyJuiceOff;

        BeardCameraEventManager.OnCameraOpenChest += ChestEffect;

        BeardCameraEventManager.OnPlayerHitFlag += FocusOnFlag;

        BeardCameraEventManager.OnNPCDialogCamera += TalkingToNPC;
        BeardCameraEventManager.OnDisengageNPC += DisengageNPC;

        BeardCameraEventManager.OnStartingQuestCamera += EngageQuestStart;
        BeardCameraEventManager.OnDisengageQuestStart += DisengageQuestStart;


    }

    void OnDisable()
    {
        BeardCameraEventManager.OnStartCamera -= StartCamera;

        BeardCameraEventManager.OnCameraLucyJuiceOn -= LucyJuiceOn;
        BeardCameraEventManager.OnCameraLucyJuiceOff -= LucyJuiceOff;

        BeardCameraEventManager.OnCameraOpenChest -= ChestEffect;

        BeardCameraEventManager.OnPlayerHitFlag -= FocusOnFlag;

        BeardCameraEventManager.OnNPCDialogCamera -= TalkingToNPC;
        BeardCameraEventManager.OnDisengageNPC -= DisengageNPC;

        BeardCameraEventManager.OnStartingQuestCamera -= EngageQuestStart;
        BeardCameraEventManager.OnDisengageQuestStart -= DisengageQuestStart;


    }


    void OnDestroy() { Stage.OnGenerated -= SetPlayer; if (Instance == this) Instance = null; }

    public static void SetPlayer()
    {

        Instance.Player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerBehaviour>();
        Instance.target = Instance.Player.transform;
        Instance.CameraAI.PushState(Instance.BeardWorldCamera);
        Instance.CameraAICalculations.PushState(Instance.CalculateNormalWorldCamera);
        Instance.smoothDampTime = .2f;
        Instance.NormalCameraOffset.x = 0f;
        //  if (Instance.Player.isFacingRight)
        //   Instance.FocusOnFlag(Instance.Flags[3]);
        //   else
        // Instance.FocusOnFlag(Instance.Flags[0]);
        Stage.OnGenerated -= SetPlayer;
    }

    void StartCamera()
    {
        FadeOut(Color.white, 4f);
       
    }

    void Update()
    {
        CameraAI.Execute();
    }


    /// <summary>
    /// If physics is being handled in fixed update, then use this
    /// </summary>

    void FixedUpdate()
    {
        CameraAICalculations.Execute();
    }




    void FocusOnFlag(Flag flag)
    {
        switch (flag.ID)
        {
            case 1:
                smoothDampTime = .8f;
                NormalCameraOffset.x = 2f;
                CurrentFlagFocus = Flags[2];
                focusing = true;
                focusTParam = 0;
                if (Flags[1].collider.isActiveAndEnabled)
                    Flags[1].ToggleCollider(false);
                break;
            case 2:
                smoothDampTime = .1f;
                if (!Flags[2].collider.isActiveAndEnabled)
                    Flags[2].ToggleCollider(true);
                focusing = false;
                focusTParam = 0;
                break;
            case 3:
                smoothDampTime = .1f;
                if (!Flags[1].collider.isActiveAndEnabled)
                    Flags[1].ToggleCollider(true);
                focusing = false;
                focusTParam = 0;
                break;
            case 4:
                smoothDampTime = .8f;
                NormalCameraOffset.x = -2f;
                CurrentFlagFocus = Flags[1];
                focusing = true;
                focusTParam = 0;
                if (Flags[2].collider.isActiveAndEnabled)
                    Flags[2].ToggleCollider(false);
                break;
        }


    }


    void BeardWorldCamera()
    {

        if (CurrentCameraOffset != NormalCameraOffset)
        {
            CurrentCameraOffset = NormalCameraOffset;
            // smoothDampTime = NormalCameraSmoothDamp;
        }
    }

    bool focusing;
    float focusTParam;
    float focusSpeed = .2f;
    bool roomTransition;

    void BeardWorldCameraCalculations()
    {
        Vector3 targetVector = target.position - NormalCameraOffset;

        switch (CurrentFlagFocus.ID)
        {
            case 2:
                if (smoothDampTime > .1f)
                    smoothDampTime -= Time.deltaTime;

                if (!Player.isFacingRight && Mathf.Abs(Player.Velocity.x) > 0f && !focusing)
                    targetVector = transform.position;
                break;
            case 3:
                if (smoothDampTime > .1f)
                    smoothDampTime -= Time.deltaTime;

                if (Player.isFacingRight && Mathf.Abs(Player.Velocity.x) > 0f && !focusing)
                    targetVector = transform.position;
                break;
        }


        float roomOffset = 0;

        if (Player.HeightAboveFloor > Stage.FloorHeight - TransitionCameraDistanceBelow)
        {
            roomOffset = Player.HeightAboveFloor - Stage.FloorHeight;
            targetVector = Vector3.Lerp(target.position, new Vector3(target.position.x, Stage.FloorHeight * (Player.CurrentFloor + (Player.HeightAboveFloor > 10 ? 1 : 0)) + controlHeight.y / 2, 0), (roomOffset + TransitionCameraDistanceBelow) / (TransitionCameraDistanceBelow + .95f)) - CurrentCameraOffset;
            roomTransition = true;
        }
        else if (Player.HeightAboveFloor > controlHeight.y && (Player.isGrounded || Player.isHanging))
        {
            NormalCameraOffset.y = 0;
            roomTransition = false;
        }
        else if (Player.HeightAboveFloor > controlHeight.y && (!Player.isGrounded || !Player.isHanging))
        {
            targetVector.y = transform.position.y;
            roomTransition = false;
        }
        else if (Player.HeightAboveFloor < .95f)
        {
            roomOffset = Player.HeightAboveFloor;
            targetVector = Vector3.Lerp(target.position, new Vector3(target.position.x, Stage.FloorHeight * (Player.CurrentFloor + (Player.HeightAboveFloor > 10 ? 1 : 0)) + controlHeight.y / 2, 0), (roomOffset + TransitionCameraDistanceBelow) / (TransitionCameraDistanceBelow + .95f)) - CurrentCameraOffset;
            roomTransition = true;
        }
        else
        {
            targetVector.y = Stage.FloorHeight * Player.CurrentFloor + controlHeight.y;
            roomTransition = false;
        }



        Vector3 finalTarget = new Vector3();

        if (roomTransition)
        {
            Vector3 pos1 = Vector3.SmoothDamp(transform.position, targetVector, ref _transitionDampVelocity, roomOffset > 0 ? TransitionCameraDamp * 1.5f : TransitionCameraDamp);
            Vector3 pos2 = Vector3.SmoothDamp(transform.position, targetVector, ref _smoothDampVelocity, smoothDampTime);
            finalTarget = new Vector3(pos2.x, pos1.y, pos2.z);
        }
        else
        {
            if (_transitionDampVelocity.y < -.02f)
            {
                Vector3 pos1 = Vector3.SmoothDamp(transform.position, targetVector, ref _transitionDampVelocity, TransitionCameraDamp);
                Vector3 pos2 = Vector3.SmoothDamp(transform.position, targetVector, ref _smoothDampVelocity, smoothDampTime);
                finalTarget = new Vector3(pos2.x, pos1.y, pos2.z);
            }
            else if (focusing)
            {
                if (focusTParam < 3f)
                    focusTParam += Time.deltaTime * focusSpeed;
                finalTarget = Vector3.Lerp(transform.position, targetVector, focusTParam);
            }
            else
            {
                finalTarget = Vector3.SmoothDamp(transform.position, targetVector, ref _smoothDampVelocity, smoothDampTime);
                _transitionDampVelocity = Vector3.zero;
            }
        }

        transform.position = finalTarget;

        KeepCameraInWorldBounds();



    }

    /// <summary>
    /// [DEPRECATED] Calculates Normal World Camera
    /// </summary>

    void CalculateNormalWorldCamera()
    {
        if (Player.HeightAboveFloor > Stage.FloorHeight - TransitionCameraDistanceBelow)
        {
            RoomTransition(Player.HeightAboveFloor - Stage.FloorHeight);
        }
        else if (Player.HeightAboveFloor > controlHeight.y)
        {
            FollowPlayerHeight();
        }
        else if (Player.HeightAboveFloor < .95f)
        {
            RoomTransition(Player.HeightAboveFloor);
        }
        else
        {
            FixedCameraHeight();
        }

        KeepCameraInWorldBounds();

    }

    /// <summary>
    /// Keeps camera in world bounds
    /// </summary>

    void KeepCameraInWorldBounds()
    {

        if (transform.position.x < mainCamera.orthographicSize * Screen.width / Screen.height - .5f)
            transform.position = new Vector3(mainCamera.orthographicSize * Screen.width / Screen.height - .5f, transform.position.y, transform.position.z);
        else if (transform.position.x > Stage.MapWidth - mainCamera.orthographicSize * Screen.width / Screen.height - .5f)
            transform.position = new Vector3(Stage.MapWidth - mainCamera.orthographicSize * Screen.width / Screen.height - .5f, transform.position.y, transform.position.z);
    }

    /// <summary>
    /// Toggles the Camera State
    /// </summary>
    /// <param name="state"></param>

    public static void ToggleCamera(bool state)
    {
        Instance.mainCamera.enabled = state;
    }



    /// <summary>
    /// Fixeds the height of the camera.
    /// </summary>

    void FixedCameraHeight()
    {
        if (target == null)
            return;

        Vector3 targetVector = new Vector3(target.position.x, Stage.FloorHeight * Player.CurrentFloor, 0) - CurrentCameraOffset + controlHeight;
        if (_transitionDampVelocity.y > .02f)
        {
            Vector3 pos1 = Vector3.SmoothDamp(transform.position, targetVector, ref _transitionDampVelocity, TransitionCameraDamp);
            Vector3 pos2 = Vector3.SmoothDamp(transform.position, targetVector, ref _smoothDampVelocity, smoothDampTime);
            transform.position = new Vector3(pos2.x, pos1.y, pos2.z);
        }
        else
        {
            transform.position = Vector3.SmoothDamp(transform.position, targetVector, ref _smoothDampVelocity, smoothDampTime);
            _transitionDampVelocity = Vector3.zero;
        }

    }

    #region NPC BEHAVIOUR METHODS

    void TalkingToNPC(NPC npc)
    {
        CameraAICalculations.PushState(TalkingToNPC);

    }

    void DisengageNPC(NPC npc)
    {
        CameraAICalculations.PopState();
    }

    void TalkingToNPC()
    {
        TalkingToNPCOffset = transform.position;
        TalkingToNPCOffset.y = target.position.y + 1f;
        if (target == null)
            return;

        Vector3 pos1 = Vector3.SmoothDamp(transform.position, TalkingToNPCOffset, ref _transitionDampVelocity, TransitionCameraDamp);
        Vector3 pos2 = Vector3.SmoothDamp(transform.position, TalkingToNPCOffset, ref _smoothDampVelocity, TransitionCameraDamp);
        transform.position = new Vector3(pos2.x, pos1.y, pos2.z);

    }

    #endregion

    #region QUEST BEHAVIOUR METHODS 

    void EngageQuestStart(QuestStart quest)
    {
        CameraAICalculations.PushState(EngagingQuestStart);
    }


    void EngagingQuestStart()
    {

        TalkingToNPCOffset = transform.position;
        TalkingToNPCOffset.y = target.position.y + 1f;
        if (target == null)
            return;

        Vector3 pos1 = Vector3.SmoothDamp(transform.position, TalkingToNPCOffset, ref _transitionDampVelocity, TransitionCameraDamp);
        Vector3 pos2 = Vector3.SmoothDamp(transform.position, TalkingToNPCOffset, ref _smoothDampVelocity, TransitionCameraDamp);
        transform.position = new Vector3(pos2.x, pos1.y, pos2.z);
    }

  

    void DisengageQuestStart(QuestStart quest)
    {
        CameraAICalculations.PopState();
    }

  

    #endregion


    /// <summary>
    /// Follows the height of the player.
    /// </summary>

    void FollowPlayerHeight()
	{
		if (target == null)
			return;

		Vector3 targetVector = target.position - CurrentCameraOffset;
		if (_transitionDampVelocity.y < -.02f)
		{
			Vector3 pos1 = Vector3.SmoothDamp(transform.position, targetVector, ref _transitionDampVelocity, TransitionCameraDamp);
			Vector3 pos2 = Vector3.SmoothDamp(transform.position, targetVector, ref _smoothDampVelocity, smoothDampTime);
			transform.position = new Vector3(pos2.x, pos1.y, pos2.z);
		}
		else
		{
			transform.position = Vector3.SmoothDamp(transform.position, targetVector, ref _smoothDampVelocity, smoothDampTime);
			_transitionDampVelocity = Vector3.zero;
		}
	}

	/// <summary>
	/// Transitions to other Room
	/// </summary>

	void RoomTransition(float offset)
	{
		if (target == null)
			return;

		Vector3 targetVector = Vector3.Lerp(target.position, new Vector3(target.position.x, Stage.FloorHeight * (Player.CurrentFloor + (Player.HeightAboveFloor > 10 ? 1 : 0)) + controlHeight.y / 2, 0), (offset + TransitionCameraDistanceBelow) / (TransitionCameraDistanceBelow + .95f)) - CurrentCameraOffset;
		Vector3 pos1 = Vector3.SmoothDamp(transform.position, targetVector, ref _transitionDampVelocity, offset > 0 ? TransitionCameraDamp * 1.5f : TransitionCameraDamp);
		Vector3 pos2 = Vector3.SmoothDamp(transform.position, targetVector, ref _smoothDampVelocity, smoothDampTime);
		transform.position = new Vector3(pos2.x, pos1.y, pos2.z);
	}



	/// <summary>
	/// Chests the effect.
	/// </summary>

	public void ChestEffect()
	{
		StartCoroutine(playLucy());
	}

	/// <summary>
	/// Enables Lucy Juice Effect
	/// </summary>
	public static void LucyJuiceOn()
	{
		//mInstance.contrastStretchEffect.enabled = true;
		Instance.bloom.enabled = true;
		Instance.bloom.bloomIntensity = 2.5f;
		//fisheye.enabled = true;
		//vortex.enabled = true;
		//twirl.enabled = true;
		//noiseEffect.enabled = true;
		// StartCoroutine(LucyJuiceEffect());

	}

	public static void SwitchCameraState(State state)
	{
		Instance.CameraState = state;
	}

	/// <summary>
	/// Disables Lucy Juice Effect
	/// </summary>
	public static void LucyJuiceOff()
	{
		//StopCoroutine(LucyJuiceEffect());

		Instance.contrastStretchEffect.enabled = false;
		Instance.bloom.enabled = false;

		//fisheye.enabled = false;
		//vortex.enabled = false;
		//twirl.enabled = false;
		// noiseEffect.enabled = false;
	}

	public void FlashScreen(Color color, float duration)
	{
		StartCoroutine(flashScreen(color, duration));
	}
	public void FlashScreen(Color color, float fadeIn, float fadeOut)
	{
		StartCoroutine(flashScreen(color, fadeIn, fadeOut));
	}

	public void FadeOut(Color color, float duration)
	{
		StartCoroutine(fadeOut(color, duration));
	}

	IEnumerator flashScreen(Color color, float fadeIn, float fadeOut)
	{
		flasher.gameObject.SetActive(true);
		flasher.color = color;
		TweenAlpha t = flasher.GetComponent<TweenAlpha>();
		t.duration = fadeIn;
		t.PlayForward();
		yield return new WaitForSeconds(fadeIn + .1f);
		t.duration = fadeOut;
		t.PlayReverse();
		yield return new WaitForSeconds(fadeOut + .1f);

		flasher.gameObject.SetActive(false);
		yield break;

	}


	IEnumerator flashScreen(Color color, float duration)
	{
		flasher.gameObject.SetActive(true);
		flasher.color = color;
		TweenAlpha t = flasher.GetComponent<TweenAlpha>();
		t.duration = duration;
		t.PlayForward();
		yield return new WaitForSeconds(duration + .1f);
		t.PlayReverse();
		yield return new WaitForSeconds(duration + .1f);

		flasher.gameObject.SetActive(false);
		yield break;

	}

	IEnumerator fadeOut(Color color, float fadeOut)
	{
		flasher.gameObject.SetActive(true);
		flasher.color = color;
		TweenAlpha t = flasher.GetComponent<TweenAlpha>();
		t.from = 1f;
		t.to = 0;
		t.duration = fadeOut;
		t.PlayForward();
		yield return new WaitForSeconds(fadeOut + .1f);

		flasher.gameObject.SetActive(false);
		yield break;

	}

	IEnumerator playLucy()
	{
		LucyJuiceOn();

		while (Instance.bloom.bloomIntensity > 0)
		{
			Instance.bloom.bloomIntensity -= Time.deltaTime * 4;

			yield return null;
		}

		LucyJuiceOff();
	}

	

	public static void FellToDeath()
	{
		Instance.target = null;
	}

	public static void SprintingCamera()
	{
		if (Instance.Player.FacingDirection == 1 && Instance.Player.Velocity.x > 1)
		{
			Instance.timeNotMoving = 0;

			if (Instance.Player.isSprinting)
			{
				Instance.resetLerpValue = 0;
				if (Instance.sprintLerpValue < Instance.MaxSprintLerpTime)
					Instance.sprintLerpValue += Time.deltaTime * Instance.lerpSprintSpeed;

				if (Instance.NormalCameraOffset.x != -Instance.sprintXModifier)
					Instance.NormalCameraOffset.x = Mathf.Lerp(-.1f, -Instance.sprintXModifier, Instance.sprintLerpValue);
			}
			else
			{
				Instance.sprintLerpValue = 0;
				if (Instance.resetLerpValue < Instance.MaxSprintLerpTime)
					Instance.resetLerpValue += Time.deltaTime * Instance.lerpSprintSpeed;

				if (Instance.NormalCameraOffset.x != -Instance.horizontalXModifier)
					Instance.NormalCameraOffset.x = Mathf.Lerp(Instance.CurrentCameraOffset.x, -Instance.horizontalXModifier, Instance.resetLerpValue);
			}
		}
		else if (Instance.Player.FacingDirection == -1 && Instance.Player.Velocity.x < -1)
		{
			Instance.timeNotMoving = 0;

			if (Instance.Player.isSprinting)
			{
				Instance.resetLerpValue = 0;
				if (Instance.sprintLerpValue < Instance.MaxSprintLerpTime)
					Instance.sprintLerpValue += Time.deltaTime * Instance.lerpSprintSpeed;

				if (Instance.NormalCameraOffset.x != Instance.sprintXModifier)
					Instance.NormalCameraOffset.x = Mathf.Lerp(.1f, Instance.sprintXModifier, Instance.sprintLerpValue);
			}
			else
			{
				Instance.sprintLerpValue = 0;
				if (Instance.resetLerpValue < Instance.MaxSprintLerpTime)
					Instance.resetLerpValue += Time.deltaTime * Instance.lerpSprintSpeed;

				if (Instance.NormalCameraOffset.x != Instance.horizontalXModifier)
					Instance.NormalCameraOffset.x = Mathf.Lerp(Instance.CurrentCameraOffset.x, Instance.horizontalXModifier, Instance.resetLerpValue);
			}
		}
	}


	public static void NormalCamera()
	{
		if (Instance.timeNotMoving > Instance.cameraLagOnStop)                                            //  Reset Camera on Lag
			Instance.NormalCameraOffset.x = 0;

		if (Instance.CurrentCameraOffset != Instance.NormalCameraOffset)
		{
			Instance.CurrentCameraOffset = Instance.NormalCameraOffset;
			Instance.smoothDampTime = Instance.NormalCameraSmoothDamp;
		}

		Instance.sprintLerpValue = 0;
		Instance.timeNotMoving += Time.deltaTime;
	}

}
