namespace AsyncStuff
{
    using System;
    using System.Threading.Tasks;

    public class SuperSpecificException : Exception
    {
        public SuperSpecificException() : base("This tells you everything you need to know to find the problem")
        {
            
        }
    }
    
    public class SomeIrrelevantResult {}
    
    public class AsyncMethods
    {
        public static async Task<string> WaitASecondAndReturnAValueAsync(string valueToReturn)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            return valueToReturn;
        }    
        
        public static async Task<string> WaitASecondAndReturnAValueAsyncAwaitFalse(string valueToReturn)
        {
            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            return valueToReturn;
        }
        
        public static async Task MutateGlobalStateAfterASecondAsync(int to)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));            
            MutableGlobalState.State = to;
        }
        
        public static async Task<SomeIrrelevantResult> ThrowAnExceptionAfterNSecondAsync(int n )
        {
            await Task.Delay(TimeSpan.FromSeconds(n));
            throw new SuperSpecificException();
        }
    }
}