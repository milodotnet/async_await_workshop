namespace AsyncStuff
{
    using System;
    using System.Threading.Tasks;

    public class Methods
    {
        public static Task<string> WaitASecondAndReturnAValue(string valueToReturn)
        {
            Task.Delay(TimeSpan.FromSeconds(1));
            return Task.FromResult(valueToReturn);
        }
    }
}