using UnityEngine;
using System.Collections;
using BeardCameraSystem;

[RequireComponent(typeof(Collider2D))]
public class Flag : MonoBehaviour {

    

    public int ID;

    private BeardCamera beardCamera;

    public bool isSet { get; private set; }

    public Vector3 flagOffset { get; private set; }

    public new Collider2D collider { get; private set; }

    void OnEnable()
    {

    }

    void Awake()
    {
        SetFlag();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.GetComponent<PlayerBehaviour>() == beardCamera.Player)
        {
            BeardCameraEventManager.FirePlayerHitFlag(this);
        }

    }

    void SetFlag()
    {
        if (isSet)
            return;
        else
        {
            beardCamera = GetComponentInParent<BeardCamera>();
            collider = GetComponent<Collider2D>();
            isSet = true;
        }
    }

    public void ToggleCollider(bool state)
    {
        collider.enabled = state;
    }
}
