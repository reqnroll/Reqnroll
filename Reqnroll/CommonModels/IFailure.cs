namespace Reqnroll.CommonModels
{
    public interface IFailure : IResult
    {
    }

    public interface IFailure<out T> : IFailure, IResult<T>
    {
    }
}
