using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Inst { get; private set; }
    void Awake() => Inst = this;

    [SerializeField] NotificationPanel notificationPanel;
    [SerializeField] TMP_Text pazeText;
    [SerializeField] TMP_Text displayActionText;
    [SerializeField] TMP_Text moveAbleText;

    [SerializeField] TMP_Text energyText;
    [SerializeField] TMP_Text moveCostText;
    public bool onDisplayActions;
    public bool moveAble;

    void Start()
    {
        StartGame();
    }

    void Update()
    {
        pazeText.text = "Paze: " + TurnManager.Inst.paze.ToString();
        energyText.text = TurnManager.Inst.Energy + " / " + TurnManager.Inst.maxEnergy;
        moveCostText.text = TurnManager.Inst.MoveCost + " / " + TurnManager.Inst.maxMoveCost;

        if(Input.GetKeyDown(KeyCode.Q))
        {
            onDisplayActions = !onDisplayActions;
            displayActionText.text = "DisplayActions: " + onDisplayActions;

            if (onDisplayActions)
            {
                List<HexNode> selectedTiles = new();
                foreach (Unit unit in UnitManager.Inst.Enemies)
                {
                    if (!unit.card.canDisplay) return;
                    selectedTiles.AddRange(unit.card.SelectedArea);
                    GridManager.Inst.AreaDisplay(AreaType.Attack, false, selectedTiles, unit);
                }
            }
            else
            {
                foreach (Unit unit in UnitManager.Inst.Enemies)
                {
                    GridManager.Inst.RevertTiles(unit);
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            moveAble = !moveAble;
            moveAbleText.text = "MoveAble: " + moveAble;
        }

        if (Input.GetMouseButtonDown(1))
        {
            UnitManager.Inst.DeSelectUnit(UnitManager.sUnit);
            CinemachineManager.Inst.SetOrthoSize(false);
        }

#if UNITY_EDITOR
        InputCheatKey();
#endif
    }

    void InputCheatKey()
    {
        if (Input.GetKeyDown(KeyCode.Z))
            TurnManager.UseEnergy(TurnManager.Inst.Energy);

        if (Input.GetKeyDown(KeyCode.X))
            TurnManager.UseMoveCost(TurnManager.Inst.MoveCost);

        if (Input.GetKeyDown(KeyCode.C))
        {
            TurnManager.UseEnergy(TurnManager.Inst.Energy);
            TurnManager.UseMoveCost(TurnManager.Inst.MoveCost);
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