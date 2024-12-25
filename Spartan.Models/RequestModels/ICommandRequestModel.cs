namespace Spartan.Models.RequestModels;

public interface ICommandRequestModel
{
    public string Plugin { get; set; }
    public string Command { get; set; }
}