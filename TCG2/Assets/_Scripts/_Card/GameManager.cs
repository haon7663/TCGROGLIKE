using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Inst { get; private set; }
    void Awake() => Inst = this;

    [SerializeField] NotificationPanel notificationPanel;
    [SerializeField] TMP_Text pazeText;
    [SerializeField] TMP_Text energyText;
    [SerializeField] TMP_Text moveCostText;

    void Start()
    {
        StartGame();
    }

    void Update()
    {
        pazeText.text = "Paze: " + TurnManager.Inst.paze.ToString();
        energyText.text = TurnManager.Inst.Energy + " / " + TurnManager.Inst.maxEnergy;
        moveCostText.text = TurnManager.Inst.MoveCost + " / " + TurnManager.Inst.maxMoveCost;

#if UNITY_EDITOR
        InputCheatKey();
#endif
    }

    void InputCheatKey()
    {
        if (Input.GetKeyDown(KeyCode.Keypad2))
            TurnManager.UseEnergy(TurnManager.Inst.Energy);

        if (Input.GetKeyDown(KeyCode.Keypad3))
            TurnManager.UseMoveCost(TurnManager.Inst.MoveCost);

        if (Input.GetKeyDown(KeyCode.Keypad4))
            CardManager.Inst.TryPutCard();

        if (Input.GetKeyDown(KeyCode.Keypad5))
            foreach(var tile in GridManager.Inst.Tiles)
            {
                tile.Value.DebugColor(Color.white);
            }
    }

    void StartGame()
    {
        TurnManager.Inst.GameSetUp();
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