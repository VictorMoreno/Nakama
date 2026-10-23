using Nakama.AnswerFeedback.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;

namespace Nakama.AnswerFeedback.Infrastructure;

public sealed class NhAnswerFeedbackRepository(INhUnitOfWork unitOfWork)
    : NhRepositoryBase<AnswerFeedbackEntry, Guid>(unitOfWork), IAnswerFeedbackRepository;
