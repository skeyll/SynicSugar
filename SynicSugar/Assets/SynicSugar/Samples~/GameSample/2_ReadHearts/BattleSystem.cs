using SynicSugar.P2P;
using UnityEngine;
using UnityEngine.UI;

namespace SynicSugar.Samples.ReadHearts
{
    [NetworkCommons(true)]
    public partial class BattleSystem : MonoBehaviour
    {
#region UI
        [SerializeField] private GameObject matchmakeCanvas, inGameObjects, resultObjects, chatCanvas, gamestartButton;
        [SerializeField] private Text turnState, battleResult, timeText;
        public Text sliderValue, chatText;
        [SerializeField] private Slider attackSlider;
#endregion
#region State
        private enum GameState
        {
            InGame, Result
        }
        private GameState gameState;
        [HideInInspector] public Player player, opponent;
        private int _currentTurn;
        public int currentTurn 
        { 
            get { return _currentTurn; }
            set 
            {
                if(player.currentTurn < currentTurn || opponent.currentTurn < currentTurn) return;
                
                StartEndphaseProcess();
            }
        }
        [SyncVar(true, 1000)] private float currentTime;
#endregion
#region Prep
        public void ActivateGameCanvas()
        {
            this.gameObject.SetActive(true);
            matchmakeCanvas.SetActive(false);
        }
        private void Start()
        {
            ConnectHub.Instance.RegisterInstance(this);
            ChangeUIActive(GameState.Result);
            gamestartButton.SetActive(isHost);
            chatCanvas.SetActive(true);
            
            //Set UserID to sync
            SetUserId();
            //After the all objects for Sync have been prepared, call StartPacketReceiver() to get packets.
            ConnectHub.Instance.StartPacketReceiver(PacketReceiveTiming.Update, 3);
        }
        private void SetUserId()
        {
            //We can set UserID in by ourself in or after "Start".
            GameObject playerObject = this.transform.Find("PlayerHP").gameObject;
            player = playerObject.GetComponent<Player>();
            player.SetOwnerID(p2pInfo.Instance.LocalUserId);

            GameObject enemyObject = this.transform.Find("EnemyHP").gameObject;
            opponent = enemyObject.GetComponent<Player>();
            opponent.SetOwnerID(p2pInfo.Instance.CurrentRemoteUserIds[0]);
        }
#endregion
#region In GameSystem
        private void StartEndphaseProcess()
        {
            _currentTurn++;
            CalculateDamage();
            ReflectResultToUI();

            if(currentTurn == 5 || player.status.CurrentHP <= 0 || opponent.status.CurrentHP <= 0)
            {
                JudgeBattleResult();
                return;
            }
        }
        private void CalculateDamage()
        {
            if(player.status.AttackDamage == opponent.status.AttackDamage) //shuffle
            { 
                int tmpHP = player.status.CurrentHP;
                player.status.CurrentHP = opponent.status.CurrentHP;
                opponent.status.CurrentHP = tmpHP;
            }
            else if(player.status.AttackDamage < opponent.status.AttackDamage)
            {
                opponent.status.CurrentHP -= player.status.AttackDamage;
            }
            else
            {
                player.status.CurrentHP -= opponent.status.AttackDamage;
            }
        }
        private void ReflectResultToUI()
        {
            player.hpText.text = player.status.CurrentHP.ToString();

            opponent.damageText.text = opponent.status.AttackDamage.ToString();
            opponent.hpText.text = opponent.status.CurrentHP.ToString();

            turnState.text = currentTurn == 5 ? "LAST" : currentTurn.ToString();
        }
        private void JudgeBattleResult()
        {
            ChangeUIActive(GameState.Result);

            string result = System.String.Empty;

            if(player.status.CurrentHP == opponent.status.CurrentHP)
            {
                result = "DRAW";
            }
            else if(player.status.CurrentHP < opponent.status.CurrentHP)
            {
                result = "LOSE";
            }
            else
            {
                result = "WIN";
            }
            battleResult.text = result;
        }
        [Rpc]
        public void StartNewGame()
        {
            _currentTurn = 1;
            turnState.text = currentTurn.ToString();
            player.InitPlayerStatus();
            opponent.InitPlayerStatus();

            ChangeUIActive(GameState.InGame);

            if(p2pInfo.Instance.IsHost())
            {
                currentTime = 0;
            }
        }
        private void ChangeUIActive(GameState state)
        {
            gameState = state;
            inGameObjects.SetActive(state == GameState.InGame);
            resultObjects.SetActive(state == GameState.Result);
        }
#endregion
        public void OnChangeSliderValue()
        {
            sliderValue.text = ((int)attackSlider.value).ToString();
        }
        private void Update()
        {
            if(gameState == GameState.InGame)
            {
                if(isHost)
                {
                    currentTime += Time.deltaTime;
                }
                timeText.text = ((int)currentTime).ToString();
            }
        }
        public async void ExitGame()
        {
            await ConnectHub.Instance.CloseSession();
            
            SceneChanger.ChangeGameScene(SCENELIST.MainMenu); 
        }
    }

    [System.Serializable]
    public class Status
    {
        public int CurrentHP;
        public int AttackDamage;
    }
}