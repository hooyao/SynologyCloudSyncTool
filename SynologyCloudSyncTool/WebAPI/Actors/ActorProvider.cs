using Akka.Actor;

namespace WebAPI.Actors
{
    public delegate IActorRef ProjectActorProvider();
    
    public delegate IActorRef AzureDownloadActorProvider();

    public delegate IActorRef DecryptActorProvider();
}