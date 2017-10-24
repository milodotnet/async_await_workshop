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
        
        public static Task<string> WaitASecondAndReturnAValueUsingTaskCompletionSource(string valueToReturn)
        {
            /*
                A way to create a Task which is your puppet. You can make the Task complete at any point you like,
                 and you can make it fault by giving it an exception at any point you like.
    
                Davies, Alex. Async in C# 5.0: Unleash the Power of Async (p. 35). O'Reilly Media. Kindle Edition.             
            */
            var tcs = new TaskCompletionSource<string>();
            Task.Delay(TimeSpan.FromSeconds(1))
                .ContinueWith(t => tcs.SetResult(valueToReturn));
           
            return tcs.Task;
        }
        public static Task MutateGlobalState()
        {
            Task.Delay(TimeSpan.FromSeconds(1)).Wait();            
            return Task.Factory.StartNew(() => MutableGlobalState.State = 5);
        }

        public static Task ThrowAnExceptionAfterOneSecond(int n)
        {
            Task.Delay(TimeSpan.FromSeconds(n)).Wait();
            return Task.Factory.StartNew(() => throw new Exception());
        }
    }
}