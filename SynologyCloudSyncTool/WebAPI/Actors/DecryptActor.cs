using Akka.Actor;
using WebAPI.Data;

namespace WebAPI.Actors
{
    public class DecryptActor:ReceiveActor
    {
        public DecryptActor()
        {
            ReceiveAsync<DecryptJob>(async Job =>
            {
                
            });
        }
    }
}