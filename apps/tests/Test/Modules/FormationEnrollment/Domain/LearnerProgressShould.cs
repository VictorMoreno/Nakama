using System.Text.Json;
using AwesomeAssertions;
using Nakama.FormationEnrollment.Domain;

namespace Test.Modules.FormationEnrollment.Domain;

[TestFixture]
public sealed class LearnerProgressShould
{
    [Test]
    public void read_progress_stored_before_exam_attempts_existed()
    {
        const string json = """{"completedModuleIds":[],"examScores":{}}""";

        LearnerProgress progress = JsonSerializer.Deserialize<LearnerProgress>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? LearnerProgress.Empty;

        progress.ExamAttempts.Should().BeEmpty();
    }
}
