using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [DllImport("__Internal")]
    private static extern void GetAuthExtern();
    [DllImport("__Internal")]
    private static extern void AuthExtern();

    public event Action MakeAuth;

    [SerializeField] private Transform ballonsPool;
    [SerializeField] private Transform bombsPool;
    [SerializeField] private Transform ballonsSpawner;
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private HudManager hudManager;
    [SerializeField] private SoundsManager soundManager;
    [SerializeField] private MusicManager musicManager;
    [SerializeField] private ProductManager productManager;

    private bool wasAuthRequest;
    private bool batterySaving;

    public Transform BallonsPool => ballonsPool;
    public Transform BombsPool => bombsPool;
    public Transform BallonsSpawner => ballonsSpawner;
    public PlayerManager PlayerManager => playerManager;
    public HudManager HudManager => hudManager;
    public SoundsManager SoundsManager => soundManager;
    public MusicManager MusicManager => musicManager;
    public ProductManager ProductManager => productManager;

    public bool IsAuth { get; private set; }

    public bool BatterySaving { get { return batterySaving; }
    set
        {
            batterySaving = value;
            if (value)
                Application.targetFrameRate = 30;
            else
                Application.targetFrameRate = 60;
        }
    }



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }

#if !UNITY_EDITOR
        Application.targetFrameRate = 60;
#endif
    }

    private void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        MakeAuth += PlayerAuth;
        GetAuthExtern();
#endif
    }

    private void OnApplicationFocus(bool focus)
    {
        AudioListener.volume = focus ? 1 : 0;
    }

    private void PlayerAuth()
    {
        if (!IsAuth)
        {
            AuthExtern();
        }
    }

    public void SetAuth(int auth)
    {
        Debug.Log("Auth == " + auth);

        if (auth == 0)
            IsAuth = false;
        else if (auth == 1)
            IsAuth = true;

        Debug.Log("IsAuth == " + IsAuth);
    }

    public void CheckAuth()
    {
        if (!IsAuth)
        {
            if (!wasAuthRequest)
            {
                wasAuthRequest = true;

                HudManager.ConfirmationWindow.SetInfo(LocalizationManager.Instance.GetLocalizedText("info.leaderboard"),
                    () => MakeAuth());
            }
        }
    }
}
