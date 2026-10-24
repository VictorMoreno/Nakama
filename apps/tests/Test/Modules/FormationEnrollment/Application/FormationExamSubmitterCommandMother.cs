using Nakama.FormationEnrollment.Application.ExamSubmitter;

namespace Test.Modules.FormationEnrollment.Application;

public static class FormationExamSubmitterCommandMother
{
    public static FormationExamSubmitter.Command With(Guid id, Guid moduleId, Guid userId, params int[] answers) =>
        new(id, moduleId, userId, answers);
}
