using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Inst { get; private set; }
    void Awake() => Inst = this;

    [SerializeField] NotificationPanel notificationPanel;

    void Start()
    {
        StartGame();
    }

    void Update()
    {
#if UNITY_EDITOR
        InputCheatKey();
#endif
    }

    void InputCheatKey()
    {
        if (Input.GetKeyDown(KeyCode.Keypad1))
            TurnManager.OnAddCard?.Invoke();

        if (Input.GetKeyDown(KeyCode.Keypad2))
            TurnManager.OnAddCard?.Invoke();

        if (Input.GetKeyDown(KeyCode.Keypad3))
            TurnManager.Inst.EndTurn();

        if (Input.GetKeyDown(KeyCode.Keypad4))
            CardManager.Inst.TryPutCard();
    }

    void StartGame()
    {
        StartCoroutine(TurnManager.Inst.StartGameCo());
    }

    public void Nodification(string message)
    {
        notificationPanel.Show(message);
    }
}
public static class YieldInstructionCache
{
    public static readonly WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();
    public static readonly WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();
    private static readonly Dictionary<float, WaitForSeconds> waitForSeconds = new Dictionary<float, WaitForSeconds>();

    public static WaitForSeconds WaitForSeconds(float seconds)
    {
        WaitForSeconds wfs;
        if (!waitForSeconds.TryGetValue(seconds, out wfs))
            waitForSeconds.Add(seconds, wfs = new WaitForSeconds(seconds));
        return wfs;
    }
}