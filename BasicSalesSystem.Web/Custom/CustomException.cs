namespace BasicSalesSystem.Web.Custom
{
    using System;

    public class CustomException : Exception
    {
        public CustomException(string message)
            : base(message) { }
    }
}
