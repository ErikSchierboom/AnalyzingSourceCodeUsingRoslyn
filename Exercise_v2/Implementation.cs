namespace Exercise_v2
{
    public static class Implementation
    {
        public   static   string Greeting (  string name = null  )
        {
            if (name == null) {
                return "Hello there!";
            }
        
            return   "Hello " +   name  + "!";
        }
    }    
}
