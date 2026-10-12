using Nakama.Analytics.Application.FeedbackSubmitter;
using Test.Modules.Shared;

namespace Test.Modules.Analytics.Application;

public static class AnalyticsFeedbackSubmitterCommandMother
{
    public static AnalyticsFeedbackSubmitter.Command Random() => With();

    public static AnalyticsFeedbackSubmitter.Command With(bool? useful = null) =>
        new(useful ?? MotherCreator.Random.Random.Bool());
}
