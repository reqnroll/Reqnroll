namespace Reqnroll.UnitTestProvider
{
    public interface IUnitTestRuntimeProvider
    {
        void TestPending(string message);
        void TestInconclusive(string message);
        void TestIgnore(string message);
    }
}