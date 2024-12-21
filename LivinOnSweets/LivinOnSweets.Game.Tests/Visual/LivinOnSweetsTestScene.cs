using osu.Framework.Testing;

namespace LivinOnSweets.Game.Tests.Visual
{
    public abstract partial class LivinOnSweetsTestScene : TestScene
    {
        protected override ITestSceneTestRunner CreateRunner() => new LivinOnSweetsTestSceneTestRunner();

        private partial class LivinOnSweetsTestSceneTestRunner : LivinOnSweetsGameBase, ITestSceneTestRunner
        {
            private TestSceneTestRunner.TestRunner runner;

            protected override void LoadAsyncComplete()
            {
                base.LoadAsyncComplete();
                Add(runner = new TestSceneTestRunner.TestRunner());
            }

            public void RunTestBlocking(TestScene test) => runner.RunTestBlocking(test);
        }
    }
}
