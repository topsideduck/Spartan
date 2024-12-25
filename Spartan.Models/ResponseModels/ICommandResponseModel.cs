namespace Spartan.Models.ResponseModels;

public interface ICommandResponseModel
{
    public string Plugin { get; set; }
    public string Command { get; set; }
    public string Output { get; set; }
}