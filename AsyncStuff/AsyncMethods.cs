namespace AsyncStuff
{
    using System;
    using System.Threading.Tasks;

    public class AsyncMethods
    {
        public static async Task<string> WaitASecondAndReturnAValueAsync(string valueToReturn)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            return valueToReturn;
        }    
        
        public static async Task MutateGlobalStateAfterASecondAsync()
        {
            await Task.Delay(TimeSpan.FromSeconds(1));            
            MutableGlobalState.State = 6;
        }        
    }
}