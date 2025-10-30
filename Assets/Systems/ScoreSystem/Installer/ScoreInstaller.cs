using Systems.LeaderBoardSystem.Manager;
using Systems.LeaderBoardSystem.Scriptable;
using Systems.ScoreSystem.Controller;
using Systems.ScoreSystem.Signal;
using UnityEngine;
using Zenject;

namespace Systems.ScoreSystem.Installer
{
    [CreateAssetMenu(fileName = "ScoreInstaller", menuName = "Installers/ScoreInstaller")]
    public class ScoreInstaller : ScriptableObjectInstaller<ScoreInstaller>
    {
        [SerializeField] private ScoreCanvasView scoreCanvasView;
        [SerializeField] private LeaderBoardScriptable leaderBoardScriptable;
        
        public override void InstallBindings()
        {
            Container.DeclareSignal<AddScoreSignal>();

            Container.Bind<LeaderBoardScriptable>().FromScriptableObject(leaderBoardScriptable).AsSingle();
            Container.Bind<LeaderboardManager>().AsSingle();
            
            Container.Bind<ScoreCanvasView>().FromComponentInNewPrefab(scoreCanvasView).AsSingle();

            Container.BindInterfacesAndSelfTo<ScoreController>().AsSingle();
        }
    }
}