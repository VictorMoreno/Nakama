using System.Linq.Expressions;
using Moq;
using Nakama.FormationDetail.Application;
using Nakama.FormationDetail.Application.Finder;
using Nakama.FormationDetail.Domain.Errors;
using Nakama.FormationEnrollment.Domain;
using Test.Modules.Shared;

namespace Test.Modules.FormationEnrollment;

public abstract class FormationEnrollmentUnitTestCase : UnitTestCase
{
    protected Mock<IFormationEnrollmentRepository> EnrollmentsRepository { get; } = new();

    [SetUp]
    public void FormationEnrollmentSetUp()
    {
        this.EnrollmentsRepository.Reset();
    }

    protected void ShouldFindFormationDetail(FormationDetailResponse detail) =>
        this.QueryBus
            .Setup(bus => bus.Ask(It.Is<FormationDetailFinder.Query>(query => query.Id == detail.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(detail);

    protected void ShouldNotFindFormationDetail(Guid id) =>
        this.QueryBus
            .Setup(bus => bus.Ask(It.Is<FormationDetailFinder.Query>(query => query.Id == id), It.IsAny<CancellationToken>()))
            .ThrowsAsync(FormationDetailErrors.NotFound(id.ToString()));

    protected void ShouldFindEnrollment(Nakama.FormationEnrollment.Domain.FormationEnrollment enrollment) =>
        this.EnrollmentsRepository
            .Setup(repository => repository.FindByFormationAndUser(
                enrollment.FormationId,
                enrollment.UserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

    protected void ShouldNotFindEnrollment() =>
        this.EnrollmentsRepository
            .Setup(repository => repository.FindByFormationAndUser(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as Nakama.FormationEnrollment.Domain.FormationEnrollment);

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
