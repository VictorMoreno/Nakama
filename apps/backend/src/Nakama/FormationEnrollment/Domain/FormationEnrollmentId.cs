using Nakama.Shared.Domain;

namespace Nakama.FormationEnrollment.Domain;

public readonly record struct FormationEnrollmentId(Guid Value) : IGuidValueObject
{
    public static FormationEnrollmentId From(Guid value) => new(value);

    public override string ToString() => this.Value.ToString();
}
