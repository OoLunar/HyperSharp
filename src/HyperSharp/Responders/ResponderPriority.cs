namespace OoLunar.HyperSharp.Responders
{
    public enum ResponderPriority
    {
        /// <summary>
        /// Low priority responders are intended to run first. Whether it be authentication or logging, these responders are the first to run.
        /// </summary>
        Low,

        /// <summary>
        /// Medium priority responders are intended to run after low priority responders. These responders are intended for things like routing.
        /// </summary>
        /// <remarks>
        /// Medium is the recommended default responder priority.
        /// </remarks>
        Medium,

        /// <summary>
        /// High priority responders are intended to be error handlers or metrics. These responders will run last.
        /// </summary>
        High
    }
}
