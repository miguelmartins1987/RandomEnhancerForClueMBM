using CSharpFunctionalExtensions;
using HookInject;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using System.Threading.Tasks;

namespace HookInjectTests
{
    [TestClass]
    public class DiceRollControllerTests
    {
        [TestMethod]
        public void TestInitialize()
        {
            Result result = DiceRollController.Initialize(ConfigurationManager.AppSettings["ApiKey"]);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(100, DiceRollController.Data.Count);
        }

        [TestMethod]
        public void TestInitializeWithTask()
        {
            Task<Result> task = Task.Run(() =>
            {
                return DiceRollController.Initialize(ConfigurationManager.AppSettings["ApiKey"]);
            });
            task.Wait();
            Assert.IsTrue(task.Result.IsSuccess);
            Assert.AreEqual(100, DiceRollController.Data.Count);
        }
    }
}
