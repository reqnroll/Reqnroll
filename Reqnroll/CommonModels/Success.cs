namespace Reqnroll.CommonModels
{
    public class Success : ISuccess
    {
    }

    public class Success<T> : Success, ISuccess<T>
    {
        public Success(T result)
        {
            Result = result;
        }

        public T Result { get; }
    }
}
