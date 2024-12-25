namespace Spartan.Models.ResponseModels;

public interface ICommandResponseModel
{
    public string Command { get; set; }
    public string Output { get; set; }
}