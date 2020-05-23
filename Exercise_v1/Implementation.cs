namespace Exercise_v1
{
    public static class Implementation
    {
        public static string Greeting(string name = null)
        {
            return $"Hello {name ?? "there"}!";
        }
    }    
}