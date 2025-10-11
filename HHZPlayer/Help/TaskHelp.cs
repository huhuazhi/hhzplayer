
using System.Threading.Tasks;

namespace HHZPlayer.Help;

public class TaskHelp
{
    public static void Run(Action action)
    {
        Task.Run(() => {
            try
            {
                action.Invoke();
            }
            catch (Exception e)
            {
                Terminal.WriteError(e);
            }
        });
    }
}
