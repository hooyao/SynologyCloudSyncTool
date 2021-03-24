using System;
using System.IO;
using Akka.Actor;
using WebAPI.Data;
using WebAPI.IO;

namespace WebAPI.Actors
{
    public class ProjectActor : ReceiveActor
    {
        private readonly IActorRef _downloadActor;

        public ProjectActor(AzureDownloadActorProvider downloadActorProvider)
        {
            this._downloadActor = downloadActorProvider();
            Receive<Project>(project =>
            {
                string tempDir = Path.Combine(Path.GetTempPath(), project.Id);
                if (!Directory.Exists(tempDir))
                {
                    Directory.CreateDirectory(tempDir);
                }

                foreach (var job in project.Jobs)
                {
                    this._downloadActor.Tell(new DownloadJob()
                    {
                        BlobId = project.ConnectorId,
                        ContainerName = project.ContainerName,
                        SourcePath = job.Path,
                        TargetPath = Path.Combine(tempDir, $"{Guid.NewGuid().ToString()}.downloadtmp")
                    });
                }
            });
        }
    }
}