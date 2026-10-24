using Nakama.Shared.Domain;

namespace Nakama.Formations.Domain.Errors;

public sealed class FormationModuleLockedByGatingError()
    : DomainError("nakama.formations.module_locked_by_gating", "El módulo anterior está bloqueado hasta completar el módulo previo.");

public sealed class FormationNotFoundError(string key)
    : DomainError("nakama.formations.not_found", $"Formation '{key}' not found.");

public sealed class FormationModuleNotFoundError(string moduleId, string formationKey)
    : DomainError("nakama.formations.module_not_found", $"Module '{moduleId}' not found in formation '{formationKey}'.");

public sealed class FormationAlreadyExistsError(string key)
    : DomainError("nakama.formations.already_exists", $"Formation '{key}' already exists.");

public sealed class FormationModuleAlreadyExistsError(string moduleId, string formationKey)
    : DomainError("nakama.formations.module_already_exists", $"Module '{moduleId}' already exists in formation '{formationKey}'.");

public sealed class FormationIconUnsupportedTypeError()
    : DomainError("nakama.formations.icon_unsupported_type", "Unsupported image type. Only PNG and JPEG are allowed.");

public sealed class FormationIconTooLargeError(long maxBytes)
    : DomainError("nakama.formations.icon_too_large", $"File exceeds the {maxBytes / (1024 * 1024)} MB limit.");

public sealed class FormationInvalidQuizQuestionCountError(int max)
    : DomainError("nakama.formations.invalid_quiz_question_count", $"El número de preguntas debe estar entre 1 y {max}.");

public sealed class FormationQuizModelRequiredError()
    : DomainError("nakama.formations.quiz_model_required", "Hay que indicar el modelo con el que generar las preguntas.");

public sealed class FormationQuizProviderNotConnectedError(string provider)
    : DomainError("nakama.formations.quiz_provider_not_connected", $"El proveedor '{provider}' todavía no tiene un adaptador conectado.");

public sealed class FormationQuizWithoutDocumentsError()
    : DomainError("nakama.formations.quiz_formation_without_documents", "La formación no tiene documentos con los que generar preguntas.");

public sealed class FormationQuizNoDocumentsSelectedError()
    : DomainError("nakama.formations.quiz_no_documents_selected", "Selecciona al menos un documento de la formación.");

public sealed class FormationQuizDocumentNotInFormationError(string documentId)
    : DomainError("nakama.formations.quiz_document_not_in_formation", $"El documento '{documentId}' no pertenece a la formación.");

public sealed class FormationExamQuizRequiredError()
    : DomainError("nakama.formations.exam_quiz_required", "Un examen necesita al menos una pregunta.");

public sealed class FormationExamPassingScoreInvalidError(int min, int max)
    : DomainError("nakama.formations.exam_passing_score_invalid", $"La nota mínima del examen debe estar entre {min} y {max}.");

public sealed class FormationExamDataOnNonExamModuleError()
    : DomainError("nakama.formations.exam_data_on_non_exam_module", "Solo los módulos de tipo examen llevan preguntas y nota mínima.");

public sealed class FormationArchivedError(string key)
    : DomainError("nakama.formations.archived", $"La formación '{key}' está dada de baja.");

public sealed class FormationNotActiveError(string key)
    : DomainError("nakama.formations.not_active", $"La formación '{key}' no está activa, así que no se puede cursar.");

public sealed class FormationDocumentNotFoundError(string id)
    : DomainError("nakama.formation_documents.not_found", $"Document '{id}' not found.");

public sealed class FormationDocumentUnsupportedTypeError()
    : DomainError("nakama.formations.document_unsupported_type", "Unsupported document type. Only PDF, Markdown, and plain text files are allowed.");

public sealed class FormationDocumentTooLargeError(long maxBytes)
    : DomainError("nakama.formations.document_too_large", $"File exceeds the {maxBytes / (1024 * 1024)} MB limit.");

public sealed class FormationQuizGenerationFailedError()
    : DomainError("nakama.formations.quiz_generation_failed", "El modelo no devolvió unas preguntas válidas. Inténtalo de nuevo.");

public static class FormationErrors
{
    public static DomainError ModuleLockedByGating { get; } = new FormationModuleLockedByGatingError();

    public static DomainError QuizModelRequired { get; } = new FormationQuizModelRequiredError();

    public static DomainError QuizFormationWithoutDocuments { get; } = new FormationQuizWithoutDocumentsError();

    public static DomainError DocumentUnsupportedType { get; } = new FormationDocumentUnsupportedTypeError();

    public static DomainError QuizNoDocumentsSelected { get; } = new FormationQuizNoDocumentsSelectedError();

    public static DomainError QuizGenerationFailed { get; } = new FormationQuizGenerationFailedError();

    public static DomainError IconUnsupportedType { get; } = new FormationIconUnsupportedTypeError();

    public static DomainError InvalidQuizQuestionCount(int max) => new FormationInvalidQuizQuestionCountError(max);

    public static DomainError QuizProviderNotConnected(string provider) => new FormationQuizProviderNotConnectedError(provider);

    public static DomainError QuizDocumentNotInFormation(Guid documentId) => new FormationQuizDocumentNotInFormationError(documentId.ToString());

    public static DomainError DocumentNotFound(Guid id) => new FormationDocumentNotFoundError(id.ToString());

    public static DomainError ExamQuizRequired { get; } = new FormationExamQuizRequiredError();

    public static DomainError ExamPassingScoreInvalid(int min, int max) => new FormationExamPassingScoreInvalidError(min, max);

    public static DomainError ExamDataOnNonExamModule { get; } = new FormationExamDataOnNonExamModuleError();

    public static DomainError Archived(FormationId id) => new FormationArchivedError(id.ToString());

    public static DomainError NotActive(FormationId id) => new FormationNotActiveError(id.ToString());

    public static DomainError DocumentTooLarge(long maxBytes) => new FormationDocumentTooLargeError(maxBytes);

    public static DomainError NotFound(FormationId id) => new FormationNotFoundError(id.ToString());

    public static DomainError ModuleNotFound(string moduleId, string formationKey) => new FormationModuleNotFoundError(moduleId, formationKey);

    public static DomainError AlreadyExists(string key) => new FormationAlreadyExistsError(key);

    public static DomainError ModuleAlreadyExists(string moduleId, string formationKey) => new FormationModuleAlreadyExistsError(moduleId, formationKey);

    public static DomainError IconTooLarge(long maxBytes) => new FormationIconTooLargeError(maxBytes);
}
