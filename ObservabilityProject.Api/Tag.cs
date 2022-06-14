namespace ObservabilityProject.Api
{
    public static class Tag
    {
        public const string Success = "result:success";
        public const string Failure = "result:failure";

        public static string[] Tags(params string[] tags) 
        { 
            return tags; 
        }
    }
}
