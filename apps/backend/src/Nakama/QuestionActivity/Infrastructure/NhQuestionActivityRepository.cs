using Nakama.QuestionActivity.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;

namespace Nakama.QuestionActivity.Infrastructure;

public sealed class NhQuestionActivityRepository(INhUnitOfWork unitOfWork)
    : NhRepositoryBase<QuestionActivityEntry, Guid>(unitOfWork), IQuestionActivityRepository;
