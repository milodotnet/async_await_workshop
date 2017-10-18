namespace AsyncStuff
{
    using System;
    using System.Threading.Tasks;

    public class SyncMethods
    {
        public static Task<string> WaitASecondAndReturnAValue(string valueToReturn)
        {
            Task.Delay(TimeSpan.FromSeconds(1)).Wait();
            return Task.FromResult(valueToReturn);
        }

        public static Task MutateGlobalState()
        {
            Task.Delay(TimeSpan.FromSeconds(1)).Wait();            
            return Task.Factory.StartNew(() => MutableGlobalState.State = 5);
        }

        public static Task ThrowAnExceptionAfterOneSecond()
        {
            Task.Delay(TimeSpan.FromSeconds(1)).Wait();
            return Task.Factory.StartNew(() => throw new Exception());
        }
    }
}