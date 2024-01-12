namespace Reqnroll.CommonModels
{
    public interface IResult
    {
    }

    public interface IResult<out T> : IResult
    {
    }
}
