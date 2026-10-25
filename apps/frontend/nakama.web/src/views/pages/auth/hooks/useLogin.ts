import { type FormEvent, useState } from "react";
import { useNavigate } from "react-router-dom";
import type { AuthenticatedUser } from "../../../../modules/auth/domain/AuthenticatedUser";
import type { IAuthRepository } from "../../../../modules/auth/domain/IAuthRepository";
import { Services } from "../../../../shared/infrastructure/injection/Services";
import { useInjection } from "../../../../shared/infrastructure/injection/useInjection";
import { useUser } from "../../../shared/contexts/UserContext";

export function useLogin() {
  const navigate = useNavigate();
  const { setCurrentUser } = useUser();
  const authRepo = useInjection<IAuthRepository>(Services.AuthRepository.id);
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      const user: AuthenticatedUser | null = await authRepo.Login(email, password);
      if (!user) {
        setError("Correo o contraseña incorrectos.");
        return;
      }
      setCurrentUser(user);
      navigate("/chat");
    } finally {
      setSubmitting(false);
    }
  }

  return { email, setEmail, password, setPassword, error, submitting, handleSubmit };
}
