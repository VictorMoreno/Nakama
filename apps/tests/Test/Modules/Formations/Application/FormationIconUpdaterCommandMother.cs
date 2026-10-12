using Nakama.Formations.Application.IconUpdater;
using Test.Modules.Formations.Domain;

namespace Test.Modules.Formations.Application;

public static class FormationIconUpdaterCommandMother
{
    public static FormationIconUpdater.Command WithUpload(Guid? id = null, string contentType = "image/png")
    {
        byte[] content = [1, 2, 3];

        return new FormationIconUpdater.Command(
            id ?? FormationMother.RandomId().Value,
            new FormationIconUpdater.Upload(contentType, new MemoryStream(content), content.Length));
    }

    public static FormationIconUpdater.Command WithoutUpload(Guid? id = null) =>
        new(id ?? FormationMother.RandomId().Value, null);
}
