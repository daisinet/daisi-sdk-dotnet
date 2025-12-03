using Daisi.Protos.V1;
using Daisi.SDK.Models;

namespace Daisi.SDK.Extensions.CommandServices
{
    public static class CommandExtensions
    {
        //public static DaisiCommand ToDaisiCommand(this Command command, params DaisiCommandParameter[] additionalParameters)
        //{
        //    return new DaisiCommand()
        //    {
        //        Message = command.Message,
        //        Name = command.Name,
        //        Parameters = command.Parameters.Select(p => new DaisiCommandParameter() { Key = p.Key, Value = p.Value }).Union(additionalParameters).ToArray()
        //    };
        //}

        //public static Command ToRpcCommand(this DaisiCommand command)
        //{
        //    var c = new Command()
        //    {
        //        Message = command.Message,
        //        Name = command.Name
        //    };
            
        //    if(command.Parameters.Any())
        //        c.Parameters.AddRange(command.Parameters.Select(p => new CommandParameter() { Key = p.Key, Value = p.Value?.ToString() }));
            
        //    return c;
        //}
    }
}
