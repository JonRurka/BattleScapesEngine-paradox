using SiliconStudio.Paradox.Engine;

namespace BattleScapesEngine
{
    class BattleScapesEngineApp
    {
        static void Main(string[] args)
        {
            // Profiler.EnableAll();
            using (var game = new Game())
            {
                game.Run();
            }
        }
    }
}
