using System.Threading;
using SynicSugar.P2P;
using UnityEngine;
using UnityEngine.UI;

namespace  SynicSugar.Samples {
    [NetworkCommons]
    public partial class BattleSystem : MonoBehaviour {
#region Singleton
        public static BattleSystem Instance { get; private set; }
        void Awake() {
            if( Instance != null ) {
                Destroy( this.gameObject );
                return;
            }
            Instance = this;
            ConnectHub.Instance.RegisterInstance(this);
        }
        void OnDestroy() {
            if( Instance == this ) {
                Instance = null;
            }
        }
#endregion
#region UI
        [SerializeField] GameObject matchmakeCanvas, inGameObjects, resultObjects, chatCanvas, gamestartButton;
        [SerializeField] Text turnState, battleResult, timeText;
        public Text sliderValue, chatText;
        [SerializeField] Slider attackSlider;
#endregion
#region State
        enum GameState{
            InGame, Result
        }
        GameState gameState;
        [HideInInspector] public Player player, opponent;
        int _currentTurn;
        public int currentTurn { 
            get { return _currentTurn; }
            set {
                if(player.currentTurn < currentTurn || opponent.currentTurn < currentTurn){
                    return;
                }
                StartEndphaseProcess();
            }
        }
        [SyncVar(true, 1000)]
        float currentTime;
#endregion
#region Connection
        public void ActivateGameCanvas(){
            this.gameObject.SetActive(true);
            matchmakeCanvas.SetActive(false);
        }
        void Start(){
            ChangeUIActive(GameState.Result);
            gamestartButton.SetActive(isHost);
            chatCanvas.SetActive(true);
            ConnectHub.Instance.StartPacketReceiver();
        }
#endregion
#region GameSystem
        void StartEndphaseProcess(){
            _currentTurn++;
            CalculateDamage();
            ReflectResultToUI();

            if(currentTurn == 5 || player.status.CurrentHP <= 0 || opponent.status.CurrentHP <= 0){
                JudgeBattleResult();
                return;
            }
        }
        void CalculateDamage(){
            if(player.status.AttackDamage == opponent.status.AttackDamage){
                int tmpHP = player.status.CurrentHP;
                player.status.CurrentHP = opponent.status.CurrentHP;
                opponent.status.CurrentHP = tmpHP;
            }else if(player.status.AttackDamage < opponent.status.AttackDamage){
                opponent.status.CurrentHP -= player.status.AttackDamage;
            }else{
                player.status.CurrentHP -= opponent.status.AttackDamage;
            }
        }
        void ReflectResultToUI(){
            player.hpText.text = player.status.CurrentHP.ToString();

            opponent.damageText.text = opponent.status.AttackDamage.ToString();
            opponent.hpText.text = opponent.status.CurrentHP.ToString();

            turnState.text = currentTurn == 5 ? "LAST" : currentTurn.ToString();
        }
        void JudgeBattleResult(){
            ChangeUIActive(GameState.Result);
            string result = System.String.Empty;
            if(player.status.CurrentHP == opponent.status.CurrentHP){
                result = "DRAW";
            }else if(player.status.CurrentHP < opponent.status.CurrentHP){
                result = "LOSE";
            }else{
                result = "WIN";
            }
            battleResult.text = result;
        }
        [Rpc]
        public void StartNewGame(){
            _currentTurn = 1;
            turnState.text = currentTurn.ToString();
            player.InitPlayerStatus();
            opponent.InitPlayerStatus();

            ChangeUIActive(GameState.InGame);

            if(p2pManager.Instance.userIds.IsHost()){
                currentTime = 0;
            }
        }
        void ChangeUIActive(GameState state){
            gameState = state;
            inGameObjects.SetActive(state == GameState.InGame);
            resultObjects.SetActive(state == GameState.Result);
        }
#endregion
        public void OnChangeSliderValue(){
            sliderValue.text = ((int)attackSlider.value).ToString();
        }
        void Update() {
            if(gameState == GameState.InGame){
                if(isHost){
                    currentTime += Time.deltaTime;
                }
                timeText.text = ((int)currentTime).ToString();
            }
        }
        public async void ExitGame(){
            p2pManager.Instance.EndConnection();
            if(isHost){
                CancellationTokenSource cnsToken = new CancellationTokenSource();
                await SynicSugar.MatchMake.MatchMakeManager.Instance.DestroyHostingLobby(cnsToken);
            }
            SynicSugar.Samples.GameModeSelect modeSelect = new SynicSugar.Samples.GameModeSelect();
            modeSelect.ChangeGameScene(GameModeSelect.GameScene.MainMenu.ToString()); //Retrun MainMenu
        }
    }

    [System.Serializable]
    public class Status {
        public int CurrentHP;
        public int AttackDamage;
    }
}