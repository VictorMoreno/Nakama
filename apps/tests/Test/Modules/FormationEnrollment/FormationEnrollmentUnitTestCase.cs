using System.Linq.Expressions;
using Moq;
using Nakama.FormationDetail.Application;
using Nakama.FormationDetail.Application.Finder;
using Nakama.FormationDetail.Domain.Errors;
using Nakama.FormationEnrollment.Domain;
using Nakama.Shared.Domain;
using Test.Modules.Shared;

namespace Test.Modules.FormationEnrollment;

public abstract class FormationEnrollmentUnitTestCase : UnitTestCase
{
    protected Mock<IQueryBus> QueryBus { get; } = new();

    protected Mock<IFormationEnrollmentRepository> EnrollmentsRepository { get; } = new();

    [SetUp]
    public void FormationEnrollmentSetUp()
    {
        this.QueryBus.Reset();
        this.EnrollmentsRepository.Reset();
    }

    protected void ShouldFindFormationDetail(FormationDetailResponse detail) =>
        this.QueryBus
            .Setup(bus => bus.Ask(It.IsAny<FormationDetailFinder.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(detail);

    protected void ShouldNotFindFormationDetail(Guid id) =>
        this.QueryBus
            .Setup(bus => bus.Ask(It.IsAny<FormationDetailFinder.Query>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(FormationDetailErrors.NotFound(id.ToString()));

    protected void ShouldFindEnrollment(Nakama.FormationEnrollment.Domain.FormationEnrollment? enrollment) =>
        this.EnrollmentsRepository
            .Setup(repository => repository.FindByFormationAndUser(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

    protected void ShouldHaveAskedForFormationDetail(Guid id) =>
        this.QueryBus.Verify(
            bus => bus.Ask(It.Is<FormationDetailFinder.Query>(query => query.Id == id), It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldHaveSavedEnrollment(Nakama.FormationEnrollment.Domain.FormationEnrollment enrollment) =>
        this.EnrollmentsRepository.Verify(
            repository => repository.Save(enrollment, It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldHaveSavedEnrollmentMatching(
        Expression<Func<Nakama.FormationEnrollment.Domain.FormationEnrollment, bool>> matches) =>
        this.EnrollmentsRepository.Verify(
            repository => repository.Save(
                It.Is<Nakama.FormationEnrollment.Domain.FormationEnrollment>(matches),
                It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldNotHaveSavedAnyEnrollment() =>
        this.EnrollmentsRepository.Verify(
            repository => repository.Save(
                It.IsAny<Nakama.FormationEnrollment.Domain.FormationEnrollment>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

    protected void ShouldHaveRemovedEnrollment(Nakama.FormationEnrollment.Domain.FormationEnrollment enrollment) =>
        this.EnrollmentsRepository.Verify(
            repository => repository.Remove(enrollment, It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldNotHaveRemovedAnyEnrollment() =>
        this.EnrollmentsRepository.Verify(
            repository => repository.Remove(
                It.IsAny<Nakama.FormationEnrollment.Domain.FormationEnrollment>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
}
